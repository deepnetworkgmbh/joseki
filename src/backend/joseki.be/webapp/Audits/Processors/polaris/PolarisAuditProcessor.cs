using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Serilog;

using webapp.BlobStorage;
using webapp.Database;
using webapp.Database.Models;

namespace webapp.Audits.Processors.polaris
{
    /// <summary>
    /// Polaris audit processor.
    /// </summary>
    public class PolarisAuditProcessor : IAuditProcessor
    {
        private static readonly ILogger Logger = Log.ForContext<PolarisAuditProcessor>();

        private readonly IBlobStorageProcessor blobStorage;
        private readonly IJosekiDatabase db;
        private readonly ChecksCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolarisAuditProcessor"/> class.
        /// </summary>
        /// <param name="blobStorage">Blob Storage implementation.</param>
        /// <param name="db">Joseki database implementation.</param>
        /// <param name="cache">Checks cache object.</param>
        public PolarisAuditProcessor(IBlobStorageProcessor blobStorage, IJosekiDatabase db, ChecksCache cache)
        {
            this.blobStorage = blobStorage;
            this.db = db;
            this.cache = cache;
        }

        /// <inheritdoc />
        public async Task Process(ScannerContainer container, CancellationToken token)
        {
            var audits = await this.blobStorage.GetUnprocessedAudits(container);

            foreach (var audit in audits)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    Logger.Information("Processing audit result from {AuditName} container was {ProcessingState}", audit.Name, "started");
                    await this.ProcessSingleAudit(audit);
                    Logger.Information("Processing audit result from {AuditName} container was {ProcessingState}", audit.Name, "finished");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Processing audit result from {AuditName} container was {ProcessingState}", audit.Name, "failed");
                }
            }
        }

        private async Task ProcessSingleAudit(AuditBlob auditBlob)
        {
            var path = $"{auditBlob.ParentContainer.Name}/{auditBlob.Name}";
            var stream = await this.blobStorage.DownloadFile(path);

            using var sr = new StreamReader(stream);
            var metadataString = sr.ReadToEnd();
            var auditMetadata = JsonConvert.DeserializeObject<AuditMetadata>(metadataString);

            if (auditMetadata.AuditResult != "succeeded")
            {
                Logger.Warning(
                    "Audit {AuditPath} result is {AuditResult} due: {FailureReason}",
                    path,
                    auditMetadata.AuditResult,
                    auditMetadata.FailureDescription);
            }
            else
            {
                try
                {
                    var audit = await this.NormalizeRawData(auditBlob, auditMetadata);

                    // TODO: Counter functions could be converted into single iterator
                    Logger.Information(
                        "Successfully processed audit {AuditId} of {AuditDate} with {CheckResultCount} check results, where succeeded {Succeeded}; failed {Failed}; no-data {NoData}",
                        audit.Id,
                        audit.Date,
                        audit.CheckResults.Count,
                        audit.CheckResults.Count(i => i.Value == CheckValue.Succeeded),
                        audit.CheckResults.Count(i => i.Value == CheckValue.Failed),
                        audit.CheckResults.Count(i => i.Value == CheckValue.NoData));

                    await this.db.SaveAuditResult(audit);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to process audit {AuditPath}", path);
                }
            }

            await this.blobStorage.MarkAsProcessed(auditBlob);
        }

        private async Task<Audit> NormalizeRawData(AuditBlob auditBlob, AuditMetadata auditMetadata)
        {
            var auditJson = await this.GetJsonObject($"{auditBlob.ParentContainer.Name}/{auditMetadata.PolarisAuditPaths}");
            var checks = await this.ParseChecksResults(auditJson, auditMetadata, auditBlob.ParentContainer.Metadata);

            var k8sJson = await this.GetJsonObject($"{auditBlob.ParentContainer.Name}/{auditMetadata.KubeMetadataPaths}");
            var k8sMetadata = new JObject
            {
                { "scanner", JToken.FromObject(auditBlob.ParentContainer.Metadata) },
                { "audit", JToken.FromObject(auditMetadata) },
                { "cluster", k8sJson },
            };

            // TODO: add place-holder checks for image-scans?
            var auditDate = DateTimeOffset.FromUnixTimeSeconds(auditMetadata.Timestamp).DateTime;
            var audit = new Audit
            {
                Id = auditMetadata.AuditId,
                Date = auditDate,
                ScannerId = $"{auditBlob.ParentContainer.Metadata.Type}/{auditBlob.ParentContainer.Metadata.Id}",
                CheckResults = checks,
                MetadataKube = new MetadataKube
                {
                    AuditId = auditMetadata.AuditId,
                    Date = auditDate,
                    JSON = k8sMetadata.ToString(Formatting.None),
                },
            };

            return audit;
        }

        private async Task<JObject> GetJsonObject(string path)
        {
            var stream = await this.blobStorage.DownloadFile(path);

            using var sr = new StreamReader(stream);
            var fileContent = sr.ReadToEnd();

            return JObject.Parse(fileContent);
        }

        private async Task<List<CheckResult>> ParseChecksResults(JObject auditJson, AuditMetadata auditMetadata, ScannerMetadata scannerMetadata)
        {
            if (!(auditJson["Results"] is JArray rootResultsArray))
            {
                throw new FormatException("Audit results file does not have Results object in it");
            }

            var checkResults = new List<CheckResult>();

            foreach (var rootResultItem in rootResultsArray)
            {
                var clusterId = string.IsNullOrEmpty(auditMetadata.ClusterId) ? scannerMetadata.Id : auditMetadata.ClusterId;
                var nsName = rootResultItem["Namespace"].Value<string>();
                var objectKind = rootResultItem["Kind"].Value<string>().ToLowerInvariant();
                var objectName = rootResultItem["Name"].Value<string>().ToLowerInvariant();
                var idPrefix = $"k8s/{clusterId}/ns/{nsName}/{objectKind}/{objectName}";

                // parse deployment/job/daemon-set/etc level checks
                if (rootResultItem["Results"] != null && rootResultItem["Results"].HasValues)
                {
                    checkResults.AddRange(await this.ProcessResultsObject(rootResultItem["Results"], idPrefix, auditMetadata.AuditId));
                }

                // parse pod level checks
                if (rootResultItem["PodResult"] != null && rootResultItem["PodResult"].HasValues)
                {
                    var podNameTokenValue = rootResultItem["PodResult"]["Name"]?.Value<string>();
                    var podName = !string.IsNullOrEmpty(podNameTokenValue)
                        ? podNameTokenValue.ToLowerInvariant()
                        : $"{objectName}-pod";

                    checkResults.AddRange(await this.ProcessResultsObject(rootResultItem["PodResult"]["Results"], $"{idPrefix}/{podName}", auditMetadata.AuditId));

                    // parse container level checks
                    if (rootResultItem["PodResult"]["ContainerResults"] is JArray containersArray)
                    {
                        for (var i = 0; i < containersArray.Count; i++)
                        {
                            var containerName = containersArray[i]["Name"] != null
                                ? containersArray[i]["Name"].Value<string>()
                                : $"{podName}-container{i + 1}";
                            checkResults.AddRange(await this.ProcessResultsObject(containersArray[i]["Results"], $"{idPrefix}/{podName}/{containerName}", auditMetadata.AuditId));
                        }
                    }
                }
            }

            return checkResults;
        }

        private async Task<List<CheckResult>> ProcessResultsObject(JToken jToken, string componentId, string auditId)
        {
            var results = new List<CheckResult>();
            foreach (var child in jToken.Children().Select(i => i.First))
            {
                var id = child["ID"].Value<string>();
                var message = child["Message"].Value<string>();
                var success = child["Success"].Value<bool>();
                var severity = child["Severity"].Value<string>();
                var category = child["Category"].Value<string>();

                var checkId = $"polaris.{id}";
                var internalCheckId = await this.cache.GetOrAddItem(checkId, () => new Check
                {
                    Id = checkId,
                    Severity = this.ToSeverity(severity),
                    Category = category,
                });

                results.Add(new CheckResult
                {
                    ExternalCheckId = checkId,
                    InternalCheckId = internalCheckId,
                    ComponentId = componentId,
                    AuditId = auditId,
                    Value = success ? CheckValue.Succeeded : CheckValue.Failed,
                    Message = message,
                });
            }

            return results;
        }

        private CheckSeverity ToSeverity(string value)
        {
            return value switch
            {
                "error" => CheckSeverity.High,
                "warning" => CheckSeverity.Medium,
                _ => CheckSeverity.Unknown
            };
        }
    }
}
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

namespace webapp.Audits.Processors.azsk
{
    /// <summary>
    /// Az-sk audit processor.
    /// </summary>
    public class AzskAuditProcessor : IAuditProcessor
    {
        private static readonly ILogger Logger = Log.ForContext<AzskAuditProcessor>();

        private readonly IBlobStorageProcessor blobStorage;
        private readonly IJosekiDatabase db;
        private readonly ChecksCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzskAuditProcessor"/> class.
        /// </summary>
        /// <param name="blobStorage">Blob Storage implementation.</param>
        /// <param name="db">Joseki database implementation.</param>
        /// <param name="cache">Checks cache object.</param>
        public AzskAuditProcessor(IBlobStorageProcessor blobStorage, IJosekiDatabase db, ChecksCache cache)
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
                    var tasks = auditMetadata
                        .AzSkAuditPaths
                        .Where(i => i.EndsWith(".json"))
                        .Select(i => this.blobStorage.DownloadFile($"{auditBlob.ParentContainer.Name}/{i}"))
                        .Select(this.ToJArray)
                        .ToArray();

                    await Task.WhenAll(tasks);

                    var audit = await this.NormalizeRawData(tasks.Select(i => i.Result).ToArray(), auditBlob, auditMetadata);

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

        private async Task<JArray> ToJArray(Task<Stream> streamTask)
        {
            var stream = await streamTask;

            using var sr = new StreamReader(stream);
            var metadataString = sr.ReadToEnd();

            return JArray.Parse(metadataString);
        }

        private async Task<Audit> NormalizeRawData(JArray[] rawData, AuditBlob auditBlob, AuditMetadata auditMetadata)
        {
            var (scope, id, name) = this.GetSubscriptionMeta(rawData);
            var (resources, checks) = await this.ParseResourcesAndChecks(rawData, auditMetadata);

            var azMetadata = new JObject
            {
                { "scanner", JToken.FromObject(auditBlob.ParentContainer.Metadata) },
                { "audit", JToken.FromObject(auditMetadata) },
                {
                    "subscription", new JObject
                    {
                        { "scope", scope },
                        { "id", id },
                        { "name", name },
                        { "resources", resources },
                    }
                },
            };

            var auditDate = DateTimeOffset.FromUnixTimeSeconds(auditMetadata.Timestamp).DateTime;
            var audit = new Audit
            {
                Id = auditMetadata.AuditId,
                Date = auditDate,
                ScannerId = $"{auditBlob.ParentContainer.Metadata.Type}/{auditBlob.ParentContainer.Metadata.Id}",
                CheckResults = checks,
                MetadataAzure = new MetadataAzure
                {
                    AuditId = auditMetadata.AuditId,
                    Date = auditDate,
                    JSON = azMetadata.ToString(Formatting.None),
                },
            };

            return audit;
        }

        private (string scope, string id, string name) GetSubscriptionMeta(JArray[] rawData)
        {
            // Based on the assumption, that a single audit belongs to the only one subscription,
            // the iterators below looks for the very first not null scope/id/name
            foreach (var jArray in rawData)
            {
                foreach (var jToken in jArray)
                {
                    // Note, first arguments checks for non-null subscription object
                    var subscription = jToken["SubscriptionContext"];
                    if (subscription?["Scope"] != null &&
                        subscription["SubscriptionId"] != null &&
                        subscription["SubscriptionName"] != null)
                    {
                        var scope = subscription["Scope"].Value<string>();
                        var id = subscription["SubscriptionId"].Value<string>();
                        var name = subscription["SubscriptionName"].Value<string>();
                        return (scope, id, name);
                    }
                }
            }

            return (null, null, null);
        }

        private async Task<(JArray resources, List<CheckResult> checks)> ParseResourcesAndChecks(JArray[] rawData, AuditMetadata auditMeta)
        {
            var checksResults = new List<CheckResult>();
            var resources = new Dictionary<string, JToken>();

            foreach (var jArray in rawData)
            {
                foreach (var item in jArray)
                {
                    // failure in processing one check-result should not impact the rest
                    try
                    {
                        var controlItem = item["ControlItem"];
                        var checkId = $"azsk.{controlItem["ControlID"].Value<string>()}";

                        var id = await this.cache.GetOrAddItem(checkId, () => new Check
                        {
                            Id = checkId,
                            Severity = this.ToSeverity(controlItem["ControlSeverity"].Value<string>()),
                            Category = item["FeatureName"].Value<string>(),
                            Description = $"{controlItem["Description"].Value<string>()}{Environment.NewLine}{controlItem["Rationale"].Value<string>()}",
                            Remediation = controlItem["Recommendation"].Value<string>(),
                        });

                        var checkResult = this.ToCheckResult(item);
                        checkResult.AuditId = auditMeta.AuditId;
                        checkResult.ExternalCheckId = checkId;
                        checkResult.InternalCheckId = id;

                        checksResults.Add(checkResult);
                        if (item["ResourceContext"] != null)
                        {
                            resources.TryAdd(checkResult.ComponentId, item["ResourceContext"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex, "Failed to process one of check results in {AuditId}", auditMeta.AuditId);
                    }
                }
            }

            var resourcesArray = new JArray(resources.Select(i => i.Value));
            return (resourcesArray, checksResults);
        }

        private CheckResult ToCheckResult(JToken rawResult)
        {
            var result = new CheckResult();

            var subscriptionId = rawResult["SubscriptionContext"]["SubscriptionId"].Value<string>();
            if (rawResult["FeatureName"].Value<string>() == "SubscriptionCore")
            {
                result.ComponentId = $"/subscriptions/{subscriptionId}";
            }
            else
            {
                var rgName = rawResult["ResourceContext"]["ResourceGroupName"].Value<string>();
                var objectType = rawResult["ResourceContext"]["ResourceTypeName"].Value<string>();
                var objectName = rawResult["ResourceContext"]["ResourceName"].Value<string>();
                result.ComponentId = $"/subscriptions/{subscriptionId}/resource_group/{rgName}/{objectType}/{objectName}";
            }

            if (!(rawResult["ControlResults"] is JArray controlResults))
            {
                result.Value = CheckValue.NoData;
            }
            else
            {
                // So far there were no case with more than 1 result, but array data type supposes that it's possible.
                // If this appears later, the code below should be reviewed based on real data sample.
                if (controlResults.Count > 1)
                {
                    Logger.Warning("There are {Count} results for {ResourceId}", controlResults.Count, result.ComponentId);
                }

                // TODO: Consider adding parsing ControlResults[].Messages array into result.Message property
                // Likely, this should be done with unique logic per check type, because some Messages might consist of huge json objects (>100LoC)
                result.Value = this.ToCheckResultValue(controlResults.First["ActualVerificationResult"].Value<string>());
            }

            return result;
        }

        private CheckValue ToCheckResultValue(string value)
        {
            return value switch
            {
                "Passed" => CheckValue.Succeeded,
                "Failed" => CheckValue.Failed,
                _ => CheckValue.NoData // At the moment, consider Verify and Manual as NoData result.
            };
        }

        private CheckSeverity ToSeverity(string value)
        {
            return value switch
            {
                "Critical" => CheckSeverity.Critical,
                "High" => CheckSeverity.High,
                "Medium" => CheckSeverity.Medium,
                _ => CheckSeverity.Unknown
            };
        }
    }
}
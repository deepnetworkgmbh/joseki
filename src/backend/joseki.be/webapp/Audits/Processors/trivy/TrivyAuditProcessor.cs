using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Serilog;

using webapp.BlobStorage;
using webapp.Database;
using webapp.Database.Models;

namespace webapp.Audits.Processors.trivy
{
    /// <summary>
    /// Trivy audit processor.
    /// </summary>
    public class TrivyAuditProcessor : IAuditProcessor
    {
        private static readonly ILogger Logger = Log.ForContext<TrivyAuditProcessor>();

        private readonly IBlobStorageProcessor blobStorage;
        private readonly IJosekiDatabase db;
        private readonly CveCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrivyAuditProcessor"/> class.
        /// </summary>
        /// <param name="blobStorage">Blob Storage implementation.</param>
        /// <param name="db">Joseki database implementation.</param>
        /// <param name="cache">CVE cache object.</param>
        public TrivyAuditProcessor(IBlobStorageProcessor blobStorage, IJosekiDatabase db, CveCache cache)
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

            try
            {
                var imageScanResult = await this.NormalizeRawData(auditBlob, auditMetadata);

                await this.db.SaveImageScanResult(imageScanResult);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to process audit {AuditPath}", path);
            }

            await this.blobStorage.MarkAsProcessed(auditBlob);
        }

        private async Task<ImageScanResultWithCVEs> NormalizeRawData(AuditBlob auditBlob, AuditMetadata auditMetadata)
        {
            var auditDate = DateTimeOffset.FromUnixTimeSeconds(auditMetadata.Timestamp).DateTime;
            var scanResult = new ImageScanResultWithCVEs
            {
                Id = auditMetadata.AuditId,
                Date = auditDate,
                ImageTag = auditMetadata.ImageTag,
            };

            if (auditMetadata.AuditResult != "succeeded")
            {
                var path = $"{auditBlob.ParentContainer.Name}/{auditBlob.Name}";
                Logger.Warning(
                    "Audit {AuditPath} result is {AuditResult} due: {FailureReason}",
                    path,
                    auditMetadata.AuditResult,
                    auditMetadata.FailureDescription);
                scanResult.Status = ImageScanStatus.Failed;
                scanResult.Description = TrivyScanDescriptionNormalizer.ToHumanReadable(auditMetadata.FailureDescription);
            }
            else
            {
                var auditResultFilePath = $"{auditBlob.ParentContainer.Name}/{auditMetadata.TrivyAuditPath}";
                var (entities, counters) = await this.ParseScanTargets(auditResultFilePath);
                scanResult.FoundCVEs = entities;
                scanResult.Counters = counters;
                scanResult.Status = ImageScanStatus.Succeeded;

                Logger.Information(
                    "Successfully processed {ImageTag} image scan of {AuditDate} with {ScanSummary}",
                    scanResult.ImageTag,
                    scanResult.Date,
                    scanResult.GetCheckResultMessage());
            }

            return scanResult;
        }

        private async Task<(List<ImageScanToCve> entities, VulnerabilityCounter[] counters)> ParseScanTargets(string resultPath)
        {
            var stream = await this.blobStorage.DownloadFile(resultPath);

            using var sr = new StreamReader(stream);
            var fileContent = sr.ReadToEnd();

            var auditJson = JArray.Parse(fileContent);

            // Keep track of severity-counters and entities separately to avoid blowing out memory usage
            // Some images might have hundreds of different CVEs.
            // If we keep the entire CVE object in memory - the service would consume much more resources than it needs.
            var entities = new List<ImageScanToCve>();
            var countersDict = new Dictionary<CveSeverity, int>();

            // trivy is able to scan OS Packages and some Application Dependencies, each of which is named "target"
            // each audit could consist of 1..N targets and each target could have 0..M CVEs
            foreach (var target in auditJson)
            {
                var targetName = target["Target"].Value<string>();

                if (target["Vulnerabilities"] is JArray vulnerabilities)
                {
                    foreach (var vulnerability in vulnerabilities)
                    {
                        var id = vulnerability["VulnerabilityID"].Value<string>();
                        var installedVersion = vulnerability["InstalledVersion"].Value<string>();

                        var internalCveId = await this.cache.GetOrAddItem(id, () => this.ParseSingleCVE(vulnerability, id));

                        // Prepare entities to insert into database
                        entities.Add(new ImageScanToCve
                        {
                            InternalCveId = internalCveId,
                            Target = targetName,
                            UsedPackageVersion = installedVersion,
                        });

                        // calculate issues by severity to compose a right Check Result message
                        var severity = this.ToSeverity(vulnerability["Severity"].Value<string>());
                        if (countersDict.TryGetValue(severity, out var counter))
                        {
                            countersDict[severity]++;
                        }
                        else
                        {
                            countersDict.Add(severity, 1);
                        }
                    }
                }
            }

            var counters = countersDict.Select(i => new VulnerabilityCounter { Severity = i.Key, Count = i.Value }).ToArray();
            return (entities, counters);
        }

        private CVE ParseSingleCVE(JToken vulnerability, string id)
        {
            var pkg = vulnerability["PkgName"].Value<string>();
            var fixedVersion = vulnerability["FixedVersion"]?.Value<string>();
            var title = vulnerability["Title"]?.Value<string>();
            var description = vulnerability["Description"]?.Value<string>();
            var severity = vulnerability["Severity"].Value<string>();

            var references = new StringBuilder();
            if (vulnerability["References"] is JArray referencesJson)
            {
                foreach (var token in referencesJson)
                {
                    references.AppendLine(token.Value<string>());
                }
            }

            return new CVE
            {
                Id = id,
                Severity = this.ToSeverity(severity),
                PackageName = pkg,
                Description = description,
                Title = title,
                Remediation = !string.IsNullOrEmpty(fixedVersion) ? $"Update the package to version {fixedVersion}" : null,
                References = references.ToString(),
            };
        }

        private CveSeverity ToSeverity(string value)
        {
            return value switch
            {
                "CRITICAL" => CveSeverity.Critical,
                "HIGH" => CveSeverity.High,
                "MEDIUM" => CveSeverity.Medium,
                "LOW" => CveSeverity.Low,
                _ => CveSeverity.Unknown
            };
        }
    }
}
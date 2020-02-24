using System;
using System.Collections.Generic;
using System.IO;
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
                    var imageScanResult = await this.NormalizeRawData(auditBlob, auditMetadata);

                    if (imageScanResult.FoundCVEs.Count > 0)
                    {
                        Logger.Information(
                            "Successfully processed {ImageTag} image scan of {AuditDate} with {FoundCVE} issues",
                            imageScanResult.ImageTag,
                            imageScanResult.Date,
                            imageScanResult.FoundCVEs.Count);
                    }
                    else
                    {
                        Logger.Information(
                            "Successfully processed {ImageTag} image scan of {AuditDate} and found no issues",
                            imageScanResult.ImageTag,
                            imageScanResult.Date);
                    }

                    await this.db.SaveImageScanResult(imageScanResult);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to process audit {AuditPath}", path);
                }
            }

            await this.blobStorage.MarkAsProcessed(auditBlob);
        }

        private async Task<ImageScanResult> NormalizeRawData(AuditBlob auditBlob, AuditMetadata auditMetadata)
        {
            var auditDate = DateTimeOffset.FromUnixTimeSeconds(auditMetadata.Timestamp).DateTime;
            var scanResult = new ImageScanResult
            {
                Id = auditMetadata.AuditId,
                Date = auditDate,
                ImageTag = auditMetadata.ImageTag,
                FoundCVEs = new List<ImageScanToCve>(),
            };

            var auditJson = await this.GetJArrayObject($"{auditBlob.ParentContainer.Name}/{auditMetadata.TrivyAuditPath}");

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

                        var internalCveId = await this.cache.GetOrAddItem(id, () =>
                        {
                            var pkg = vulnerability["PkgName"].Value<string>();
                            var fixedVersion = vulnerability["FixedVersion"]?.Value<string>();
                            var title = vulnerability["Title"]?.Value<string>();
                            var description = vulnerability["Description"]?.Value<string>();
                            var severity = vulnerability["Severity"].Value<string>();

                            var references = new StringBuilder();
                            if (target["References"] is JArray referencesJson)
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
                        });

                        scanResult.FoundCVEs.Add(new ImageScanToCve
                        {
                            CveId = id,
                            InternalCveId = internalCveId,
                            ScanId = scanResult.Id,
                            ImageScan = scanResult,
                            Target = targetName,
                            UsedPackageVersion = installedVersion,
                        });
                    }
                }
            }

            return scanResult;
        }

        private async Task<JArray> GetJArrayObject(string path)
        {
            var stream = await this.blobStorage.DownloadFile(path);

            using var sr = new StreamReader(stream);
            var fileContent = sr.ReadToEnd();

            return JArray.Parse(fileContent);
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
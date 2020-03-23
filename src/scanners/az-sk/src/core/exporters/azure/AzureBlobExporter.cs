using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using core.Configuration;
using core.core;
using core.helpers;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;

using Newtonsoft.Json;

using Serilog;

namespace core.exporters.azure
{
    public class AzureBlobExporter : IExporter
    {
        private static readonly ILogger Logger = Log.ForContext<AzureBlobExporter>();
        private readonly AzBlobExporterConfiguration blobCfg;
        private readonly AzSkConfiguration scannerCfg;
        private readonly string scannerVersion;
        private readonly string azskVersion;

        public AzureBlobExporter(ConfigurationParser config)
        {
            this.blobCfg = config.GetAzBlobConfig();
            this.scannerCfg = config.GetScannerConfig();
            this.scannerVersion = config.ScannerVersion;
            this.azskVersion = config.AzskVersion;
        }

        /// <inheritdoc />
        public async Task UploadAsync(SubscriptionScanDetails details, CancellationToken cancellation)
        {
            Logger.Information("Uploading scan details for {AzureSubscription} with result {ScanResult}", details.Subscription, details.ScanResult);

            var folder = BlobFolderNameGenerator.ForDate(details.Timestamp);

            var metadata = await this.UploadAuditResult(details, folder, cancellation);

            await this.UploadAuditMetadata(folder, metadata, cancellation);

            await this.UpdateScannerMetadata(cancellation);
        }

        public async Task UpdateScannerMetadata(CancellationToken cancellation)
        {
            Logger.Information("Updating scanner metadata");

            ScannerMetadata scannerMeta = null;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var shortId = this.scannerCfg.Id.Substring(0, 8);
            var metadataUrl = new Uri($"{this.blobCfg.BasePath}/azsk-{shortId}?{this.blobCfg.Sas}");

            var metaBlob = new CloudBlockBlob(metadataUrl);
            try
            {
                if (await metaBlob.ExistsAsync(cancellation))
                {
                    var content = await metaBlob.DownloadTextAsync(cancellation);

                    if (!string.IsNullOrEmpty(content))
                    {
                        scannerMeta = JsonConvert.DeserializeObject<ScannerMetadata>(content);

                        if (scannerMeta?.Heartbeat < now)
                        {
                            scannerMeta.Heartbeat = now;
                            scannerMeta.Periodicity = this.scannerCfg.Periodicity;
                            scannerMeta.HeartbeatPeriodicity = this.blobCfg.HeartbeatPeriodicity;
                        }
                    }
                }

                if (scannerMeta == null)
                {
                    scannerMeta = new ScannerMetadata
                    {
                        Id = this.scannerCfg.Id,
                        Periodicity = this.scannerCfg.Periodicity,
                        Type = "azsk",
                        Heartbeat = now,
                        HeartbeatPeriodicity = this.blobCfg.HeartbeatPeriodicity,
                    };
                }

                var stringMetadata = JsonConvert.SerializeObject(scannerMeta);
                await metaBlob.UploadTextAsync(stringMetadata, cancellation);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Scanner metadata update failed");
            }

            Logger.Information("Scanner metadata update was finished");
        }

        /// <inheritdoc />
        public Task UploadBulkAsync(IEnumerable<SubscriptionScanDetails> results, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        private async Task<AuditMetadata> UploadAuditResult(SubscriptionScanDetails details, string folder, CancellationToken cancellation)
        {
            var metadata = new AuditMetadata
            {
                AuditId = Guid.NewGuid().ToString(),
                ScannerVersion = this.scannerVersion,
                Periodicity = this.scannerCfg.Periodicity,
                AzSkVersion = this.azskVersion,
                Timestamp = ((DateTimeOffset)details.Timestamp).ToUnixTimeSeconds(),
            };

            if (details.ScanResult == ScanResult.Succeeded)
            {
                try
                {
                    var tasks = details
                        .ResultFiles
                        .Select(rf => this.UploadSingleAuditFile(details, folder, cancellation, rf))
                        .ToArray();
                    var taskResults = await Task.WhenAll(tasks);

                    metadata.AuditResult = "succeeded";
                    metadata.AzSkAuditPaths = tasks.Select(i => i.Result).ToArray();
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Audit result upload failed");
                    metadata.AuditResult = "upload-failed";
                    metadata.FailureDescription = ex.Message;
                }
            }
            else
            {
                // TODO: add failure description
                metadata.AuditResult = "audit-failed";
            }

            return metadata;
        }

        private async Task<string> UploadSingleAuditFile(SubscriptionScanDetails details, string folder, CancellationToken cancellation, ResultFile resultFile)
        {
            var auditPath = $"{folder}/{resultFile.FileName}";
            Logger.Information("Uploading scan result for {AzureSubscription} to {AuditPath}", details.Subscription, auditPath);

            var resultUrl = new Uri($"{this.blobCfg.BasePath}/{auditPath}?{this.blobCfg.Sas}");
            var resultBlob = new CloudBlockBlob(resultUrl);
            await resultBlob.UploadFromFileAsync(
                resultFile.FullPath,
                AccessCondition.GenerateEmptyCondition(),
                new BlobRequestOptions
                {
                    RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(10), 3),
                },
                new OperationContext(),
                cancellation);

            return auditPath;
        }

        private async Task UploadAuditMetadata(string folder, AuditMetadata metadata, CancellationToken cancellation)
        {
            var metadataPath = $"{folder}/meta";
            Logger.Information("Uploading metadata to {MetadataPath}", metadataPath);

            try
            {
                var metadataUrl = new Uri($"{this.blobCfg.BasePath}/{metadataPath}?{this.blobCfg.Sas}");
                var auditMetaBlob = new CloudBlockBlob(metadataUrl);
                var stringMetadata = JsonConvert.SerializeObject(metadata);
                await auditMetaBlob.UploadTextAsync(
                    stringMetadata,
                    Encoding.UTF8,
                    AccessCondition.GenerateEmptyCondition(),
                    new BlobRequestOptions
                    {
                        RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(10), 3),
                    },
                    new OperationContext(),
                    cancellation);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Audit metadata upload failed");
            }
        }
    }
}
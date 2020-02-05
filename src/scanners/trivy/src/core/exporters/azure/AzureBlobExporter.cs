using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private readonly TrivyAzblobScannerConfiguration config;

        public AzureBlobExporter(TrivyAzblobScannerConfiguration config)
        {
            this.config = config;
        }

        /// <inheritdoc />
        public async Task UploadAsync(ImageScanDetails details, CancellationToken cancellation)
        {
            Logger.Information("Uploading scan details for {Image} with result {ScanResult}", details.Image.FullName, details.ScanResult);

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

            var shortId = this.config.Id.Substring(0, 8);
            var metadataUrl = new Uri($"{this.config.AzureBlobBaseUrl}/trivy-{shortId}?{this.config.AzureBlobSasToken}");

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
                        }
                    }
                }

                if (scannerMeta == null)
                {
                    scannerMeta = new ScannerMetadata
                    {
                        Id = this.config.Id,
                        Periodicity = "on-message",
                        Type = "trivy",
                        Heartbeat = now,
                        HeartbeatPeriodicity = this.config.HeartbeatPeriodicity,
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
        public Task UploadBulkAsync(IEnumerable<ImageScanDetails> results, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        private async Task<AuditMetadata> UploadAuditResult(ImageScanDetails details, string folder, CancellationToken cancellation)
        {
            var auditPath = $"{folder}/scan-result.json";
            Logger.Information("Uploading scan result for {Image} to {AuditPath}", details.Image.FullName, auditPath);

            var metadata = new AuditMetadata
            {
                AuditId = Guid.NewGuid().ToString(),
                ScannerVersion = this.config.Version,
                TrivyVersion = this.config.TrivyVersion,
                Timestamp = ((DateTimeOffset)details.Timestamp).ToUnixTimeSeconds(),
            };

            if (details.ScanResult == ScanResult.Succeeded)
            {
                try
                {
                    var resultUrl = new Uri($"{this.config.AzureBlobBaseUrl}/{auditPath}?{this.config.AzureBlobSasToken}");
                    var resultBlob = new CloudBlockBlob(resultUrl);
                    await resultBlob.UploadTextAsync(
                        details.Payload,
                        Encoding.UTF8,
                        AccessCondition.GenerateEmptyCondition(),
                        new BlobRequestOptions
                        {
                            RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(10), 3),
                        },
                        new OperationContext(),
                        cancellation);

                    metadata.TrivyAuditPath = auditPath;
                    metadata.AuditResult = "succeeded";
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
                metadata.AuditResult = "audit-failed";
                metadata.FailureDescription = details.Payload;
            }

            return metadata;
        }

        private async Task UploadAuditMetadata(string folder, AuditMetadata metadata, CancellationToken cancellation)
        {
            var metadataPath = $"{folder}/meta";
            Logger.Information("Uploading metadata to {MetadataPath}", metadataPath);

            try
            {
                var metadataUrl = new Uri($"{this.config.AzureBlobBaseUrl}/{metadataPath}?{this.config.AzureBlobSasToken}");
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
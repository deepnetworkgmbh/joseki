﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Serilog;

using webapp.Configuration;
using webapp.Exceptions;

namespace webapp.BlobStorage
{
    /// <summary>
    /// Azure Blob Storage implementation of Blob Storage Processor.
    /// </summary>
    public class AzureBlobStorageProcessor : IBlobStorageProcessor
    {
        /// <summary>
        /// Metadata tag name for "processed" audits.
        /// </summary>
        public const string ProcessedMetadataKey = "processed";

        private static readonly ILogger Logger = Log.ForContext<AzureBlobStorageProcessor>();

        private readonly JosekiConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageProcessor"/> class.
        /// </summary>
        /// <param name="config">The application configuration object.</param>
        public AzureBlobStorageProcessor(ConfigurationParser config)
        {
            this.config = config.Get();
        }

        /// <inheritdoc />
        public async Task<ScannerContainer[]> ListAllContainers()
        {
            var uri = new Uri($"{this.config.AzureBlob.BasePath}/?{this.config.AzureBlob.Sas}");
            var client = new Azure.Storage.Blobs.BlobServiceClient(uri);

            var containers = new List<ScannerContainer>();
            await foreach (var container in client.GetBlobContainersAsync())
            {
                // skip "system" containers
                if (!container.Name.StartsWith("0-"))
                {
                    containers.Add(new ScannerContainer(container.Name));
                }
            }

            return containers.ToArray();
        }

        /// <inheritdoc />
        public async Task<Stream> DownloadFile(string relativePath)
        {
            try
            {
                var uri = new Uri($"{this.config.AzureBlob.BasePath}/{relativePath}?{this.config.AzureBlob.Sas}");
                var client = new BlobClient(uri);

                var blob = await client.DownloadAsync();
                return blob.Value.Content;
            }
            catch (RequestFailedException ex)
            {
                Logger.Warning(ex, "Failed to download file from {BlobRelativePath}", relativePath);
                throw new JosekiException(ex, $"Failed to download file from {relativePath}");
            }
        }

        /// <inheritdoc />
        public async Task<AuditBlob[]> GetUnprocessedAudits(ScannerContainer container)
        {
            var uri = new Uri($"{this.config.AzureBlob.BasePath}/{container.Name}?{this.config.AzureBlob.Sas}");
            var client = new Azure.Storage.Blobs.BlobContainerClient(uri);

            var blobs = new List<AuditBlob>();
            await foreach (var blob in client.GetBlobsAsync(BlobTraits.Metadata))
            {
                // skip all not metadata file.
                if (!blob.Name.EndsWith("meta"))
                {
                    continue;
                }

                if (blob.Metadata == null || !blob.Metadata.ContainsKey(ProcessedMetadataKey))
                {
                    blobs.Add(new AuditBlob
                    {
                        Name = blob.Name,
                        ParentContainer = container,
                    });
                }
            }

            return blobs.ToArray();
        }

        /// <inheritdoc />
        public async Task MarkAsProcessed(AuditBlob auditBlob)
        {
            var uri = new Uri($"{this.config.AzureBlob.BasePath}/{auditBlob.ParentContainer.Name}/{auditBlob.Name}?{this.config.AzureBlob.Sas}");
            var client = new Azure.Storage.Blobs.BlobClient(uri);
            await client.SetMetadataAsync(new Dictionary<string, string>() { { ProcessedMetadataKey, string.Empty } });
        }

        /// <summary>
        /// The method returns all audit files in the container.
        /// </summary>
        /// <param name="container">The container with audits.</param>
        /// <returns>Array of Audit blobs.</returns>
        public async Task MarkAllAuditsAsUnprocessed(ScannerContainer container)
        {
            var uri = new Uri($"{this.config.AzureBlob.BasePath}/{container.Name}?{this.config.AzureBlob.Sas}");
            var client = new Azure.Storage.Blobs.BlobContainerClient(uri);

            await foreach (var blob in client.GetBlobsAsync())
            {
                // skip all not metadata file.
                if (!blob.Name.EndsWith("meta"))
                {
                    continue;
                }

                await client.GetBlobClient(blob.Name).SetMetadataAsync(new Dictionary<string, string>());
            }
        }
    }
}
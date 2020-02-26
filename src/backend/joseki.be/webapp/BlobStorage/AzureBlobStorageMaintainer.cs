using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

using Serilog;

using webapp.Configuration;

namespace webapp.BlobStorage
{
    /// <summary>
    /// Azure blob storage implementation of IBlobStorageMaintainer.
    /// </summary>
    public class AzureBlobStorageMaintainer : IBlobStorageMaintainer
    {
        private const string ArchiveContainerName = "0-archive";

        private const string ArchivedAtMetadataKeyName = "auditArchivedAt";

        private static readonly ILogger Logger = Log.ForContext<AzureBlobStorageMaintainer>();

        private readonly JosekiConfiguration config;
        private readonly BlobServiceClient storageClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageMaintainer"/> class.
        /// </summary>
        /// <param name="parser">The application configuration object.</param>
        public AzureBlobStorageMaintainer(ConfigurationParser parser)
        {
            this.config = parser.Get();
            var connectionString = string.Format(this.config.AzureBlob.ConnectionString, this.config.AzureBlob.AccountName, this.config.AzureBlob.AccountKey);
            this.storageClient = new BlobServiceClient(connectionString);
        }

        /// <param name="cancellation"></param>
        /// <inheritdoc />
        public async Task<int> MoveProcessedBlobsToArchive(CancellationToken cancellation)
        {
            Logger.Information("Archive processed audits was started");
            var archiveContainerClient = this.storageClient.GetBlobContainerClient(ArchiveContainerName);

            var blobsToCopy = new List<(AuditBlob blob, CopyFromUriOperation copyOperation)>();

            await foreach (var container in this.storageClient.GetBlobContainersAsync(cancellationToken: cancellation))
            {
                // skip "system" containers
                if (container.Name.StartsWith("0-"))
                {
                    continue;
                }

                var scannerContainerClient = this.storageClient.GetBlobContainerClient(container.Name);

                var scannerContainer = new ScannerContainer(container.Name);
                var regularBlobs = new List<string>();
                var auditsToArchive = new Dictionary<string, List<string>>();

                // Azure Storage stores all files inside container without any hierarchy. Thus joseki has to build the hierarchy here:
                // - Find processed audits by presence of Processed tag on metadata file
                // - All blobs, that have the same prefix as processed metadata file are moved to the same dictionary item
                // - one dictionary item - is one audit folder
                await foreach (var blob in scannerContainerClient.GetBlobsAsync(BlobTraits.Metadata, cancellationToken: cancellation))
                {
                    // not metadata files are stored in memory to be placed to corresponding dictionary cells later
                    if (!blob.Name.EndsWith("meta") && blob.Name != container.Name)
                    {
                        regularBlobs.Add(blob.Name);
                        continue;
                    }

                    // if metadata file has processed tag - create a dictionary key-value pair
                    if (blob.Metadata == null || blob.Metadata.ContainsKey(AzureBlobStorageProcessor.ProcessedMetadataKey))
                    {
                        // the folder name is file_path without "/meta" at the end.
                        var auditFolderName = blob.Name[..^5];
                        auditsToArchive.Add(auditFolderName, new List<string> { blob.Name });
                    }
                }

                // iterate through unsorted blob files and place them to dictionary items according to their prefix
                foreach (var regularBlob in regularBlobs)
                {
                    var auditFolderName = regularBlob[..regularBlob.IndexOf('/')];
                    if (auditsToArchive.TryGetValue(auditFolderName, out var fileNames))
                    {
                        fileNames.Add(regularBlob);
                    }
                }

                // Move each audit folder to Archive container
                // to move files, here is used Copy From URL method, which avoid loading blobs to joseki side
                // https://docs.microsoft.com/en-us/rest/api/storageservices/copy-blob-from-url
                // to generate the URL, joseki uses read-only SAS token with one hour expiration time.
                foreach (var blobToArchive in auditsToArchive.SelectMany(i => i.Value))
                {
                    Logger.Information("Moving to archive {BlobPath}", $"{container.Name}/{blobToArchive}");

                    var fileToCopyUri = $"{this.config.AzureBlob.BasePath}/{container.Name}/{blobToArchive}";

                    var archiveBlobClient = archiveContainerClient.GetBlobClient($"{container.Name}/{blobToArchive}");
                    var operation = await archiveBlobClient.StartCopyFromUriAsync(
                        this.GetUniqueTemporarySasUrl(fileToCopyUri),
                        new Dictionary<string, string>
                        {
                            { ArchivedAtMetadataKeyName, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                        },
                        cancellationToken: cancellation);
                    blobsToCopy.Add((new AuditBlob { Name = blobToArchive, ParentContainer = scannerContainer }, operation));
                }
            }

            // wait copy operations to complete, but not more than 1 hour
            var linkedTokens = CancellationTokenSource.CreateLinkedTokenSource(
                cancellation,
                new CancellationTokenSource(TimeSpan.FromHours(1)).Token);
            foreach (var (_, copyOperation) in blobsToCopy)
            {
                await copyOperation.WaitForCompletionAsync(linkedTokens.Token);
            }

            // each copied blob could be deleted
            var deleteBlobTasks = blobsToCopy
                .Where(i => i.copyOperation.HasCompleted)
                .Select(i => this.DeleteBlob(i.blob))
                .ToArray();
            await Task.WhenAll(deleteBlobTasks);

            // TODO: retry operations automatically?
            // TODO: also, consider moving in two stages: first move all not metadata files, the second - move metadata too.
            // but now only log errors
            if (deleteBlobTasks.Length != blobsToCopy.Count)
            {
                var blobPaths = blobsToCopy
                    .Where(i => i.copyOperation.HasCompleted)
                    .Select(i => $"{i.blob.ParentContainer.Name}/{i.blob.Name}");

                Logger.Error("{FailedCount} blobs failed to be moved to Archive:", blobsToCopy.Count - deleteBlobTasks.Length);
                foreach (var blobPath in blobPaths)
                {
                    Logger.Warning("Blob {BlobPath} failed to be copied", blobPath);
                }
            }

            Logger.Information("Archive processed audits was finished");

            return deleteBlobTasks.Length;
        }

        /// <param name="cancellation"></param>
        /// <inheritdoc />
        public async Task<int> CleanupArchive(CancellationToken cancellation)
        {
            Logger.Information("Archive cleanup was {State}", "started");
            var deletedBlobs = 0;

            var archiveContainerClient = this.storageClient.GetBlobContainerClient(ArchiveContainerName);

            var expirationTime = DateTimeOffset
                .UtcNow
                .AddDays(-this.config.Watchmen.ArchiveTtlDays)
                .ToUnixTimeSeconds();

            await foreach (var blob in archiveContainerClient.GetBlobsAsync(BlobTraits.Metadata, cancellationToken: cancellation))
            {
                try
                {
                    var unixEpoch = long.Parse(blob.Metadata[ArchivedAtMetadataKeyName]);
                    if (unixEpoch < expirationTime)
                    {
                        Logger.Information("Deleting blob {BlobPath} from Archive", blob.Name);
                        deletedBlobs++;
                        await archiveContainerClient.DeleteBlobAsync(blob.Name, cancellationToken: cancellation);
                    }
                }
                catch (Exception ex)
                {
                    // it should not happen normally, so it should not stop other blobs from processing :)
                    // but just in case, here is the logger
                    Logger.Warning(ex, "Failed to parse archive-date from {BlobPath}", blob.Name);
                }
            }

            Logger.Information("Archive cleanup was {State}", "finished");

            return deletedBlobs;
        }

        /// <summary>
        /// Deletes everything from the container.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task CleanupContainer(ScannerContainer container)
        {
            var uri = new Uri($"{this.config.AzureBlob.BasePath}/{container.Name}?{this.config.AzureBlob.Sas}");
            var client = new BlobContainerClient(uri);

            await foreach (var blob in client.GetBlobsAsync())
            {
                if (blob.Name == container.Name)
                {
                    continue;
                }

                await client.GetBlobClient(blob.Name).DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
        }

        private Uri GetUniqueTemporarySasUrl(string blobUri)
        {
            // Create a service level SAS that only allows reading
            var sas = new AccountSasBuilder
            {
                Services = AccountSasServices.Blobs,
                ResourceTypes = AccountSasResourceTypes.Object,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
            };
            sas.SetPermissions(AccountSasPermissions.Read);

            var credential = new StorageSharedKeyCredential(this.config.AzureBlob.AccountName, this.config.AzureBlob.AccountKey);

            var sasUri = new UriBuilder(blobUri)
            {
                Query = sas.ToSasQueryParameters(credential).ToString(),
            };

            return sasUri.Uri;
        }

        private async Task DeleteBlob(AuditBlob blob)
        {
            var uri = new Uri($"{this.config.AzureBlob.BasePath}/{blob.ParentContainer.Name}?{this.config.AzureBlob.Sas}");
            var client = new BlobContainerClient(uri);
            await client.GetBlobClient(blob.Name).DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }
    }
}
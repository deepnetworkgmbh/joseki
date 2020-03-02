using System;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using webapp.BlobStorage;
using webapp.Configuration;
using webapp.Infrastructure;

namespace webapp.BackgroundJobs
{
    /// <summary>
    /// Archive processed audits and delete expired archive records.
    /// </summary>
    public class ArchiveWatchman
    {
        private static readonly ILogger Logger = Log.ForContext<ArchiveWatchman>();

        private readonly IBlobStorageMaintainer blobStorage;
        private readonly JosekiConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveWatchman"/> class.
        /// </summary>
        /// <param name="blobStorage">The Blob Storage.</param>
        /// <param name="config">Joseki Backend configuration.</param>
        public ArchiveWatchman(IBlobStorageMaintainer blobStorage, ConfigurationParser config)
        {
            this.blobStorage = blobStorage;
            this.config = config.Get();
        }

        /// <summary>
        /// Every Watchmen.ArchiverPeriodicityHours hours:
        /// - removes expired audits from Archive,
        /// - moves processed audits to Archive.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task Watch(CancellationToken cancellation)
        {
            var initialized = false;
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    Logger.Information("Archiver watchman is going out.");

                    try
                    {
                        // first, delete stale records, then move processed audits to archive
                        // if the order is opposite - Cleanup task has a bit more work to do
                        var deleted = await this.blobStorage.CleanupArchive(cancellation);
                        var archived = await this.blobStorage.MoveProcessedBlobsToArchive(cancellation);

                        if (!initialized)
                        {
                            JosekiStateManager.ArchiverIsInitialized();
                            initialized = true;
                        }

                        Logger.Information("Archiver archived {ArchivedCount} and deleted {DeletedCount} blobs", archived, deleted);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Archiver watchman failed now, but they comes back tomorrow");
                    }

                    Logger.Information("Archiver watchman finished the detour.");
                    await Task.Delay(TimeSpan.FromHours(this.config.Watchmen.ArchiverPeriodicityHours), cancellation);
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Information(ex, "Archiver watchman was canceled");
                }
            }
        }
    }
}
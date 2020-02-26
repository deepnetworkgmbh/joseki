using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Serilog;

using webapp.Audits;
using webapp.BlobStorage;
using webapp.Configuration;

namespace webapp.BackgroundJobs
{
    /// <summary>
    /// ScannerContainers watchman is responsible for following actual state of root-level containers in Blob Storage
    /// and maintaining up-to-date work items in Scheduler Assistant.
    /// </summary>
    public class ScannerContainersWatchman
    {
        private static readonly ILogger Logger = Log.ForContext<ScannerContainersWatchman>();

        private readonly IBlobStorageProcessor blobStorage;
        private readonly SchedulerAssistant scheduler;
        private readonly JosekiConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerContainersWatchman"/> class.
        /// </summary>
        /// <param name="blobStorage">The Blob Storage.</param>
        /// <param name="scheduler">The scheduler assistant.</param>
        /// <param name="config">Joseki Backend configuration.</param>
        public ScannerContainersWatchman(IBlobStorageProcessor blobStorage, SchedulerAssistant scheduler, ConfigurationParser config)
        {
            this.blobStorage = blobStorage;
            this.scheduler = scheduler;
            this.config = config.Get();
        }

        /// <summary>
        /// Every Watchmen.ScannerContainersPeriodicity seconds lists containers from Blob storage and updates scheduler-assistant.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task Watch(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    Logger.Information("Scanner Containers watchman is going out.");

                    var containers = await this.blobStorage.ListAllContainers();
                    var metadataTasks = containers
                        .Select(this.DownloadAndParseMetadata)
                        .ToArray();
                    await Task.WhenAll(metadataTasks);

                    var schedulerItems = metadataTasks.Select(t => t.Result).ToArray();
                    this.scheduler.UpdateWorkingItems(schedulerItems);

                    Logger.Information("Scanner Containers watchman finished the detour.");
                    await Task.Delay(TimeSpan.FromSeconds(this.config.Watchmen.ScannerContainersPeriodicity), cancellation);
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Information(ex, "Scanner Containers watchman was canceled");
                }
            }
        }

        private async Task<ScannerContainer> DownloadAndParseMetadata(ScannerContainer container)
        {
            const int oneHourSeconds = 3600;

            var stream = await this.blobStorage.DownloadFile(container.MetadataFilePath);

            using var sr = new StreamReader(stream);
            var metadataString = sr.ReadToEnd();

            container.Metadata = JsonConvert.DeserializeObject<ScannerMetadata>(metadataString);

            var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var delta = unixNow - container.Metadata.Heartbeat;

            if (delta < container.Metadata.HeartbeatPeriodicity)
            {
                // do nothing
            }
            else if (delta < oneHourSeconds || delta < container.Metadata.HeartbeatPeriodicity * 2)
            {
                var lastExecution = DateTimeOffset.FromUnixTimeSeconds(container.Metadata.Heartbeat);
                Logger.Warning("Scanner {ScannerContainer} keeps silence since {LastExecution}", container.Name, lastExecution);
            }
            else
            {
                var lastExecution = DateTimeOffset.FromUnixTimeSeconds(container.Metadata.Heartbeat);
                Logger.Error("Scanner {ScannerContainer} keeps silence since {LastExecution}", container.Name, lastExecution);
            }

            return container;
        }
    }
}
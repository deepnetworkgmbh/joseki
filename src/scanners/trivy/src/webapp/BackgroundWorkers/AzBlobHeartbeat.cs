using System;
using System.Threading;
using System.Threading.Tasks;

using core.exporters.azure;

using Microsoft.Extensions.Hosting;

using Serilog;

using webapp.Configuration;

namespace webapp.BackgroundWorkers
{
    /// <summary>
    /// Updates associated with current scanner metadata in Azure blob Storage each N seconds.
    /// </summary>
    public class AzBlobHeartbeat : IHostedService
    {
        private static readonly ILogger Logger = Log.ForContext<AzBlobHeartbeat>();

        private readonly ConfigurationParser configuration;

        private Timer heartbeat;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzBlobHeartbeat"/> class.
        /// </summary>
        /// <param name="configuration">The scanner configuration.</param>
        public AzBlobHeartbeat(ConfigurationParser configuration)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.configuration.Get().Exporter is AzBlobExporterConfiguration)
            {
                Logger.Information("Starting Azure blob heartbeat");

                var config = this.configuration.GetTrivyAzConfig();
                var exporter = new AzureBlobExporter(config);

                this.heartbeat = new Timer(
                    async (state) => await exporter.UpdateScannerMetadata(cancellationToken),
                    null,
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(config.HeartbeatPeriodicity));

                Logger.Information("Azure Blob heartbeat service was started");
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.heartbeat == null)
            {
                return;
            }

            Logger.Information("Stopping Azure Blob heartbeat");

            await this.heartbeat.DisposeAsync();

            Logger.Information("Azure Blob heartbeat service was stopped");
        }
    }
}
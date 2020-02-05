using System;
using System.Threading;
using System.Threading.Tasks;

using core.scanners;

using Microsoft.Extensions.Hosting;

using Serilog;

using webapp.Infrastructure;

namespace webapp.BackgroundWorkers
{
    /// <summary>
    /// Updates Trivy database every hour.
    /// </summary>
    public class TrivyDbUpdater : IHostedService
    {
        private static readonly ILogger Logger = Log.ForContext<TrivyDbUpdater>();

        private readonly Trivy trivyScanner;

        private Timer trivyUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrivyDbUpdater"/> class.
        /// </summary>
        /// <param name="scanner">Scanner instance.</param>
        public TrivyDbUpdater(IScanner scanner)
        {
            this.trivyScanner = scanner as Trivy;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Only if scanner is Trivy, the object has to register db-updater.
            if (this.trivyScanner != null)
            {
                try
                {
                    await this.trivyScanner.UpdateDb();
                    StateManager.SetReady();
                    StateManager.SetLive();

                    this.trivyUpdater = new Timer(async (state) => await this.UpdateDb(), null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, "Failed to initialize trivy database");
                    throw;
                }
            }
            else
            {
                StateManager.SetReady();
                StateManager.SetLive();
            }
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.trivyUpdater != null)
            {
                await this.trivyUpdater.DisposeAsync();
            }
        }

        private async Task UpdateDb()
        {
            try
            {
                await this.trivyScanner.UpdateDb();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update trivy database");
            }
        }
    }
}
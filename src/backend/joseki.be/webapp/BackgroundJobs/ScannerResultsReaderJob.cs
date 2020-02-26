using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace webapp.BackgroundJobs
{
    /// <summary>
    /// The object is responsible for getting new audit-results from Blob Storage.
    /// To accomplish the task it glues together ScannerContainers Watchman, Scheduler Assistant, and Audit Processors Factory.
    /// </summary>
    public class ScannerResultsReaderJob : BackgroundService
    {
        private static readonly ILogger Logger = Log.ForContext<ScannerResultsReaderJob>();

        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerResultsReaderJob"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public ScannerResultsReaderJob(IServiceProvider services)
        {
            this.services = services;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.Information("Starting audit processors");

                using var scope = this.services.CreateScope();
                var scannerContainersWatchman = scope.ServiceProvider.GetRequiredService<ScannerContainersWatchman>();
                var schedulerAssistant = scope.ServiceProvider.GetRequiredService<SchedulerAssistant>();

                await Task.WhenAll(
                    schedulerAssistant.Run(stoppingToken),
                    scannerContainersWatchman.Watch(stoppingToken));

                Logger.Information("Audit processors finished the work");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Scanner Result Listener job failed");
                throw;
            }
        }
    }
}
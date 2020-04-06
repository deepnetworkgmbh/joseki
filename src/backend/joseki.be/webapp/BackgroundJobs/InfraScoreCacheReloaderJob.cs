using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Serilog;

namespace webapp.BackgroundJobs
{
    /// <summary>
    /// The object is responsible for keeping actual records in Infrastructure-Score cache.
    /// </summary>
    public class InfraScoreCacheReloaderJob : BackgroundService
    {
        private static readonly ILogger Logger = Log.ForContext<InfraScoreCacheReloaderJob>();

        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfraScoreCacheReloaderJob"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public InfraScoreCacheReloaderJob(IServiceProvider services)
        {
            this.services = services;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.Information("Starting infra-score cache-reloader job");

                var watchman = new InfraScoreCacheWatchman(this.services);
                await watchman.Watch(stoppingToken);

                Logger.Information("Infra-score cache-reloader job was finished");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Infra-score cache-reloader job failed");
                throw;
            }
        }
    }
}
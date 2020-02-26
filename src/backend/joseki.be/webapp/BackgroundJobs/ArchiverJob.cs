using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace webapp.BackgroundJobs
{
    /// <summary>
    /// The object is responsible for moving processed audits to Archive.
    /// </summary>
    public class ArchiverJob : BackgroundService
    {
        private static readonly ILogger Logger = Log.ForContext<ArchiverJob>();

        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiverJob"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public ArchiverJob(IServiceProvider services)
        {
            this.services = services;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.Information("Starting archiver job");

                using var scope = this.services.CreateScope();
                var archiveWatchman = scope.ServiceProvider.GetRequiredService<ArchiveWatchman>();

                await archiveWatchman.Watch(stoppingToken);

                Logger.Information("Archiver job was finished");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Archiver job failed");
                throw;
            }
        }
    }
}
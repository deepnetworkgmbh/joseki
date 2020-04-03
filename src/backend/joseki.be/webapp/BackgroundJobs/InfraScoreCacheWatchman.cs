using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

using webapp.Configuration;
using webapp.Database.Cache;
using webapp.Infrastructure;

namespace webapp.BackgroundJobs
{
    /// <summary>
    /// Regularly reloads Infrastructure Score cache items.
    /// </summary>
    public class InfraScoreCacheWatchman
    {
        private static readonly ILogger Logger = Log.ForContext<InfraScoreCacheWatchman>();

        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfraScoreCacheWatchman"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public InfraScoreCacheWatchman(IServiceProvider services)
        {
            this.services = services;
        }

        /// <summary>
        /// Every Watchmen.InfraScorePeriodicityHours hours forces cache reload.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task Watch(CancellationToken cancellation)
        {
            var initialized = false;
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    Logger.Information("InfraScoreCache watchman is going out.");

                    using var scope = this.services.CreateScope();
                    var cache = scope.ServiceProvider.GetRequiredService<IInfrastructureScoreCache>();
                    var config = scope.ServiceProvider.GetRequiredService<ConfigurationParser>().Get();

                    try
                    {
                        await cache.ReloadEntireCache();

                        if (!initialized)
                        {
                            JosekiStateManager.ScoreCacheIsInitialized();
                            initialized = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "InfraScoreCache watchman failed now, but they comes back later");
                    }

                    Logger.Information("InfraScoreCache watchman finished the detour.");
                    await Task.Delay(TimeSpan.FromHours(config.Watchmen.InfraScorePeriodicityHours), cancellation);
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Information(ex, "InfraScoreCache watchman was canceled");
                }
            }
        }
    }
}
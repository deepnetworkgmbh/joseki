using System;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using webapp.Configuration;
using webapp.Database;
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

        private readonly IInfrastructureScoreCache cache;
        private readonly JosekiConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfraScoreCacheWatchman"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="config">Joseki Backend configuration.</param>
        public InfraScoreCacheWatchman(IInfrastructureScoreCache cache, ConfigurationParser config)
        {
            this.cache = cache;
            this.config = config.Get();
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

                    try
                    {
                        await this.cache.ReloadEntireCache();

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
                    await Task.Delay(TimeSpan.FromHours(this.config.Watchmen.InfraScorePeriodicityHours), cancellation);
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Information(ex, "InfraScoreCache watchman was canceled");
                }
            }
        }
    }
}
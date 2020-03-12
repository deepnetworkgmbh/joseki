using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Serilog;

using webapp.Database.Models;
using webapp.Models;

namespace webapp.Database
{
    /// <summary>
    /// Takes care of keeping pre-calculated counter-summaries.
    /// </summary>
    public class InfrastructureScoreCache : IInfrastructureScoreCache
    {
        private static readonly ConcurrentDictionary<string, CacheItem> Cache = new ConcurrentDictionary<string, CacheItem>();
        private static readonly ILogger Logger = Log.ForContext<InfrastructureScoreCache>();

        private readonly IInfraScoreDbWrapper db;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfrastructureScoreCache"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        public InfrastructureScoreCache(IInfraScoreDbWrapper db)
        {
            this.db = db;
        }

        /// <inheritdoc />
        public async Task ReloadEntireCache()
        {
            Logger.Information("Reloading Infrastructure Score cache");
            var sw = new Stopwatch();
            sw.Start();

            var components = await this.db.GetAllComponentsIds();
            foreach (var component in components)
            {
                var audits = await this.db.GetLastMonthAudits(component);
                foreach (var auditEntity in audits)
                {
                    var summary = await this.db.GetCounterSummariesForAudit(auditEntity.Id);
                    var cacheItem = new CacheItem
                    {
                        AuditDate = auditEntity.Date.Date,
                        ComponentId = auditEntity.ComponentId,
                        Summary = summary,
                    };
                    Cache.AddOrUpdate(cacheItem.Key, key => cacheItem, (key, old) => cacheItem);
                }
            }

            // calculate overall-infrastructure summaries
            foreach (var grouping in Cache.Values.GroupBy(i => i.AuditDate))
            {
                var summary = new CountersSummary();
                foreach (var item in grouping)
                {
                    summary.Add(item.Summary);
                }

                var cacheItem = new CacheItem
                {
                    AuditDate = grouping.Key,
                    ComponentId = Audit.OverallId,
                    Summary = summary,
                };
                Cache.AddOrUpdate(cacheItem.Key, key => cacheItem, (key, old) => cacheItem);
            }

            sw.Stop();
            Logger.Information("Reloading Infrastructure Score cache took {Elapsed}", sw.Elapsed);
        }

        /// <inheritdoc />
        public async Task<CountersSummary> GetCountersSummary(string componentId, DateTime date)
        {
            var cacheKey = CacheItem.GetKey(componentId, date);
            if (Cache.TryGetValue(cacheKey, out var cachedItem))
            {
                return cachedItem.RequiresUpdate
                    ? await this.ReloadCacheItem(componentId, date)
                    : cachedItem.Summary;
            }
            else
            {
                return await this.ReloadCacheItem(componentId, date);
            }
        }

        private async Task<CountersSummary> ReloadCacheItem(string componentId, DateTime date)
        {
            Logger.Information("Reloading {ComponentId} {AuditDate} cache item", componentId, date);
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                if (componentId == Audit.OverallId)
                {
                    return await this.ReloadOverallCacheItem(date);
                }

                var auditEntity = await this.db.GetAudit(componentId, date);
                if (auditEntity == null)
                {
                    var emptyItem = new CacheItem
                    {
                        AuditDate = date,
                        ComponentId = componentId,
                        Summary = new CountersSummary(),
                    };
                    Cache.AddOrUpdate(emptyItem.Key, key => emptyItem, (key, old) => emptyItem);

                    return emptyItem.Summary;
                }

                var summary = await this.db.GetCounterSummariesForAudit(auditEntity.Id);
                var cacheItem = new CacheItem
                {
                    AuditDate = auditEntity.Date,
                    ComponentId = auditEntity.ComponentId,
                    Summary = summary,
                };
                Cache.AddOrUpdate(cacheItem.Key, key => cacheItem, (key, old) => cacheItem);

                if (Cache.TryGetValue(CacheItem.GetKey(Audit.OverallId, date), out var overallItem))
                {
                    overallItem.ForceReload();
                }

                return summary;
            }
            finally
            {
                sw.Stop();
                Logger.Information("Reloading {ComponentId} {AuditDate} cache item took {Elapsed}", componentId, date, sw.Elapsed);
            }
        }

        private async Task<CountersSummary> ReloadOverallCacheItem(DateTime date)
        {
            var audits = await this.db.GetAudits(date);

            if (audits.Length == 0)
            {
                var emptyItem = new CacheItem
                {
                    AuditDate = date,
                    ComponentId = Audit.OverallId,
                    Summary = new CountersSummary(),
                };
                Cache.AddOrUpdate(emptyItem.Key, key => emptyItem, (key, old) => emptyItem);

                return emptyItem.Summary;
            }

            var summaries = new List<CountersSummary>(audits.Length);
            foreach (var auditEntity in audits)
            {
                var summary = await this.db.GetCounterSummariesForAudit(auditEntity.Id);
                summaries.Add(summary);
                var cacheItem = new CacheItem
                {
                    AuditDate = auditEntity.Date.Date,
                    ComponentId = auditEntity.ComponentId,
                    Summary = summary,
                };

                Cache.AddOrUpdate(cacheItem.Key, (key) => cacheItem, (key, old) => cacheItem);
            }

            // calculate overall-infrastructure summaries
            var overallSummary = new CountersSummary();
            foreach (var item in summaries)
            {
                overallSummary.Add(item);
            }

            var overallCacheItem = new CacheItem
            {
                AuditDate = date.Date,
                ComponentId = Audit.OverallId,
                Summary = overallSummary,
            };

            Cache.AddOrUpdate(overallCacheItem.Key, key => overallCacheItem, (key, old) => overallCacheItem);

            return overallSummary;
        }

        private class CacheItem
        {
            private bool needsUpdate;

            public CacheItem()
            {
                this.CachedAt = DateTime.UtcNow;
            }

            public CountersSummary Summary { get; set; }

            public string ComponentId { get; set; }

            public DateTime AuditDate { get; set; }

            public DateTime CachedAt { get; set; }

            public bool RequiresUpdate
            {
                get
                {
                    if (this.needsUpdate)
                    {
                        return true;
                    }

                    var now = DateTime.UtcNow;

                    // Update empty items after 15 minutes
                    if (this.Summary.Total == 0)
                    {
                        return (now - this.CachedAt).TotalMinutes >= 15;
                    }

                    // Update _recent_ items in cache after 1 hour
                    if ((now - this.AuditDate).TotalDays < 2)
                    {
                        return (now - this.CachedAt).TotalHours >= 1;
                    }

                    return (now - this.CachedAt).TotalDays >= 1;
                }
            }

            public string Key => GetKey(this.ComponentId, this.AuditDate);

            public static string GetKey(string componentId, DateTime date)
            {
                return $"{componentId}__{date:yyyyMMdd}";
            }

            public void ForceReload()
            {
                this.needsUpdate = true;
            }
        }
    }

    /// <summary>
    /// Interface to abstract infra-score cache for testing.
    /// </summary>
    public interface IInfrastructureScoreCache
    {
        /// <summary>
        /// Pre-calculate counters for all available scanners during last month.
        /// </summary>
        /// <returns>A task object.</returns>
        Task ReloadEntireCache();

        /// <summary>
        /// Returns counters summary for requested component and date.
        /// First method tries to look inside local cache.
        /// Only if there is no record in the case - requests it from DB.
        /// </summary>
        /// <param name="componentId">Infrastructure component identifier.</param>
        /// <param name="date">Audit date.</param>
        /// <returns>Counters Summary for requested component and date.</returns>
        Task<CountersSummary> GetCountersSummary(string componentId, DateTime date);
    }
}
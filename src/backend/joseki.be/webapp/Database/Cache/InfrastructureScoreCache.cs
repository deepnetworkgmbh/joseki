using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using Serilog;

using webapp.Database.Models;
using webapp.Models;

namespace webapp.Database.Cache
{
    /// <summary>
    /// Takes care of keeping pre-calculated counter-summaries.
    /// </summary>
    public class InfrastructureScoreCache : IInfrastructureScoreCache
    {
        private static readonly ILogger Logger = Log.ForContext<InfrastructureScoreCache>();

        private readonly IInfraScoreDbWrapper db;
        private readonly IMemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfrastructureScoreCache"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        /// <param name="cache">In-memory cache.</param>
        public InfrastructureScoreCache(IInfraScoreDbWrapper db, IMemoryCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <inheritdoc />
        public async Task ReloadEntireCache()
        {
            Logger.Information("Reloading Infrastructure Score cache");
            var sw = new Stopwatch();
            sw.Start();

            var allItems = new List<CacheItem>();
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

                    this.InsertIntoCache(cacheItem);
                    allItems.Add(cacheItem);
                }
            }

            // calculate overall-infrastructure summaries
            foreach (var grouping in allItems.GroupBy(i => i.AuditDate))
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

                this.InsertIntoCache(cacheItem);
            }

            sw.Stop();
            Logger.Information("Reloading Infrastructure Score cache took {Elapsed}", sw.Elapsed);
        }

        /// <inheritdoc />
        public async Task<CountersSummary> GetCountersSummary(string componentId, DateTime date)
        {
            var cacheKey = CacheItem.GetKey(componentId, date);
            if (this.cache.TryGetValue<CacheItem>(cacheKey, out var cachedItem))
            {
                return cachedItem.Summary;
            }
            else
            {
                return await this.ReloadCacheItem(componentId, date);
            }
        }

        private static DateTimeOffset GetExpirationTime(CacheItem item)
        {
            var now = DateTime.UtcNow;

            // Update empty items after 15 minutes
            if (item.Summary.Total == 0)
            {
                return now.AddMinutes(15);
            }

            // Update _recent_ items in cache after 1 hour
            if ((now - item.AuditDate).TotalDays < 2)
            {
                return now.AddHours(1);
            }

            return now.AddDays(1);
        }

        private void InsertIntoCache(CacheItem item)
        {
            this.cache.Set(item.Key, item, absoluteExpiration: GetExpirationTime(item));
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
                    this.InsertIntoCache(emptyItem);

                    return emptyItem.Summary;
                }

                var summary = await this.db.GetCounterSummariesForAudit(auditEntity.Id);
                var cacheItem = new CacheItem
                {
                    AuditDate = auditEntity.Date,
                    ComponentId = auditEntity.ComponentId,
                    Summary = summary,
                };

                this.InsertIntoCache(cacheItem);

                // forcing reload of the overall entry
                if (this.cache.TryGetValue(CacheItem.GetKey(Audit.OverallId, date), out var _))
                {
                    this.cache.Remove(CacheItem.GetKey(Audit.OverallId, date));
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

                this.InsertIntoCache(emptyItem);

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

                this.InsertIntoCache(cacheItem);
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

            this.InsertIntoCache(overallCacheItem);

            return overallSummary;
        }

        private class CacheItem
        {
            public CountersSummary Summary { get; set; }

            public string ComponentId { get; set; }

            public DateTime AuditDate { get; set; }

            public string Key => GetKey(this.ComponentId, this.AuditDate);

            public static string GetKey(string componentId, DateTime date)
            {
                return $"{CacheGroup.InfraScore}_{componentId}__{date:yyyyMMdd}";
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
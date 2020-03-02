using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using Serilog;

using webapp.Models;

namespace webapp.Database
{
    /// <summary>
    /// Takes care of keeping pre-calculated counter-summaries.
    /// </summary>
    public class InfrastructureScoreCache
    {
        /// <summary>
        /// Overall infrastructure id.
        /// </summary>
        public const string OverallId = "/all";

        private static readonly ConcurrentDictionary<string, CacheItem> Cache = new ConcurrentDictionary<string, CacheItem>();
        private static readonly ILogger Logger = Log.ForContext<InfrastructureScoreCache>();

        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfrastructureScoreCache"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        public InfrastructureScoreCache(JosekiDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Pre-calculate counters for all available scanners during last month.
        /// </summary>
        /// <returns>A task object.</returns>
        public async Task ReloadEntireCache()
        {
            Logger.Information("Reloading Infrastructure Score cache");
            var sw = new Stopwatch();
            sw.Start();

            var components = await this.GetAllComponentsIds();
            foreach (var component in components)
            {
                var audits = await this.GetLastMonthAudits(component);
                foreach (var auditEntity in audits)
                {
                    var summary = await this.GetCounterSummariesForAudit(auditEntity.Id);
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
                    summary.Failed += item.Summary.Failed;
                    summary.Warning += item.Summary.Warning;
                    summary.Passed += item.Summary.Passed;
                    summary.NoData += item.Summary.NoData;
                }

                var cacheItem = new CacheItem
                {
                    AuditDate = grouping.Key,
                    ComponentId = OverallId,
                    Summary = summary,
                };
                Cache.AddOrUpdate(cacheItem.Key, key => cacheItem, (key, old) => cacheItem);
            }

            sw.Stop();
            Logger.Information("Reloading Infrastructure Score cache took {Elapsed}", sw.Elapsed);
        }

        /// <summary>
        /// Returns counters summary for requested component and date.
        /// First method tries to look inside local cache.
        /// Only if there is no record in the case - requests it from DB.
        /// </summary>
        /// <param name="componentId">Infrastructure component identifier.</param>
        /// <param name="date">Audit date.</param>
        /// <returns>Counters Summary for requested component and date.</returns>
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
                if (componentId == OverallId)
                {
                    return await this.ReloadOverallCacheItem(date);
                }

                var auditEntity = await this.GetAudit(componentId, date);
                if (auditEntity == null)
                {
                    throw new Exception("There is no audit for requested date and component");
                }

                var summary = await this.GetCounterSummariesForAudit(auditEntity.Id);
                var cacheItem = new CacheItem
                {
                    AuditDate = auditEntity.Date,
                    ComponentId = auditEntity.ComponentId,
                    Summary = summary,
                };
                Cache.AddOrUpdate(cacheItem.Key, key => cacheItem, (key, old) => cacheItem);

                if (Cache.TryGetValue(CacheItem.GetKey(OverallId, date), out var overallItem))
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
            var audits = await this.GetAudits(date);

            if (audits.Length == 0)
            {
                throw new Exception("There is no audits for requested date");
            }

            var summaries = new List<CountersSummary>(audits.Length);
            foreach (var auditEntity in audits)
            {
                var summary = await this.GetCounterSummariesForAudit(auditEntity.Id);
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
                overallSummary.Failed += item.Failed;
                overallSummary.Warning += item.Warning;
                overallSummary.Passed += item.Passed;
                overallSummary.NoData += item.NoData;
            }

            var overallCacheItem = new CacheItem
            {
                AuditDate = date.Date,
                ComponentId = OverallId,
                Summary = overallSummary,
            };

            Cache.AddOrUpdate(overallCacheItem.Key, key => overallCacheItem, (key, old) => overallCacheItem);

            return overallSummary;
        }

        /// <summary>
        /// Returns all scanner-ids used during last month.
        /// </summary>
        /// <returns>Array of unique scanner identifiers.</returns>
        private async Task<string[]> GetAllComponentsIds()
        {
            var oneMonthAgo = DateTime.UtcNow.Date.AddDays(-31);
            var ids = await this.db.Set<AuditEntity>()
                .Where(i => i.Date >= oneMonthAgo)
                .Select(i => i.ComponentId)
                .Distinct()
                .ToArrayAsync();

            return ids;
        }

        /// <summary>
        /// Returns latest audit entities for each day during the last month.
        /// If there is several audits for the same day - methods returns only the last one.
        /// </summary>
        /// <param name="componentId">Scanner identifier to get audits for.</param>
        /// <returns>Array of latest audits per day.</returns>
        private async Task<AuditEntity[]> GetLastMonthAudits(string componentId)
        {
            var oneMonthAgo = DateTime.UtcNow.Date.AddDays(-31);
            var audits = await this.db.Set<AuditEntity>()
                .Where(i => i.Date >= oneMonthAgo && i.ComponentId == componentId)
                .ToArrayAsync();

            return audits
                .GroupBy(i => i.Date.Date)
                .Select(i => i.OrderByDescending(a => a.Date).First())
                .ToArray();
        }

        /// <summary>
        /// Returns latest audit entity for requested day.
        /// If there is several audits for the same day - methods returns only the last one.
        /// </summary>
        /// <param name="componentId">Scanner identifier to get audits for.</param>
        /// <param name="date">Audit date.</param>
        /// <returns>Latest audits for the day.</returns>
        private async Task<AuditEntity> GetAudit(string componentId, DateTime date)
        {
            var audit = await this.db.Set<AuditEntity>()
                .Where(i => i.Date.Date == date.Date && i.ComponentId == componentId)
                .OrderByDescending(i => i.Date)
                .FirstOrDefaultAsync();

            return audit;
        }

        /// <summary>
        /// Returns latest audits for each scanner for requested day.
        /// If there is several audits for the same day - methods returns only the latest one per scanner.
        /// </summary>
        /// <param name="date">Audits date.</param>
        /// <returns>Latest audits for each scanner for the day.</returns>
        private async Task<AuditEntity[]> GetAudits(DateTime date)
        {
            var theDay = date.Date;
            var theNextDay = theDay.AddDays(1);
            var oneDayAudits = await this.db.Set<AuditEntity>().Where(i => i.Date >= theDay && i.Date < theNextDay).ToArrayAsync();

            var audits = oneDayAudits
                .GroupBy(i => i.ComponentId)
                .Select(i => i.OrderByDescending(a => a.Date).First())
                .ToArray();

            return audits;
        }

        /// <summary>
        /// Calculate Counter Summary for requested audits.
        /// </summary>
        /// <param name="auditId">Audit identifier to calculate summaries.</param>
        /// <returns>counters-summary for requested audit-id.</returns>
        private async Task<CountersSummary> GetCounterSummariesForAudit(int auditId)
        {
            var checkResults = await this.db.Set<CheckResultEntity>()
                .Include("Check")
                .Where(i => i.AuditId == auditId)
                .Select(i => new
                {
                    i.AuditId,
                    i.ComponentId,
                    i.Check.CheckId,
                    i.Check.Severity,
                    i.Value,
                })
                .ToArrayAsync();

            var summary = new CountersSummary();
            foreach (var checkResult in checkResults)
            {
                switch (checkResult.Value)
                {
                    case CheckValue.Failed:
                        if (checkResult.Severity == joseki.db.entities.CheckSeverity.Critical || checkResult.Severity == joseki.db.entities.CheckSeverity.High)
                        {
                            summary.Failed++;
                        }
                        else
                        {
                            summary.Warning++;
                        }

                        break;
                    case CheckValue.Succeeded:
                        summary.Passed++;
                        break;
                    case CheckValue.InProgress:
                    case CheckValue.NoData:
                        summary.NoData++;
                        break;
                }
            }

            return summary;
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

                    // Update _recent_ items in cache frequently
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
}
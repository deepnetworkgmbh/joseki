﻿using System;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using Serilog;

using webapp.Configuration;
using webapp.Database.Models;

namespace webapp.Database.Cache
{
    /// <summary>
    /// Keeps track of CVE items to reduce amount of interactions with real database.
    /// The objects stores in memory identifiers of CVEs, that are already in database.
    /// It does requests only to update expired items or query not cached yet records.
    /// The cache is expected to be used in Singleton mode.
    /// </summary>
    public class CveCache
    {
        private static readonly ILogger Logger = Log.ForContext<CveCache>();

        private readonly JosekiConfiguration config;
        private readonly JosekiDbContext db;
        private readonly IMemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CveCache"/> class.
        /// </summary>
        /// <param name="config">Joseki configuration object.</param>
        /// <param name="db">Joseki database instance.</param>
        /// <param name="cache">In-memory cache.</param>
        public CveCache(ConfigurationParser config, JosekiDbContext db, IMemoryCache cache)
        {
            this.config = config.Get();
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// The method gets CVE integer identifier from in-memory list, or if it's not there does requests to the Database.
        /// The method could do one of the following:
        /// - returns integer Id from memory if it's there;
        /// - if Id is not cached yet - queries it from the Database;
        /// - if CVE is not in the Database yet - inserts it and stores the result in memory for future requests.
        /// If the cache item was not updated longer than a configured threshold - updates the record in Database.
        /// </summary>
        /// <param name="id">CVE string identifier.</param>
        /// <param name="cveFactory">The factory method to generate a new CVE object.</param>
        /// <returns>The integer identifier, which is used as Database Primary and Foreign Keys.</returns>
        public async Task<int> GetOrAddItem(string id, Func<CVE> cveFactory)
        {
            // if item is not in cache - get it from db or add a new one;
            if (!this.cache.TryGetValue<CveCacheItem>(id, out var item))
            {
                var entity = await this.db.Set<CveEntity>().AsNoTracking().FirstOrDefaultAsync(e => e.CveId == id);

                if (entity == null)
                {
                    Logger.Information("Adding new CVE item {CveId} to the database", id);
                    var addedEntity = this.db.Set<CveEntity>().Add(cveFactory().ToEntity());
                    await this.db.SaveChangesAsync();

                    entity = addedEntity.Entity;
                }

                var cacheItem = new CveCacheItem
                {
                    CveId = id,
                    Id = entity.Id,
                    UpdatedAt = entity.DateUpdated,
                };
                item = this.cache.Set(cacheItem.Key, cacheItem);
            }

            var threshold = DateTime.UtcNow.AddDays(-this.config.Cache.CveTtl);
            if (item.UpdatedAt < threshold)
            {
                Logger.Information("Updating expired CVE item {CheckId} in the database", id);
                var entity = await this.db.Set<CveEntity>().FirstOrDefaultAsync(i => i.CveId == id);
                var check = cveFactory();

                entity.PackageName = check.PackageName;
                entity.Title = check.Title;
                entity.Severity = check.Severity.ToEntity();
                entity.Description = check.Description;
                entity.References = check.References;
                entity.Remediation = check.Remediation;
                this.db.Set<CveEntity>().Update(entity);
                await this.db.SaveChangesAsync();

                item.UpdatedAt = DateTime.UtcNow;
            }

            return item.Id;
        }
    }

    internal class CveCacheItem
    {
        public int Id { get; set; }

        public string CveId { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string Key => GetKey(this.CveId);

        public static string GetKey(string cveId)
        {
            return $"{CacheGroup.Cve}_{cveId}";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using joseki.db;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using webapp.Database.Models;

namespace webapp.Database.Cache
{
    /// <summary>
    /// Keeps track of Ownership items to reduce amount of interactions with real database.
    /// </summary>
    public class OwnershipCache : IOwnershipCache
    {
        private static readonly string CacheName = "ownership_cache";

        private readonly JosekiDbContext db;
        private readonly IMemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnershipCache"/> class.
        /// </summary>
        /// <param name="db">Joseki database instance.</param>
        /// <param name="cache">In-memory cache.</param>
        public OwnershipCache(JosekiDbContext db, IMemoryCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns the Ownership entries.
        /// In this scenario it is updated daily, so the expiration is set to 1 day.
        /// If the ownership will be updated manually, this cache should be invalidated.
        /// </summary>
        public async Task<List<OwnershipEntity>> GetEntries()
        {
            if (!this.cache.TryGetValue<IEnumerable<OwnershipEntity>>(CacheName, out var entries))
            {
                var db_entries = await this.db.Set<OwnershipEntity>().AsNoTracking().ToListAsync();
                entries = this.cache.Set(CacheName, db_entries, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
            }

            return entries.ToList();
        }

        /// <summary>
        /// Updates the cache with new entries, extend a possible expiration date.
        /// </summary>
        /// <param name="entries">Ownership entries.</param>
        public void SetEntries(List<OwnershipEntity> entries)
        {
            this.cache.Set(CacheName, entries, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
        }

        /// <summary>
        /// Clears the current cache, triggering reload.
        /// </summary>
        public void Invalidate()
        {
            this.cache.Remove(CacheName);
        }
    }

    /// <summary>
    /// Simple Ownership cache
    /// Serves the recent ownership entries.
    /// </summary>
    public interface IOwnershipCache
    {
        /// <summary>
        /// Returns cached entries.
        /// If cache is empty, loads entries from database.
        /// </summary>
        Task<List<OwnershipEntity>> GetEntries();

        /// <summary>
        /// Updates the entries with set of provided entries.
        /// </summary>
        void SetEntries(List<OwnershipEntity> entries);

        /// <summary>
        /// Clears the current cache, triggering reload.
        /// </summary>
        void Invalidate();
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using webapp.Database.Cache;
using webapp.Database.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// IOwnershipCache extension to query component ownership.
    /// </summary>
    public static class OwnershipExtensions
    {
        /// <summary>
        /// Returns an owner using a componentId.
        /// Uses IOwnershipCache to speed up lookup.
        /// </summary>
        /// <param name="cache">IOwnershipCache is cached Ownership table.</param>
        /// <param name="componentId">Id string of the component.</param>
        /// <returns>String as owner (email).</returns>
        public static async Task<string> GetOwner(this IOwnershipCache cache, string componentId)
        {
            var entries = await cache.GetEntries();
            IComponentId id = ComponentId.ComponentIdFactory(componentId);

            // more detailed component Id should be selected,
            // for this reason we first query the identity on object level.
            var objectOwner = entries.FirstOrDefault(x => x.ComponentId == id.ObjectLevel);
            if (!objectOwner.Equals(default) && !string.IsNullOrEmpty(objectOwner.Owner))
            {
                return objectOwner.Owner;
            }

            // if no owner on object level returned, check the parent (group owner).
            var groupOwner = entries.FirstOrDefault(x => x.ComponentId == id.GroupLevel);
            if (!groupOwner.Equals(default) && !string.IsNullOrEmpty(groupOwner.Owner))
            {
                return groupOwner.Owner;
            }

            // if no owner on group level returned, check root owner.
            var rootOwner = entries.FirstOrDefault(x => x.ComponentId == id.RootLevel);
            if (!rootOwner.Equals(default) && !string.IsNullOrEmpty(rootOwner.Owner))
            {
                return rootOwner.Owner;
            }

            // if nothing is found, return empty owner.
            return string.Empty;
        }
    }
}
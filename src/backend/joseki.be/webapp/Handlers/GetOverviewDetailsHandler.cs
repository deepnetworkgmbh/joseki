using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using webapp.Database.Cache;
using webapp.Exceptions;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-overview-details request.
    /// </summary>
    public class GetOverviewDetailsHandler
    {
        private readonly JosekiDbContext db;
        private readonly IOwnershipCache ownershipCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOverviewDetailsHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        /// <param name="ownershipCache">Ownership cache using OwnershipEntity.</param>
        public GetOverviewDetailsHandler(JosekiDbContext db, IOwnershipCache ownershipCache)
        {
            this.db = db;
            this.ownershipCache = ownershipCache;
        }

        /// <summary>
        /// Returns CheckResult set for requested date, filtered and sorted.
        /// </summary>
        /// <param name="sortBy">The ordering parameters as a string, concated by ,.</param>
        /// <param name="filterBy">The filtering parameters as a string, concated by ,.</param>
        /// <param name="date">The date to get details for.</param>
        /// <param name="pageSize">Size of each result set.</param>
        /// <param name="pageIndex">Index of each result set.</param>
        /// <returns>list of CheckResultSet.</returns>
        public async Task<CheckResultSet> GetDetails(string sortBy, string filterBy, DateTime date, int pageSize, int pageIndex)
        {
            var checks = await this.GetChecks(date);

            var result = new CheckResultSet();
            try
            {
                checks = FilterCheckList(checks, filterBy);

                result.PageIndex = pageIndex;
                result.PageSize = pageSize;
                result.TotalResults = checks.Count;
                result.SortBy = sortBy;
                result.FilterBy = filterBy;
                result.Checks = (pageSize == 0)
                              ? SortCheckList(checks, sortBy).ToArray()
                              : SortCheckList(checks, sortBy)
                                    .Skip(pageSize * pageIndex)
                                    .Take(pageSize)
                                    .ToArray();
            }
            catch (Exception error)
            {
                result.Error = error.ToString();
            }

            return result;
        }

        /// <summary>
        /// Returns autocomplete data for selected date.
        /// </summary>
        /// <param name="filterBy">The filtering parameters as a string, concated by ,.</param>
        /// <param name="date">The date to get details for.</param>
        /// <param name="omitEmpty">Filter out the checks with no result in their segment.</param>
        /// <returns>string array.</returns>
        public async Task<Dictionary<string, CheckFilter[]>> GetAutoCompleteData(string filterBy, DateTime date, bool omitEmpty = false)
        {
            // get all scan results
            var allChecks = await this.GetChecks(date);

            // get filtered scan results
            var filteredChecks = FilterCheckList(allChecks, filterBy);

            // construct the result set using both [all/filtered] scan results.
            var results = new Dictionary<string, CheckFilter[]>();

            results.TryAdd("component", allChecks
                        .Select(x => x.Component.Name)
                        .Distinct()
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, filteredChecks.Count(c => c.Component.Name == filter)))
                        .ToArray());

            results.TryAdd("category", allChecks
                        .Select(x => x.Category)
                        .Distinct()
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, filteredChecks.Count(c => c.Category == filter)))
                        .ToArray());

            results.TryAdd("collection", allChecks.GroupBy(x => x.Collection.Type + " " + x.Collection.Name)
                        .Select(x => new
                        {
                            type = x.First().Collection.Type,
                            name = x.First().Collection.Name,
                            str = x.First().Collection.Type + ":" + x.First().Collection.Name,
                        })
                        .OrderBy(x => x.str)
                        .Select(filter =>
                            new CheckFilter(
                                filter.str,
                                filteredChecks.Count(c => c.Collection.Type == filter.type && c.Collection.Name == filter.name)))
                        .ToArray());

            results.TryAdd("control", allChecks
                        .Select(x => x.Control.Id)
                        .Distinct()
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, filteredChecks.Count(c => c.Control.Id == filter)))
                        .ToArray());

            results.TryAdd("resource", allChecks.GroupBy(x => x.Resource.Type + " " + x.Resource.Name)
                        .Select(x => new
                        {
                            type = x.First().Resource.Type,
                            name = x.First().Resource.Name,
                            str = x.First().Resource.Type + ":" + x.First().Resource.Name,
                        })
                        .OrderBy(x => x.str)
                        .Select(filter =>
                            new CheckFilter(
                                filter.str,
                                filteredChecks.Count(c => c.Resource.Type == filter.type && c.Resource.Name == filter.name)))
                        .ToArray());

            results.TryAdd("owner", allChecks
                        .Select(x => x.Resource.Owner)
                        .Distinct()
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, filteredChecks.Count(c => c.Resource.Owner == filter)))
                        .ToArray());

            results.TryAdd("result", allChecks
                        .Select(x => x.Result.ToString())
                        .Distinct()
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, filteredChecks.Count(c => c.Result.ToString() == filter)))
                        .ToArray());

            // Small optimization for reducing the search results by not returning empty checks results.
            // Used by Component Detail Scan Result filtering.
            if (omitEmpty)
            {
                foreach (var checkGroup in results.Keys.ToList())
                {
                    results[checkGroup] = results[checkGroup].Where(x => x.Count > 0).ToArray();
                }
            }

            return results;
        }

        /// <summary>
        /// Returns autocomplete data for selected field.
        /// </summary>
        /// <param name="date">The date to get details for.</param>
        /// <returns>string array.</returns>
        public async Task<List<OverviewCheck>> GetChecks(DateTime date)
        {
            var theDay = date.Date;
            var theNextDay = theDay.AddDays(1);
            var oneDayAudits = await this.db.Set<AuditEntity>()
                .Include(i => i.InfrastructureComponent)
                .AsNoTracking()
                .Where(i => i.Date >= theDay && i.Date < theNextDay)
                .ToArrayAsync();

            var audits = oneDayAudits
                .GroupBy(i => i.ComponentId)
                .Select(i => i.OrderByDescending(a => a.Date).First())
                .ToArray();

            // 4. Get all the check details
            var checkResults = await this.db.Set<CheckResultEntity>()
                .Include(i => i.Check)
                .Include(i => i.Audit)
                .AsNoTracking()
                .Where(i => audits.Contains(i.Audit))
                .Select(i => new
                {
                    component = new InfrastructureComponent(i.Audit.InfrastructureComponent.ComponentId)
                    {
                        Category = InfraScoreExtensions.GetCategory(i.Audit.InfrastructureComponent.ComponentId),
                        Name = i.Audit.InfrastructureComponent.ComponentName,
                    },
                    i.ComponentId,
                    i.Check.CheckId,
                    i.Check.Severity,
                    i.Check.Category,
                    i.Check.Description,
                    i.Value,
                    i.Message,
                })
                .ToArrayAsync();

            var checks = new List<OverviewCheck>();
            foreach (var entity in checkResults)
            {
                CheckResult checkResult;
                switch (entity.Value)
                {
                    case CheckValue.Failed:
                        if (entity.Severity == CheckSeverity.Critical || entity.Severity == CheckSeverity.High)
                        {
                            checkResult = CheckResult.Failed;
                        }
                        else
                        {
                            checkResult = CheckResult.Warning;
                        }

                        break;
                    case CheckValue.Succeeded:
                        checkResult = CheckResult.Success;
                        break;
                    default:
                        checkResult = CheckResult.NoData;
                        break;
                }

                var message = entity.Message ?? entity.Description;
                var (collection, resource, tags) = ParseCollectionAndResource(entity.ComponentId);
                resource.Owner = await this.ownershipCache.GetOwner(entity.ComponentId);

                var check = new OverviewCheck(
                    date,
                    collection,
                    resource,
                    entity.Category,
                    new CheckControl(entity.CheckId, message),
                    checkResult,
                    entity.component)
                {
                    Tags = tags,
                };
                checks.Add(check);
            }

            return checks;
        }

        private static List<OverviewCheck> FilterCheckList(List<OverviewCheck> list, string filterBy)
        {
            if (string.IsNullOrEmpty(filterBy) || filterBy == "*")
            {
                return list;
            }

            var checks = list.AsEnumerable();

            var filterArgs = filterBy.Split('&');
            foreach (var arg in filterArgs)
            {
                var args = arg.ToLower().Split("=");
                var argName = args[0];
                var argValues = args[1].Split(",");

                switch (argName)
                {
                    case "result":
                        checks = checks.Where(x => argValues.Contains(x.Result.ToString().ToLower()));
                        break;

                    case "component":
                        checks = checks.Where(x => argValues.Contains(x.Component.Name.ToLower()));
                        break;

                    case "category":
                        checks = checks.Where(x => argValues.Contains(x.Category.ToLower()));
                        break;

                    case "control":
                        checks = checks.Where(x => argValues.Contains(x.Control.Id.ToLower()));
                        break;

                    case "owner":
                        checks = checks.Where(x => argValues.Contains(x.Resource.Owner.ToLower()));
                        break;

                    case "collection":

                        var collectionPredicate = PredicateBuilder.False<OverviewCheck>();

                        foreach (var collectionFilter in argValues)
                        {
                            var collectionObject = collectionFilter.Split(":");
                            var collectionType = collectionObject[0].ToLower();
                            var collectionValue = collectionObject[1].ToLower();

                            collectionPredicate = collectionPredicate
                                                    .Or(x => x.Collection.Type.ToLower() == collectionType &&
                                                             x.Collection.Name.ToLower() == collectionValue);
                        }

                        checks = checks.Where(collectionPredicate.Compile());
                        break;

                    case "resource":

                        var resourcePredicate = PredicateBuilder.False<OverviewCheck>();

                        foreach (var resourceFilter in argValues)
                        {
                            var resourceObject = resourceFilter.Split(":");
                            var resourceType = resourceObject[0].ToLower();
                            var resourceValue = resourceObject[1].ToLower();

                            resourcePredicate = resourcePredicate
                                                  .Or(x => x.Resource.Type.ToLower() == resourceType &&
                                                           x.Resource.Name.ToLower() == resourceValue);
                        }

                        checks = checks.Where(resourcePredicate.Compile());
                        break;
                }
            }

            return checks.ToList();
        }

        /// <summary>
        /// Sorts check list according to sortBy.
        /// (currently only sorted by one column).
        /// </summary>
        /// <param name="checks">List of OverviewChecks.</param>
        /// <param name="sortBy">string containing sort information.</param>
        /// <returns>Sorted list of OverviewChecks.</returns>
        private static List<OverviewCheck> SortCheckList(List<OverviewCheck> checks, string sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return checks;
            }

            var queryableChecks = checks.AsQueryable();

            var sortArgs = sortBy.Split(',');
            foreach (var arg in sortArgs)
            {
                var ascending = arg.StartsWith("+");
                var argName = arg.Substring(1, arg.Length - 1).ToLower();

                switch (argName)
                {
                    case "category":
                        queryableChecks = SortByCategory(queryableChecks, ascending);
                        break;
                    case "control":
                        queryableChecks = SortByControl(queryableChecks, ascending);
                        break;
                    case "collection":
                        queryableChecks = SortByCollection(queryableChecks, ascending);
                        break;
                    case "component":
                        queryableChecks = SortByComponent(queryableChecks, ascending);
                        break;
                    case "resource":
                        queryableChecks = SortByResource(queryableChecks, ascending);
                        break;
                    case "owner":
                        queryableChecks = SortByOwner(queryableChecks, ascending);
                        break;
                    case "result":
                        queryableChecks = SortByResult(queryableChecks, ascending);
                        break;
                }
            }

            return queryableChecks.ToList();
        }

        private static IQueryable<OverviewCheck> SortByCategory(IEnumerable<OverviewCheck> checks, bool ascending)
        {
            return ascending ? checks.OrderBy(x => x.Category).AsQueryable()
                             : checks.OrderByDescending(x => x.Category).AsQueryable();
        }

        private static IQueryable<OverviewCheck> SortByControl(IEnumerable<OverviewCheck> checks, bool ascending)
        {
            return ascending ? checks.OrderBy(x => x.Control.Id).ThenBy(x => x.Control.Message).AsQueryable()
                             : checks.OrderByDescending(x => x.Control.Id).ThenByDescending(x => x.Control.Message).AsQueryable();
        }

        private static IQueryable<OverviewCheck> SortByComponent(IEnumerable<OverviewCheck> checks, bool ascending)
        {
            return ascending ? checks.OrderBy(x => x.Component.Name).AsQueryable()
                             : checks.OrderByDescending(x => x.Component.Name).AsQueryable();
        }

        private static IQueryable<OverviewCheck> SortByResource(IEnumerable<OverviewCheck> checks, bool ascending)
        {
            return ascending ? checks.OrderBy(x => x.Resource.Type).ThenBy(x => x.Resource.Name).AsQueryable()
                             : checks.OrderByDescending(x => x.Resource.Type).ThenByDescending(x => x.Resource.Name).AsQueryable();
        }

        private static IQueryable<OverviewCheck> SortByOwner(IEnumerable<OverviewCheck> checks, bool ascending)
        {
            return ascending ? checks.OrderBy(x => x.Resource.Owner).AsQueryable()
                             : checks.OrderByDescending(x => x.Resource.Owner).AsQueryable();
        }

        private static IQueryable<OverviewCheck> SortByResult(IEnumerable<OverviewCheck> checks, bool ascending)
        {
            return ascending ? checks.OrderBy(x => x.Result).AsQueryable()
                             : checks.OrderByDescending(x => x.Result).AsQueryable();
        }

        private static IQueryable<OverviewCheck> SortByCollection(IEnumerable<OverviewCheck> checks, bool ascending)
        {
            return ascending ? checks.OrderBy(x => x.Collection.Type).ThenBy(x => x.Collection.Name).AsQueryable()
                             : checks.OrderByDescending(x => x.Collection.Type).ThenByDescending(x => x.Collection.Name).AsQueryable();
        }

        private static (Collection collection, Resource resource, Dictionary<string, string> tags) ParseCollectionAndResource(string componentId)
        {
            const string subscriptionsToken = "/subscriptions/";
            const string resourceGroupToken = "/resource_group/";
            const string nsToken = "/ns/";
            const string imageToken = "/image/";
            const string podToken = "pod";
            const string containerToken = "container";

            var tags = new Dictionary<string, string>();

            // sample component-id:
            // - /k8s/123a4567-abcd-1234-5678-1122334455ef/ns/joseki/CronJobs/scanner-azsk/container/scanner-azsk/image/deepnetwork/joseki-scanner-azsk:0.1.0
            // - /subscriptions/123c4567-abcd-1234-5678-1122334455fe/resource_group/rg-joseki/KeyVault/kv-joseki
            if (componentId.StartsWith(subscriptionsToken))
            {
                var indexOfRg = componentId.IndexOf(resourceGroupToken, StringComparison.InvariantCultureIgnoreCase);

                if (indexOfRg == -1)
                {
                    var subscriptionId = componentId.Substring(subscriptionsToken.Length);
                    return (new Collection("subscription", subscriptionId), new Resource("subscription", subscriptionId, componentId), tags);
                }

                var rgStartsAt = indexOfRg + resourceGroupToken.Length;

                var rgName = ParseToken(componentId, rgStartsAt);
                var resourceType = ParseToken(componentId, rgStartsAt + rgName.Length + 1);
                var resourceName = ParseToken(componentId, rgStartsAt + rgName.Length + resourceType.Length + 2);

                var collection = new Collection("resource group", rgName);
                var resource = new Resource(resourceType, resourceName, componentId.Substring(0, rgStartsAt + rgName.Length + resourceType.Length + resourceName.Length + 2));
                return (collection, resource, tags);
            }
            else
            {
                var nsStartsAt = componentId.IndexOf(nsToken, StringComparison.InvariantCultureIgnoreCase) + nsToken.Length;

                var nsName = ParseToken(componentId, nsStartsAt);
                var resourceType = ParseToken(componentId, nsStartsAt + nsName.Length + 1);
                var resourceName = ParseToken(componentId, nsStartsAt + nsName.Length + resourceType.Length + 2);

                var collection = new Collection("namespace", nsName);
                var resource = new Resource(resourceType, resourceName, componentId.Substring(0, nsStartsAt + nsName.Length + resourceType.Length + resourceName.Length + 2));

                // parse image-tag
                var imageTagIndex = componentId.IndexOf(imageToken, StringComparison.InvariantCultureIgnoreCase);
                if (imageTagIndex > 0)
                {
                    var imageTag = componentId.Substring(imageTagIndex + imageToken.Length);
                    tags.Add("imageTag", imageTag);
                }

                // parse component-group: "pod spec" or "container container-name"
                var group = componentId.Substring(resource.Id.Length + 1);
                if (group == podToken)
                {
                    tags.Add("subGroup", "Pod spec");
                }
                else if (group.StartsWith(containerToken))
                {
                    var parts = group.Split('/');
                    tags.Add("subGroup", $"Container {parts[1]}");
                }

                return (collection, resource, tags);
            }
        }

        private static string ParseToken(string componentId, int startAt)
        {
            var i = startAt;
            while (i < componentId.Length && componentId[i] != '/')
            {
                i++;
            }

            return componentId.Substring(startAt, i - startAt);
        }
    }
}
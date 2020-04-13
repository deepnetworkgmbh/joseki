﻿using System;
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
    /// Prepares response to get-component-details request.
    /// </summary>0
    public class GetOverviewDetailsHandler
    {
        private readonly JosekiDbContext db;
        private readonly IInfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOverviewDetailsHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        /// <param name="cache">Score cache.</param>
        public GetOverviewDetailsHandler(JosekiDbContext db, IInfrastructureScoreCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns infrastructure history for requested component.
        /// </summary>
        /// <param name="sortBy">The ordering parameters as a string, concated by ,.</param>
        /// <param name="filterBy">The filtering parameters as a string, concated by ,.</param>
        /// <param name="date">The date to get details for.</param>
        /// <param name="pageSize">Size of each result set.</param>
        /// <param name="pageIndex">Index of each result set.</param>
        /// <returns>Component details.</returns>
        public async Task<CheckResultSet> GetDetails(string sortBy, string filterBy, DateTime date, int pageSize, int pageIndex)
        {
            var checks = await this.GetChecks(date);

            if (!string.IsNullOrEmpty(filterBy))
            {
                filterBy = Base64Decode(filterBy);
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                sortBy = Base64Decode(sortBy);
            }

            var result = new CheckResultSet();
            try
            {
                checks = FilterCheckList(checks, filterBy);

                result.PageIndex = pageIndex;
                result.PageSize = pageSize;
                result.TotalResults = checks.Count;
                result.SortBy = sortBy;
                result.FilterBy = filterBy;
                result.Checks = SortCheckList(checks, sortBy)
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
        /// <returns>string array.</returns>
        public async Task<Dictionary<string, CheckFilter[]>> GetAutoCompleteData(string filterBy, DateTime date)
        {
            // get all scan results
            var allChecks = await this.GetChecks(date);

            if (!string.IsNullOrEmpty(filterBy))
            {
                filterBy = Base64Decode(filterBy);
            }

            // get filtered scan results
            var filteredChecks = FilterCheckList(allChecks, filterBy);

            // construct the result set using both [all/filtered] scan results.
            var results = new Dictionary<string, CheckFilter[]>();

            results.TryAdd("category", allChecks.GroupBy(x => x.Category)
                        .Select(x => x.First().Category)
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, !filteredChecks.Any(c => c.Category == filter)))
                        .ToArray());

            results.TryAdd("component", allChecks.GroupBy(x => x.Component.Name)
                        .Select(x => x.First().Component.Name)
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, !filteredChecks.Any(c => c.Component.Name == filter)))
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
                                !filteredChecks.Any(c => c.Collection.Type == filter.type && c.Collection.Name == filter.name)))
                        .ToArray());

            results.TryAdd("control", allChecks.GroupBy(x => x.Control.Id)
                        .Select(x => x.First().Control.Id)
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, !filteredChecks.Any(c => c.Control.Id == filter)))
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
                                !filteredChecks.Any(c => c.Resource.Type == filter.type && c.Resource.Name == filter.name)))
                        .ToArray());

            results.TryAdd("result", allChecks.GroupBy(x => x.Result.ToString())
                        .Select(x => x.First().Result.ToString())
                        .OrderBy(x => x)
                        .Select(filter => new CheckFilter(filter, !filteredChecks.Any(c => c.Result.ToString() == filter)))
                        .ToArray());

            return results;
        }

        /// <summary>
        /// Returns autocomplete data for selected field.
        /// </summary>
        /// <param name="date">The date to get details for.</param>
        /// <returns>string array.</returns>
        public async Task<List<OverviewCheck>> GetChecks(DateTime date)
        {
            // 1. get audit which represents the "date".
            var audits = await this.db.Set<AuditEntity>()
                .Include(i => i.InfrastructureComponent)
                .AsNoTracking()
                .Where(i => i.Date.Date == date.Date)
                .OrderByDescending(i => i.Date)
                .ToArrayAsync();

            // 4. Get all the check details
            var checkResults = await this.db.Set<CheckResultEntity>()
                .AsNoTracking()
                .Include(i => i.Check)
                .Include(i => i.Audit)
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

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
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
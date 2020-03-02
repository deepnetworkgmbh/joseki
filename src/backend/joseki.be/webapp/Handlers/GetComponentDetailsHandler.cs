using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using webapp.Database;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-component-details request.
    /// </summary>0
    public class GetComponentDetailsHandler
    {
        private readonly JosekiDbContext db;
        private readonly InfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetComponentDetailsHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        /// <param name="cache">Score cache.</param>
        public GetComponentDetailsHandler(JosekiDbContext db, InfrastructureScoreCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns infrastructure history for requested component.
        /// </summary>
        /// <param name="componentId">Component identifier.</param>
        /// <param name="date">The date to get details for.</param>
        /// <returns>Component details.</returns>
        public async Task<InfrastructureComponentSummaryWithHistory> GetDetails(string componentId, DateTime date)
        {
            // 1. get audit which represents the "date".
            var audit = await this.db.Set<AuditEntity>()
                .Where(i => i.Date.Date == date.Date && i.ComponentId == componentId)
                .OrderByDescending(i => i.Date)
                .FirstOrDefaultAsync();

            // 2. prepare history for the last month
            var today = DateTime.UtcNow.Date;
            var componentHistory = new List<ScoreHistoryItem>();
            foreach (var summaryDate in Enumerable.Range(-30, 31).Select(i => today.AddDays(i)))
            {
                var historyItem = await this.cache.GetCountersSummary(componentId, summaryDate);
                componentHistory.Add(new ScoreHistoryItem(summaryDate, historyItem.Score));
            }

            // 3. when the history is ready, calculate summary for the requested date.
            var currentSummary = await this.cache.GetCountersSummary(componentId, date.Date);
            var componentDetails = new InfrastructureComponentSummaryWithHistory
            {
                Date = date,
                Component = new InfrastructureComponent(componentId)
                {
                    Category = InfraScoreExtensions.GetCategory(componentId),
                    Name = audit.ComponentName,
                },
                Current = currentSummary,
                ScoreHistory = componentHistory.ToArray(),
            };

            // 4. Get all the check details
            var checkResults = await this.db.Set<CheckResultEntity>()
                .Include("Check")
                .Where(i => i.AuditId == audit.Id)
                .Select(i => new
                {
                    i.ComponentId,
                    i.Check.CheckId,
                    i.Check.Severity,
                    i.Check.Category,
                    i.Value,
                    i.Message,
                })
                .ToArrayAsync();

            var checks = new List<Check>();
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

                var (collection, resource) = ParseCollectionAndResource(entity.ComponentId);
                checks.Add(new Check(
                    componentDetails.Component,
                    date,
                    collection,
                    resource,
                    entity.Category,
                    new CheckControl(entity.Category, entity.CheckId, entity.Message),
                    checkResult));
            }

            componentDetails.Checks = checks.ToArray();

            return componentDetails;
        }

        private static (Collection collection, Resource resource) ParseCollectionAndResource(string componentId)
        {
            const string subscriptionsToken = "/subscriptions/";
            const string resourceGroupToken = "/resource_group/";
            const string nsToken = "/ns/";

            // sample component-id:
            // - /k8s/123a4567-abcd-1234-5678-1122334455ef/ns/joseki/CronJobs/scanner-azsk/container/scanner-azsk/image/deepnetwork/joseki-scanner-azsk:0.1.0
            // - /subscriptions/123c4567-abcd-1234-5678-1122334455fe/resource_group/rg-joseki/KeyVault/kv-joseki
            if (componentId.StartsWith(subscriptionsToken))
            {
                var indexOfRg = componentId.IndexOf(resourceGroupToken, StringComparison.InvariantCultureIgnoreCase);

                if (indexOfRg == -1)
                {
                    var subscriptionId = componentId.Substring(subscriptionsToken.Length);
                    return (new Collection("subscription", subscriptionId), new Resource("subscription", subscriptionId, componentId));
                }

                var rgStartsAt = indexOfRg + resourceGroupToken.Length;

                var rgName = ParseToken(componentId, rgStartsAt);
                var resourceType = ParseToken(componentId, rgStartsAt + rgName.Length + 1);
                var resourceName = ParseToken(componentId, rgStartsAt + rgName.Length + resourceType.Length + 2);

                var collection = new Collection("resource group", rgName);
                var resource = new Resource(resourceType, resourceName, componentId.Substring(0, rgStartsAt + rgName.Length + resourceType.Length + resourceName.Length + 2));
                return (collection, resource);
            }
            else
            {
                var nsStartsAt = componentId.IndexOf(nsToken, StringComparison.InvariantCultureIgnoreCase) + nsToken.Length;

                var nsName = ParseToken(componentId, nsStartsAt);
                var resourceType = ParseToken(componentId, nsStartsAt + nsName.Length + 1);
                var resourceName = ParseToken(componentId, nsStartsAt + nsName.Length + resourceType.Length + 2);

                var collection = new Collection("namespace", nsName);
                var resource = new Resource(resourceType, resourceName, componentId.Substring(0, nsStartsAt + nsName.Length + resourceType.Length + resourceName.Length + 2));
                return (collection, resource);
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
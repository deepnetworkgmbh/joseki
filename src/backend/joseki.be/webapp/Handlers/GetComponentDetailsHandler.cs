using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetComponentDetailsHandler
    {
        private readonly JosekiDbContext db;
        private readonly IInfrastructureScoreCache cache;
        private readonly GetKnowledgebaseItemsHandler docsHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetComponentDetailsHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        /// <param name="cache">Score cache.</param>
        /// <param name="docsHandler">Knowledgebase items handler.</param>
        public GetComponentDetailsHandler(JosekiDbContext db, IInfrastructureScoreCache cache, GetKnowledgebaseItemsHandler docsHandler)
        {
            this.db = db;
            this.cache = cache;
            this.docsHandler = docsHandler;
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
                .Include(i => i.InfrastructureComponent)
                .AsNoTracking()
                .Where(i => i.Date.Date == date.Date && i.ComponentId == componentId)
                .OrderByDescending(i => i.Date)
                .FirstOrDefaultAsync();

            // 1.1. try to get InfrastructureComponent if no audit fo a requested date
            string componentName;
            if (audit == null)
            {
                var component = await this.db.Set<InfrastructureComponentEntity>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.ComponentId == componentId);

                if (component == null)
                {
                    throw new JosekiException($"Unknown componentId: {componentId}");
                }

                componentName = component.ComponentName;
            }
            else
            {
                componentName = audit.InfrastructureComponent.ComponentName;
            }

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
                    Name = componentName,
                },
                Current = currentSummary,
                ScoreHistory = componentHistory.OrderBy(i => i.RecordedAt).ToArray(),
            };

            // if no audit for a given date - return result
            if (audit == null)
            {
                componentDetails.Checks = new Check[0];
                componentDetails.CategorySummaries = new CheckCategorySummary[0];
                return componentDetails;
            }

            // 4. Get all the check details
            var checkResults = await this.db.Set<CheckResultEntity>()
                .AsNoTracking()
                .Include(i => i.Check)
                .Where(i => i.AuditId == audit.Id)
                .Select(i => new
                {
                    i.ComponentId,
                    i.Check.CheckId,
                    i.Check.Severity,
                    i.Check.Category,
                    i.Check.Description,
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

                var message = entity.Message ?? entity.Description;
                var (collection, resource, tags) = ParseCollectionAndResource(entity.ComponentId);
                var check = new Check(
                    date,
                    collection,
                    resource,
                    entity.Category,
                    new CheckControl(entity.CheckId, message),
                    checkResult)
                {
                    Tags = tags,
                };
                checks.Add(check);
            }

            componentDetails.Checks = checks.ToArray();

            // 5. enrich with category descriptions
            var scannerType = audit.InfrastructureComponent.ScannerId.Split('/').First();
            var categories = checks
                .Select(i => i.Category)
                .Distinct()
                .Select(i => new { Id = $"{scannerType}.{i.Replace(" ", string.Empty)}".ToLowerInvariant(), Category = i })
                .ToDictionary(i => i.Id, j => j.Category);
            var ids = categories.Keys.ToArray();

            var docs = await this.docsHandler.GetItemsByIds(ids);
            componentDetails.CategorySummaries = docs
                .Select(i => new CheckCategorySummary { Description = i.Content, Category = categories[i.Id] })
                .ToArray();

            return componentDetails;
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
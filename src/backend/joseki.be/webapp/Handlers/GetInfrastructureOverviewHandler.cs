using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using webapp.Database;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-overall-infrastructure request.
    /// </summary>
    public class GetInfrastructureOverviewHandler
    {
        private readonly IJosekiDatabase db;
        private readonly InfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInfrastructureOverviewHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        /// <param name="cache">Score cache.</param>
        public GetInfrastructureOverviewHandler(IJosekiDatabase db, InfrastructureScoreCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns overall infrastructure overview object.
        /// </summary>
        /// <param name="date">date to calculate overview.</param>
        /// <returns>Infrastructure overview.</returns>
        public async Task<InfrastructureOverview> GetOverview(DateTime date)
        {
            var audits = await this.db.GetAuditedComponents(date);

            // 1. Calculate Overall data first, as it reloads caches for component audits automatically
            var overallHistory = new List<ScoreHistoryItem>();
            foreach (var dateTime in audits.Select(i => i.Date.Date).Distinct())
            {
                var historyItem = await this.cache.GetCountersSummary(InfrastructureScoreCache.OverallId, dateTime);
                overallHistory.Add(new ScoreHistoryItem(dateTime, historyItem.Score));
            }

            var overall = new InfrastructureComponentSummaryWithHistory
            {
                Component = new InfrastructureComponent(InfrastructureScoreCache.OverallId)
                {
                    Category = InfrastructureCategory.Overall,
                    Name = "Overall infrastructure",
                },
                Date = date,
                ScoreHistory = overallHistory.ToArray(),
                Current = await this.cache.GetCountersSummary(InfrastructureScoreCache.OverallId, date),
            };

            // 2. Calculate each component data
            // The code expects that each date has only unique component-identifiers
            // Thus, iterating only over requested date - gives unique components
            var components = new List<InfrastructureComponentSummaryWithHistory>();
            foreach (var audit in audits.Where(i => i.Date.Date == date.Date))
            {
                // first, find all audits for the particular component and prepare history items for it
                var componentHistory = new List<ScoreHistoryItem>();
                foreach (var historyAudit in audits.Where(i => i.ComponentId == audit.ComponentId))
                {
                    var historyItem = await this.cache.GetCountersSummary(historyAudit.ComponentId, historyAudit.Date);
                    componentHistory.Add(new ScoreHistoryItem(historyAudit.Date, historyItem.Score));
                }

                // when the history is ready, calculate summary for the requested date.
                var currentSummary = await this.cache.GetCountersSummary(audit.ComponentId, audit.Date);
                var component = new InfrastructureComponentSummaryWithHistory
                {
                    Date = date,
                    Component = new InfrastructureComponent(audit.ComponentId)
                    {
                        Category = GetCategory(audit.ComponentId),
                        Name = audit.ComponentName,
                    },
                    Current = currentSummary,
                    ScoreHistory = componentHistory.ToArray(),
                };

                components.Add(component);
            }

            return new InfrastructureOverview
            {
                Overall = overall,
                Components = components.ToArray(),
            };
        }

        private static InfrastructureCategory GetCategory(string componentId)
        {
            if (componentId.StartsWith("/k8s/"))
            {
                return InfrastructureCategory.Kubernetes;
            }
            else if (componentId.StartsWith("/subscriptions/"))
            {
                return InfrastructureCategory.Subscription;
            }

            throw new NotSupportedException($"Not supported category for {componentId}");
        }
    }
}
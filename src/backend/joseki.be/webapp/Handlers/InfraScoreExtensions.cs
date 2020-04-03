using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using webapp.Database.Cache;
using webapp.Database.Models;
using webapp.Exceptions;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Common Infra-Score extensions.
    /// </summary>
    public static class InfraScoreExtensions
    {
        /// <summary>
        /// Composes an infrastructure overview object for a requested date and provided audits.
        /// </summary>
        /// <param name="cache">Extended cache object.</param>
        /// <param name="date">The date to calculate overview for.</param>
        /// <param name="audits">List of audits to include into overview.</param>
        /// <returns>Complete infrastructure-overview object.</returns>
        public static async Task<InfrastructureOverview> GetInfrastructureOverview(this IInfrastructureScoreCache cache, DateTime date, Audit[] audits)
        {
            // 0. if no audits - return stub
            if (audits.Length == 0)
            {
                return new InfrastructureOverview
                {
                    Overall = new InfrastructureComponentSummaryWithHistory
                    {
                        Component = new InfrastructureComponent(Audit.OverallId)
                        {
                            Category = InfrastructureCategory.Overall,
                            Name = Audit.OverallName,
                        },
                        Date = date,
                        ScoreHistory = new ScoreHistoryItem[0],
                        Current = new CountersSummary(),
                    },
                    Components = new InfrastructureComponentSummaryWithHistory[0],
                };
            }

            // 1. Calculate Overall data first, as it reloads caches for component audits automatically
            var overallHistory = new List<ScoreHistoryItem>();
            foreach (var dateTime in audits.Select(i => i.Date.Date).Distinct())
            {
                var historyItem = await cache.GetCountersSummary(Audit.OverallId, dateTime);
                overallHistory.Add(new ScoreHistoryItem(dateTime, historyItem.Score));
            }

            var overall = new InfrastructureComponentSummaryWithHistory
            {
                Component = new InfrastructureComponent(Audit.OverallId)
                {
                    Category = InfrastructureCategory.Overall,
                    Name = Audit.OverallName,
                },
                Date = date,
                ScoreHistory = overallHistory.OrderBy(i => i.RecordedAt).ToArray(),
                Current = await cache.GetCountersSummary(Audit.OverallId, date),
            };

            // 2. Calculate each component data
            // The code expects that each date has only unique component-identifiers
            // Thus, iterating only over requested date - gives unique components
            var today = DateTime.UtcNow.Date;
            var month = Enumerable.Range(-30, 31).Select(i => today.AddDays(i)).ToArray();
            var components = new List<InfrastructureComponentSummaryWithHistory>();
            foreach (var audit in audits.Where(i => i.Date.Date == date.Date))
            {
                // first, get counters for each date during last month
                var componentHistory = new List<ScoreHistoryItem>();
                foreach (var summaryDate in month)
                {
                    var historyItem = await cache.GetCountersSummary(audit.ComponentId, summaryDate);
                    componentHistory.Add(new ScoreHistoryItem(summaryDate, historyItem.Score));
                }

                // when the history is ready, calculate summary for the requested date.
                var currentSummary = await cache.GetCountersSummary(audit.ComponentId, audit.Date);
                var component = new InfrastructureComponentSummaryWithHistory
                {
                    Date = date,
                    Component = new InfrastructureComponent(audit.ComponentId)
                    {
                        Category = GetCategory(audit.ComponentId),
                        Name = audit.ComponentName,
                    },
                    Current = currentSummary,
                    ScoreHistory = componentHistory.OrderBy(i => i.RecordedAt).ToArray(),
                };

                components.Add(component);
            }

            return new InfrastructureOverview
            {
                Overall = overall,
                Components = components.ToArray(),
            };
        }

        /// <summary>
        /// Composes a component history array.
        /// </summary>
        /// <param name="cache">Extended cache object.</param>
        /// <param name="componentId">Component identifier.</param>
        /// <param name="componentName">Component name.</param>
        /// <returns>Complete component history object.</returns>
        public static async Task<InfrastructureComponentSummaryWithHistory[]> GetInfrastructureHistory(this IInfrastructureScoreCache cache, string componentId, string componentName)
        {
            var today = DateTimeOffset.UtcNow.Date;
            var infrastructureCategory = GetCategory(componentId);
            var components = new List<InfrastructureComponentSummaryWithHistory>();

            foreach (var date in Enumerable.Range(-30, 31).Select(i => today.AddDays(i)))
            {
                var currentSummary = await cache.GetCountersSummary(componentId, date);
                var component = new InfrastructureComponentSummaryWithHistory
                {
                    Date = date,
                    Component = new InfrastructureComponent(componentId)
                    {
                        Category = infrastructureCategory,
                        Name = componentName,
                    },
                    Current = currentSummary,
                };

                components.Add(component);
            }

            return components.ToArray();
        }

        /// <summary>
        /// Gets component category by identifier.
        /// </summary>
        /// <param name="componentId">Component unique identifier.</param>
        /// <returns>Component category.</returns>
        public static InfrastructureCategory GetCategory(string componentId)
        {
            if (componentId.StartsWith("/k8s/"))
            {
                return InfrastructureCategory.Kubernetes;
            }
            else if (componentId.StartsWith("/subscriptions/"))
            {
                return InfrastructureCategory.Subscription;
            }
            else if (componentId == Audit.OverallId)
            {
                return InfrastructureCategory.Overall;
            }

            throw new JosekiException($"Not supported category for {componentId}");
        }
    }
}
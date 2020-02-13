using System;
using System.Collections.Generic;
using webapp.Models;

namespace webapp.Controllers
{
    /// <summary>
    /// data wrapper before connecting to real results.
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// list of dates for ui tests.
        /// date behaves like a key while comparing scans.
        /// thus an overall scan and a component scan
        /// must all have the same initial scan date associated.
        /// </summary>
        public static DateTime[] Dates =
        {
            new DateTime(2020, 02, 1, 12, 0, 0),
            new DateTime(2020, 02, 2, 12, 0, 0),
            new DateTime(2020, 02, 3, 12, 0, 0),
            new DateTime(2020, 02, 4, 12, 0, 0),
            new DateTime(2020, 02, 5, 12, 0, 0),
            new DateTime(2020, 02, 6, 12, 0, 0),
            new DateTime(2020, 02, 7, 12, 0, 0),
            new DateTime(2020, 02, 8, 12, 0, 0),
            new DateTime(2020, 02, 9, 12, 0, 0),
            new DateTime(2020, 02, 10, 12, 0, 0),
            new DateTime(2020, 02, 11, 12, 0, 0),
            new DateTime(2020, 02, 12, 12, 0, 0),
        };

        /// <summary>
        /// list of inftastructure components.
        /// </summary>
        public static InfrastructureComponent[] Components =
        {
            new InfrastructureComponent() { Name = "Overall", Category = InfrastructureCategory.Overall },
            new InfrastructureComponent() { Name = "Subscription1", Category = InfrastructureCategory.Subscription },
            new InfrastructureComponent() { Name = "common-cluster", Category = InfrastructureCategory.Kubernetes },
        };

        /// <summary>
        /// list of counter summaries.
        /// </summary>
        public static Dictionary<string, CountersSummary[]> GetCounters =
            new Dictionary<string, CountersSummary[]>
        {
                {
                    Components[0].Id, new CountersSummary[]
                    {
                        new CountersSummary() { NoData = 11, Failed = 22, Warning = 30, Passed = 68 },
                        new CountersSummary() { NoData = 10, Failed = 20, Warning = 30, Passed = 58 },
                        new CountersSummary() { NoData = 9, Failed = 18, Warning = 30, Passed = 41 },
                        new CountersSummary() { NoData = 8, Failed = 16, Warning = 20, Passed = 36 },
                        new CountersSummary() { NoData = 7, Failed = 14, Warning = 20, Passed = 38 },
                        new CountersSummary() { NoData = 6, Failed = 12, Warning = 20, Passed = 59 },
                        new CountersSummary() { NoData = 5, Failed = 10, Warning = 20, Passed = 39 },
                        new CountersSummary() { NoData = 4, Failed = 8, Warning = 10, Passed = 59 },
                        new CountersSummary() { NoData = 3, Failed = 6, Warning = 10, Passed = 69 },
                        new CountersSummary() { NoData = 2, Failed = 4, Warning = 10, Passed = 70 },
                        new CountersSummary() { NoData = 1, Failed = 2, Warning = 10, Passed = 97 },
                        new CountersSummary() { NoData = 0, Failed = 0, Warning = 10, Passed = 99 },
                    }
                },
                {
                    Components[1].Id, new CountersSummary[]
                    {
                        new CountersSummary() { NoData = 0, Failed = 12, Warning = 0, Passed = 33 },
                        new CountersSummary() { NoData = 1, Failed = 10, Warning = 0, Passed = 44 },
                        new CountersSummary() { NoData = 2, Failed = 8, Warning = 0, Passed = 45 },
                        new CountersSummary() { NoData = 3, Failed = 6, Warning = 0, Passed = 36 },
                        new CountersSummary() { NoData = 4, Failed = 4, Warning = 0, Passed = 38 },
                        new CountersSummary() { NoData = 5, Failed = 2, Warning = 4, Passed = 79 },
                        new CountersSummary() { NoData = 5, Failed = 0, Warning = 4, Passed = 49 },
                        new CountersSummary() { NoData = 4, Failed = 2, Warning = 4, Passed = 39 },
                        new CountersSummary() { NoData = 3, Failed = 4, Warning = 2, Passed = 69 },
                        new CountersSummary() { NoData = 2, Failed = 4, Warning = 1, Passed = 70 },
                        new CountersSummary() { NoData = 1, Failed = 2, Warning = 1, Passed = 71 },
                        new CountersSummary() { NoData = 0, Failed = 0, Warning = 1, Passed = 82 },
                    }
                },
                {
                    Components[2].Id, new CountersSummary[]
                    {
                        new CountersSummary() { NoData = 0, Failed = 15, Warning = 5, Passed = 10 },
                        new CountersSummary() { NoData = 0, Failed = 15, Warning = 3, Passed = 11 },
                        new CountersSummary() { NoData = 0, Failed = 14, Warning = 3, Passed = 22 },
                        new CountersSummary() { NoData = 1, Failed = 13, Warning = 2, Passed = 21 },
                        new CountersSummary() { NoData = 0, Failed = 13, Warning = 2, Passed = 23 },
                        new CountersSummary() { NoData = 0, Failed = 13, Warning = 2, Passed = 10 },
                        new CountersSummary() { NoData = 1, Failed = 6, Warning = 7, Passed = 30 },
                        new CountersSummary() { NoData = 0, Failed = 4, Warning = 5, Passed = 33 },
                        new CountersSummary() { NoData = 0, Failed = 2, Warning = 4, Passed = 34 },
                        new CountersSummary() { NoData = 2, Failed = 1, Warning = 3, Passed = 66 },
                        new CountersSummary() { NoData = 2, Failed = 2, Warning = 1, Passed = 67 },
                        new CountersSummary() { NoData = 0, Failed = 0, Warning = 1, Passed = 69 },
                    }
                },
        };

        /// <summary>
        /// score history, combination of dates and scores.
        /// </summary>
        public static ScoreHistoryItem[] GetScoreHistory(InfrastructureComponent component)
        {
            var result = new List<ScoreHistoryItem>();

            for (int i = 0; i < Dates.Length; i++)
            {
                result.Add(new ScoreHistoryItem(Dates[i], GetCounters[component.Id][i].Score));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns summary for the selected component.
        /// </summary>
        public static List<InfrastructureComponentSummary> GetComponentSummary(InfrastructureComponent component = null)
            {
                // if no component provided, it's overall
                if (component == null)
                {
                    component = overallComponent;
                }

                var result = new List<InfrastructureComponentSummary>();
                for (int i = 0; i < Dates.Length; i++)
                {
                    var summary = new InfrastructureComponentSummary()
                    {
                        Date = Dates[i],
                        Component = component,
                        Current = GetCounters[component.Id][i],
                        ScoreHistory = GetScoreHistory(component),
                        ScoreTrend = Trend.GetTrend(GetScoreHistory(component)),
                    };
                    result.Add(summary);
                }

                return result;
            }

        /// <summary>
        /// Returns summary for all components except overall.
        /// </summary>
        public static List<InfrastructureComponentSummary> GetComponentSummaries()
        {
            var result = new List<InfrastructureComponentSummary>();

            // list component summaries except overall
            foreach (InfrastructureComponent component in Components)
            {
                if (component.Category == InfrastructureCategory.Overall)
                {
                    continue;
                }

                var summaryForComponent = GetComponentSummary(component);
                result.AddRange(summaryForComponent);
            }

            return result;
        }

        /// <summary>
        /// static placeholder for overall component.
        /// </summary>
        private static InfrastructureComponent overallComponent = Components[0];
    }
}
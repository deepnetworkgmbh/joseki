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
        /// list of consistent dates for ui tests.
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
        /// list of counter summaries.
        /// </summary>
        public static CountersSummary[] Counters =
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
        };

        /// <summary>
        /// score history, combination of dates and counters.
        /// </summary>
        public static ScoreHistoryItem[] OverallScoreHistory = new ScoreHistoryItem[]
        {
            new ScoreHistoryItem(Dates[0], Counters[0].Passed),
            new ScoreHistoryItem(Dates[1], Counters[1].Passed),
            new ScoreHistoryItem(Dates[2], Counters[2].Passed),
            new ScoreHistoryItem(Dates[3], Counters[3].Passed),
            new ScoreHistoryItem(Dates[4], Counters[4].Passed),
            new ScoreHistoryItem(Dates[5], Counters[5].Passed),
            new ScoreHistoryItem(Dates[6], Counters[6].Passed),
            new ScoreHistoryItem(Dates[7], Counters[7].Passed),
            new ScoreHistoryItem(Dates[8], Counters[8].Passed),
            new ScoreHistoryItem(Dates[9], Counters[9].Passed),
            new ScoreHistoryItem(Dates[10], Counters[10].Passed),
            new ScoreHistoryItem(Dates[11], Counters[11].Passed),
        };

        /// <summary>
        /// overall dummy data using counters and dates.
        /// this list is long because it's mocking data change over time.
        /// in a real time scenario, only data on related date should be returned.
        /// </summary>
        public static Dictionary<DateTime, InfrastructureComponentSummary> Overall
            = new Dictionary<DateTime, InfrastructureComponentSummary>()
            {
                {
                    Dates[0], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[0],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[1], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[1],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[2], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[2],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[3], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[3],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[4], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[4],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[5], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[5],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[6], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[6],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[7], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[7],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[8], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[8],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[9], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[9],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[10], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[10],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
                {
                    Dates[11], new InfrastructureComponentSummary()
                    {
                        Name = "Overall",
                        Category = InfrastructureCategory.Overall,
                        Current = Counters[11],
                        ScoreHistory = Data.OverallScoreHistory,
                        ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                    }
                },
            };

        /// <summary>
        /// list of components (clusters and subscriptions).
        /// this list is long because it's mocking data change over time.
        /// in a real time scenario, only data on related date should be returned.
        /// </summary>
        public static Dictionary<DateTime, List<InfrastructureComponentSummary>> Components = new Dictionary<DateTime, List<InfrastructureComponentSummary>>
            {
                {
                    Dates[0], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 90,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 14,
                                Passed = 96,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[1], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 23,
                                Passed = 80,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 90,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[2], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 23,
                                Passed = 70,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 85,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[3], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 23,
                                Passed = 70,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 60,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[4], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 23,
                                Passed = 60,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 50,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[5], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 23,
                                Passed = 40,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 66,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[6], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 23,
                                Passed = 30,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 20,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[7], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 23,
                                Passed = 44,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 20,
                                Passed = 55,
                                Warning = 30,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[8], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 15,
                                NoData = 13,
                                Passed = 44,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 10,
                                NoData = 10,
                                Passed = 55,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[9], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 5,
                                NoData = 3,
                                Passed = 44,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 5,
                                NoData = 10,
                                Passed = 35,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[10], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 5,
                                NoData = 3,
                                Passed = 34,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 5,
                                NoData = 10,
                                Passed = 15,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
                {
                    Dates[11], new List<InfrastructureComponentSummary>
                    {
                        new InfrastructureComponentSummary
                        {
                            Name = "common-cluster",
                            Category = InfrastructureCategory.Kubernetes,
                            Current = new CountersSummary
                            {
                                Failed = 5,
                                NoData = 3,
                                Passed = 24,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                        new InfrastructureComponentSummary
                        {
                            Name = "Subscription 1",
                            Category = InfrastructureCategory.Subscription,
                            Current = new CountersSummary
                            {
                                Failed = 5,
                                NoData = 10,
                                Passed = 5,
                                Warning = 10,
                            },
                            ScoreHistory = Data.OverallScoreHistory,
                            ScoreTrend = Trend.GetTrend(Data.OverallScoreHistory),
                        },
                    }
                },
            };
    }
}
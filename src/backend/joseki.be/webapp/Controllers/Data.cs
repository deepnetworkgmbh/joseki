using System;
using System.Collections.Generic;
using System.Linq;
using webapp.Models;

namespace webapp.Controllers
{
    /// <summary>
    /// data wrapper before connecting to real results.
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// kubernetes check collections and resources.
        /// </summary>
        public static Dictionary<Collection, Resource[]> K8sCollections { get; }
            = new Dictionary<Collection, Resource[]>
        {
            {
                new Collection("namespace", "default"), new Resource[]
                {
                    new Resource("pod", "default-app-pod"),
                    new Resource("pod2", "app-pod2"),
                    new Resource("deployment", "deployment-1"),
                    new Resource("service", "backend"),
                    new Resource("service", "frontend"),
                    new Resource("endpoint", "frontend"),
                    new Resource("volume", "volume1"),
                }
            },
            {
                new Collection("namespace", "test"), new Resource[]
                {
                    new Resource("pod", "test-app-pod"),
                    new Resource("deployment", "deployment-1"),
                    new Resource("pod", "test-app-pod2"),
                    new Resource("deployment", "deployment-2"),
                }
            },
            {
                new Collection("namespace", "app1"), new Resource[]
                {
                    new Resource("pod", "test-app-pod"),
                    new Resource("deployment", "deployment-1"),
                }
            },
        };

        /// <summary>
        /// kubernetes check controls.
        /// </summary>
        public static CheckControl[] K8sControls { get; } =
        {
            new CheckControl("Networking", "hostport", "Host Post is not defined."),
            new CheckControl("Networking", "nixrootweak", "Host network not configured."),
            new CheckControl("Security", "runasroot", "Should not be allowed to run as root."),
            new CheckControl("Security", "hostPIDSet", "Host PID is not configured."),
            new CheckControl("Security", "hostNetworkSet", "Host network is not configured."),
            new CheckControl("Security", "nixrootweak", "Root user has weak password."),
            new CheckControl("Security", "privesc", "Priviledge escalation should not be allowed."),
            new CheckControl("Resources", "cpuReqSet", "CPU requests must be set."),
            new CheckControl("Resources", "cpuLimitSet", "CPU limits must be set."),
            new CheckControl("Health Checks", "liveness", "Liveness probe should be configured."),
            new CheckControl("Health Checks", "readiness", "Readiness probe should be configured."),
            new CheckControl("Images", "imgcorrupt", "Container image corrupt."),
            new CheckControl("Images", "imgtoobig", "Container image too big."),
        };

        /// <summary>
        /// azure check collections and resources.
        /// </summary>
        public static Dictionary<Collection, Resource[]> AzCollections { get; }
            = new Dictionary<Collection, Resource[]>
        {
            {
                new Collection("resource group", "common-rg"), new Resource[]
                {
                    new Resource("VM", "vm1"),
                    new Resource("VM", "dev-ubuntu-18.04"),
                    new Resource("SQL", "pg12-eu"),
                    new Resource("virtual network", "vr1"),
                    new Resource("Event Hub", "eh-dev"),
                }
            },
            {
                new Collection("resource group", "test-rg"), new Resource[]
                {
                    new Resource("VM", "win-10-ui-test"),
                    new Resource("Storage", "test-storage"),
                }
            },
        };

        /// <summary>
        /// kubernetes check controls.
        /// </summary>
        public static CheckControl[] AzControls { get; } =
        {
            new CheckControl("Networking", "firewallSet", "firewall is not enabled."),
            new CheckControl("Networking", "certfault", "invalid certificate."),
            new CheckControl("Networking", "badnic", "NIC misconfigured."),
            new CheckControl("Storage", "invalidTemp", "temp directory not defined."),
            new CheckControl("Storage", "lowfreespace", "running out of disk space."),
            new CheckControl("Storage", "backupnotdefined", "A backup for this storage is not defined."),
            new CheckControl("VM", "cpulimit", "Invalid CPU limit."),
            new CheckControl("VM", "ramlimit", "Invalid RAM limit."),
            new CheckControl("VM", "virtfailed", "Virtualization failed."),
            new CheckControl("SQL", "sqluser", "Sql user password is not secure."),
            new CheckControl("SQL", "sqlpaging", "Paging file not configured."),
            new CheckControl("Event Hub", "ehports", "Event hub ports not configured."),
            new CheckControl("Event Hub", "ehshare", "Event hub share keys not defined."),
        };

        /// <summary>
        /// list of dates for ui tests.
        /// date behaves like a key while comparing scans.
        /// thus an overall scan and a component scan
        /// must all have the same initial scan date associated.
        /// </summary>
        public static DateTime[] Dates { get; } =
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
        /// mock list of Checks.
        /// </summary>
        public static List<Check> ComponentChecks()
        {
            var list = new List<Check>();

            foreach (var date in Dates)
            {
                foreach (var component in Components)
                {
                    switch (component.Category)
                    {
                        case InfrastructureCategory.Overall:
                            continue;

                        case InfrastructureCategory.Kubernetes:
                            foreach (var collection in K8sCollections)
                            {
                                foreach (var resource in collection.Value)
                                {
                                    foreach (var control in K8sControls)
                                    {
                                        list.Add(new Check(component, date, collection.Key, resource, control.Category, control, RandomSeverity()));
                                    }
                                }
                            }

                            break;

                        case InfrastructureCategory.Subscription:
                            foreach (var collection in AzCollections)
                            {
                                foreach (var resource in collection.Value)
                                {
                                    foreach (var control in AzControls.Where(x => x.Category == resource.Type))
                                    {
                                        list.Add(new Check(component, date, collection.Key, resource, control.Category, control, RandomSeverity()));
                                    }
                                }
                            }

                            break;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Mocked components.
        /// </summary>
        public static InfrastructureComponent[] Components { get; } =
        {
            new InfrastructureComponent() { Name = "Overall", Category = InfrastructureCategory.Overall },
            new InfrastructureComponent() { Name = "Subscription1", Category = InfrastructureCategory.Subscription },
            new InfrastructureComponent() { Name = "common-cluster", Category = InfrastructureCategory.Kubernetes },
        };

        /// <summary>
        /// list of counter summaries.
        /// </summary>
        public static Dictionary<string, CountersSummary[]> Counters { get; } =
            new Dictionary<string, CountersSummary[]>
        {
                {
                    Components[0].Id, new[]
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
                    Components[1].Id, new[]
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
                    Components[2].Id, new[]
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
                result.Add(new ScoreHistoryItem(Dates[i], Counters[component.Id][i].Score));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns summary for the selected component.
        /// </summary>
        public static List<InfrastructureComponentSummaryWithHistory> GetComponentSummary(InfrastructureComponent component = null)
            {
                // if no component provided, it's overall
                if (component == null)
                {
                    component = OverallComponent;
                }

                var result = new List<InfrastructureComponentSummaryWithHistory>();
                for (int i = 0; i < Dates.Length; i++)
                {
                    var summary = new InfrastructureComponentSummaryWithHistory()
                    {
                        Date = Dates[i],
                        Component = component,
                        Current = Counters[component.Id][i],
                        ScoreHistory = GetScoreHistory(component),
                        ScoreTrend = Trend.GetTrend(GetScoreHistory(component)),
                        Checks = ComponentChecks().Where(check => check.Component.Id == component.Id && check.Date == Dates[i]).ToArray(),
                    };
                    result.Add(summary);
                }

                return result;
            }

        /// <summary>
        /// Returns summary for all components except overall.
        /// </summary>
        public static List<InfrastructureComponentSummaryWithHistory> GetComponentSummaries()
        {
            var result = new List<InfrastructureComponentSummaryWithHistory>();

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
        /// return a random severity.
        /// </summary>
        /// <returns>CheckSeverity.</returns>
        public static CheckSeverity RandomSeverity()
        {
            if (new Random().Next() * 100 > 70)
            {
                return CheckSeverity.Success;
            }

            var v = Enum.GetValues(typeof(CheckSeverity));
            return (CheckSeverity)v.GetValue(new Random().Next(v.Length));
        }

        /// <summary>
        /// static placeholder for overall component.
        /// </summary>
        private static readonly InfrastructureComponent OverallComponent = Components[0];
        private static InfrastructureComponent az = Components[1];
        private static InfrastructureComponent k8s = Components[2];
    }
}
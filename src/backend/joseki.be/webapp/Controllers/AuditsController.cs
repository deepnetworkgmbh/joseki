using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using webapp.Models;

namespace webapp.Controllers
{
    /// <summary>
    /// .Audit data endpoints.
    /// </summary>
    [ApiController]
    [Route("api/audits")]
    public class AuditsController : Controller
    {
        /// <summary>
        /// Returns the overall infrastructure overview for Joseki landing page.
        /// </summary>
        /// <returns>The overall infrastructure overview.</returns>
        [HttpGet]
        [Route("overview", Name = "get-overview")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverview))]
        public Task<ObjectResult> GetOverview()
        {
            var summary = new InfrastructureOverview();
            var overallScoreHistory = new ScoreHistoryItem[]
            {
                new ScoreHistoryItem(new DateTime(2020, 02, 1, 12, 0, 1), 87),
                new ScoreHistoryItem(new DateTime(2020, 02, 2, 12, 0, 1), 76),
                new ScoreHistoryItem(new DateTime(2020, 02, 3, 12, 0, 1), 79),
                new ScoreHistoryItem(new DateTime(2020, 02, 4, 12, 0, 1), 69),
                new ScoreHistoryItem(new DateTime(2020, 02, 5, 12, 0, 1), 79),
                new ScoreHistoryItem(new DateTime(2020, 02, 6, 12, 0, 1), 59),
                new ScoreHistoryItem(new DateTime(2020, 02, 7, 12, 0, 1), 39),
                new ScoreHistoryItem(new DateTime(2020, 02, 8, 12, 0, 1), 59),
                new ScoreHistoryItem(new DateTime(2020, 02, 9, 12, 0, 1), 78),
                new ScoreHistoryItem(new DateTime(2020, 02, 10, 12, 0, 1), 86),
                new ScoreHistoryItem(new DateTime(2020, 02, 11, 12, 0, 1), 81),
                new ScoreHistoryItem(new DateTime(2020, 02, 12, 12, 0, 1), 88),
            };

            summary.Overall = new InfrastructureComponentSummary
            {
                Name = "Overall",
                Category = InfrastructureCategory.Overall,
                Current = new CountersSummary
                {
                    Failed = 10,
                    NoData = 20,
                    Passed = 90,
                    Warning = 30,
                },
                ScoreHistory = overallScoreHistory,
                ScoreTrend = Trend.GetTrend(overallScoreHistory),
            };
            summary.Components = new[]
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
                    ScoreHistory = overallScoreHistory,
                    ScoreTrend = Trend.GetTrend(overallScoreHistory),
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
                    ScoreHistory = overallScoreHistory,
                    ScoreTrend = Trend.GetTrend(overallScoreHistory),
                },
            };

            return Task.FromResult(this.StatusCode(200, summary));
        }
    }
}
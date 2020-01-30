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
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-10), 52),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-9), 87),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-8), 96),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-4), 79),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-1), 78),
                new ScoreHistoryItem(DateTime.UtcNow, 86),
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
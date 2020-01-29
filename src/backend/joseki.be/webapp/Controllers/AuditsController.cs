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
            var overallScoreHistory = new short[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 52, 70, 68, 75, 75, 75, 80, 85, 78 };

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
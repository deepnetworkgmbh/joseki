using System;
using System.Collections.Generic;
using System.Linq;
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
        private static List<InfrastructureComponentSummary> overallSummary = Data.GetComponentSummary();
        private static List<InfrastructureComponentSummary> componentSummaries = Data.GetComponentSummaries();

        /// <summary>
        /// Returns the overall infrastructure overview for Joseki landing page.
        /// </summary>
        /// <returns>The overall infrastructure overview.</returns>
        [HttpGet]
        [Route("overview", Name = "get-overview")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverview))]
        public Task<ObjectResult> GetOverview(DateTime? date = null)
        {
            var summary = new InfrastructureOverview();
            var indexDate = (date == null) ? Data.Dates.First() : date;

            summary.Overall = overallSummary.FirstOrDefault(summary => summary.Date == indexDate);
            summary.Components = componentSummaries.Where(summary => summary.Date == indexDate).ToArray();

            return Task.FromResult(this.StatusCode(200, summary));
        }

        /// <summary>
        /// Returns the overall infrastructure overview diff.
        /// </summary>
        /// <returns>The overall infrastructure overview diff.</returns>
        [HttpGet]
        [Route("overviewdiff", Name = "get-overview-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverviewDiff))]
        public Task<ObjectResult> GetOverviewDiff(DateTime? date1 = null, DateTime? date2 = null)
        {
            var diff = new InfrastructureOverviewDiff();

            if (date1 != null && date2 != null)
            {
                try
                {
                    diff.Overall1 = overallSummary.FirstOrDefault(summary => summary.Date == date1);
                    diff.Overall2 = overallSummary.FirstOrDefault(summary => summary.Date == date2);
                    diff.Components1 = componentSummaries.Where(summary => summary.Date == date1).ToArray();
                    diff.Components2 = componentSummaries.Where(summary => summary.Date == date2).ToArray();
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }

            return Task.FromResult(this.StatusCode(200, diff));
        }
    }
}
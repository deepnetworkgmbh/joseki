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
        private static List<InfrastructureComponentSummaryWithHistory> overallSummary = Data.GetComponentSummary();
        private static List<InfrastructureComponentSummaryWithHistory> componentSummaries = Data.GetComponentSummaries();

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
        [Route("overview/diff", Name = "get-overview-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverviewDiff))]
        public Task<ObjectResult> GetOverviewDiff(DateTime date1, DateTime date2)
        {
            var diff = new InfrastructureOverviewDiff();

            if (date1 != null && date2 != null)
            {
                try
                {
                    diff.Overall1 = overallSummary.FirstOrDefault(summary => summary.Date == date1) as InfrastructureComponentSummary;
                    diff.Overall2 = overallSummary.FirstOrDefault(summary => summary.Date == date2) as InfrastructureComponentSummary;
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

        /// <summary>
        /// Returns the component scan history.
        /// </summary>
        /// <returns>the component scan history.</returns>
        [HttpGet]
        [Route("component/history", Name = "get-component-history")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory[]))]
        public Task<ObjectResult> GetComponentHistory(string id)
        {
            var list = string.IsNullOrEmpty(id) ? overallSummary.ToArray()
                                                : componentSummaries.Where(x => x.Component.Id == id).ToArray();

            return Task.FromResult(this.StatusCode(200, list));
        }

        /// <summary>
        /// Returns the component summary detail.
        /// </summary>
        /// <returns>the component summary detail.</returns>
        [HttpGet]
        [Route("component/detail", Name = "get-component-detail")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory))]
        public Task<ObjectResult> GetComponentDetail(string id, DateTime? date = null)
        {
            if (date == null)
            {
                date = componentSummaries.OrderBy(x => x.Date).First().Date;
            }

            var result = componentSummaries.FirstOrDefault(x => x.Component.Id == id && x.Date == date);

            return Task.FromResult(this.StatusCode(200, result));
        }
    }
}
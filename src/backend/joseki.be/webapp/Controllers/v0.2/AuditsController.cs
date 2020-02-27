using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using webapp.Models;

namespace webapp.Controllers.v0._2
{
    /// <summary>
    /// Audit data endpoints.
    /// </summary>
    [ApiController]
    [ApiVersion("0.2")]
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
        public Task<ObjectResult> GetOverview(DateTime? date = null)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the component summary diff.
        /// </summary>
        /// <returns>the component summary diff.</returns>
        [HttpGet]
        [Route("component/diff", Name = "get-component-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentDiff))]
        public Task<ObjectResult> GetComponentDiff(string id, DateTime date1, DateTime date2)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using webapp.Handlers;
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
        private static readonly ILogger Logger = Log.ForContext<AuditsController>();
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditsController"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public AuditsController(IServiceProvider services)
        {
            this.services = services;
        }

        /// <summary>
        /// Returns the overall infrastructure overview for Joseki landing page.
        /// </summary>
        /// <returns>The overall infrastructure overview.</returns>
        [HttpGet]
        [Route("overview", Name = "get-overall-infrastructure-overview")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverview))]
        public async Task<ObjectResult> GetOverview([FromQuery]DateTime? date = null)
        {
            #region input validation

            var oneMonthAgo = DateTime.UtcNow.Date.AddDays(-31);
            if (date.HasValue)
            {
                if (date < oneMonthAgo)
                {
                    return this.BadRequest($"Requested date {date} is more than one month ago. Joseki supports only 31 days.");
                }
                else if (date >= DateTime.UtcNow.Date.AddDays(1))
                {
                    return this.BadRequest($"Requested date {date} is in future. Unfortunately, Joseki could not see future yet.");
                }
            }

            #endregion

            try
            {
                var handler = this.services.GetService<GetInfrastructureOverviewHandler>();
                if (date == null)
                {
                    date = DateTime.UtcNow;
                }

                var overview = await handler.GetOverview(date.Value);
                return this.StatusCode(200, overview);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get infrastructure overview on {AuditDate}", date);
                return this.StatusCode(500, $"Failed to get infrastructure overview");
            }
        }

        /// <summary>
        /// Returns the overall infrastructure overview diff.
        /// </summary>
        /// <returns>The overall infrastructure overview diff.</returns>
        [HttpGet]
        [Route("overview/diff", Name = "get-overall-infrastructure-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverviewDiff))]
        public async Task<ObjectResult> GetOverviewDiff([FromQuery]DateTime date1, [FromQuery]DateTime date2)
        {
            #region input validation

            var oneMonthAgo = DateTime.UtcNow.Date.AddDays(-31);
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            if (date1 < oneMonthAgo)
            {
                return this.BadRequest($"Requested date {date1} is more than one month ago. Joseki supports only 31 days.");
            }
            else if (date1 >= tomorrow)
            {
                return this.BadRequest($"Requested date {date1} is in future. Unfortunately, Joseki could not see future yet.");
            }

            if (date2 < oneMonthAgo)
            {
                return this.BadRequest($"Requested date {date2} is more than one month ago. Joseki supports only 31 days.");
            }
            else if (date2 >= tomorrow)
            {
                return this.BadRequest($"Requested date {date2} is in future. Unfortunately, Joseki could not see future yet.");
            }

            #endregion

            try
            {
                var handler = this.services.GetService<GetInfrastructureOverviewDiffHandler>();

                var overview = await handler.GetOverview(date1, date2);
                return this.StatusCode(200, overview);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get infrastructure overview diff for {AuditDate1} and {AuditDate2}", date1, date2);
                return this.StatusCode(500, $"Failed to get infrastructure overview diff");
            }
        }

        /// <summary>
        /// Returns the overall infrastructure overview diff.
        /// </summary>
        /// <param name="id">Unique component identifier.</param>
        /// <returns>The overall infrastructure overview diff.</returns>
        [HttpGet]
        [Route("component/{id}/history", Name = "get-single-component-history")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory[]))]
        public Task<ObjectResult> GetComponentHistory([FromRoute]string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the component summary detail.
        /// </summary>
        /// <returns>the component summary detail.</returns>
        [HttpGet]
        [Route("component/{id}/details", Name = "get-single-component-detail")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory))]
        public Task<ObjectResult> GetComponentDetail([FromRoute]string id, [FromQuery]DateTime? date = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the component summary diff.
        /// </summary>
        /// <returns>the component summary diff.</returns>
        [HttpGet]
        [Route("component/{id}/diff", Name = "get-single-component-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentDiff))]
        public Task<ObjectResult> GetComponentDiff([FromRoute]string id, [FromQuery]DateTime date1, [FromQuery]DateTime date2)
        {
            throw new NotImplementedException();
        }
    }
}
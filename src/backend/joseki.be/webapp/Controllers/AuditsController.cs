using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using webapp.Database.Models;
using webapp.Handlers;
using webapp.Models;

namespace webapp.Controllers
{
    /// <summary>
    /// .Audit data endpoints.
    /// </summary>
    [ApiController]
    [ApiVersion("0.1")]
    [Route("api/audits")]
    public class AuditsController : Controller
    {
        private static readonly ILogger Logger = Log.ForContext<AuditsController>();

        private static List<InfrastructureComponentSummaryWithHistory> overallSummary = Data.GetComponentSummary();
        private static List<InfrastructureComponentSummaryWithHistory> componentSummaries = Data.GetComponentSummaries();

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
        [Route("overview", Name = "get-overview")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverview))]
        public async Task<ObjectResult> GetOverview(DateTime? date = null)
        {
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
        [Route("overview/diff", Name = "get-overview-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverviewDiff))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetOverviewDiff(DateTime date1, DateTime date2)
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

                var overview = await handler.GetDiff(date1, date2);
                return this.StatusCode(200, overview);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get infrastructure overview diff for {AuditDate1} and {AuditDate2}", date1, date2);
                return this.StatusCode(500, $"Failed to get infrastructure overview diff");
            }
        }

        /// <summary>
        /// Returns the component scan history.
        /// </summary>
        /// <returns>the component scan history.</returns>
        [HttpGet]
        [Route("component/history", Name = "get-component-history")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory[]))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetComponentHistory(string id)
        {
            var componentId = string.IsNullOrEmpty(id)
                ? Audit.OverallId
                : HttpUtility.UrlDecode(id);

            try
            {
                var handler = this.services.GetService<GetInfrastructureHistoryHandler>();

                var history = await handler.GetHistory(componentId);
                return this.StatusCode(200, history);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get component {ComponentId} history", componentId);
                return this.StatusCode(500, $"Failed to get component {componentId} history");
            }
        }

        /// <summary>
        /// Returns the component summary detail.
        /// </summary>
        /// <returns>the component summary detail.</returns>
        [HttpGet]
        [Route("component/detail", Name = "get-component-detail")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetComponentDetail(string id, DateTime? date = null)
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

            var unescapedId = HttpUtility.UrlDecode(id);
            var detailsDate = date?.Date ?? DateTime.UtcNow.Date;
            try
            {
                var handler = this.services.GetService<GetComponentDetailsHandler>();

                var details = await handler.GetDetails(unescapedId, detailsDate);
                return this.StatusCode(200, details);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get component {ComponentId} details", unescapedId);
                return this.StatusCode(500, $"Failed to get component {unescapedId} details");
            }
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
            var result = new InfrastructureComponentDiff()
            {
                Summary1 = componentSummaries.FirstOrDefault(x => x.Component.Id == id && x.Date == date1),
                Summary2 = componentSummaries.FirstOrDefault(x => x.Component.Id == id && x.Date == date2),
            };

            return Task.FromResult(this.StatusCode(200, result));
        }
    }
}
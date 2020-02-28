using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

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
using System;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using webapp.Database.Models;
using webapp.Handlers;
using webapp.Models;

using ImageScanResult = webapp.Database.Models.ImageScanResult;

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
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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
                return this.StatusCode(500, "Failed to get infrastructure overview");
            }
        }

        /// <summary>
        /// Returns the overall infrastructure overview diff.
        /// </summary>
        /// <returns>The overall infrastructure overview diff.</returns>
        [HttpGet]
        [Route("overview/diff", Name = "get-overall-infrastructure-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverviewDiff))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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

                var overview = await handler.GetDiff(date1, date2);
                return this.StatusCode(200, overview);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get infrastructure overview diff for {AuditDate1} and {AuditDate2}", date1, date2);
                return this.StatusCode(500, "Failed to get infrastructure overview diff");
            }
        }

        /// <summary>
        /// Returns the overall infrastructure overview history.
        /// </summary>
        /// <returns>The overall infrastructure overview history.</returns>
        [HttpGet]
        [Route("overview/history", Name = "get-overall-infrastructure-history")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory[]))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetOverallInfrastructureHistory()
        {
            try
            {
                var handler = this.services.GetService<GetInfrastructureHistoryHandler>();

                var history = await handler.GetHistory(Audit.OverallId);
                return this.StatusCode(200, history);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get infrastructure overview history");
                return this.StatusCode(500, "Failed to get infrastructure overview history");
            }
        }

        /// <summary>
        /// Returns the single component history.
        /// </summary>
        /// <param name="id">Unique component identifier.</param>
        /// <returns>The component history.</returns>
        [HttpGet]
        [Route("component/{id}/history", Name = "get-single-component-history")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory[]))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetComponentHistory([FromRoute]string id)
        {
            var unescapedId = HttpUtility.UrlDecode(id);
            try
            {
                var handler = this.services.GetService<GetInfrastructureHistoryHandler>();

                var history = await handler.GetHistory(unescapedId);
                return this.StatusCode(200, history);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get component {ComponentId} history", unescapedId);
                return this.StatusCode(500, $"Failed to get component {unescapedId} history");
            }
        }

        /// <summary>
        /// Returns the component summary detail.
        /// </summary>
        /// <returns>the component summary detail.</returns>
        [HttpGet]
        [Route("component/{id}/details", Name = "get-single-component-detail")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentSummaryWithHistory))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetComponentDetail([FromRoute]string id, [FromQuery]DateTime? date = null)
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
        [Route("component/{id}/diff", Name = "get-single-component-diff")]
        [ProducesResponseType(200, Type = typeof(InfrastructureComponentDiff))]
        public async Task<ObjectResult> GetComponentDiff([FromRoute]string id, [FromQuery]DateTime date1, [FromQuery]DateTime date2)
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

            var unescapedId = HttpUtility.UrlDecode(id);

            try
            {
                var handler = this.services.GetService<GetComponentDetailsHandler>();

                var details1 = await handler.GetDetails(unescapedId, date1);
                var details2 = await handler.GetDetails(unescapedId, date2);

                return this.StatusCode(200, new InfrastructureComponentDiff
                {
                    Summary1 = details1,
                    Summary2 = details2,
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get component {ComponentId} diff between {AuditDate1} and {AuditDate2}", unescapedId, date1, date2);
                return this.StatusCode(500, $"Failed to get component {unescapedId} diff between {date1} and {date2}");
            }
        }

        /// <summary>
        /// Returns the Image scans history.
        /// </summary>
        /// <param name="imageTag">Full image tag.</param>
        /// <returns>The Image scans history.</returns>
        [HttpGet]
        [Route("container-image/{imageTag}/history", Name = "get-image-scan-history")]
        [ProducesResponseType(200, Type = typeof(ContainerImageScanResult[]))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetImageScanHistory([FromRoute]string imageTag)
        {
            var unescapedTag = HttpUtility.UrlDecode(imageTag);
            try
            {
                var handler = this.services.GetService<GetImageScanHandler>();

                var history = await handler.GetHistory(unescapedTag);
                return this.StatusCode(200, history);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get Image Scan {ImageTag} history", unescapedTag);
                return this.StatusCode(500, $"Failed to get Image Scan {unescapedTag} history");
            }
        }

        /// <summary>
        /// Returns the Image Scan detail.
        /// </summary>
        /// <returns>the component summary detail.</returns>
        [HttpGet]
        [Route("container-image/{imageTag}/details", Name = "get-image-scan-detail")]
        [ProducesResponseType(200, Type = typeof(ContainerImageScanResult))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetImageScanDetail([FromRoute]string imageTag, [FromQuery]DateTime? date = null)
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

            var unescapedTag = HttpUtility.UrlDecode(imageTag);
            var detailsDate = date?.Date ?? DateTime.UtcNow.Date;
            try
            {
                var handler = this.services.GetService<GetImageScanHandler>();

                var details = await handler.GetDetails(unescapedTag, detailsDate);
                return this.StatusCode(200, details);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get Image Scan {ImageTag} details", unescapedTag);
                return this.StatusCode(500, $"Failed to get Image Scan {unescapedTag} details");
            }
        }

        /// <summary>
        /// Returns the Image Scan diff.
        /// </summary>
        /// <param name="imageTag">The full image tag.</param>
        /// <param name="date1">The first date to compare.</param>
        /// <param name="date2">The second date to compare.</param>
        /// <returns>The Image Scan diff.</returns>
        [HttpGet]
        [Route("container-image/{imageTag}/diff", Name = "get-image-scan-diff")]
        [ProducesResponseType(200, Type = typeof(ContainerImageScanResultDiff))]
        public async Task<ObjectResult> GetImageScanDiff([FromRoute]string imageTag, [FromQuery]DateTime date1, [FromQuery]DateTime date2)
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

            var unescapedTag = HttpUtility.UrlDecode(imageTag);

            try
            {
                var handler = this.services.GetService<GetImageScanHandler>();

                var scan1 = await handler.GetDetails(unescapedTag, date1);
                var scan2 = await handler.GetDetails(unescapedTag, date2);

                return this.StatusCode(200, new ContainerImageScanResultDiff
                {
                    Scan1 = scan1,
                    Scan2 = scan2,
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get Image Scan {ImageTag} diff between {AuditDate1} and {AuditDate2}", unescapedTag, date1, date2);
                return this.StatusCode(500, $"Failed to get Image Scan {unescapedTag} diff between {date1} and {date2}");
            }
        }
    }
}
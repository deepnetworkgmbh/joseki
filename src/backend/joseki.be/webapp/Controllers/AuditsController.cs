using System;
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
    /// <summary>
    /// Returns the overall infrastructure overview for Joseki landing page.
    /// </summary>
    /// <returns>The overall infrastructure overview.</returns>
        [HttpGet]
        [Route("overview", Name = "get-overview")]
        [ProducesResponseType(200, Type = typeof(InfrastructureOverview))]
        public Task<ObjectResult> GetOverview(DateTime? date = null)
        {
            // a simple mechanism to lookup the overall data from Data date dictionary
            // it should use the data layer instead.
            var summary = new InfrastructureOverview();
            var indexDate = Data.Overall.Keys.First();
            if (date != null)
            {
                var findDay = Data.Overall.FirstOrDefault(x => x.Key == date);
                if (findDay.Value != null)
                {
                    indexDate = findDay.Key;
                }
            }

            summary.Overall = Data.Overall[indexDate];
            summary.Components = Data.Components[indexDate].ToArray();

            return Task.FromResult(this.StatusCode(200, summary));
        }
    }
}
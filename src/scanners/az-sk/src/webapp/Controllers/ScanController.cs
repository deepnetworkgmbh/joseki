using System;
using System.Threading.Tasks;

using core;

using Microsoft.AspNetCore.Mvc;

using Serilog;

namespace webapp.Controllers
{
    /// <summary>
    /// Triggers new az-sk scans.
    /// </summary>
    [ApiController]
    [Route("scan")]
    public class ScanController : ControllerBase
    {
        private static readonly ILogger Logger = Log.ForContext<ScanController>();

        private readonly SubscriptionScanner scanner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanController"/> class.
        /// </summary>
        public ScanController(SubscriptionScanner scanner)
        {
            this.scanner = scanner;
        }

        /// <summary>
        /// Scan the subscription with az-sk.
        /// </summary>
        /// <param name="subscription">The subscription to scan.</param>
        /// <returns>The scan result.</returns>
        [HttpPost]
        [Route("subscription/{subscription}")]
        public async Task<ObjectResult> ScanSubscription([FromRoute] string subscription)
        {
            try
            {
                var result = await this.scanner.Scan(subscription, DateTime.UtcNow);
                return this.StatusCode(201, result);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Failed to scan the subscription {AzureSubscription}", subscription);
                return this.StatusCode(500, $"Failed to scan a subscription because of exception: {ex.Message}");
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using core.core;
using core.exporters;
using core.scanners;
using Serilog;

namespace core
{
    public class SubscriptionScanner
    {
        private static readonly ILogger Logger = Log.ForContext<SubscriptionScanner>();

        private readonly IScanner scanner;
        private readonly IExporter exporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionScanner"/> class.
        /// </summary>
        /// <param name="scanner">The scanner implementation.</param>
        /// <param name="exporter">The exporter implementation.</param>
        public SubscriptionScanner(IScanner scanner, IExporter exporter)
        {
            this.scanner = scanner;
            this.exporter = exporter;
        }

        public async Task<SubscriptionScanDetails> Scan(string subscription)
        {
            try
            {
                var details = await this.scanner.Scan(subscription);
                await this.exporter.UploadAsync(details, CancellationToken.None);
                return details;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to scan {AzureSubscription}", subscription);
                throw;
            }
        }
    }
}

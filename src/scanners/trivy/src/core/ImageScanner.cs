using System;
using System.Threading;
using System.Threading.Tasks;
using core.core;
using core.exporters;
using core.scanners;
using Serilog;

namespace core
{
    public class ImageScanner
    {
        private static readonly ILogger Logger = Log.ForContext<ImageScanner>();

        private readonly IScanner scanner;
        private readonly IExporter exporter;

        public ImageScanner(IScanner scanner, IExporter exporter)
        {
            this.scanner = scanner;
            this.exporter = exporter;
        }

        public async Task<ImageScanDetails> Scan(ContainerImage image)
        {
            try
            {
                var details = await this.scanner.Scan(image);
                await this.exporter.UploadAsync(details, CancellationToken.None);
                return details;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to scan {Image}", image.FullName);
                throw;
            }
        }
    }
}

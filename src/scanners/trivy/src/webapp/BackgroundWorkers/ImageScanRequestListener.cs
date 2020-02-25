using System.Threading;
using System.Threading.Tasks;

using core;
using core.core;

using Microsoft.Extensions.Hosting;

using Serilog;

using webapp.Models;
using webapp.Queues;

namespace webapp.BackgroundWorkers
{
    /// <summary>
    /// Background task to dequeue new Image Scan Request messages.
    /// </summary>
    public class ImageScanRequestListener : IHostedService
    {
        private static readonly ILogger Logger = Log.ForContext<ImageScanRequestListener>();

        private readonly IQueueListener queueListener;
        private readonly ImageScanner scanner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageScanRequestListener"/> class.
        /// </summary>
        /// <param name="queueListener">Queue listener implementation.</param>
        /// <param name="scanner">Image Scanner instance.</param>
        public ImageScanRequestListener(IQueueListener queueListener, ImageScanner scanner)
        {
            this.queueListener = queueListener;
            this.scanner = scanner;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Starting Image Scan Request listener");

            this.queueListener.Listen(this.ProcessImageScanRequest, cancellationToken);

            Logger.Information("Image Scan Request listener was started");

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Stopping Image Scan Request listener");

            this.queueListener.Dispose();

            Logger.Information("Image Scan Request listener was stopped");

            return Task.CompletedTask;
        }

        private Task ProcessImageScanRequest(ImageScanRequestMessage msg)
        {
            return this.scanner.Scan(new ScanRequest
            {
                Image = ContainerImage.FromFullName(msg.Payload.ImageFullName),
                ScanId = msg.Payload.ImageScanId,
            });
        }
    }
}
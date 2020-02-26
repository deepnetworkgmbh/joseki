using System;
using System.Threading.Tasks;

using Azure.Storage.Queues;

using Newtonsoft.Json;

using Serilog;

using webapp.Configuration;
using webapp.Database.Models;

namespace webapp.Queues
{
    /// <summary>
    /// Azure Storage Queue implementation of Queue Service.
    /// </summary>
    public class AzureStorageQueue : IQueue
    {
        private static readonly ILogger Logger = Log.ForContext<AzureStorageQueue>();

        private readonly QueueClient imageScanQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageQueue"/> class.
        /// </summary>
        /// <param name="parser">The service configuration.</param>
        public AzureStorageQueue(ConfigurationParser parser)
        {
            var config = parser.Get();
            var connectionString = string.Format(config.AzureQueue.ConnectionString, config.AzureQueue.AccountName, config.AzureQueue.AccountKey);
            this.imageScanQueue = new QueueClient(connectionString, config.AzureQueue.ImageScanRequestsQueue);
        }

        /// <inheritdoc />
        public async Task EnqueueImageScanRequest(ImageScanResultWithCVEs imageScan)
        {
            Logger.Information("Enqueueing Image {ImageTag} Scan request", imageScan.ImageTag);

            try
            {
                var message = new ImageScanRequestMessage
                {
                    Headers = new MessageHeaders
                    {
                        CreationTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        PayloadVersion = ImageScanRequestPayload.VERSION,
                    },
                    Payload = new ImageScanRequestPayload
                    {
                        ImageFullName = imageScan.ImageTag,
                        ImageScanId = imageScan.Id,
                    },
                };

                var messageJson = JsonConvert.SerializeObject(message, Formatting.None);
                var bytes = System.Text.Encoding.UTF8.GetBytes(messageJson);
                var base64String = Convert.ToBase64String(bytes);
                var response = await this.imageScanQueue.SendMessageAsync(base64String, timeToLive: TimeSpan.FromDays(7));

                Logger.Information(
                    "Image {ImageTag} Scan request was queued. Message identifier: {QueueMessageId}",
                    imageScan.ImageTag,
                    response.Value.MessageId);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Image {ImageTag} Scan request failed to be queued", imageScan.ImageTag);
            }
        }
    }
}
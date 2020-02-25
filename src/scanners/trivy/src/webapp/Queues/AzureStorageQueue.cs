using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.Storage.RetryPolicies;

using Newtonsoft.Json;

using Serilog;

using webapp.Configuration;
using webapp.Models;

namespace webapp.Queues
{
    /// <summary>
    /// Azure Storage Queue implementation of Messaging Service.
    /// </summary>
    public class AzureStorageQueue : IQueueListener
    {
        private const int BASERETRYSTEP = 5_000;

        private static readonly ILogger Logger = Log.ForContext<AzureStorageQueue>();

        private readonly CloudQueue mainQueue;
        private readonly CloudQueue quarantineQueue;

        private readonly Random delayRandomizer = new Random(DateTime.UtcNow.Second);
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private int listenIterationsCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageQueue"/> class.
        /// </summary>
        /// <param name="cfg">The Azure Storage Queue configuration.</param>
        public AzureStorageQueue(AzQueueConfiguration cfg)
        {
            this.mainQueue = new CloudQueue(new Uri($"{cfg.BasePath}/{cfg.MainQueue}?{cfg.MainQueueSas}"));
            this.quarantineQueue = new CloudQueue(new Uri($"{cfg.BasePath}/{cfg.QuarantineQueue}?{cfg.QuarantineQueueSas}"));
        }

        /// <inheritdoc />
        public async Task Listen(Func<ImageScanRequestMessage, Task> handler, CancellationToken cancellationToken)
        {
            // Stop Listening if any of two cases happen:
            // 1. The code, that invoked this method wants to exit
            // 2. Dispose method is called
            var linkedTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.cts.Token);
            while (!linkedTokens.IsCancellationRequested)
            {
                try
                {
                    var queueMessage = await this.mainQueue.GetMessageAsync(
                        TimeSpan.FromMinutes(1),
                        new QueueRequestOptions
                        {
                            RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(5), 3),
                            MaximumExecutionTime = TimeSpan.FromMinutes(5),
                        },
                        null,
                        linkedTokens.Token);

                    // Dequeued until queue is empty
                    if (queueMessage == null)
                    {
                        await this.Delay(linkedTokens.Token);
                    }
                    else
                    {
                        var (contentIsSafe, messageContent) = await this.TryGetMessageContent(queueMessage, linkedTokens.Token);

                        if (contentIsSafe)
                        {
                            await this.TryProcessMessage(queueMessage, messageContent, handler, cancellationToken);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    Logger.Information("Azure queue listener was stopped");
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Unexpected exception during message listening");
                    await this.Delay(cancellationToken);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.cts.Cancel();
        }

        private async Task<(bool ok, string content)> TryGetMessageContent(CloudQueueMessage queueMessage, CancellationToken cancellationToken)
        {
            try
            {
                return (true, queueMessage.AsString);
            }
            catch (FormatException formatEx)
            {
                Logger.Warning(formatEx, "The message {MessageId} is in wrong format", queueMessage.Id);

                try
                {
                    // try to read message content as bytes to move it to quarantine
                    var bytes = queueMessage.AsBytes;
                    var rawString = Encoding.UTF8.GetString(bytes);
                    await this.quarantineQueue.AddMessageAsync(new CloudQueueMessage(rawString), cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to move message {MessageId} to quarantine", queueMessage.Id);
                }

                try
                {
                    Logger.Warning("Delete the message {MessageId} from the queue", queueMessage.Id);
                    await this.mainQueue.DeleteMessageAsync(queueMessage, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to delete the message {MessageId}", queueMessage.Id);
                }
            }

            return (false, null);
        }

        private async Task TryProcessMessage(
            CloudQueueMessage queueMessage,
            string messageContent,
            Func<ImageScanRequestMessage, Task> handler,
            CancellationToken cancellationToken)
        {
            if (queueMessage.DequeueCount > 3)
            {
                Logger.Warning("Moving message {MessageId} to quarantine", queueMessage.Id);

                await this.quarantineQueue.AddMessageAsync(new CloudQueueMessage(messageContent), cancellationToken);
                await this.mainQueue.DeleteMessageAsync(queueMessage, cancellationToken);
            }
            else
            {
                Logger.Information("Try to process the message {MessageId}", queueMessage.Id);

                var processed = false;
                try
                {
                    var imageScanRequest = JsonConvert.DeserializeObject<ImageScanRequestMessage>(messageContent);

                    await handler(imageScanRequest);

                    await this.mainQueue.DeleteMessageAsync(queueMessage, cancellationToken);
                    processed = true;
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to process Message {MessageId}. Attempt {RetryCount}", queueMessage.Id, queueMessage.DequeueCount);
                }

                if (!processed)
                {
                    var visibilityTimeout = TimeSpan.FromMilliseconds(BASERETRYSTEP * queueMessage.DequeueCount);
                    await this.mainQueue.UpdateMessageAsync(queueMessage, visibilityTimeout, MessageUpdateFields.Visibility, cancellationToken);
                }
            }
        }

        private async Task Delay(CancellationToken cancellationToken)
        {
            // Several instance of the service might be running at the same time.
            // Timeout here is supposed to distribute their listening activities
            var delayMs = this.delayRandomizer.Next(1000, 5000);
            await Task.Delay(delayMs, cancellationToken);

            // Log only once per 500-2500 seconds (8-42 minutes) to reduce amount of logs
            if (++this.listenIterationsCounter == 500)
            {
                Logger.Information("Image Scan Request Listener Heartbeat");
                this.listenIterationsCounter = 0;
            }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;

using webapp.Models;

namespace webapp.Queues
{
    /// <summary>
    /// An abstraction to dequeue Image Scan Request messages.
    /// </summary>
    public interface IQueueListener : IDisposable
    {
        /// <summary>
        /// Encapsulates reliable Image Scan Request queue listening:
        /// Get messages from queue, adjust visibility timeouts, delete or move to quarantine queue at the end.
        /// </summary>
        /// <param name="handler">Handler for a single Image Scan Request message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task object.</returns>
        Task Listen(Func<ImageScanRequestMessage, Task> handler, CancellationToken cancellationToken);
    }
}
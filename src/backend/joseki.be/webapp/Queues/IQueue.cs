using System.Threading.Tasks;

using webapp.Database.Models;

namespace webapp.Queues
{
    /// <summary>
    /// Represents Queue Service Service.
    /// </summary>
    public interface IQueue
    {
        /// <summary>
        /// Enqueue a single image scan request.
        /// </summary>
        /// <param name="imageScan">The image-scan details.</param>
        /// <returns>A task object.</returns>
        Task EnqueueImageScanRequest(ImageScanResultWithCVEs imageScan);
    }
}
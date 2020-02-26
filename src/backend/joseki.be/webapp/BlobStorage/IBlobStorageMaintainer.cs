using System.Threading;
using System.Threading.Tasks;

namespace webapp.BlobStorage
{
    /// <summary>
    /// Takes care of managing Archive and Garbage Bin in the Blob Storage service.
    /// </summary>
    public interface IBlobStorageMaintainer
    {
        /// <summary>
        /// Moves all processed audit blobs from scanner containers to Archive container.
        /// </summary>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>Number of records, that were moved to Archive.</returns>
        Task<int> MoveProcessedBlobsToArchive(CancellationToken cancellation);

        /// <summary>
        /// Deletes all expired blobs from Archive.
        /// </summary>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>Number of records, that were removed from Archive.</returns>
        Task<int> CleanupArchive(CancellationToken cancellation);
    }
}
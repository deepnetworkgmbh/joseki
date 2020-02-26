using System.Threading.Tasks;

namespace webapp.BlobStorage
{
    /// <summary>
    /// Takes care of managing Archive and Garbage Bin in the Blob Storage service.
    /// </summary>
    public interface IBlobStorageMaintainer
    {
        /// <summary>
        /// Moves audit blob from scanner container to Archive container.
        /// </summary>
        /// <param name="blob">Audit blob object.</param>
        /// <returns>A task object.</returns>
        Task MoveToArchive(AuditBlob blob);

        /// <summary>
        /// Moves blob from Archive to Garbage Bin.
        /// </summary>
        /// <param name="blob">Audit blob object.</param>
        /// <returns>A task object.</returns>
        Task DeleteStale(AuditBlob blob);

        /// <summary>
        /// Moves the entire scanner container to Garbage Bin.
        /// </summary>
        /// <param name="container">Scanner container object.</param>
        /// <returns>A task object.</returns>
        Task DeleteStale(ScannerContainer container);

        /// <summary>
        /// Deletes all expired blobs from Garbage Bin.
        /// </summary>
        /// <returns>A task object.</returns>
        Task CleanupGarbageBin();
    }
}
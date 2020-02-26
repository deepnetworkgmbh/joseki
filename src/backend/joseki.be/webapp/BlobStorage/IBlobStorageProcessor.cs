using System.IO;
using System.Threading.Tasks;

namespace webapp.BlobStorage
{
    /// <summary>
    /// General abstraction in front of Blob Storage service.
    /// </summary>
    public interface IBlobStorageProcessor
    {
        /// <summary>
        /// Get all root-level containers. Each single container represents a separate scanner service.
        /// </summary>
        /// <returns>Array of container objects.</returns>
        Task<ScannerContainer[]> ListAllContainers();

        /// <summary>
        /// Asynchronously downloads a file from server.
        /// </summary>
        /// <param name="relativePath">The relative path to file, starting with scanner container name.</param>
        /// <returns>A blob-content stream.</returns>
        Task<Stream> DownloadFile(string relativePath);

        /// <summary>
        /// Returns only unprocessed audit blobs.
        /// </summary>
        /// <param name="container">The scanner container name.</param>
        /// <returns>Array of blobs with unprocessed audit results.</returns>
        Task<AuditBlob[]> GetUnprocessedAudits(ScannerContainer container);

        /// <summary>
        /// Marks the blob as processed.
        /// </summary>
        /// <param name="auditBlob">Audit blob to mark.</param>
        /// <returns>A task object.</returns>
        Task MarkAsProcessed(AuditBlob auditBlob);
    }
}
using System.Threading;
using System.Threading.Tasks;

using webapp.BlobStorage;

namespace webapp.Audits.Processors
{
    /// <summary>
    /// A base interface for scanner-specific audit processors.
    /// </summary>
    public interface IAuditProcessor
    {
        /// <summary>
        /// Gets audit data from Blob Storage, normalize it, saves results to the Database.
        /// </summary>
        /// <param name="container">The container with audit results to process.</param>
        /// <param name="token">A signal to stop processing.</param>
        /// <returns>A task object, which indicates the end of the processing.</returns>
        Task Process(ScannerContainer container, CancellationToken token);
    }
}
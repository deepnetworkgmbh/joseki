using System.Threading;
using System.Threading.Tasks;

using webapp.BlobStorage;

namespace webapp.Audits.Processors
{
    /// <summary>
    /// Does nothing.
    /// </summary>
    public class AuditProcessorStub : IAuditProcessor
    {
        /// <inheritdoc />
        public Task Process(ScannerContainer container, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
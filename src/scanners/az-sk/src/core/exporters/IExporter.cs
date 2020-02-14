using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using core.core;

namespace core.exporters
{
    public interface IExporter
    {
        Task UploadAsync(SubscriptionScanDetails details, CancellationToken cancellation);

        Task UploadBulkAsync(IEnumerable<SubscriptionScanDetails> results, CancellationToken cancellation);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using core.core;

namespace core.exporters
{
    public interface IExporter
    {
        Task UploadAsync(ImageScanDetails details, CancellationToken cancellation);

        Task UploadBulkAsync(IEnumerable<ImageScanDetails> results, CancellationToken cancellation);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using core.core;

namespace core.exporters
{
    public interface IExporter
    {
        Task UploadAsync(ImageScanDetails details);

        Task UploadBulkAsync(IEnumerable<ImageScanDetails> results);
    }
}
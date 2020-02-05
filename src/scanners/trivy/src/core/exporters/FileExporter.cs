using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using core.core;
using core.helpers;

using Serilog;

namespace core.exporters
{
    public class FileExporter : IExporter
    {
        private static readonly ILogger Logger = Log.ForContext<FileExporter>();

        private readonly string folderPath;

        public FileExporter(string folderPath)
        {
            // if folder path is not provided, use default folder
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".image-scanner",
                    "exports");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            this.folderPath = folderPath;
        }

        public async Task UploadAsync(ImageScanDetails details)
        {
            await this.WriteSingleItem(details);
        }

        public async Task UploadBulkAsync(IEnumerable<ImageScanDetails> results)
        {
            await Task.WhenAll(results.Select(this.WriteSingleItem));
        }

        private async Task WriteSingleItem(ImageScanDetails result)
        {
            // write JSON directly to a file
            var img = result
                .Image
                .FullName
                .Replace('/', '_')
                .Replace(':', '_');

            var resultPath = Path.Combine(this.folderPath, $"{img}.json");
            var jsonResult = JsonSerializerWrapper.Serialize(result);
            await File.WriteAllTextAsync(resultPath, jsonResult);

            Logger.Information("{Image} scanning result was written to {FileName}", result.Image.FullName, resultPath);
        }
    }
}
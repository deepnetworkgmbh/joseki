using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
                    ".azsk-scanner",
                    "exports");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            this.folderPath = folderPath;
        }

        public async Task UploadAsync(SubscriptionScanDetails details, CancellationToken cancellation)
        {
            await this.WriteSingleItem(details, cancellation);
        }

        public async Task UploadBulkAsync(IEnumerable<SubscriptionScanDetails> results, CancellationToken cancellation)
        {
            await Task.WhenAll(results.Select(r => this.WriteSingleItem(r, cancellation)));
        }

        private async Task WriteSingleItem(SubscriptionScanDetails result, CancellationToken cancellation)
        {
            var resultFolder = Path.Combine(this.folderPath, result.Id);
            if (!Directory.Exists(resultFolder))
            {
                Directory.CreateDirectory(resultFolder);
            }

            for (var i = 0; i < result.ResultFiles.Count; i++)
            {
                var destPath = Path.Combine(resultFolder, result.ResultFiles[i].FileName);
                await using (Stream source = File.Open(result.ResultFiles[i].FullPath, FileMode.Open))
                {
                    await using Stream destination = File.Create(destPath);
                    await source.CopyToAsync(destination, cancellation);
                }

                result.ResultFiles[i].FullPath = destPath;
            }

            var metadata = Path.Combine(resultFolder, $"meta.json");
            var jsonResult = JsonSerializerWrapper.Serialize(result);
            await File.WriteAllTextAsync(metadata, jsonResult, cancellation);

            Logger.Information("{AzureSubscription} scanning result was written to {Folder}", result.Subscription, resultFolder);
        }
    }
}
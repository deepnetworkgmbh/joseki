using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using core.Configuration;
using core.core;

namespace core.scanners
{
    public class FakeAzSk : IScanner
    {
        private readonly FakeAzSkConfiguration config;

        public FakeAzSk(FakeAzSkConfiguration config)
        {
            this.config = config;
        }

        /// <inheritdoc />
        public Task<SubscriptionScanDetails> Scan(string subscription, DateTime scanDate)
        {
            // Overall "score" in test data files gradually improves from 1st to 15th of each month
            // and then gradually decreases from 16th till the end of the month
            var folderName = scanDate.Day < 16
                ? scanDate.Day
                : 31 - scanDate.Day;
            var resultsPath = Path.Combine(this.config.FakeResultsFolderPath, subscription, folderName.ToString());

            var resultsDir = new DirectoryInfo(resultsPath);
            var results = resultsDir
                .GetFileSystemInfos()
                .Select(i => new ResultFile(i.Name, i.FullName))
                .ToList();

            return Task.FromResult(new SubscriptionScanDetails
            {
                Id = Guid.NewGuid().ToString(),
                ScanResult = ScanResult.Succeeded,
                Subscription = subscription,
                Timestamp = scanDate,
                ResultFiles = results,
            });
        }
    }
}
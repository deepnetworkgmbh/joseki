using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using core.Configuration;
using core.core;
using RunProcessAsTask;
using Serilog;

namespace core.scanners
{
    public class AzSk : IScanner
    {
        private static readonly ILogger Logger = Log.ForContext<AzSk>();
        private static readonly string ScanResultsFolder;
        private readonly AzSkConfiguration scannerCfg;

        static AzSk()
        {
            // create scan results folder if not already exists
            ScanResultsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".azsk-scanner",
                "azsk-scan-results");

            if (!Directory.Exists(ScanResultsFolder))
            {
                Directory.CreateDirectory(ScanResultsFolder);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzSk"/> class.
        /// </summary>
        /// <param name="config">The scanner configuration.</param>
        public AzSk(ConfigurationParser config)
        {
            this.scannerCfg = config.GetScannerConfig();
        }

        public async Task<SubscriptionScanDetails> Scan(string subscription)
        {
            Logger.Information("{AzureSubscription} scan was started", subscription);

            var customOutputFolder = Path.Combine(ScanResultsFolder, CreateRandomFileName($"{subscription}_", 6));
            Directory.CreateDirectory(customOutputFolder);

            // commands that will be executed by powershell
            var arguments =
                $"{this.scannerCfg.AuditScriptPath} -SubscriptionId {subscription} -TenantId {this.scannerCfg.TenantId} " +
                $"-ServicePrincipalId {this.scannerCfg.ServicePrincipalId} -ServicePrincipalPassword {this.scannerCfg.ServicePrincipalPassword} " +
                $"-OutputFolder {customOutputFolder}";

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = arguments,
                };

                var processResults = await ProcessEx.RunAsync(processStartInfo);

                Logger
                    .Information(
                        "{AzureSubscription} scan was finished with exit code {ExitCode} in {ScanningTime}",
                        subscription,
                        processResults.ExitCode,
                        processResults.RunTime);

                var result = SubscriptionScanDetails.New();
                result.Subscription = subscription;
                result.ResultFiles = new List<ResultFile>();

                // Results directory looks like ./AzSKLogs/Sub_VSE BizSpark/20200211_153220_GRS/
                // Log files and json result are located in subfolder "Etc"
                var dirs = new DirectoryInfo(customOutputFolder).GetDirectories()[0].GetDirectories()[0].GetDirectories();

                foreach (var dir in dirs)
                {
                    result.ResultFiles.Add(await AggregateLogs(dir.GetFileSystemInfos("Etc/*.LOG"), dir));

                    var report = dir.GetFileSystemInfos("Etc/SecurityEvaluationData*.json").FirstOrDefault();
                    if (report != null)
                    {
                        result.ResultFiles.Add(new ResultFile(report.Name, report.FullName));
                    }
                }

                if (processResults.ExitCode != 0)
                {
                    var logs = string.Join(Environment.NewLine, processResults.StandardOutput);
                    Logger.Error("{AzureSubscription} scan failed: {FailedScanLogs}", subscription, logs);

                    result.ScanResult = ScanResult.Failed;
                }
                else
                {
                    Logger.Information("{AzureSubscription} was succeeded", subscription);

                    result.ScanResult = ScanResult.Succeeded;
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{AzureSubscription} scan failed", subscription);

                var result = SubscriptionScanDetails.New();
                result.Subscription = subscription;
                result.ScanResult = ScanResult.Failed;

                return result;
            }
        }

        private static string CreateRandomFileName(string prefix, int length)
        {
            var random = new Random();

            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            var str = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return prefix + str;
        }

        private static async Task<ResultFile> AggregateLogs(IEnumerable<FileSystemInfo> logs, DirectoryInfo dir)
        {
            var fileName = $"{dir.Name}_aggregated.LOG";
            var destinationPath = Path.Combine(dir.FullName, "Etc", fileName);
            await using var destination = File.Create(destinationPath);

            foreach (var logFile in logs)
            {
                var headerString = $"{Environment.NewLine}{Environment.NewLine}" +
                    $"************************************************{Environment.NewLine}" +
                    $"****** FROM {logFile.Name}{Environment.NewLine}" +
                    $"************************************************{Environment.NewLine}";
                var header = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(headerString));

                await destination.WriteAsync(header);
                await using Stream source = File.Open(logFile.FullName, FileMode.Open);
                await source.CopyToAsync(destination);
            }

            return new ResultFile
            {
                FileName = fileName,
                FullPath = destinationPath,
            };
        }
    }
}
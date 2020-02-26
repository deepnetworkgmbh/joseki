using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using core.core;
using Newtonsoft.Json.Linq;
using RunProcessAsTask;
using Serilog;

namespace core.scanners
{
    public class Trivy : IScanner
    {
        private static readonly ILogger Logger = Log.ForContext<Trivy>();
        private static readonly string ScanResultsFolder;

        private readonly string cachePath;
        private readonly string trivyBinaryPath;
        private readonly Dictionary<string, RegistryCredentials> registriesMap;

        static Trivy()
        {
            // create scan results folder if not already exists
            ScanResultsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".image-scanner",
                "trivy-scan-results");

            if (!Directory.Exists(ScanResultsFolder))
            {
                Directory.CreateDirectory(ScanResultsFolder);
            }
        }

        public Trivy(string cachePath, string trivyBinaryPath, RegistryCredentials[] registries)
        {
            if (string.IsNullOrEmpty(cachePath))
            {
                cachePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                            "/.image-scanner/.trivycache";
            }

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            this.cachePath = cachePath;
            this.trivyBinaryPath = trivyBinaryPath;

            this.registriesMap = new Dictionary<string, RegistryCredentials>();

            // != null verification is safety check against corrupted yaml configurations
            foreach (var r in registries.Where(i => i != null))
            {
                // TryAdd inserts new element to dictionary only if Key is a new entry
                this.registriesMap.TryAdd(r.Name, r);
            }
        }

        public async Task<ImageScanDetails> Scan(ScanRequest request)
        {
            Logger.Information("{Image} scan was started", request.Image);

            // set the scan result file name
            var scanResultFile = ScanResultsFolder + CreateRandomFileName("/result-", 6);

            // commands that will be executed by trivy
            var arguments =
                $"--skip-update --cache-dir {this.cachePath} -f json -o {scanResultFile} {request.Image.FullName}";

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = this.trivyBinaryPath,
                    Arguments = arguments,
                };

                // If the provided private Container Registry (CR) name is equal to CR of image to be scanned,
                // set private CR credentials as env vars to the process
                if (this.registriesMap.TryGetValue(request.Image.ContainerRegistry, out var registry))
                {
                    Logger.Information("Scanning {Image} from {RegistryAddress}", request.Image, registry.Address);

                    processStartInfo.EnvironmentVariables["TRIVY_AUTH_URL"] = registry.Address;
                    processStartInfo.EnvironmentVariables["TRIVY_USERNAME"] = registry.Username;
                    processStartInfo.EnvironmentVariables["TRIVY_PASSWORD"] = registry.Password;
                }
                else
                {
                    Logger.Information("Scanning {Image} from {RegistryAddress}", request.Image, "Default Docker Hub");
                }

                var processResults = await ProcessEx.RunAsync(processStartInfo);

                var scanOutput = processResults.ExitCode != 0
                    ? "[]"
                    : JArray.Parse(File.ReadAllText(@scanResultFile)).ToString();

                Logger
                    .Information("{Image} scan was finished with exit code {ExitCode} in {ScanningTime}", request.Image, processResults.ExitCode, processResults.RunTime);

                var logs = string.Join(Environment.NewLine, processResults.StandardOutput);

                var result = new ImageScanDetails
                {
                    Id = request.ScanId,
                    Timestamp = DateTime.UtcNow,
                    Image = request.Image,
                    ScannerType = ScannerType.Trivy,
                };

                var fatalError = logs
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(e => e.Contains("FATAL"));

                if (fatalError != null)
                {
                    var fatalLogText = fatalError.Split("FATAL")[1];

                    Logger.Error("{Image} scan failed: {FailedScanLogs}", request.Image, fatalLogText);

                    result.ScanResult = ScanResult.Failed;
                    result.Payload = fatalLogText;
                }
                else
                {
                    Logger.Error("{Image} was succeeded", request.Image);

                    result.ScanResult = ScanResult.Succeeded;
                    result.Payload = scanOutput;
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Image} scan failed. Error in Trivy process", request.Image);

                var result = new ImageScanDetails
                {
                    Id = request.ScanId,
                    Timestamp = DateTime.UtcNow,
                    Image = request.Image,
                    ScannerType = ScannerType.Trivy,
                    ScanResult = ScanResult.Failed,
                    Payload = ex.Message,
                };

                return result;
            }
        }

        public async Task UpdateDb()
        {
            Logger.Information("Trivy db update started");

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = this.trivyBinaryPath,
                    Arguments = $"--download-db-only --cache-dir {this.cachePath}",
                };

                var processResults = await ProcessEx.RunAsync(processStartInfo);

                Logger.Information("Trivy db update was finished with exit code {ExitCode} in {ProcessingTime}", processResults.ExitCode, processResults.RunTime);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Trivy db update failed");
                throw;
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
    }
}
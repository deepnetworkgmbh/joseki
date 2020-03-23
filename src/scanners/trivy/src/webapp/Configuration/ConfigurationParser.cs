using System;
using System.IO;

using core.core;

using Microsoft.Extensions.Configuration;

using Serilog;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeResolvers;

namespace webapp.Configuration
{
    /// <summary>
    /// Parse YAML based application configuration.
    /// </summary>
    public class ConfigurationParser
    {
        private static readonly ILogger Logger = Log.ForContext<ConfigurationParser>();
        private static readonly IDeserializer Deserializer;

        private readonly Lazy<ImageScannerConfiguration> imageScannerConfig;
        private readonly string scannerVersion;
        private readonly string trivyVersion;

        static ConfigurationParser()
        {
            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeResolver(new DynamicTypeResolver())
                .WithTagMapping("!trivy-scanner", typeof(TrivyConfiguration))
                .WithTagMapping("!file-exporter", typeof(FileExporterConfiguration))
                .WithTagMapping("!az-blob", typeof(AzBlobExporterConfiguration))
                .WithTagMapping("!az-storage-queue", typeof(AzQueueConfiguration))
                .Build();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationParser"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration object.</param>
        public ConfigurationParser(IConfiguration configuration)
        {
            const string configPathEnvVar = "IMAGE_SCANNER_CONFIG_FILE_PATH";
            const string trivyPathEnvVar = "TRIVY_BINARY_PATH";
            const string trivyVersionEnvVar = "TRIVY_VERSION";
            const string versionVarName = "SCANNER_VERSION";

            if (string.IsNullOrEmpty(configuration[trivyPathEnvVar]))
            {
                Logger.Fatal("There is no {EnvVar} environment variable", trivyPathEnvVar);
                throw new Exception(trivyPathEnvVar);
            }

            this.TrivyPath = configuration[trivyPathEnvVar];

            if (string.IsNullOrEmpty(configuration[trivyVersionEnvVar]))
            {
                Logger.Fatal("There is no {EnvVar} environment variable", trivyVersionEnvVar);
                throw new Exception(trivyVersionEnvVar);
            }

            this.trivyVersion = configuration[trivyVersionEnvVar];

            if (string.IsNullOrEmpty(configuration[versionVarName]))
            {
                Logger.Fatal("There is no {EnvVar} environment variable", versionVarName);
                throw new Exception(versionVarName);
            }

            this.scannerVersion = configuration[versionVarName];

            var configFilePath = configuration[configPathEnvVar];
            if (string.IsNullOrEmpty(configFilePath))
            {
                Logger.Fatal("There is no {EnvVar} environment variable", configPathEnvVar);
                throw new Exception(configPathEnvVar);
            }

            if (!File.Exists(configFilePath))
            {
                Logger.Fatal("Image Scanner config file does not exist at {ConfigFilePath}", configFilePath);
                throw new Exception($"Image Scanner config file does not exist at {configFilePath}");
            }

            this.imageScannerConfig = new Lazy<ImageScannerConfiguration>(() => this.Init(configFilePath));
        }

        /// <summary>
        /// Parse the string into dotnet object.
        /// </summary>
        /// <param name="input">String representation of YAML file.</param>
        /// <returns>The application configuration object.</returns>
        public static ImageScannerConfiguration Parse(string input)
        {
            return Deserializer.Deserialize<ImageScannerConfiguration>(input);
        }

        /// <summary>
        /// Returns trivy binary path.
        /// </summary>
        public string TrivyPath { get; }

        /// <summary>
        /// Parses Scanner Config on the first request and cache the result in memory.
        /// </summary>
        /// <returns>Image Scanner configuration object.</returns>
        public ImageScannerConfiguration Get()
        {
            return this.imageScannerConfig.Value;
        }

        /// <summary>
        /// Creates Scanner configuration specific for Trivy scanner with Azure Blob exporter.
        /// </summary>
        /// <returns>A new instance of <see cref="TrivyAzblobScannerConfiguration"/>.</returns>
        public TrivyAzblobScannerConfiguration GetTrivyAzConfig()
        {
            var trivyCfg = (TrivyConfiguration)this.imageScannerConfig.Value.Scanner;
            var azBlobCfg = (AzBlobExporterConfiguration)this.imageScannerConfig.Value.Exporter;
            return new TrivyAzblobScannerConfiguration
            {
                Id = trivyCfg.Id,
                Version = this.scannerVersion,
                TrivyVersion = this.trivyVersion,
                AzureBlobBaseUrl = azBlobCfg.BasePath,
                AzureBlobSasToken = azBlobCfg.Sas,
                HeartbeatPeriodicity = azBlobCfg.HeartbeatPeriodicity,
            };
        }

        // Parse the application configuration file.
        private ImageScannerConfiguration Init(string configFilePath)
        {
            var configString = File.ReadAllText(configFilePath);

            return Parse(configString);
        }
    }
}
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
            const string envVarName = "IMAGE_SCANNER_CONFIG_FILE_PATH";
            var configFilePath = configuration[envVarName];

            if (string.IsNullOrEmpty(configFilePath))
            {
                Logger.Fatal("There is no IMAGE_SCANNER_CONFIG_FILE_PATH environment variable with Image Scanner config filepath");
                throw new Exception(envVarName);
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
                Version = trivyCfg.Version,
                TrivyVersion = trivyCfg.TrivyVersion,
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
using System;
using System.IO;

using Serilog;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeResolvers;

namespace core.Configuration
{
    /// <summary>
    /// Parse YAML based application configuration.
    /// </summary>
    public class ConfigurationParser
    {
        private static readonly ILogger Logger = Log.ForContext<ConfigurationParser>();
        private static readonly IDeserializer Deserializer;

        private readonly Lazy<ScannerConfiguration> scannerConfig;

        static ConfigurationParser()
        {
            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeResolver(new DynamicTypeResolver())
                .WithTagMapping("!azsk-scanner", typeof(AzSkConfiguration))
                .WithTagMapping("!file-exporter", typeof(FileExporterConfiguration))
                .WithTagMapping("!az-blob", typeof(AzBlobExporterConfiguration))
                .Build();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationParser"/> class.
        /// </summary>
        /// <param name="configFilePath">The application configuration file path.</param>
        public ConfigurationParser(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                Logger.Fatal("Provided config filepath is empty");
                throw new Exception("Provided config filepath is empty");
            }

            if (!File.Exists(configFilePath))
            {
                Logger.Fatal("az-sk Scanner config file does not exist at {ConfigFilePath}", configFilePath);
                throw new Exception($"az-sk Scanner config file does not exist at {configFilePath}");
            }

            this.scannerConfig = new Lazy<ScannerConfiguration>(() => this.Init(configFilePath));

            const string azskVersionEnvVar = "AZSK_VERSION";
            const string versionEnvVar = "SCANNER_VERSION";

            this.AzskVersion = Environment.GetEnvironmentVariable(azskVersionEnvVar);
            if (string.IsNullOrEmpty(this.AzskVersion))
            {
                Logger.Fatal("There is no {EnvVar} environment variable", azskVersionEnvVar);
                throw new Exception(azskVersionEnvVar);
            }

            this.ScannerVersion = Environment.GetEnvironmentVariable(versionEnvVar);
            if (string.IsNullOrEmpty(this.ScannerVersion))
            {
                Logger.Fatal("There is no {EnvVar} environment variable", versionEnvVar);
                throw new Exception(versionEnvVar);
            }
        }

        public string ScannerVersion { get; }

        public string AzskVersion { get; }

        /// <summary>
        /// Parse the string into dotnet object.
        /// </summary>
        /// <param name="input">String representation of YAML file.</param>
        /// <returns>The application configuration object.</returns>
        public static ScannerConfiguration Parse(string input)
        {
            return Deserializer.Deserialize<ScannerConfiguration>(input);
        }

        /// <summary>
        /// Returns current Azure Blob Storage related configuration.
        /// </summary>
        /// <returns>Blob Storage config.</returns>
        public AzBlobExporterConfiguration GetAzBlobConfig()
        {
            return (AzBlobExporterConfiguration)this.scannerConfig.Value.Exporter;
        }

        /// <summary>
        /// Returns current az-sk scanner related configuration.
        /// </summary>
        /// <returns>az-sk scanner config.</returns>
        public AzSkConfiguration GetScannerConfig()
        {
            return (AzSkConfiguration)this.scannerConfig.Value.Scanner;
        }

        /// <summary>
        /// Parses Scanner Config on the first request and cache the result in memory.
        /// </summary>
        /// <returns>az-sk Scanner configuration object.</returns>
        public ScannerConfiguration Get()
        {
            return this.scannerConfig.Value;
        }

        // Parse the application configuration file.
        private ScannerConfiguration Init(string configFilePath)
        {
            var configString = File.ReadAllText(configFilePath);

            return Parse(configString);
        }
    }
}
using System;
using System.IO;

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

        private readonly Lazy<JosekiConfiguration> scannerConfig;

        static ConfigurationParser()
        {
            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeResolver(new DynamicTypeResolver())
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
                Logger.Fatal("Joseki Backend service config file does not exist at {ConfigFilePath}", configFilePath);
                throw new Exception($"Joseki Backend service config file does not exist at {configFilePath}");
            }

            this.scannerConfig = new Lazy<JosekiConfiguration>(() => this.Init(configFilePath));
        }

        /// <summary>
        /// Parse the string into dotnet object.
        /// </summary>
        /// <param name="input">String representation of YAML file.</param>
        /// <returns>The application configuration object.</returns>
        public static JosekiConfiguration Parse(string input)
        {
            return Deserializer.Deserialize<JosekiConfiguration>(input);
        }

        /// <summary>
        /// Parses Joseki Config on the first request and cache the result in memory.
        /// </summary>
        /// <returns>Joseki backend service configuration object.</returns>
        public JosekiConfiguration Get()
        {
            return this.scannerConfig.Value;
        }

        // Parse the application configuration file.
        private JosekiConfiguration Init(string configFilePath)
        {
            var configString = File.ReadAllText(configFilePath);

            return Parse(configString);
        }
    }
}
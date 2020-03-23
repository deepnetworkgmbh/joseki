using System.IO;

using FluentAssertions;

using NUnit.Framework;

using webapp.Configuration;

namespace tests
{
    [TestFixture]
    public class YamlConfigTests
    {
        [Test]
        public void ParseConfigWithFileExporter()
        {
            // Arrange
            var stringConfig = File.ReadAllText("./config-sample.file.yaml");

            // Act
            var config = ConfigurationParser.Parse(stringConfig);

            // Assert
            config.Scanner.Should().BeOfType<TrivyConfiguration>();
            config.Scanner.As<TrivyConfiguration>().Id.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<TrivyConfiguration>().CachePath.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<TrivyConfiguration>().Registries.Should().HaveCount(2);
            config.Exporter.Should().BeOfType<FileExporterConfiguration>();
            config.Queue.Should().BeNull();
        }

        [Test]
        public void ParseConfigWithAzureDependencies()
        {
            // Arrange
            var stringConfig = File.ReadAllText("./config-sample.az.yaml");

            // Act
            var config = ConfigurationParser.Parse(stringConfig);

            // Assert
            config.Scanner.Should().BeOfType<TrivyConfiguration>();
            config.Scanner.As<TrivyConfiguration>().Id.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<TrivyConfiguration>().CachePath.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<TrivyConfiguration>().Registries.Should().HaveCount(2);
            config.Exporter.Should().BeOfType<AzBlobExporterConfiguration>();
            config.Exporter.As<AzBlobExporterConfiguration>().BasePath.Should().NotBeNullOrWhiteSpace();
            config.Exporter.As<AzBlobExporterConfiguration>().Sas.Should().NotBeNullOrWhiteSpace();
            config.Exporter.As<AzBlobExporterConfiguration>().HeartbeatPeriodicity.Should().BeGreaterThan(0);
            config.Queue.Should().BeOfType<AzQueueConfiguration>();
            config.Queue.As<AzQueueConfiguration>().BasePath.Should().NotBeNullOrWhiteSpace();
            config.Queue.As<AzQueueConfiguration>().MainQueue.Should().NotBeNullOrWhiteSpace();
            config.Queue.As<AzQueueConfiguration>().MainQueueSas.Should().NotBeNullOrWhiteSpace();
            config.Queue.As<AzQueueConfiguration>().QuarantineQueue.Should().NotBeNullOrWhiteSpace();
            config.Queue.As<AzQueueConfiguration>().QuarantineQueueSas.Should().NotBeNullOrWhiteSpace();
        }
    }
}

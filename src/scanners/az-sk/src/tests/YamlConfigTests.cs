using System.IO;

using core.Configuration;

using FluentAssertions;

using NUnit.Framework;

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
            config.Scanner.Should().BeOfType<AzSkConfiguration>();
            config.Scanner.As<AzSkConfiguration>().Id.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().AuditScriptPath.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().TenantId.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().ServicePrincipalId.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().ServicePrincipalPassword.Should().NotBeNullOrWhiteSpace();
            config.Exporter.Should().BeOfType<FileExporterConfiguration>();
        }

        [Test]
        public void ParseConfigWithAzureDependencies()
        {
            // Arrange
            var stringConfig = File.ReadAllText("./config-sample.az.yaml");

            // Act
            var config = ConfigurationParser.Parse(stringConfig);

            // Assert
            config.Scanner.Should().BeOfType<AzSkConfiguration>();
            config.Scanner.As<AzSkConfiguration>().Id.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().AuditScriptPath.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().TenantId.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().ServicePrincipalId.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<AzSkConfiguration>().ServicePrincipalPassword.Should().NotBeNullOrWhiteSpace();
            config.Exporter.Should().BeOfType<AzBlobExporterConfiguration>();
            config.Exporter.As<AzBlobExporterConfiguration>().BasePath.Should().NotBeNullOrWhiteSpace();
            config.Exporter.As<AzBlobExporterConfiguration>().Sas.Should().NotBeNullOrWhiteSpace();
            config.Exporter.As<AzBlobExporterConfiguration>().HeartbeatPeriodicity.Should().BeGreaterThan(0);
        }
    }
}

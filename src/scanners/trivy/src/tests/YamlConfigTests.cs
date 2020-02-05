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
        public void HappyPath()
        {
            // Arrange
            var stringConfig = File.ReadAllText("./image-scanner.config-sample.yaml");

            // Act
            var config = ConfigurationParser.Parse(stringConfig);

            // Assert
            config.Exporter.Should().BeOfType<FileExporterConfiguration>();
            config.Scanner.Should().BeOfType<TrivyConfiguration>();
            config.Scanner.As<TrivyConfiguration>().BinaryPath.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<TrivyConfiguration>().CachePath.Should().NotBeNullOrWhiteSpace();
            config.Scanner.As<TrivyConfiguration>().Registries.Should().HaveCount(2);
        }
    }
}

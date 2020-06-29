using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using webapp.Configuration;

namespace tests.configuration
{
    [TestClass]
    public class JosekiConfigurationTests
    {
        [TestMethod]
        public void CheckAzureADConfiguration()
        {
            var parser = new ConfigurationParser("config.sample.yaml");
            var configuration = parser.Get();

            configuration.AzureAD.Instance.Should().Be("https://login.microsoftonline.com/");
            configuration.AzureAD.Domain.Should().Be("yourdomain.onmicrosoft.com");
            configuration.AzureAD.TenantId.Should().Be("00000000-0000-0000-0000-000000000000");
            configuration.AzureAD.ClientId.Should().Be("00000000-0000-0000-0000-000000000000");
            configuration.AzureAD.ClientSecret.Should().Be("client-secret-here");
        }
    }
}
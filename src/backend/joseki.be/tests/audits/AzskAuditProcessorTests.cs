using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using webapp.Audits;
using webapp.Audits.PostProcessors;
using webapp.Audits.Processors.azsk;
using webapp.BlobStorage;
using webapp.Configuration;
using webapp.Database;
using webapp.Database.Cache;
using webapp.Database.Models;

namespace tests.audits
{
    [TestClass]
    public class AzskAuditProcessorTests
    {
        private const int ExpectedCheckResults = 12;

        [TestMethod]
        public async Task ProcessAuditHappyPath()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");

            var checksCache = new ChecksCache(parser, context, new MemoryCache(new MemoryCacheOptions()));

            var blobsMock = new Mock<IBlobStorageProcessor>(MockBehavior.Strict);
            var dbMock = new Mock<IJosekiDatabase>();

            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));
            var postProcessor = new Mock<ExtractOwnershipProcessor>(context, ownershipCache);
            var processor = new AzskAuditProcessor(blobsMock.Object, dbMock.Object, checksCache, postProcessor.Object);

            var container = new ScannerContainer(Path.Combine("audits", "samples", "azsk"))
            {
                Metadata = new ScannerMetadata
                {
                    Type = ScannerType.Azsk,
                    Id = Guid.NewGuid().ToString(),
                },
            };
            var audit = new AuditBlob { Name = "meta.json", ParentContainer = container };
            blobsMock
                .Setup(i => i.GetUnprocessedAudits(container))
                .ReturnsAsync(new[] { audit })
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/{audit.Name}"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/{audit.Name}"))
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/resources.json"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/resources.json"))
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/subscription.json"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/subscription.json"))
                .Verifiable();
            blobsMock
                .Setup(i => i.MarkAsProcessed(audit))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act & Assert
            await processor.Process(container, CancellationToken.None);
            blobsMock.Verify();
            dbMock.Verify(i => i.SaveAuditResult(It.Is<Audit>(a => VerifyHappyPathAudit(a, container))));
        }

        private static bool VerifyHappyPathAudit(Audit audit, ScannerContainer container)
        {
            const string auditId = "ec783b2d-1da0-4a30-8e52-b72dd3174eae";
            var auditDate = DateTimeOffset.FromUnixTimeSeconds(1583892106).DateTime;

            audit.Id.Should().Be(auditId);
            audit.Date.Should().BeCloseTo(auditDate, TimeSpan.FromMinutes(1));
            audit.ScannerId.Should().Be($"{container.Metadata.Type}/{container.Metadata.Id}");
            audit.ComponentId.Should().Be("/subscriptions/bc76e4b4-bc3e-46f0-b0c1-955c4c5fbe2a");
            audit.ComponentName.Should().Be("Test Subscription");
            audit.MetadataAzure.AuditId.Should().Be(auditId);
            audit.MetadataAzure.Date.Should().BeCloseTo(auditDate, TimeSpan.FromMinutes(1));
            audit.MetadataAzure.JSON.Should().NotBeNullOrWhiteSpace();

            // TODO: maybe check each check in more details?
            audit.CheckResults.Should().HaveCount(ExpectedCheckResults);
            return true;
        }
    }
}
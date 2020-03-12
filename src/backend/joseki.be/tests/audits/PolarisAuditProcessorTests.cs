using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using webapp.Audits;
using webapp.Audits.Processors.polaris;
using webapp.BlobStorage;
using webapp.Configuration;
using webapp.Database;
using webapp.Database.Models;
using webapp.Queues;

namespace tests.audits
{
    [TestClass]
    public class PolarisAuditProcessorTests
    {
        private const int UniqueImageTagCount = 7;
        private const int ExpectedCheckResults = 145;

        [TestMethod]
        public async Task ProcessAuditHappyPath()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var parser = new ConfigurationParser("config.sample.yaml");
            var checksCache = new ChecksCache(parser, context);

            var blobsMock = new Mock<IBlobStorageProcessor>(MockBehavior.Strict);
            var dbMock = new Mock<IJosekiDatabase>();
            var queueMock = new Mock<IQueue>();

            var processor = new PolarisAuditProcessor(blobsMock.Object, dbMock.Object, checksCache, queueMock.Object);

            var container = new ScannerContainer(Path.Combine("audits", "samples", "polaris"))
            {
                Metadata = new ScannerMetadata
                {
                    Type = ScannerType.Polaris,
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
                .Setup(i => i.DownloadFile($"{container.Name}/audit.json"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/audit.json"))
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/k8s-meta.json"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/k8s-meta.json"))
                .Verifiable();
            blobsMock
                .Setup(i => i.MarkAsProcessed(audit))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act & Assert
            await processor.Process(container, CancellationToken.None);
            blobsMock.Verify();

            dbMock.Verify(i => i.GetNotExpiredImageScans(It.Is<string[]>(tags => tags.Length == UniqueImageTagCount)));
            dbMock.Verify(i => i.SaveInProgressImageScan(It.IsAny<ImageScanResultWithCVEs>()), Times.Exactly(UniqueImageTagCount));
            queueMock.Verify(i => i.EnqueueImageScanRequest(It.IsAny<ImageScanResultWithCVEs>()), Times.Exactly(UniqueImageTagCount));

            dbMock.Verify(i => i.SaveAuditResult(It.Is<Audit>(a => VerifyHappyPathAudit(a, container))));
        }

        private static bool VerifyHappyPathAudit(Audit audit, ScannerContainer container)
        {
            const string auditId = "3aab7985-5756-4cd6-8e3f-3353aa4b98a4";
            var auditDate = DateTimeOffset.FromUnixTimeSeconds(1583892004).DateTime;

            audit.Id.Should().Be(auditId);
            audit.Date.Should().BeCloseTo(auditDate, TimeSpan.FromMinutes(1));
            audit.ScannerId.Should().Be($"{container.Metadata.Type}/{container.Metadata.Id}");
            audit.ComponentId.Should().Be("/k8s/3aab7985-d1a2-161d-8236-3353aa4b98a4");
            audit.ComponentName.Should().Be("10.0.0.1");
            audit.MetadataKube.AuditId.Should().Be(auditId);
            audit.MetadataKube.Date.Should().BeCloseTo(auditDate, TimeSpan.FromMinutes(1));
            audit.MetadataKube.JSON.Should().NotBeNullOrWhiteSpace();

            // TODO: maybe check each check in more details?
            audit.CheckResults.Should().HaveCount(ExpectedCheckResults);

            return true;
        }
    }
}
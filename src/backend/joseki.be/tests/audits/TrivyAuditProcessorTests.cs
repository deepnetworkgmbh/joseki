using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using webapp.Audits;
using webapp.Audits.Processors.trivy;
using webapp.BlobStorage;
using webapp.Configuration;
using webapp.Database;
using webapp.Database.Cache;
using webapp.Database.Models;

namespace tests.audits
{
    [TestClass]
    public class TrivyAuditProcessorTests
    {
        [TestMethod]
        public async Task ProcessFailedScan()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var cveCache = new CveCache(parser, context, new MemoryCache(new MemoryCacheOptions()));

            var blobsMock = new Mock<IBlobStorageProcessor>(MockBehavior.Strict);
            var dbMock = new Mock<IJosekiDatabase>();

            var processor = new TrivyAuditProcessor(blobsMock.Object, dbMock.Object, cveCache);

            var container = new ScannerContainer(Path.Combine("audits", "samples", "trivy"))
            {
                Metadata = new ScannerMetadata
                {
                    Type = ScannerType.Trivy,
                    Id = Guid.NewGuid().ToString(),
                },
            };
            var audit = new AuditBlob { Name = "meta_failed.json", ParentContainer = container };
            blobsMock
                .Setup(i => i.GetUnprocessedAudits(container))
                .ReturnsAsync(new[] { audit })
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/{audit.Name}"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/{audit.Name}"))
                .Verifiable();
            blobsMock
                .Setup(i => i.MarkAsProcessed(audit))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act & Assert
            await processor.Process(container, CancellationToken.None);
            blobsMock.Verify();

            dbMock.Verify(i => i.SaveImageScanResult(It.Is<ImageScanResultWithCVEs>(a => VerifyFailedScan(a))));
        }

        [TestMethod]
        public async Task ProcessScanResultWithoutCVEs()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var cveCache = new CveCache(parser, context, new MemoryCache(new MemoryCacheOptions()));

            var blobsMock = new Mock<IBlobStorageProcessor>(MockBehavior.Strict);
            var dbMock = new Mock<IJosekiDatabase>();

            var processor = new TrivyAuditProcessor(blobsMock.Object, dbMock.Object, cveCache);

            var container = new ScannerContainer(Path.Combine("audits", "samples", "trivy"))
            {
                Metadata = new ScannerMetadata
                {
                    Type = ScannerType.Trivy,
                    Id = Guid.NewGuid().ToString(),
                },
            };
            var audit = new AuditBlob { Name = "meta_no_cve.json", ParentContainer = container };
            blobsMock
                .Setup(i => i.GetUnprocessedAudits(container))
                .ReturnsAsync(new[] { audit })
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/{audit.Name}"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/{audit.Name}"))
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/result_no_cves.json"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/result_no_cves.json"))
                .Verifiable();
            blobsMock
                .Setup(i => i.MarkAsProcessed(audit))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act & Assert
            await processor.Process(container, CancellationToken.None);
            blobsMock.Verify();

            dbMock.Verify(i => i.SaveImageScanResult(It.Is<ImageScanResultWithCVEs>(a => VerifyNoCveScan(a))));
        }

        [TestMethod]
        public async Task ProcessScanResultWithSingleTarget()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var cveCache = new CveCache(parser, context, new MemoryCache(new MemoryCacheOptions()));

            var blobsMock = new Mock<IBlobStorageProcessor>(MockBehavior.Strict);
            var dbMock = new Mock<IJosekiDatabase>();

            var processor = new TrivyAuditProcessor(blobsMock.Object, dbMock.Object, cveCache);

            var container = new ScannerContainer(Path.Combine("audits", "samples", "trivy"))
            {
                Metadata = new ScannerMetadata
                {
                    Type = ScannerType.Trivy,
                    Id = Guid.NewGuid().ToString(),
                },
            };
            var audit = new AuditBlob { Name = "meta_single_target.json", ParentContainer = container };
            blobsMock
                .Setup(i => i.GetUnprocessedAudits(container))
                .ReturnsAsync(new[] { audit })
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/{audit.Name}"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/{audit.Name}"))
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/result_single_target.json"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/result_single_target.json"))
                .Verifiable();
            blobsMock
                .Setup(i => i.MarkAsProcessed(audit))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act & Assert
            await processor.Process(container, CancellationToken.None);
            blobsMock.Verify();

            dbMock.Verify(i => i.SaveImageScanResult(It.Is<ImageScanResultWithCVEs>(a => VerifySingleTargetScan(a))));
        }

        [TestMethod]
        public async Task ProcessScanResultWithMultipleTargets()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var cveCache = new CveCache(parser, context, new MemoryCache(new MemoryCacheOptions()));

            var blobsMock = new Mock<IBlobStorageProcessor>(MockBehavior.Strict);
            var dbMock = new Mock<IJosekiDatabase>();

            var processor = new TrivyAuditProcessor(blobsMock.Object, dbMock.Object, cveCache);

            var container = new ScannerContainer(Path.Combine("audits", "samples", "trivy"))
            {
                Metadata = new ScannerMetadata
                {
                    Type = ScannerType.Trivy,
                    Id = Guid.NewGuid().ToString(),
                },
            };
            var audit = new AuditBlob { Name = "meta_multi_target.json", ParentContainer = container };
            blobsMock
                .Setup(i => i.GetUnprocessedAudits(container))
                .ReturnsAsync(new[] { audit })
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/{audit.Name}"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/{audit.Name}"))
                .Verifiable();
            blobsMock
                .Setup(i => i.DownloadFile($"{container.Name}/result_multi_target.json"))
                .ReturnsAsync(File.OpenRead($"{container.Name}/result_multi_target.json"))
                .Verifiable();
            blobsMock
                .Setup(i => i.MarkAsProcessed(audit))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act & Assert
            await processor.Process(container, CancellationToken.None);
            blobsMock.Verify();

            dbMock.Verify(i => i.SaveImageScanResult(It.Is<ImageScanResultWithCVEs>(a => VerifyMultiTargetsScan(a))));
        }

        private static bool VerifyFailedScan(ImageScanResultWithCVEs scanResult)
        {
            var scanDate = DateTimeOffset.FromUnixTimeSeconds(1583894802).DateTime;

            scanResult.Id.Should().Be("1abcd181-e4ac-43a6-94c0-47f06bb3311e");
            scanResult.Date.Should().BeCloseTo(scanDate, TimeSpan.FromMinutes(1));
            scanResult.ImageTag.Should().Be("registry.com/repository/failed-image:v0.2");
            scanResult.Status.Should().Be(ImageScanStatus.Failed);
            scanResult.Description.Should().Be(TrivyScanDescriptionNormalizer.UnknownOS);

            return true;
        }

        private static bool VerifyNoCveScan(ImageScanResultWithCVEs scanResult)
        {
            var scanDate = DateTimeOffset.FromUnixTimeSeconds(1583894783).DateTime;

            scanResult.Id.Should().Be("44440604-22b6-4a8b-b1f7-0faddd20c37d");
            scanResult.Date.Should().BeCloseTo(scanDate, TimeSpan.FromMinutes(1));
            scanResult.ImageTag.Should().Be("registry.com/repository/no-cve-image:v0.2");
            scanResult.Status.Should().Be(ImageScanStatus.Succeeded);
            scanResult.Description.Should().BeNullOrEmpty();
            scanResult.FoundCVEs.Should().HaveCount(0);
            scanResult.Counters.Should().HaveCount(0);

            return true;
        }

        private static bool VerifySingleTargetScan(ImageScanResultWithCVEs scanResult)
        {
            // NOTE: the result has EIGHT CVE, but CVE-2019-13050 and CVE-2019-14855 are found in two different packages
            // Thus, there are EIGHT FoundCVEs, but counters has only SIX items (two medium and two low instead of three each)
            var scanDate = DateTimeOffset.FromUnixTimeSeconds(1583894784).DateTime;

            scanResult.Id.Should().Be("33340604-22b6-4a8b-b1f7-0faddd20c37a");
            scanResult.Date.Should().BeCloseTo(scanDate, TimeSpan.FromMinutes(1));
            scanResult.ImageTag.Should().Be("registry.com/repository/single-target-image:v0.2");
            scanResult.Status.Should().Be(ImageScanStatus.Succeeded);
            scanResult.Description.Should().BeNullOrEmpty();
            scanResult.FoundCVEs.Select(i => i.Target).Distinct().Should().HaveCount(1);
            scanResult.FoundCVEs.Should().HaveCount(8);
            foreach (var counter in scanResult.Counters)
            {
                switch (counter.Severity)
                {
                    case CveSeverity.High:
                        counter.Count.Should().Be(2, "single target result should have 2 HIGH priority CVEs");
                        break;
                    case CveSeverity.Medium:
                        counter.Count.Should().Be(2, "single target result should have 3 MEDIUM priority CVEs");
                        break;
                    case CveSeverity.Low:
                        counter.Count.Should().Be(2, "single target result should have 3 LOW priority CVEs");
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        private static bool VerifyMultiTargetsScan(ImageScanResultWithCVEs scanResult)
        {
            var scanDate = DateTimeOffset.FromUnixTimeSeconds(1583894785).DateTime;

            scanResult.Id.Should().Be("12240604-22b6-4a8b-b1f7-0faddd20c37e");
            scanResult.Date.Should().BeCloseTo(scanDate, TimeSpan.FromMinutes(1));
            scanResult.ImageTag.Should().Be("registry.com/repository/multi-target-image:v0.2");
            scanResult.Status.Should().Be(ImageScanStatus.Succeeded);
            scanResult.Description.Should().BeNullOrEmpty();
            scanResult.FoundCVEs.Select(i => i.Target).Distinct().Should().HaveCount(2);
            scanResult.FoundCVEs.Should().HaveCount(12);
            foreach (var counter in scanResult.Counters)
            {
                switch (counter.Severity)
                {
                    case CveSeverity.Critical:
                        counter.Count.Should().Be(1, "multi target result should have 1 CRITICAL priority CVE");
                        break;
                    case CveSeverity.High:
                        counter.Count.Should().Be(4, "multi target result should have 4 HIGH priority CVEs");
                        break;
                    case CveSeverity.Medium:
                        counter.Count.Should().Be(4, "multi target result should have 4 MEDIUM priority CVEs");
                        break;
                    case CveSeverity.Low:
                        counter.Count.Should().Be(2, "multi target result should have 2 LOW priority CVEs");
                        break;
                    case CveSeverity.Unknown:
                        counter.Count.Should().Be(1, "multi target result should have 1 CVE with UNKNOWN severity");
                        break;
                    default:
                        break;
                }
            }

            return true;
        }
    }
}
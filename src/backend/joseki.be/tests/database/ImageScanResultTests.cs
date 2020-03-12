using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using joseki.db.entities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Database;
using webapp.Database.Models;

using CheckValue = webapp.Database.Models.CheckValue;
using CveSeverity = joseki.db.entities.CveSeverity;
using ImageScanStatus = webapp.Database.Models.ImageScanStatus;

namespace tests.database
{
    [TestClass]
    public class ImageScanResultTests
    {
        [TestMethod]
        public void GetShortResultImageScanCalculatesCorrectCounters()
        {
            // Arrange
            // create 1 critical, 2 high, 3 medium, 4 low, 5 unknown severity issues
            var entity = new ImageScanResultEntity
            {
                FoundCVEs = new List<ImageScanToCveEntity>
                {
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Critical } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.High } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.High } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Medium } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Medium } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Medium } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Low } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Low } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Low } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Low } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Unknown } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Unknown } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Unknown } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Unknown } },
                    new ImageScanToCveEntity { CVE = new CveEntity { Severity = CveSeverity.Unknown } },
                },
            };

            // Act
            var scanResult = entity.GetShortResult();

            // Assert
            scanResult.Counters.Should().HaveCount(5);
            scanResult.Counters.First(i => i.Severity == webapp.Database.Models.CveSeverity.Critical).Count.Should().Be(1);
            scanResult.Counters.First(i => i.Severity == webapp.Database.Models.CveSeverity.High).Count.Should().Be(2);
            scanResult.Counters.First(i => i.Severity == webapp.Database.Models.CveSeverity.Medium).Count.Should().Be(3);
            scanResult.Counters.First(i => i.Severity == webapp.Database.Models.CveSeverity.Low).Count.Should().Be(4);
            scanResult.Counters.First(i => i.Severity == webapp.Database.Models.CveSeverity.Unknown).Count.Should().Be(5);
        }

        [TestMethod]
        [DataRow(ImageScanStatus.Queued, CheckValue.InProgress)]
        [DataRow(ImageScanStatus.Failed, CheckValue.NoData)]
        public void ImageScanResultGetCheckResultValue_NotSucceededStatuses(ImageScanStatus status, CheckValue expectedValue)
        {
            var scanResult = new ImageScanResult { Status = status };

            scanResult.GetCheckResultValue().Should().Be(expectedValue);
        }

        [TestMethod]
        public void ImageScanResultGetCheckResultValueSucceededWithoutCVE()
        {
            var scanResult = new ImageScanResult { Status = ImageScanStatus.Succeeded };

            scanResult.GetCheckResultValue().Should().Be(CheckValue.Succeeded);
        }

        [TestMethod]
        [DataRow(webapp.Database.Models.CveSeverity.Critical, CheckValue.Failed)]
        [DataRow(webapp.Database.Models.CveSeverity.High, CheckValue.Failed)]
        [DataRow(webapp.Database.Models.CveSeverity.Medium, CheckValue.Failed)]
        [DataRow(webapp.Database.Models.CveSeverity.Low, CheckValue.Succeeded)]
        [DataRow(webapp.Database.Models.CveSeverity.Unknown, CheckValue.Succeeded)]
        public void ImageScanResultGetCheckResultValueSucceededWithCVE(webapp.Database.Models.CveSeverity severity, CheckValue expectedValue)
        {
            var scanResult = new ImageScanResult
            {
                Status = ImageScanStatus.Succeeded,
                Counters = new[] { new VulnerabilityCounter { Severity = severity, Count = 1 } },
            };

            scanResult.GetCheckResultValue().Should().Be(expectedValue);
        }

        [TestMethod]
        [DataRow(ImageScanStatus.Queued, "The scan is in progress")]
        [DataRow(ImageScanStatus.Failed, "The image scan failed")]
        [DataRow(ImageScanStatus.Succeeded, "No issues")]
        public void ImageScanResultGetCheckResultMessageWithoutIssues(ImageScanStatus status, string expectedMessage)
        {
            var scanResult = new ImageScanResult
            {
                Status = status,
                Counters = new VulnerabilityCounter[0],
            };

            scanResult.GetCheckResultMessage().Should().Be(expectedMessage);
        }

        [TestMethod]
        public void ImageScanResultGetCheckResultMessageWithAllIssuesTypes()
        {
            var scanResult = new ImageScanResult
            {
                Status = ImageScanStatus.Succeeded,
                Counters = new[]
                {
                    new VulnerabilityCounter { Severity = webapp.Database.Models.CveSeverity.Unknown, Count = 5 },
                    new VulnerabilityCounter { Severity = webapp.Database.Models.CveSeverity.Medium, Count = 3 },
                    new VulnerabilityCounter { Severity = webapp.Database.Models.CveSeverity.Critical, Count = 1 },
                    new VulnerabilityCounter { Severity = webapp.Database.Models.CveSeverity.High, Count = 2 },
                    new VulnerabilityCounter { Severity = webapp.Database.Models.CveSeverity.Low, Count = 4 },
                },
            };
            var expectedMessage = "1 Critical; 2 High; 3 Medium; 4 Low; 5 Unknown";

            scanResult.GetCheckResultMessage().Should().Be(expectedMessage);
        }

        [TestMethod]
        public void ImageScanResultGetCheckResultMessageWithNotAllIssuesTypes()
        {
            var scanResult = new ImageScanResult
            {
                Status = ImageScanStatus.Succeeded,
                Counters = new[]
                {
                    new VulnerabilityCounter { Severity = webapp.Database.Models.CveSeverity.High, Count = 42 },
                    new VulnerabilityCounter { Severity = webapp.Database.Models.CveSeverity.Low, Count = 1024 },
                },
            };
            var expectedMessage = "42 High; 1024 Low";

            scanResult.GetCheckResultMessage().Should().Be(expectedMessage);
        }
    }
}
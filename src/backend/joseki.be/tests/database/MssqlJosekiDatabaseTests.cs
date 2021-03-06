﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db.entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Configuration;
using webapp.Database;
using webapp.Database.Models;

using CheckValue = webapp.Database.Models.CheckValue;
using CveSeverity = webapp.Database.Models.CveSeverity;
using ImageScanStatus = webapp.Database.Models.ImageScanStatus;

namespace tests.database
{
    [TestClass]
    public class MssqlJosekiDatabaseTests
    {
        [TestMethod]
        public async Task SaveAuditResultSavesCorrectAudit()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);
            var audit = new Audit
            {
                ComponentId = Guid.NewGuid().ToString(),
                ComponentName = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                ScannerId = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid().ToString(),
            };

            // Act & Assert
            context.Audit.Should().HaveCount(0);
            await db.SaveAuditResult(audit);
            context.Audit.Should().HaveCount(1);

            var actual = await context.Audit.FirstAsync();
            actual.AuditId.Should().Be(audit.Id);
            actual.Date.Should().Be(audit.Date);
            actual.ComponentId.Should().Be(audit.ComponentId);
        }

        [TestMethod]
        public async Task SaveAuditResultSavesAzureMetadata()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);
            var audit = new Audit
            {
                MetadataAzure = new MetadataAzure
                {
                    Date = DateTime.UtcNow,
                    JSON = Guid.NewGuid().ToString(),
                },
            };

            // Act & Assert
            context.MetadataAzure.Should().HaveCount(0);
            await db.SaveAuditResult(audit);
            context.MetadataAzure.Should().HaveCount(1);

            var actual = await context.MetadataAzure.FirstAsync();
            actual.JSON.Should().Be(audit.MetadataAzure.JSON);
            actual.Date.Should().Be(audit.MetadataAzure.Date);
        }

        [TestMethod]
        public async Task SaveAuditResultSavesKubeMetadata()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);
            var audit = new Audit
            {
                MetadataKube = new MetadataKube
                {
                    Date = DateTime.UtcNow,
                    JSON = Guid.NewGuid().ToString(),
                },
            };

            // Act & Assert
            context.MetadataKube.Should().HaveCount(0);
            await db.SaveAuditResult(audit);
            context.MetadataKube.Should().HaveCount(1);

            var actual = await context.MetadataKube.FirstAsync();
            actual.JSON.Should().Be(audit.MetadataKube.JSON);
            actual.Date.Should().Be(audit.MetadataKube.Date);
        }

        [TestMethod]
        public async Task SaveAuditResultSavesCheckResults()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);
            var audit = new Audit
            {
                CheckResults = new List<CheckResult>
                {
                    new CheckResult { InternalCheckId = 1, ComponentId = Guid.NewGuid().ToString(), Message = Guid.NewGuid().ToString(), Value = CheckValue.NoData },
                    new CheckResult { InternalCheckId = 2, ComponentId = Guid.NewGuid().ToString(), Message = Guid.NewGuid().ToString(), Value = CheckValue.Failed },
                    new CheckResult { InternalCheckId = 3, ComponentId = Guid.NewGuid().ToString(), Message = Guid.NewGuid().ToString(), Value = CheckValue.InProgress },
                    new CheckResult { InternalCheckId = 4, ComponentId = Guid.NewGuid().ToString(), Message = Guid.NewGuid().ToString(), Value = CheckValue.Succeeded },
                },
            };

            // Act & Assert
            context.CheckResult.Should().HaveCount(0);
            await db.SaveAuditResult(audit);
            context.CheckResult.Should().HaveCount(4);

            var entities = await context.CheckResult.ToArrayAsync();
            foreach (var actual in entities)
            {
                var expected = audit.CheckResults.First(i => i.InternalCheckId == actual.CheckId);
                actual.ComponentId.Should().Be(expected.ComponentId);
                actual.Message.Should().Be(expected.Message);
                actual.Value.Should().Be(expected.Value.ToEntity());
            }
        }

        [TestMethod]
        public async Task SaveInProgressImageScanSavesCorrectEntity()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);
            var scan = new ImageScanResultWithCVEs
            {
                Date = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageTag = Guid.NewGuid().ToString(),
                Status = ImageScanStatus.Queued,
            };

            // Act & Assert
            context.ImageScanResult.Should().HaveCount(0);
            await db.SaveInProgressImageScan(scan);
            context.ImageScanResult.Should().HaveCount(1);

            var actual = await context.ImageScanResult.FirstAsync();
            actual.Date.Should().Be(scan.Date);
            actual.ExternalId.Should().Be(scan.Id);
            actual.Description.Should().Be(scan.Description);
            actual.ImageTag.Should().Be(scan.ImageTag);
            actual.Status.Should().Be(joseki.db.entities.ImageScanStatus.Queued);
        }

        [TestMethod]
        public async Task SaveImageScanResultCouldSaveNewEntity()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);
            var cves = new List<ImageScanToCve>
            {
                new ImageScanToCve { InternalCveId = 1, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                new ImageScanToCve { InternalCveId = 2, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                new ImageScanToCve { InternalCveId = 3, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
            };
            var scan = new ImageScanResultWithCVEs
            {
                Date = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageTag = Guid.NewGuid().ToString(),
                Status = ImageScanStatus.Succeeded,
                FoundCVEs = cves,
            };

            // Act & Assert
            context.ImageScanResult.Should().HaveCount(0);
            context.ImageScanResultToCve.Should().HaveCount(0);

            await db.SaveImageScanResult(scan);

            context.ImageScanResult.Should().HaveCount(1);
            context.ImageScanResultToCve.Should().HaveCount(cves.Count);

            var actual = await context.ImageScanResult.FirstAsync();
            actual.Date.Should().Be(scan.Date);
            actual.ExternalId.Should().Be(scan.Id);
            actual.Description.Should().Be(scan.Description);
            actual.ImageTag.Should().Be(scan.ImageTag);
            actual.Status.Should().Be(joseki.db.entities.ImageScanStatus.Succeeded);

            foreach (var actualCve in await context.ImageScanResultToCve.ToArrayAsync())
            {
                var expectedCve = cves.First(i => i.InternalCveId == actualCve.CveId);
                actualCve.Target.Should().Be(expectedCve.Target);
                actualCve.UsedPackage.Should().Be(expectedCve.UsedPackage);
                actualCve.UsedPackageVersion.Should().Be(expectedCve.UsedPackageVersion);
            }
        }

        [TestMethod]
        public async Task SaveImageScanResultCouldUpdateExistingEntity()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var queuedScan = new ImageScanResultEntity
            {
                Date = DateTime.UtcNow.AddMinutes(-10),
                ExternalId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageTag = Guid.NewGuid().ToString(),
                Status = joseki.db.entities.ImageScanStatus.Queued,
            };
            context.Set<ImageScanResultEntity>().Add(queuedScan);
            await context.SaveChangesAsync();

            var scan = new ImageScanResultWithCVEs
            {
                Date = DateTime.UtcNow,
                Id = queuedScan.ExternalId,
                Description = Guid.NewGuid().ToString(),
                ImageTag = queuedScan.ImageTag,
                Status = ImageScanStatus.Succeeded,
                FoundCVEs = new List<ImageScanToCve>
                {
                    new ImageScanToCve { InternalCveId = 1, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                    new ImageScanToCve { InternalCveId = 2, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                    new ImageScanToCve { InternalCveId = 3, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                },
            };

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            // Act & Assert
            context.ImageScanResult.Should().HaveCount(1);
            context.ImageScanResultToCve.Should().HaveCount(0);

            await db.SaveImageScanResult(scan);

            context.ImageScanResult.Should().HaveCount(1);
            context.ImageScanResultToCve.Should().HaveCount(scan.FoundCVEs.Count);

            // Only Date, Description, Status, and CVEs should be updated
            var actual = await context.ImageScanResult.FirstAsync();
            actual.Date.Should().Be(scan.Date);
            actual.Description.Should().Be(scan.Description);
            actual.Status.Should().Be(joseki.db.entities.ImageScanStatus.Succeeded);

            foreach (var actualCve in await context.ImageScanResultToCve.ToArrayAsync())
            {
                var expectedCve = scan.FoundCVEs.First(i => i.InternalCveId == actualCve.CveId);
                actualCve.Target.Should().Be(expectedCve.Target);
                actualCve.UsedPackage.Should().Be(expectedCve.UsedPackage);
                actualCve.UsedPackageVersion.Should().Be(expectedCve.UsedPackageVersion);
            }
        }

        [TestMethod]
        public async Task SaveImageScanResultUpdatesInProgressCheckResults()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var scan = new ImageScanResultWithCVEs
            {
                Date = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageTag = Guid.NewGuid().ToString(),
                Status = ImageScanStatus.Succeeded,
                FoundCVEs = new List<ImageScanToCve>
                {
                    new ImageScanToCve { InternalCveId = 1, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                    new ImageScanToCve { InternalCveId = 2, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                    new ImageScanToCve { InternalCveId = 3, Target = Guid.NewGuid().ToString(), UsedPackage = Guid.NewGuid().ToString(), UsedPackageVersion = Guid.NewGuid().ToString() },
                },
                Counters = new[]
                {
                    new VulnerabilityCounter { Count = 1, Severity = CveSeverity.High },
                    new VulnerabilityCounter { Count = 2, Severity = CveSeverity.Medium },
                },
            };

            // only the first one should be updated
            var checkResults = new[]
            {
                new CheckResultEntity { Id = 100, Value = joseki.db.entities.CheckValue.InProgress, ComponentId = $"/k8s/.../image/{scan.ImageTag}", DateUpdated = DateTime.UtcNow.AddDays(-1) },
                new CheckResultEntity { Id = 200, Value = joseki.db.entities.CheckValue.InProgress, ComponentId = $"/k8s/.../image/{Guid.NewGuid()}", DateUpdated = DateTime.UtcNow.AddDays(-1) },
                new CheckResultEntity { Id = 300, Value = joseki.db.entities.CheckValue.Succeeded, ComponentId = $"/k8s/.../image/{scan.ImageTag}", DateUpdated = DateTime.UtcNow.AddDays(-1) },
            };

            // this is the hack -_-
            // Use sync version, because it does not update DateUpdated & DateCreated
            context.CheckResult.AddRange(checkResults);
            context.SaveChanges();

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            // Act & Assert
            var saveRequestedAt = DateTime.UtcNow;
            await db.SaveImageScanResult(scan);

            var updatedCheckResults = await context.CheckResult.Where(i => i.DateUpdated >= saveRequestedAt).ToArrayAsync();
            updatedCheckResults.Should().HaveCount(1);
            updatedCheckResults.First().Id.Should().Be(100);
        }

        [TestMethod]
        public async Task GetNotExpiredImageScansReturnsEmptyArrayIfNoActualScanResult()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            context.ImageScanResult.Add(new ImageScanResultEntity { ImageTag = Guid.NewGuid().ToString(), Date = DateTime.UtcNow.AddHours(-1) });
            await context.SaveChangesAsync();

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            // Act & Assert
            var notExpiredScans = await db.GetNotExpiredImageScans(new[] { Guid.NewGuid().ToString() });
            notExpiredScans.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetNotExpiredImageScansReturnsOnlyNotExpiredScans()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            var notExpiredTag = Guid.NewGuid().ToString();
            var expiredTag = Guid.NewGuid().ToString();
            var scans = new[]
            {
                new ImageScanResultEntity { ImageTag = expiredTag, Date = DateTime.UtcNow.AddHours(-(parser.Get().Cache.ImageScanTtl + 1)), Status = joseki.db.entities.ImageScanStatus.Failed },
                new ImageScanResultEntity { ImageTag = notExpiredTag, Date = DateTime.UtcNow.AddHours(-1), Status = joseki.db.entities.ImageScanStatus.Succeeded },
                new ImageScanResultEntity { ImageTag = Guid.NewGuid().ToString(), Date = DateTime.UtcNow, Status = joseki.db.entities.ImageScanStatus.Queued },
            };
            context.ImageScanResult.AddRange(scans);
            await context.SaveChangesAsync();

            // Act & Assert
            var notExpiredScans = await db.GetNotExpiredImageScans(new[] { expiredTag, notExpiredTag });
            notExpiredScans.Should().HaveCount(1);
            notExpiredScans.First().Status.Should().Be(ImageScanStatus.Succeeded);
        }

        [TestMethod]
        public async Task GetNotExpiredImageScansReturnsTheLatestScan()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            var tag = Guid.NewGuid().ToString();
            var scans = new[]
            {
                new ImageScanResultEntity { ImageTag = tag, Date = DateTime.UtcNow.AddHours(-5), Status = joseki.db.entities.ImageScanStatus.Failed },
                new ImageScanResultEntity { ImageTag = tag, Date = DateTime.UtcNow, Status = joseki.db.entities.ImageScanStatus.Queued },
                new ImageScanResultEntity { ImageTag = tag, Date = DateTime.UtcNow.AddHours(-3), Status = joseki.db.entities.ImageScanStatus.Succeeded },
            };
            context.ImageScanResult.AddRange(scans);
            await context.SaveChangesAsync();

            // Act & Assert
            var notExpiredScans = await db.GetNotExpiredImageScans(new[] { tag });
            notExpiredScans.Should().HaveCount(1);
            notExpiredScans.First().Status.Should().Be(ImageScanStatus.Queued);
        }

        [TestMethod]
        public async Task GetAuditedComponentsWithHistoryReturnsEmptyArrayIfNoAudits()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            // Act & Assert
            var audits = await db.GetAuditedComponentsWithHistory(DateTime.UtcNow);
            audits.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAuditedComponentsWithHistoryReturnsOnlyOneMonthData()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            var componentId = Guid.NewGuid().ToString();
            var today = DateTime.UtcNow;

            // create more than 31 entries, which the oldest ones would be filtered-out
            var entities = Enumerable.Range(0, 35)
                .Select(i => today.AddDays(-i)).Select(i => new AuditEntity
                {
                    Date = i,
                    ComponentId = componentId,
                    InfrastructureComponent = new InfrastructureComponentEntity(),
                });
            context.AddRange(entities);
            await context.SaveChangesAsync();

            // Act & Assert
            var audits = await db.GetAuditedComponentsWithHistory(today);
            audits.Should().HaveCount(31);
            audits.All(i => i.Date >= today.AddDays(-30)).Should().BeTrue("All audits should be not earlier than 30 days ago");
        }

        [TestMethod]
        public async Task GetAuditedComponentsWithHistoryReturnsOnlyOneAuditPerDay()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var parser = new ConfigurationParser("config.sample.yaml");
            var db = new MssqlJosekiDatabase(context, parser);

            // Create two audits at each day during the last month: at 3am and 10am
            var componentId = Guid.NewGuid().ToString();
            var today3am = DateTime.UtcNow.Date.AddHours(3);
            var today10am = today3am.AddHours(7);
            var entries3am = Enumerable.Range(0, 31)
                .Select(i => new AuditEntity
                {
                    Date = today3am.AddDays(-i),
                    ComponentId = componentId,
                    InfrastructureComponent = new InfrastructureComponentEntity(),
                });
            var entries10am = Enumerable.Range(0, 31)
                .Select(i => new AuditEntity
                {
                    Date = today10am.AddDays(-i),
                    ComponentId = componentId,
                    InfrastructureComponent = new InfrastructureComponentEntity(),
                });
            context.AddRange(entries3am);
            context.AddRange(entries10am);
            context.Add(new InfrastructureComponentEntity { ComponentId = componentId });
            await context.SaveChangesAsync();

            // Act & Assert
            // DB object should choose only one latest audit at each day (e.g. discard all 3am items, but leave 10am ones)
            var audits = await db.GetAuditedComponentsWithHistory(today10am.AddHours(2));
            audits.Should().HaveCount(31);
            audits.All(i => i.Date.Hour == 10).Should().BeTrue("All audits should be at 10 am");
        }
    }
}
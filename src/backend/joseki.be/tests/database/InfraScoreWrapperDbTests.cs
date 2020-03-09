using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Database;

namespace tests.database
{
    [TestClass]
    public class InfraScoreWrapperDbTests
    {
        [TestMethod]
        public async Task GetAllComponentsIdsReturnsIdsForLast31Days()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            // create two audits, that satisfy the criteria ...
            var today = DateTime.UtcNow.Date;
            var correctAudits = new[]
            {
                new AuditEntity { Date = today, ComponentId = Guid.NewGuid().ToString() },
                new AuditEntity { Date = today.AddDays(-30), ComponentId = Guid.NewGuid().ToString() },
            };
            await context.Audit.AddRangeAsync(correctAudits);

            // ... and three audits, that do not
            var incorrectAudits = new[]
            {
                new AuditEntity { Date = today.AddDays(-31), ComponentId = Guid.NewGuid().ToString() },
                new AuditEntity { Date = today.AddDays(-60), ComponentId = Guid.NewGuid().ToString() },
                new AuditEntity { Date = today.AddDays(-90), ComponentId = Guid.NewGuid().ToString() },
            };
            await context.Audit.AddRangeAsync(incorrectAudits);

            await context.SaveChangesAsync();

            // Act & Assert
            var allComponentsIds = await wrapper.GetAllComponentsIds();

            // returned array should have only items from correctAudits variable
            allComponentsIds.Should().HaveCount(correctAudits.Length);
            allComponentsIds.Should().Contain(correctAudits.Select(i => i.ComponentId));
        }

        [TestMethod]
        public async Task GetAllComponentsIdsReturnsOnlyUniqueIds()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var today = DateTime.UtcNow.Date;
            var componentId = Guid.NewGuid().ToString();
            var audits = new[]
            {
                new AuditEntity { Date = today, ComponentId = componentId },
                new AuditEntity { Date = today.AddDays(-1), ComponentId = componentId },
            };
            await context.Audit.AddRangeAsync(audits);
            await context.SaveChangesAsync();

            // Act & Assert
            var allComponentsIds = await wrapper.GetAllComponentsIds();

            // returned array should have the only one componentId
            allComponentsIds.Should().ContainSingle(componentId);
        }

        [TestMethod]
        public async Task GetLastMonthAuditsReturnsOnlyRequestedComponentAudits()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditDate = DateTime.UtcNow.Date.AddDays(-1);
            var componentId = Guid.NewGuid().ToString();
            await context.Audit.AddRangeAsync(new[]
            {
                new AuditEntity { Date = auditDate, ComponentId = componentId },
                new AuditEntity { Date = DateTime.UtcNow.AddDays(-2), ComponentId = Guid.NewGuid().ToString() },
            });
            await context.SaveChangesAsync();

            // Act & Assert
            var audits = await wrapper.GetLastMonthAudits(componentId);
            audits.Should().ContainSingle(a => a.ComponentId == componentId && a.Date == auditDate);
        }

        [TestMethod]
        public async Task GetLastMonthAuditsReturnsOnlyLast31DaysAudits()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var componentId = Guid.NewGuid().ToString();
            var today = DateTime.UtcNow.Date;
            var entities = Enumerable
                .Range(0, 35)
                .Select(i => new AuditEntity { Date = today.AddDays(-i), ComponentId = componentId });

            await context.Audit.AddRangeAsync(entities);
            await context.SaveChangesAsync();

            // Act
            var audits = await wrapper.GetLastMonthAudits(componentId);

            // Assert
            var oneMonthAgo = today.AddDays(-30);
            audits.Should().HaveCount(31);
            audits.All(i => i.Date >= oneMonthAgo).Should().BeTrue();
        }

        [TestMethod]
        public async Task GetLastMonthAuditsReturnsOnlyLatestAuditAtAnyGivenDay()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            // create three audits at each day at 00:00, 08:00, 16:00
            var componentId = Guid.NewGuid().ToString();
            var today = DateTime.UtcNow.Date;
            foreach (var auditDate in Enumerable.Range(0, 31).Select(i => today.AddDays(-i)))
            {
                var entities = Enumerable
                    .Range(0, 3)
                    .Select(i => new AuditEntity { Date = auditDate.AddHours(i * 8), ComponentId = componentId });
                await context.Audit.AddRangeAsync(entities);
            }

            await context.SaveChangesAsync();

            // Act
            var audits = await wrapper.GetLastMonthAudits(componentId);

            // Assert
            // returned audit should be the latest one at each day
            audits.Should().HaveCount(31);
            audits.All(i => i.Date.Hour == 16).Should().BeTrue();
        }

        [TestMethod]
        public async Task GetAuditReturnsOnlyLatestAuditAtRequestedDate()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            // create three audits a day before at 00:00, 08:00, 16:00
            var componentId = Guid.NewGuid().ToString();
            var auditDate = DateTime.UtcNow.Date.AddDays(-1);
            var entities = Enumerable
                .Range(0, 3)
                .Select(i => new AuditEntity { Date = auditDate.AddHours(i * 8), ComponentId = componentId });
            await context.Audit.AddRangeAsync(entities);

            await context.SaveChangesAsync();

            // Act
            var audit = await wrapper.GetAudit(componentId, auditDate);

            // Assert
            // returned audit should be the latest one at requested day
            audit.Date.Hour.Should().Be(16);
        }

        [TestMethod]
        public async Task GetAuditReturnsOnlyRequestedComponentAudit()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditDate = DateTime.UtcNow.Date.AddDays(-1);
            var componentId = Guid.NewGuid().ToString();
            await context.Audit.AddRangeAsync(new[]
            {
                new AuditEntity { Date = auditDate, ComponentId = componentId },
                new AuditEntity { Date = auditDate, ComponentId = Guid.NewGuid().ToString() },
            });
            await context.SaveChangesAsync();

            // Act
            var audit = await wrapper.GetAudit(componentId, auditDate);

            // Assert
            audit.ComponentId.Should().Be(componentId);
            audit.Date.Should().Be(auditDate);
        }

        [TestMethod]
        public async Task GetAuditReturnsOnlyAuditAtRequestedDay()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditDate = DateTime.UtcNow.Date.AddDays(-1);
            var componentId = Guid.NewGuid().ToString();
            await context.Audit.AddRangeAsync(new[]
            {
                new AuditEntity { Date = auditDate, ComponentId = componentId },
                new AuditEntity { Date = auditDate.AddDays(-1), ComponentId = componentId },
            });
            await context.SaveChangesAsync();

            // Act
            var audit = await wrapper.GetAudit(componentId, auditDate);

            // Assert
            audit.ComponentId.Should().Be(componentId);
            audit.Date.Should().Be(auditDate);
        }

        [TestMethod]
        public async Task GetAuditsReturnsOnlyAuditsAtRequestedDay()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditDate = DateTime.UtcNow.Date.AddDays(-1);
            await context.Audit.AddRangeAsync(new[]
            {
                new AuditEntity { Date = auditDate, ComponentId = Guid.NewGuid().ToString() },
                new AuditEntity { Date = auditDate.AddDays(-1), ComponentId = Guid.NewGuid().ToString() },
                new AuditEntity { Date = auditDate.AddDays(1), ComponentId = Guid.NewGuid().ToString() },
            });
            await context.SaveChangesAsync();

            // Act
            var audits = await wrapper.GetAudits(auditDate);

            // Assert
            audits.Should().ContainSingle(i => i.Date == auditDate);
        }

        [TestMethod]
        public async Task GetAuditsReturnsOnlyLatestAuditAtRequestedDate()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            // create three audits a day before at 00:00, 08:00, 16:00
            var componentId = Guid.NewGuid().ToString();
            var auditDate = DateTime.UtcNow.Date.AddDays(-1);
            var entities = Enumerable
                .Range(0, 3)
                .Select(i => new AuditEntity { Date = auditDate.AddHours(i * 8), ComponentId = componentId });
            await context.Audit.AddRangeAsync(entities);

            await context.SaveChangesAsync();

            // Act
            var audits = await wrapper.GetAudits(auditDate);

            // Assert
            // returned audit should be the latest one at requested day
            audits.Should().ContainSingle(i => i.Date.Hour == 16);
        }

        [TestMethod]
        public async Task GetAuditsReturnsOnlyUniqueComponentAudits()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditDate = DateTime.UtcNow.Date.AddDays(-1);
            var componentId = Guid.NewGuid().ToString();
            await context.Audit.AddRangeAsync(new[]
            {
                new AuditEntity { Id = 1, Date = auditDate, ComponentId = componentId }, // this one should be ignored
                new AuditEntity { Id = 2, Date = auditDate.AddHours(6), ComponentId = componentId },
                new AuditEntity { Id = 3, Date = auditDate.AddHours(12), ComponentId = Guid.NewGuid().ToString() },
                new AuditEntity { Id = 4, Date = auditDate.AddHours(23), ComponentId = Guid.NewGuid().ToString() },
            });
            await context.SaveChangesAsync();

            // Act
            var audits = await wrapper.GetAudits(auditDate);

            // Assert
            audits.Should().HaveCount(3);
            audits.All(i => i.Id > 1).Should().BeTrue();
        }

        [TestMethod]
        public async Task GetCounterSummariesForAuditReturnsSummaryOnlyForRequestedAudit()
        {
            // Arrange
            var randomizer = new Random();
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditId = randomizer.Next();
            await context.CheckResult.AddRangeAsync(new[]
            {
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Succeeded, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
                new CheckResultEntity { AuditId = auditId + 1, Value = CheckValue.Succeeded, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
            });
            await context.SaveChangesAsync();

            // Act
            var summary = await wrapper.GetCounterSummariesForAudit(auditId);

            // Assert
            summary.Failed.Should().Be(0);
            summary.Warning.Should().Be(0);
            summary.NoData.Should().Be(0);
            summary.Passed.Should().Be(1);
        }

        [TestMethod]
        public async Task GetCounterSummariesForAuditReturnsZeroSummaryForNotExistingId()
        {
            // Arrange
            var randomizer = new Random();
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditId = randomizer.Next();
            await context.CheckResult.AddRangeAsync(new[]
            {
                new CheckResultEntity { AuditId = auditId - 1, Value = CheckValue.Succeeded, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
                new CheckResultEntity { AuditId = auditId + 1, Value = CheckValue.Succeeded, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
            });
            await context.SaveChangesAsync();

            // Act
            var summary = await wrapper.GetCounterSummariesForAudit(auditId);

            // Assert
            summary.Failed.Should().Be(0);
            summary.Warning.Should().Be(0);
            summary.NoData.Should().Be(0);
            summary.Passed.Should().Be(0);
        }

        [TestMethod]
        public async Task GetCounterSummariesForAuditCountsOnlyCriticalAndHighSeverityIssuesAsFailed()
        {
            // Arrange
            var randomizer = new Random();
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditId = randomizer.Next();
            await context.CheckResult.AddRangeAsync(new[]
            {
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.High } },
            });
            await context.SaveChangesAsync();

            // Act
            var summary = await wrapper.GetCounterSummariesForAudit(auditId);

            // Assert
            summary.Failed.Should().Be(2);
            summary.Warning.Should().Be(0);
            summary.NoData.Should().Be(0);
            summary.Passed.Should().Be(0);
        }

        [TestMethod]
        public async Task GetCounterSummariesForAuditCountsAllNotCriticalAndHighSeverityIssuesAsWarnings()
        {
            // Arrange
            var randomizer = new Random();
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditId = randomizer.Next();
            await context.CheckResult.AddRangeAsync(new[]
            {
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Medium } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Low } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Unknown } },
            });
            await context.SaveChangesAsync();

            // Act
            var summary = await wrapper.GetCounterSummariesForAudit(auditId);

            // Assert
            summary.Failed.Should().Be(0);
            summary.Warning.Should().Be(3);
            summary.NoData.Should().Be(0);
            summary.Passed.Should().Be(0);
        }

        [TestMethod]
        public async Task GetCounterSummariesForAuditCountsNoDataAndInProgressAsNoData()
        {
            // Arrange
            var randomizer = new Random();
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditId = randomizer.Next();
            await context.CheckResult.AddRangeAsync(new[]
            {
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.NoData, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.InProgress, Check = new CheckEntity { Severity = CheckSeverity.High } },
            });
            await context.SaveChangesAsync();

            // Act
            var summary = await wrapper.GetCounterSummariesForAudit(auditId);

            // Assert
            summary.Failed.Should().Be(0);
            summary.Warning.Should().Be(0);
            summary.NoData.Should().Be(2);
            summary.Passed.Should().Be(0);
        }

        [TestMethod]
        public async Task GetCounterSummariesForAuditReturnsCorrectSummary()
        {
            // Arrange
            var randomizer = new Random();
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var wrapper = new InfraScoreDbWrapper(context);

            var auditId = randomizer.Next();
            await context.CheckResult.AddRangeAsync(new[]
            {
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Succeeded, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.High } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Medium } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Low } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.Failed, Check = new CheckEntity { Severity = CheckSeverity.Unknown } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.NoData, Check = new CheckEntity { Severity = CheckSeverity.Critical } },
                new CheckResultEntity { AuditId = auditId, Value = CheckValue.InProgress, Check = new CheckEntity { Severity = CheckSeverity.High } },
            });
            await context.SaveChangesAsync();

            // Act
            var summary = await wrapper.GetCounterSummariesForAudit(auditId);

            // Assert
            summary.Failed.Should().Be(2);
            summary.Warning.Should().Be(3);
            summary.NoData.Should().Be(2);
            summary.Passed.Should().Be(1);
        }
    }
}
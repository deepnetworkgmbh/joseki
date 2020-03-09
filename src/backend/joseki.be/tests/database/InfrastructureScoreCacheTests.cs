using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db.entities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using webapp.Database;
using webapp.Database.Models;
using webapp.Models;

namespace tests.database
{
    [TestClass]
    public class InfrastructureScoreCacheTests
    {
        private static readonly Random Randomizer = new Random();
        private static readonly string[] ComponentIds = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        private static readonly DateTime[] AuditDates = new[]
        {
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(-1),
            DateTime.UtcNow.Date.AddDays(-2),
            DateTime.UtcNow.Date.AddDays(-3),
            DateTime.UtcNow.Date.AddDays(-4),
        };

        private static List<(string componentId, DateTime date, CountersSummary summary)> counters;
        private static Mock<IInfraScoreDbWrapper> dbMock;
        private static InfrastructureScoreCache cache;

        [ClassInitialize]
        public static async Task ClassInit(TestContext testContext)
        {
            counters = new List<(string componentId, DateTime date, CountersSummary summary)>();
            dbMock = new Mock<IInfraScoreDbWrapper>(MockBehavior.Strict);

            dbMock.Setup(db => db.GetAllComponentsIds()).ReturnsAsync(ComponentIds).Verifiable();

            foreach (var id in ComponentIds)
            {
                var audits = AuditDates.Select(i => new AuditEntity { Id = Randomizer.Next(), Date = i, ComponentId = id }).ToArray();
                dbMock.Setup(db => db.GetLastMonthAudits(id)).ReturnsAsync(audits).Verifiable();

                foreach (var audit in audits)
                {
                    var summary = new CountersSummary
                    {
                        Failed = Randomizer.Next(0, 100),
                        Warning = Randomizer.Next(0, 100),
                        NoData = Randomizer.Next(0, 100),
                        Passed = Randomizer.Next(0, 100),
                    };
                    counters.Add((id, audit.Date, summary));
                    dbMock.Setup(db => db.GetCounterSummariesForAudit(audit.Id)).ReturnsAsync(summary).Verifiable();
                }
            }

            cache = new InfrastructureScoreCache(dbMock.Object);
            await cache.ReloadEntireCache();
            dbMock.Verify();
        }

        [TestInitialize]
        public void TestInit()
        {
            // NOTE: The cache is expected to have all values in memory,
            // so no actual db-requests are done when values from class-init are requested
            // Reset Mock ensures that, by causing exceptions on any db request
            // Also, it avoids single tests setup overlapping
            dbMock.Reset();
        }

        [TestMethod]
        public async Task ReloadEntireCacheSavesAllComponentSummaries()
        {
            // Act & Assert
            foreach (var componentId in ComponentIds)
            {
                foreach (var auditDate in AuditDates)
                {
                    var actualSummary = await cache.GetCountersSummary(componentId, auditDate);
                    var (_, _, expectedSummary) = counters.First(i => i.componentId == componentId && i.date == auditDate);
                    actualSummary.NoData.Should().Be(expectedSummary.NoData);
                    actualSummary.Warning.Should().Be(expectedSummary.Warning);
                    actualSummary.Failed.Should().Be(expectedSummary.Failed);
                    actualSummary.Passed.Should().Be(expectedSummary.Passed);
                }
            }
        }

        [TestMethod]
        public async Task ReloadEntireCacheSavesOverallSummaries()
        {
            // Act & Assert
            foreach (var auditDate in AuditDates)
            {
                var actualSummary = await cache.GetCountersSummary(Audit.OverallId, auditDate);

                var expectedSummary = new CountersSummary();
                var oneDayAudits = counters.Where(i => i.date == auditDate).Select(i => i.summary).ToArray();
                Trace.WriteLine(counters.Count);
                foreach (var summary in oneDayAudits)
                {
                    expectedSummary.Add(summary);
                }

                actualSummary.NoData.Should().Be(expectedSummary.NoData);
                actualSummary.Warning.Should().Be(expectedSummary.Warning);
                actualSummary.Failed.Should().Be(expectedSummary.Failed);
                actualSummary.Passed.Should().Be(expectedSummary.Passed);
            }
        }

        [TestMethod]
        public async Task RequestingNotCachedDateLeadsToDbRequest()
        {
            // Arrange
            var auditDate = DateTime.UtcNow.Date.AddDays(-31);
            var componentId = ComponentIds.First();

            // Act & Assert
            await cache
                .Invoking(c => c.GetCountersSummary(componentId, auditDate))
                .Should()
                .ThrowAsync<Exception>();
        }

        [TestMethod]
        public async Task RequestingNotCachedComponentIdLeadsToDbRequest()
        {
            // Arrange
            var componentId = Guid.NewGuid().ToString();
            var auditDate = AuditDates.First();

            // Act & Assert
            await cache
                .Invoking(c => c.GetCountersSummary(componentId, auditDate))
                .Should()
                .ThrowAsync<Exception>();
        }

        [TestMethod]
        public async Task RequestingNotExistingComponentIdSavesEmptyRecord()
        {
            // Arrange
            var componentId = Guid.NewGuid().ToString();
            var auditDate = AuditDates.First();
            dbMock.Setup(i => i.GetAudit(componentId, auditDate)).ReturnsAsync((AuditEntity)null).Verifiable();

            // Act
            var summary = await cache.GetCountersSummary(componentId, auditDate);

            // Assert
            dbMock.Verify();
            summary.Total.Should().Be(0);
        }

        [TestMethod]
        public async Task RequestingNotCachedComponentReloadsItFromDb()
        {
            // Arrange
            var componentId = Guid.NewGuid().ToString();
            var auditDate = AuditDates.First();

            var audit = new AuditEntity { Id = Randomizer.Next(), Date = auditDate, ComponentId = componentId };
            dbMock.Setup(i => i.GetAudit(componentId, auditDate)).ReturnsAsync(audit).Verifiable();

            var expectedSummary = new CountersSummary
            {
                Failed = Randomizer.Next(0, 100),
                Warning = Randomizer.Next(0, 100),
                NoData = Randomizer.Next(0, 100),
                Passed = Randomizer.Next(0, 100),
            };
            dbMock.Setup(i => i.GetCounterSummariesForAudit(audit.Id)).ReturnsAsync(expectedSummary).Verifiable();

            // Act
            var actualSummary = await cache.GetCountersSummary(componentId, auditDate);

            // Assert
            dbMock.Verify();

            actualSummary.NoData.Should().Be(expectedSummary.NoData);
            actualSummary.Warning.Should().Be(expectedSummary.Warning);
            actualSummary.Failed.Should().Be(expectedSummary.Failed);
            actualSummary.Passed.Should().Be(expectedSummary.Passed);
        }

        [TestMethod]
        public async Task RequestingNotExistingDateForOverallReturnsEmptySummary()
        {
            // Arrange
            var componentId = Audit.OverallId;
            var auditDate = DateTime.UtcNow.Date.AddDays(1);
            dbMock.Setup(i => i.GetAudits(auditDate)).ReturnsAsync(new AuditEntity[0]).Verifiable();

            // Act
            var summary = await cache.GetCountersSummary(componentId, auditDate);

            // Assert
            dbMock.Verify();
            summary.Total.Should().Be(0);
        }

        [TestMethod]
        public async Task RequestingNotCachedDateForOverallReloadsAllComponentsFromDb()
        {
            // Arrange
            var auditDate = AuditDates.OrderBy(i => i).First().AddDays(-1);
            var expectedSummary = new CountersSummary();

            var audits = ComponentIds.Select(id => new AuditEntity { Id = Randomizer.Next(), Date = auditDate, ComponentId = id }).ToArray();
            dbMock.Setup(db => db.GetAudits(auditDate)).ReturnsAsync(audits).Verifiable();

            foreach (var audit in audits)
            {
                var summary = new CountersSummary
                {
                    Failed = Randomizer.Next(0, 100),
                    Warning = Randomizer.Next(0, 100),
                    NoData = Randomizer.Next(0, 100),
                    Passed = Randomizer.Next(0, 100),
                };
                expectedSummary.Add(summary);
                dbMock.Setup(db => db.GetCounterSummariesForAudit(audit.Id)).ReturnsAsync(summary).Verifiable();
            }

            // Act
            var actualSummary = await cache.GetCountersSummary(Audit.OverallId, auditDate);

            // Assert
            dbMock.Verify();
            actualSummary.NoData.Should().Be(expectedSummary.NoData);
            actualSummary.Warning.Should().Be(expectedSummary.Warning);
            actualSummary.Failed.Should().Be(expectedSummary.Failed);
            actualSummary.Passed.Should().Be(expectedSummary.Passed);
        }

        [TestMethod]
        public async Task RequestingNotCachedDateForOverallReloadsAllComponentsFromDb2()
        {
            // Arrange
            var auditDate = AuditDates.OrderBy(i => i).First().AddDays(-1);

            var audits = ComponentIds.Select(id => new AuditEntity { Id = Randomizer.Next(), Date = auditDate, ComponentId = id }).ToArray();
            dbMock.Setup(db => db.GetAudits(auditDate)).ReturnsAsync(audits).Verifiable();

            foreach (var audit in audits)
            {
                var summary = new CountersSummary
                {
                    Failed = Randomizer.Next(0, 100),
                    Warning = Randomizer.Next(0, 100),
                    NoData = Randomizer.Next(0, 100),
                    Passed = Randomizer.Next(0, 100),
                };
                dbMock.Setup(db => db.GetCounterSummariesForAudit(audit.Id)).ReturnsAsync(summary).Verifiable();
            }

            // reload all components from db and reset mock to throw an exception on any subsequent db-request
            var actualSummary = await cache.GetCountersSummary(Audit.OverallId, auditDate);
            dbMock.Reset();

            // Act & Assert
            foreach (var componentId in ComponentIds)
            {
                await cache.GetCountersSummary(componentId, auditDate);
            }
        }
    }
}
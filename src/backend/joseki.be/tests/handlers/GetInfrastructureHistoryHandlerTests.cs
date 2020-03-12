using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using webapp.Database;
using webapp.Database.Models;
using webapp.Exceptions;
using webapp.Handlers;
using webapp.Models;

namespace tests.handlers
{
    [TestClass]
    public class GetInfrastructureHistoryHandlerTests
    {
        [TestMethod]
        public async Task QueryOverallHistory()
        {
            // Arrange
            const int oneMonthHistoryItemsCount = 31;
            const string componentId = Audit.OverallId;

            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var context = new JosekiDbContext(options);
            var cacheMock = new Mock<IInfrastructureScoreCache>();
            var handler = new GetInfrastructureHistoryHandler(context, cacheMock.Object);

            // Act
            var history = await handler.GetHistory(componentId);

            // Assert
            cacheMock.Verify(i => i.GetCountersSummary(componentId, It.IsAny<DateTime>()), Times.Exactly(oneMonthHistoryItemsCount));
            history.Should().HaveCount(oneMonthHistoryItemsCount);
            history.All(i => i.Component.Category == InfrastructureCategory.Overall && i.Component.Name == Audit.OverallName).Should().BeTrue();
        }

        [DataRow("/k8s/", InfrastructureCategory.Kubernetes)]
        [DataRow("/subscriptions/", InfrastructureCategory.Subscription)]
        [TestMethod]
        public async Task QueryComponentHistoryHappyPath(string componentPrefix, InfrastructureCategory category)
        {
            // Arrange
            const int oneMonthHistoryItemsCount = 31;
            var componentId = $"{componentPrefix}/{Guid.NewGuid().ToString()}";
            var componentName = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var context = new JosekiDbContext(options);
            var cacheMock = new Mock<IInfrastructureScoreCache>();
            var handler = new GetInfrastructureHistoryHandler(context, cacheMock.Object);

            context.Audit.Add(new AuditEntity { ComponentId = componentId, ComponentName = componentName });
            await context.SaveChangesAsync();

            // Act
            var history = await handler.GetHistory(componentId);

            // Assert
            cacheMock.Verify(i => i.GetCountersSummary(componentId, It.IsAny<DateTime>()), Times.Exactly(oneMonthHistoryItemsCount));
            history.Should().HaveCount(oneMonthHistoryItemsCount);
            history.All(i => i.Component.Category == category && i.Component.Name == componentName).Should().BeTrue();
        }

        [TestMethod]
        public async Task QueryComponentHistoryGetLatestComponentName()
        {
            // Arrange
            var componentId = $"/k8s/{Guid.NewGuid().ToString()}";
            var componentName = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var context = new JosekiDbContext(options);
            var cacheMock = new Mock<IInfrastructureScoreCache>();
            var handler = new GetInfrastructureHistoryHandler(context, cacheMock.Object);

            context.Audit.AddRange(
                new AuditEntity { ComponentId = componentId, ComponentName = Guid.NewGuid().ToString(), Date = DateTime.UtcNow.AddHours(-1) },
                new AuditEntity { ComponentId = componentId, ComponentName = componentName, Date = DateTime.UtcNow },
                new AuditEntity { ComponentId = componentId, ComponentName = Guid.NewGuid().ToString(), Date = DateTime.UtcNow.AddDays(-1) },
                new AuditEntity { ComponentId = componentId, ComponentName = Guid.NewGuid().ToString(), Date = DateTime.UtcNow.AddDays(-2) });
            await context.SaveChangesAsync();

            // Act
            var history = await handler.GetHistory(componentId);

            // Assert
            history.All(i => i.Component.Name == componentName).Should().BeTrue();
        }

        [TestMethod]
        public async Task QueryComponentHistorythrowsExceptionIfNoAudits()
        {
            // Arrange
            var componentId = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var context = new JosekiDbContext(options);
            var cacheMock = new Mock<IInfrastructureScoreCache>();
            var handler = new GetInfrastructureHistoryHandler(context, cacheMock.Object);

            // Act & Assert
            await handler
                .Invoking(h => h.GetHistory(componentId))
                .Should()
                .ThrowAsync<AuditNotFoundException>();
        }
    }
}
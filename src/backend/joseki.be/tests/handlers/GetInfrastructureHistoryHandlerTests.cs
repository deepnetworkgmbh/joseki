using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db.entities;

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

            await using var context = JosekiTestsDb.CreateUniqueContext();
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

            await using var context = JosekiTestsDb.CreateUniqueContext();
            var cacheMock = new Mock<IInfrastructureScoreCache>();
            var handler = new GetInfrastructureHistoryHandler(context, cacheMock.Object);

            context.InfrastructureComponent.Add(new InfrastructureComponentEntity { ComponentId = componentId, ComponentName = componentName });
            await context.SaveChangesAsync();

            // Act
            var history = await handler.GetHistory(componentId);

            // Assert
            cacheMock.Verify(i => i.GetCountersSummary(componentId, It.IsAny<DateTime>()), Times.Exactly(oneMonthHistoryItemsCount));
            history.Should().HaveCount(oneMonthHistoryItemsCount);
            history.All(i => i.Component.Category == category && i.Component.Name == componentName).Should().BeTrue();
        }

        [TestMethod]
        public async Task QueryComponentHistoryThrowsExceptionIfNoAudits()
        {
            // Arrange
            var componentId = Guid.NewGuid().ToString();

            await using var context = JosekiTestsDb.CreateUniqueContext();
            var cacheMock = new Mock<IInfrastructureScoreCache>();
            var handler = new GetInfrastructureHistoryHandler(context, cacheMock.Object);

            // Act & Assert
            await handler
                .Invoking(h => h.GetHistory(componentId))
                .Should()
                .ThrowAsync<ComponentNotFoundException>();
        }
    }
}
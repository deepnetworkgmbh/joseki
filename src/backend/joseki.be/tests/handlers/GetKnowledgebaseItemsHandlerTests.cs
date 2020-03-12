using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db.entities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Handlers;
using webapp.Models;

namespace tests.handlers
{
    [TestClass]
    public class GetKnowledgebaseItemsHandlerTests
    {
        [TestMethod]
        public async Task GetAllReturnsNothingForEmptyDatabase()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var handler = new GetKnowledgebaseItemsHandler(context);

            // Act
            var items = await handler.GetAll();

            // Assert
            items.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAllReturnsAllItemsFromTheDatabase()
        {
            // Arrange
            var itemsCount = new Random().Next(5, 10);
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var handler = new GetKnowledgebaseItemsHandler(context);

            var entities = Enumerable
                .Range(1, itemsCount)
                .Select(i => new KnowledgebaseEntity { ItemId = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() });

            // Act
            context.Knowledgebase.AddRange(entities);
            await context.SaveChangesAsync();
            var items = await handler.GetAll();

            // Assert
            context.Knowledgebase.Should().HaveCount(itemsCount);
            items.Should().HaveCount(itemsCount);
        }

        [TestMethod]
        public async Task GetItemsByIdsReturnsOnlyRequestedIds()
        {
            // Arrange
            var itemsCount = new Random().Next(5, 10);
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var handler = new GetKnowledgebaseItemsHandler(context);

            var entities = Enumerable
                .Range(1, itemsCount)
                .Select(i => new KnowledgebaseEntity { ItemId = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() })
                .ToArray();
            context.Knowledgebase.AddRange(entities);
            await context.SaveChangesAsync();

            // Act
            var item1 = entities[1];
            var item3 = entities[3];
            var items = await handler.GetItemsByIds(new[] { item1.ItemId, item3.ItemId });

            // Assert
            items.Should().HaveCount(2);
            items.FirstOrDefault(i => i.Id == item1.ItemId).Should().NotBeNull();
            items.FirstOrDefault(i => i.Id == item3.ItemId).Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetItemByIdReturnsCorrectEntry()
        {
            // Arrange
            var itemsCount = new Random().Next(5, 10);
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var handler = new GetKnowledgebaseItemsHandler(context);

            var entities = Enumerable
                .Range(1, itemsCount)
                .Select(i => new KnowledgebaseEntity { ItemId = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() })
                .ToArray();
            context.Knowledgebase.AddRange(entities);
            await context.SaveChangesAsync();

            // Act
            var expectedItem = entities[2];
            var actualItem = await handler.GetItemById(expectedItem.ItemId);

            // Assert
            actualItem.Id.Should().Be(expectedItem.ItemId);
            actualItem.Content.Should().Be(expectedItem.Content);
        }

        [TestMethod]
        public async Task GetItemByIdReturnsNotFoundRecordForWrongId()
        {
            // Arrange
            var itemsCount = new Random().Next(5, 10);
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var handler = new GetKnowledgebaseItemsHandler(context);

            var entities = Enumerable
                .Range(1, itemsCount)
                .Select(i => new KnowledgebaseEntity { ItemId = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() })
                .ToArray();
            context.Knowledgebase.AddRange(entities);
            await context.SaveChangesAsync();

            // Act
            var item = await handler.GetItemById(Guid.NewGuid().ToString());

            // Assert
            item.Should().Be(KnowledgebaseItem.NotFound);
        }
    }
}
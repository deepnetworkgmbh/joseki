using System;
using System.IO;
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
        private static readonly string BaseTestPath = "../../../testfiles/";

        [TestMethod]
        public async Task GetAllReturnsNothingForEmptyDatabase()
        {
            // Prepare
            var (handler, path) = await this.getUniqueHandlerAsync();

            try
            {
                // Act
                var items = await handler.GetAll();

                // Assert
                items.Should().BeEmpty();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                // Cleanup
                this.cleanupHandlerFolder(path);
            }
        }

        [TestMethod]
        public async Task GetAllReturnsAllItemsFromTheDatabase()
        {
            // Prepare
            var (handler, path) = await this.getUniqueHandlerAsync();

            try
            {
                // Arrange
                var itemsCount = new Random().Next(5, 10);

                var entities = Enumerable
                    .Range(1, itemsCount)
                    .Select(i => new KnowledgebaseItem { Id = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() });

                // Act
                foreach (var item in entities)
                {
                    await handler.AddItem(item);
                }

                // Assert
                var items = await handler.GetAll();
                items.Should().HaveCount(itemsCount);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                // Cleanup
                this.cleanupHandlerFolder(path);
            }
        }

        [TestMethod]
        public async Task GetItemsByIdsReturnsOnlyRequestedIds()
        {
            // Prepare
            var (handler, path) = await this.getUniqueHandlerAsync();

            try
            {
                // Arrange
                var itemsCount = new Random().Next(5, 10);

                var entities = Enumerable
                    .Range(1, itemsCount)
                    .Select(i => new KnowledgebaseItem { Id = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() })
                    .ToArray();

                foreach (var item in entities)
                {
                    await handler.AddItem(item);
                }

                // Act
                var item1 = entities[1];
                var item3 = entities[3];
                var items = await handler.GetItemsByIds(new[] { item1.Id, item3.Id });

                // Assert
                items.Should().HaveCount(2);
                items.FirstOrDefault(i => i.Id == item1.Id).Should().NotBeNull();
                items.FirstOrDefault(i => i.Id == item3.Id).Should().NotBeNull();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                // Cleanup
                this.cleanupHandlerFolder(path);
            }
        }

        [TestMethod]
        public async Task GetItemByIdReturnsCorrectEntry()
        {
            // Prepare
            var (handler, path) = await this.getUniqueHandlerAsync();

            try
            {
                // Arrange
                var itemsCount = new Random().Next(5, 10);

                var entities = Enumerable
                    .Range(1, itemsCount)
                    .Select(i => new KnowledgebaseItem { Id = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() })
                    .ToArray();

                foreach (var item in entities)
                {
                    await handler.AddItem(item);
                }

                // Act
                var expectedItem = entities[2];
                var actualItem = await handler.GetItemById(expectedItem.Id);

                // Assert
                actualItem.Id.Should().Be(expectedItem.Id);
                actualItem.Content.Should().Be(expectedItem.Content);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                // Cleanup
                this.cleanupHandlerFolder(path);
            }
        }

        [TestMethod]
        public async Task GetItemByIdReturnsNotFoundRecordForWrongId()
        {
            // Prepare
            var (handler, path) = await this.getUniqueHandlerAsync();

            try
            {
                // Arrange
                var itemsCount = new Random().Next(5, 10);

                var entities = Enumerable
                    .Range(1, itemsCount)
                    .Select(i => new KnowledgebaseItem { Id = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() })
                    .ToArray();

                foreach (var testitem in entities)
                {
                    await handler.AddItem(testitem);
                }

                // Act
                var item = await handler.GetItemById(Guid.NewGuid().ToString());

                // Assert
                item.Should().Be(KnowledgebaseItem.NotFound);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                // Cleanup
                this.cleanupHandlerFolder(path);
            }
        }

        [TestMethod]
        public async Task GetMetadataItemsReturnsOnlyMetadataRecords()
        {
            // Prepare
            var (handler, path) = await this.getUniqueHandlerAsync();

            try
            {
                // Arrange
                var itemsCount = new Random().Next(5, 10);

                var expectedItems = new[]
                {
                    new KnowledgebaseItem { Id = $"metadata.{Guid.NewGuid()}", Content = Guid.NewGuid().ToString() },
                    new KnowledgebaseItem { Id = $"metadata.{Guid.NewGuid()}", Content = Guid.NewGuid().ToString() },
                };
                var entities = Enumerable
                    .Range(1, itemsCount)
                    .Select(i => new KnowledgebaseItem { Id = Guid.NewGuid().ToString(), Content = Guid.NewGuid().ToString() })
                    .ToList();

                foreach (var testitem in entities.Concat(expectedItems))
                {
                    await handler.AddItem(testitem);
                }

                // Act
                var actualItems = await handler.GetMetadataItems();

                // Assert
                actualItems.Should().HaveCount(expectedItems.Length);
                foreach (var actualItem in actualItems)
                {
                    var expectedItem = expectedItems.First(i => i.Id == actualItem.Id);
                    actualItem.Content.Should().Be(expectedItem.Content);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                // Cleanup
                this.cleanupHandlerFolder(path);
            }
        }

        private async Task<(GetKnowledgebaseItemsHandler, string)> getUniqueHandlerAsync()
        {
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var path = Path.Combine(BaseTestPath, Guid.NewGuid().ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var handler = new GetKnowledgebaseItemsHandler(context, path);
            return (handler, path);
        }

        private void cleanupHandlerFolder(string handlerRootPath)
        {
            if (Directory.Exists(handlerRootPath))
            {
                Directory.Delete(handlerRootPath, true);
            }
        }
    }
}
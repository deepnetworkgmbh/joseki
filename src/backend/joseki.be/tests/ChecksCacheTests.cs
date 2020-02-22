using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Configuration;
using webapp.Database;
using webapp.Database.Models;

namespace tests
{
    [TestClass]
    public class ChecksCacheTests
    {
        [TestMethod]
        public async Task GetNotExistingItemAddOneRecordToDb()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var parser = new ConfigurationParser("config.sample.yaml");
            var checksCache = new ChecksCache(parser, context);

            var id = $"azsk.{Guid.NewGuid().ToString()}";
            var check = new Check { Id = id, Category = Guid.NewGuid().ToString(), Description = Guid.NewGuid().ToString(), Severity = CheckSeverity.High };

            // Act & Assert
            context.Check.Count().Should().Be(0, "context should be empty before GetOrAddItem");
            await checksCache.GetOrAddItem(id, () => check);
            context.Check.Count().Should().Be(1, "context should have a single value after GetOrAddItem");
        }

        [TestMethod]
        public async Task SeveralGetRequestsAddOnlySingleRecordToDb()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var parser = new ConfigurationParser("config.sample.yaml");
            var checksCache = new ChecksCache(parser, context);

            var id = $"azsk.{Guid.NewGuid().ToString()}";
            var check = new Check { Id = id, Category = Guid.NewGuid().ToString(), Description = Guid.NewGuid().ToString(), Severity = CheckSeverity.High };

            // Act & Assert
            context.Check.Count().Should().Be(0, "context should be empty before GetOrAddItem");
            await checksCache.GetOrAddItem(id, () => check);
            await checksCache.GetOrAddItem(id, () => check);
            await checksCache.GetOrAddItem(id, () => check);
            context.Check.Count().Should().Be(1, "context should have a single value after GetOrAddItem");
        }
    }
}
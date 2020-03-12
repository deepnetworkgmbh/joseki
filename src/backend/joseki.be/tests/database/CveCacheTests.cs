using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Configuration;
using webapp.Database;
using webapp.Database.Models;

using CveSeverity = webapp.Database.Models.CveSeverity;

namespace tests.database
{
    [TestClass]
    public class CveCacheTests
    {
        private readonly Random randomizer = new Random();

        [TestMethod]
        public async Task GetNotExistingItemAddOneRecordToDb()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var parser = new ConfigurationParser("config.sample.yaml");
            var cveCache = new CveCache(parser, context);

            var id = this.GetCveId();
            var cve = new CVE { Id = id, Description = Guid.NewGuid().ToString(), };

            // Act & Assert
            context.Cve.Count().Should().Be(0, "context should be empty before GetOrAddItem");
            await cveCache.GetOrAddItem(id, () => cve);
            context.Cve.Count().Should().Be(1, "context should have a single value after GetOrAddItem");
        }

        [TestMethod]
        public async Task GetExistingItemDoesNotAddNewRecords()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var parser = new ConfigurationParser("config.sample.yaml");
            var cveCache = new CveCache(parser, context);

            var id = this.GetCveId();
            var cve = new CVE { Id = id, Description = Guid.NewGuid().ToString(), };
            context.Cve.Add(cve.ToEntity());
            await context.SaveChangesAsync();

            // Act & Assert
            context.Cve.Count().Should().Be(1, "context should have the only one record before GetOrAddItem");
            await cveCache.GetOrAddItem(id, () => cve);
            context.Cve.Count().Should().Be(1, "context should still have the only one record after GetOrAddItem");
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
            var cveCache = new CveCache(parser, context);

            var id = this.GetCveId();
            var cve = new CVE { Id = id, Description = Guid.NewGuid().ToString(), };

            // Act & Assert
            context.Cve.Count().Should().Be(0, "context should be empty before the first GetOrAddItem");
            await cveCache.GetOrAddItem(id, () => cve);
            await cveCache.GetOrAddItem(id, () => cve);
            await cveCache.GetOrAddItem(id, () => cve);
            context.Cve.Count().Should().Be(1, "context should have a single value after three GetOrAddItem");
        }

        [TestMethod]
        public async Task ExpiredThresholdCausesRecordUpdate()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new JosekiDbContext(options);
            var parser = new ConfigurationParser("config.sample.yaml");
            var cveCache = new CveCache(parser, context);

            var id = this.GetCveId();
            var now = DateTime.UtcNow;
            var expirationDate = now.AddDays(-(parser.Get().Cache.CveTtl + 1));
            var oldCve = new CveEntity
            {
                CveId = id,
                Severity = joseki.db.entities.CveSeverity.Medium,
                PackageName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Remediation = Guid.NewGuid().ToString(),
                References = Guid.NewGuid().ToString(),
                DateUpdated = expirationDate,
                DateCreated = expirationDate,
            };

            // this is the hack -_-
            // Use sync version, because it does not update DateUpdated & DateCreated
            context.Cve.Add(oldCve);
            context.SaveChanges();

            var newCve = new CVE
            {
                Id = id,
                Description = Guid.NewGuid().ToString(),
                Remediation = Guid.NewGuid().ToString(),
                Severity = CveSeverity.High,
                PackageName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString(),
                References = Guid.NewGuid().ToString(),
            };

            // Act & Assert
            context.Cve.Count().Should().Be(1, "context should have the only one record before GetOrAddItem");
            await cveCache.GetOrAddItem(id, () => newCve);

            var actualEntity = await context.Cve.FirstAsync(i => i.CveId == id);
            actualEntity.Description.Should().Be(newCve.Description);
            actualEntity.Remediation.Should().Be(newCve.Remediation);
            actualEntity.PackageName.Should().Be(newCve.PackageName);
            actualEntity.Title.Should().Be(newCve.Title);
            actualEntity.References.Should().Be(newCve.References);
            actualEntity.Severity.Should().Be(joseki.db.entities.CveSeverity.High);
            actualEntity.DateUpdated.Should().BeOnOrAfter(now);
        }

        private string GetCveId()
        {
            return $"CVE-{DateTime.UtcNow.Year}-{this.randomizer.Next(10000)}";
        }
    }
}
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using webapp.Database.Cache;
using webapp.Database.Models;
using webapp.Handlers;

#pragma warning disable SA1601
namespace tests.ownership
{
    [TestClass]
    public partial class OwnershipTests
    {
        [TestMethod]
        public async Task OwnershipCache_ShouldInvalidateOnCall()
        {
            await using var context = JosekiTestsDb.CreateUniqueContext();

            const string rootLevelId = "/subscriptions/0000-000-000-0000";

            var ownerEntry = new OwnershipEntity
            {
                ComponentId = rootLevelId,
                Owner = "root@test.com",
            };

            context.Ownership.Add(ownerEntry);
            await context.SaveChangesAsync();

            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));

            var owner1 = await ownershipCache.GetOwner(rootLevelId);
            owner1.Should().Be("root@test.com");

            // remove entry from db
            context.Ownership.Remove(ownerEntry);
            await context.SaveChangesAsync();

            // see that cache is in progress.
            var ownerAgain = await ownershipCache.GetOwner(rootLevelId);
            ownerAgain.Should().Be("root@test.com");

            // invalidate cache
            ownershipCache.Invalidate();

            var ownerNull = await ownershipCache.GetOwner(rootLevelId);
            ownerNull.Should().Be(string.Empty);
        }
    }
}
#pragma warning restore SA1601

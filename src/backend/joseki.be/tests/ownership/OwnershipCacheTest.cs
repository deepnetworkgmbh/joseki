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

            context.Ownership.AddRange(new OwnershipEntity[]
            {
                new OwnershipEntity
                {
                    ComponentId = rootLevelId,
                    Owner = "root@test.com",
                },
            });

            await context.SaveChangesAsync();

            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));

            var owner1 = await ownershipCache.GetOwner(rootLevelId);
            owner1.Should().Be("root@test.com");
        }
    }
}
#pragma warning restore SA1601

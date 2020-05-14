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
    // [TestClass]
    public partial class OwnershipTests
    {
        [TestMethod]
        public async Task TestCascadedOwnership_All3Groups()
        {
            await using var context = JosekiTestsDb.CreateUniqueContext();

            const string rootLevelId = "/subscriptions/0000-000-000-0000";
            const string groupLevelId = "/subscriptions/0000-000-000-0000/resource_group/das-rg";
            const string objectLevelId = "/subscriptions/0000-000-000-0000/resource_group/das-rg/VirtualNetwork/das-vn";

            context.Ownership.AddRange(new OwnershipEntity[]
            {
                new OwnershipEntity
                {
                    ComponentId = rootLevelId,
                    Owner = "root@test.com",
                },
                new OwnershipEntity
                {
                    ComponentId = groupLevelId,
                    Owner = "group-owner@test.com",
                },
                new OwnershipEntity
                {
                    ComponentId = objectLevelId,
                    Owner = "object-owner@test.com",
                },
            });
            await context.SaveChangesAsync();

            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));

            var owner1 = await ownershipCache.GetOwner(rootLevelId);
            owner1.Should().Be("root@test.com");

            var owner2 = await ownershipCache.GetOwner(groupLevelId);
            owner2.Should().Be("group-owner@test.com");

            var owner3 = await ownershipCache.GetOwner(objectLevelId);
            owner3.Should().Be("object-owner@test.com");
        }

        [TestMethod]
        public async Task TestCascadedOwnership_BlankObjectOwnerShouldReturnGroupLevel()
        {
            await using var context = JosekiTestsDb.CreateUniqueContext();

            const string rootLevelId = "/subscriptions/0000-000-000-0000";
            const string groupLevelId = "/subscriptions/0000-000-000-0000/resource_group/das-rg";
            const string objectLevelId = "/subscriptions/0000-000-000-0000/resource_group/das-rg/VirtualNetwork/das-vn";

            context.Ownership.AddRange(new OwnershipEntity[]
            {
                new OwnershipEntity
                {
                    ComponentId = rootLevelId,
                    Owner = "root@test.com",
                },
                new OwnershipEntity
                {
                    ComponentId = groupLevelId,
                    Owner = "group-owner@test.com",
                },
                new OwnershipEntity
                {
                    ComponentId = objectLevelId,
                    Owner = string.Empty,
                },
            });
            await context.SaveChangesAsync();

            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));

            var owner = await ownershipCache.GetOwner(objectLevelId);
            owner.Should().Be("group-owner@test.com");
        }

        [TestMethod]
        public async Task TestCascadedOwnership_BlankObjectAndGroupOwnerShouldReturnRootLevel()
        {
            await using var context = JosekiTestsDb.CreateUniqueContext();

            const string rootLevelId = "/subscriptions/0000-000-000-0000";
            const string groupLevelId = "/subscriptions/0000-000-000-0000/resource_group/das-rg";
            const string objectLevelId = "/subscriptions/0000-000-000-0000/resource_group/das-rg/VirtualNetwork/das-vn";

            context.Ownership.AddRange(new OwnershipEntity[]
            {
                new OwnershipEntity
                {
                    ComponentId = rootLevelId,
                    Owner = "root@test.com",
                },
                new OwnershipEntity
                {
                    ComponentId = groupLevelId,
                    Owner = string.Empty,
                },
                new OwnershipEntity
                {
                    ComponentId = objectLevelId,
                    Owner = string.Empty,
                },
            });
            await context.SaveChangesAsync();

            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));

            var owner = await ownershipCache.GetOwner(objectLevelId);
            owner.Should().Be("root@test.com");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Invalid identifier")]
        public async Task InvalidComponentId_ShouldThrowException()
        {
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));

            const string invalidId_missingSlashPrefix = "subscription/0000-000-000-0000/";

            var owner1 = await ownershipCache.GetOwner(invalidId_missingSlashPrefix);
            owner1.Should().Be(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Identifier empty")]
        public async Task EmptyComponentId_ShouldThrowException()
        {
            await using var context = JosekiTestsDb.CreateUniqueContext();
            var ownershipCache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));

            string emptyComponentId = string.Empty;

            var owner1 = await ownershipCache.GetOwner(emptyComponentId);
            owner1.Should().Be(string.Empty);
        }
    }
}
#pragma warning restore SA1601

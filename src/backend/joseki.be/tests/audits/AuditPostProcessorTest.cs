using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using webapp.Audits.PostProcessors;
using webapp.Database.Cache;
using webapp.Database.Models;

namespace tests.audits
{
    [TestClass]
    public class AuditPostProcessorTest
    {
        [TestMethod]
        public async Task ExtractOwnershipFromAzskAudit()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            context.Ownership.Count().Should().Be(0, "ownership list should be empty");

            var cache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));
            var processor = new ExtractOwnershipProcessor(context, cache);
            var auditMetaPath = Path.Combine("audits", "samples", "azsk_audit.json");

            var audit = new Audit
            {
                MetadataAzure = new MetadataAzure
                {
                    Id = 1,
                    AuditId = string.Empty,
                    Date = DateTime.Now,
                    JSON = File.ReadAllText(auditMetaPath),
                },
            };

            await processor.Process(audit, CancellationToken.None);

            var list = context.Ownership.ToList();

            context.Ownership.Count().Should().Be(17, "ownership list should have 17 items");

            foreach (var expectedComponentId in azskAuditComponentIds)
            {
                var item = context.Ownership.FirstOrDefault(x => x.ComponentId == expectedComponentId);
                item.Should().NotBe(null, expectedComponentId);
            }
        }

        private static List<string> azskAuditComponentIds = new List<string>
        {
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/cs-storage-rg/Storage/0000000-storage-00000000",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/ws-default-rg/LogAnalytics/DefaultWorkspace-00000000-0000-0000-0000-000000000000-WEU",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/infra-rg/KeyVault/testkv",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/infra-rg/Storage/teststorage",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/dev01Dev/Storage/dev01devdiag",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/dev01Dev/VirtualMachine/dev01DevVm",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/dev01Dev/VirtualMachine/dev01Lin",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/dev01Dev/VirtualNetwork/dev01Dev-vnet",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/app1dev/VirtualMachine/dev",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/app1dev/VirtualNetwork/app1dev-vnet",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/someone-rg/Storage/someoneshare",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/somecorp/KeyVault/haubuskv",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/rg-lolfood/ContainerRegistry/lolfoodacr",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/rg-lolfood/KubernetesService/aks-lolfood",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/rg-lolfood/Storage/lolfoodpromsnapshots",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/rg-lolvpn/VirtualMachine/lolvpn",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resource_group/rg-lolvpn/VirtualNetwork/lolvpn",
        };

        [TestMethod]
        public async Task ExtractOwnershipFromK8sAudit()
        {
            // Arrange
            await using var context = JosekiTestsDb.CreateUniqueContext();

            var cache = new OwnershipCache(context, new MemoryCache(new MemoryCacheOptions()));
            var processor = new ExtractOwnershipProcessor(context, cache);

            context.Ownership.Count().Should().Be(0, "ownership list should be empty");

            var auditMetaPath = Path.Combine("audits", "samples", "k8s_audit.json");

            var audit = new Audit
            {
                MetadataKube = new MetadataKube
                {
                    Id = 1,
                    AuditId = string.Empty,
                    Date = DateTime.Now,
                    JSON = File.ReadAllText(auditMetaPath),
                },
            };

            await processor.Process(audit, CancellationToken.None);

            var list = context.Ownership.ToList();

            context.Ownership.Count().Should().Be(40, "ownership list should have 40 items");
        }
    }
}

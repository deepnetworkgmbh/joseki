using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using joseki.db;
using Newtonsoft.Json;
using Serilog;
using webapp.Database;
using webapp.Database.Models;

namespace webapp.Audits.PostProcessors
{
    /// <summary>
    /// Post-processor for Audit to extract ownership information after created.
    /// </summary>
    public class ExtractOwnershipProcessor : IAuditPostProcessor
    {
        private static readonly ILogger Logger = Log.ForContext<ExtractOwnershipProcessor>();

        private readonly JosekiDbContext db;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractOwnershipProcessor"/> class.
        /// </summary>
        /// <param name="db">Joseki database implementation.</param>
        public ExtractOwnershipProcessor(JosekiDbContext db)
        {
            this.db = db;
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
        }

        /// <summary>
        /// Process the audit, extract ownership data and update db context.
        /// </summary>
        /// <param name="audit">Audit to be processed.</param>
        /// <param name="token">A signal to stop processing.</param>
        public Task Process(Audit audit, CancellationToken token)
        {
            var newOwnerships = new List<OwnershipEntity>();

            if (audit.MetadataKube != null)
            {
                newOwnerships = this.ExtractOwnershipFromK8sAudit(audit.MetadataAzure);
            }

            if (audit.MetadataAzure != null)
            {
                newOwnerships = this.ExtractOwnershipFromAzskAudit(audit.MetadataAzure);
            }

            foreach (var ownership in newOwnerships)
            {
                this.db.Ownership.Add(ownership);
            }

            this.db.SaveChanges();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Extract ownership data from metadata JSON.
        /// </summary>
        /// <param name="metadata">Azsk scan metadata.</param>
        /// <returns>list of new ownerships.</returns>
        private List<OwnershipEntity> ExtractOwnershipFromAzskAudit(MetadataAzure metadata)
        {
            var ownershipList = new List<OwnershipEntity>();

            dynamic json = JsonConvert.DeserializeObject(metadata.JSON, this.jsonSerializerSettings);

            var resources = json.subscription.resources;

            foreach (var resource in resources)
            {
                if (resource == null)
                {
                    // skip first null
                    continue;
                }

                Resource resourceObj = JsonConvert.DeserializeObject<Resource>(resource.ToString());

                ownershipList.Add(new OwnershipEntity()
                {
                    Owner = resourceObj.GetOwner(),
                    ComponentId = resourceObj.GetSimplifiedId(),
                });
            }

            return ownershipList;
        }

        private List<OwnershipEntity> ExtractOwnershipFromK8sAudit(MetadataAzure metadata)
        {
            Console.WriteLine("Extracting ownership from MetadataKube");
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class representation of an audit scan metadata.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// ResourceDetails segment of the audit.
        /// </summary>
        public ResourceDetails ResourceDetails { get; set; }

        /// <summary>
        /// ResourceTypeName segment of the audit.
        /// </summary>
        public string ResourceTypeName { get; set; }

        /// <summary>
        /// Returns simplified ResourceId by replacing the
        /// ResourceDetails.Type with ResourceTypeName
        /// e.g Microsoft.ContainerService/managedClusters => KubernetesService.
        /// </summary>
        public string GetSimplifiedId()
        {
            return this.ResourceDetails.Id
                       .Replace("/resourceGroups/", "/resource_group/")
                       .Replace($"/providers/{this.ResourceDetails.Type}/", $"/{this.ResourceTypeName}/");
        }

        /// <summary>
        /// Returns the owner of the resource, if defined.
        /// </summary>
        public string GetOwner()
        {
            return this.ResourceDetails?.Tags?.owner?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Class representation of ResourceDetails in audit.
    /// </summary>
    public class ResourceDetails
    {
        /// <summary>
        /// Id of the resource.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of the resource.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Tags for the resource.
        /// </summary>
        public ResourceTag Tags { get; set; }
    }

    /// <summary>
    /// Class representation of ResourceTag in audit.
    /// </summary>
    public class ResourceTag
    {
        /// <summary>
        /// Owner of the resource.
        /// </summary>
        public string owner { get; set; }

        /// <summary>
        /// Owner change date of the resource.
        /// </summary>
        public string changedate { get; set; }
    }
}

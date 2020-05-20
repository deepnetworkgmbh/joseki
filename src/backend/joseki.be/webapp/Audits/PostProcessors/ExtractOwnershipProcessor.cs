using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using joseki.db;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using webapp.Database;
using webapp.Database.Cache;
using webapp.Database.Models;

namespace webapp.Audits.PostProcessors
{
#pragma warning disable SA1124
    /// <summary>
    /// Post-processor for Audit to extract ownership information after created.
    /// </summary>
    public class ExtractOwnershipProcessor : IAuditPostProcessor
    {
        private static readonly ILogger Logger = Log.ForContext<ExtractOwnershipProcessor>();

        private readonly JosekiDbContext db;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly IOwnershipCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractOwnershipProcessor"/> class.
        /// </summary>
        /// <param name="db">Joseki database implementation.</param>
        /// <param name="cache">Ownership in memory cache.</param>
        public ExtractOwnershipProcessor(JosekiDbContext db, IOwnershipCache cache)
        {
            this.db = db;
            this.cache = cache;
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
        public async Task Process(Audit audit, CancellationToken token)
        {
            var newOwnerships = new List<OwnershipInfo>();

            if (audit.MetadataKube != null)
            {
                newOwnerships = this.ExtractOwnershipFromK8sAudit(audit.MetadataKube, token);
            }

            if (audit.MetadataAzure != null)
            {
                newOwnerships = this.ExtractOwnershipFromAzskAudit(audit.MetadataAzure, token);
            }

            if (newOwnerships.Count == 0)
            {
                return;
            }

            foreach (var ownershipInfo in newOwnerships)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var existingOwnership = await this.db.Ownership.FirstOrDefaultAsync(x => x.ComponentId == ownershipInfo.ComponentId);
                if (existingOwnership != null)
                {
                    existingOwnership.Owner = ownershipInfo.Owner;
                    existingOwnership.DateUpdated = ownershipInfo.ChangeDate;
                }
                else
                {
                    // do not create new component with no owner
                    if (!string.IsNullOrEmpty(ownershipInfo.Owner))
                    {
                        await this.db.Ownership.AddAsync(new OwnershipEntity
                        {
                            Owner = ownershipInfo.Owner,
                            ComponentId = ownershipInfo.ComponentId,
                        });
                    }
                }
            }

            await this.db.SaveChangesAsync();

            // clear current ownership cache (will trigger reload)
            this.cache.Invalidate();
        }

        /// <summary>
        /// Extract ownership data from metadata JSON.
        /// </summary>
        /// <param name="metadata">Azsk scan metadata.</param>
        /// <param name="token">A signal to stop processing.</param>
        /// <returns>list of new ownerships.</returns>
        private List<OwnershipInfo> ExtractOwnershipFromAzskAudit(MetadataAzure metadata, CancellationToken token)
        {
            var ownershipList = new List<OwnershipInfo>();

            dynamic json = JsonConvert.DeserializeObject(metadata.JSON, this.jsonSerializerSettings);

            var resources = json.subscription.resources;

            foreach (var resource in resources)
            {
                if (token.IsCancellationRequested)
                {
                    return new List<OwnershipInfo>();
                }

                if (resource == null)
                {
                    continue;
                }

                Resource resourceObj = JsonConvert.DeserializeObject<Resource>(resource.ToString());
                ownershipList.Add(new OwnershipInfo
                {
                    Owner = resourceObj.Owner,
                    ComponentId = resourceObj.ResourceId,
                    ChangeDate = resourceObj.OwnerChangeDate,
                });
            }

            return ownershipList;
        }

        /// <summary>
        /// Extract ownership data from metadata JSON.
        /// </summary>
        /// <param name="metadata">Kubernetes scan metadata.</param>
        /// <param name="token">A signal to stop processing.</param>
        /// <returns>list of new ownerships.</returns>
        private List<OwnershipInfo> ExtractOwnershipFromK8sAudit(MetadataKube metadata, CancellationToken token)
        {
            var ownershipList = new List<OwnershipInfo>();

            dynamic json = JsonConvert.DeserializeObject(metadata.JSON, this.jsonSerializerSettings);

            var clusterId = json["audit"]["cluster-id"];

            // we're interested in Namespaces, Deployments, StatefulSets, DaemonSets, Jobs, CronJobs
            #region Namespaces
            foreach (var nsobj in json["cluster"]["Namespaces"])
            {
                metadata meta = JsonConvert.DeserializeObject<metadata>(nsobj.metadata.ToString());
                ownershipList.Add(new OwnershipInfo
                {
                    Owner = meta.Owner,
                    ComponentId = $"/k8s/{clusterId}/ns/{meta.name}",
                });
            }
            #endregion

            #region Deployments
            foreach (var nsobj in json["cluster"]["Deployments"])
            {
                metadata meta = JsonConvert.DeserializeObject<metadata>(nsobj.metadata.ToString());
                ownershipList.Add(new OwnershipInfo
                {
                    Owner = meta.Owner,
                    ComponentId = $"/k8s/{clusterId}/ns/{meta._namespace}/deployment/{meta.name}",
                });
            }
            #endregion

            #region StatefulSets
            foreach (var nsobj in json["cluster"]["StatefulSets"])
            {
                metadata meta = JsonConvert.DeserializeObject<metadata>(nsobj.metadata.ToString());
                ownershipList.Add(new OwnershipInfo
                {
                    Owner = meta.Owner,
                    ComponentId = $"/k8s/{clusterId}/ns/{meta._namespace}/statefulset/{meta.name}",
                });
            }
            #endregion

            #region DaemonSets
            foreach (var nsobj in json["cluster"]["DaemonSets"])
            {
                metadata meta = JsonConvert.DeserializeObject<metadata>(nsobj.metadata.ToString());
                ownershipList.Add(new OwnershipInfo
                {
                    Owner = meta.Owner,
                    ComponentId = $"/k8s/{clusterId}/ns/{meta._namespace}/daemonset/{meta.name}",
                });
            }
            #endregion

            #region Jobs
            foreach (var nsobj in json["cluster"]["Jobs"])
            {
                metadata meta = JsonConvert.DeserializeObject<metadata>(nsobj.metadata.ToString());
                ownershipList.Add(new OwnershipInfo
                {
                    Owner = meta.Owner,
                    ComponentId = $"/k8s/{clusterId}/ns/{meta._namespace}/job/{meta.name}",
                });
            }
            #endregion

            #region CronJobs
            foreach (var nsobj in json["cluster"]["CronJobs"])
            {
                metadata meta = JsonConvert.DeserializeObject<metadata>(nsobj.metadata.ToString());
                ownershipList.Add(new OwnershipInfo
                {
                    Owner = meta.Owner,
                    ComponentId = $"/k8s/{clusterId}/ns/{meta._namespace}/cronjob/{meta.name}",
                });
            }
            #endregion

            return ownershipList;
        }
    }

    #region azsk-serialization

    /// <summary>
    /// Class hosting ownership info.
    /// </summary>
    public class OwnershipInfo
    {
        /// <summary>
        /// Name of the owner.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Component Id of ownership.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Change date of ownership.
        /// </summary>
        public DateTime ChangeDate { get; set; }
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
        public string ResourceId => this.ResourceDetails.Id
                       .Replace("/resourceGroups/", "/resource_group/")
                       .Replace($"/providers/{this.ResourceDetails.Type}/", $"/{this.ResourceTypeName}/");

        /// <summary>
        /// Returns the owner of the resource, if defined.
        /// </summary>
        public string Owner => this.ResourceDetails?.Tags?.owner?.ToString() ?? string.Empty;

        /// <summary>
        /// Returns the change date of the owner, if defined.
        /// </summary>
        public DateTime OwnerChangeDate => this.ResourceDetails?.Tags?.changedate ?? DateTime.Now;
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
        public DateTime changedate { get; set; }
    }

    #endregion

    #region k8s-serialization

    /// <summary>
    /// Metadata section of the resource.
    /// </summary>
    public class metadata
    {
        /// <summary>
        /// Name of the resource.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Namespace of the resource.
        /// </summary>
        [JsonProperty("namespace")]
        public string _namespace { get; set; }

        /// <summary>
        /// Labels of the resource.
        /// </summary>
        public labels labels { get; set; }

        /// <summary>
        /// Returns owner of resource, if any.
        /// </summary>
        public string Owner => this.labels?.owner?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Labels section of metadata.
    /// </summary>
    public class labels
    {
        /// <summary>
        /// Owner of the resource.
        /// </summary>
        public string owner { get; set; }
    }
    #endregion

#pragma warning restore SA1124
}
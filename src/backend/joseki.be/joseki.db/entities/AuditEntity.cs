using System;
using System.Collections.Generic;

namespace joseki.db.entities
{
    /// <summary>
    /// The entity represents a single Audit object.
    /// </summary>
    public class AuditEntity : IJosekiBaseEntity
    {
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public DateTime DateUpdated { get; set; }

        /// <inheritdoc />
        public DateTime DateCreated { get; set; }

        /// <inheritdoc />
        public string ChangedBy { get; set; }

        /// <summary>
        /// External audit identifier.
        /// </summary>
        public string AuditId { get; set; }

        /// <summary>
        /// date when audit was performed.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Which infrastructure component was audited: k8s-cluster id or azure-subscription-id.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Related infrastructure component identifier.
        /// </summary>
        public int InfrastructureComponentId { get; set; }

        /// <summary>
        /// Reference to related infrastructure component.
        /// </summary>
        public InfrastructureComponentEntity InfrastructureComponent { get; set; }

        /// <summary>
        /// List of all check-results, that belongs to the audit.
        /// </summary>
        public List<CheckResultEntity> CheckResults { get; set; }

        /// <summary>
        /// Reference to associated Kubernetes metadata.
        /// The property is null, if audit belongs to azure.
        /// Only one of MetadataAzure and MetadataKube is not null.
        /// </summary>
        public MetadataKubeEntity MetadataKube { get; set; }

        /// <summary>
        /// Reference to associated Azure metadata.
        /// The property is null, if audit belongs to kubernetes.
        /// Only one of MetadataAzure and MetadataKube is not null.
        /// </summary>
        public MetadataAzureEntity MetadataAzure { get; set; }
    }
}
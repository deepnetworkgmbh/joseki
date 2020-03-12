using System;
using System.Collections.Generic;

namespace webapp.Database.Models
{
    /// <summary>
    /// The entity represents a single Audit object.
    /// </summary>
    public class Audit
    {
        /// <summary>
        /// Overall infrastructure id.
        /// </summary>
        public const string OverallId = "/all";

        /// <summary>
        /// Overall infrastructure name.
        /// </summary>
        public const string OverallName = "Overall infrastructure";

        /// <summary>
        /// Unique audit identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// date when audit was performed.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Which scanner did the audit.
        /// TODO: Maybe create a ScannerId type to automatically check scanner types.
        /// </summary>
        public string ScannerId { get; set; }

        /// <summary>
        /// Which infrastructure component was audited: k8s-cluster id or azure-subscription-id.
        /// TODO: Maybe create a ComponentId type to automatically split it to parts and do all the checks.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Human-friendly infrastructure component name.
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// List of all check-results, that belongs to the audit.
        /// </summary>
        public List<CheckResult> CheckResults { get; set; }

        /// <summary>
        /// Reference to associated Kubernetes metadata.
        /// The property is null, if audit belongs to azure.
        /// Only one of MetadataAzure and MetadataKube is not null.
        /// </summary>
        public MetadataKube MetadataKube { get; set; }

        /// <summary>
        /// Reference to associated Azure metadata.
        /// The property is null, if audit belongs to kubernetes.
        /// Only one of MetadataAzure and MetadataKube is not null.
        /// </summary>
        public MetadataAzure MetadataAzure { get; set; }
    }
}
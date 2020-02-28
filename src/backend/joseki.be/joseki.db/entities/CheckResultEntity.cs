using System;

namespace joseki.db.entities
{
    /// <summary>
    /// A single check result object.
    /// </summary>
    public class CheckResultEntity : IJosekiBaseEntity
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
        /// The reference to Check entity.
        /// </summary>
        public int CheckId { get; set; }

        /// <summary>
        /// Unique component identifier in format:
        /// - for azure components: `subscription/{id}/resource_group/{rg_name}/{object_type}/{object_name}`;
        /// - for k8s resources: `k8s/{cluster_id}/namespace/{ns_name}/{object_type}/{object_name}/pod/{pod_name}/container/{container_name}/{image_tag}`,
        /// where all sections after `{object_name}` are optional, as there might be separate checks on object-type, pod, container, or image.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// The reference to Audit entity.
        /// </summary>
        public int AuditId { get; set; }

        /// <summary>
        /// The check result itself.
        /// </summary>
        public CheckValue Value { get; set; }

        /// <summary>
        /// Free-format associated message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Audit object.
        /// </summary>
        public AuditEntity Audit { get; set; }

        /// <summary>
        /// Check object.
        /// </summary>
        public CheckEntity Check { get; set; }
    }

    /// <summary>
    /// Represents the check result value.
    /// </summary>
    public enum CheckValue
    {
        /// <summary>
        /// There is no result due one of the reasons:
        /// - the check failed,
        /// - the check requires a manual step,
        /// - the check result should be verified by end-user.
        /// </summary>
        NoData,

        /// <summary>
        /// The check is still in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// The Component failed to satisfy Check requirements.
        /// </summary>
        Failed,

        /// <summary>
        /// The Component satisfied Check requirements.
        /// </summary>
        Succeeded,
    }
}
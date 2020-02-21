using System;

namespace joseki.db.entities
{
    /// <summary>
    /// Json-serialized kubernetes metadata, actual at a particular audit time.
    /// </summary>
    public class MetadataKubeEntity : IJosekiBaseEntity
    {
        /// <summary>
        /// The record identifier.
        /// </summary>
        public int Id { get; set; }

        /// <inheritdoc />
        public DateTime DateUpdated { get; set; }

        /// <inheritdoc />
        public DateTime DateCreated { get; set; }

        /// <inheritdoc />
        public string ChangedBy { get; set; }

        /// <summary>
        /// Reference to the audit record.
        /// </summary>
        public int AuditId { get; set; }

        /// <summary>
        /// Date and time, when metadata was collected.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Json-encoded representation of kube-metadata.
        /// </summary>
        public string JSON { get; set; }

        /// <summary>
        /// Reference to Audit row.
        /// </summary>
        public AuditEntity Audit { get; set; }
    }
}
using System;

namespace webapp.Database.Models
{
    /// <summary>
    /// Json-serialized azure metadata, actual at a particular audit time.
    /// </summary>
    public class MetadataAzure
    {
        /// <summary>
        /// The record identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Reference to the audit record.
        /// </summary>
        public string AuditId { get; set; }

        /// <summary>
        /// Date and time, when metadata was collected.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Json-encoded representation of azure-metadata.
        /// </summary>
        public string JSON { get; set; }
    }
}
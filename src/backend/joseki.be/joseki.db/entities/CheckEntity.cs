using System;

namespace joseki.db.entities
{
    /// <summary>
    /// Describes in details a single check type.
    /// </summary>
    public class CheckEntity : IJosekiBaseEntity
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
        /// The check identifier in form "{scanner-type}.{scanner-check-id}".
        /// </summary>
        public string CheckId { get; set; }

        /// <summary>
        /// It groups related check types, for example "k8s.security", "azure.storage".
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The severity answers the question: how dangerous the failure of this check could be.
        /// It helps to prioritize check results.
        /// </summary>
        public CheckSeverity Severity { get; set; }

        /// <summary>
        /// Detailed check description: what exactly it verifies and why it's important.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The suggestion how to fix the issue.
        /// </summary>
        public string Remediation { get; set; }
    }

    /// <summary>
    /// Check severities.
    /// </summary>
    public enum CheckSeverity
    {
        /// <summary>
        /// The level is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Top-level issues, that often should be addressed as soon as possible.
        /// </summary>
        Critical,

        /// <summary>
        /// Important issue, which should be carefully reviewed.
        /// </summary>
        High,

        /// <summary>
        /// Medium-level problem, which could cause moderate damage or loss.
        /// </summary>
        Medium,

        /// <summary>
        /// The lowest severity in the rank. Represents only a slight damage.
        /// </summary>
        Low,
    }
}
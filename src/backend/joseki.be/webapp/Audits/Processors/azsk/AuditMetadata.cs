using Newtonsoft.Json;

namespace webapp.Audits.Processors.azsk
{
    /// <summary>
    /// The object is deserialized from Blob Storage container.
    /// </summary>
    public class AuditMetadata
    {
        /// <summary>
        /// Unique audit identifier.
        /// </summary>
        [JsonProperty(PropertyName = "audit-id")]
        public string AuditId { get; set; }

        /// <summary>
        /// Az-sk scanner version.
        /// </summary>
        [JsonProperty(PropertyName = "scanner-version")]
        public string ScannerVersion { get; set; }

        /// <summary>
        /// az-sk scanner periodicity.
        /// </summary>
        [JsonProperty(PropertyName = "periodicity")]
        public string Periodicity { get; set; }

        /// <summary>
        /// The last time, when az-sk scanner performed azure subscriptions audit.
        /// The value is in unix-epoch seconds.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Indicates if audit was successful or not.
        /// Could be one of values: "succeeded", "failed".
        /// </summary>
        [JsonProperty(PropertyName = "audit-result")]
        public string AuditResult { get; set; }

        /// <summary>
        /// Described audit failure reason.
        /// </summary>
        [JsonProperty(PropertyName = "failure-description")]
        public string FailureDescription { get; set; }

        /// <summary>
        /// The version of az-sk tool used to perform the audit.
        /// </summary>
        [JsonProperty(PropertyName = "azsk-version")]
        public string AzSkVersion { get; set; }

        /// <summary>
        /// Path to all audit related files in Blob Storage.
        /// </summary>
        [JsonProperty(PropertyName = "azsk-audit-paths")]
        public string[] AzSkAuditPaths { get; set; }
    }
}
using Newtonsoft.Json;

namespace webapp.Audits.Processors.trivy
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
        /// Full image tag.
        /// </summary>
        [JsonProperty(PropertyName = "image-tag")]
        public string ImageTag { get; set; }

        /// <summary>
        /// trivy scanner version.
        /// </summary>
        [JsonProperty(PropertyName = "scanner-version")]
        public string ScannerVersion { get; set; }

        /// <summary>
        /// The time, when trivy scanner performed image scan.
        /// The value is in unix-epoch seconds.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Indicates if scan was successful or not.
        /// Could be one of values: "succeeded", "upload-failed", "audit-failed".
        /// </summary>
        [JsonProperty(PropertyName = "audit-result")]
        public string AuditResult { get; set; }

        /// <summary>
        /// Described audit failure reason.
        /// </summary>
        [JsonProperty(PropertyName = "failure-description")]
        public string FailureDescription { get; set; }

        /// <summary>
        /// The version of trivy tool used to perform the scan.
        /// </summary>
        [JsonProperty(PropertyName = "trivy-version")]
        public string TrivyVersion { get; set; }

        /// <summary>
        /// Path to trivy audit file in Blob Storage.
        /// </summary>
        [JsonProperty(PropertyName = "trivy-audit-path")]
        public string TrivyAuditPath { get; set; }
    }
}
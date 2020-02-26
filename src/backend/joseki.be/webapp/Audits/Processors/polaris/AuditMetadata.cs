using Newtonsoft.Json;

namespace webapp.Audits.Processors.polaris
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
        /// Unique k8s cluster identifier.
        /// </summary>
        [JsonProperty(PropertyName = "cluster-id")]
        public string ClusterId { get; set; }

        /// <summary>
        /// polaris scanner version.
        /// </summary>
        [JsonProperty(PropertyName = "scanner-version")]
        public string ScannerVersion { get; set; }

        /// <summary>
        /// The last time, when polaris scanner performed kubernetes cluster audit.
        /// The value is in unix-epoch seconds.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Indicates if audit was successful or not.
        /// Could be one of values: "succeeded", "failed".
        /// </summary>
        [JsonProperty(PropertyName = "result")]
        public string AuditResult { get; set; }

        /// <summary>
        /// Described audit failure reason.
        /// </summary>
        [JsonProperty(PropertyName = "failure-description")]
        public string FailureDescription { get; set; }

        /// <summary>
        /// The version of polaris tool used to perform the audit.
        /// </summary>
        [JsonProperty(PropertyName = "polaris-version")]
        public string PolarisVersion { get; set; }

        /// <summary>
        /// Path to polaris audit file in Blob Storage.
        /// </summary>
        [JsonProperty(PropertyName = "polaris-audit-path")]
        public string PolarisAuditPaths { get; set; }

        /// <summary>
        /// Path to kubernetes metadata file in Blob Storage.
        /// </summary>
        [JsonProperty(PropertyName = "k8s-meta-path")]
        public string KubeMetadataPaths { get; set; }
    }
}
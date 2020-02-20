using Newtonsoft.Json;

namespace core.exporters.azure
{
    public class AuditMetadata
    {
        [JsonProperty(PropertyName = "audit-id")]
        public string AuditId { get; set; }

        /// <summary>
        /// Full image tag.
        /// </summary>
        [JsonProperty(PropertyName = "image-tag")]
        public string ImageTag { get; set; }

        [JsonProperty(PropertyName = "scanner-version")]
        public string ScannerVersion { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty(PropertyName = "audit-result")]
        public string AuditResult { get; set; }

        [JsonProperty(PropertyName = "failure-description")]
        public string FailureDescription { get; set; }

        [JsonProperty(PropertyName = "trivy-version")]
        public string TrivyVersion { get; set; }

        [JsonProperty(PropertyName = "trivy-audit-path")]
        public string TrivyAuditPath { get; set; }
    }
}
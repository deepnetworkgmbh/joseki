using Newtonsoft.Json;

namespace core.exporters.azure
{
    public class AuditMetadata
    {
        [JsonProperty(PropertyName = "audit-id")]
        public string AuditId { get; set; }

        [JsonProperty(PropertyName = "scanner-version")]
        public string ScannerVersion { get; set; }

        [JsonProperty(PropertyName = "periodicity")]
        public string Periodicity { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty(PropertyName = "audit-result")]
        public string AuditResult { get; set; }

        [JsonProperty(PropertyName = "failure-description")]
        public string FailureDescription { get; set; }

        [JsonProperty(PropertyName = "azsk-version")]
        public string AzSkVersion { get; set; }

        [JsonProperty(PropertyName = "azsk-audit-paths")]
        public string[] AzSkAuditPaths { get; set; }
    }
}
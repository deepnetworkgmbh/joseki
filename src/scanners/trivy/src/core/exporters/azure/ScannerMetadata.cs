using Newtonsoft.Json;

namespace core.exporters.azure
{
    public class ScannerMetadata
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "trivy";

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "periodicity")]
        public string Periodicity { get; set; } = "on-message";

        [JsonProperty(PropertyName = "heartbeat-periodicity")]
        public int HeartbeatPeriodicity { get; set; }

        [JsonProperty(PropertyName = "heartbeat")]
        public long Heartbeat { get; set; }
    }
}
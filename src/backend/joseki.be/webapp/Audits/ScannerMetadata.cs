using Newtonsoft.Json;

namespace webapp.Audits
{
    /// <summary>
    /// Scanner metadata: identifier, type, heartbeat parameters.
    /// </summary>
    public class ScannerMetadata
    {
        /// <summary>
        /// The scanner type.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public ScannerType Type { get; set; }

        /// <summary>
        /// Unique scanner identifier.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// How often scanner is running. Could be one of two options:
        /// - "on-message";
        /// - "{cron expression}".
        /// </summary>
        [JsonProperty(PropertyName = "periodicity")]
        public string Periodicity { get; set; }

        /// <summary>
        /// How often scanner metadata is updated. Used to catch issues with scanner components or to clean-up stale/abandoned containers.
        /// </summary>
        [JsonProperty(PropertyName = "heartbeat-periodicity")]
        public int HeartbeatPeriodicity { get; set; }

        /// <summary>
        /// Unix epoch seconds value of the last heartbeat.
        /// </summary>
        [JsonProperty(PropertyName = "heartbeat")]
        public long Heartbeat { get; set; }
    }

    /// <summary>
    /// Available scanner types.
    /// </summary>
    public enum ScannerType
    {
        /// <summary>
        /// Unknown scanner type.
        /// </summary>
        None,

        /// <summary>
        /// Represents polaris scanner.
        /// </summary>
        Polaris,

        /// <summary>
        /// Represents trivy scanner.
        /// </summary>
        Trivy,

        /// <summary>
        /// Represents azsk scanner.
        /// </summary>
        Azsk,
    }
}
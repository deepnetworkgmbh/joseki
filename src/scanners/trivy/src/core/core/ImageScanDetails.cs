using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace core.core
{
    public class ImageScanDetails
    {
        public static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public static ImageScanDetails New()
        {
            return new ImageScanDetails
            {
                Id = GenerateId(),
                Timestamp = DateTime.UtcNow,
            };
        }

        public static ImageScanDetails NotFound(ContainerImage image)
        {
            return new ImageScanDetails
            {
                Timestamp = DateTime.UtcNow,
                ScanResult = ScanResult.NotFound,
                Image = image,
            };
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "scannerType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScannerType? ScannerType { get; set; }

        [JsonProperty(PropertyName = "scanResult")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScanResult ScanResult { get; set; }

        [JsonProperty(PropertyName = "payload")]
        public string Payload { get; set; }

        [JsonProperty(PropertyName = "image")]
        public ContainerImage Image { get; set; }

        [JsonProperty(PropertyName = "rank")]
        public ScanRank Rank { get; set; }
    }
}
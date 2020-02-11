using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace core.core
{
    public class SubscriptionScanDetails
    {
        public static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public static SubscriptionScanDetails New()
        {
            return new SubscriptionScanDetails
            {
                Id = GenerateId(),
                Timestamp = DateTime.UtcNow,
            };
        }

        public static SubscriptionScanDetails NotFound(string subscription)
        {
            return new SubscriptionScanDetails
            {
                Timestamp = DateTime.UtcNow,
                ScanResult = ScanResult.NotFound,
                Subscription = subscription,
            };
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "scanResult")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScanResult ScanResult { get; set; }

        [JsonProperty(PropertyName = "resultFiles")]
        public List<ResultFile> ResultFiles { get; set; }

        [JsonProperty(PropertyName = "subscription")]
        public string Subscription { get; set; }
    }

    public class ResultFile
    {
        public ResultFile()
        {
        }

        public ResultFile(string name, string path)
        {
            this.FileName = name;
            this.FullPath = path;
        }

        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "FullPath")]
        public string FullPath { get; set; }
    }
}
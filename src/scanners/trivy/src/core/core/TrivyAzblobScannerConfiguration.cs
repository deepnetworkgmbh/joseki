using System.Security;

namespace core.core
{
    public class TrivyAzblobScannerConfiguration
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public int HeartbeatPeriodicity { get; set; }

        public string TrivyVersion { get; set; }

        public string AzureBlobBaseUrl { get; set; }

        public string AzureBlobSasToken { get; set; }
    }
}
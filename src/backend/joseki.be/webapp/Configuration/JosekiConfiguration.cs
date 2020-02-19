namespace webapp.Configuration
{
    /// <summary>
    /// Joseki Backend service configuration.
    /// </summary>
    public class JosekiConfiguration
    {
        /// <summary>
        /// Azure Blob related configuration.
        /// </summary>
        public AzureBlobConfig AzureBlob { get; set; }

        /// <summary>
        /// Aggregated Watchmen configs.
        /// </summary>
        public Watchmen Watchmen { get; set; }
    }

    /// <summary>
    /// Azure Blob related configuration.
    /// </summary>
    public class AzureBlobConfig
    {
        /// <summary>
        /// Base Azure Blob Storage URL.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Sas token.
        /// </summary>
        public string Sas { get; set; }
    }

    /// <summary>
    /// Watchmen related configurations.
    /// </summary>
    public class Watchmen
    {
        /// <summary>
        /// How often ScannerContainersWatchman is listing root-level containers.
        /// The measurement is in seconds.
        /// </summary>
        public int ScannerContainersPeriodicity { get; set; }
    }
}
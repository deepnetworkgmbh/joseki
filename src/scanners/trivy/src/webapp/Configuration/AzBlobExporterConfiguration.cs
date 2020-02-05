namespace webapp.Configuration
{
    /// <summary>
    /// Represents Azure Blob Exporter configuration.
    /// </summary>
    public class AzBlobExporterConfiguration : IExporterConfiguration
    {
        /// <summary>
        /// Base Azure Blob Storage URL.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Sas token.
        /// </summary>
        public string Sas { get; set; }

        /// <summary>
        /// How often the application should update scanner metadata file.
        /// </summary>
        public int HeartbeatPeriodicity { get; set; }
    }
}
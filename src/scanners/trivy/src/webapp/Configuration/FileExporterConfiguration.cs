namespace webapp.Configuration
{
    /// <summary>
    /// Represents File Exporter configuration.
    /// </summary>
    public class FileExporterConfiguration : IExporterConfiguration
    {
        /// <summary>
        /// Path to the folder with Scan Results.
        /// </summary>
        public string Path { get; set; }
    }
}
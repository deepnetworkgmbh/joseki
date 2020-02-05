namespace webapp.Configuration
{
    /// <summary>
    /// The application configuration.
    /// </summary>
    public class ImageScannerConfiguration
    {
        /// <summary>
        /// The scanner configuration.
        /// </summary>
        public IScannerConfiguration Scanner { get; set; }

        /// <summary>
        /// The Scan Result exporter configuration.
        /// </summary>
        public IExporterConfiguration Exporter { get; set; }
    }

    /// <summary>
    /// The base interface for all types of scanners configuration.
    /// </summary>
    public interface IScannerConfiguration
    {
    }

    /// <summary>
    /// The base interface for all types of exporters configuration.
    /// </summary>
    public interface IExporterConfiguration
    {
    }
}
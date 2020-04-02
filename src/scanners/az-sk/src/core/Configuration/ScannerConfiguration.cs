namespace core.Configuration
{
    /// <summary>
    /// The application configuration.
    /// </summary>
    public class ScannerConfiguration
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
        /// <summary>
        /// Scanner identifier.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// How often the scanner is scheduled to run.
        /// </summary>
        string Periodicity { get; set; }
    }

    /// <summary>
    /// The base interface for all types of exporters configuration.
    /// </summary>
    public interface IExporterConfiguration
    {
    }
}
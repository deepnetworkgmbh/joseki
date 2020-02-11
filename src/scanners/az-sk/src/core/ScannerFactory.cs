using System;

using core.Configuration;
using core.exporters;
using core.exporters.azure;
using core.scanners;

namespace core
{
    /// <summary>
    /// Helps instantiating correct Scanner, Exporter and Importer implementations based on the application configuration.
    /// </summary>
    public class ScannerFactory
    {
        private readonly ConfigurationParser configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerFactory"/> class.
        /// </summary>
        /// <param name="configuration">the application configuration.</param>
        public ScannerFactory(ConfigurationParser configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets scanner implementation.
        /// </summary>
        /// <returns>At the moment, only azsk is supported, so returns azsk scanner object.</returns>
        public IScanner GetScanner()
        {
            var scannerConfiguration = this.configuration.Get();

            return scannerConfiguration.Scanner switch
            {
                AzSkConfiguration _ => new AzSk(this.configuration),
                _ => throw new NotImplementedException("At the moment only azsk scanner is supported")
            };
        }

        /// <summary>
        /// Gets exporter implementation.
        /// </summary>
        /// <returns>At the moment, only file is supported, so returns File Exporter object.</returns>
        public IExporter GetExporter()
        {
            var scannerConfiguration = this.configuration.Get();

            return scannerConfiguration.Exporter switch
            {
                FileExporterConfiguration fileExporterConfiguration => new FileExporter(fileExporterConfiguration.Path),
                AzBlobExporterConfiguration _ => new AzureBlobExporter(this.configuration),
                _ => throw new NotImplementedException("At the moment only file and Azure blob exporters are supported")
            };
        }
    }
}
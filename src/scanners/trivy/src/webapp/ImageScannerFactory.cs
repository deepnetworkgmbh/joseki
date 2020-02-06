using System;

using core.exporters;
using core.exporters.azure;
using core.scanners;

using webapp.Configuration;
using webapp.Queues;

namespace webapp
{
    /// <summary>
    /// Helps instantiating correct Scanner, Exporter and Importer implementations based on the application configuration.
    /// </summary>
    public class ImageScannerFactory
    {
        private readonly ConfigurationParser configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageScannerFactory"/> class.
        /// </summary>
        /// <param name="configuration">the application configuration.</param>
        public ImageScannerFactory(ConfigurationParser configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets scanner implementation.
        /// </summary>
        /// <returns>At the moment, only trivy is supported, so returns Trivy scanner object.</returns>
        public IScanner GetScanner()
        {
            var scannerConfiguration = this.configuration.Get();

            return scannerConfiguration.Scanner switch
            {
                TrivyConfiguration trivy => new Trivy(trivy.CachePath, trivy.BinaryPath, trivy.Registries),
                _ => throw new NotImplementedException("At the moment only trivy scanner is supported")
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
                AzBlobExporterConfiguration _ => new AzureBlobExporter(this.configuration.GetTrivyAzConfig()),
                _ => throw new NotImplementedException("At the moment only file and Azure blob exporters are supported")
            };
        }

        /// <summary>
        /// Gets Messaging Service implementation.
        /// </summary>
        /// <returns>At the moment only Azure Storage Queue is supported.</returns>
        public IQueueListener GetQueue()
        {
            var scannerConfiguration = this.configuration.Get();

            return scannerConfiguration.Queue switch
            {
                AzQueueConfiguration azQueue => new AzureStorageQueue(azQueue),
                _ => throw new NotImplementedException("At the moment, only Azure Storage Queue is supported")
            };
        }
    }
}
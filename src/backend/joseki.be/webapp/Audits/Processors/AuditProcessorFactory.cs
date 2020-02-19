using System;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

using webapp.Audits.Processors.azsk;

namespace webapp.Audits.Processors
{
    /// <summary>
    /// The factory knows how to instantiate proper Audit Processor object.
    /// </summary>
    public class AuditProcessorFactory
    {
        private static readonly ILogger Logger = Log.ForContext<AuditProcessorFactory>();
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditProcessorFactory"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public AuditProcessorFactory(IServiceProvider services)
        {
            this.services = services;
        }

        /// <summary>
        /// Based on scanner-metadata finds a proper Audit Processor object.
        /// </summary>
        /// <param name="metadata">Scanner metadata object.</param>
        /// <returns>Audit Processor instance.</returns>
        public IAuditProcessor GetProcessor(ScannerMetadata metadata)
        {
            Logger.Information("Instantiating {ScannerType} processor", metadata.Type);

            switch (metadata.Type)
            {
                case ScannerType.Azsk:
                    return this.services.GetService<AzskAuditProcessor>();
                case ScannerType.Polaris:
                    return this.services.GetService<PolarisAuditProcessor>();
                case ScannerType.Trivy:
                    return this.services.GetService<TrivyAuditProcessor>();
                default:
                    Logger.Warning("AuditProcessorFactory was requested to instantiate {ScannerType} processor, which is not supported", metadata.Type);
                    throw new NotSupportedException($"{metadata.Type} audit processor is not supported");
            }
        }
    }
}
using core.scanners;

namespace webapp.Configuration
{
    /// <summary>
    /// Trivy scanner configuration.
    /// </summary>
    public class TrivyConfiguration : IScannerConfiguration
    {
        /// <summary>
        /// The address of trivy binary.
        /// </summary>
        public string BinaryPath { get; set; }

        /// <summary>
        /// Trivy cache location.
        /// </summary>
        public string CachePath { get; set; }

        /// <summary>
        /// Array of Container Registry credentials.
        /// </summary>
        public RegistryCredentials[] Registries { get; set; }
    }
}
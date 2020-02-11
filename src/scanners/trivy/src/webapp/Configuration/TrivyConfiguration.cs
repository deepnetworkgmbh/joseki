using core.scanners;

namespace webapp.Configuration
{
    /// <summary>
    /// Trivy scanner configuration.
    /// </summary>
    public class TrivyConfiguration : IScannerConfiguration
    {
        /// <summary>
        /// Scanner identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Scanner version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Trivy binary version.
        /// </summary>
        public string TrivyVersion { get; set; }

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
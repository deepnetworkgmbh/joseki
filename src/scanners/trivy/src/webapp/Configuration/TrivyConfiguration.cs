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
        /// Trivy cache location.
        /// </summary>
        public string CachePath { get; set; }

        /// <summary>
        /// Array of Container Registry credentials.
        /// </summary>
        public RegistryCredentials[] Registries { get; set; }
    }
}
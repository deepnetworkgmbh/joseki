namespace core.scanners
{
    /// <summary>
    /// Container Registry credentials.
    /// </summary>
    public class RegistryCredentials
    {
        /// <summary>
        /// Container registry unique name. For example, name of organization in Docker Hub or myacr.azurecr.io.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Container registry address. For example, https://registry.hub.docker.com or myacr.azurecr.io.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The username to connect to the registry.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password to connect to the registry.
        /// </summary>
        public string Password { get; set; }
    }
}
namespace core.Configuration
{
    /// <summary>
    /// Azure Security Kit scanner configuration.
    /// </summary>
    public class AzSkConfiguration : IScannerConfiguration
    {
        /// <summary>
        /// Scanner identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// How often the scanner is scheduled to run.
        /// </summary>
        public string Periodicity { get; set; }

        /// <summary>
        /// Path to audit script file.
        /// </summary>
        public string AuditScriptPath { get; set; }

        /// <summary>
        /// Azure Tenant identifier.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Service Principal identifier.
        /// </summary>
        public string ServicePrincipalId { get; set; }

        /// <summary>
        /// Service Principal password.
        /// </summary>
        public string ServicePrincipalPassword { get; set; }
    }
}
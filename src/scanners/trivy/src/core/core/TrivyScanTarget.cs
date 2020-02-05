using Newtonsoft.Json;

namespace core.core
{
    /// <summary>
    /// List of vulnerabilities in the scanned target (OS and application packages).
    /// </summary>
    public class TrivyScanTarget
    {
        /// <summary>
        /// The target name. for example, OS name or ruby gems file.
        /// </summary>
        [JsonProperty(PropertyName = "Target")]
        public string Target { get; set; }

        /// <summary>
        /// The list of found vulnerabilities.
        /// </summary>
        [JsonProperty(PropertyName = "Vulnerabilities")]
        public TrivyVulnerabilityDescription[] Vulnerabilities { get; set; }
    }

    /// <summary>
    /// Single vulnerability details.
    /// </summary>
    public class TrivyVulnerabilityDescription
    {
        /// <summary>
        /// CVE name.
        /// </summary>
        [JsonProperty(PropertyName = "VulnerabilityID")]
        public string VulnerabilityID { get; set; }

        /// <summary>
        /// Package name, whenre CVE was discovered.
        /// </summary>
        [JsonProperty(PropertyName = "PkgName")]
        public string PkgName { get; set; }

        /// <summary>
        /// The version of package in container.
        /// </summary>
        [JsonProperty(PropertyName = "InstalledVersion")]
        public string InstalledVersion { get; set; }

        /// <summary>
        /// the version of package, where the CVE was fixed.
        /// </summary>
        [JsonProperty(PropertyName = "FixedVersion")]
        public string FixedVersion { get; set; }

        /// <summary>
        /// Short CVE title.
        /// </summary>
        [JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// Detailed CVE description.
        /// </summary>
        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// The severity.
        /// </summary>
        [JsonProperty(PropertyName = "Severity")]
        public string Severity { get; set; }

        /// <summary>
        /// List of references with further information.
        /// </summary>
        [JsonProperty(PropertyName = "References")]
        public string[] References { get; set; }
    }
}
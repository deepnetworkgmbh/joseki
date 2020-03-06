using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace webapp.Models
{
    /// <summary>
    /// Represents container image scan result.
    /// </summary>
    public class ContainerImageScanResult
    {
        /// <summary>
        /// The full image name.
        /// </summary>
        [JsonProperty(PropertyName = "Image")]
        public string Image { get; set; }

        /// <summary>
        /// The scan date.
        /// </summary>
        [JsonProperty(PropertyName = "Image")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Scan result.
        /// </summary>
        [JsonProperty(PropertyName = "ScanResult")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScanResult ScanResult { get; set; }

        /// <summary>
        /// Description for Failed scans.
        /// </summary>
        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Counts issues by Severity.
        /// </summary>
        [JsonProperty(PropertyName = "Counters")]
        public VulnerabilityCounters[] Counters { get; set; }

        /// <summary>
        /// List of scanned targets.
        /// </summary>
        [JsonProperty(PropertyName = "Targets")]
        public ImageScanTarget[] Targets { get; set; }
    }

    /// <summary>
    /// Represents scan result for the same image, but for two dates.
    /// Used to display Image scan diff page.
    /// </summary>
    public class ContainerImageScanResultDiff
    {
        /// <summary>
        /// The result of image scan at Date1.
        /// </summary>
        public ContainerImageScanResult Scan1 { get; set; }

        /// <summary>
        /// The result of image scan at Date2.
        /// </summary>
        public ContainerImageScanResult Scan2 { get; set; }
    }

    /// <summary>
    /// Scan result enum.
    /// </summary>
    public enum ScanResult
    {
        /// <summary>
        /// Enum value when a scan was not found.
        /// </summary>
        [EnumMember(Value = "NOT_FOUND")]
        NotFound = 0,

        /// <summary>
        /// Enum value when a scan failed.
        /// </summary>
        [EnumMember(Value = "FAILED")]
        Failed = 1,

        /// <summary>
        /// Enum value when a scan succeeded.
        /// </summary>
        [EnumMember(Value = "SUCCEEDED")]
        Succeeded = 2,
    }

    /// <summary>
    /// Represents the amount of issues with a particular severity.
    /// </summary>
    public class VulnerabilityCounters
    {
        /// <summary>
        /// The CVE severity.
        /// </summary>
        [JsonProperty(PropertyName = "Severity")]
        public string Severity { get; set; }

        /// <summary>
        /// Amount of CVEs with this severity.
        /// </summary>
        [JsonProperty(PropertyName = "Count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// List of vulnerabilities in the scanned target (OS and application packages).
    /// </summary>
    public class ImageScanTarget
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
        public VulnerabilityDescription[] Vulnerabilities { get; set; }
    }

    /// <summary>
    /// Single vulnerability details.
    /// </summary>
    public class VulnerabilityDescription
    {
        /// <summary>
        /// CVE identifier.
        /// </summary>
        [JsonProperty(PropertyName = "VulnerabilityID")]
        public string VulnerabilityID { get; set; }

        /// <summary>
        /// Packages that have CVE or depend on a package with CVE.
        /// </summary>
        [JsonProperty(PropertyName = "DependenciesWithCVE")]
        public string[] DependenciesWithCVE { get; set; }

        /// <summary>
        /// Package name, where CVE was discovered.
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
        [JsonProperty(PropertyName = "Remediation")]
        public string Remediation { get; set; }

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
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace core.core
{
    /// <summary>
    /// Represents detailed information about a single CVE.
    /// </summary>
    public class CveDetails
    {
        /// <summary>
        /// CVE identifier.
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        /// <summary>
        /// Package name, where CVE was discovered.
        /// </summary>
        [JsonProperty(PropertyName = "Package")]
        public string PackageName { get; set; }

        /// <summary>
        /// The version of package with CVE.
        /// </summary>
        [JsonProperty(PropertyName = "InstalledVersion")]
        public string InstalledVersion { get; set; }

        /// <summary>
        /// The version of package, where the CVE was fixed.
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

        /// <summary>
        /// The list of images, where the vulnerable package was used.
        /// </summary>
        [JsonProperty(PropertyName = "Images")]
        public List<string> ImageTags { get; set; }

        public static CveDetails FromTrivyDescription(TrivyVulnerabilityDescription cve, ContainerImage containerImage)
        {
            return new CveDetails
            {
                Id = cve.VulnerabilityID,
                Title = cve.Title,
                PackageName = cve.PkgName,
                InstalledVersion = cve.InstalledVersion,
                FixedVersion = cve.FixedVersion,
                Severity = cve.Severity,
                Description = cve.Description,
                References = (string[])cve.References?.Clone(),
                ImageTags = new List<string> { containerImage.FullName },
            };
        }

        public CveDetails DeduplicateImages()
        {
            this.ImageTags = this.ImageTags.Where(i => i != null).Distinct().ToList();

            return this;
        }
    }
}
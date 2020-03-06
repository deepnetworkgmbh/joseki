namespace webapp.Database.Models
{
    /// <summary>
    /// Maps exact image scans to list of CVEs in this scan.
    /// </summary>
    public class ImageScanToCve
    {
        /// <summary>
        /// Internal CVE identifier.
        /// </summary>
        public int InternalCveId { get; set; }

        /// <summary>
        /// Where the CVE was discovered: container image name or application dependencies file.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The package that have CVE or depends on a package with CVE.
        /// </summary>
        public string UsedPackage { get; set; }

        /// <summary>
        /// States which exact version of vulnerable package `ScanId` discovered.
        /// </summary>
        public string UsedPackageVersion { get; set; }
    }
}
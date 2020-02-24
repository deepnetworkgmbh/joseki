namespace webapp.Database.Models
{
    /// <summary>
    /// Maps exact image scans to list of CVEs in this scan.
    /// </summary>
    public class ImageScanToCve
    {
        /// <summary>
        /// Image Scan Result identifier.
        /// </summary>
        public string ScanId { get; set; }

        /// <summary>
        /// CVE identifier.
        /// </summary>
        public string CveId { get; set; }

        /// <summary>
        /// Internal CVE identifier.
        /// </summary>
        public int InternalCveId { get; set; }

        /// <summary>
        /// Where the CVE was discovered: container image name or application dependencies file.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// States which exact version of vulnerable package `ScanId` discovered.
        /// </summary>
        public string UsedPackageVersion { get; set; }

        /// <summary>
        /// Reference to Image Scan Result object.
        /// </summary>
        public ImageScanResult ImageScan { get; set; }
    }
}
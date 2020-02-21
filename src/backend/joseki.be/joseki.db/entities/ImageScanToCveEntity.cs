using System;

namespace joseki.db.entities
{
    /// <summary>
    /// Maps exact image scans to list of CVEs in this scan.
    /// </summary>
    public class ImageScanToCveEntity : IJosekiBaseEntity
    {
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public DateTime DateUpdated { get; set; }

        /// <inheritdoc />
        public DateTime DateCreated { get; set; }

        /// <inheritdoc />
        public string ChangedBy { get; set; }

        /// <summary>
        /// Image Scan Result identifier.
        /// </summary>
        public int ScanId { get; set; }

        /// <summary>
        /// CVE identifier.
        /// </summary>
        public int CveId { get; set; }

        /// <summary>
        /// Where the CVE was discovered: container image name or application dependencies file.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// States which exact version of vulnerable package `ScanId` discovered.
        /// </summary>
        public string UsedPackageVersion { get; set; }

        /// <summary>
        /// Reference to associated CVE object.
        /// </summary>
        public CveEntity CVE { get; set; }

        /// <summary>
        /// Reference to Image Scan Result object.
        /// </summary>
        public ImageScanResultEntity ImageScan { get; set; }
    }
}
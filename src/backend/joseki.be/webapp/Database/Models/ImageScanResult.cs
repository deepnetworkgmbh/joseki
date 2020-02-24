using System;
using System.Linq;

namespace webapp.Database.Models
{
    /// <summary>
    /// Short representation of Image Scan Result.
    /// </summary>
    public class ImageScanResult
    {
        /// <summary>
        /// Full image tag, which includes registry, image name and version.
        /// For example `mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10`.
        /// </summary>
        public string ImageTag { get; set; }

        /// <summary>
        /// UTC point of time when scan was performed.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The status of image scan.
        /// </summary>
        public ImageScanStatus Status { get; set; }

        /// <summary>
        /// Counts issues by Severity.
        /// </summary>
        public VulnerabilityCounter[] Counters { get; set; }

        /// <summary>
        /// Maps the object to Check Result value.
        /// </summary>
        /// <returns>Check Result value.</returns>
        public CheckValue GetCheckResultValue()
        {
            switch (this.Status)
            {
                case ImageScanStatus.Queued:
                    return CheckValue.InProgress;
                case ImageScanStatus.Failed:
                    return CheckValue.NoData;
                case ImageScanStatus.Succeeded:
                    // consider Low and Unknown priorities issues as not-an-issue;
                    var anyImportant = this.Counters.Any(i => i.Count > 0 && i.Severity >= CveSeverity.Medium);
                    return anyImportant
                        ? CheckValue.Failed
                        : CheckValue.Succeeded;
                default:
                    return CheckValue.NoData;
            }
        }

        /// <summary>
        /// Compose human-readable result description with format "{severity-1} {issues-count-1}; {severity-2} {issues-count-2};...".
        /// </summary>
        /// <returns>Short human-readable image-scan summary.</returns>
        public string GetCheckResultMessage()
        {
            if (this.Status == ImageScanStatus.Queued)
            {
                return "The scan is in progress";
            }
            else if (this.Counters.Length == 0)
            {
                return "No issues";
            }
            else
            {
                var ordered = this.Counters
                    .Where(i => i.Count > 0)
                    .OrderByDescending(i => i.Severity)
                    .Select(i => $"{i.Count} {i.Severity.ToString()}");

                return string.Join("; ", ordered);
            }
        }
    }

    /// <summary>
    /// Represents the amount of issues with a particular severity.
    /// </summary>
    public class VulnerabilityCounter
    {
        /// <summary>
        /// The CVE severity.
        /// </summary>
        public CveSeverity Severity { get; set; }

        /// <summary>
        /// Amount of CVEs with this severity.
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// The status of the Image Scan.
    /// </summary>
    public enum ImageScanStatus
    {
        /// <summary>
        /// Image Scan was already queued, but the result is not handled yet.
        /// </summary>
        Queued,

        /// <summary>
        /// Image scanner failed to perform the scan.
        /// </summary>
        Failed,

        /// <summary>
        /// Image scan succeeded.
        /// This status does not mean, that the image passed the scan!
        /// </summary>
        Succeeded,
    }
}
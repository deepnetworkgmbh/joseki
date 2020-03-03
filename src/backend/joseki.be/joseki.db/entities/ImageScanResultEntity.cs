using System;
using System.Collections.Generic;

namespace joseki.db.entities
{
    /// <summary>
    /// The goal of image scan is verify if known CVEs (Common Vulnerabilities and Exposures) present in used by image packages.
    /// Therefore, the Scan Result consists of an array of CVEs. The array might be empty, if no vulnerabilities was found.
    /// </summary>
    public class ImageScanResultEntity : IJosekiBaseEntity
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
        /// Unique identifier.
        /// </summary>
        public string ExternalId { get; set; }

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
        /// The status of the Image Scan.
        /// </summary>
        public ImageScanStatus Status { get; set; }

        /// <summary>
        /// Human-readable description of the scan.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of discovered vulnerabilities.
        /// </summary>
        public List<ImageScanToCveEntity> FoundCVEs { get; set; }
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
using System;
using System.Threading.Tasks;

using webapp.Database.Models;

namespace webapp.Database
{
    /// <summary>
    /// An abstraction layer on top of Joseki database.
    /// </summary>
    public interface IJosekiDatabase
    {
        /// <summary>
        /// Saves all the audit related data to the database:
        /// - Audit itself,
        /// - associated Check-Results,
        /// - not existing before this point Checks,
        /// - Azure or Kube metadata.
        /// </summary>
        /// <param name="audit">The entire audit object, including checks, check-results, and metadata.</param>
        /// <returns>A task object.</returns>
        Task SaveAuditResult(Audit audit);

        /// <summary>
        /// Saves all the image-scan related data to the database:
        /// - image scan itself;
        /// - associated CVEs.
        /// </summary>
        /// <param name="imageScanResult">The entire Image Scan Result object.</param>
        /// <returns>A task object.</returns>
        Task SaveImageScanResult(ImageScanResultWithCVEs imageScanResult);

        /// <summary>
        /// Saves a placeholder for in-progress image-scan.
        /// </summary>
        /// <param name="imageScanResult">The image scan place-holder.</param>
        /// <returns>A task object.</returns>
        Task SaveInProgressImageScan(ImageScanResultWithCVEs imageScanResult);

        /// <summary>
        /// Queries the database for image-scan results, that are within scan TTL.
        /// Image Scan TTL is defined by joseki configuration.
        /// </summary>
        /// <param name="imageTags">Array of unique image-tags.</param>
        /// <returns>Not expired image scans.</returns>
        Task<ImageScanResult[]> GetNotExpiredImageScans(string[] imageTags);

        /// <summary>
        /// Gets latest audits for each component at particular date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>Latest audits.</returns>
        Task<Audit[]> GetAuditedComponentsWithHistory(DateTime date);
    }
}
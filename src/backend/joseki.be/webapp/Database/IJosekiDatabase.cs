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
        Task SaveImageScanResult(ImageScanResult imageScanResult);
    }
}
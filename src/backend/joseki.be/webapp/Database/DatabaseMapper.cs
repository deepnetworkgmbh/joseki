using System.Linq;

using joseki.db.entities;

using webapp.Database.Models;

using CheckSeverity = joseki.db.entities.CheckSeverity;
using CheckValue = joseki.db.entities.CheckValue;

namespace webapp.Database
{
    /// <summary>
    /// Maps internal object ot database entities and vice versa.
    /// </summary>
    public static class DatabaseMapper
    {
        /// <summary>
        /// Creates Check entity from internal model.
        /// </summary>
        /// <param name="check">Internal Check model.</param>
        /// <returns>Database compatible entity.</returns>
        public static CheckEntity ToEntity(this Check check)
        {
            return new CheckEntity
            {
                CheckId = check.Id,
                Category = check.Category,
                Description = check.Description,
                Remediation = check.Remediation,
                Severity = check.Severity.ToEntity(),
            };
        }

        /// <summary>
        /// Creates Audit entity from internal model.
        /// </summary>
        /// <param name="audit">Internal Audit model.</param>
        /// <returns>Database compatible entity.</returns>
        public static AuditEntity ToEntity(this Audit audit)
        {
            var entity = new AuditEntity
            {
                AuditId = audit.Id,
                Date = audit.Date,
                ScannerId = audit.ScannerId,
            };

            if (audit.MetadataKube != null)
            {
                entity.MetadataKube = audit.MetadataKube.ToEntity();
            }
            else if (audit.MetadataAzure != null)
            {
                entity.MetadataAzure = audit.MetadataAzure.ToEntity();
            }

            entity.CheckResults = audit.CheckResults.Select(i => i.ToEntity()).ToList();

            return entity;
        }

        /// <summary>
        /// Creates Check Result entity from internal model.
        /// </summary>
        /// <param name="checkResult">Internal Check Result model.</param>
        /// <returns>Database compatible entity.</returns>
        public static CheckResultEntity ToEntity(this CheckResult checkResult)
        {
            return new CheckResultEntity
            {
                CheckId = checkResult.InternalCheckId,
                ComponentId = checkResult.ComponentId,
                Message = checkResult.Message,
                Value = checkResult.Value.ToEntity(),
            };
        }

        /// <summary>
        /// Creates Kubernetes Metadata entity from internal model.
        /// </summary>
        /// <param name="metadata">Internal metadata model.</param>
        /// <returns>Database compatible entity.</returns>
        public static MetadataKubeEntity ToEntity(this MetadataKube metadata)
        {
            return new MetadataKubeEntity
            {
                Date = metadata.Date,
                JSON = metadata.JSON,
            };
        }

        /// <summary>
        /// Creates Azure Metadata entity from internal model.
        /// </summary>
        /// <param name="metadata">Internal metadata model.</param>
        /// <returns>Database compatible entity.</returns>
        public static MetadataAzureEntity ToEntity(this MetadataAzure metadata)
        {
            return new MetadataAzureEntity
            {
                Date = metadata.Date,
                JSON = metadata.JSON,
            };
        }

        /// <summary>
        /// Creates Image Scan entity from internal model.
        /// </summary>
        /// <param name="scan">Internal Image Scan model.</param>
        /// <returns>Database compatible entity.</returns>
        public static ImageScanResultEntity ToEntity(this ImageScanResult scan)
        {
            var entity = new ImageScanResultEntity
            {
                ExternalId = scan.Id,
                ImageTag = scan.ImageTag,
                Date = scan.Date,
                FoundCVEs = scan.FoundCVEs.Select(i => i.ToEntity()).ToList(),
            };

            return entity;
        }

        /// <summary>
        /// Creates Image Scan to CVE entity from internal model.
        /// </summary>
        /// <param name="scanToCve">Internal Image Scan to CVE model.</param>
        /// <returns>Database compatible entity.</returns>
        public static ImageScanToCveEntity ToEntity(this ImageScanToCve scanToCve)
        {
            return new ImageScanToCveEntity
            {
                CveId = scanToCve.InternalCveId,
                Target = scanToCve.Target,
                UsedPackageVersion = scanToCve.UsedPackageVersion,
            };
        }

        /// <summary>
        /// Creates CVE entity from internal model.
        /// </summary>
        /// <param name="cve">Internal CVE model.</param>
        /// <returns>Database compatible entity.</returns>
        public static CveEntity ToEntity(this CVE cve)
        {
            return new CveEntity
            {
                CveId = cve.Id,
                PackageName = cve.PackageName,
                Severity = cve.Severity.ToEntity(),
                Title = cve.Title,
                Description = cve.Description,
                Remediation = cve.Remediation,
                References = cve.References,
            };
        }

        /// <summary>
        /// Creates CheckValue enum value from internal enum.
        /// </summary>
        /// <param name="value">Internal enum.</param>
        /// <returns>Database compatible enum.</returns>
        public static CheckValue ToEntity(this Database.Models.CheckValue value)
        {
            return value switch
            {
                Models.CheckValue.Succeeded => CheckValue.Succeeded,
                Models.CheckValue.Failed => CheckValue.Failed,
                _ => CheckValue.NoData
            };
        }

        /// <summary>
        /// Creates CheckSeverity enum value from internal enum.
        /// </summary>
        /// <param name="severity">Internal enum.</param>
        /// <returns>Database compatible enum.</returns>
        public static CheckSeverity ToEntity(this Database.Models.CheckSeverity severity)
        {
            return severity switch
            {
                Models.CheckSeverity.Critical => CheckSeverity.Critical,
                Models.CheckSeverity.High => CheckSeverity.High,
                Models.CheckSeverity.Medium => CheckSeverity.Medium,
                Models.CheckSeverity.Low => CheckSeverity.Low,
                Models.CheckSeverity.Unknown => CheckSeverity.Unknown,
                _ => CheckSeverity.Unknown
            };
        }

        /// <summary>
        /// Creates joseki.db.entities.CveSeverity enum value from internal enum.
        /// </summary>
        /// <param name="severity">Internal enum.</param>
        /// <returns>Database compatible enum.</returns>
        public static joseki.db.entities.CveSeverity ToEntity(this Models.CveSeverity severity)
        {
            return severity switch
            {
                Models.CveSeverity.Critical => joseki.db.entities.CveSeverity.Critical,
                Models.CveSeverity.High => joseki.db.entities.CveSeverity.High,
                Models.CveSeverity.Medium => joseki.db.entities.CveSeverity.Medium,
                Models.CveSeverity.Low => joseki.db.entities.CveSeverity.Low,
                Models.CveSeverity.Unknown => joseki.db.entities.CveSeverity.Unknown,
                _ => joseki.db.entities.CveSeverity.Unknown
            };
        }
    }
}
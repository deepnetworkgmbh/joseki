using System;
using System.Collections.Generic;
using System.Linq;

using joseki.db.entities;

using webapp.Database.Models;

using CheckSeverity = joseki.db.entities.CheckSeverity;
using CheckValue = joseki.db.entities.CheckValue;
using CveSeverity = joseki.db.entities.CveSeverity;
using ImageScanStatus = joseki.db.entities.ImageScanStatus;

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
                ComponentId = audit.ComponentId,
                ComponentName = audit.ComponentName,
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
        public static ImageScanResultEntity ToEntity(this ImageScanResultWithCVEs scan)
        {
            var entity = new ImageScanResultEntity
            {
                ExternalId = scan.Id,
                ImageTag = scan.ImageTag,
                Date = scan.Date,
                Status = scan.Status.ToEntity(),
                Description = scan.Description,
                FoundCVEs = scan.FoundCVEs?.Select(i => i.ToEntity()).ToList(),
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
                Models.CheckValue.InProgress => CheckValue.InProgress,
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
        public static CveSeverity ToEntity(this Models.CveSeverity severity)
        {
            return severity switch
            {
                Models.CveSeverity.Critical => CveSeverity.Critical,
                Models.CveSeverity.High => CveSeverity.High,
                Models.CveSeverity.Medium => CveSeverity.Medium,
                Models.CveSeverity.Low => CveSeverity.Low,
                Models.CveSeverity.Unknown => CveSeverity.Unknown,
                _ => CveSeverity.Unknown
            };
        }

        /// <summary>
        /// Creates joseki.db.entities.ImageScanStatus enum value from internal enum.
        /// </summary>
        /// <param name="status">Internal enum.</param>
        /// <returns>Database compatible enum.</returns>
        public static ImageScanStatus ToEntity(this Models.ImageScanStatus status)
        {
            return status switch
            {
                Models.ImageScanStatus.Failed => ImageScanStatus.Failed,
                Models.ImageScanStatus.Queued => ImageScanStatus.Queued,
                Models.ImageScanStatus.Succeeded => ImageScanStatus.Succeeded,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        /// <summary>
        /// Creates internal enum value from database-compatible.
        /// </summary>
        /// <param name="severity">Database compatible enum.</param>
        /// <returns>Internal enum.</returns>
        public static Models.CveSeverity FromEntity(this CveSeverity severity)
        {
            return severity switch
            {
                CveSeverity.Critical => Models.CveSeverity.Critical,
                CveSeverity.High => Models.CveSeverity.High,
                CveSeverity.Medium => Models.CveSeverity.Medium,
                CveSeverity.Low => Models.CveSeverity.Low,
                CveSeverity.Unknown => Models.CveSeverity.Unknown,
                _ => Models.CveSeverity.Unknown
            };
        }

        /// <summary>
        /// Creates Image Scan model from database entity.
        /// </summary>
        /// <param name="entity">Database entity.</param>
        /// <returns>Internal model.</returns>
        public static ImageScanResult GetShortResult(this ImageScanResultEntity entity)
        {
            var counters = new Dictionary<CveSeverity, int>();
            foreach (var foundCve in entity.FoundCVEs)
            {
                if (counters.TryGetValue(foundCve.CVE.Severity, out var counter))
                {
                    counters[foundCve.CVE.Severity]++;
                }
                else
                {
                    counters.Add(foundCve.CVE.Severity, 1);
                }
            }

            var model = new ImageScanResult
            {
                ImageTag = entity.ImageTag,
                Date = entity.Date,
                Status = entity.Status.FromEntity(),
                Description = entity.Description,
                Counters = counters.Select(i => new VulnerabilityCounter { Severity = i.Key.FromEntity(), Count = i.Value }).ToArray(),
            };

            return model;
        }

        /// <summary>
        /// Creates Audit internal model from database entity.
        /// </summary>
        /// <param name="entity">Database compatible entity.</param>
        /// <returns>Internal Audit model.</returns>
        public static Audit FromEntity(this AuditEntity entity)
        {
            var audit = new Audit
            {
                Id = entity.AuditId,
                Date = entity.Date,
                ScannerId = entity.ScannerId,
                ComponentId = entity.ComponentId,
                ComponentName = entity.ComponentName,
            };

            return audit;
        }

        /// <summary>
        /// Creates ImageScanStatus enum value from database-compatible enum.
        /// </summary>
        /// <param name="status">Database compatible enum.</param>
        /// <returns>Internal enum.</returns>
        public static Models.ImageScanStatus FromEntity(this ImageScanStatus status)
        {
            return status switch
            {
                ImageScanStatus.Failed => Models.ImageScanStatus.Failed,
                ImageScanStatus.Succeeded => Models.ImageScanStatus.Succeeded,
                ImageScanStatus.Queued => Models.ImageScanStatus.Queued,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}
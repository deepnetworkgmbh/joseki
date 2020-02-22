using joseki.db.entities;

using webapp.Database.Models;

using CheckSeverity = joseki.db.entities.CheckSeverity;

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
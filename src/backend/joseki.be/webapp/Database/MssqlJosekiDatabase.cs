using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Serilog;

using webapp.Database.Models;

namespace webapp.Database
{
    /// <summary>
    /// MsSQL implementation of Database.
    /// </summary>
    public class MssqlJosekiDatabase : IJosekiDatabase
    {
        private static readonly ILogger Logger = Log.ForContext<MssqlJosekiDatabase>();
        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlJosekiDatabase"/> class.
        /// </summary>
        public MssqlJosekiDatabase(JosekiDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc />
        public async Task SaveAuditResult(Audit audit)
        {
            Logger.Information("Saving audit {AuditId} with {CheckResults} Check Results", audit.Id, audit.CheckResults.Count);

            this.db.Set<AuditEntity>().Add(audit.ToEntity());
            await this.db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task SaveImageScanResult(ImageScanResult imageScanResult)
        {
            Logger.Information("Saving Image Scan {ImageScanId} with {FoundCVE} found CVEs", imageScanResult.Id, imageScanResult.FoundCVEs?.Count ?? 0);

            this.db.Set<ImageScanResultEntity>().Add(imageScanResult.ToEntity());
            await this.db.SaveChangesAsync();
        }
    }
}
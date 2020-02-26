using System;
using System.Linq;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using Serilog;

using webapp.Configuration;
using webapp.Database.Models;

using CheckValue = joseki.db.entities.CheckValue;

namespace webapp.Database
{
    /// <summary>
    /// MsSQL implementation of Database.
    /// </summary>
    public class MssqlJosekiDatabase : IJosekiDatabase
    {
        private static readonly ILogger Logger = Log.ForContext<MssqlJosekiDatabase>();
        private readonly JosekiDbContext db;
        private readonly JosekiConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlJosekiDatabase"/> class.
        /// </summary>
        public MssqlJosekiDatabase(JosekiDbContext db, ConfigurationParser parser)
        {
            this.db = db;
            this.config = parser.Get();
        }

        /// <inheritdoc />
        public async Task SaveAuditResult(Audit audit)
        {
            Logger.Information("Saving audit {AuditId} with {CheckResults} Check Results", audit.Id, audit.CheckResults.Count);

            this.db.Set<AuditEntity>().Add(audit.ToEntity());
            await this.db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task SaveInProgressImageScan(ImageScanResultWithCVEs imageScanResult)
        {
            Logger.Information("Saving In-Progress Image Scan {ImageScanId} for {ImageTag}", imageScanResult.Id, imageScanResult.ImageTag);

            this.db.Set<ImageScanResultEntity>().Add(imageScanResult.ToEntity());
            await this.db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task SaveImageScanResult(ImageScanResultWithCVEs imageScanResult)
        {
            Logger.Information(
                "Saving Image Scan {ImageScanId} for {ImageTag} with {FoundCVE} found CVEs",
                imageScanResult.Id,
                imageScanResult.ImageTag,
                imageScanResult.FoundCVEs?.Count ?? 0);

            var existingScanResult = await this.db.Set<ImageScanResultEntity>().FirstOrDefaultAsync(i => i.ExternalId == imageScanResult.Id);

            if (existingScanResult == null)
            {
                this.db.Set<ImageScanResultEntity>().Add(imageScanResult.ToEntity());
            }
            else
            {
                var newEntity = imageScanResult.ToEntity();
                existingScanResult.Date = newEntity.Date;
                existingScanResult.FoundCVEs = newEntity.FoundCVEs;
                existingScanResult.Status = newEntity.Status;

                this.db.Set<ImageScanResultEntity>().Update(existingScanResult);
            }

            // update in-progress check-results
            var checkResults = await this.db.Set<CheckResultEntity>()
                .Where(i => i.Value == CheckValue.InProgress && i.ComponentId.EndsWith(imageScanResult.ImageTag))
                .ToArrayAsync();

            foreach (var result in checkResults)
            {
                result.Value = imageScanResult.GetCheckResultValue().ToEntity();
                result.Message = imageScanResult.GetCheckResultMessage();
                this.db.Update(result);
            }

            await this.db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<ImageScanResult[]> GetNotExpiredImageScans(string[] imageTags)
        {
            var expirationTime = DateTime.UtcNow.AddHours(-this.config.Cache.ImageScanTtl);
            var existingScans = await this.db.Set<ImageScanResultEntity>()
                .Include("FoundCVEs.CVE")
                .AsNoTracking()
                .Where(sr => imageTags.Contains(sr.ImageTag) && sr.Date > expirationTime)
                .Select(i => i.GetShortResult())
                .ToArrayAsync();

            return existingScans
                .GroupBy(i => i.ImageTag)
                .Select(i => i.OrderByDescending(scan => scan.Date).FirstOrDefault())
                .ToArray();
        }
    }
}
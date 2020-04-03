using System;
using System.Collections.Generic;
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
            Logger.Information("Saving audit {AuditId} with {CheckResults} Check Results", audit.Id, audit.CheckResults?.Count ?? 0);

            var componentEntity = await this.db
                .Set<InfrastructureComponentEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ComponentId == audit.ComponentId);

            if (componentEntity == null)
            {
                Logger.Information(
                    "Saving new Infrastructure Component record with id:{ComponentId} and scanner:{ScannerId}",
                    audit.ComponentId,
                    audit.ScannerId);

                componentEntity = this.db.Set<InfrastructureComponentEntity>()
                    .Add(new InfrastructureComponentEntity
                    {
                        ComponentId = audit.ComponentId,
                        ComponentName = audit.ComponentName,
                        ScannerId = audit.ScannerId,
                    })
                    .Entity;
                await this.db.SaveChangesAsync();
            }

            this.db.Set<AuditEntity>().Add(audit.ToEntity(componentEntity.Id));
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
                existingScanResult.Description = newEntity.Description;

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

        /// <inheritdoc />
        public async Task<Audit[]> GetAuditedComponentsWithHistory(DateTime date)
        {
            // find unique component-ids for this date;
            var theDay = date.Date;
            var theNextDay = theDay.AddDays(1);
            var componentIds = await this.db.Set<AuditEntity>()
                .AsNoTracking()
                .Where(i => i.Date >= theDay && i.Date < theNextDay)
                .Select(i => i.ComponentId)
                .Distinct()
                .ToArrayAsync();

            // find all audits for these components during the 31 days (~month)
            var oneMonthAgo = DateTime.UtcNow.Date.AddDays(-30);
            var oneMonthAudits = await this.db.Set<AuditEntity>()
                .Include(i => i.InfrastructureComponent)
                .AsNoTracking()
                .Where(i => i.Date > oneMonthAgo && componentIds.Contains(i.ComponentId))
                .ToArrayAsync();

            // Select only the most recent audit for each date for each component
            var result = new List<Audit>(oneMonthAudits.Length);
            foreach (var oneComponentAudits in oneMonthAudits.GroupBy(i => i.ComponentId))
            {
                result.AddRange(oneComponentAudits
                    .GroupBy(i => i.Date.Date)
                    .Select(i => i.OrderByDescending(scan => scan.Date).First().FromEntity()));
            }

            return result.ToArray();
        }
    }
}
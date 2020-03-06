using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using webapp.Audits.Processors.trivy;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-image-scan-details request.
    /// </summary>0
    public class GetImageScanHandler
    {
        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetImageScanHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        public GetImageScanHandler(JosekiDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Returns Image Scan details history for requested Tag.
        /// </summary>
        /// <param name="imageTag">Image full tag.</param>
        /// <returns>Image scan details.</returns>
        public async Task<ContainerImageScanResult[]> GetHistory(string imageTag)
        {
            var today = DateTime.UtcNow.Date;

            var results = new List<ContainerImageScanResult>();
            foreach (var date in Enumerable.Range(-30, 31).Select(i => today.AddDays(i)))
            {
                results.Add(await this.GetImageScanResult(imageTag, date, this.CalculateCounters));
            }

            return results.ToArray();
        }

        /// <summary>
        /// Returns Image Scan details for requested Tag and Date.
        /// </summary>
        /// <param name="imageTag">Image full tag.</param>
        /// <param name="date">The date to get details for.</param>
        /// <returns>Image scan details.</returns>
        public Task<ContainerImageScanResult> GetDetails(string imageTag, DateTime date)
        {
            return this.GetImageScanResult(imageTag, date.Date, this.ParseCveDetails);
        }

        /// <summary>
        /// Returns only CVE counters for Image Scan at requested Date.
        /// </summary>
        /// <param name="imageTag">Image full tag.</param>
        /// <param name="date">The date to get details for.</param>
        /// <returns>Image scan details.</returns>
        public Task<ContainerImageScanResult> GetCounters(string imageTag, DateTime date)
        {
            return this.GetImageScanResult(imageTag, date.Date, this.CalculateCounters);
        }

        private async Task<ContainerImageScanResult> GetImageScanResult(string imageTag, DateTime date, Func<ImageScanResultEntity, Task<ContainerImageScanResult>> processCVEs)
        {
            // get scan-result which represents the "date".
            var scans = await this.db.Set<ImageScanResultEntity>()
                .AsNoTracking()
                .Where(i => i.Date.Date == date.Date && i.ImageTag == imageTag)
                .ToArrayAsync();

            // if there is no scans - 404
            if (scans.Length == 0)
            {
                return new ContainerImageScanResult
                {
                    Counters = new VulnerabilityCounters[0],
                    Description = "No Image Scan Result at requested date",
                    Image = imageTag,
                    ScanResult = ScanResult.NotFound,
                    Date = date,
                };
            }

            // if there is only queued scans, but no results for this date - 404
            if (scans.All(i => i.Status == ImageScanStatus.Queued))
            {
                return new ContainerImageScanResult
                {
                    Counters = new VulnerabilityCounters[0],
                    Description = "Image Scan was queued, but there is no response",
                    Image = imageTag,
                    ScanResult = ScanResult.NotFound,
                    Date = date,
                };
            }

            // get latest failed or succeeded scan-result
            var scan = scans
                .Where(i => i.Status != ImageScanStatus.Queued)
                .OrderByDescending(i => i.Date)
                .First();

            // if it's failed - return it with description
            if (scan.Status == ImageScanStatus.Failed)
            {
                return new ContainerImageScanResult
                {
                    Counters = new VulnerabilityCounters[0],
                    Description = scan.Description,
                    Image = imageTag,
                    ScanResult = ScanResult.Failed,
                    Date = scan.Date,
                };
            }

            // if it's succeeded - query and process found CVEs
            return await processCVEs(scan);
        }

        private async Task<ContainerImageScanResult> ParseCveDetails(ImageScanResultEntity scan)
        {
            var cves = await this.db.Set<ImageScanToCveEntity>()
                .Include("CVE")
                .AsNoTracking()
                .Where(i => i.ScanId == scan.Id)
                .Select(i => new { i.Target, i.UsedPackage, i.UsedPackageVersion, i.CVE.CveId, i.CVE.Severity, i.CVE.PackageName, i.CVE.Description, i.CVE.Title, i.CVE.References, i.CVE.Remediation })
                .ToArrayAsync();

            var targets = new List<ImageScanTarget>();
            foreach (var target in cves.GroupBy(i => i.Target))
            {
                var vulnerabilities = new List<VulnerabilityDescription>();
                foreach (var groupedByCve in target.GroupBy(i => i.CveId))
                {
                    var cve = groupedByCve.First();
                    vulnerabilities.Add(new VulnerabilityDescription
                    {
                        VulnerabilityID = cve.CveId,
                        PkgName = cve.PackageName,
                        Title = cve.Title,
                        Severity = cve.Severity.ToString(),
                        Description = cve.Description,
                        DependenciesWithCVE = groupedByCve.Select(i => i.UsedPackage).ToArray(),
                        InstalledVersion = cve.UsedPackageVersion,
                        Remediation = cve.Remediation,
                        References = string.IsNullOrEmpty(cve.References)
                            ? new string[0]
                            : cve.References.Split(TrivyAuditProcessor.LineSeparator).Where(i => !string.IsNullOrWhiteSpace(i)).ToArray(),
                    });
                }

                targets.Add(new ImageScanTarget
                {
                    Target = target.Key,
                    Vulnerabilities = vulnerabilities.ToArray(),
                });
            }

            return new ContainerImageScanResult
            {
                Image = scan.ImageTag,
                ScanResult = ScanResult.Succeeded,
                Counters = new VulnerabilityCounters[0],
                Targets = targets.ToArray(),
                Date = scan.Date,
            };
        }

        private async Task<ContainerImageScanResult> CalculateCounters(ImageScanResultEntity scan)
        {
            var cves = await this.db.Set<ImageScanToCveEntity>()
                .Include("CVE")
                .AsNoTracking()
                .Where(i => i.ScanId == scan.Id)
                .Select(i => new { i.CVE.Id, i.CVE.Severity })
                .ToArrayAsync();

            var counters = cves
                .GroupBy(i => i.Severity)
                .Select(i => new VulnerabilityCounters() { Count = i.Count(), Severity = i.Key.ToString() })
                .ToArray();

            return new ContainerImageScanResult
            {
                Image = scan.ImageTag,
                ScanResult = ScanResult.Succeeded,
                Counters = counters,
                Date = scan.Date,
            };
        }
    }
}
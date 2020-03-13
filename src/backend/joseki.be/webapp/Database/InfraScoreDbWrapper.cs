using System;
using System.Linq;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using webapp.Models;

namespace webapp.Database
{
    /// <summary>
    /// Takes care of database related operations used by InfrastructureScoreCache.
    /// </summary>
    public class InfraScoreDbWrapper : IInfraScoreDbWrapper
    {
        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfraScoreDbWrapper"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        public InfraScoreDbWrapper(JosekiDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc />
        public async Task<string[]> GetAllComponentsIds()
        {
            var oneMonthAgo = DateTime.UtcNow.Date.AddDays(-30);
            var ids = await this.db.Set<AuditEntity>()
                .AsNoTracking()
                .Where(i => i.Date >= oneMonthAgo)
                .Select(i => i.ComponentId)
                .Distinct()
                .ToArrayAsync();

            return ids;
        }

        /// <inheritdoc />
        public async Task<AuditEntity[]> GetLastMonthAudits(string componentId)
        {
            var oneMonthAgo = DateTime.UtcNow.Date.AddDays(-30);
            var audits = await this.db.Set<AuditEntity>()
                .Include(i => i.InfrastructureComponent)
                .AsNoTracking()
                .Where(i => i.Date >= oneMonthAgo && i.ComponentId == componentId)
                .ToArrayAsync();

            return audits
                .GroupBy(i => i.Date.Date)
                .Select(i => i.OrderByDescending(a => a.Date).First())
                .ToArray();
        }

        /// <inheritdoc />
        public async Task<AuditEntity> GetAudit(string componentId, DateTime date)
        {
            var audit = await this.db.Set<AuditEntity>()
                .Include(i => i.InfrastructureComponent)
                .AsNoTracking()
                .Where(i => i.Date.Date == date.Date && i.ComponentId == componentId)
                .OrderByDescending(i => i.Date)
                .FirstOrDefaultAsync();

            return audit;
        }

        /// <inheritdoc />
        public async Task<AuditEntity[]> GetAudits(DateTime date)
        {
            var theDay = date.Date;
            var theNextDay = theDay.AddDays(1);
            var oneDayAudits = await this.db.Set<AuditEntity>()
                .Include(i => i.InfrastructureComponent)
                .AsNoTracking()
                .Where(i => i.Date >= theDay && i.Date < theNextDay)
                .ToArrayAsync();

            var audits = oneDayAudits
                .GroupBy(i => i.ComponentId)
                .Select(i => i.OrderByDescending(a => a.Date).First())
                .ToArray();

            return audits;
        }

        /// <inheritdoc />
        public async Task<CountersSummary> GetCounterSummariesForAudit(int auditId)
        {
            var checkResults = await this.db.Set<CheckResultEntity>()
                .Include(i => i.Check)
                .AsNoTracking()
                .Where(i => i.AuditId == auditId)
                .Select(i => new
                {
                    i.AuditId,
                    i.Check.Severity,
                    i.Value,
                })
                .ToArrayAsync();

            var summary = new CountersSummary();
            foreach (var checkResult in checkResults)
            {
                switch (checkResult.Value)
                {
                    case CheckValue.Failed:
                        if (checkResult.Severity == CheckSeverity.Critical || checkResult.Severity == CheckSeverity.High)
                        {
                            summary.Failed++;
                        }
                        else
                        {
                            summary.Warning++;
                        }

                        break;
                    case CheckValue.Succeeded:
                        summary.Passed++;
                        break;
                    case CheckValue.InProgress:
                    case CheckValue.NoData:
                        summary.NoData++;
                        break;
                }
            }

            return summary;
        }
    }

    /// <summary>
    /// Interface is extracted to embrace infra-score-cache layer testing.
    /// </summary>
    public interface IInfraScoreDbWrapper
    {
        /// <summary>
        /// Returns all scanner-ids used during last month.
        /// </summary>
        /// <returns>Array of unique scanner identifiers.</returns>
        Task<string[]> GetAllComponentsIds();

        /// <summary>
        /// Returns latest audit entities for each day during the last month.
        /// If there is several audits for the same day - methods returns only the last one.
        /// </summary>
        /// <param name="componentId">Scanner identifier to get audits for.</param>
        /// <returns>Array of latest audits per day.</returns>
        Task<AuditEntity[]> GetLastMonthAudits(string componentId);

        /// <summary>
        /// Returns latest audit entity for requested day.
        /// If there is several audits for the same day - methods returns only the last one.
        /// </summary>
        /// <param name="componentId">Scanner identifier to get audits for.</param>
        /// <param name="date">Audit date.</param>
        /// <returns>Latest audits for the day.</returns>
        Task<AuditEntity> GetAudit(string componentId, DateTime date);

        /// <summary>
        /// Returns latest audits for each scanner for requested day.
        /// If there is several audits for the same day - methods returns only the latest one per scanner.
        /// </summary>
        /// <param name="date">Audits date.</param>
        /// <returns>Latest audits for each scanner for the day.</returns>
        Task<AuditEntity[]> GetAudits(DateTime date);

        /// <summary>
        /// Calculate Counter Summary for requested audits.
        /// </summary>
        /// <param name="auditId">Audit identifier to calculate summaries.</param>
        /// <returns>counters-summary for requested audit-id.</returns>
        Task<CountersSummary> GetCounterSummariesForAudit(int auditId);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using webapp.Database;
using webapp.Database.Cache;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-overall-infrastructure-diff request.
    /// </summary>
    public class GetInfrastructureOverviewDiffHandler
    {
        private readonly IJosekiDatabase db;
        private readonly IInfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInfrastructureOverviewDiffHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        /// <param name="cache">Score cache.</param>
        public GetInfrastructureOverviewDiffHandler(IJosekiDatabase db, IInfrastructureScoreCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns overall infrastructure overview objects for two dates.
        /// </summary>
        /// <param name="date1">The first date to calculate overview.</param>
        /// <param name="date2">The second date to calculate overview.</param>
        /// <param name="filterComponentIds">list of component id's that current user can access.</param>
        /// <returns>Infrastructure overview diff.</returns>
        public async Task<InfrastructureOverviewDiff> GetDiff(DateTime date1, DateTime date2, List<string> filterComponentIds = null)
        {
            var infra1 = await this.GetInfrastructureOverview(date1, filterComponentIds);
            var infra2 = await this.GetInfrastructureOverview(date2, filterComponentIds);

            return new InfrastructureOverviewDiff
            {
                Components1 = infra1.Components,
                Components2 = infra2.Components,
                Overall1 = infra1.Overall,
                Overall2 = infra2.Overall,
            };
        }

        private async Task<InfrastructureOverview> GetInfrastructureOverview(DateTime date, List<string> filterComponentIds = null)
        {
            var audits = await this.db.GetAuditedComponentsWithHistory(date);

            if (filterComponentIds != null)
            {
                audits = audits.Where(x => filterComponentIds.Contains(x.ComponentId)).ToArray();
            }

            return await this.cache.GetInfrastructureOverview(date, audits);
        }
    }
}
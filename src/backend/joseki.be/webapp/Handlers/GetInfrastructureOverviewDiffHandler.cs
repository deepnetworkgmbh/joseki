using System;
using System.Threading.Tasks;

using webapp.Database;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-overall-infrastructure-diff request.
    /// </summary>
    public class GetInfrastructureOverviewDiffHandler
    {
        private readonly IJosekiDatabase db;
        private readonly InfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInfrastructureOverviewDiffHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        /// <param name="cache">Score cache.</param>
        public GetInfrastructureOverviewDiffHandler(IJosekiDatabase db, InfrastructureScoreCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns overall infrastructure overview objects for two dates.
        /// </summary>
        /// <param name="date1">The first date to calculate overview.</param>
        /// <param name="date2">The second date to calculate overview.</param>
        /// <returns>Infrastructure overview diff.</returns>
        public async Task<InfrastructureOverviewDiff> GetDiff(DateTime date1, DateTime date2)
        {
            var infra1 = await this.GetInfrastructureOverview(date1);
            var infra2 = await this.GetInfrastructureOverview(date2);

            return new InfrastructureOverviewDiff
            {
                Components1 = infra1.Components,
                Components2 = infra2.Components,
                Overall1 = infra1.Overall,
                Overall2 = infra2.Overall,
            };
        }

        private async Task<InfrastructureOverview> GetInfrastructureOverview(DateTime date)
        {
            var audits = await this.db.GetAuditedComponents(date);

            return await this.cache.GetInfrastructureOverview(date, audits);
        }
    }
}
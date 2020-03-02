using System;
using System.Threading.Tasks;

using webapp.Database;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-overall-infrastructure request.
    /// </summary>
    public class GetInfrastructureOverviewHandler
    {
        private readonly IJosekiDatabase db;
        private readonly InfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInfrastructureOverviewHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        /// <param name="cache">Score cache.</param>
        public GetInfrastructureOverviewHandler(IJosekiDatabase db, InfrastructureScoreCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns overall infrastructure overview object.
        /// </summary>
        /// <param name="date">date to calculate overview.</param>
        /// <returns>Infrastructure overview.</returns>
        public async Task<InfrastructureOverview> GetOverview(DateTime date)
        {
            var audits = await this.db.GetAuditedComponents(date);

            return await this.cache.GetInfrastructureOverview(date, audits);
        }
    }
}
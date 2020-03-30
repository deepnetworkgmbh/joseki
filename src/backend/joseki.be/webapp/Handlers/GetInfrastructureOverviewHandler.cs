using System;
using System.Threading.Tasks;

using webapp.Database;
using webapp.Database.Cache;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-overall-infrastructure request.
    /// </summary>
    public class GetInfrastructureOverviewHandler
    {
        private readonly IJosekiDatabase db;
        private readonly IInfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInfrastructureOverviewHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database.</param>
        /// <param name="cache">Score cache.</param>
        public GetInfrastructureOverviewHandler(IJosekiDatabase db, IInfrastructureScoreCache cache)
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
            var audits = await this.db.GetAuditedComponentsWithHistory(date);

            return await this.cache.GetInfrastructureOverview(date, audits);
        }
    }
}
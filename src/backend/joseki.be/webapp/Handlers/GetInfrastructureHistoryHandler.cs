using System.Linq;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using webapp.Database;
using webapp.Database.Models;
using webapp.Exceptions;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-infrastructure-history request.
    /// </summary>
    public class GetInfrastructureHistoryHandler
    {
        private readonly JosekiDbContext db;
        private readonly IInfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInfrastructureHistoryHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        /// <param name="cache">Score cache.</param>
        public GetInfrastructureHistoryHandler(JosekiDbContext db, IInfrastructureScoreCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Returns infrastructure history for requested component.
        /// </summary>
        /// <param name="componentId">Component identifier.</param>
        /// <returns>Infrastructure history.</returns>
        public async Task<InfrastructureComponentSummaryWithHistory[]> GetHistory(string componentId)
        {
            string componentName;
            if (componentId == Audit.OverallId)
            {
                componentName = Audit.OverallName;
            }
            else
            {
                componentName = await this.db.Set<InfrastructureComponentEntity>()
                    .Where(i => i.ComponentId == componentId)
                    .Select(i => i.ComponentName)
                    .FirstOrDefaultAsync();

                if (componentName == null)
                {
                    throw new ComponentNotFoundException($"There is no components with id {componentId}");
                }
            }

            return await this.cache.GetInfrastructureHistory(componentId, componentName);
        }
    }
}
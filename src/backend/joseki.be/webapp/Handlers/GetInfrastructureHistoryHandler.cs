using System.Threading.Tasks;

using webapp.Database;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Prepares response to get-infrastructure-history request.
    /// </summary>
    public class GetInfrastructureHistoryHandler
    {
        private readonly InfrastructureScoreCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInfrastructureHistoryHandler"/> class.
        /// </summary>
        /// <param name="cache">Score cache.</param>
        public GetInfrastructureHistoryHandler(InfrastructureScoreCache cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Returns infrastructure history for requested component.
        /// </summary>
        /// <param name="componentId">Component identifier.</param>
        /// <returns>Infrastructure history.</returns>
        public async Task<InfrastructureComponentSummaryWithHistory[]> GetHistory(string componentId)
        {
            return await this.cache.GetInfrastructureHistory(componentId);
        }
    }
}
using System;

namespace webapp.Models
{
    /// <summary>
    /// Returns infrastructure summary: scores by category, counters, etc.
    /// </summary>
    public class InfrastructureOverviewDiff
    {
        /// <summary>
        /// First overall infrastructure summary.
        /// </summary>
        public InfrastructureComponentSummary Overall1 { get; set; }

        /// <summary>
        /// Second overall infrastructure summary.
        /// </summary>
        public InfrastructureComponentSummary Overall2 { get; set; }

        /// <summary>
        /// Components of first summary.
        /// </summary>
        public InfrastructureComponentSummary[] Components1 { get; set; }

        /// <summary>
        /// Components of second summary.
        /// </summary>
        public InfrastructureComponentSummary[] Components2 { get; set; }
    }
}

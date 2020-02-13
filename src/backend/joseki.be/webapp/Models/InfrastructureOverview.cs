namespace webapp.Models
{
    /// <summary>
    /// Returns infrastructure summary: scores by category, counters, etc.
    /// </summary>
    public class InfrastructureOverview
    {
        /// <summary>
        /// Overall infrastructure summary.
        /// </summary>
        public InfrastructureComponentSummaryWithHistory Overall { get; set; }

        /// <summary>
        /// Separate summary for each involved component.
        /// </summary>
        public InfrastructureComponentSummaryWithHistory[] Components { get; set; }
    }
}
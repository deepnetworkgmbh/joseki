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
        public InfrastructureComponentSummary Overall { get; set; }

        /// <summary>
        /// Separate summary for each involved component.
        /// </summary>
        public InfrastructureComponentSummary[] Components { get; set; }
    }
}
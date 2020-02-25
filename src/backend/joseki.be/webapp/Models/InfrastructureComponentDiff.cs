using System;

namespace webapp.Models
{
    /// <summary>
    /// Returns infrastructure summary diff.
    /// </summary>
    public class InfrastructureComponentDiff
    {
        /// <summary>
        /// First Component infrastructure summary.
        /// </summary>
        public InfrastructureComponentSummary Summary1 { get; set; }

        /// <summary>
        /// Second Component infrastructure summary.
        /// </summary>
        public InfrastructureComponentSummary Summary2 { get; set; }
    }
}

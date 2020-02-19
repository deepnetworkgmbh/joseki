using System;

namespace webapp.Models
{
    /// <summary>
    /// Summarizes a single infrastructure unit stats: score, trends, name.
    /// </summary>
    public class InfrastructureComponentSummary
    {
        /// <summary>
        /// The generation date of the Summary.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The component of the current summary.
        /// </summary>
        public InfrastructureComponent Component { get; set; }

        /// <summary>
        /// Latest known check-result counters.
        /// </summary>
        public CountersSummary Current { get; set; }

        /// <summary>
        /// List of every check in component.
        /// </summary>
        public Check[] Checks { get; set; }
    }

    /// <summary>
    /// Summarizes a single infrastructure unit stats: score, trends, name.
    /// </summary>
    public class InfrastructureComponentSummaryWithHistory : InfrastructureComponentSummary
    {
        /// <summary>
        /// Holds Scores per last 30 days.
        /// If no data for a day - places 0.
        /// </summary>
        public ScoreHistoryItem[] ScoreHistory { get; set; }

        /// <summary>
        /// Pre-calculated parameters for drawing trend line.
        /// </summary>
        public Trend ScoreTrend { get; set; }
    }
}
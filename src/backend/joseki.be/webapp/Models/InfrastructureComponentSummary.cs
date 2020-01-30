using System;

namespace webapp.Models
{
    /// <summary>
    /// Summarizes a single infrastructure unit stats: score, trends, name.
    /// </summary>
    public class InfrastructureComponentSummary
    {
        /// <summary>
        /// The name of the component: dev-cluster, subscription-1, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The bucket of infrastructure component: Cloud Subscription, Kubernetes cluster, etc.
        /// </summary>
        public InfrastructureCategory Category { get; set; }

        /// <summary>
        /// Latest known check-result counters.
        /// </summary>
        public CountersSummary Current { get; set; }

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

    /// <summary>
    /// Represents the score of component at a given date.
    /// </summary>
    public class ScoreHistoryItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreHistoryItem"/> class with default values.
        /// </summary>
        public ScoreHistoryItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreHistoryItem"/> class with provided Date and Score values.
        /// </summary>
        /// <param name="date">The date of the score record.</param>
        /// <param name="score">The score value.</param>
        public ScoreHistoryItem(DateTime date, short score)
        {
            this.RecordedAt = date;
            this.Score = score;
        }

        /// <summary>
        /// The date of the Score.
        /// </summary>
        public DateTime RecordedAt { get; set; }

        /// <summary>
        /// Score value.
        /// </summary>
        public short Score { get; set; }
    }

    /// <summary>
    /// Holds properties to draw a straight trend line.
    /// Current version is based on https://math.stackexchange.com/a/204021
    /// TODO: figure out the right trend formula. (base on diffs, moving-averages?).
    /// </summary>
    public class Trend
    {
        /// <summary>
        /// The slope of the trend line.
        /// </summary>
        public float Slope { get; set; }

        /// <summary>
        /// The offset of the trend line.
        /// </summary>
        public float Offset { get; set; }

        /// <summary>
        /// Calculates Trend parameters for subset of score values.
        /// </summary>
        /// <param name="values">The score values.</param>
        /// <returns>Trend object.</returns>
        public static Trend GetTrend(ScoreHistoryItem[] values)
        {
            var trend = new Trend();

            // if no values, trend is the origin - (0,0).
            if (values == null || values.Length == 0)
            {
                return trend;
            }

            var n = values.Length;
            float sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
            var now = DateTime.UtcNow;

            for (int i = 0; i < values.Length; i++)
            {
                var x = 30 - (now - values[i].RecordedAt).Days;
                var y = values[i].Score;

                sumX += x;
                sumXX += x * x;
                sumY += y;
                sumXY += x * y;
            }

            trend.Slope = ((n * sumXY) - (sumX * sumY)) / ((n * sumXX) - (sumX * sumX));
            trend.Offset = (sumY - (trend.Slope * sumX)) / n;

            return trend;
        }
    }
}
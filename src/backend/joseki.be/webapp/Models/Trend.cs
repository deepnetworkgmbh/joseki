using System;

namespace webapp.Models
{
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

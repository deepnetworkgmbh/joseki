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
        public short[] ScoreHistory { get; set; }

        /// <summary>
        /// Pre-calculated parameters for drawing trend line.
        /// </summary>
        public Trend ScoreTrend { get; set; }
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
        public static Trend GetTrend(short[] values)
        {
            var trend = new Trend();

            // if no values, trend is the origin - (0,0).
            if (values == null || values.Length == 0)
            {
                return trend;
            }

            var n = values.Length;
            float sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;

            for (int i = 0; i < values.Length; i++)
            {
                var x = i;
                var y = values[i];

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
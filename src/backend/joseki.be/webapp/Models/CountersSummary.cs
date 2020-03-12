using System;

namespace webapp.Models
{
    /// <summary>
    /// Represents short summary of check-result counters.
    /// </summary>
    public class CountersSummary
    {
        /// <summary>
        /// Amount of Passed checks.
        /// </summary>
        public int Passed { get; set; }

        /// <summary>
        /// Amount of failed checks.
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// Amount of Warnings.
        /// </summary>
        public int Warning { get; set; }

        /// <summary>
        /// Amount of checks with no-data result: requires a manual verification or Joseki is not able to perform the check.
        /// </summary>
        public int NoData { get; set; }

        /// <summary>
        /// Total checks amount.
        /// </summary>
        public int Total => this.Passed + this.Failed + this.Warning + this.NoData;

        /// <summary>
        /// The audit score. It indicates how close the infrastructure is to known best-practices configuration.
        /// NoData checks are excluded, Passed and Failed has doubled weight.
        /// The final formula is Passed*2/(Failed*2 + Passed*2 + Warning).
        /// </summary>
        public int Score =>
            this.Failed == 0 && this.Passed == 0 && this.Warning == 0
                ? 0
                : Convert.ToInt16(Math.Round(
                    100M * this.Passed * 2 / ((this.Failed * 2) + (this.Passed * 2) + this.Warning),
                    MidpointRounding.AwayFromZero));

        /// <summary>
        /// Adds counters from another Summary to the current one.
        /// </summary>
        /// <param name="another">counters to add.</param>
        public void Add(CountersSummary another)
        {
            this.NoData += another.NoData;
            this.Warning += another.Warning;
            this.Failed += another.Failed;
            this.Passed += another.Passed;
        }
    }
}
using System;

namespace webapp.Models
{
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
        public ScoreHistoryItem(DateTime date, int score)
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
        public int Score { get; set; }
    }
}

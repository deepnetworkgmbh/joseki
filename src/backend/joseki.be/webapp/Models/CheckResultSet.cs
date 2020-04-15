using System;

namespace webapp.Models
{
    /// <summary>
    /// Model for returning check resultset on overview detail.
    /// </summary>
    public class CheckResultSet
    {
        /// <summary>
        /// List of checks in resultset.
        /// </summary>
        public OverviewCheck[] Checks { get; set; }

        /// <summary>
        /// Pagination size of the resultset.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Current pagination index of the resultset.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Current resultset sorted by.
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Current resultset filtered by.
        /// </summary>
        public string FilterBy { get; set; }

        /// <summary>
        /// Number of total results in the result set.
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// Error message for the query.
        /// </summary>
        public string Error { get; set; }
    }
}
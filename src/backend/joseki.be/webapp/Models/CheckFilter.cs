using System;

namespace webapp.Models
{
    /// <summary>
    /// A view model for overview scan result page to return autocomplete items.
    /// </summary>
    public class CheckFilter
    {
        /// <summary>
        /// Name of the filter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Handles the count of subsequent searches.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckFilter"/> class.
        /// </summary>
        /// <param name="name">name of the filter.</param>
        /// <param name="count">handles the count of subsequent searches.</param>
        public CheckFilter(string name, int count)
        {
            this.Name = name;
            this.Count = count;
        }
    }
}

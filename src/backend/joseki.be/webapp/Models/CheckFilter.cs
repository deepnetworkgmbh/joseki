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
        /// Handles if it exist on subsequent searches.
        /// </summary>
        public bool FilteredOut { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckFilter"/> class.
        /// </summary>
        /// <param name="name">name of the filter.</param>
        /// <param name="filteredOut">does the filter exist on subsequent searches.</param>
        public CheckFilter(string name, bool filteredOut)
        {
            this.Name = name;
            this.FilteredOut = filteredOut;
        }
    }
}

using System;

namespace webapp.Models
{
    /// <summary>
    /// Collection type and name of a check.
    /// (eg: namespace: default, resource group: common).
    /// </summary>
    public class Collection
    {
        /// <summary>
        /// Type of the collection.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Name of the collection.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Collection"/> class.
        /// collection constructor.
        /// </summary>
        /// <param name="type">type of collection.</param>
        /// <param name="name">name of collection.</param>
        public Collection(string type, string name)
        {
            this.Type = type;
            this.Name = name;
        }
    }
}

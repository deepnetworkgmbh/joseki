using System;

namespace webapp.Models
{
    /// <summary>
    /// Resource of a check.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Id of the resource.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the resource.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Owner of the resource.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// Resource constructor.
        /// </summary>
        /// <param name="id">id of Resource.</param>
        /// <param name="type">type of Resource.</param>
        /// <param name="name">name of Resource.</param>
        /// <param name="owner">owner email of Resource.</param>
        public Resource(string type, string name, string id = "", string owner = "")
        {
            this.Id = (id == string.Empty) ? Guid.NewGuid().ToString() : id;
            this.Name = name;
            this.Type = type;
            this.Owner = owner;
        }
    }
}

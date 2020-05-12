using System;
using joseki.db.entities;

namespace webapp.Database.Models
{
    /// <summary>
    /// Relation data to track down the ownership of a component.
    /// </summary>
    public class OwnershipEntity : IJosekiBaseEntity
    {
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public DateTime DateUpdated { get; set; }

        /// <inheritdoc />
        public DateTime DateCreated { get; set; }

        /// <inheritdoc />
        public string ChangedBy { get; set; }

        /// <summary>
        /// The component Id (path) of the ownership.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// The owner.
        /// </summary>
        public string Owner { get; set; }
    }
}

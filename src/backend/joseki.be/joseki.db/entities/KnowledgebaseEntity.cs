using System;

namespace joseki.db.entities
{
    /// <summary>
    /// The entity represents a single Knowledge-base item.
    /// </summary>
    public class KnowledgebaseEntity : IJosekiBaseEntity
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
        /// External knowledge-base identifier.
        /// the id consist of a several sections separated with '/'.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// The knowledge-base item content.
        /// </summary>
        public string Content { get; set; }
    }
}
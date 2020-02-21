using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace joseki.db.entities
{
    /// <summary>
    /// The base interface for EF core entities.
    /// </summary>
    public interface IJosekiBaseEntity
    {
        /// <summary>
        /// Auto-increment PK column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Set to current UTC time every time when the record is changed.
        /// </summary>
        public DateTime DateUpdated { get; set; }

        /// <summary>
        /// UTC time, when the record was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Name of the actor who requested the record creation/change.
        /// </summary>
        public string ChangedBy { get; set; }
    }
}
using System;

namespace joseki.db.entities
{
    /// <summary>
    /// The entity represents Infrastructure Component object.
    /// </summary>
    public class InfrastructureComponentEntity : IJosekiBaseEntity
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
        /// Which scanner is responsible for auditing.
        /// </summary>
        public string ScannerId { get; set; }

        /// <summary>
        /// External infrastructure component identifier: k8s-cluster id or azure-subscription-id.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Human-friendly infrastructure component name.
        /// </summary>
        public string ComponentName { get; set; }
    }
}
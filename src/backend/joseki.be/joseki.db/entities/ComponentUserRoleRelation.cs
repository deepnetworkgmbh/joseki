using System;
using System.ComponentModel.DataAnnotations;

namespace joseki.db.entities
{
    /// <summary>
    /// Holds the Component - User - Role relation information.
    /// </summary>
    public class ComponentUserRoleRelation
    {
        /// <summary>
        /// Id of the record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The id of the user who has the role on component
        /// provided from Azure AD.
        /// </summary>
        [MaxLength(36)]
        public string UserId { get; set;  }

        /// <summary>
        /// The id of the app role.
        /// </summary>
        [MaxLength(36)]
        public string RoleId { get; set; }

        /// <summary>
        /// The Id of the component.
        /// </summary>
        [MaxLength(36)]
        public string ComponentId { get; set; }
    }
}

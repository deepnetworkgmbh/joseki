using System;
using System.Collections.Generic;
using joseki.db.entities;

namespace webapp.Models
{
#pragma warning disable CS1591
    /// <summary>
    /// Infrastructure component with user role definition.
    /// </summary>
    public class ComponentsWithRoles
    {
        /// <summary>
        /// Id of the component.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Assigned user roles.
        /// </summary>
        public List<UserRolePair> UserRoles { get; set; }
    }

    /// <summary>
    /// User-Role pair to be used within ComponentsWithRoles.
    /// </summary>
    public class UserRolePair
    {
        public string UserId { get; set; }

        public string RoleId { get; set; }
    }
#pragma warning restore CS1591

}

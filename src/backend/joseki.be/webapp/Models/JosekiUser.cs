using System;
using System.Collections.Generic;

namespace webapp.Authentication
{
    /// <summary>
    /// Active Directory User.
    /// </summary>
    public class JosekiUser
    {
        /// <summary>
        /// Id of user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User's joseki app roles.
        /// </summary>
        public List<string> AppRoles { get; set; }
    }
}

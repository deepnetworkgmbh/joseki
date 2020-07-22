using System;
using System.Collections.Generic;
using Microsoft.Graph;

namespace webapp.Authentication
{
    /// <summary>
    /// Container for static Joseki App roles.
    /// </summary>
    public static class JosekiAppRoles
    {
        /// <summary>
        /// Admin role, has access to all components.
        /// </summary>
        public static JosekiAppRole AdminRole
        {
            get
            {
                return new JosekiAppRole
                {
                    Id = "c20e145e-5459-4a6c-a074-b942bbd4cfe1",
                    Name = "JosekiAdmin",
                };
            }
        }

        /// <summary>
        /// Reader role, has only access to assigned components.
        /// </summary>
        public static JosekiAppRole ReaderRole
        {
            get
            {
                return new JosekiAppRole
                {
                    Id = "1b4f816e-5eaf-48b9-8613-7923830595a1",
                    Name = "JosekiReader",
                };
            }
        }

        /// <summary>
        /// Returns all list of app roles.
        /// Role Guids are staticly defined and being used in App registration on Azure AD.
        /// </summary>
        public static List<JosekiAppRole> GetJosekiAppRoles()
        {
            return new List<JosekiAppRole>
            {
                AdminRole,
                ReaderRole,
            };
        }

        /// <summary>
        /// Returns list of user's app roles.
        /// </summary>
        public static List<string> GetUserRoles(IUserAppRoleAssignmentsCollectionPage roles)
        {
            var result = new List<string>();
            var staticRoles = GetJosekiAppRoles();
            foreach (var staticRole in staticRoles)
            {
                foreach (var role in roles)
                {
                    if (staticRole.Id == role.AppRoleId.ToString())
                    {
                        result.Add(staticRole.Name);
                    }
                }
            }

            return result;
        }
    }
}
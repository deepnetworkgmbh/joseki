using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using joseki.db;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace webapp.Authentication
{
    /// <summary>
    /// User access control handler.
    /// </summary>
    public class UserAccessControlHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;
        private readonly bool authEnabled;
        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAccessControlHandler"/> class.
        /// </summary>
        public UserAccessControlHandler(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, JosekiDbContext db)
        {
            this.configuration = configuration;
            var authEnabledConfiguration = this.configuration["DEV_JOSEKI_AUTH_ENABLED"];
            this.authEnabled = authEnabledConfiguration != null && bool.Parse(authEnabledConfiguration) == true;
            this.httpContextAccessor = httpContextAccessor;
            this.db = db;
        }

        /// <summary>
        /// Returns if Auth is enabled.
        /// </summary>
        public bool IsAuthEnabled()
        {
            return this.authEnabled;
        }

        /// <summary>
        /// Returns the unique identifier of the user in the context.
        /// </summary>
        public string GetUid()
        {
            var objectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            var claims = this.httpContextAccessor.HttpContext.User.Claims;
            return claims.FirstOrDefault(s => s.Type == objectIdentifier)?.Value;
        }

        /// <summary>
        /// Returns the roles of the user in the context.
        /// </summary>
        public List<string> GetRoles()
        {
            var roleIdentifier = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            return this.httpContextAccessor
                       .HttpContext
                       .User
                       .Claims
                       .Where(s => s.Type == roleIdentifier)
                       .Select(c => c.Value)
                       .ToList();
        }

        /// <summary>
        /// Must be called when user has JosekiReader role,
        /// If the user has JosekiAdmin role , filtering is not used.
        /// Returns list of component Id's filtered with the assigned permissions.
        /// </summary>
        public async Task<List<string>> GetFilteredComponentIds()
        {
            var result = new List<string>();
            var userid = this.GetUid();
            var userAssignedComponentIdList = await this.db.ComponentUserRoleRelations
                                                  .Where(x => x.UserId == userid)
                                                  .Select(x => x.ComponentId)
                                                  .ToArrayAsync();

            return await this.db.InfrastructureComponent
                                      .Where(x => userAssignedComponentIdList.Contains(x.Id))
                                      .Select(x => x.ComponentId)
                                      .ToListAsync();
        }
    }
}

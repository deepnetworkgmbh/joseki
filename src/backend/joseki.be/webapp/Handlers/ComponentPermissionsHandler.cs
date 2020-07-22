using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using joseki.db;
using joseki.db.entities;
using Microsoft.EntityFrameworkCore;
using webapp.Authentication;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Returns the list of components joint with the granular user roles.
    /// </summary>
    public class ComponentPermissionsHandler
    {
        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentPermissionsHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        public ComponentPermissionsHandler(JosekiDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Returns list of components with user roles.
        /// </summary>
        public async Task<List<ComponentsWithRoles>> GetUserPermissionsOnComponents()
        {
            // get list of infrastructure components
            return await this.db.Set<InfrastructureComponentEntity>()
                            .AsNoTracking()
                            .Select(c => new ComponentsWithRoles
                            {
                                Id = c.Id,
                                Name = c.ComponentName,
                                UserRoles = this.db.ComponentUserRoleRelations
                                                .Where(x => x.ComponentId == c.Id)
                                                .Select(r => new UserRolePair
                                                {
                                                    UserId = r.UserId,
                                                    RoleId = r.RoleId,
                                                }).ToList(),
                            })
                            .ToListAsync();
        }

        /// <summary>
        /// Updates component-user-role relations.
        /// </summary>
        /// <param name="update">List of updated components with new user-role definition.</param>
        public async Task<bool> SetUserPermissionsOnComponents(List<ComponentsWithRoles> update)
        {
            // check for updates and additions
            foreach (var component in update)
            {
                foreach (var userrole in component.UserRoles)
                {
                    var record = this.db
                                     .ComponentUserRoleRelations
                                     .FirstOrDefault(x => x.ComponentId == component.Id && x.UserId == userrole.UserId);

                    // there was no permission before, add.
                    if (record == null)
                    {
                        this.db.ComponentUserRoleRelations.Add(new ComponentUserRoleRelation
                        {
                            ComponentId = component.Id,
                            UserId = userrole.UserId,
                            RoleId = userrole.RoleId,
                        });
                        await this.db.SaveChangesAsync();
                    }

                    // this record exists.
                    else
                    {
                        // update if any change occured.
                        if (record.RoleId != userrole.RoleId)
                        {
                            record.RoleId = userrole.RoleId;
                            await this.db.SaveChangesAsync();
                        }
                    }
                }
            }

            // check for removals
            foreach (var record in this.db.ComponentUserRoleRelations)
            {
                var componentOnUpdate = update.FirstOrDefault(x => x.Id == record.ComponentId);
                if (componentOnUpdate == null)
                {
                    // TODO: component not available, remove permissions?
                    continue;
                }

                var existsOnUpdate = componentOnUpdate.UserRoles.FirstOrDefault(x => x.UserId == record.UserId && x.RoleId == record.RoleId);
                if (existsOnUpdate == null)
                {
                    // the record does not exist on update, remove.
                    this.db.ComponentUserRoleRelations.Remove(record);
                    await this.db.SaveChangesAsync();
                }
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Serilog;
using webapp.Authentication;
using webapp.Configuration;
using webapp.Handlers;
using webapp.Models;

namespace webapp.Controllers.v0_1
{
    /// <summary>
    /// Azure AD endpoints.
    /// </summary>
    [ApiController]
    [ApiVersion("0.1")]
    [Route("api/accesscontrol")]
    [Authorize(Roles = "JosekiAdmin")]
    public class AccessControlController : Controller
    {
        private static readonly ILogger Logger = Log.ForContext<AccessControlController>();

        private readonly JosekiConfiguration configuration;
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessControlController"/> class.
        /// </summary>
        /// <param name="config">ConfigurationParser.</param>
        /// <param name="services">DI container.</param>
        public AccessControlController(ConfigurationParser config, IServiceProvider services)
        {
            this.configuration = config.Get();
            this.services = services;
        }

        /// <summary>
        /// Return all users in an active directory.
        /// </summary>
        [HttpGet]
        [Route("users", Name = "get-users")]
        [ProducesResponseType(200, Type = typeof(List<JosekiUser>))]
        public async Task<ObjectResult> GetAllUsers()
        {
            var users = await this.GetUsers();
            return this.StatusCode(200, users);
        }

        /// <summary>
        /// Return all components with user permissions.
        /// </summary>
        [HttpGet]
        [Route("components", Name = "get-components")]
        [ProducesResponseType(200, Type = typeof(List<InfrastructureComponent>))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetAllComponents()
        {
            try
            {
                var handler = this.services.GetService<ComponentPermissionsHandler>();
                var users = await this.GetUsers(true);
                var components = await handler.GetUserPermissionsOnComponents();
                var roles = JosekiAppRoles.GetJosekiAppRoles();

                var result = new
                {
                    Users = users,
                    Components = components,
                    Roles = roles,
                };

                return this.StatusCode(200, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get component permissions");
                return this.StatusCode(500, $"Failed to get component permissions");
            }
        }

        /// <summary>
        /// Set user/component/role relations.
        /// </summary>
        [HttpPost]
        [Route("setPermissions", Name = "set-permissions")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> SetPermissions(List<ComponentsWithRoles> components)
        {
            try
            {
                var handler = this.services.GetService<ComponentPermissionsHandler>();
                var success = await handler.SetUserPermissionsOnComponents(components);
                return this.StatusCode(200, new
                {
                    Success = success,
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get component permissions");
                return this.StatusCode(500, $"Failed to get component permissions");
            }
        }

        /// <summary>
        /// Return list of users in azure ad.
        /// </summary>
        private async Task<List<JosekiUser>> GetUsers(bool onlyWithAccess = false)
        {
            var users = new List<JosekiUser>();
            try
            {
                var confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(this.configuration.AzureAD.ClientId)
                    .WithTenantId(this.configuration.AzureAD.TenantId)
                    .WithClientSecret(this.configuration.AzureAD.ClientSecret)
                    .Build();

                ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
                GraphServiceClient graphClient = new GraphServiceClient(authProvider);

                var result = await graphClient.Users
                    .Request()
                    .Select("Id, displayName, mail")
                    .GetAsync();

                foreach (var userdata in result.CurrentPage)
                {
                    if (string.IsNullOrEmpty(userdata.Mail))
                    {
                        continue;
                    }

                    // get user's app roles
                    var appRoleAssignments = await graphClient.Users[userdata.Id]
                        .AppRoleAssignments
                        .Request()
                        .GetAsync();

                    var josekiRoles = JosekiAppRoles.GetUserRoles(appRoleAssignments);

                    if (onlyWithAccess)
                    {
                        if (josekiRoles.Count == 0)
                        {
                            continue;
                        }
                    }

                    var user = new JosekiUser()
                    {
                        Id = userdata.Id,
                        Name = userdata.DisplayName,
                        Email = userdata.Mail,
                        AppRoles = josekiRoles,
                    };

                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get list of users");
            }

            // TODO: handle list more than 50 users/roles
            return users;
        }
    }
}

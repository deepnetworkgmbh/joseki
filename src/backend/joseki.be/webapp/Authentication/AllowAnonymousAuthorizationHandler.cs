using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace webapp.Handlers
{
    /// <summary>
    /// This authorization handler will bypass all requirements.
    /// </summary>
    public class AllowAnonymousAuthorizationHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization request.
        /// </summary>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (IAuthorizationRequirement requirement in context.PendingRequirements.ToList())
            {
                // Simply pass all requirements
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

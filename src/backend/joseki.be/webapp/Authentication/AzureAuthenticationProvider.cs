using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using webapp.Configuration;

namespace webapp.Authentication
{
    /// <summary>
    /// Azure AD Provider.
    /// </summary>
    public class AzureAuthenticationProvider : IAuthenticationProvider
    {
        private JosekiConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAuthenticationProvider"/> class.
        /// </summary>
        public AzureAuthenticationProvider(JosekiConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Handle authenticate request.
        /// </summary>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            // string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            ClientCredential creds = new ClientCredential(this.configuration.AzureAD.ClientId, this.configuration.AzureAD.ClientSecret);

            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's database
            string authority = this.configuration.AzureAD.Instance + this.configuration.AzureAD.TenantId;
            AuthenticationContext authenticationContext = new AuthenticationContext(authority);
            AuthenticationResult authResult = await authenticationContext.AcquireTokenAsync("https://graph.microsoft.com/", creds);

            request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
        }
    }
}

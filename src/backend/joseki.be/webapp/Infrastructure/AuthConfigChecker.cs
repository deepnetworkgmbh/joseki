using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace webapp.Infrastructure
{
    /// <summary>
    /// This class checks the available configuration and validates
    /// if necessary authentication information is provided.
    /// </summary>
    public static class AuthConfigChecker
    {
        /// <summary>
        /// Check the available configuration.
        /// </summary>
        /// <param name="configuration">App configuration.</param>
        /// <returns>Return true if the application has auth configuration enabled.</returns>
        public static bool Check(IConfiguration configuration)
        {
            try
            {
                var values = new List<string>();
                values.Add(configuration["AzureAd:Instance"]);
                values.Add(configuration["AzureAd:Domain"]);
                values.Add(configuration["AzureAd:TenantId"]);
                values.Add(configuration["AzureAd:ClientId"]);
                values.Add(configuration["AzureAd:ClientSecret"]);
                return !values.Any(x => string.IsNullOrEmpty(x));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
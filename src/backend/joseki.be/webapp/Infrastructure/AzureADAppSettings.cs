using System;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webapp.Configuration;

namespace webapp.Infrastructure
{
    /// <summary>
    /// Configure appsettings.json on runtime because Microsoft.Identity.Web requires it to be configured.
    /// </summary>
    public static class AzureADAppSettings
    {
        /// <summary>
        /// Use JosekiSettings to inject AzureAD settings into appsettings.json.
        /// </summary>
        public static void ConfigureAzureAD(this IConfigurationBuilder builder, HostBuilderContext builderContext, IConfigurationRoot configuration)
        {
            const string azureADKey = "AzureAD";
            var environment = builderContext.HostingEnvironment;
            var fileProvider = environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo("appsettings.json");
            var physicalPath = fileInfo.PhysicalPath;

            var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(physicalPath));

            // Check if Authorization is enabled on configuration.
            // If enabled, inject AzureAD configuration into appSettings.
            var authEnabled = configuration["DEV_JOSEKI_AUTH_ENABLED"];
            if (authEnabled != null && bool.Parse(authEnabled) == true)
            {
                var configFilePath = configuration["JOSEKI_CONFIG_FILE_PATH"];
                var josekiConfig = new ConfigurationParser(configFilePath).Get();
                var azureADConfiguration = JObject.Parse(JsonConvert.SerializeObject(josekiConfig.AzureAD));

                if (!jObject.ContainsKey(azureADKey))
                {
                    jObject.Add(azureADKey, azureADConfiguration);
                }
                else
                {
                    jObject[azureADKey] = azureADConfiguration;
                }
            }
            else
            {
                jObject.Remove(azureADKey);
            }

            File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        }
    }
}
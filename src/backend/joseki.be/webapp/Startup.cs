using System.IO;
using System.Text.Json.Serialization;

using joseki.db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Serilog.Events;

using Swashbuckle.AspNetCore.SwaggerGen;

using webapp.Audits.PostProcessors;
using webapp.Audits.Processors.azsk;
using webapp.Audits.Processors.polaris;
using webapp.Audits.Processors.trivy;
using webapp.Authentication;
using webapp.BackgroundJobs;
using webapp.BlobStorage;
using webapp.Configuration;
using webapp.Database;
using webapp.Database.Cache;
using webapp.Handlers;
using webapp.Infrastructure;
using webapp.Queues;

namespace webapp
{
    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration object.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        private readonly string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

        /// <summary>
        /// The application configuration object.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Services collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: add explicit CORS origins
            services.AddCors(options =>
            {
                options.AddPolicy(
                    this.myAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("*");
                        builder.WithHeaders("*");
                    });
            });
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(0, 1);
            });
            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            services
                .AddHealthChecks()
                .AddCheck("Live", () => JosekiStateManager.Live, new[] { "liveness" })
                .AddCheck("Ready", () => JosekiStateManager.Ready, new[] { "readiness" });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(c =>
            {
                // add a custom operation filter which sets default values
                c.OperationFilter<SwaggerDefaultValues>();

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(System.AppContext.BaseDirectory, xmlFile);

                // Set the comments path for the swagger json and ui.
                c.IncludeXmlComments(xmlPath);
            });

            services.AddSingleton(provider =>
            {
                const string envVarName = "JOSEKI_CONFIG_FILE_PATH";
                var configFilePath = this.Configuration[envVarName];
                return new ConfigurationParser(configFilePath);
            });

            services.AddTransient<IBlobStorageProcessor, AzureBlobStorageProcessor>();
            services.AddTransient<IBlobStorageMaintainer, AzureBlobStorageMaintainer>();
            services.AddTransient<IQueue, AzureStorageQueue>();

            services.AddDbContext<JosekiDbContext>((provider, options) =>
            {
                var config = provider.GetService<ConfigurationParser>().Get();
                options.UseSqlServer(
                    config.Database.ConnectionString,
                    o => o
                        .MigrationsAssembly(typeof(JosekiDbContext).Assembly.GetName().Name)
                        .EnableRetryOnFailure());
            });

            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddScoped<IJosekiDatabase, MssqlJosekiDatabase>();
            services.AddScoped<IInfraScoreDbWrapper, InfraScoreDbWrapper>();
            services.AddTransient<IInfrastructureScoreCache, InfrastructureScoreCache>();
            services.AddTransient<ChecksCache>();
            services.AddTransient<CveCache>();
            services.AddTransient<IOwnershipCache, OwnershipCache>();

            services.AddTransient<AzskAuditProcessor>();
            services.AddTransient<PolarisAuditProcessor>();
            services.AddTransient<TrivyAuditProcessor>();
            services.AddTransient<ExtractOwnershipProcessor>();

            services.AddTransient<GetInfrastructureOverviewHandler>();
            services.AddTransient<GetInfrastructureOverviewDiffHandler>();
            services.AddTransient<GetInfrastructureHistoryHandler>();
            services.AddTransient<GetComponentDetailsHandler>();
            services.AddTransient<GetImageScanHandler>();
            services.AddTransient<GetKnowledgebaseItemsHandler>();
            services.AddTransient<GetOverviewDetailsHandler>();

            services.AddTransient<ComponentPermissionsHandler>();
            services.AddTransient<UserAccessControlHandler>();

            services.AddScoped<ScannerContainersWatchman>();
            services.AddScoped<SchedulerAssistant>();
            services.AddScoped<ArchiveWatchman>();
            services.AddScoped<InfraScoreCacheWatchman>();

            // to be able to access httpContext on service level.
            services.AddHttpContextAccessor();

            var blobsEnabled = this.Configuration["DEV_JOSEKI_BLOB_STORAGE_ENABLED"];
            if (blobsEnabled == null || bool.Parse(blobsEnabled))
            {
                services.AddHostedService<ScannerResultsReaderJob>();
                services.AddHostedService<ArchiverJob>();
            }

            services.AddHostedService<InfraScoreCacheReloaderJob>();

            var authEnabled = this.Configuration["DEV_JOSEKI_AUTH_ENABLED"];
            if (authEnabled != null && bool.Parse(authEnabled) == true)
            {
                services.AddProtectedWebApi(this.Configuration)
                        .AddInMemoryTokenCaches();
            }
            else
            {
                services.AddSingleton<IAuthorizationHandler, AllowAnonymousAuthorizationHandler>();
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Builder object.</param>
        /// <param name="env">Environment configuration.</param>
        /// <param name="provider">API version provider.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                Program.LoggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/health/liveness", CreateHealthCheckOptions("liveness"));
            app.UseHealthChecks("/health/readiness", CreateHealthCheckOptions("readiness"));

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            app.UseMiddleware<HttpRequestLoggingMiddleware>();

            app.UseCors(this.myAllowSpecificOrigins);

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static HealthCheckOptions CreateHealthCheckOptions(string tag)
        {
            return new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains(tag),
            };
        }
    }
}

using System.IO;
using System.Text.Json.Serialization;

using joseki.db;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using webapp.Audits.Processors;
using webapp.Audits.Processors.azsk;
using webapp.Audits.Processors.polaris;
using webapp.Audits.Processors.trivy;
using webapp.BackgroundJobs;
using webapp.BlobStorage;
using webapp.Configuration;
using webapp.Database;
using webapp.Infrastructure;

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
                    builder => { builder.WithOrigins("*"); });
            });
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            services
                .AddHealthChecks()
                .AddCheck("Live", () => HealthCheckResult.Healthy(), new[] { "liveness" })
                .AddCheck("Ready", () => HealthCheckResult.Healthy(), new[] { "readiness" });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Joseki Backend", Version = "v1" });
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

            services.AddDbContext<JosekiDbContext>((provider, options) =>
            {
                var config = provider.GetService<ConfigurationParser>().Get();
                var sqlConnectionString = string.Format(config.Database.ConnectionString, config.Database.Username, config.Database.Password);
                options.UseSqlServer(
                    sqlConnectionString,
                    o => o
                        .MigrationsAssembly(typeof(JosekiDbContext).Assembly.GetName().Name)
                        .EnableRetryOnFailure());
            });
            services.AddTransient<IJosekiDatabase, MssqlJosekiDatabase>();

            services.AddTransient<AzskAuditProcessor>();
            services.AddTransient<PolarisAuditProcessor>();
            services.AddTransient<TrivyAuditProcessor>();
            services.AddTransient<AuditProcessorFactory>();

            services.AddScoped<ScannerContainersWatchman>();
            services.AddSingleton<SchedulerAssistant>();
            services.AddHostedService<ScannerResultsReaderJob>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Builder object.</param>
        /// <param name="env">Environment configuration.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/health/liveness", CreateHealthCheckOptions("liveness"));
            app.UseHealthChecks("/health/readiness", CreateHealthCheckOptions("readiness"));

            RunDbMigrations(app);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Joseki Backend V1");
            });

            app.UseMiddleware<HttpRequestLoggingMiddleware>();

            app.UseCors(this.myAllowSpecificOrigins);

            app.UseHttpsRedirection();
            app.UseRouting();
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

        /// <summary>
        /// Apply database schema migrations on service startup.
        /// </summary>
        /// <param name="app">A instance of <see cref="IApplicationBuilder"/>.</param>
        private static void RunDbMigrations(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<JosekiDbContext>();
            context.Database.Migrate();
        }
    }
}

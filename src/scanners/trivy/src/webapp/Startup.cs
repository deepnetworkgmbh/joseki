using System.Text.Json.Serialization;

using core;
using core.exporters;
using core.scanners;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using webapp.BackgroundWorkers;
using webapp.Configuration;
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
            // TODO: core uses Newtonsoft.Json, while here is System.Text.Json. Eventually, the only one should be left
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            services
                .AddHealthChecks()
                .AddCheck("Live", () => StateManager.Live, new[] { "liveness" })
                .AddCheck("Ready", () => StateManager.Ready, new[] { "readiness" });

            services.AddSingleton<ConfigurationParser>();
            services.AddSingleton<ImageScannerFactory>();
            services.AddTransient(provider => provider.GetService<ImageScannerFactory>().GetScanner());
            services.AddTransient(provider => provider.GetService<ImageScannerFactory>().GetExporter());
            services.AddTransient(provider => provider.GetService<ImageScannerFactory>().GetQueue());
            services.AddSingleton(provider =>
            {
                var scanner = provider.GetService<IScanner>();
                var exporter = provider.GetService<IExporter>();
                return new ImageScanner(scanner, exporter);
            });
            services.AddHostedService<TrivyDbUpdater>();
            services.AddHostedService<AzBlobHeartbeat>();
            services.AddHostedService<ImageScanRequestListener>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Container Image Scanner API", Version = "v1" });
            });
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

            app.UseMiddleware<HttpRequestLoggingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Container Image Scanner API V1");
            });

            app.UseRouting();

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

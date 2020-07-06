using System;

using joseki.db;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using webapp.Infrastructure;

namespace webapp
{
    /// <summary>
    /// The entry point.
    /// </summary>
    public class Program
    {
        private static bool shouldRunDbMigrations;

        internal static LoggingLevelSwitch LoggingLevelSwitch { get; set; } = new LoggingLevelSwitch();

        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="args">Input arguments.</param>
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            if (shouldRunDbMigrations)
            {
                RunDbMigrations(host.Services);
            }

            host.Run();
        }

        /// <summary>
        /// Creates a new instance of ASP.NET core application.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>HostBuilder object.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();

                    var configuration = config.Build();
                    shouldRunDbMigrations = string.Equals(
                        configuration["ASPNETCORE_ENVIRONMENT"],
                        Environments.Production,
                        StringComparison.OrdinalIgnoreCase);

                    config.ConfigureAzureAD(hostingContext, configuration);
                })
                .UseSerilog((ctx, config) =>
                {
                    config
                        .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext();

                    config.WriteTo.Console();
                });

        /// <summary>
        /// Apply database schema migrations on service startup.
        /// </summary>
        /// <param name="services">An instance of <see cref="IServiceProvider"/>.</param>
        private static void RunDbMigrations(IServiceProvider services)
        {
            Log.Information("DB migrations started");

            using var serviceScope = services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<JosekiDbContext>();
            context.Database.Migrate();

            Log.Information("DB migrations finished");
        }
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;

namespace webapp
{
    /// <summary>
    /// The entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="args">Input arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                .ConfigureAppConfiguration((hostingContext, config) => { config.AddEnvironmentVariables(); })
                .UseSerilog((ctx, config) =>
                {
                    config
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext();

                    config.WriteTo.Console();
                });
    }
}

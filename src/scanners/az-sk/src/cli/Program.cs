using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommandLine;

using core;
using core.Configuration;

using Serilog;

namespace cli
{
    class Program
    {
        private static readonly ILogger Logger = Log.ForContext<Program>();

        public class Options
        {
            [Option('c', "config", Required = true, HelpText = "Path to config file.")]
            public string ConfigPath { get; set; }

            [Option('s', "subscriptions", Required = true, Min = 1, HelpText = "Azure Subscription identifiers to be audited")]
            public IEnumerable<string> Subscriptions { get; set; }
        }

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => RunScanWith(opts).Wait())
                .WithNotParsed(HandleParseError);
        }

        private static async Task RunScanWith(Options opts)
        {
            var config = new ConfigurationParser(opts.ConfigPath);
            var factory = new ScannerFactory(config);

            // Az-Sk allows to use only one output folder for all concurrently running processes.
            // Also, user has no control over this folder structure.
            // Therefore it's easier to run scan sequentially.
            foreach (var subscription in opts.Subscriptions)
            {
                try
                {
                    var subscriptionScanner = new SubscriptionScanner(factory.GetScanner(), factory.GetExporter());
                    var result = await subscriptionScanner.Scan(subscription);
                    Logger.Information("Subscription {Subscription} was scanned with result: {ScanResult}", subscription, result.ScanResult);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "A subscription {Subscription} scanning failed", subscription);
                }
            }
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
            {
                Logger.Error("Failed to parse arguments: {Error}", error.Tag);
            }
        }
    }
}

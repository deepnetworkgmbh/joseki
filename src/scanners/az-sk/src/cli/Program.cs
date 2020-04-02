using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CommandLine;

using core;
using core.Configuration;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace cli
{
    class Program
    {
        private static LoggingLevelSwitch loggingLevelSwitch;

        public class Options
        {
            [Option('c', "config", Required = true, HelpText = "Path to config file.")]
            public string ConfigPath { get; set; }

            [Option('s', "subscriptions", Required = true, Min = 1, HelpText = "Azure Subscription identifiers to be audited")]
            public IEnumerable<string> Subscriptions { get; set; }

            [Option("from", Required = false, HelpText = "Audit infrastructure since this date")]
            public DateTime? From { get; set; }

            [Option("to", Required = false, HelpText = "Audit infrastructure till this date")]
            public DateTime? To { get; set; }

            [Option('v', "verbose", Default = false, HelpText = "Verbose log level.")]
            public bool Verbose { get; set; }
        }

        static void Main(string[] args)
        {
            loggingLevelSwitch = new LoggingLevelSwitch();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => RunScanWith(opts).Wait())
                .WithNotParsed(HandleParseError);
        }

        private static async Task RunScanWith(Options opts)
        {
            if (opts.From.HasValue != opts.To.HasValue)
            {
                Log.Error("Specifying the only one From or To dates is not supported. Both values should be set or removed");
                return;
            }
            else if (opts.From.HasValue && opts.From > opts.To)
            {
                Log.Error("From date should be behind To date");
                return;
            }

            var today = DateTime.UtcNow.Date;
            var startDate = opts.From ?? today;
            var endDate = opts.To ?? today;

            if (opts.Verbose)
            {
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
            }

            var config = new ConfigurationParser(opts.ConfigPath);
            var factory = new ScannerFactory(config);

            // Az-Sk allows to use only one output folder for all concurrently running processes.
            // Also, user has no control over this folder structure.
            // Therefore it's easier to run scan sequentially.
            foreach (var subscription in opts.Subscriptions)
            {
                try
                {
                    var scanDate = startDate;
                    while (scanDate <= endDate)
                    {
                        var subscriptionScanner = new SubscriptionScanner(factory.GetScanner(), factory.GetExporter());
                        var result = await subscriptionScanner.Scan(subscription, scanDate);
                        Log
                            .ForContext<Program>()
                            .Information("Subscription {Subscription} was scanned with result: {ScanResult} at {ScanDate}", subscription, result.ScanResult, scanDate);

                        scanDate = scanDate.AddDays(1);
                    }

                }
                catch (Exception ex)
                {
                    Log
                        .ForContext<Program>()
                        .Error(ex, "A subscription {Subscription} scanning failed", subscription);
                }
            }
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
            {
                Log
                    .ForContext<Program>()
                    .Error("Failed to parse arguments: {Error}", error.Tag);
            }
        }
    }
}

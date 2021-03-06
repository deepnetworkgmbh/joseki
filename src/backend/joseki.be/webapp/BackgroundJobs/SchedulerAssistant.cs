﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Events;

using webapp.Audits;
using webapp.Audits.Processors;
using webapp.Audits.Processors.azsk;
using webapp.Audits.Processors.polaris;
using webapp.Audits.Processors.trivy;
using webapp.BlobStorage;

namespace webapp.BackgroundJobs
{
    /// <summary>
    /// Maintains a list of containers, that should be periodically processed.
    /// Knows when to run a single container processor.
    /// </summary>
    public class SchedulerAssistant
    {
        private static readonly ILogger Logger = Log.ForContext<SchedulerAssistant>();

        private readonly ConcurrentDictionary<string, SchedulableItem> containers = new ConcurrentDictionary<string, SchedulableItem>();
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerAssistant"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public SchedulerAssistant(IServiceProvider services)
        {
            this.services = services;
        }

        /// <summary>
        /// The method runs handler function for all containers according to their heartbeat periods.
        /// </summary>
        /// <param name="cancellation">Indicates when to stop running.</param>
        /// <returns>A Task object, which completes when CancellationToken is canceled.</returns>
        public async Task Run(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    var item = this.containers.Values.OrderBy(c => c.NextProcessingTime).FirstOrDefault();

                    if (item == null)
                    {
                        var delay = TimeSpan.FromMinutes(1);
                        Logger.Information("Scheduler is sleeping for {Delay}. Reason: {TimeoutReason}", delay, "No working items in the queue");
                        await Task.Delay(delay, cancellation);
                    }
                    else
                    {
                        item.StartNewIteration();

                        Logger.Write(item.GetLogEventLevel(), "Scheduler is processing {ContainerName}", item.Container.Name);
                        var now = DateTime.UtcNow;
                        var nextProcessingTime = item.NextProcessingTime;

                        if (nextProcessingTime > now)
                        {
                            var delay = item.NextProcessingTime - now;

                            Logger.Write(item.GetLogEventLevel(), "Scheduler is sleeping for {Delay}. Reason: {TimeoutReason}", delay, "Too early to run");
                            await Task.Delay(delay, cancellation);
                        }

                        try
                        {
                            using var serviceScope = this.services.CreateScope();
                            var auditProcessor = GetProcessor(serviceScope, item.Container.Metadata);
                            await auditProcessor.Process(item.Container, cancellation);

                            item.LastProcessed = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning(ex, "Scheduler Assistant encountered unexpected exception during processing {ContainerName}", item.Container.Name);
                        }

                        Logger.Write(item.GetLogEventLevel(), "Scheduler has processed {ContainerName}", item.Container.Name);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Information(ex, "Scheduler Assistant was canceled");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Scheduler Assistant encountered unexpected exception");
                    throw;
                }
            }
        }

        /// <summary>
        /// Compares list of actual containers and working set.
        /// </summary>
        public void UpdateWorkingItems(ScannerContainer[] actualItems)
        {
            Logger.Information(
                "Updating Scheduler working items. Old count: {OldCount}; New Count: {NewCount}",
                this.containers.Count,
                actualItems.Length);

            var anyItem = this.containers.Values.FirstOrDefault();
            var nextGeneration = (anyItem?.Generation ?? 0) + 1;

            // The first iterator goes through actual list
            // It adds new items to the dictionary or update existing ones
            foreach (var container in actualItems)
            {
                var schedulableItem = this.containers.GetOrAdd(container.Name, new SchedulableItem { Container = container, LastProcessed = DateTime.MinValue });
                schedulableItem.Generation = nextGeneration;
                schedulableItem.Periodicity = GetProcessorPeriodicity(container.Metadata);
            }

            // The second iterator removes items, that were not touched by the first iterator
            var toDelete = this.containers.Values.Where(i => i.Generation < nextGeneration).ToArray();
            foreach (var item in toDelete)
            {
                Logger.Information("Removing {ContainerName} from Scheduler.", item.Container.Name);
                this.containers.TryRemove(item.Container.Name, out var _);
            }

            Logger.Information("Scheduler working items were updated");
        }

        private static TimeSpan GetProcessorPeriodicity(ScannerMetadata metadata)
        {
            return metadata.Periodicity == "on-message"
                ? TimeSpan.FromMinutes(1)
                : TimeSpan.FromMinutes(10);
        }

        private static IAuditProcessor GetProcessor(IServiceScope scope, ScannerMetadata metadata)
        {
            switch (metadata.Type)
            {
                case ScannerType.Azsk:
                    return scope.ServiceProvider.GetRequiredService<AzskAuditProcessor>();
                case ScannerType.Polaris:
                    return scope.ServiceProvider.GetRequiredService<PolarisAuditProcessor>();
                case ScannerType.Trivy:
                    return scope.ServiceProvider.GetRequiredService<TrivyAuditProcessor>();
                default:
                    Logger.Warning("AuditProcessorFactory was requested to instantiate {ScannerType} processor, which is not supported", metadata.Type);
                    throw new NotSupportedException($"{metadata.Type} audit processor is not supported");
            }
        }

        /// <summary>
        /// Helps to keep track of scanner-containers processing time.
        /// </summary>
        internal class SchedulableItem
        {
            private DateTime lastLoggableIterationAt;
            private bool isLoggableIteration;

            /// <summary>
            /// The container, which should be periodically processed.
            /// </summary>
            internal ScannerContainer Container { get; set; }

            /// <summary>
            /// Last time when the container was processed.
            /// </summary>
            internal DateTime LastProcessed { get; set; }

            /// <summary>
            /// How often the container should be processed.
            /// </summary>
            internal TimeSpan Periodicity { get; set; }

            /// <summary>
            /// Indicates a generation to which the item belongs. The property used to get rid of stale items.
            /// </summary>
            internal int Generation { get; set; }

            /// <summary>
            /// Returns the point of time, when the container should be processed.
            /// </summary>
            internal DateTime NextProcessingTime => this.LastProcessed.Add(this.Periodicity);

            /// <summary>
            /// The fundamental problem - once in a while, joseki should write log messages.
            /// BUT, if joseki writes on each iteration of scanner processor - logs might be flooded with meaningless events.
            /// For example, Trivy scanner-processors are executed each minute, which leads to 3*60 log-events each hour.
            ///
            /// To reduce the noise, this method helps to switch log-level from Debug to Information each hour.
            /// </summary>
            internal void StartNewIteration()
            {
                var sinceLastLog = DateTime.UtcNow - this.lastLoggableIterationAt;

                // if last logged iteration was more than 1 hour ago - log this one too.
                if (sinceLastLog.TotalHours >= 1)
                {
                    this.lastLoggableIterationAt = DateTime.UtcNow;
                    this.isLoggableIteration = true;
                    return;
                }

                // if we logged the previous iteration - skip this one
                if (this.isLoggableIteration)
                {
                    this.isLoggableIteration = false;
                }
            }

            /// <summary>
            /// If iteration should be logged _globally_ - returns Information log-level.
            /// Otherwise - returns Debug.
            /// </summary>
            /// <returns>Serilog log-level enumeration.</returns>
            internal LogEventLevel GetLogEventLevel()
            {
                return this.isLoggableIteration
                    ? LogEventLevel.Information
                    : LogEventLevel.Debug;
            }
        }
    }
}
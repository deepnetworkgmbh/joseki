using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Serilog;
using webapp.Audits.PostProcessors;
using webapp.BlobStorage;
using webapp.Database;
using webapp.Database.Cache;
using webapp.Database.Models;
using webapp.Queues;

namespace webapp.Audits.Processors.polaris
{
    /// <summary>
    /// Polaris audit processor.
    /// </summary>
    public class PolarisAuditProcessor : IAuditProcessor
    {
        private static readonly ILogger Logger = Log.ForContext<PolarisAuditProcessor>();

        private readonly IBlobStorageProcessor blobStorage;
        private readonly IJosekiDatabase db;
        private readonly ChecksCache cache;
        private readonly IQueue queue;
        private readonly IAuditPostProcessor extractOwnershipProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolarisAuditProcessor"/> class.
        /// </summary>
        /// <param name="blobStorage">Blob Storage implementation.</param>
        /// <param name="db">Joseki database implementation.</param>
        /// <param name="cache">Checks cache object.</param>
        /// <param name="queue">Queue Service implementation.</param>
        /// <param name="postProcessor">ExtractOwnershipProcessor.</param>
        public PolarisAuditProcessor(IBlobStorageProcessor blobStorage, IJosekiDatabase db, ChecksCache cache, IQueue queue, ExtractOwnershipProcessor postProcessor)
        {
            this.blobStorage = blobStorage;
            this.db = db;
            this.cache = cache;
            this.queue = queue;
            this.extractOwnershipProcessor = postProcessor;
        }

        /// <inheritdoc />
        public async Task Process(ScannerContainer container, CancellationToken token)
        {
            var audits = await this.blobStorage.GetUnprocessedAudits(container);

            foreach (var audit in audits)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    Logger.Information("Processing audit result from {AuditName} container was {ProcessingState}", audit.Name, "started");
                    await this.ProcessSingleAudit(audit);
                    Logger.Information("Processing audit result from {AuditName} container was {ProcessingState}", audit.Name, "finished");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Processing audit result from {AuditName} container was {ProcessingState}", audit.Name, "failed");
                }
            }
        }

        private async Task ProcessSingleAudit(AuditBlob auditBlob)
        {
            var path = $"{auditBlob.ParentContainer.Name}/{auditBlob.Name}";
            var stream = await this.blobStorage.DownloadFile(path);

            using var sr = new StreamReader(stream);
            var metadataString = sr.ReadToEnd();
            var auditMetadata = JsonConvert.DeserializeObject<AuditMetadata>(metadataString);

            if (auditMetadata.AuditResult != "succeeded")
            {
                Logger.Warning(
                    "Audit {AuditPath} result is {AuditResult} due: {FailureReason}",
                    path,
                    auditMetadata.AuditResult,
                    auditMetadata.FailureDescription);
            }
            else
            {
                try
                {
                    var audit = await this.NormalizeRawData(auditBlob, auditMetadata);

                    int succeeded = 0, failed = 0, nodata = 0, inprogress = 0;
                    foreach (var checkResult in audit.CheckResults)
                    {
                        switch (checkResult.Value)
                        {
                            case CheckValue.Succeeded:
                                succeeded++;
                                break;
                            case CheckValue.Failed:
                                failed++;
                                break;
                            case CheckValue.NoData:
                                nodata++;
                                break;
                            case CheckValue.InProgress:
                                inprogress++;
                                break;
                        }
                    }

                    Logger.Information(
                        "Successfully processed audit {AuditId} of {AuditDate} with {CheckResultCount} check results, where succeeded {Succeeded}; failed {Failed}; in-progress {InProgress}; no-data {NoData}",
                        audit.Id,
                        audit.Date,
                        audit.CheckResults.Count,
                        succeeded,
                        failed,
                        nodata,
                        inprogress);

                    await this.db.SaveAuditResult(audit);

                    // TODO: use a cancellation token inside this process?
                    await this.extractOwnershipProcessor.Process(audit, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to process audit {AuditPath}", path);
                }
            }

            await this.blobStorage.MarkAsProcessed(auditBlob);
        }

        private async Task<Audit> NormalizeRawData(AuditBlob auditBlob, AuditMetadata auditMetadata)
        {
            var clusterId = string.IsNullOrEmpty(auditMetadata.ClusterId) ? auditBlob.ParentContainer.Metadata.Id : auditMetadata.ClusterId;

            var auditJson = await this.GetJsonObject($"{auditBlob.ParentContainer.Name}/{auditMetadata.PolarisAuditPaths}");
            var checks = await this.ParseChecksResults(auditJson, auditMetadata, clusterId);

            var k8sJson = await this.GetJsonObject($"{auditBlob.ParentContainer.Name}/{auditMetadata.KubeMetadataPaths}");

            try
            {
                var imageScanCheckResults = await this.EnrichAuditWithImageScans(auditMetadata.AuditId, k8sJson, clusterId);
                checks.AddRange(imageScanCheckResults);
            }
            catch (Exception ex)
            {
                // Failure here should not impact the rest of audit
                Logger.Warning(ex, "Failed to enrich audit {AuditId} Check Results with image scans", auditMetadata.AuditId);
            }

            var k8sMetadata = new JObject
            {
                { "scanner", JToken.FromObject(auditBlob.ParentContainer.Metadata) },
                { "audit", JToken.FromObject(auditMetadata) },
                { "cluster", k8sJson },
            };

            var auditDate = DateTimeOffset.FromUnixTimeSeconds(auditMetadata.Timestamp).DateTime;
            var infraComponentId = $"/k8s/{auditMetadata.ClusterId}";
            var k8sName = this.ParseClusterName(k8sJson) ?? infraComponentId;
            var audit = new Audit
            {
                Id = auditMetadata.AuditId,
                Date = auditDate,
                ScannerId = $"{auditBlob.ParentContainer.Metadata.Type}/{auditBlob.ParentContainer.Metadata.Id}",
                ComponentId = infraComponentId,
                ComponentName = k8sName,
                CheckResults = checks,
                MetadataKube = new MetadataKube
                {
                    AuditId = auditMetadata.AuditId,
                    Date = auditDate,
                    JSON = k8sMetadata.ToString(Formatting.None),
                },
            };

            return audit;
        }

        private string ParseClusterName(JObject k8sJson)
        {
            try
            {
                var name = k8sJson["SourceName"].Value<string>();
                var starts = 0;
                var ends = name.Length;

                if (name.StartsWith("https://"))
                {
                    starts = 8;
                }

                if (name.EndsWith(":443"))
                {
                    ends -= 4;
                }

                return name[starts..ends];
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Failed to parse cluster name");
                return string.Empty;
            }
        }

        private async Task<JObject> GetJsonObject(string path)
        {
            var stream = await this.blobStorage.DownloadFile(path);

            using var sr = new StreamReader(stream);
            var fileContent = sr.ReadToEnd();

            return JObject.Parse(fileContent);
        }

        private async Task<List<CheckResult>> ParseChecksResults(JObject auditJson, AuditMetadata auditMetadata, string clusterId)
        {
            if (!(auditJson["Results"] is JArray rootResultsArray))
            {
                throw new FormatException("Audit results file does not have Results object in it");
            }

            var checkResults = new List<CheckResult>();

            foreach (var rootResultItem in rootResultsArray)
            {
                var nsName = rootResultItem["Namespace"].Value<string>();
                var objectKind = rootResultItem["Kind"].Value<string>().ToLowerInvariant();
                var objectName = rootResultItem["Name"].Value<string>().ToLowerInvariant();
                var idPrefix = $"/k8s/{clusterId}/ns/{nsName}/{objectKind}/{objectName}";

                // parse deployment/job/daemon-set/etc level checks
                if (rootResultItem["Results"] != null && rootResultItem["Results"].HasValues)
                {
                    checkResults.AddRange(await this.ProcessResultsObject(rootResultItem["Results"], idPrefix, auditMetadata.AuditId));
                }

                // parse pod level checks
                if (rootResultItem["PodResult"] != null && rootResultItem["PodResult"].HasValues)
                {
                    checkResults.AddRange(await this.ProcessResultsObject(rootResultItem["PodResult"]["Results"], $"{idPrefix}/pod", auditMetadata.AuditId));

                    // parse container level checks
                    if (rootResultItem["PodResult"]["ContainerResults"] is JArray containersArray)
                    {
                        for (var i = 0; i < containersArray.Count; i++)
                        {
                            var containerName = containersArray[i]["Name"] != null
                                ? containersArray[i]["Name"].Value<string>()
                                : $"{objectName}-container{i + 1}";
                            checkResults.AddRange(await this.ProcessResultsObject(containersArray[i]["Results"], $"{idPrefix}/container/{containerName}", auditMetadata.AuditId));
                        }
                    }
                }
            }

            return checkResults;
        }

        private async Task<List<CheckResult>> ProcessResultsObject(JToken jToken, string componentId, string auditId)
        {
            var results = new List<CheckResult>();
            foreach (var child in jToken.Children().Select(i => i.First))
            {
                var id = child["ID"].Value<string>();
                var message = child["Message"].Value<string>();
                var success = child["Success"].Value<bool>();
                var severity = child["Severity"].Value<string>();
                var category = child["Category"].Value<string>();

                var checkId = $"polaris.{id}";
                var internalCheckId = await this.cache.GetOrAddItem(checkId, () => new Check
                {
                    Id = checkId,
                    Severity = this.ToSeverity(severity),
                    Category = category,
                });

                results.Add(new CheckResult
                {
                    ExternalCheckId = checkId,
                    InternalCheckId = internalCheckId,
                    ComponentId = componentId,
                    AuditId = auditId,
                    Value = success ? CheckValue.Succeeded : CheckValue.Failed,
                    Message = message,
                });
            }

            return results;
        }

        private CheckSeverity ToSeverity(string value)
        {
            return value switch
            {
                "error" => CheckSeverity.High,
                "warning" => CheckSeverity.Medium,
                _ => CheckSeverity.Unknown
            };
        }

        private async Task<CheckResult[]> EnrichAuditWithImageScans(string auditId, JObject k8sMeta, string clusterId)
        {
            var checkResults = new List<CheckResult>();
            var checkId = await this.cache.GetImageScanCheck();

            // 1. Create Dictionary<ImageTag, List<ComponentId>> from k8sMeta
            var imageToComponents = new Dictionary<string, List<string>>();
            if (k8sMeta["Deployments"] is JArray deployments)
            {
                this.GetImagesFromResourcesGroup(clusterId, deployments, "deployment", imageToComponents);
            }

            if (k8sMeta["StatefulSets"] is JArray statefulSets)
            {
                this.GetImagesFromResourcesGroup(clusterId, statefulSets, "statefulset", imageToComponents);
            }

            if (k8sMeta["DaemonSets"] is JArray daemonSets)
            {
                this.GetImagesFromResourcesGroup(clusterId, daemonSets, "daemonset", imageToComponents);
            }

            if (k8sMeta["Jobs"] is JArray jobs)
            {
                this.GetImagesFromResourcesGroup(clusterId, jobs, "job", imageToComponents);
            }

            if (k8sMeta["CronJobs"] is JArray cronJobs)
            {
                this.GetImagesFromCronJobs(clusterId, cronJobs, imageToComponents);
            }

            // 2. query latest image-scan results for image-tags
            var scanResults =
                (await this.db.GetNotExpiredImageScans(imageToComponents.Keys.ToArray()))
                .ToDictionary(i => i.ImageTag, i => i);

            // 3. enqueue new scans if needed
            // 4. add corresponding check-results
            foreach (var (imageTag, components) in imageToComponents)
            {
                if (scanResults.TryGetValue(imageTag, out var existingResult))
                {
                    // if there is already not-expired image-scan in the database - create check-results from it
                    // NOTE: the result might be in any state:
                    // - Queued (the scan is in progress),
                    // - Failed (trivy failed to perform the scan),
                    // - Succeed (trivy successfully did a scan recently)
                    checkResults.AddRange(components.Select(component => new CheckResult
                    {
                        AuditId = auditId,
                        ComponentId = component,
                        InternalCheckId = checkId,
                        Value = existingResult.GetCheckResultValue(),
                        Message = existingResult.GetCheckResultMessage(),
                    }));
                }
                else
                {
                    // if no actual scan in DB, then:
                    // - queue a new scan;
                    // - add in-progress result.
                    var inProgressScan = new ImageScanResultWithCVEs
                    {
                        Id = Guid.NewGuid().ToString(),
                        Date = DateTime.UtcNow,
                        ImageTag = imageTag,
                    };

                    await this.queue.EnqueueImageScanRequest(inProgressScan);
                    await this.db.SaveInProgressImageScan(inProgressScan);

                    checkResults.AddRange(components.Select(component => new CheckResult
                    {
                        AuditId = auditId,
                        ComponentId = component,
                        InternalCheckId = checkId,
                        Value = CheckValue.InProgress,
                        Message = "The scan is in progress",
                    }));
                }
            }

            return checkResults.ToArray();
        }

        private void GetImagesFromResourcesGroup(string clusterId, JArray resourceGroup, string objectKind, Dictionary<string, List<string>> dict)
        {
            foreach (var resource in resourceGroup)
            {
                var nsName = resource["metadata"]["namespace"].Value<string>();
                var objectName = resource["metadata"]["name"].Value<string>();

                if (resource["spec"]["template"]["spec"]["initContainers"] is JArray initContainers)
                {
                    for (var i = 0; i < initContainers.Count; i++)
                    {
                        var containerName = initContainers[i]["name"] != null
                            ? initContainers[i]["name"].Value<string>()
                            : $"{objectName}-container{i + 1}";
                        var imageTag = initContainers[i]["image"].Value<string>();

                        var componentId = $"/k8s/{clusterId}/ns/{nsName}/{objectKind}/{objectName}/container/{containerName}/image/{imageTag}";
                        if (!dict.TryAdd(imageTag, new List<string> { componentId }))
                        {
                            dict[imageTag].Add(componentId);
                        }
                    }
                }

                if (resource["spec"]["template"]["spec"]["containers"] is JArray containers)
                {
                    for (var i = 0; i < containers.Count; i++)
                    {
                        var containerName = containers[i]["name"] != null
                            ? containers[i]["name"].Value<string>()
                            : $"{objectName}-container{i + 1}";
                        var imageTag = containers[i]["image"].Value<string>();

                        var componentId = $"/k8s/{clusterId}/ns/{nsName}/{objectKind}/{objectName}/container/{containerName}/image/{imageTag}";
                        if (!dict.TryAdd(imageTag, new List<string> { componentId }))
                        {
                            dict[imageTag].Add(componentId);
                        }
                    }
                }
            }
        }

        private void GetImagesFromCronJobs(string clusterId, JArray resourceGroup, Dictionary<string, List<string>> dict)
        {
            foreach (var resource in resourceGroup)
            {
                var nsName = resource["metadata"]["namespace"].Value<string>();
                var objectName = resource["metadata"]["name"].Value<string>();

                if (resource["spec"]["jobTemplate"]["spec"]["template"]["spec"]["initContainers"] is JArray initContainers)
                {
                    for (var i = 0; i < initContainers.Count; i++)
                    {
                        var containerName = initContainers[i]["name"] != null
                            ? initContainers[i]["name"].Value<string>()
                            : $"{objectName}-container{i + 1}";
                        var imageTag = initContainers[i]["image"].Value<string>();

                        var componentId = $"/k8s/{clusterId}/ns/{nsName}/cronjob/{objectName}/container/{containerName}/image/{imageTag}";
                        if (!dict.TryAdd(imageTag, new List<string> { componentId }))
                        {
                            dict[imageTag].Add(componentId);
                        }
                    }
                }

                if (resource["spec"]["jobTemplate"]["spec"]["template"]["spec"]["containers"] is JArray containers)
                {
                    for (var i = 0; i < containers.Count; i++)
                    {
                        var containerName = containers[i]["name"] != null
                            ? containers[i]["name"].Value<string>()
                            : $"{objectName}-container{i + 1}";
                        var imageTag = containers[i]["image"].Value<string>();

                        var componentId = $"/k8s/{clusterId}/ns/{nsName}/cronjob/{objectName}/container/{containerName}/image/{imageTag}";
                        if (!dict.TryAdd(imageTag, new List<string> { componentId }))
                        {
                            dict[imageTag].Add(componentId);
                        }
                    }
                }
            }
        }
    }
}
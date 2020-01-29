# Joseki Backend

- [Joseki Backend](#joseki-backend)
  - [Scenarious](#scenarious)
    - [Process Audit Results](#process-audit-results)
    - [Get Audit results](#get-audit-results)
      - [Overall infrastructure: latest](#overall-infrastructure-latest)
      - [Overall infrastructure: history](#overall-infrastructure-history)
      - [Overall infrastructure: diff](#overall-infrastructure-diff)
      - [Azure subscription: latest](#azure-subscription-latest)
      - [Azure subscription: history](#azure-subscription-history)
      - [Azure subscription: diff](#azure-subscription-diff)
      - [Kubernetes cluster: latest](#kubernetes-cluster-latest)
      - [Kubernetes cluster: history](#kubernetes-cluster-history)
      - [Kubernetes cluster: diff](#kubernetes-cluster-diff)
      - [Container Image: latest](#container-image-latest)
      - [Container Image: history](#container-image-history)
      - [Container Image: diff](#container-image-diff)
      - [Check overview](#check-overview)
  - [Technologies](#technologies)
  - [Runtime](#runtime)
  - [Configuration](#configuration)
  - [Data layer](#data-layer)
    - [Database Data Model](#database-data-model)
    - [Reading audit results](#reading-audit-results)
      - [Processing az-sk results](#processing-az-sk-results)
      - [Processing polaris results](#processing-polaris-results)
      - [Processing trivy results](#processing-trivy-results)
      - [Processing kube-bench results](#processing-kube-bench-results)
    - [Housekeeping Audit Blobs](#housekeeping-audit-blobs)
  - [Inter-process communication](#inter-process-communication)
    - [Frontend API](#frontend-api)
      - [Swagger](#swagger)
    - [Messaging Service](#messaging-service)
      - [Enqueue image scan](#enqueue-image-scan)

At the very first iteration, *Joseki* is focused on Azure and Kubernetes analysis.

## Scenarious

Listed scenarios cover only [V1](/PRODUCTOVERVIEW.md#v1---initial-release) priority items

### Process Audit Results

`Backend` application is responsible for getting latest audit/scan results from **Blob Storage**, normalize these data and inserts into the **Database** optimized for querying.

![Process Audit Results sequence diagram](/docs_files/diagrams/be-process-audit-results.png)

Implementation details of the process is described in [Reading Audit Results](#reading-audit-results) section.

### Get Audit results

`Backend` application exposes hefty endpoints to query audit result.

*Joseki* supports three types of queries:

1. The latest known state of the queried resources (shortened `latest`);
2. Diff between two dates (`diff`);
3. Historical data for a range of dates (`hist`).

All these query types could be executed against four groups of objects:

1. Overall infrastructure (`Q_ov`).
2. Azure subscription (`Q_sub`).
3. Kubernetes cluster (`Q_k8s`).
4. Container image (`Q_img`).

Total, 12 different query endpoints are exposed.

#### Overall infrastructure: latest

`Q_ov latest` is used by `frontend` as entry-point to the *Joseki* dashboard. The endpoint returns:

- overall infrastructure score, score history, score-trend
- amount of passed/failed/warning/nodata checks
- the same data, but per each subscription and kubernetes clsuter

Where

- *score* - is percentage of succeeded checks;
- *score history* - score value for the last 30 days (0 for days with no data);
- *score trend* - vector that summarizes score changes over last 30 days.

#### Overall infrastructure: history

`Q_ov hist` lists check results for a selected periood of time. Each daily result includes:

- amount of passed/failed/warning/nodata checks for the entire infrastructure
- the score

#### Overall infrastructure: diff

`Q_ov diff` compares check results for two selected dates:

- amount of passed/failed/warning/nodata checks
- the score

The data is displayed for the entire infrastructure and broken down by subscriptions and kubernetes clusters.

#### Azure subscription: latest

`Q_sub latest` gives detailed overview of resources in a single Azure subsription:

- score, score history, score-trend
- count of passed/failed/warning/nodata checks for the last 30 days
- list of latest check-results
  - check-id
  - check description
  - remediation
  - resource-id
  - category
  - results of the same check over last 30 days

#### Azure subscription: history

`Q_sub hist` lists check results for a selected periood of time. Each daily result includes:

- amount of passed/failed/warning/nodata checks for a selected subscription
- the score

#### Azure subscription: diff

`Q_sub hist` compares the state of the same Azure subscription in two points of time. The result includes:

- score and count of passed/failed/warning/nodata checks at date `A` and `B`
- list of checks:
  - check-id
  - resource-id
  - category
  - comparison result:
    - present only in `A`
    - present only in `B`
    - the same in both
    - present in both, but with different result

#### Kubernetes cluster: latest

`Q_k8s latest` gives detailed overview of resources in a single kubernetes cluster:

- score, score history, score-trend
- count of passed/failed/warning/nodata checks for the last 30 days
- list of latest check-results; the list aggregates data from `polaris`, `trivy` scanners:
  - check-id
  - check description
  - remediation
  - resource-id
  - category
  - results of the same check over last 30 days

#### Kubernetes cluster: history

`Q_k8s hist` lists check results for a selected periood of time. Each daily result includes:

- amount of passed/failed/warning/nodata checks for a selected kubernetes cluster
- the score

#### Kubernetes cluster: diff

`Q_k8s diff` compares the state of the same Kubernetes cluster in two points of time. The result includes:

- score and count of passed/failed checks at date `A` and `B`
- list of checks:
  - check-id
  - resource-id
  - category
  - comparison result:
    - present only in `A`
    - present only in `B`
    - the same in both
    - present in both, but with different result

#### Container Image: latest

`Q_img latest` lists found issues and highlights, what resources use the image:

- counts of CVEs grouped by severity for the last 30 days
- list of CVEs:
  - CVE id
  - severity
  - title and description
  - remediation
  - links with further info
- resources, that use the container image

#### Container Image: history

`Q_img hist` lists CVEs counts grouped by severity for a selected periood of time.

#### Container Image: diff

`Q_img diff` compares the container-image scan result in two points of time. The result includes:

- counts of CVEs grouped by severity at date `A` and `B`
- list of CVEs:
  - CVE id
  - severity
  - title and description
  - comparison result:
    - present only in `A`
    - present only in `B`
    - the same in both
- resources, that use the container image
  - present only in `A`
  - present only in `B`
  - the same in both

#### Check overview

Check overview explains the check purpose and lists what resources were evaluated.

- general check information
  - check-id
  - check description
  - category
  - remediation
- counts resources that failed/passed the check for the last 30 days
- the current list of resources, that were evaluated by this check

## Technologies

`Backend` is `ASP.NET core 3` application, hosted in docker-container.

The application uses several Azure cloud services:

- [Azure PostgreSQL](https://azure.microsoft.com/de-de/services/postgresql/) as **Database**;
- [Azure Blob Storages](https://azure.microsoft.com/en-us/services/storage/blobs/) as **Blob Storage** to process audit result;
- [Azure Queue Storage](https://azure.microsoft.com/de-de/services/storage/queues/) as **Messaging Service**.

The current choice is based on the most familiar products/framework of the dev-team at the moment of writing.

## Runtime

`Backend` application is expected to run

- as linux docker container;
- in a **single instance** mode as some parts of the service might not support competing instances.

## Configuration

- `Backend` config is located at path specified in `CONFIG_FILE_PATH` environment variable path;
- `LOG_FORMAT` - `plain-text` or `json`.

`Backend` configuration file defines which type of **Blob Storage**, **Messaging Service**, and **Database** should be used, credentials/connection strings, and others.

(**TODO:** store sensitive data in secure place instead of file system)

## Data layer

`Backend` application uses two types of data storage: **Blobs** and **Database**:

- Blob Storage with audit results, which application reads and maintains reading checkpoints;
- Database for normalized audit data, configuration files, and others.

### Database Data Model

TBD

### Reading audit results

Audit Result reader is `ASP.NET Core` [Hosted Service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio) object, which

- wakes up every `N` minutes (`N` is a property in configuration file);
- gets root level containers with metadata from **Blob Storage**;
- invokes proper audit-processor for each container according to scanner type;
  - scanner type is determined based on container name `{scanner-type}-{scanner-short-id}`

Audit-processors - are scanner type specific objects, that:

- read scanner metadata file `{scanner-type}-{scanner-short-id}.meta` and calculate seconds since hearbeat was updated: `DateTime.UtcNow - heartbeat` and do one of the following:
  - if it is more than `heartbeat-periodicity` - log it with *warning* level
  - if it is two times more than `heartbeat-periodicity` and more than one hour - log it with *error* level (**TODO:** reduce amount of error logs. add a tag to the container?)
  - if it is more than a week - add `stale` tag to scanner folder metadata
- get all `{yyyyMMdd-HHmmss}-{hash:7}` containers with audit results that does not have metadata tag `processed` and schedule a Task to process each container. After processing container content, the task should add metadata tag `processed`.

#### Processing az-sk results

TBD

#### Processing polaris results

TBD

#### Processing trivy results

TBD

#### Processing kube-bench results

TBD

### Housekeeping Audit Blobs

Each scanner has a simple task: perform audit and persist the result in a known location, where `Backend` application can access it. The solution is straight-forward but has a major disadvantage - eventually, the blob would be flooded with files. It might cause performance degradation and adds operational complexity - *something* should keep track of processed/unprocessed files to avoid redundant traffic over the network.

Audit Blobs Watchman - is another background process, which takes care of processed files and tries to keep operational space in fit.

It's an `ASP.NET Core` [Hosted Service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio), which:

- once a day moves *processed* blobs to *archive*;
- once a day moves *stale* scanner folders to *garbage-bin*;
- once a week moves archived blobs to *garbage-bin* after expiring `archive-retention-period`;
- once a week deletes items from *garbage-bin* after expiring `garbage-retention-period`.

The job tries to perform actions at defined in configuration file time: `audit-blobs-watchman.time`.

## Inter-process communication

`Backend` application *communicates* with all other services in a few ways:

- exposes REST API for `Frontend`
- asynchronously reads Audit results from **Blob Storage**
- enqueue image-scan requests to **Messaging Service**

### Frontend API

Communication with `Frontend` web-application happens through REST API over HTTPs. It is implemented with [ASP.NET Web APIs](https://dotnet.microsoft.com/apps/aspnet/apis)

#### Swagger

API is documented using Swagger tool [Swashbunckle](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle) and accessible at `https://{backend-host}:{backend-port}/swagger`.

### Messaging Service

`Backend` uses **Messaging Service** to send image-scan requests to `trivy` scanner. Access to the Messaging Service is abstracted, so adding new implementation is possible. The first implementation uses Azure Queue Storage and `Backend` accesses it throught [official Nuget library](https://docs.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues).

If message enqueueing failed due transient failure, `Backend` retries the operation 3 times using [exponential backoff](https://en.wikipedia.org/wiki/Exponential_backoff). If all the attempts fail, error-log event is recorded.

#### Enqueue image scan

Image-scan request message envelop consists of two parts: `headers` (system info, like version, tracing data, etc) and `payload` (request details). The initial version is pretty simple, but might be extended later with more data:

```json
{
  "headers": {
    "creation-time": "int",
    "payload-version": "string"
  },
  "payload":{
    "image-tag": "string"
  }
}
```

Enqueue process is triggered from audit-result processors: they enqueue image-scan request for each found image-tag, which was not scanned more than configured container-image scan-result cache retention time.
At the moment, only `polaris` audit-data processor supports it.

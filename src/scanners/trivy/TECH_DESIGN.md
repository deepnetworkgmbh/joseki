# Trivy Scanner

- [Trivy Scanner](#trivy-scanner)
  - [Implementation details](#implementation-details)
  - [Configuration](#configuration)
  - [Blob Storage service](#blob-storage-service)
    - [Azure Blob Storage](#azure-blob-storage)
    - [Audit result format](#audit-result-format)
    - [Scanner metadata](#scanner-metadata)
  - [Messaging service](#messaging-service)
    - [Azure Queue Storage](#azure-queue-storage)

|          |                      |
| -------- | -------------------- |
| Date:    | April 17th, 2020     |
| Status:  | Obsolete (to review) |
| Authors: | @v1r7u               |

The scanner wraps [Trivy by Aqua Security](https://github.com/aquasecurity/trivy) - vulnerability scanner for containers.

The scanner itself does not add anything new to the underlying trivy application. You can consider it only as a trivy scan data shipper:

- receives a new scan-request from **Messaging Service**
- invoke trivy with provided configuration;
- upload results to a **Blob Storage** service.

## Implementation details

The scanner is `asp.net core 3` web application, which listens to a new scan-requests from the **Messaging Service**. Upon receiving a new scan request, the application figures out container registry, where the requested images is stored, scans it, and uploads scan results to a Blob Storage

The scanner consists of modules:

- `configuration` - reads scanner configuration;
- `container-registry` - is able by container-image tag return container-registry credentials. The initial version takes credentials from configuration file, later could be replaced with secure secrets storage;
- `blob-uploader` - knows how to upload scan results to **Blob Storage**;
- `queue-listener` - knows how to process messages from the queue in reliable fashion;
- `scanner` itself - glues all pieces together: instantiate correct object types based on config, retrieves container-registry credentials, invokes trivy, uploads scan-results;

## Configuration

Trivy-specific config should be located at path specified in `IMAGE_SCANNER_CONFIG_FILE_PATH` environment variable path. Config has:

- scanner identifier and version;
- path to trivy binaries and their version;
- `registries` - array of container-registries credentials (address, username, password)
- exact Blob Storage service implementation is constructed based on yaml tag `!{storage-type}`
  - At the moment, Azure Blob Storage `base_path` and `sas_token` are provided as properties of the same config file;
- exact Messaging Service implementation is constructed based on yaml tag `!{messaging-type}`environment variable
  - At the moment, all Azure Queue Storage parameters are provided as properties of the same config file (queue names, sas-tokens, base-path);

## Blob Storage service

At the moment only Azure Blob Storage is supported.

### Azure Blob Storage

To access the service the scanners uses official [Microsoft Storage Blob](https://www.nuget.org/packages/Microsoft.Azure.Storage.Blob/) Nuget library

Permission to write data to exact folder is given with [Shared Access Signature](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview). To generate a SAS token the following `az cli` could be used:

```sh
az storage container generate-sas --name CONTAINER_NAME --account-key STORAGE_ACCOUNT_KEY --account-name STORAGE_ACCOUNT_NAME --expiry 2020-02-01 --permissions rw
```

### Audit result format

Uploaded audit results should follow the general [technical design doc](/TECH_DESIGN.md#backend-and-scanners), where

- `scanner-type` is `trivy`;
- `scanner-id` is `SCANNER_IDENTIFIER` environment variable;
- `scanner-periodicity` is `on-message` environment variable;

Each audit result folder should have two files:

- `meta` - json object, which describes audit metadata:
  - `audit-id`
  - `scanner-version`
  - `timestamp` - unix epoch in seconds;
  - `audit-result` - `succeeded`, `audit-failed`, `upload-failed`;
  - `failure-description` - (optional) if `audit-result` is failed, explains the reason;
  - `trivy-version`
  - `trivy-audit-path` - path to file with audit result
- `scan-result.json` - `trivy` scan result;

Each file upload is retried 3 times with exponential backoff. If still failing - `meta` file has human-readable explanation in `failure-description` property.

### Scanner metadata

Each `heartbeat-period` seconds, the scanner should update general metadata file at `/trivy-{scanner-id-short-hash}/trivy-{scanner-id-short-hash}.meta` with schema:

```json
{
  "scanner-type": "trivy",
  "scanner-id": "{UUID}",
  "scanner-periodicity": "on-message",
  "heartbeat-periodicity": "int",
  "heartbeat": 1579619671
}
```

## Messaging service

New image scan-requests come from a **Messaging Service**.

The scanner supports horizontal scaling. To achieve that, it reads scan-requests from the queue in competing mode:

- set `Visibility Timeout` at read time: once a message is requested from the Messaging Service - it should not be visible to other consumers;
- *safely* try to scan an image if succeded - delete the message from the queue;
- if action failed because of a transient error, do an [exponential backoff](https://en.wikipedia.org/wiki/Exponential_backoff): based on message dequeue count updates message visibility timeout in **Messaging service**.
- if dequeue count is more than 3 or processing failed because of non-transient error, move [a poisoned message to the quarantine queue](https://alexandrebrisebois.wordpress.com/2013/08/14/poison-queues-are-a-must/).

Visibility timeout prevents multiple instances of the scanner to process the same message in the same time. Exponential backoff helps to deal with transient failures (for example, external dependency is not available just right now, but might work fine after 30 seconds). Quarantine queue avoids a poisoned message to stuck in the queue forever (later human-operator might review the queue and return messages to a normal queue if there was a problem, which was fixed afterward).

At the moment only [Azure Queue Storage](https://docs.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction) is supported.

### Azure Queue Storage

To access Azure Queue Storage the scanner uses official [Microsoft Storage Queue](https://www.nuget.org/packages/Microsoft.Azure.Storage.Queue/) Nuget library. To generate a SAS token the following `az cli` could be used:

```sh
az storage queue generate-sas --name image-scan-requests --account-key STORAGE_ACCOUNT_KEY --account-name STORAGE_ACCOUNT_NAME --expiry 2020-02-01 --permissions pu

az storage queue generate-sas --name image-scan-requests-quarantine --account-key STORAGE_ACCOUNT_KEY --account-name STORAGE_ACCOUNT_NAME --expiry 2020-02-01 --permissions a
```

***NOTE:*** `image-scan-requests` queue should have `--permissions pu`, which means *process* and *update*; while `image-scan-requests-quarantine` queue should have only `--permissions a` only *to enqueu* messages.

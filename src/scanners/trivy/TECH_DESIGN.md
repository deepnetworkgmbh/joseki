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

The scanner wraps [Trivy by Aqua Security](https://github.com/aquasecurity/trivy) - vulnerability scanner for containers.

The scanner itself does not add anything new to the underlying trivy application. You can consider it only as a thin trivy scan data shipper:

- receives a new scan-request from **Messaging Service**
- invoke trivy with provided configuration;
- upload results to a **Blob Storage** service.

## Implementation details

The core part of the scanner is built around a single container-image scan, which can be shipped as:

- stand-alone service, which itself listens to a new scan-requests from the **Messaging Service**;
- [FaaS](https://en.wikipedia.org/wiki/Function_as_a_service) solution, which takes care of scan-requests listening and then just invokes core-scanner.

The scanner adjusts to the underlying Trivy app - it's written in golang and Trivy itself is referenced as golang dependency.

The scanner consists of modules:

- `configuration` - reads config;
- `container-registry` - is able by container-image tag return container-registry credentials. The initial version takes credentials from configuration file, later could be replaced with secure secrets storage;
- `blob-uploader` - knows how to upload scan results to **Blob Storage**;
- `core` - glues all pieces together: instantiate correct object types based on config, retrieves container-registry credentials, invokes trivy, uploads scan-results;
- (optional) `message-listener` - in-case of stand-alone service, it's a service entry point, which reads scan-requests from a queue;
- (optional) `one-time-runner` - in case of FaaS, only scans provided image: invokes `core` module once.

## Configuration

- trivy-specific config should be located at path specified in `TRIVY_CONFIG_PATH` environment variable path. Config has:
  - `heartbeat-period` in seconds to indicate how often the scanner app should update metadata file
  - `registries` - array of container-registries credentials (address, username, password)
- exact Blob Storage service implementation is constructed based on `BLOB_STORAGE_SERVICE_TYPE` environment variable;
- exact Messaging Service implementation is constructed based on `MESSAGING_SERVICE_TYPE` environment variable;
- `SCANNER_IDENTIFIER` is deployment-time scanner identifier;
- `LOG_FORMAT` - `plain-text` or `json`.

## Blob Storage service

At the moment only Azure Blob Storage is supported.

### Azure Blob Storage

To access the service the scanners uses official azure library [azure-storage-blob-go](https://github.com/Azure/azure-storage-blob-go).

Permission to write data to exact folder is given with [Shared Access Signature](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview), provided through environment variable `AZ_BLOB_STORAGE_SAS` (**TODO:** use secure secrets storage instead of env-var).

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

Each file upload is retried 5 times with exponential backoff. If still failing - `meta` file has human-readable explanation in `failure-description` property.

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

In case of FaaS runtime model, glueing the scanner with **Messaging Service** is platform specific and is out of this document scope.

In case of stand-alone application, the scanner should support horizontal scaling. To achieve that, it should be enough to read scan-requests from the queue in competing mode:

- set `Visibility Timeout` at read time: once a message is requested from the Messaging Service - it should not be visible to other consumers;
- *safely* try to scan an image if succeded - delete the message from the queue;
- if action failed because of a transient error, do an [exponential backoff](https://en.wikipedia.org/wiki/Exponential_backoff): based on message dequeue count update message visibility timeout in **Messaging service**.
- if dequeue count is more than 3 or processing failed because of non-transient error, move [a poisoned message to the quarantine queue](https://alexandrebrisebois.wordpress.com/2013/08/14/poison-queues-are-a-must/).

Visibility timeout prevents multiple instances of the scanner to process the same message in the same time. Exponential backoff helps to deal with transient failures (for example, external dependency is not available just right now, but might work fine after 30 seconds). Quarantine queue avoids a poisoned message to stuck in the queue forever (later human-operator might review the queue and return messages to a normal queue if there was a problem, which was fixed afterward).

At the moment only [Azure Queue Storage](https://docs.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction) is supported.

### Azure Queue Storage

To access Azure Queue Storage the scanner uses official [azure-storage-queue-go](https://github.com/Azure/azure-storage-queue-go) library.

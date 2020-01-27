# kube-bench Scanner

- [kube-bench Scanner](#kube-bench-scanner)
  - [Implementation details](#implementation-details)
  - [Configuration](#configuration)
  - [Blob Storage service](#blob-storage-service)
    - [Azure Blob Storage](#azure-blob-storage)
    - [Audit result format](#audit-result-format)
    - [Scanner metadata](#scanner-metadata)

The scanner wraps [kube-bench by Aqua Security](https://github.com/aquasecurity/kube-bench), which checks whether Kubernetes is deployed securely by running the checks documented in the [CIS Kubernetes Benchmark](https://www.cisecurity.org/benchmark/kubernetes/).

The scanner itself does not add anything new to the original kube-bench application. You can consider it only as a thin kube-bench audit data shipper:

- invoke kube-bench with provided configuration;
- upload results to a **Blob Storage** service.

## Implementation details

The scanner adjusts to the underlying `kube-bench` app - it's written in golang and adds only a Blob Storage abstraction on top of it. `kube-bench` itself is referenced as golang dependency.

## Configuration

- `kube-bench` config should be located at path specified in `KUBEBENCH_CONFIG_PATH` environment variable path;
- exact Blob Storage service implementation is constructed based on `BLOB_STORAGE_SERVICE_TYPE` environment variable;
- `SCANNER_IDENTIFIER` and `SCANNER_PERIODICITY` is deployment-time scanner identifier and periodicity;
- `LOG_FORMAT` - `plain-text` or `json`.

## Blob Storage service

At the moment only Azure Blob Storage is supported.

### Azure Blob Storage

To access the service the scanners uses official azure library [azure-storage-blob-go](https://github.com/Azure/azure-storage-blob-go).

Permission to write data to exact folder is given with [Shared Access Signature](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview), provided through environment variable `AZ_BLOB_STORAGE_SAS` (**TODO:** use secure secrets storage instead of env-var).

### Audit result format

Uploaded audit results should follow the general [technical design doc](/TECH_DESIGN.md#backend-and-scanners), where

- `scanner-type` is `kube-bench`;
- `scanner-id` is `SCANNER_IDENTIFIER` environment variable;
- `scanner-periodicity` is `SCANNER_PERIODICITY` environment variable;

Each audit result folder should have two files:

- `meta` - json object, which describes audit metadata:
  - `audit-id`
  - `scanner-version`
  - `timestamp` - unix epoch in seconds;
  - `audit-result` - `succeeded`, `audit-failed`, `upload-failed`;
  - `failure-description` - (optional) if `audit-result` is failed, explains the reason;
  - `kube-bench-version`
  - `kube-bench-audit-path` - path to file with audit result
- `audit.json` - `kube-bench` audit result;

Audit result file upload is retried 5 times with exponential backoff. If still failing - `meta` file has human-readable explanation in `failure-description` property.

### Scanner metadata

After each audit iteration is completed, the scanner should update general metadata file at `/kube-bench-{scanner-id-short-hash}/kube-bench-{scanner-id-short-hash}.meta` with schema:

```json
{
  "scanner-type": "kube-bench",
  "scanner-id": "{UUID}",
  "scanner-periodicity": "on-cron-{cron-expression}",
  "heartbeat-periodicity": "int",
  "heartbeat": 1579620202
}
```

Where `heartbeat-periodicity` is scanner-periodicity in seconds.

# Polaris Scanner

- [Polaris Scanner](#polaris-scanner)
  - [Implementation details](#implementation-details)
  - [Configuration](#configuration)
  - [Blob Storage service](#blob-storage-service)
    - [Azure Blob Storage](#azure-blob-storage)
    - [Audit result format](#audit-result-format)
    - [Scanner metadata](#scanner-metadata)

The scanner wraps [Polaris by Fairwinds](https://github.com/FairwindsOps/polaris) - kubernetes objects best-practices validator.

The scanner itself does not add anything new to the original polaris application. You can consider it only as a thin polaris audit data shipper:

- invoke polaris with provided configuration;
- upload results to a **Blob Storage** service.

Please, refer to generic `scanners` [README](/src/scanners/README.md) for common scanners requirements.

## Implementation details

The scanner adjusts to the underlying Polaris app - it's written in golang and adds only a Blob Storage abstraction on top of it. Polaris itself is referenced as golang dependency.

## Configuration

- Polaris config should be located at path specified in `POLARIS_CONFIG_PATH` environment variable path;
- exact Blob Storage service implementation is constructed based on `BLOB_STORAGE_SERVICE_TYPE` environment variable;
- if is running out of the target cluster, `KUBECONFIG_PATH` environment variable should point to kubeconfig file;
- `SCANNER_IDENTIFIER` and `SCANNER_PERIODICITY` is deployment-time scanner identifier and periodicity;
- `LOG_FORMAT` - `plain-text` or `json`.

## Blob Storage service

At the moment only Azure Blob Storage is supported.

### Azure Blob Storage

To access the service the scanners uses official azure library [azure-storage-blob-go](https://github.com/Azure/azure-storage-blob-go).

Permission to write data to exact folder is given with [Shared Access Signature](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview), provided through environment variable `AZ_BLOB_STORAGE_SAS` (**TODO:** use secure secrets storage instead of env-var).

### Audit result format

Uploaded audit results should follow the general [technical design doc](/TECH_DESIGN.md#backend-and-scanners), where

- `scanner-type` is `polaris`;
- `scanner-id` is `SCANNER_IDENTIFIER` environment variable;
- `scanner-periodicity` is `SCANNER_PERIODICITY` environment variable;

Each audit result folder should have three files:

- `meta` - json object, which describes audit metadata:
  - `audit-id`
  - `scanner-version`
  - `timestamp` - unix epoch in seconds;
  - `audit-result` - `succeeded`, `audit-failed`, `upload-failed`;
  - `failure-description` - (optional) if `audit-result` is failed, explains the reason;
  - `polaris-version`
  - `polaris-audit-path` - path to file with audit result
  - `kubeclient-version`
  - `k8s-meta-path` - path to file with kubernetes metadata snapshot
- `audit.json` - `polaris` audit result;
- `k8s-meta.json` - kubernetes object metadata snapshot at the moment of auditing.

Each file upload is retried 5 times with exponential backoff. If still failing - `meta` file has human-readable explanation in `failure-description` property.

### Scanner metadata

After each audit iteration is completed, the scanner should update general metadata file at `/polaris-{scanner-id-short-hash}/polaris-{scanner-id-short-hash}.meta` with schema:

```json
{
  "scanner-type": "polaris",
  "scanner-id": "{UUID}",
  "scanner-periodicity": "on-cron-{cron-expression}",
  "heartbeat-periodicity": "int",
  "heartbeat": 1579619671
}
```

Where `heartbeat-periodicity` is scanner-periodicity in seconds.

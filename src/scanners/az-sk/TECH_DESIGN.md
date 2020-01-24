# az-sk Scanner

- [az-sk Scanner](#az-sk-scanner)
  - [Implementation details](#implementation-details)
  - [Configuration](#configuration)
  - [Blob Storage service](#blob-storage-service)
    - [Azure Blob Storage](#azure-blob-storage)
    - [Audit result format](#audit-result-format)
    - [Scanner metadata](#scanner-metadata)

The scanner wraps [Secure DevOps Kit for Azure by Microsoft](https://azsk.azurewebsites.net/index.html) - Azure subscription security validator.

The scanner itself does not add anything new to the original az-sk tool. You can consider it only as a thin az-sk audit data shipper:

- invoke az-sk with provided configuration;
- upload results to a **Blob Storage** service.

**NOTE:** az-sk in most cases is used on Windows OS, because of dependency on PowerShell. However there is [AzSK on PowerShell Core](https://azsk.azurewebsites.net/08-Miscellaneous-Features/Readme.html#try-azsk-on-powershell-core), which should unlock Linux capabilities. (**TODO:** ensure, it's possible to run it in linux container)

Please, refer to generic `scanners` [README](/src/scanners/README.md) for common scanners requirements.

## Implementation details

The scanner adjusts to the underlying az-sk tool - it's written in PowerShell and adds only a Blob Storage abstraction on top of it. `az-sk` itself is referenced as PowerShell module.

## Configuration

The scanner configuration are provided as PowerShell parameters.

- `-SubscriptionId` - identifier of target Azure subscription
- exact Blob Storage service implementation is constructed based on `-BlobStorageType` parameter;
- `-ScannerId` and `-ScannerPeriodicity` is deployment-time scanner identifier and periodicity;

## Blob Storage service

At the moment only Azure Blob Storage is supported.

### Azure Blob Storage

To access the service the scanners uses official azure PowerShell module [Az](https://docs.microsoft.com/en-us/powershell/azure/?view=azps-3.3.0).

Permission to write data to exact folder is given with [Shared Access Signature](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview), provided through parameter `-AzBlobStorageSas`.

### Audit result format

Uploaded audit results should follow the general [technical design doc](/TECH_DESIGN.md#backend-and-scanners), where

- `scanner-type` is `az-sk`;
- `scanner-id` is `SCANNER_IDENTIFIER` environment variable;
- `scanner-periodicity` is `SCANNER_PERIODICITY` environment variable;

Each audit result folder should have three files:

- `meta` - json object, which describes audit metadata:
  - `audit-id`
  - `scanner-version`
  - `timestamp` - unix epoch in seconds;
  - `audit-result` - `succeeded`, `audit-failed`, `upload-failed`;
  - `failure-description` - (optional) if `audit-result` is failed, explains the reason;
  - `az-sk-version`
  - `az-sk-audit-path` - path to file with audit result
- `audit.json` - `az-sk` audit result;

Each file upload is retried 5 times with exponential backoff. If still failing - `meta` file has human-readable explanation in `failure-description` property.

### Scanner metadata

After each audit iteration is completed, the scanner should update general metadata file at `/az-sk-{scanner-id-short-hash}/az-sk-{scanner-id-short-hash}.meta` with schema:

```json
{
  "scanner-type": "az-sk",
  "scanner-id": "{UUID}",
  "scanner-periodicity": "on-cron-{cron-expression}",
  "heartbeat-periodicity": "int",
  "heartbeat": 1579619671
}
```

Where `heartbeat-periodicity` is scanner-periodicity in seconds.

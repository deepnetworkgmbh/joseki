# Scanners

`Scanners` - short-lived applications, that perform a single infrastructure audit. They can be scheduled like a cron job, or be triggered by message in a queue.

Every single application is independent from one another. In most cases they require only read-only access to scanned target and **Blob Storage** to upload audit results. Hegh-level communication overview described at general [tech design file](/TECH_DESIGN.md#backend-and-scanners).

Every scanner, except of `az-sk`, is hosted as docker container.

## Supported Scanner types

*Joseki* supports the following `scanners`:

- [az-sk](/src/scanners/az-sk/TECH_DESIGN.md) to audit azure infrastructure;
- [kube-bench](/src/scanners/kube-bench/TECH_DESIGN.md) to audit kubernetes cluster configuration;
- [polaris](/src/scanners/polaris/TECH_DESIGN.md) to audit kubernetes objects configuration;
- [trivy](/src/scanners/trivy/TECH_DESIGN.md) to scan docker images for known vulnerabilities.

Each link leads to corresponding scanner technical design document.

If you want to add any other scanner type, please create a *github issue* or vote for the existing one. Also *pull requests* are more than welcome ;) Please refer out [contribution guide](/CONTRIBUTING.md).

## Supported Blob Storage implementation

Access to **Blob Storage** service in each `scanner` application is abstracted to have a possibility to use different implementations.

At the moment, *Joseki* supports only [Azure Blob Storage](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-overview).

If you want to add any other **Blob Storage** implementation, please create a *github issue* or vote for the existing one. Also *pull requests* are more than welcome ;) Please refer out [contribution guide](/CONTRIBUTING.md).

### Azure Blob Storage

Each `scanner` application has write-only access to own folder in a single Azure Storage Account.

During the `scanner` provisioning process, A new folder with name `{scanner-type}-{scanner-id-short-hash}` is created and [Shared Access Signature](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview) token is created with write-only permission. (**TODO:** add SAS token rotation).

### Scanner metadata

Each scanner instance should maintain own metadata file at `/{scanner-type}-{scanner-id-short-hash}/{scanner-type}-{scanner-id-short-hash}.meta`

## Supported Message Queue implementation

At the moment, only `trivy` scanners are triggered based on messages from **Message Queue** service. Access to the service in the scanner is abstracted to have a possibility to use different implementations.

At the moment, *Joseki* supports only [Azure Queue Storage](https://docs.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction).

If you want to add any other **Message Queue** implementation, please create a *github issue* or vote for the existing one. Also *pull requests* are more than welcome ;) Please refer out [contribution guide](/CONTRIBUTING.md).

### Azure Queue Storage

...

## Observability

Everything listed below belonds to each scanner except of `az-sk`.

Each `scanners` application writes log-events to standard output. Logs could be serialized in two formats depending on `LOG_FORMAT` environment variable:

- plain-text string
- structured form encoded as `json` string .

The initial version of `scanners` is not going to support _tracing_ or any kind of _metrics_ exposure. If you want to convert log-events to metrics using logs post-processing pipeline.

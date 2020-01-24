# Technical Design

- [Technical Design](#technical-design)
  - [Easy to start, but customizable](#easy-to-start-but-customizable)
  - [Components](#components)
    - [Scanners](#scanners)
    - [Backend](#backend)
    - [Frontend](#frontend)
    - [Infrastructure](#infrastructure)
    - [Inter-service communication](#inter-service-communication)
      - [Frontend and Backend](#frontend-and-backend)
      - [Backend and Scanners](#backend-and-scanners)
  - [Technologies](#technologies)
    - [Supported Blob Storages](#supported-blob-storages)
      - [Azure Blob Storage](#azure-blob-storage)
    - [Supported Messaging Services](#supported-messaging-services)
      - [Azure Queue Storage](#azure-queue-storage)
  - [Observability](#observability)
  - [Development process](#development-process)
    - [Single repository](#single-repository)
    - [Ready to run at any commit in master](#ready-to-run-at-any-commit-in-master)
    - [Getting Started examples](#getting-started-examples)
    - [CI/CD](#cicd)
    - [Adding new scanners types, Blob Storages, Messaging Services, Databases](#adding-new-scanners-types-blob-storages-messaging-services-databases)

*Joseki* should:

- integrate multiple security/configuration related products under a common umbrella

- provide hierarchical scan/audit results:

  - a set of *cloud-infrastructure* audits
  - some products in the *cloud-infrastructure* might have *product-specific* scanners (kubernetes cluster, virtual-machines, etc)
  - *kubernetes audit* might consist of *cluster-configuration* and *object-metadata* audits
  - *virtual-machine scan* can have a set of *docker-container* scans
  - object with associated public-ip (vm, k8s-service) might be scanned with zaproxy.

  Sample hierarchy:

  ```text
  - az-subscription-1
    - vm-1
      - configuration audit
      - packages scan
      - container-1 image scan result
      - container-2 image scan result
    - vm-2
      ...
    - AKS-1
      - k8s-objects audit
        - pod-1
          - image scan result
          - config audit
        - pod-2
        ...
      - k8s-cluster audit
      - service-1 zaproxy scan
    - AKS-2
      ...
  - az-subscription-2
    ...
  ```

- provide extensibility points for adding new

  - scanners
  - persistence layer to store historical data and *Joseki* configuration
  - reporting destinations
  - task-tracking integrations

## Easy to start, but customizable

When a new user starts working with *Joseki*, the application needs to *just work*, but once it works, it needs to be fully customizable and fully adaptable.

Therefore, each service should be able to run with good-enough defaults, but also be flexible to change these defaults by experienced user.

## Components

*Joseki* consists of three main parts:

- `frontend` - a web application, which interacts with end-user;
- `backend` - expose web-api for `frontend` and does the most of business logic: shaping audit data, historical view, reporting, configuration. To simplify the first phase of development, the entire backend is created as a single service.
- `scanners` - a set of applications (one per audit/scan type), that once in a while perform audit/scan operation and uploads raw results to a Blob Storage. Each scanner job can be deployed to different locations: cloud or bare-metal; VMs, kubernetes or [ACI](https://azure.microsoft.com/en-us/services/container-instances/)/[Cloud Run](https://cloud.google.com/run/)/[Fargate](https://aws.amazon.com/fargate/); FaaS.

![General-overview](./docs/diagrams/joseki-white.png)

### Scanners

**Scanners** are independent short-lived applications, that perform a single infrastructure audit.

Each scanner lifecycle is similar to the following:

- being scheduled to a particular compute resources (VM, k8s, FaaS). Scheduling might happen according to user-defined schedule (cron) or triggered by event (message in a queue);
- read target (k8s, cloud resources, docker-image) configuration;
- validates target configuration;
- writes audit results to a **Blob Storage**.

Every single application is independent from one another. In most cases they require only read-only access to scanned target and **Blob Storage** to upload audit results.

Also each scanner instance should maintain own metadata file at **Blob Storage** `/{scanner-type}-{scanner-id-short-hash}/{scanner-type}-{scanner-id-short-hash}.meta`

Every scanner is hosted as docker container.

Detailed scanners technical design is located next to scanner sources: `/src/scanners/{scanner-type}/TECH_DESIGN.md`, where `{scanner-type}` is one of `trivy`, `polaris`, `az-sk`, `kube-bench`.

### Backend

**Backend** - is monolithic application, which encapsulate the most of *Joseki* business logic:

- audit data normalization,
- house-keeping the configuration,
- API for Frontend,
- reporting
- third-party integrations

It exposes API for `Frontend` application and asynchronously communicates with `Scanners` through **Blob Storage** and **Messaging Queue**.

The application is hosted as docker-container and gets own configuration through `settings.json` file and environment variables.

Detailed technical design is described in [backend technical design](./src/backend/TECH_DESIGN.md) document.

### Frontend

`Frontend` is web application to interact with end-user. The application provides interactive dashboards, configuration panel for overall product settings, and others.

The application hosted in docker-container and gets own configuration from configuration file and environment variables.

Detailed technical design is described in [frontent technical design](./src/frontend/TECH_DESIGN.md) document.

### Infrastructure

All the services are wrapped in docker-containers and could run on any infrastructure, which has container runtime.

`Backend` application requires a **Database** to persist:

- normalized scan/audit results;
- `Joseki` configuration
- reports metadata

`Backend` reads audit/scan results from **Blob Storage**.

`Scanners` uses **Blob Storage** to upload audit/scan results and some of them might use **Messaging Service** as trigger source.

### Inter-service communication

There are two types of communication:

- `Frontend` and `Backend`;
- `Backend` and `Scanners`.

#### Frontend and Backend

`Frontend` application depends only on `Backend` REST API. The services communicates through HTTPs. At the moment, user authentication and authorization are not supported, but it's in [product roadmap](./ROADMAP.md).

Available API endpoints are described at `https://{backend-host:port}/swagger`.

#### Backend and Scanners

`Backend` and `Scanners` asynchronously interact through **Blob Storage** and **Messaging Service**:

- `Scanners` uploads audit/scan results and own metadata in agreed format to the service;
- `Backend` reads raw data from **Blob Storage** and writes normalized data to **Database**.

The Messaging Service is used only by `trivy` scanner and `backend` application. Please refer to [Messaging Service](/src/scanners/README.md#messaging-service) section of `trivy` and [Enqueue Image Scan](/src/backend/TECH_DESIGN.md#enqueue-image-scan) section of `backend` design docs for more details.

Each scanner has access to the only one folder in **Blob Storage**, while the entire storage file system might be shared between several scanners. `backend` application has read-only access to the entire Blob Storage. Please, refer to [Process Audit Results](/src/backend/TECH_DESIGN.md#process-audit-results) section of `backend` technical design doc.

The overall **Blob Storage** file system might looks like:

```plain
.
├── az-sk-7b71f5c/
│  ├── az-sk-7b71f5c.meta
│  ├── 20200121-090000-689e203/
│  │   ├── meta
│  │   └── audit.json
│  └── 20200120-090000-321b236/
│      ├── meta
│      └── audit.json
├── polaris-98fa7fb
│  ├── polaris-98fa7fb.meta
│  ├── 20200121-000000-3c2cc77/
│  │   ├── meta
│  │   ├── audit.json
│  │   └── k8s-meta.json
│  └── 20200121-060000-de80123/
│      ├── meta
│      ├── audit.json
│      └── k8s-meta.json
└── trivy-a334676
│  ├── trivy-a334676.meta
│  ├── 20200121-081222-c59f5ef/
│  │   ├── meta
│  │   └── audit.json
│  └── 20200121-081233-a200d35/
│      ├── meta
│      └── audit.json
...
```

Where each root level folder corresponds to a separate scanner instance and contains:

- `{scanner-type}-{scanner-short-id}.meta` file - scanner instance metadata, serialized as json object. The metadata contains
  - `scanner-type` - `as-sk`, `polaris`, `trivy`, `kube-bench`,
  - `scanner-id` - UUID serialized to string,
  - `scanner-periodicity` - `on-cron-{cron-expression}` or `on-message`,
  - `heartbeat-periodicity` - `int` seconds: how often hearbeat property is updated,
  - `heartbeat` - [unix epoch time](https://en.wikipedia.org/wiki/Unix_time) in seconds.
- `{yyyyMMdd-HHmmss}-{hash:7}` - separate folder for each audit. Folder name consist of UTC date and time + 7 random hex characters to ensure uniqueness.
  - audit folder may contain any number of files (depends on scanner type)

## Technologies

For initial implementation *Joseki* uses:

- [Vue.js](https://vuejs.org/) for **Frontend**;
- [dotnet core](https://github.com/dotnet/core) for **Backend**;
- `scanners` programming language/framework vary depending on a tool, which a scanner wraps;
- [Azure PostgreSQL](https://azure.microsoft.com/de-de/services/postgresql/) as **Database**;
- [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/) as **Blob Storage**;
- [Azure Queue Storage](https://azure.microsoft.com/de-de/services/storage/queues/) as **Messaging Service**.

Service-level access to cloud dependencies is abstracted, therefore changing used products later should be possible.

The current choice is based on the most familiar products/framework of the dev-team at the moment of writing.

### Supported Blob Storages

Access to **Blob Storage** service in each application is abstracted to have a possibility to use different implementations.

At the moment, *Joseki* supports only [Azure Blob Storage](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-overview).

#### Azure Blob Storage

During the `scanner` provisioning process, A new folder with name `{scanner-type}-{scanner-id-short-hash}` is created and [Shared Access Signature](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview) token is created with write-only permission. (**TODO:** add SAS token rotation).

### Supported Messaging Services

At the moment, only `trivy` scanners are triggered based on messages from **Message Queue** service. Access to the service in the scanner is abstracted to have a possibility to use different implementations.

At the moment, *Joseki* supports only [Azure Queue Storage](https://docs.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction).

#### Azure Queue Storage

Access to the Queue Storage is restricted with [shared access signatures (SAS)](https://docs.microsoft.com/en-us/azure/storage/common/storage-sas-overview).

`backend` has `add` permission on `image-scan-requests` queue.

`trivy` scanner has `process` persmission on `image-scan-requests` queue and `add` permission on `image-scan-requests-quarantine`.

## Observability

Each application writes log-events to standard output. Logs could be serialized in two formats depending on `LOG_FORMAT` environment variable:

- plain-text string
- structured form encoded as `json` string .

The initial *Joseki* version is not going to support _tracing_ or any kind of _metrics_ exposure.

## Development process

The development process is optimized for a small team, and is focused on keeping sources at deployable state. Therefore, everything is stored in a single repository and pushes to `master` branch are permitted only through pull-requests, protected with ci-pipelines.

When project grows - the repository could be split according to service boundaries.

### Single repository

All the sources, documentation, getting-started samples are stored in the single repository:

- `.github` folder consists of pr-templates, issue-templates, github-actions pipelines.
- `examples` is a set of getting-started scripts/templates to deploy the solution as simple as possible
- `src` folder has four subfolders:
  - `backend`: web-api, data management, reporting, etc,
  - `frontend`: web-application,
  - `infrastructure`: templates/scripts to provision required infrastructure in misc clouds,
  - `scanners` has a dedicated sub-folder for each scanner type.
- `root` has project-level documents: [readme](./README.md), [product overview](./PRODUCTOVERVIEW.md), tech-design (this), [roadmap](./ROADMAP.md), [contributor guidelines](./CONTRIBUTING.md), and others

### Ready to run at any commit in master

Any commit in `master` branch should be in fully-functional state. To ensure this:

- the branch should be protected against direct pushes;
- all the changes should go through pull-requests;
- each PR should be validated with ci-pipeline.

### Getting Started examples

Getting Started examples is vital for the project - they should provide the easiest possible way to start using the application. Therefore, each example should:

- be in functional state (covered with acceptance tests?);
- has a readme explaining what exactly is deployed with it;
- explicitly say if the sample is production ready and if not what should be hardened.

If possible, each sample should has **a single script** to provision the solution.

### CI/CD

The repository has a set of continuous-integration pipelines to protect `master` branch from unsafe code and release trusted docker-images.

Logically, pipelines can be grouped into three categories:

1. _Pull Request_. They validate that code is safe to be merged: compile; run tests/analyzers/linters. Each scanner, service, infrastructure templates might have separate pipeline.
2. _Commit to `master`_. These pipelines main task is to publish ready-to-use docker images. Each job, service, infrastructure templates might have separate pipeline.
3. _Release_ - ensures getting-started samples are in working state, and creates Github release with pinned services versions.

*Continuous Delivery* pipeline, which deploys *Joseki* to the real infrastructure is out of this repository scope.

### Adding new scanners types, Blob Storages, Messaging Services, Databases

If you want to add any other **Message Queue** or **Blob Storage** implementation, add new scanner type - please create a *github issue* or vote for the existing one. Also *pull requests* are more than welcome ;) Please refer our [contribution guide](/CONTRIBUTING.md)

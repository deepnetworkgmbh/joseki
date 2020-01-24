# Product Overview

Security is a massive and complicated topic and there are dozens of open-sourced tools on the market that can help to make a product safer. The tools often are summoned to enforce known best-practices to docker images, kubernetes, and cloud infrastructure at large. However, this approach that relies on many tools comes up with its own set of problems:

- a lot of tools cover just a single aspect of security management
- tools are disconnected and just figuring out how to use them together is a hassle
- often, they have no reporting capabilities and no historical overview.

*Joseki* is here to address these problems.

## What is Joseki

Joseki is an open source security tool that is mainly designed to **audit the configuration of cloud systems**. It differs from other tools by combining various scanners to **target many object types**, reducing the number of tools needed to be learned, installed and maintained.

Audit results are seamlessly combined in the user interface, making it easier to consume and understand. The results are **ranked based on severity** and each discovered issue is accompanied with a **recommended action** to resolve.

Joseki also offers **a historical view** and **reporting** to monitor the security of your systems over time and inform relevant parties from the state of affairs.

### Joseki cornerstones

- *Scans* - scheduled configuration audits
  - Scan periods can be adjusted (e.g. daily, weekly, etc.)
  - Currently, scan targets can be limited via configuration only. In the future, we're planning to allow this via the UI.
- In the V1 version, from the UI you cannot select a subset of targets to be scanned. However, via scanners configuration, you can limit what the scanners can access and will scan.
- Audit different types of objects via different underlying scanners. These objects are:
  - azure cloud infrastructure: databases, networks, vendor-specific products.
  - k8s objects: deployment, statefulset, etc.
  - docker images.
- Rank all found issues based on their severity.
  - The user can override the severity of specific types of issues.
- Suggest remedies or solutions to discovered issues whenever possible.
  - Some problems may not have a solution at the moment. (e.g. a CVE that is recently discovered and is not yet addressed)
- Reporting and historical overview

### Out of scope

- Preventing issues being introduced to a system but rather catch issues on a given system. Therefore, it's not suitable to use as part of CI/CD pipelines and associated tasks.
- Real-time protection - scans/audit are expected to be scheduled daily/hourly.
- Addressing any of the found issues directly. (i.e. you cannot fix any issue from the product itself, it just displays results + suggestions)

## Installation, Hosting, Runtime

Joseki is currently designed to be installed in your systems (i.e. on premise or on cloud). SaaS is currently out of scope.

The product, at a high level, consists of:

- `backend` and `frontend` applications. `Frontend` depends on `Backend` API.
- `scanners` jobs, that depends only on the scanned target.
- auxiliary infrastructure: database, blob storage, messaging service (possible to use a cloud-provider for all of these).

The entire product can be installed into a single node (i.e. a VM) with all of its components, as long as it has access to targets to be scanned. We recommend installing the product into your private network and not exposing publicly as it doesn't support any authentication.

Individual scanners *can be* installed separately and scaled horizontally. This depends on the scanner type and would require some configuration during the installation. For example, multiple instances of `trivy` can be installed and the product would divide the work between these instances to increase throughput. It may not necessarily be as simple for other types of scanners due to the nature of the scan being performed.

The product needs read-only access to targets to be scanned (cloud-vendor and/or kubernetes APIs). Scanners have each their own configuration. They can be enabled or disabled based on needs.

The product `frontend` is a SPA that runs on the browser. The `backend` exposes a set of APIs that are designed to be consumed by the UI. Currently, the APIs are not meant to be used by other developers.

The installation requires system or platform engineer who understands system topology and security implications of installed components. However, the end-product (web-interface) could be used by any team members: dev/ops/dev-ops teams, security experts, and management.

## Roadmap

### V1 - Initial Release

- Scanner Types:
  - Container Image vulnerabilities scanner [trivy](https://github.com/aquasecurity/trivy)
  - Kubernetes objects validator [polaris](https://github.com/FairwindsOps/polaris)
  - Kubernetes cluster configuration validator [kube-bench](https://github.com/aquasecurity/kube-bench)
  - Azure infrastructure auditor [az-sk](https://github.com/azsk/DevOpsKit)

- Historical data:
  - The users can see the past scans (i.e. scan x done on day y)
  - The users can diff the results of two scans.

- Reporting:
  - The product supports on demand reports (i.e. the user explicitly requests a scan report)
  - System can be configured to automatically send reports / scan results to predefined e-mail addresses.
  - The user can see previously generated reports
  - The product can produce scheduled reports
    - can be sent to email
    - can be downloaded later from the product
  - The product has multiple report types:
    - the current state of the system,
    - diff from the last report,
    - include only subset of scanners to report on.

- All types of checks will have a unique identifier.
  - (Implementation detail: every scanner already has its own internal unique identifiers. We can just add our own prefix to them)

- The product supports suppressions / tolerations.
  - User can specifically suppress an issue or basically ask it to be not reported.
  - This can be done via the UI or can be done via a configuration file.

### V2

- New scanner types:
  - Web application scanners. For example, [ZAProxy](https://github.com/zaproxy/zaproxy)
  - Cloud infrastructure auditors. For example, [cloud-sploit](https://github.com/cloudsploit), [scout-suite](https://github.com/nccgroup/ScoutSuite), [security-monkey](https://github.com/Netflix/security_monkey).
  - Augment kube-bench with [kube-hunter](https://github.com/aquasecurity/kube-hunter)
  - Kubernetes anomaly detectors. For example, [falco](https://github.com/falcosecurity/falco)
  - Scan VM configurations / images
- Role Based Authentication (RBAC) 
- Role Based Views
- Support Attestation
- creating new security checks (likely, [Open Policy Agent](https://www.openpolicyagent.org/) integration)
- Issue tracking: integrates with Jira / Azure DevOps etc
  - Create an issue/task etc in the bug tracking system for a found issue.
  - Status of these issues can be tracked from the product.
- Support triaging of individual issues and taking actions.
- A manual scan can be triggered from the UI.
- The product exposes underlying target discovery.
  - It lists all the scannable targets discovered to the user
  - Product gives instructions how to install these scanners manually and make sure scanners have access to all discovered targets
- The product could expose check-results as metrics, that can be visualized in 3rd party tools (for example, Grafana) or be alerted (for example, alert-manager or Grafana). In this case we need to decide on the interface of this (Prometheus metrics schema is probably a good idea).
- Suppressions can have a time limit. i.e. suprress for a month etc
- Publish schema for scanner / backend integration (and / or) backend / UI integration to make it easier for 3rd parties to add their scanner to our product

### V3

- Automated scanners provisioning/deprovisioning:
  - User can select/unselect targets from the *target discovery* list.
  - The product supports scanners provisioning from UI
- alternative scanners:
  - Container Image vulnerabilities scanners. For example, [clair](https://github.com/quay/clair).
  - Kubernetes objects validators. For example, [cluster-lint](https://github.com/digitalocean/clusterlint).
- The product runs in read & write mode and can - in some cases - actively fix/address an issue (e.g. `kubelet` config)
- Cost analysis under this umbrella?

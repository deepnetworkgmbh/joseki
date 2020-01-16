# Product Overview

Security is a massive and complicated topic, but there are dozens of open sourced tools on the market that can help to make a product safer. The tools often are summoned to force known best-practices to docker images, kubernetes, cloud infrastructure.

Unfortunately, there are problems:

- a lot of tools cover just a single aspect of security management
- tools are disconnected and just figuring out how to use them together is a hassle
- often, they have no reporting capabilities and no historical overview.

*Kubegaard* comes to fill in these gaps.

## Kubegaard cornerstones

Fundamentally the product focuses on configuration that you **already** have in your system and help you to improve it.

- The product supports scheduled configuration audits/scans.
  - The schedule can be configured to run with certain periods
  - In the V1 version, from the UI you cannot select a subset of targets to be scanned. However, via scanners configuration, you can limit what the scanners can access and will scan.

- The product can audit different types of objects via different underlying scanners. These objects are:
  - cloud infrastructure: databases, networks, vendor-specific products.
  - k8s objects: deployment, statefulset, etc.
  - docker images.

- The product ranks all found issues based on their severity.
  - The user can override the severity of specific types of issues.

- The product suggests remedies or solutions to discovered issues whenever possible.
  - a CVE that is discovered may not be addressed yet. There may not be a solution to offer.

- The product offers reporting and historical overview.

**The V1 version of the product is not designed**:

- to prevent issues being introduced to a system but rather catch issues on a given system. Therefore, it's not suitable to use as part of CI/CD pipelines and associated tasks.
- for real-time protection - scans/audit are expected to be scheduled daily/hourly.
- to address any of found issues directly. (i.e. you cannot fix any issue from the product itself, it just displays results + suggestions)

## Installation, Hosting, Runtime

- Initial version is designed to be installed to the customer's own systems (i.e. on premise or on cloud).
  - SaaS is out of scope currently

- The product consists of:
  - `backend` and `frontend` applications. `Frontend` depends on `Backend` API.
  - `scanners` jobs, that depends only on scanned target.
  - auxiliary infrastructure: database, blob storage, messaging service (possible to use a cloud-provider for all of these).

- The product can be installed into a single node / VM with all of its components, as long as it has access to targets to be scanned.
  - Individual scanners *might be* installed separately and be horizontally scalable. This depends on the scanner type and would require some configuration during the installation.
    - For example: Multiple instances of `trivy` can be installed and the product would divide the work between these instances to increase throughput.

- The product needs read-only access to targets to be scanned (cloud-vendor and/or kubernetes apis).

- The product installation supports configuration.
  - individual scanners can be enabled / disabled.

- The product UI runs on the browser.

- The product API is meant to be used by the UI only and is not designed to be used by other developers atm.

- The V1 version of the product is expected to be used in customer's private network.

- Installation process requires system/platform engineer, who understands system topology and security implications of installed components. However the end-product (web-interface) could be used by any team members: dev/ops/dev-ops teams, security experts, management.

## V1 Features

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
  - User can specifically suppress an issue or basically asked it to be not reported.
  - This can be done via the UI or can be done via a configuration file.

## Further enhancement

## V2

- New scanner types:
  - Web application scanners. For example, [ZAProxy](https://github.com/zaproxy/zaproxy)
  - Cloud infrastructure auditors. For example, [cloud-sploit](https://github.com/cloudsploit), [scout-suite](https://github.com/nccgroup/ScoutSuite), [security-monkey](https://github.com/Netflix/security_monkey).
  - Augment kube-bench with [kube-hunter](https://github.com/aquasecurity/kube-hunter)
  - Kubernetes anomaly detectors. For example, [falco](https://github.com/falcosecurity/falco)
- RBAC
- creating new security checks (likely, [Open Policy Agent](https://www.openpolicyagent.org/) integration)
- Issue tracking: integrates with Jira / Azure DevOps etc
  - Create an issue/task etc in the bug tracking system for a found issue.
  - Status of these issues can be tracked from the product.
- A manual scan can be triggered from the UI.
- The product supports target discovery.
  - It lists all the scannable targets discovered to the user
  - User can select/unselect targets from the list.
  - The product supports scanners provisioning from UI
- The product could expose check-results as metrics, that can be visualized in 3rd party tools (for example, Grafana) or be alerted (for example, alert-manager or Grafana). In this case we need to decide on the interface of this (Prometheus metrics schema is probably a good idea).

## V3

- alternative scanners:
  - Container Image vulnerabilities scanners. For example, [clair](https://github.com/quay/clair).
  - Kubernetes objects validators. For example, [cluster-lint](https://github.com/digitalocean/clusterlint).
- The product runs in read & write mode and can - in some cases - actively fix/address an issue (e.g. `kubelet` config)
- Cost analysis under this umbrella?

# Joseki

![Backend ci status](https://github.com/deepnetworkgmbh/joseki/workflows/be_ci_master/badge.svg)
![Frontend ci status](https://github.com/deepnetworkgmbh/joseki/workflows/fe_ci_master/badge.svg)
![Scanner azsk ci status](https://github.com/deepnetworkgmbh/joseki/workflows/scanner_azsk_ci_master/badge.svg)
![Scanner trivy ci status](https://github.com/deepnetworkgmbh/joseki/workflows/scanner_trivy_ci_master/badge.svg)
![Scanner polaris ci status](https://github.com/deepnetworkgmbh/joseki/workflows/scanner_polaris_ci_master/badge.svg)

*Joseki* is a set of services to help you keeping cloud-infrastructure, kubernetes, and docker-images configuration closer to known best-practices.

The project integrates multiple open-sourced tools:

- Container Image vulnerabilities scanner [trivy](https://github.com/aquasecurity/trivy).
- Kubernetes objects validator [polaris](https://github.com/FairwindsOps/polaris).
- Cloud infrastructure auditor [az-sk](https://github.com/azsk/DevOpsKit)

More are coming ;) Take a look on the project [Overview](./PRODUCTOVERVIEW.md), [Technical Design](./TECH_DESIGN.md), and [Roadmap](./PRODUCTOVERVIEW.md#roadmap).

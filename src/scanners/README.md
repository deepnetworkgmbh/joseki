# Scanners

`Scanners` - short-lived applications, that perform a single infrastructure audit. They can be scheduled like a cron job, or be triggered by a message in a queue.

## Supported Scanner types

*Joseki* supports the following `scanners`:

- [az-sk](/src/scanners/az-sk/TECH_DESIGN.md) to audit azure infrastructure;
- [kube-bench](/src/scanners/kube-bench/TECH_DESIGN.md) to audit kubernetes cluster configuration;
- [polaris](/src/scanners/polaris/TECH_DESIGN.md) to audit kubernetes objects configuration;
- [trivy](/src/scanners/trivy/TECH_DESIGN.md) to scan docker images for known vulnerabilities.

Each link leads to corresponding scanner technical design document.

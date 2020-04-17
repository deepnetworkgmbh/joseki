# Joseki deployment

`Joseki` consists of `backend`, `frontend`, `trivy-scanner`, `azsk-scanner`, `polaris-scanner` services that depend on `Blob Storage`, `Queue Service`, and `Database`.

`Joseki` services are designed to be platform-agnostic, so it could run at any container-orchestration platform, or just on barebone VMs. But at the moment, only kubernetes-based deployment is tested.

`src/deploy` folder contains a set of scripts, which could be used to provision the entire solution in one go (for example, [all_in_one_az_k8s.sh](src/deploy/all_in_one_az_k8s.sh)), or part by part (for example, [deploy_frontend.sh](src/deploy/frontend/deploy_frontend.sh) or [deploy_azsk_scanner_job.sh](src/deploy/azsk/deploy_azsk_scanner_job.sh)). Each script file has `usage()` function, which describes all supported parameters and associated rules.

## Complete deployment samples

The rest of the document provides code-samples that illustrates complete deployment to different environments.

### Kubernetes + Azure

This sample uses a bash script and requires installed `az cli`, `kubectl` packages.

**Ensure the following _requirements_ are met before running the script**:

- `az cli` logged into a subscription, where `Joseki` components should be created;
- `kubectl` pointing to a kubernetes context, where `Joseki` services should be deployed;
- Kubernetes cluster provisioned in Azure VNet with enabled `Microsoft.SQL` Service Endpoint;
- `AzSK` needs a Service Principal (SP) with _Reader_ role in audited subscriptions:
  - required SP could be created as part of the deployment process, but in such a case logged-in account should have permission to create new SPs;
  - or you could provide existing SP id, password, tenant-id to avoid the creation of a new one.

_Outcome_:

- Azure Resource Group with `Storage Account`, `MS SQL Server`, and `Key Vault` to save sensitive data required for further deployments,
- Kubernetes namespace with
  - `backend`, `frontend`, `trivy` applications,
  - `azsk-scanner` and `polaris-scanner` cron-jobs, which runs each day at `02:00AM UTC`,
  - `azsk-scanner` and `polaris-scanner` one-time jobs, which audits the current state of the system.
- `joseki.env` file, which could be reused for subsequent k8s application releases.

The script could be invoked as

```bash
(cd src/deploy && ./all_in_one_az_k8s.sh -s "subscription-id1 subscription-id2" -k "/subscriptions/{SubID}/resourceGroups/{ResourceGroup}/providers/Microsoft.Network/virtualNetworks/{VNETName}/subnets/{SubnetName}'")
```

It also supports more input parameters. Please, check the script `usage()` function for more details.

Once the deployment is done, the application could be accessed at `http://localhost:8080` after running:

- `kubectl port-forward svc/joseki-be -n joseki 5001:80`
- `kubectl port-forward svc/joseki-fe -n joseki 8080:80`

*NOTE: likely, kubernetes audit data is available almost immediately, while Azure audit arrives within 10-30 minutes, as azsk requires more time to perform all the checks.*

#!/bin/bash

# exit on any errors (except conditional checks of executed commands)
set -e

ENV_FILE="joseki.env"

BASE_NAME="joseki"
LOCATION="westeurope"
K8S_NAMESPACE="joseki"

FRONTEND_VERSION=$(< "../frontend/version")
BACKEND_VERSION=$(< "../backend/version")
SCANNER_TRIVY_VERSION=$(< "../scanners/trivy/version")
SCANNER_AZSK_VERSION=$(< "../scanners/az-sk/version")
SCANNER_POLARIS_VERSION=$(< "../scanners/polaris/version")

SUBSCRIPTIONS=""
SP_ID=""
SP_PASSWORD=""
TENANT_ID=""
K8S_SUBNET_ID=""
APP_DOMAIN=""
CLIENT_ID=""

usage() {
  echo "Usage: $0 -k K8S_SUBNET_ID -s SUBSCRIPTIONS [ -i SP_ID ] [ -p SP_PASSWORD ] [ -t TENANT_ID ] [ -b BASE_NAME ] [ -l LOCATION ] [ -n K8S_NAMESPACE ] [ -d APP_DOMAIN ]" 1>&2 
  echo ""
  echo "The script creates all Joseki components in a single run: Azure infrastructure and k8s objects"
  echo ""
  echo "Joseki uses AzSK scanner, which requires AAD Service Principal with READER role in audited subscriptions. If Service Principal credentials (SP_ID, SP_PASSWORD, TENANT_ID) are not provided, the script will create a new object in Azure Active Directory with READER role in each given subscription from SUBSCRIPTIONS parameter"
  echo ""
  echo "-k (required) - Kubernetes cluster Azure VNet subnet identifier in format '/subscriptions/{SubID}/resourceGroups/{ResourceGroup}/providers/Microsoft.Network/virtualNetworks/{VNETName}/subnets/{SubnetName}'"
  echo "-s (required) - list of Azure subscription identifiers separated by whitespace, for example 'subscription1 subscription2 subscription3'"
  echo "-i (optional) - Service Principal identifier"
  echo "-p (optional) - Service Principal password"
  echo "-t (optional) - Service Principal Tenant identifier"
  echo "-b (optional) - base name for all Azure resources. If empty, used default 'joseki' value"
  echo "-l (optional) - Azure resources location. If empty, used default 'westeurope' value"
  echo "-n (optional) - Kubernetes namespaces to deploy services to. If empty, used default 'joseki' value"
  echo "-d (optional) - Azure AD App Domain. If empty, auth is disabled."
  echo "-c (optional) - Azure AD App Client ID."
}

exit_abnormal() {
  usage
  exit 1
}

while getopts b:l:n:k:s:i:p:t:d:c: option
do
    case "${option}" in
        b) BASE_NAME=${OPTARG};;
        l) LOCATION=${OPTARG};;
        n) K8S_NAMESPACE=${OPTARG};;
        k) K8S_SUBNET_ID=${OPTARG};;
        s) SUBSCRIPTIONS=${OPTARG};;
        i) SP_ID=${OPTARG};;
        p) SP_PASSWORD=${OPTARG};;
        t) TENANT_ID=${OPTARG};;
        d) APP_DOMAIN=${OPTARG};;
        c) CLIENT_ID=${OPTARG};;
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

if [ "$SUBSCRIPTIONS" = "" ]; then
  echo "SUBSCRIPTIONS parameter is required"
  exit_abnormal
fi

if [ "$K8S_SUBNET_ID" = "" ]; then
  echo "K8S_SUBNET_ID parameter is required"
  exit_abnormal
fi

if [ ! -f "$ENV_FILE" ]; 
then
  (cd ./infrastructure && ./azure.sh -b "$BASE_NAME" -l "$LOCATION" -f "./../$ENV_FILE" -n "$K8S_SUBNET_ID")

  KEY_VAULT_NAME=$(< $ENV_FILE grep KEY_VAULT_NAME | cut -d' ' -f2)
  (cd ./infrastructure && ./service_principal.sh -k "$KEY_VAULT_NAME" -s "$SUBSCRIPTIONS" -i "$SP_ID" -i "$SP_PASSWORD" -i "$TENANT_ID")

  POLARIS_SCANNER_ID=$(cat /proc/sys/kernel/random/uuid || /usr/bin/uuidgen | tr "[:upper:]" "[:lower:]")
  AZSK_SCANNER_ID=$(cat /proc/sys/kernel/random/uuid || /usr/bin/uuidgen | tr "[:upper:]" "[:lower:]")
  TRIVY_SCANNER_ID=$(cat /proc/sys/kernel/random/uuid || /usr/bin/uuidgen | tr "[:upper:]" "[:lower:]")
  echo "POLARIS_SCANNER_ID $POLARIS_SCANNER_ID
AZSK_SCANNER_ID $AZSK_SCANNER_ID
TRIVY_SCANNER_ID $TRIVY_SCANNER_ID" >> "$ENV_FILE"
else
  POLARIS_SCANNER_ID=$(< $ENV_FILE grep POLARIS_SCANNER_ID | cut -d' ' -f2)
  AZSK_SCANNER_ID=$(< $ENV_FILE grep AZSK_SCANNER_ID | cut -d' ' -f2)
  TRIVY_SCANNER_ID=$(< $ENV_FILE grep TRIVY_SCANNER_ID | cut -d' ' -f2)

fi

STORAGE_ACCOUNT_NAME=$(< $ENV_FILE grep STORAGE_ACCOUNT_NAME | cut -d' ' -f2)
SQLSERVER_NAME=$(< $ENV_FILE grep SQLSERVER_NAME | cut -d' ' -f2)
SQLDB_NAME=$(< $ENV_FILE grep SQLDB_NAME | cut -d' ' -f2)
KEY_VAULT_NAME=$(< $ENV_FILE grep KEY_VAULT_NAME | cut -d' ' -f2)

# Read ClientID from ENV to see if a successful registration was done
CLIENT_ID=$(< $ENV_FILE grep CLIENT_ID | cut -d' ' -f2)

EXISTING_NS=$(kubectl get ns "$K8S_NAMESPACE" -o name --ignore-not-found)
if [ -z "$EXISTING_NS" ]; then
  echo "Creating namespace $K8S_NAMESPACE"
  kubectl create ns "$K8S_NAMESPACE"
fi

# Call registerapp.sh if APP_DOMAIN is provided but there is no previous successful execution.
if [[ "$APP_DOMAIN" != "" && "$CLIENT_ID" == "" ]]; then
  (cd ./auth && ./registerapp.sh -b "$BASE_NAME" -k "$KEY_VAULT_NAME" -t "$TENANT_ID" -d "$APP_DOMAIN" -f "./../$ENV_FILE")
fi

(cd ./trivy && ./deploy_trivy_scanner.sh -t "$SCANNER_TRIVY_VERSION" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -i "$TRIVY_SCANNER_ID")

(cd ./azsk && ./deploy_scheduled_azsk_scanner.sh -t "$SCANNER_AZSK_VERSION" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -k "$KEY_VAULT_NAME" -s "$SUBSCRIPTIONS" -i "$AZSK_SCANNER_ID")
(cd ./azsk && ./deploy_azsk_scanner_job.sh -t "$SCANNER_AZSK_VERSION" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -k "$KEY_VAULT_NAME" -s "$SUBSCRIPTIONS" -i "$AZSK_SCANNER_ID")

(cd ./polaris && ./deploy_scheduled_polaris_scanner.sh -t "$SCANNER_POLARIS_VERSION" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -i "$POLARIS_SCANNER_ID")
(cd ./polaris && ./deploy_polaris_scanner_job.sh -t "$SCANNER_POLARIS_VERSION" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -i "$POLARIS_SCANNER_ID")

(cd ./backend && ./deploy_backend.sh  -t "$BACKEND_VERSION" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -s "$SQLSERVER_NAME" -d "$SQLDB_NAME" -k "$KEY_VAULT_NAME")

(cd ./frontend && ./deploy_frontend.sh -t "$FRONTEND_VERSION" -n "$K8S_NAMESPACE" -k "$KEY_VAULT_NAME")

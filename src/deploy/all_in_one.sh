#!/bin/bash

# exit on any errors (except conditional checks of executed commands)
set -e

ENV_FILE="joseki.env"

BASE_NAME="joseki"
K8S_NAMESPACE="joseki"

SUBSCRIPTIONS=""
K8S_SUBNET_ID=""

POLARIS_SCANNER_ID=$(cat /proc/sys/kernel/random/uuid)
AZSK_SCANNER_ID=$(cat /proc/sys/kernel/random/uuid)
TRIVY_SCANNER_ID=$(cat /proc/sys/kernel/random/uuid)


usage() {
  echo "Usage: $0 -s SUBSCRIPTIONS -i K8S_SUBNET_ID  [ -b BASE_NAME ] [ -n K8S_NAMESPACE ][ -p POLARIS_SCANNER_ID ] [ -a AZSK_SCANNER_ID ] [ -t TRIVY_SCANNER_ID ]" 1>&2 
  echo ""
  echo "-s (required) - list of Azure subscription identifiers separated by whitespace, for example 'subscription1 subscription2 subscription3'"
  echo "-i (required) - Kubernetes cluster Azure VNet subnet identifier in format '/subscriptions/{SubID}/resourceGroups/{ResourceGroup}/providers/Microsoft.Network/virtualNetworks/{VNETName}/subnets/{SubnetName}'"
  echo "-b (optional) - base name for all Azure resources. If empty, used default 'joseki' value"
  echo "-n (optional) - Kubernetes namespaces to deploy services to. If empty, used default 'joseki' value"
  echo "-p (optional) - polaris scanner identifier. If empty, it's generated from /proc/sys/kernel/random/uuid"
  echo "-a (optional) - azsk scanner identifier. If empty, it's generated from /proc/sys/kernel/random/uuid"
  echo "-t (optional) - trivy scanner identifier. If empty, it's generated from /proc/sys/kernel/random/uuid"
  echo ""
}

exit_abnormal() {
  usage
  exit 1
}

while getopts b:n:s:i:p:a:t: option
do
    case "${option}" in
        b) BASE_NAME=${OPTARG};;
        n) K8S_NAMESPACE=${OPTARG};;
        s) SUBSCRIPTIONS=${OPTARG};;
        i) K8S_SUBNET_ID=${OPTARG};;
        p) POLARIS_SCANNER_ID=${OPTARG};;
        a) AZSK_SCANNER_ID=${OPTARG};;
        t) TRIVY_SCANNER_ID=${OPTARG};;
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

if [ ! -f "$ENV_FILE" ]; then
  (cd ./infrastructure && ./azure.sh -b "$BASE_NAME" -f "./../$ENV_FILE" -s "$SUBSCRIPTIONS" -n "$K8S_SUBNET_ID")
fi

STORAGE_ACCOUNT_NAME=$(< $ENV_FILE grep STORAGE_ACCOUNT_NAME | cut -d' ' -f2)
SQLSERVER_NAME=$(< $ENV_FILE grep SQLSERVER_NAME | cut -d' ' -f2)
SQLDB_NAME=$(< $ENV_FILE grep SQLDB_NAME | cut -d' ' -f2)
KEY_VAULT_NAME=$(< $ENV_FILE grep KEY_VAULT_NAME | cut -d' ' -f2)

EXISTING_NS=$(kubectl get ns "$K8S_NAMESPACE" -o name --ignore-not-found)
if [ -z "$EXISTING_NS" ]; then
  echo "Creating namespace $K8S_NAMESPACE"
  kubectl create ns "$K8S_NAMESPACE"
fi

(cd ./trivy && ./deploy_trivy_scanner.sh -t "0.2.1" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -i "$TRIVY_SCANNER_ID")

(cd ./azsk && ./deploy_scheduled_azsk_scanner.sh -t "0.2.2" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -k "$KEY_VAULT_NAME" -s "$SUBSCRIPTIONS" -i "$AZSK_SCANNER_ID")
(cd ./azsk && ./deploy_azsk_scanner_job.sh -t "0.2.2" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -k "$KEY_VAULT_NAME" -s "$SUBSCRIPTIONS" -i "$AZSK_SCANNER_ID")

(cd ./polaris && ./deploy_scheduled_polaris_scanner.sh -t "0.2.2" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -i "$POLARIS_SCANNER_ID")
(cd ./polaris && ./deploy_polaris_scanner_job.sh -t "0.2.2" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -i "$POLARIS_SCANNER_ID")

(cd ./backend && ./deploy_backend.sh  -t "0.2.15" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -s "$SQLSERVER_NAME" -d "$SQLDB_NAME" -k "$KEY_VAULT_NAME")

(cd ./frontend && ./deploy_frontend.sh -t "0.2.22" -n "$K8S_NAMESPACE")

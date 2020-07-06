#!/bin/bash
# shellcheck disable=SC2016

# Deploys trivy scanner to default kubernetes context
# The script creates write-only SAS token to access given BLOB_STORAGE_NAME. The token is valid 3 months

# exit on any errors (except conditional checks of executed commands)
set -e

BLOB_STORAGE_NAME=""
SCANNER_ID=""
IMAGE_TAG="edge"
K8S_NAMESPACE="joseki"

usage() {
  echo "Usage: $0 -b BLOB_STORAGE_NAME -i SCANNER_ID [ -t IMAGE_TAG ] [ -n K8S_NAMESPACE ] " 1>&2 
  echo ""
  echo "-b (required) - Blob Storage Account name"
  echo "-i (required) - Scanner unique identifier"
  echo "-t (optional) - docker image tag. If is not given, the default 'edge' value is used"
  echo "-n (optional) - kubernetes namespace to deploy scanner too. If is not given, the default 'joseki' value is used"
}

exit_abnormal() {
  usage
  exit 1
}

while getopts b:i:t:n: option
do
    case "${option}" in
        b) BLOB_STORAGE_NAME=${OPTARG};;
        i) SCANNER_ID=${OPTARG};;
        t) IMAGE_TAG=${OPTARG};;
        n) K8S_NAMESPACE=${OPTARG};;
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

if [ "$BLOB_STORAGE_NAME" = "" ]; then
  echo "BLOB STORAGE ACCOUNT NAME is required"
  exit_abnormal
fi

if [ "$SCANNER_ID" = "" ]; then
  echo "SCANNER ID is required"
  exit_abnormal
fi

echo ""
echo "Deploying trivy-scanner $SCANNER_ID to namespace $K8S_NAMESPACE"
echo ""

STORAGE_ACCOUNT_KEY=$(az storage account keys list --account-name "$BLOB_STORAGE_NAME" --query [0].value -o tsv)
CONTAINER_NAME="trivy-${SCANNER_ID:0:8}"
END_DATE=$(date -u -d "3 months" '+%Y-%m-%d')
az storage container create --account-name "$BLOB_STORAGE_NAME" --account-key "$STORAGE_ACCOUNT_KEY" --name "$CONTAINER_NAME"
STORAGE_SAS=$(az storage container generate-sas --account-name "$BLOB_STORAGE_NAME" --account-key "$STORAGE_ACCOUNT_KEY" --name "$CONTAINER_NAME" --expiry "$END_DATE" --permissions rw -o tsv)
MAIN_QUEUE_SAS=$(az storage queue generate-sas --name image-scan-requests --account-name "$BLOB_STORAGE_NAME" --account-key "$STORAGE_ACCOUNT_KEY" --expiry "$END_DATE" --permissions pu -o tsv)
QUARANTINE_QUEUE_SAS=$(az storage queue generate-sas --name image-scan-requests-quarantine --account-name "$BLOB_STORAGE_NAME" --account-key "$STORAGE_ACCOUNT_KEY" --expiry "$END_DATE" --permissions a -o tsv)

rm -rf ./working_dir; mkdir ./working_dir
cp "./k8s/templates/config_scanner.yaml.tmpl" ./working_dir/config_scanner.yaml
cp "./k8s/templates/kustomization.yaml.tmpl" ./working_dir/kustomization.yaml
cp "./k8s/templates/rbac.yaml.tmpl" ./working_dir/rbac.yaml
cp "./k8s/templates/scanner_trivy.yaml.tmpl" ./working_dir/scanner_trivy.yaml

sed -i 's|${trivy.scannerId}|'"$SCANNER_ID"'|' ./working_dir/config_scanner.yaml
sed -i 's|${trivy.storageAccountName}|'"$BLOB_STORAGE_NAME"'|' ./working_dir/config_scanner.yaml
sed -i 's|${trivy.containerName}|'"$CONTAINER_NAME"'|' ./working_dir/config_scanner.yaml
sed -i 's|${trivy.storageAccountSas}|'"${STORAGE_SAS//&/\\&}"'|' ./working_dir/config_scanner.yaml
sed -i 's|${trivy.mainQueueSas}|'"${MAIN_QUEUE_SAS//&/\\&}"'|' ./working_dir/config_scanner.yaml
sed -i 's|${trivy.quarantineQueueSas}|'"${QUARANTINE_QUEUE_SAS//&/\\&}"'|' ./working_dir/config_scanner.yaml

sed -i 's|${trivy.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/scanner_trivy.yaml
sed -i 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/scanner_trivy.yaml

sed -i 's|${trivy.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/kustomization.yaml
sed -i 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/kustomization.yaml

sed -i 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/rbac.yaml

kubectl apply -f ./working_dir/rbac.yaml
kubectl apply -k ./working_dir

rm -rf ./working_dir

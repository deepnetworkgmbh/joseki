#!/bin/bash
# shellcheck disable=SC2016

# Deploys single-time polaris scanner to default kubernetes context
# The script creates write-only SAS token to access given BLOB_STORAGE_NAME. The token is valid 1 day

# exit on any errors (except conditional checks of executed commands)
set -e

BLOB_STORAGE_NAME=""
SCANNER_ID=""
IMAGE_TAG="edge"
K8S_NAMESPACE="joseki"
SCANNER_CONFIG_TEMPLATE="./k8s/templates/config_scanner.yaml.tmpl"

usage() {
  echo "Usage: $0 -b BLOB_STORAGE_NAME -i SCANNER_ID [ -t IMAGE_TAG ] [ -n K8S_NAMESPACE ] [ -c SCANNER_CONFIG_TEMPLATE] " 1>&2 
  echo ""
  echo "-b (required) - Blob Storage Account name"
  echo "-i (required) - Scanner unique identifier"
  echo "-t (optional) - docker image tag. If is not given, the default 'edge' value is used"
  echo "-n (optional) - kubernetes namespace to deploy scanner too. If is not given, the default 'joseki' value is used"
  echo "-c (optional) - path to scanner configuration template file. If is not given, the default './k8s/templates/config_scanner.yaml.tmpl' value is used"
}

exit_abnormal() {
  usage
  exit 1
}

while getopts b:i:t:n:c: option
do
    case "${option}" in
        b) BLOB_STORAGE_NAME=${OPTARG};;
        i) SCANNER_ID=${OPTARG};;
        t) IMAGE_TAG=${OPTARG};;
        n) K8S_NAMESPACE=${OPTARG};;
        c) SCANNER_CONFIG_TEMPLATE=${OPTARG};;
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
echo "Deploying polaris-scanner $SCANNER_ID to namespace $K8S_NAMESPACE"
echo ""

STORAGE_ACCOUNT_KEY=$(az storage account keys list --account-name "$BLOB_STORAGE_NAME" --query [0].value -o tsv)
CONTAINER_NAME="polaris-${SCANNER_ID:0:8}"
END_DATE=$(date -u -d "1 day" '+%Y-%m-%dT%H:%MZ')
az storage container create --account-name "$BLOB_STORAGE_NAME" --account-key "$STORAGE_ACCOUNT_KEY" --name "$CONTAINER_NAME"
STORAGE_SAS=$(az storage container generate-sas --account-name "$BLOB_STORAGE_NAME" --account-key "$STORAGE_ACCOUNT_KEY" --name "$CONTAINER_NAME" --expiry "$END_DATE" --permissions rw -o tsv)

SCANNER_NAME="scanner-$CONTAINER_NAME"

rm -rf ./working_dir; mkdir ./working_dir
cp "$SCANNER_CONFIG_TEMPLATE" ./working_dir/config_scanner.yaml
cp "./k8s/config_polaris.yaml" ./working_dir/config_polaris.yaml
cp "./k8s/templates/kustomization.yaml.tmpl" ./working_dir/kustomization.yaml
cp "./k8s/templates/rbac.yaml.tmpl" ./working_dir/rbac.yaml
cp "./k8s/templates/scanner_polaris_job.yaml.tmpl" ./working_dir/scanner_polaris.yaml

sed -i 's|${SCANNER_ID}|'"$SCANNER_ID"'|' ./working_dir/config_scanner.yaml
sed -i 's|${BLOB_NAME}|'"$BLOB_STORAGE_NAME"'|' ./working_dir/config_scanner.yaml
sed -i 's|${CONTAINER_NAME}|'"$CONTAINER_NAME"'|' ./working_dir/config_scanner.yaml
sed -i 's|${SAS_TOKEN}|'"${STORAGE_SAS//&/\\&}"'|' ./working_dir/config_scanner.yaml

sed -i 's|${polaris.scannerName}|'"$SCANNER_NAME"'|' ./working_dir/scanner_polaris.yaml
sed -i 's|${polaris.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/scanner_polaris.yaml
sed -i 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/scanner_polaris.yaml

sed -i 's|${polaris.scannerName}|'"$SCANNER_NAME"'|' ./working_dir/kustomization.yaml
sed -i 's|${polaris.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/kustomization.yaml
sed -i 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/kustomization.yaml

sed -i 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/rbac.yaml

kubectl apply -f ./working_dir/rbac.yaml
kubectl apply -k ./working_dir

rm -rf ./working_dir

#!/bin/bash
# shellcheck disable=SC2016

# Deploys Backend service to default kubernetes context
# The script creates SAS token to manage given BLOB_STORAGE_NAME containers. The token is valid 3 months

# exit on any errors (except conditional checks of executed commands)
set -e

IMAGE_TAG="edge"
K8S_NAMESPACE="joseki"
BLOB_STORAGE_NAME=""
SQL_SERVER_NAME=""
SQL_DB_NAME=""
KEY_VAULT_NAME=""

usage() {
  echo "Usage: $0 -b BLOB_STORAGE_NAME -s SQL_SERVER_NAME -d SQL_DB_NAME -k KEY_VAULT_NAME [ -t IMAGE_TAG ] [ -n K8S_NAMESPACE ] " 1>&2 
  echo ""
  echo "-b (required) - Blob Storage Account name"
  echo "-s (required) - SQL Server name"
  echo "-d (required) - SQL database name"
  echo "-k (required) - Key Vault name"
  echo "-t (optional) - docker image tag. If is not given, the default 'edge' value is used"
  echo "-n (optional) - kubernetes namespace to deploy scanner too. If is not given, the default 'joseki' value is used"
}

exit_abnormal() {
  usage
  exit 1
}

while getopts b:s:d:k:t:n: option
do
    case "${option}" in
        b) BLOB_STORAGE_NAME=${OPTARG};;
        s) SQL_SERVER_NAME=${OPTARG};;
        d) SQL_DB_NAME=${OPTARG};;
        k) KEY_VAULT_NAME=${OPTARG};;
        t) IMAGE_TAG=${OPTARG};;
        n) K8S_NAMESPACE=${OPTARG};;
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

if [ "$BLOB_STORAGE_NAME" = "" ] || 
   [ "$SQL_SERVER_NAME" = "" ] || 
   [ "$SQL_DB_NAME" = "" ] || 
   [ "$KEY_VAULT_NAME" = "" ]; then
  echo "Not all required options are given"
  exit_abnormal
fi

echo ""
echo "Deploying backend service to namespace $K8S_NAMESPACE"
echo ""

BLOB_STORAGE_KEY=$(az storage account keys list --account-name "$BLOB_STORAGE_NAME" --query [0].value -o tsv)
END_DATE=$(date -u -d "3 months" '+%Y-%m-%d')
BLOB_STORAGE_SAS=$(az storage account generate-sas --account-name "$BLOB_STORAGE_NAME" --account-key "$BLOB_STORAGE_KEY" --expiry "$END_DATE" --permissions rwdlac --services b --resource-types sco -o tsv)

SQL_USERNAME=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "SQLADMIN" --query value -o tsv)
SQL_PASSWORD=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "SQLPASSWORD" --query value -o tsv)

AUTH_ENABLED=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "AUTH-ENABLED" --query value -o tsv)

rm -rf ./working_dir; mkdir ./working_dir
cp "./k8s/templates/config.yaml.tmpl" ./working_dir/config.yaml
cp "./k8s/templates/kustomization.yaml.tmpl" ./working_dir/kustomization.yaml
cp "./k8s/templates/rbac.yaml.tmpl" ./working_dir/rbac.yaml
cp "./k8s/templates/be.yaml.tmpl" ./working_dir/be.yaml

sed -i '' 's|${be.sqlServer}|'"$SQL_SERVER_NAME"'|' ./working_dir/config.yaml
sed -i '' 's|${be.sqlDb}|'"$SQL_DB_NAME"'|' ./working_dir/config.yaml
sed -i '' 's|${be.sqlUsername}|'"$SQL_USERNAME"'|' ./working_dir/config.yaml
sed -i '' 's|${be.sqlPassword}|'"$SQL_PASSWORD"'|' ./working_dir/config.yaml
sed -i '' 's|${be.blobStorageName}|'"$BLOB_STORAGE_NAME"'|' ./working_dir/config.yaml
sed -i '' 's|${be.blobStorageKey}|'"$BLOB_STORAGE_KEY"'|' ./working_dir/config.yaml
sed -i '' 's|${be.blobStorageSas}|'"${BLOB_STORAGE_SAS//&/\\&}"'|' ./working_dir/config.yaml

if [ "$AUTH_ENABLED" = "true" ]; then
  CLIENT_ID=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "CLIENT-ID" --query value -o tsv)
  CLIENT_SECRET=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "CLIENT-SECRET" --query value -o tsv)
  TENANT_ID=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "TENANT-ID" --query value -o tsv)
  AD_DOMAIN=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "AD-DOMAIN" --query value -o tsv)
  AD_INSTANCE=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "AD-INSTANCE" --query value -o tsv)

  sed -i '' 's|${be.azInstance}|'"${AD_INSTANCE//&/\\&}"'|' ./working_dir/config.yaml
  sed -i '' 's|${be.azDomain}|'"${AD_DOMAIN//&/\\&}"'|' ./working_dir/config.yaml
  sed -i '' 's|${be.azTenantId}|'"${TENANT_ID//&/\\&}"'|' ./working_dir/config.yaml
  sed -i '' 's|${be.azClientId}|'"${CLIENT_ID//&/\\&}"'|' ./working_dir/config.yaml
  sed -i '' 's|${be.azClientSecret}|'"${CLIENT_SECRET//&/\\&}"'|' ./working_dir/config.yaml
fi

sed -i '' 's|${be.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/be.yaml
sed -i '' 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/be.yaml

sed -i '' 's|${be.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/kustomization.yaml
sed -i '' 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/kustomization.yaml

sed -i '' 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/rbac.yaml

kubectl apply -f ./working_dir/rbac.yaml
kubectl apply -k ./working_dir

rm -rf ./working_dir

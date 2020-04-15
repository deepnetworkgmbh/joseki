#!/bin/bash

# Creates Azure resources required by Joseki

# exit on any errors (except conditional checks of executed commands)
set -e

LOCATION=westeurope
BASE_NAME=""
SQL_ADMIN=""
ENV_FILE="./joseki.env"

usage() {
  echo "Usage: $0 -b BASE_NAME -n K8S_SUBNET_ID [ -l LOCATION ] [ -a SQL_ADMIN ] [ -f ENV_FILE] " 1>&2 
  echo ""
  echo "-b (required) - common part of name used in each created resource. ONLY ALPHANUMERIC VALUES are supported. It's used as:"
  echo "    * 'rg-$BASE_NAME' for resource group"
  echo "    * 'sql-$BASE_NAME-$SALT' for sql server"
  echo "    * and others."
  echo "-n (required) - Kubernetes cluster Azure VNet subnet identifier in format '/subscriptions/{SubID}/resourceGroups/{ResourceGroup}/providers/Microsoft.Network/virtualNetworks/{VNETName}/subnets/{SubnetName}'"
  echo "-l (optional) - if is not given, default value is 'westeurope'"
  echo "-a (optional) - if is not given, value is randomly generated"
  echo "-f (optional) - saves names of generated components (environment description) to a file. This file is later used by 'deploy_SERVICE_NAME.sh' scripts. If is not given, default value is './joseki.env'"
}

exit_abnormal() {
  usage
  exit 1
}

while getopts b:l:a:n:f: option
do
    case "${option}" in
        b) BASE_NAME=${OPTARG};;
        l) LOCATION=${OPTARG};;
        a) SQL_ADMIN=${OPTARG};;
        n) K8S_SUBNET_ID=${OPTARG};;
        f) ENV_FILE=${OPTARG};;
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

if [ "$BASE_NAME" = "" ]; then
  exit_abnormal
else
  if [[ "$BASE_NAME" =~ [^a-z0-9] ]]; then
    echo "Unsupported BASE_NAME value"
    echo ""
    exit_abnormal
  fi
fi

if [ "$K8S_SUBNET_ID" = "" ]; then
  echo "K8S_SUBNET_ID is required"
  exit_abnormal
fi

if [ "$SQL_ADMIN" = "" ]; then
  echo "Generating random SQL_ADMIN username..."
  SQL_ADMIN=$(< /dev/urandom tr -dc 'a-zA-Z' | fold -w 16 | head -n 1)
fi

TAGS="application=joseki"
RG_NAME="rg-$BASE_NAME"
SALT=$(< /dev/urandom tr -dc 'a-z0-9' | fold -w 4 | head -n 1)

SQLSERVER_NAME="sql-$BASE_NAME-$SALT"
SQLDB_NAME="sqldb-$BASE_NAME"
SQL_PASSWORD=$(< /dev/urandom tr -dc 'a-zA-Z0-9!$%#' | fold -w 64 | head -n 1)

STORAGE_ACCOUNT_NAME="st$BASE_NAME$SALT"
QUEUE_SCAN_REQUEST="image-scan-requests"
QUEUE_SCAN_REQUEST_QUARANTINE="image-scan-requests-quarantine"

KEY_VAULT_NAME="kv-$BASE_NAME-$SALT"


### RESOURCE GROUP
echo "Creating Resource Group $RG_NAME in $LOCATION"
az group create --location "$LOCATION" --name "$RG_NAME" --tags "$TAGS"


### SQL DATABASE
echo "Creating SQL Server $SQLSERVER_NAME with database $SQLDB_NAME"
az sql server create --resource-group "$RG_NAME" --admin-password "$SQL_PASSWORD" --admin-user "$SQL_ADMIN" --name "$SQLSERVER_NAME"
az sql db create --resource-group "$RG_NAME" --server "$SQLSERVER_NAME" --name "$SQLDB_NAME" --service-objective S0 --tags "$TAGS"
# TODO: create db-user

az sql server vnet-rule create --server "$SQLSERVER_NAME" --name "k8s-cluster-subnet" \
  -g "$RG_NAME" --subnet "$K8S_SUBNET_ID"


### STORAGE ACCOUNT
echo "Creating Storage Account $STORAGE_ACCOUNT_NAME with queues $QUEUE_SCAN_REQUEST and $QUEUE_SCAN_REQUEST_QUARANTINE"
az storage account create --resource-group "$RG_NAME" --name "$STORAGE_ACCOUNT_NAME" --tags "$TAGS"

STORAGE_ACCOUNT_KEY=$(az storage account keys list --account-name "$STORAGE_ACCOUNT_NAME" --query [0].value -o tsv)
az storage queue create --name "$QUEUE_SCAN_REQUEST" --account-key "$STORAGE_ACCOUNT_KEY" --account-name "$STORAGE_ACCOUNT_NAME"
az storage queue create --name "$QUEUE_SCAN_REQUEST_QUARANTINE" --account-key "$STORAGE_ACCOUNT_KEY" --account-name "$STORAGE_ACCOUNT_NAME"


### KEY VAULT
echo "Creating Key Vault $KEY_VAULT_NAME and saving sql-username and sql-password to it"
az keyvault create --resource-group "$RG_NAME" --location "$LOCATION" --name "$KEY_VAULT_NAME" --enable-soft-delete false --tags "$TAGS"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "SQLADMIN" --value "$SQL_ADMIN"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "SQLPASSWORD" --value "$SQL_PASSWORD"


### SAVES ENV_FILE
touch "$ENV_FILE"
{
  echo "RG_NAME $RG_NAME
SQLSERVER_NAME $SQLSERVER_NAME
SQLDB_NAME $SQLDB_NAME
STORAGE_ACCOUNT_NAME $STORAGE_ACCOUNT_NAME
KEY_VAULT_NAME $KEY_VAULT_NAME"
} > "$ENV_FILE"


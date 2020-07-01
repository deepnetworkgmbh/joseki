#!/bin/bash

# Creates app registration for API and SPA on Azure

# exit on any errors (except conditional checks of executed commands)
set -e

### Variables
BASENAME=""
ENV_FILE=""
TENANT_ID=""
DOMAIN=""
CLIENT_SECRET=$(openssl rand -base64 44)
INSTANCE="https://login.microsoftonline.com/"

usage() {
  echo "Usage: $0 -b BASENAME -k KEY_VAULT_NAME -d DOMAIN [ -t TENANT_ID ] [ -i INSTANCE ] [ -f ENV_FILE ]" 1>&2 
  echo ""
  echo "-b (required) - base application name"
  echo "-k (required) - keyvault name"
  echo "-d (required) - azure app domain"
  echo "-i (optional) - tenant id, if empty will read from keyvault"
  echo "-i (optional) - azure ad instance, defaults to https://login.microsoftonline.com/"
  echo "-f (optional) - saves CLIENT_ID and AD-DOMAIN of registered app to env file."
}

exit_abnormal() {
  usage
  exit 1
}

while getopts b:k:t:d:i:f: option
do
    case "${option}" in
        b) BASENAME=${OPTARG};;
        k) KEY_VAULT_NAME=${OPTARG};;
        t) TENANT_ID=${OPTARG};;
        d) DOMAIN=${OPTARG};;
        i) INSTANCE=${OPTARG};;
        f) ENV_FILE=${OPTARG};;
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

APP_NAME="${BASENAME}_app"

### Get Current User's tenantID
# TENANTID=$(az account show --query tenantId -o tsv)
if [ "$TENANT_ID" = "" ]; then
  TENANT_ID=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "TENANT-ID" --query value -o tsv)
fi
echo "Tenant ID: $TENANT_ID"

### App Registration
echo "Creating App Registration ${APP_NAME}"

CLIENT_ID=$(az ad app create -o tsv \
    --display-name $APP_NAME \
    --available-to-other-tenants false \
    --oauth2-allow-implicit-flow true  \
    --reply-urls http://localhost:8080/home \
    --app-roles @approles.json \
    --credential-description "App Secret" \
    --password "${CLIENT_SECRET}" \
    --query appId -o tsv)

echo "CLIENT_ID is ${CLIENT_ID}"

# create a service principal for app
echo "Creating Service Principal"
az ad sp create --id $CLIENT_ID

# update owner of web application
echo "Updating owner"
OWNERID=$(az ad signed-in-user show --query objectId -o tsv)
az ad app owner add --id $CLIENT_ID --owner-object-id $OWNERID

# update identifier uri
echo "Updating identifier uri for $APP_NAME"
az ad app update --id $CLIENT_ID --identifier-uris "api://${CLIENT_ID}"

# add api permission to the application 
echo "Adding API Scope for $APP_NAME"
OAUTHPERMISSIONID=$(az ad app show --id $CLIENT_ID --query "oauth2Permissions[0].id" -o tsv)
az ad app permission add --id $CLIENT_ID --api $CLIENT_ID --api-permissions $OAUTHPERMISSIONID=Scope
az ad app permission grant --id $CLIENT_ID --api $CLIENT_ID

az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "CLIENT-ID" --value "$CLIENT_ID"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "CLIENT-SECRET" --value "$CLIENT_SECRET"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "AD-DOMAIN" --value "$DOMAIN"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "AD-INSTANCE" --value "$INSTANCE"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "AUTH-ENABLED" --value "true"

if [ "$ENV_FILE" != "" ]; then
  ### SAVES ENV_FILE
  touch "$ENV_FILE"
  {
    echo "AD-DOMAIN $DOMAIN
CLIENT_ID $CLIENT_ID"
  } > "$ENV_FILE"
fi
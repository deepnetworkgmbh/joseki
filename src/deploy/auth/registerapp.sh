#!/bin/bash

# Creates app registration for API and SPA on Azure

# exit on any errors (except conditional checks of executed commands)
set -e

### Variables
BASENAME="joseki"
CONFIG_FILE="config.env"
APP_NAME="${BASENAME}_app_10"
CLIENT_SECRET=$(openssl rand -base64 44)

### Get Current User's tenantID
TENANTID=$(az account show --query tenantId -o tsv)
echo "Tenant ID: $TENANTID"

### App Registration
echo "Creating App Registration ${APP_NAME}"

CLIENTID=$(az ad app create -o tsv \
    --display-name $APP_NAME \
    --available-to-other-tenants false \
    --oauth2-allow-implicit-flow true  \
    --reply-urls http://localhost:4200 https://joseki.dev http://localhost:5000 \
    --app-roles @approles.json \
    --credential-description "App Secret" \
    --password "${CLIENT_SECRET}" \
    --query appId -o tsv)

## CLIENTID=$(az ad app list --display-name $APP_NAME --query [].appId -o tsv)
echo "CLIENT_ID is ${CLIENTID}"

# create a service principal for app
echo "Creating Service Principal"
az ad sp create --id $CLIENTID

# update owner of web application
echo "Updating owner"
OWNERID=$(az ad signed-in-user show --query objectId -o tsv)
az ad app owner add --id $CLIENTID --owner-object-id $OWNERID

# update identifier uri
echo "Updating identifier uri for $APP_NAME"
az ad app update --id $CLIENTID --identifier-uris "api://${CLIENTID}"

# add api permission to the application 
echo "Adding API Scope for $APP_NAME"
OAUTHPERMISSIONID=$(az ad app show --id $CLIENTID --query "oauth2Permissions[0].id" -o tsv)
az ad app permission add --id $CLIENTID --api $CLIENTID --api-permissions $OAUTHPERMISSIONID=Scope
az ad app permission grant --id $CLIENTID --api $CLIENTID

touch "$CONFIG_FILE"
{
  echo "TENANT_ID $TENANTID"
  echo "CLIENT_ID $CLIENTID"
  echo "CLIENT_SECRET $CLIENT_SECRET"
} > "$CONFIG_FILE"

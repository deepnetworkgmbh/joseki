#!/bin/bash

# exit on any errors (except conditional checks of executed commands)
set -e

ENV_FILE="joseki.env"
BASE_NAME="joseki"
K8S_NAMESPACE="joseki-auth"

FRONTEND_VERSION=$(< "../frontend/version")
BACKEND_VERSION=$(< "../backend/version")
SCANNER_TRIVY_VERSION=$(< "../scanners/trivy/version")
SCANNER_AZSK_VERSION=$(< "../scanners/az-sk/version")
SCANNER_POLARIS_VERSION=$(< "../scanners/polaris/version")

APP_DOMAIN=""

usage() {
  echo "Usage: $0 -d APP_DOMAIN" 1>&2 
  echo ""
  echo "-d (required) - azure app domain"
  echo "-n (optional) - kubernetes namespace. default is joseki-auth."
}

exit_abnormal() {
  usage
  exit 1
}


while getopts d:n: option
do
    case "${option}" in
        d) APP_DOMAIN=${OPTARG};;
        n) K8S_NAMESPACE=${OPTARG};;     
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

# Read Scanner configuration from joseki.env
POLARIS_SCANNER_ID=$(< $ENV_FILE grep POLARIS_SCANNER_ID | cut -d' ' -f2)
AZSK_SCANNER_ID=$(< $ENV_FILE grep AZSK_SCANNER_ID | cut -d' ' -f2)
TRIVY_SCANNER_ID=$(< $ENV_FILE grep TRIVY_SCANNER_ID | cut -d' ' -f2)

# Read azure configuration from joseki.env
STORAGE_ACCOUNT_NAME=$(< $ENV_FILE grep STORAGE_ACCOUNT_NAME | cut -d' ' -f2)
SQLSERVER_NAME=$(< $ENV_FILE grep SQLSERVER_NAME | cut -d' ' -f2)
SQLDB_NAME=$(< $ENV_FILE grep SQLDB_NAME | cut -d' ' -f2)
KEY_VAULT_NAME=$(< $ENV_FILE grep KEY_VAULT_NAME | cut -d' ' -f2)

echo "K8S Namespace: $K8S_NAMESPACE"

echo "STORAGE_ACCOUNT_NAME : $STORAGE_ACCOUNT_NAME"
echo "SQLSERVER_NAME : $SQLSERVER_NAME"
echo "SQLDB_NAME : $SQLDB_NAME"
echo "KEY_VAULT_NAME : $KEY_VAULT_NAME"

# Read ClientID from ENV to see if a successful registration was done
CLIENT_ID=$(< $ENV_FILE grep CLIENT_ID | cut -d' ' -f2)

if [ "$KEY_VAULT_NAME" = "" ]; then
  echo "Could not get KEY_VAULT_NAME"
  exit 1
fi 
echo "Keyvault is $KEY_VAULT_NAME"

# Read TenantID from keyvault
TENANT_ID=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "TENANT-ID" --query value -o tsv)
if [[ $TENANT_ID == *"SecretNotFound"* ]]; then
  echo "Could not get TENANT_ID"
  exit 1
fi
echo "Tenant Id is $TENANT_ID"

if [ "$POLARIS_SCANNER_ID" = "" ] || 
   [ "$AZSK_SCANNER_ID" = "" ] || 
   [ "$TRIVY_SCANNER_ID" = "" ]; then
  echo "Could not get scanner ID list"
  exit 1
fi 

if [ "$STORAGE_ACCOUNT_NAME" = "" ] || 
   [ "$SQLSERVER_NAME" = "" ] || 
   [ "$SQLDB_NAME" = "" ] || 
   [ "$TENANT_ID" = "" ]; then
  echo ""
  echo "Cannot continue deploying Joseki with Auth configuration."
  echo "Make sure you ran a successful deployment and have a joseki.env file generated."
  exit 1
fi

EXISTING_NS=$(kubectl get ns "$K8S_NAMESPACE" -o name --ignore-not-found)
if [ -z "$EXISTING_NS" ]; then
  echo "Creating namespace $K8S_NAMESPACE"
  kubectl create ns "$K8S_NAMESPACE"
fi

# Create app registration 
if [ "$CLIENT_ID" = "" ]; then
  echo "ClientID not found, registering app..."
  (cd ./auth && ./registerapp.sh -b "$BASE_NAME" -k "$KEY_VAULT_NAME" -t "$TENANT_ID" -d "$APP_DOMAIN" -f "./../$ENV_FILE")
fi

# Deploy backend
(cd ./backend && ./deploy_backend.sh  -t "$BACKEND_VERSION" -n "$K8S_NAMESPACE" -b "$STORAGE_ACCOUNT_NAME" -s "$SQLSERVER_NAME" -d "$SQLDB_NAME" -k "$KEY_VAULT_NAME")

# Deploy frontend
(cd ./frontend && ./deploy_frontend.sh -t "$FRONTEND_VERSION" -n "$K8S_NAMESPACE" -k "$KEY_VAULT_NAME")

#!/bin/bash

# Saves Azure Service Principal credentials to the key vault; creates a new Service Principal if necessary

# exit on any errors (except conditional checks of executed commands)
set -e

SUBSCRIPTIONS=""
KEY_VAULT_NAME=""
SP_ID=""
SP_PASSWORD=""
TENANT_ID=""

usage() {
  echo ""
  echo "Usage: $0 -k KEY_VAULT_NAME [ -s SUBSCRIPTIONS ] [ -i SP_ID ] [ -p SP_PASSWORD ] [ -t TENANT_ID ]" 1>&2 
  echo ""
  echo "The script is responsible for saving proper Service Principal object into provided Key Vault"
  echo "It could be done in one of the ways:"
  echo "    1. if SUBSCRIPTIONS list is given, the script creates a new Service Principal object with READER role in given subscriptions"
  echo "    2. if SUBSCRIPTIONS list is empty, the script just saves given SP_ID, SP_PASSWORD, TENANT_ID to the Key Vault"
  echo "NOTE: ONE OF TWO PARAMETERS-SETS IS REQUIRED:"
  echo "    - If SUBSCRIPTIONS is empty - SP_ID, SP_PASSWORD, TENANT_ID are mandatory"
  echo "    - If SUBSCRIPTIONS is not empty - SP_ID, SP_PASSWORD, TENANT_ID are ignored"
  echo ""
  echo "-k (required) - Key Vault name"
  echo "-s (optional) - list of Azure subscription identifiers separated by whitespace, for example 'subscription1 subscription2 subscription3'."
  echo "-i (optional) - Service Principal identifier"
  echo "-p (optional) - Service Principal password"
  echo "-t (optional) - Service Principal Tenant identifier"
}

exit_abnormal() {
  usage
  exit 1
}

while getopts k:s:i:p:t: option
do
    case "${option}" in
        s) SUBSCRIPTIONS=${OPTARG};;
        k) KEY_VAULT_NAME=${OPTARG};;
        i) SP_ID=${OPTARG};;
        p) SP_PASSWORD=${OPTARG};;
        t) TENANT_ID=${OPTARG};;
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

if [ "$SUBSCRIPTIONS" = "" ];
then
  if [ "$SP_ID" = "" ] && [ "$SP_PASSWORD" = "" ] && [ "$TENANT_ID" = "" ];
  then
    echo "Not all required options are given"
    exit_abnormal
  fi
else
  # shellcheck disable=SC2206
  IDS=(${SUBSCRIPTIONS})
  SCOPES=""
  for id in "${IDS[@]}"
  do
    SCOPES="$SCOPES/subscriptions/$id "
  done

  echo "Creating 'azsk-scanner-$SALT' Service Principal within scope '$SCOPES'"
  create_sp_cmd="az ad sp create-for-rbac -n azsk-scanner-$SALT --role Reader --scopes $SCOPES -o tsv"
  SP=$($create_sp_cmd)
  SP_ID=$(echo "$SP" | cut -f1)
  SP_PASSWORD=$(echo "$SP" | cut -f4)
  TENANT_ID=$(echo "$SP" | cut -f5)
fi

az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "SP-ID" --value "$SP_ID"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "SP-PASSWORD" --value "$SP_PASSWORD"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "TENANT-ID" --value "$TENANT_ID"

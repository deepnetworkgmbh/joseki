#!/bin/bash

# grant graph access on the registered app.

CLIENT_ID=""

usage() {
  echo "Grant Graph Access on registered app."
  echo "Usage: $0 -c CLIENT_ID" 1>&2 
  echo ""
  echo "-c (required) - client id"
}

exit_abnormal() {
  usage
  exit 1
}

while getopts c: option
do
    case "${option}" in
        c) CLIENT_ID=${OPTARG};;        
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

if [ "$CLIENT_ID" = "" ]; then
  echo "Cannot operate without client ID."
  echo "ClientID can be obtained from the Azure App Registration page."
  exit 1
fi 

GRAPH_ID=$(az ad sp list --query "[?appDisplayName=='Microsoft Graph'].appId | [0]" --all -o tsv)
READ_ALL_USERS=$(az ad sp show --id $GRAPH_ID --query "oauth2Permissions[?value=='User.Read.All'].id | [0]" -o tsv);
DIRECTORY_READ_ALL=$(az ad sp show --id $GRAPH_ID --query "oauth2Permissions[?value=='Directory.Read.All'].id | [0]" -o tsv);

echo "GRAPH: $GRAPH_ID"
echo "Read All Users: $READ_ALL_USERS"
echo "Directory Read All: $DIRECTORY_READ_ALL"

az ad app permission add --id $CLIENT_ID --api $GRAPH_ID --api-permissions $READ_ALL_USERS=Role
az ad app permission add --id $CLIENT_ID --api $GRAPH_ID --api-permissions $DIRECTORY_READ_ALL=Role
az ad app permission grant --id $CLIENT_ID --api $GRAPH_ID

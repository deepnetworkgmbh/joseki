#!/bin/bash
# shellcheck disable=SC2016

# Deploys Frontend service to default kubernetes context

# exit on any errors (except conditional checks of executed commands)
set -e

IMAGE_TAG="edge"
K8S_NAMESPACE="joseki"

usage() {
  echo "Usage: $0 [ -t IMAGE_TAG ] [ -n K8S_NAMESPACE ] " 1>&2 
  echo ""
  echo "-t (optional) - docker image tag. If is not given, the default 'edge' value is used"
  echo "-n (optional) - kubernetes namespace to deploy scanner too. If is not given, the default 'joseki' value is used"
}

exit_abnormal() {
  usage
  exit 1
}

while getopts t:n: option
do
    case "${option}" in
        t) IMAGE_TAG=${OPTARG};;
        n) K8S_NAMESPACE=${OPTARG};;
        *) # If unknown (any other) option:
          exit_abnormal
          ;;
    esac
done

echo ""
echo "Deploying frontend service to namespace $K8S_NAMESPACE"
echo ""

rm -rf ./working_dir; mkdir ./working_dir
cp "./k8s/templates/config.json.tmpl" ./working_dir/config.json
cp "./k8s/templates/kustomization.yaml.tmpl" ./working_dir/kustomization.yaml
cp "./k8s/templates/rbac.yaml.tmpl" ./working_dir/rbac.yaml
cp "./k8s/templates/fe.yaml.tmpl" ./working_dir/fe.yaml

sed -i '' 's|${fe.apiUrl}|http://localhost:5001/api|' ./working_dir/config.json
sed -i '' 's|${fe.timezone}|Europe/Berlin|' ./working_dir/config.json

if [ "$AUTH_ENABLED" = "true" ]; then
  CLIENT_ID=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "CLIENT-ID" --query value -o tsv)
  TENANT_ID=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "TENANT-ID" --query value -o tsv)
  AD_DOMAIN=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "AD-DOMAIN" --query value -o tsv) 
fi

sed -i '' 's|${fe.authEnabled}|'"${AUTH_ENABLED//&/\\&}"'|' ./working_dir/config.json
sed -i '' 's|${fe.domain}|'"${AD_DOMAIN//&/\\&}"'|' ./working_dir/config.json
sed -i '' 's|${fe.tenantId}|'"${TENANT_ID//&/\\&}"'|' ./working_dir/config.json
sed -i '' 's|${fe.clientId}|'"${CLIENT_ID//&/\\&}"'|' ./working_dir/config.json

sed -i '' 's|${fe.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/fe.yaml
sed -i '' 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/fe.yaml

sed -i '' 's|${fe.imageTag}|'"$IMAGE_TAG"'|' ./working_dir/kustomization.yaml
sed -i '' 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/kustomization.yaml

sed -i '' 's|${joseki.namespace}|'"$K8S_NAMESPACE"'|' ./working_dir/rbac.yaml

kubectl apply -f ./working_dir/rbac.yaml
kubectl apply -k ./working_dir

rm -rf ./working_dir

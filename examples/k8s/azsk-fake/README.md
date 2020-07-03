# Fake az-sk scanner

Fake scanner is able to generate test-data, which running joseki instance could pick up and ingest.

To provision a fake scanner perform three actions:

1. create `azsk-d3cf9345` container in Storage Account and generate SAS token to access it:

   ```bash
   ACCOUNT_NAME=...
   ACCOUNT_KEY=...
   EXPIRY_DATE=$(date -d "+3 month" -u +'%Y-%m-%d')
   CONTAINER_NAME=azsk-d3cf9345
   az storage container create --account-name $ACCOUNT_NAME --name $CONTAINER_NAME
   az storage container generate-sas --name $CONTAINER_NAME --account-key $ACCOUNT_KEY --account-name $ACCOUNT_NAME --expiry $EXPIRY_DATE --permissions rw
   ```

2. insert corrent Storage Account Name and SAS token to `fake-scanner-azsk-config.yaml` file;
3. Run the script:

    ```bash
    IMAGE_TAG=edge
    sed -i 's|#{azsk.imageTag}#|'"$IMAGE_TAG"'|' ./kustomization.yaml
    sed -i 's|#{azsk.imageTag}#|'"$IMAGE_TAG"'|' ./testdata_generator.yaml

    kubectl create configmap azsk-test-data --from-file ./../../../src/scanners/az-sk/azsk_test_data.tar.xz
    kubectl apply -k .
    ```

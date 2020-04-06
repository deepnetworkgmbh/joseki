# Fake polaris scanner

Fake scanner is able to generate test-data, which running joseki instance could pick up and ingest.

To provision a fake scanner perform three actions:

1. create `polaris-480e3d1f` container in Storage Account and generate SAS token to access it:

   ```bash
    STORAGE_ACCOUNT_NAME=...
    STORAGE_ACCOUNT_KEY=`az storage account keys list --account-name $STORAGE_ACCOUNT_NAME --query [0].value -o tsv`

    SCANNER_ID=480e3d1f-ccba-499d-bbf1-b98925d806ad
    CONTAINER_NAME="polaris-${SCANNER_ID:0:8}"
    az storage container create --name $CONTAINER_NAME --account-name $STORAGE_ACCOUNT_NAME --account-key $STORAGE_ACCOUNT_KEY

    END_DATE=`date -u -d "3 months" '+%Y-%m-%dT%H:%MZ'`
    STORAGE_SAS=`az storage container generate-sas --account-name $STORAGE_ACCOUNT_NAME --account-key $STORAGE_ACCOUNT_KEY  --name $CONTAINER_NAME --expiry $END_DATE --https-only --permissions rw -o tsv`
    sed -i 's|BLOB_NAME|'"$STORAGE_ACCOUNT_NAME"'|' ./fake-scanner-polaris-config.yaml
    sed -i 's|CONTAINER_NAME|'"$CONTAINER_NAME"'|' ./fake-scanner-polaris-config.yaml
    sed -i 's|insert-sas-token-here|'"$STORAGE_SAS"'|' ./fake-scanner-polaris-config.yaml
   ```

2. review `fake-scanner-polaris-config.yaml` file after running the script above;
3. Run the script:

    ```bash
    IMAGE_TAG=edge
    sed -i 's|#{polaris.imageTag}#|'"$IMAGE_TAG"'|' ./kustomization.yaml
    sed -i 's|#{polaris.imageTag}#|'"$IMAGE_TAG"'|' ./testdata_generator.yaml

    kubectl create configmap polaris-test-data --from-file ./../../../src/scanners/polaris/polaris_test_data.tar.xz
    kubectl apply -k .
    ```

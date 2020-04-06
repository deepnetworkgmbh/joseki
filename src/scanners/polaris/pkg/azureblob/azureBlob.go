package azureblob

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/url"
	"time"

	"github.com/Azure/azure-storage-blob-go/azblob"
	"github.com/deepnetworkgmbh/joseki/src/scanners/polaris/pkg/config"
	"github.com/deepnetworkgmbh/joseki/src/scanners/polaris/pkg/scanner"
	"github.com/google/uuid"
)

type Client struct {
	config   config.Config
	BaseUrl  string
	SasToken string
}

func CreateBlobClient(config config.Config) *Client {
	blobclient := Client{
		config:   config,
		BaseUrl:  config.AzureBlob.StorageBaseUrl,
		SasToken: config.AzureBlob.SasToken,
	}

	return &blobclient
}

type ScannerMetadata struct {
	Type                 string `json:"type"`
	Id                   string `json:"id"`
	Periodicity          string `json:"periodicity"`
	HeartbeatPeriodicity int64  `json:"heartbeat-periodicity"`
	Heartbeat            int64  `json:"heartbeat"`
}

type AuditMetadata struct {
	Id                 string      `json:"audit-id"`
	ClusterId          string      `json:"cluster-id"`
	ScannerVersion     string      `json:"scanner-version"`
	Timestamp          int64       `json:"timestamp"`
	Result             AuditResult `json:"result"`
	FailureDescription string      `json:"failure-description,omitempty"`
	PolarisVersion     string      `json:"polaris-version"`
	PolarisAuditPath   string      `json:"polaris-audit-path"`
	KubeMetadataPath   string      `json:"k8s-meta-path"`
}

type AuditResult string

const (
	Succeeded    AuditResult = "succeeded"
	AuditFailed              = "audit-failed"
	Unknown                  = "unknown"
	UploadFailed             = "upload-failed"
)

func (client *Client) UploadAuditResult(polarisResult scanner.PolarisAudit, scanDate time.Time) (err error) {
	result := normalizeResult(polarisResult.Result)
	folderName := GenerateAuditFolderName(scanDate)

	auditMeta := AuditMetadata{
		Id:                 uuid.New().String(),
		ClusterId:          client.config.Scanner.ClusterId,
		ScannerVersion:     client.config.GetScannerVersion(),
		Timestamp:          scanDate.Unix(),
		Result:             result,
		FailureDescription: polarisResult.FailureDescription,
		PolarisVersion:     client.config.GetPolarisVersion(),
		PolarisAuditPath:   fmt.Sprintf("%s/audit.json", folderName),
		KubeMetadataPath:   fmt.Sprintf("%s/k8s-meta.json", folderName),
	}

	if result == AuditFailed {
		auditMeta.Result = AuditFailed
		auditMeta.FailureDescription = polarisResult.FailureDescription
	} else {
		err = client.uploadObject(auditMeta.PolarisAuditPath, polarisResult.Audit)
		if err != nil {
			auditMeta.Result = UploadFailed
			auditMeta.FailureDescription = "Uploading audit result to the blob storage failed"
		}

		err = client.uploadObject(auditMeta.KubeMetadataPath, polarisResult.KubeMetadata)
		if err != nil {
			auditMeta.Result = UploadFailed
			auditMeta.FailureDescription = "Uploading kubernetes metadata to the blob storage failed"
		}
	}

	auditMetadataPath := fmt.Sprintf("%s/meta", folderName)
	err = client.uploadObject(auditMetadataPath, auditMeta)

	scannerMetadataPath := fmt.Sprintf("polaris-%s", client.config.Scanner.Id[:8])
	err = client.uploadObject(scannerMetadataPath, client.config.GetScannerMetadata())

	return err
}

func (client *Client) uploadObject(blobPath string, obj interface{}) error {
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Minute)
	defer cancel()

	credential := azblob.NewAnonymousCredential()
	p := azblob.NewPipeline(credential, azblob.PipelineOptions{})
	auditBlob := azblob.NewBlockBlobURL(createBlobUrl(client.BaseUrl, client.SasToken, blobPath), p)

	bytes, err := json.MarshalIndent(obj, "", "\t")
	if err != nil {
		return err
	}

	_, err = azblob.UploadBufferToBlockBlob(ctx, bytes, auditBlob, azblob.UploadToBlockBlobOptions{
		BlockSize:   4 * 1024 * 1024,
		Parallelism: 16})
	if err != nil {
		log.Fatal(err)
		return err
	}

	return nil
}

func normalizeResult(result string) AuditResult {
	switch result {
	case "success":
		return Succeeded
	case "failed":
		return AuditFailed
	default:
		return Unknown
	}
}

func createBlobUrl(baseUrl string, sas string, blobName string) url.URL {
	blobUrl, _ := url.Parse(fmt.Sprintf("%s/%s?%s", baseUrl, blobName, sas))

	return *blobUrl
}

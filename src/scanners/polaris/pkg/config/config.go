package config

import (
	"bytes"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"time"

	"k8s.io/apimachinery/pkg/util/yaml"
)

type Config struct {
	Scanner         Scanner         `json:"scanner"`
	Polaris         Polaris         `json:"polaris"`
	BlobStorageType BlobStorageType `json:"blobStorageType"`
	AzureBlob       AzureBlob       `json:"azureBlob,omitempty"`
	LogFormat       LogFormat       `json:"logFormat"`
}

type Scanner struct {
	Id                   string `json:"id"`
	ClusterId            string `json:"cluster-id"`
	Periodicity          string `json:"periodicity"`
	HeartbeatPeriodicity int64  `json:"heartbeat-periodicity"`
	Version              string `json:"version"`
}

type Polaris struct {
	ConfigPath string `json:"configPath"`
	Version    string `json:"version"`
}

type LogFormat string

const (
	Plain LogFormat = "plain-text"
	Json            = "json"
)

type BlobStorageType string

const (
	Azure BlobStorageType = "azure-blob-storage"
)

type AzureBlob struct {
	StorageBaseUrl string `json:"storageBaseUrl"`
	SasToken       string `json:"sasToken"`
}

type Metadata struct {
	Type                 string `json:"type"`
	Id                   string `json:"id"`
	Periodicity          string `json:"periodicity"`
	HeartbeatPeriodicity int64  `json:"heartbeat-periodicity"`
	Heartbeat            int64  `json:"heartbeat"`
}

func Parse(path string) (Config, error) {
	conf := Config{}

	rawBytes, err := ioutil.ReadFile(path)
	if err != nil {
		log.Fatalf("Error reading config file %v", err)
		return conf, err
	}

	reader := bytes.NewReader(rawBytes)
	d := yaml.NewYAMLOrJSONDecoder(reader, 4096)
	for {
		if err := d.Decode(&conf); err != nil {
			if err == io.EOF {
				return conf, nil
			}
			return conf, fmt.Errorf("Decoding config failed: %v", err)
		}
	}
}

func (config *Config) GetScannerMetadata() Metadata {
	unixNow := time.Now().UTC().Unix()
	return Metadata{
		Type:                 "polaris",
		Id:                   config.Scanner.Id,
		Periodicity:          config.Scanner.Periodicity,
		HeartbeatPeriodicity: config.Scanner.HeartbeatPeriodicity,
		Heartbeat:            unixNow,
	}
}

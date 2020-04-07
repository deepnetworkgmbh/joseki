package config

import (
	"bytes"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"os"
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
	IsFake               bool   `json:"isFake,omitempty"`
	FakeResultsPath      string `json:"fakeResultsPath,omitempty"`
	From                 string `json:"from,omitempty"`
	To                   string `json:"to,omitempty"`
}

type Polaris struct {
	ConfigPath string `json:"configPath"`
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

				if conf.Scanner.IsFake {
					if (len(conf.Scanner.From) > 0) != (len(conf.Scanner.To) > 0) {
						return conf, fmt.Errorf("from and To dates should both have values, or both be empty")
					}

					if len(conf.Scanner.FakeResultsPath) == 0 {
						return conf, fmt.Errorf("missing config value: FakeResultsPath")
					}
				} else {
					if len(conf.Scanner.To) > 0 || len(conf.Scanner.From) > 0 {
						return conf, fmt.Errorf("from and To dates are supported only for fake scanner")
					}
				}

				return conf, nil
			}
			return conf, fmt.Errorf("decoding config failed: %v", err)
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

func (config *Config) GetScannerVersion() string {
	return os.Getenv("SCANNER_VERSION")
}

func (config *Config) GetPolarisVersion() string {
	return "0.6.0"
}

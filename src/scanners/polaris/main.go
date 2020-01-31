package main

import (
	"flag"
	"fmt"
	"github.com/deepnetworkgmbh/joseki/src/scanners/polaris/pkg/azureblob"
	"github.com/deepnetworkgmbh/joseki/src/scanners/polaris/pkg/scanner"
	"log"
	"os"
)

const (
	Version = "0.1.0"
)

func main() {
	version := flag.Bool("version", false, "Prints the version of Polaris scanner")
	configPath := flag.String(
		"config",
		"./examples/scanner-config.yaml",
		"Location of Polaris scanner configuration file")

	if *version {
		fmt.Printf("Polaris version %s\n", Version)
		os.Exit(0)
	}

	config, err := scanner.ParseConfig(*configPath)
	if err != nil {
		log.Fatalf("Failed to parse configuration at %s %v", configPath, err)
		os.Exit(1)
	}

	audit(config)
}

func audit(config scanner.Config) {
	audit := scanner.Audit(config.Polaris.ConfigPath)

	client := azureblob.CreateBlobClient(config)
	err := client.UploadAuditResult(audit)
	if err != nil {
		os.Exit(1)
	}

}

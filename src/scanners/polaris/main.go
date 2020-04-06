package main

import (
	"flag"
	"fmt"
	"log"
	"os"
	"time"

	"github.com/deepnetworkgmbh/joseki/src/scanners/polaris/pkg/config"

	"github.com/deepnetworkgmbh/joseki/src/scanners/polaris/pkg/azureblob"
	"github.com/deepnetworkgmbh/joseki/src/scanners/polaris/pkg/scanner"
)

const (
	Version = "0.2.2"
)

func main() {
	version := flag.Bool("version", false, "Prints the version of scanner")
	configPath := flag.String(
		"config",
		"./examples/fake-scanner-config.yaml",
		"Location of scanner configuration file")

	flag.Parse()

	if *version {
		fmt.Printf("Scanner version %s\n", Version)
		os.Exit(0)
	}

	log.Printf("Parsing config from %v", *configPath)
	config, err := config.Parse(*configPath)
	if err != nil {
		log.Fatalf("Failed to parse configuration at %s %v", *configPath, err)
		os.Exit(1)
	}

	audit(config)
}

func audit(config config.Config) {
	today := time.Now().UTC()
	startDate := today
	endDate := today

	var err error
	if len(config.Scanner.From) > 0 {
		startDate, err = time.Parse("2006-01-02", config.Scanner.From)
		if err != nil {
			log.Fatalf("Failed to parse From date %s: %v", config.Scanner.From, err)
			os.Exit(1)
		}
	}

	if len(config.Scanner.To) > 0 {
		endDate, err = time.Parse("2006-01-02", config.Scanner.To)
		if err != nil {
			log.Fatalf("Failed to parse To date %s: %v", config.Scanner.To, err)
			os.Exit(1)
		}
	}

	if endDate.Before(startDate) {
		log.Fatalf("start-date %v should be erlier than end-date %v", startDate, endDate)
		os.Exit(1)
	}

	scanDate := startDate
	audit := scanner.PolarisAudit{}

	for {

		if config.Scanner.IsFake {
			audit = scanner.LoadFromFile(config.Scanner.FakeResultsPath, scanDate)
		} else {
			audit = scanner.Audit(config.Polaris.ConfigPath)
		}

		client := azureblob.CreateBlobClient(config)
		err := client.UploadAuditResult(audit, scanDate)
		if err != nil {
			os.Exit(1)
		}

		scanDate = scanDate.AddDate(0, 0, 1)

		if scanDate.After(endDate) {
			return
		}
	}


}

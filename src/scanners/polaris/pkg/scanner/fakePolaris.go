package scanner

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"path/filepath"
	"strconv"
	"time"
)

func LoadFromFile(fakeResultsPath string, scanDate time.Time) (PolarisAudit){
	audit := PolarisAudit{}

	folderName := ""
	day := scanDate.Day()
	if day < 16 {
		folderName = strconv.Itoa(day)
	} else {
		folderName = strconv.Itoa(31 - day)
	}

	fakeResultsFolder, err := filepath.Abs(fakeResultsPath)
	if err != nil {
		log.Fatalf("Error parsing fake-results path %s: %v", fakeResultsPath, err)

		audit.Result = "failed"
		audit.FailureDescription =  fmt.Sprintf("Error parsing fake-results path %s: %v", fakeResultsPath, err)
		return audit
	}

	auditPath := filepath.Join(fakeResultsFolder, folderName, "audit.json")
	k8sPath := filepath.Join(fakeResultsFolder, folderName, "k8s-meta.json")

	k8sFileContent, _ := ioutil.ReadFile(k8sPath)
	var k interface{}
	err = json.Unmarshal(k8sFileContent, &k)
	if err != nil {
		log.Fatalf("Error fetching Kubernetes resources %v", err)

		audit.Result = "failed"
		audit.FailureDescription =  fmt.Sprintf("Error fetching Kubernetes resources %v", err)
		return audit
	}

	auditFileContent, _ := ioutil.ReadFile(auditPath)
	var auditData interface{}
	err = json.Unmarshal(auditFileContent, &auditData)
	if err != nil {
		log.Fatalf("Error getting audit data: %v", err)
		audit.Result = "failed"
		audit.FailureDescription =  fmt.Sprintf("Error getting audit data: %v", err)
		return audit
	}

	audit.Result = "success"
	audit.KubeMetadata = &k
	audit.Audit = &auditData
	return audit
}
package scanner

import (
	"fmt"
	polarisConfig "github.com/fairwindsops/polaris/pkg/config"
	"github.com/fairwindsops/polaris/pkg/kube"
	"github.com/fairwindsops/polaris/pkg/validator"
	"log"
)

func Audit(configPath string) (PolarisAudit){
	audit := PolarisAudit{}

	c, err := polarisConfig.ParseFile(configPath)
	if err != nil {
		log.Fatalf("Error parsing config at %s: %v", configPath, err)

		audit.Result = "failed"
		audit.FailureDescription =  fmt.Sprintf("Error parsing config at %s: %v", configPath, err)
		return audit
	}

	k, err := kube.CreateResourceProviderFromCluster()
	if err != nil {
		log.Fatalf("Error fetching Kubernetes resources %v", err)

		audit.Result = "failed"
		audit.FailureDescription =  fmt.Sprintf("Error fetching Kubernetes resources %v", err)
		return audit
	}

	auditData, err := validator.RunAudit(c, k)
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
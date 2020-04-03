package scanner


type PolarisAudit struct {
	Result             string
	FailureDescription string
	Audit              interface{}
	KubeMetadata       interface{}
}
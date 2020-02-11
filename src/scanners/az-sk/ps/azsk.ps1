 param (
    [string]$SubscriptionId,
    [string]$TenantId,
    [string]$ServicePrincipalId,
    [string]$ServicePrincipalPassword,
    [string]$OutputFolder
 )

$passwd = ConvertTo-SecureString $ServicePrincipalPassword -AsPlainText -Force
$pscredential = New-Object System.Management.Automation.PSCredential($ServicePrincipalId, $passwd)
Connect-AzAccount -ServicePrincipal -Credential $pscredential -Tenant $TenantId

Set-AzSKUserPreference -OutputFolderPath $OutputFolder

### Run Subscription Security check
Get-AzSKSubscriptionSecurityStatus -SubscriptionId $SubscriptionId -DoNotOpenOutputFolder

### Run Components Security check 
Get-AzSKAzureServicesSecurityStatus -SubscriptionId $SubscriptionId -DoNotOpenOutputFolder
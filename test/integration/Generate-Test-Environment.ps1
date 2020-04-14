[cmdletbinding(SupportsShouldProcess=$True)]

#
# This script is part of azmi-tool project
#    It is generating personal test environment used for integration testing
#
#

param (

    [Parameter(Mandatory,Position=0,HelpMessage="Run Get-AzSubscription to see subscription IDs")]
    [ValidatePattern('[0-9a-f\-]{36}')]
    [string]$SubscriptionID,

    [Parameter(Mandatory,Position=1,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
    [string]$ResourceGroupName,

    [Parameter(Mandatory=$false,HelpMessage="Location in which to create resources. If not specified, inherited from Resource Group")]
    [string]$LocationName,

    [Parameter(Mandatory=$false)]
    [string]$StorageAccountName,

    [Parameter(Mandatory=$false)]
    [string]$ManagedIdentityName,

    [Parameter()]
    [switch]$SkipModulesCheck


)


#
#  Internal Functions
#

function Write-SVerbose($Message) {Write-Verbose "$(Get-Date -f G)   $CommandName $Message"}
function Write-VVerbose($Message) {Write-Verbose "$(Get-Date -f T)   $Message"}
function Write-EEror($Message) {Write-Error "$CommandName`: $Message"}
function Get-RandomDigits($Count) {(1..$Count | % {0..9 | Get-Random}) -join '' }

function Test-StorageAccountName($Name) {
    try {
        if ([System.Net.Dns]::Resolve($Name + '.blob.core.windows.net')) {
            return 'not ok'
        } else {
            return 'ok'
        }
    } catch {
        return 'ok'
    }
}

function Test-ManagedIdentityName($Name) {
    if (Get-AzUserAssignedIdentity -ea 0 | where Name -eq $ManagedIdentityName) {
        return 'not ok'
    } else {
        return 'ok'
    }
}
-r.vault.azure.net

function Test-KeyVaultName($Name) {
    
}

function Get-RandomName($TestFunctionName) {
    $RandomDigits = 1;
    $RandomAttempt =1
    $IsFree = $false
    $TestFunc = (Get-Item "function:$TestFunctionName").ScriptBlock
    do {
        $Name = 'azmitest' + (Get-RandomDigits $RandomDigits)
        Write-VVerbose "Trying $Name"
        if ($TestFunc.Invoke($Name) -eq 'ok') {
            Write-VVerbose "Free!"
            $IsFree = $true
        } else {
            Write-VVerbose "Already existing"
            if ($RandomAttempt -ge $RandomDigits) {
                $RandomAttempt = 1
                $RandomDigits++
            } else {
                $RandomAttempt++
            }
        }
    } until ($IsFree)
    return $Name
}


#
#
#    Script start
#
#

$CommandName = 'Generate-Test-Environment.ps1'
Write-SVerbose "starting"


#
# Verify if we have required modules
#
if ($SkipModulesCheck) {
    Write-VVerbose "Skipping modules check"
} else {
    Write-VVerbose "Checking for required modules"
    $RequiredModules = @('Az.Accounts','Az.Resources','Az.Storage','Az.ManagedServiceIdentity')
    $AllModules = Get-Module -List -Verbose:$false | Select-Object -ExpandProperty Name
    $AllFound = $true

    foreach ($M1 in $RequiredModules) {
        if ($M1 -notin $AllModules) {
            Write-EError "Not installed module: $M1"
            $AllFound = $false
        }
    }

    if (!$AllFound) {
        throw "Please fix missing module(s). Run Install-Module <name>"
    } else {
        Write-VVerbose "All modules found. You can skip it next time with -SkipModulesCheck"
    }
}


#
# Verify if we are logged in to the Azure
#

Write-VVerbose "Checking Azure connection"
if ($Ctx = Get-AzContext -ea 0) {
    if ($Ctx.Subscription.Id -eq $SubscriptionID) {
        Write-VVerbose "Azure subscription found: $($Ctx.Subscription.Name)"
    } else {
        Set-AzContext -SubscriptionId $SubscriptionID -ErrorAction Stop
        $Ctx = Get-AzContext
    }
} else {
    throw "Please login to required Azure (Login-AzAccount)"
}

#
# Verify and create Resource group
#

Write-VVerbose "Checking Resource Group"
if ($RG = Get-AzResourceGroup -Name $ResourceGroupName -ea 0) {
    Write-VVerbose "Resource group $ResourceGroupName found."
    $AzLocation = Get-AzLocation | where Location -eq ($RG.Location)
} else {
    Write-VVerbose "Creating new resource group $ResourceGroupName."
    if ($LocationName) {
        $AzLocation = Get-AzLocation | where DisplayName -eq $LocationName
        if (!$AzLocation) {
            throw "Azure Location $LocationName not found. Run Get-AzLocation for list"
        }
    } else {
        $AzLocation = Get-AzLocation | Get-Random
    }
    if (($pscmdlet.ShouldProcess($Ctx.Subscription.Name,"Create Resource Group $ResourceGroupName"))) {
        New-AzResourceGroup -Name $ResourceGroupName -Location $AzLocation.Location -ea Stop | Out-Null
        Write-VVerbose "New resource group $ResourceGroupName created in $($AzLocation.DisplayName)."
    }
}


#
#  Verify and create managed identity
#

Write-VVerbose "Checking managed identity name"
if (($ManagedIdentityName) -and ($MIObj = Get-AzUserAssignedIdentity -ea 0 | where Name -eq $ManagedIdentityName)) {
    Write-VVerbose "Using existing managed identity $ManagedIdentityName"
} else {
    if (!$ManagedIdentityName) {
        Write-VVerbose "Generate random managed identity name"
        $ManagedIdentityName = Get-RandomName Test-ManagedIdentityName
    }

    Write-VVerbose "Creating managed identity $ManagedIdentityName"
    if (($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Managed Identity $ManagedIdentityName"))) {
        $MIObj = New-AzUserAssignedIdentity -ResourceGroupName $ResourceGroupName -Name $ManagedIdentityName -Location $AzLocation.Location
        Write-VVerbose "New managed identity $ManagedIdentityName created in $ResourceGroupName."
    }
}


#
# Verify and create Storage Account
#

if ($StorageAccountName -and (Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $StorageAccountName -ea 0)) {
    Write-VVerbose "Storage account $StorageAccountName is existing."
} else {
    if (!$StorageAccountName) {
        Write-VVerbose "Generate random storage account name"
        $StorageAccountName = Get-RandomName Test-StorageAccountName
    }

    Write-VVerbose "Creating storage account $StorageAccountName"
    if (($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Storage Account $StorageAccountName"))) {
        New-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $StorageAccountName -SkuName Standard_LRS -Location $AzLocation.Location | Out-Null
        Write-VVerbose "New storage account $StorageAccountName created in $ResourceGroupName."
    }
}
if (($pscmdlet.ShouldProcess("Storage Account $StorageAccountName","Obtain Storage Account Context"))) {
    try {
        $SAObj = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $StorageAccountName -ErrorAction Stop
    } catch {
        throw "Cannot obtain storage account $StorageAccountName context"
    }
}



#
# Create storage containers and assign permissions
#

if (($pscmdlet.ShouldProcess("Storage Account $StorageAccountName","Create containers"))) {
    Write-VVerbose "Creating 4 containers in $StorageAccountName"
    New-AzStorageContainer -Name 'dummy' -Permission Off -Context $SAObj.Context -ea 0 | Out-Null # command fails first time
    New-AzStorageContainer -Name 'azmi-na' -Permission Off -Context $SAObj.Context | Out-Null
    New-AzStorageContainer -Name 'azmi-ro' -Permission Off -Context $SAObj.Context | Out-Null
    New-AzStorageContainer -Name 'azmi-rw' -Permission Off -Context $SAObj.Context | Out-Null
    New-AzStorageContainer -Name 'azmi-ls' -Permission Off -Context $SAObj.Context | Out-Null

    Write-VVerbose "Verify containers"
    if ((Get-AzStorageContainer -Context $SAObj.Context | where Name -Match '^azmi').Count -ne 4) {
        throw "Failed to create properly containers"
    }
    Write-VVerbose "Containers created successfully"
}

Write-VVerbose "Granting access to containers"
$ScopePrefix = $SAObj.Id + '/blobServices/default/containers/'
if (($pscmdlet.ShouldProcess("Storage Account $StorageAccountName","Grant container accesses"))) {
    New-AzRoleAssignment -ObjectId $MIObj.PrincipalId -RoleDefinitionName 'Storage Blob Data Reader' -Scope ($ScopePrefix+'azmi-ro') | Out-Null
    New-AzRoleAssignment -ObjectId $MIObj.PrincipalId -RoleDefinitionName 'Storage Blob Data Contributor' -Scope ($ScopePrefix+'azmi-rw') | Out-Null
    New-AzRoleAssignment -ObjectId $MIObj.PrincipalId -RoleDefinitionName 'Storage Blob Data Contributor' -Scope ($ScopePrefix+'azmi-ls') | Out-Null
    Write-VVerbose "Access to containers granted"
}


#
# Create storage blobs
#

$TempFile = New-TemporaryFile
Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-na' -Blob 'file1' -Context $SAObj.Context | Out-Null
Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ro' -Blob 'file1' -Context $SAObj.Context | Out-Null
Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server1-file1' -Context $SAObj.Context | Out-Null
Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server1-file2' -Context $SAObj.Context | Out-Null
Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server1-file3' -Context $SAObj.Context | Out-Null
Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server2-file1' -Context $SAObj.Context | Out-Null
Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server2-file2' -Context $SAObj.Context | Out-Null
Remove-Item -Path $TempFile.FullName -Force


#
#  Create Key Vaults
#

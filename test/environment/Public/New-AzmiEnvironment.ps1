function New-AzmiEnvironment {

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
    [string]$KeyVaultsBaseName,

    [Parameter(Mandatory=$false)]
    [string]$ManagedIdentityName,

    [Parameter()]
    [switch]$SkipModulesCheck
)

throw "Not implemented command!"

Test-AzmiSubscription -SubscriptionID $SubscriptionID -Verbose
New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -LocationName $LocationName -Verbose
New-AzmiManagedIdentity -ResourceGroupName $ResourceGroupName -ManagedIdentityName $ManagedIdentityName -Verbose
New-AzmiStorageAccount -ResourceGroupName $ResourceGroupName -StorageAccountName $StorageAccountName -Verbose
# TODO: Catch generated storage account if originally null
New-AzmiStorageContainers -ResourceGroupName $ResourceGroupName -StorageAccountName $StorageAccountName

# New-AzmiKeyVaults
}

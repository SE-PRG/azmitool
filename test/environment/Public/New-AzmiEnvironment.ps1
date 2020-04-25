function New-AzmiEnvironment {

    param (

        [Parameter(Mandatory=$true,Position=0,HelpMessage="Run Get-AzSubscription to see subscription IDs")]
        [ValidatePattern('[0-9a-f\-]{36}')]
        [string]$SubscriptionID,

        [Parameter(Mandatory=$false,Position=1,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,HelpMessage="Location in which to create resources. If not specified, inherited from Resource Group")]
        [string]$LocationName,

        [Parameter(Mandatory=$false)]
        [string]$StorageAccountName,

        [Parameter(Mandatory=$false)]
        [string]$KeyVaultsBaseName,

        [Parameter(Mandatory=$false)]
        [string]$ManagedIdentityName = 'azmitest'

    )

    #throw "Not implemented command!"

    Test-AzmiSubscription -SubscriptionID $SubscriptionID -Verbose
    New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -LocationName $LocationName -Verbose
    New-AzmiManagedIdentity -ResourceGroupName $ResourceGroupName -ManagedIdentityName $ManagedIdentityName -Verbose
    New-AzmiStorageAccount -ResourceGroupName $ResourceGroupName -StorageAccountName $StorageAccountName -Verbose
    New-AzmiKeyVaults -ResourceGroupName $ResourceGroupName -KeyVaultsBaseName $KeyVaultsBaseName

    # TODO: Think something about output, maybe we can just prepare arguments for testing script?
}

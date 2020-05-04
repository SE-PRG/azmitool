function New-AzmiKeyVaults {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (

        [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="The base name for key vaults to create")]
        [string]$KeyVaultsBaseName,

        [Parameter(Mandatory=$false,Position=2,HelpMessage="The name of managed identity to use or create")]
        [string]$ManagedIdentityName = 'azmitest'

    )


    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #

    if ($KeyVaultsBaseName) {
        $KVNames = @("$KeyVaultsBaseName-na","$KeyVaultsBaseName-ro")
        Write-AzmiVerbose "Checking for existence of Key Vaults @($KVNames -join ', ')..."
        $KeyVaults = Get-AzKeyVault -ea 0
        $KVObjs = $KVNames | % {$KeyVaults | where VaultName -eq $_}
        if ($KVObjs) {
            if ($KVObjs.Count -eq 2) {
                Write-AzmiVerbose "Key Vaults @($KVNames -join ', ') are existing."
            } else {
                throw "Found only Key Vault $($KVobjs.VaultName)"
            }
        } else {
            Write-AzmiVerbose "Key Vaults not found."
        }
    }

    if (!$KVObjs) {

        if (!$KeyVaultsBaseName) {
            Write-AzmiVerbose "Generating random base name for Key Vaults"
            $KeyVaultsBaseName = Get-AzmiRandomName Test-AzmiKeyVaultBaseName
            $KVNames = @("$KeyVaultsBaseName-na","$KeyVaultsBaseName-ro")
            Write-AzmiVerbose "Using Key vaults base name $KeyVaultsBaseName"
        }

        # first confirm if we have resource group
        $RGObj = New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        # TODO: Add RGObj as argument from pipeline

        if (($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Key Vaults"))) {
            try {
                $KVObjs = foreach ($KVName1 in $KVNames) {
                    Write-AzmiVerbose "Creating Key Vault $KVName1"
                    New-AzKeyVault -Name $KVName1 `
                        -Location $RGObj.Location `
                        -ResourceGroupName $ResourceGroupName `
                        -wa 0 -ea Stop
                }
                Write-AzmiVerbose "New Key Vaults $($KVName -join ', ') created in $ResourceGroupName."
            } catch {
                throw "Failed to create Key Vaults: $_"
            }
        }

        Write-AzmiVerbose "Check for existence of Managed Identity"
        $MIObj = New-AzmiManagedIdentity -ResourceGroupName $ResourceGroupName -ManagedIdentityName $ManagedIdentityName

        # grant access to RO key vault
        if (($pscmdlet.ShouldProcess("Key Vault $KeyVaultsBaseName-ro","Grant access"))) {
            # Get policy on all three services
            Set-AzKeyVaultAccessPolicy -VaultName "$KeyVaultsBaseName-ro" `
                -ObjectId $MIObj.PrincipalId `
                -PermissionsToKeys Get `
                -PermissionsToSecrets Get `
                -PermissionsToCertificates Get `
                -wa 0 -ea Stop
        }

        # create secrets
        foreach ($KeyVaultName in @("$KeyVaultsBaseName-ro","$KeyVaultsBaseName-na")) {
            if (($pscmdlet.ShouldProcess("Key Vault $KeyVaultName","Create secrets"))) {
                foreach ($SecretValue in @('version1','version2')) {
                    Set-AzKeyVaultSecret -VaultName $KeyVaultName `
                        -Name 'secret1' `
                        -SecretValue (ConvertTo-SecureString $SecretValue -AsPlainText -Force) | Out-Null
                }
            }
        }
    }



    # return value
    $KVObjs

    # end of command
    Write-AzmiVerbose "Finished" -Format G
}
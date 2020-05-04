function New-AzmiManagedIdentity {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (

        [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="Managed Identity to be used with Azmi testing")]
        [string]$ManagedIdentityName = 'azmitest'
    )


    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #


    Write-AzmiVerbose "Checking managed identity $ManagedIdentityName..."
    $MIObj = Get-AzUserAssignedIdentity -ea 0 | where Name -eq $ManagedIdentityName
    if ($MIObj) {
        Write-AzmiVerbose "Found existing managed identity $ManagedIdentityName"
    } else {
        Write-AzmiVerbose "Managed identity $ManagedIdentityName not found."

        # first confirm if we have resource group
        $RGObj = New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        # TODO: Add RGObj as argument from pipeline

        if (($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Managed Identity $ManagedIdentityName"))) {
            Write-AzmiVerbose "Creating new managed identity $ManagedIdentityName..."
            $MIObj = New-AzUserAssignedIdentity -ResourceGroupName $ResourceGroupName -Name $ManagedIdentityName -Location $RGObj.Location
            Write-AzmiVerbose "New managed identity $ManagedIdentityName created in $ResourceGroupName."
        }
    }

    # return value
    $MIObj

    # end of command
    Write-AzmiVerbose "Finished" -Format G

}

Set-Alias -Name New-AzmiMI -Value New-AzmiManagedIdentity

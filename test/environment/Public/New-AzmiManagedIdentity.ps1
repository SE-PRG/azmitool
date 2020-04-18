function New-AzmiManagedIdentity {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (

        [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="Location for newly created resource group, run Get-AzLocation for options")]
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
        Write-VVerbose "Found existing managed identity $ManagedIdentityName"
    } else {
        Write-AzmiVerbose "Managed identity $ManagedIdentityName not found."

        # first confirm if we have resource group
        $RGObj = New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        # TODO: Add RGObj as argument from pipeline

        if (($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Managed Identity $ManagedIdentityName"))) {
            Write-AzmiVerbose "Creating new managed identity $ManagedIdentityName..."
            $MIObj = New-AzResourceGroup -Name $ResourceGroupName -Location $RGObj.Location -ea Stop
            Write-AzmiVerbose "New managed identity $ManagedIdentityName created in $ResourceGroupName."
        }
    }

    # return value
    $MIObj

    # end of command
    Write-AzmiVerbose "Finished" -Format G

}

Set-Alias -Name New-AzmiMI -Value New-AzmiManagedIdentity


function New-AzmiResourceGroup {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (
        [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="Location for newly created resource group, run Get-AzLocation for options")]
        [string]$LocationName

        )

    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #

    Write-AzmiVerbose "Checking for resource group $ResourceGroupName..."
    $RGObj = Get-AzResourceGroup -Name $ResourceGroupName -ea 0
    if ($RGObj) {
        Write-AzmiVerbose "Existing resource group $ResourceGroupName found."
    } else {

        if (!$LocationName) {
            $LocationName = Get-AzLocation | Get-Random | Select -Expand Location
            Write-AzmiVerbose "Using random location $LocationName"
        }

        if (($pscmdlet.ShouldProcess("Location $LocationName","Create Resource Group $ResourceGroupName"))) {
            Write-AzmiVerbose "Creating new resource group $ResourceGroupName."
            $RGObj = New-AzResourceGroup -Name $ResourceGroupName -Location $LocationName -ea Stop
            Write-AzmiVerbose "New resource group $ResourceGroupName created in $LocationName."
        }
    }

    # return value
    $RGObj

    # end of command
    Write-AzmiVerbose "Finished" -Format G

}


Set-Alias -Name New-AzmiRG -Value New-AzmiResourceGroup

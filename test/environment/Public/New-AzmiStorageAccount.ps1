function New-AzmiStorageAccount {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (

    [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="Location for newly created resource group, run Get-AzLocation for options")]
        [string]$ManagedIdentityName = 'azmitest',

        [Parameter(Mandatory=$false,Position=2,HelpMessage="The name of storage account to create")]
        [string]$StorageAccountName
    )


    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #

    if ($StorageAccountName) {
        $SAObj = Get-AzStorageAccount -ea 0 | ? Name -eq $StorageAccountName
        if ($SAObj) {
            Write-AzmiVerbose "Storage account $StorageAccountName is existing."
        }
    }

    if (!$SAObj) {

        if (!$StorageAccountName) {
            Write-AzmiVerbose "Generating random storage account name"
            $StorageAccountName = Get-AzmiRandomName Test-AzmiStorageAccountName
            Write-AzmiVerbose "Using storage account name $StorageAccountName"
        }

        # first confirm if we have resource group
        $RGObj = New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        # TODO: Add RGObj as argument from pipeline

        if (($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Storage Account $StorageAccountName"))) {
            Write-AzmiVerbose "Creating storage account $StorageAccountName"
            $SAObj = New-AzStorageAccount -ResourceGroupName $ResourceGroupName `
                -Name $StorageAccountName `
                -SkuName Standard_LRS `
                -Location $RGObj.Location `
                -Kind BlobStorage `
                -AccessTier Cool `
                -ErrorAction Stop
            Write-AzmiVerbose "New storage account $StorageAccountName created in $ResourceGroupName."
        }
    }

    # return value
    $SAObj

    # end of command
    Write-AzmiVerbose "Finished" -Format G

}

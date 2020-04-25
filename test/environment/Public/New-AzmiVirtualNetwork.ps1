function New-AzmiVirtualNetwork {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (

        [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="The name of virtual network to create")]
        [string]$VNetName = 'azmitest-VNET'
    )


    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #

    # check if virtual network exists
    Write-AzmiVerbose "Checking for existence of Virtual Network $VNetName..."
    $VNetObj = Get-AzVirtualNetwork -Name $VNetName -ResourceGroupName $ResourceGroupName -ea 0
    if ($VNetObj) {
        Write-AzmiVerbose "Virtual Network $VNetName is existing."
    } else {
        Write-AzmiVerbose "Virtual Network $VNetName is not existing."
    }

    # first confirm if we have resource group
    if (!$VNetObj) {
        $RGObj = New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        # TODO: Add RGObj as argument from pipeline
    }

    if ((!$VNetObj) -and ($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Virtual Network $VNetName"))) {

        # Create an inbound network security group rule for port 22
        Write-AzmiVerbose "Creating NSG Rule"
        $nsgRuleSSH = New-AzNetworkSecurityRuleConfig `
            -Name "AllowSSHFromMyLocation"  `
            -Description 'Allow SSH to port 22 only from my public IP address' `
            -Protocol "Tcp" `
            -Direction "Inbound" `
            -Priority 1000 `
            -SourceAddressPrefix (irm az.iric.online/myip -verbose:$false) `
            -SourcePortRange * `
            -DestinationAddressPrefix * `
            -DestinationPortRange 22 `
            -Access "Allow"

        # Create a network security group
        Write-AzmiVerbose "Creating Network Security Group"
        $nsg = New-AzNetworkSecurityGroup `
            -ResourceGroupName $ResourceGroupName `
            -Location $RGObj.Location `
            -Name "$VNetName-NSG" `
            -SecurityRules $nsgRuleSSH `
            -wa 0 -ea Stop -Force

        Write-AzmiVerbose "Creating VNet subnet"
        $subnetConfig = New-AzVirtualNetworkSubnetConfig `
            -Name "azmiSubnet" `
            -AddressPrefix 192.168.1.0/24 `
            -NetworkSecurityGroup $nsg `
            -wa 0 -ea Stop

        Write-AzmiVerbose "Creating Virtual Network..."
        $vnet = New-AzVirtualNetwork `
            -ResourceGroupName $ResourceGroupName `
            -Location $RGObj.Location `
            -Name $VNetName `
            -AddressPrefix 192.168.0.0/16 `
            -Subnet $subnetConfig `
            -wa 0 -ea Stop -Force
        Write-AzmiVerbose "Virtual Network created"

        $VNetObj = Get-AzVirtualNetwork -Name $VNetName -ResourceGroupName $ResourceGroupName
    }

    # return value
    $VNetObj

    # end of command
    Write-AzmiVerbose "Finished" -Format G

}

Set-Alias -Name New-AzmiVNet -Value New-AzmiVirtualNetwork
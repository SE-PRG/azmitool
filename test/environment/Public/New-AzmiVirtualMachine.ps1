function New-AzmiVirtualMachine {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (

        [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="The name of virtual machine to create")]
        [string]$VirtualMachineName = 'azmitest',

        [Parameter(Mandatory=$false,Position=2,HelpMessage="The name of managed identity to use or create")]
        [string]$ManagedIdentityName = 'azmitest',

        [Parameter(Mandatory=$false,Position=3,HelpMessage="The name of virtual network to create")]
        [string]$VNetName = 'azmitest-VNET'
    )


    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #

    # check if virtual machine exists
    Write-AzmiVerbose "Checking for existence of Virtual Machine $VirtualMachineName..."
    $VMObj = Get-AzVM -ea 0 | ? Name -eq $VirtualMachineName
    if ($VMObj) {
        Write-AzmiVerbose "Virtual Machine $VirtualMachineName is existing."
    } else {
        Write-AzmiVerbose "Virtual Machine $VirtualMachineName is not existing."
    }


    # first confirm if we have resource group and VNET
    if (!$VMObj) {
        $RGObj = New-AzmiResourceGroup -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        # TODO: Add RGObj as argument from pipeline
        $VNetObj = New-AzmiVirtualNetwork -ResourceGroupName $ResourceGroupName -VNetName $VNetName
        $MIObj = New-AzmiManagedIdentity -ResourceGroupName $ResourceGroupName -ManagedIdentityName $ManagedIdentityName
    }

    if (!$VMObj -and ($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Public IP $VirtualMachineName-PIP"))) {

        # TODO: In next version, move PIP to network and use load balancer port translation
        Write-AzmiVerbose "Creating Public IP Address"
        $pip = New-AzPublicIpAddress `
            -ResourceGroupName $ResourceGroupName `
            -Location $RGObj.Location `
            -AllocationMethod Static `
            -IdleTimeoutInMinutes 4 `
            -Name "$VirtualMachineName-PIP" `
            -DomainNameLabel "$VirtualMachineName-$(Get-Random)" `
            -wa 0 -ea Stop
    }

    if (!$VMObj -and ($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Network Interface $VirtualMachineName-NIC"))) {    
        # Create a virtual network card and associate with public IP address and NSG
        $nic = New-AzNetworkInterface `
            -Name "$VirtualMachineName-NIC" `
            -ResourceGroupName $ResourceGroupName `
            -Location $RGObj.Location `
            -SubnetId $VNetObj.Subnets[0].Id `
            -PublicIpAddressId $pip.Id `
            -wa 0 -ea Stop
    }

    if (!$VMObj -and ($pscmdlet.ShouldProcess("Resource Group $ResourceGroupName","Create Virtual Machine $VirtualMachineName"))) {

        # Define a blank credential object
        $blankPassword = ConvertTo-SecureString ' ' -AsPlainText -Force
        $cred = New-Object System.Management.Automation.PSCredential ($env:USERNAME, $blankPassword)

        Write-AzmiVerbose "Creating VM OS Configuration"
        $vmConfig = New-AzVMConfig `
            -VMName $VirtualMachineName `
            -VMSize "Standard_D1" `
            -IdentityType UserAssigned `
            -IdentityId $MIObj.Id | `
        Set-AzVMOperatingSystem `
            -Linux `
            -ComputerName $VirtualMachineName `
            -Credential $cred `
            -DisablePasswordAuthentication | `
        Set-AzVMSourceImage `
            -PublisherName "Canonical" `
            -Offer "UbuntuServer" `
            -Skus "18.04-LTS" `
            -Version "latest" | `
        Add-AzVMNetworkInterface `
            -Id $nic.Id | `
        Set-AzVMBootDiagnostic -Disable `

        # Configure the SSH key
        $SSHKeyPath = '~/.ssh/id_rsa.pub'
        if (Test-Path $SSHKeyPath) {
            Add-AzVMSshPublicKey -VM $vmconfig `
                -KeyData (Get-Content $SSHKeyPath) `
                -Path "/home/$($env:USERNAME)/.ssh/authorized_keys" | Out-Null
                Write-AzmiVerbose "Added SSH key $SSHKeyPath"
        } else {
            Write-Warning "Cannot find public SSH key, please add manually with Add-AzVMSshPublicKey"
        }

        Write-AzmiVerbose "Creating Virtual Machine $VirtualMachineName, please wait..."
        New-AzVM -VM $vmConfig `
            -ResourceGroupName $ResourceGroupName `
            -Location $RGObj.Location `
            -wa 0 -ea Stop | Out-Null
        Write-AzmiVerbose "New Virtual Machine $VirtualMachineName created in $ResourceGroupName."

        $VMObj = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $VirtualMachineName -ea Stop
    }

    # return value
    $VMObj

    # end of command
    Write-AzmiVerbose "Finished" -Format G

}


Set-Alias -Name New-AzmiVM -Value New-AzmiVirtualMachine
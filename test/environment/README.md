# Test Environment for azmi tool

This PowerShell module is creating testing environment for azmi tool.
https://github.com/SRE-PRG/azmitool

## Contents

- Basic Usage
- Required permissions
- Commands
- Test VMs

## Basic Usage

Prior to using this PowerShell module you need to import it. No local admin rights are needed.

```PowerShell
git clone https://github.com/SRE-PRG/azmitool # skip if you already have it clonned
Import-Module .\azmitool\test\environment\AzmiEnvironment.psd1 -Force
```

After this you will get additional commands in your session, which will enable you to create environment. The most simple way how to create it, is like this:

```PowerShell
New-AzmiEnvironment -SubscriptionID $MySubscriptionId -Verbose
```

That command will do following:
- verify you are logged in to Azure and set your subscription as active
- create resource group **AzmiEnvironment**
- create managed identity **azmitest**
- create storage account like **azmitestNN**, containers and blobs
- create key vaults like **azmitestNN-ro / na**, secrets, certificates and keys

All names are configurable using command arguments and also available as separate commands.
Detailed explanations are within separate commands explanations below.

## Required permissions

In order to create new resource group and assign permissions on objects within, you must have Owner rights on subscription level.

If you are using existing resource group, then it is enough to have owner rights on that resource group.

Due to Key Vault limitations, you cannot use AAD guest accounts for creating this environment.

On your local environment (laptop or desktop machine), you do not need to have any special rights.
Module will download and install required dependant Azure modules.

## Module commands

To see list of all commands or respective commands help pages, after importing module, run any of commands:
```PowerShell
Get-Command -Module AzmiEnvironment
Get-Help <CommandName>
Get-Command -Module AzmiEnvironment | Get-Help | Select Name, Synopsis
```

### New-AzmiEnvironment

This is the basic module command which is explained above. It is just calling other commands listed below.

### New-AzmiResourceGroup

New-AzmiResourceGroup creates new resource group **AzmiEnvironment**, if not existing.
If location is not specified, it will assign random one from list of available Azure data centers.
It is recommended to specify Azure location closes to you.

### New-AzmiManagedIdentity

New-AzmiManagedIdentity creates new managed identity **azmitest**, if not existing.

### New-AzmiStorageAccount

New-AzmiStorageAccount creates storage account, containers and blobs.

If storage account name is not specified, it will assign globally unique name starting with **azmitest** and followed by a few digits.

After storage account is created, it will create required storage containers and assign proper accesses on them for specified managed identity. Finally, storage blobs needed for tests will be created.

### New-AzmiKeyVaults

New-AzmiKeyVaults is creating two key vaults required for testing and their objects and assigning permissions.

Key vault names will be created by appending `-ro` and `-na` to provided base name. If base name is not specified, it will assign name to ensure globally unique names starting with **azmitest** and followed by a few digits.

After key vaults are created, it will create required objects in them and grant access policies for provided managed identity.

## Test VMs

Module provides also two commands for creating VMs that can be used for testing.

### New-AzmiVirtualNetwork

New-AzmiVirtualNetwork will create Azure Virtual Network (VNet) where VM(s) will be created.
If name is not provided, it will use default one **azmitest-VNET**.

Network Security Group will be created to allow access only from your IP address.
Your public IP will be determined using az.iric.online/MyIP tool (GitHub repo [here](https://github.com/iricigor/MyIP)).
If needed, additional IPs can be configured directly in Azure Portal.

### New-AzmiVirtualMachine

New-AzmiVirtualMachine will first create Public IP address and Network Interface for new VM and assign it in before created VNet.

After that, it will create new Ubuntu 18.04 VM. If it finds, it will upload your SSH public key to the VM so you can login directly login using your currently logged in username.

### Create Azure DevOps testing pool

This part is not automated, as it will require much higher privileges also on your ADO instance.

Briefly, you can configure new ADO pipeline agent pool and install ADO agent on your VM(s) created with script above and then you can have automated testing environment directly from ADO.

This section will be expanded in future.

https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/v2-linux?view=azure-devops

1. Create new pool
2. Create new PAT (Read & manage on Agent Pools)
3. Download ADO agent
4. Configure and start agent service
5. Install additional components (dotnet, pwsh, ...)


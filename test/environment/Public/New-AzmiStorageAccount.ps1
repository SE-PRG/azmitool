function New-AzmiStorageAccount {
    [cmdletbinding(SupportsShouldProcess=$True)]

    param (

        [Parameter(Mandatory=$false,Position=0,HelpMessage="Run Get-AzResourceGroup to see existing resource groups")]
        [string]$ResourceGroupName = 'AzmiEnvironment',

        [Parameter(Mandatory=$false,Position=1,HelpMessage="The name of storage account to create")]
        [string]$StorageAccountName,

        [Parameter(Mandatory=$false,Position=2,HelpMessage="The name of managed identity to use or create")]
        [string]$ManagedIdentityName = 'azmitest'
    )


    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #

    # check if storage account exists
    if ($StorageAccountName) {
        Write-AzmiVerbose "Checking for existence of Storage account $StorageAccountName..."
        $SAObj = Get-AzStorageAccount -ea 0 | ? StorageAccountName -eq $StorageAccountName
        if ($SAObj) {
            Write-AzmiVerbose "Storage account $StorageAccountName is existing."
        } else {
            Write-AzmiVerbose "Storage account $StorageAccountName is not existing."
        }
    }

    # create storage account
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

    # check if containers exist
    if (($pscmdlet.ShouldProcess("Storage Account $StorageAccountName","Check existence of containers"))) {
        # although first action is get action, must be processed with -WhatIf due to Context non-existence
        Write-AzmiVerbose "Checking Storage Account Containers..."
        $ContObjs = Get-AzStorageContainer -Context $SAObj.Context -Prefix 'azmi' -ea 0
        if ($ContObjs) {
            $ContNames = $ContObjs.Name -join ', '
            if ($ContObjs.Count -eq 4) {
                Write-AzmiVerbose "Storage Account Containers are existing: $ContNames`."
            } else {
                throw "We did not find exactly 4 containers, but $($ContObjs.Count): $ContNames"
            }
        } else {
            Write-AzmiVerbose "There are no azmi Storage Account Containers."
        }
    }

    # create containers
    if ((!$ContObjs) -and ($pscmdlet.ShouldProcess("Storage Account $StorageAccountName","Create 4 containers"))) {

        Write-AzmiVerbose "Creating 4 containers in $StorageAccountName"
        try {
            New-AzStorageContainer -Name 'dummy' -Permission Off -Context $SAObj.Context -ea 0 | Out-Null # command fails first time
            New-AzStorageContainer -Name 'azmi-na' -Permission Off -Context $SAObj.Context -ea Stop | Out-Null
            New-AzStorageContainer -Name 'azmi-ro' -Permission Off -Context $SAObj.Context -ea Stop | Out-Null
            New-AzStorageContainer -Name 'azmi-rw' -Permission Off -Context $SAObj.Context -ea Stop | Out-Null
            New-AzStorageContainer -Name 'azmi-ls' -Permission Off -Context $SAObj.Context -ea Stop | Out-Null
            Write-AzmiVerbose "Containers created"
        } catch {
            throw "Creating containers failed: $_"
        }
    }

    # grant containers access
    if ((!$ContObjs) -and ($pscmdlet.ShouldProcess("Storage Account $StorageAccountName","Grant container accesses"))) {


        Write-AzmiVerbose "Check for existence of Managed Identity"
        $MIObj = New-AzmiManagedIdentity -ResourceGroupName $ResourceGroupName -ManagedIdentityName $ManagedIdentityName

        Write-AzmiVerbose "Granting access to containers"
        $ScopePrefix = $SAObj.Id + '/blobServices/default/containers/'
        try {
            New-AzRoleAssignment -ObjectId $MIObj.PrincipalId -RoleDefinitionName 'Storage Blob Data Reader' -Scope ($ScopePrefix+'azmi-ro') -ea Stop | Out-Null
            New-AzRoleAssignment -ObjectId $MIObj.PrincipalId -RoleDefinitionName 'Storage Blob Data Contributor' -Scope ($ScopePrefix+'azmi-rw') -ea Stop | Out-Null
            New-AzRoleAssignment -ObjectId $MIObj.PrincipalId -RoleDefinitionName 'Storage Blob Data Contributor' -Scope ($ScopePrefix+'azmi-ls') -ea Stop | Out-Null
            Write-AzmiVerbose "Access to containers granted"
        } catch {
            throw "Granting containers access failed: $_"
        }
    }

    # create blobs
    if ((!$ContObjs) -and ($pscmdlet.ShouldProcess("Storage Account $StorageAccountName","Create storage blobs"))) {

        Write-AzmiVerbose "Create Storage Blobs"
        try {
            $TempFile = New-TemporaryFile
            Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-na' -Blob 'file1' -Context $SAObj.Context -ea Stop -Verbose:$false | Out-Null
            Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ro' -Blob 'file1' -Context $SAObj.Context -ea Stop -Verbose:$false | Out-Null
            Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server1-file1' -Context $SAObj.Context -ea Stop -Verbose:$false | Out-Null
            Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server1-file2' -Context $SAObj.Context -ea Stop -Verbose:$false | Out-Null
            Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server1-file3' -Context $SAObj.Context -ea Stop -Verbose:$false | Out-Null
            Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server2-file1' -Context $SAObj.Context -ea Stop -Verbose:$false | Out-Null
            Set-AzStorageBlobContent -File $TempFile.FullName -Container 'azmi-ls' -Blob 'server2-file2' -Context $SAObj.Context -ea Stop -Verbose:$false | Out-Null
            Remove-Item -Path $TempFile.FullName -Force
            Write-AzmiVerbose "Storage Blobs Created"
        } catch {
            throw "Creating storage blobs failed: $_"
        }
    }

    # return value
    $SAObj

    # end of command
    Write-AzmiVerbose "Finished" -Format G

}

<img align="right" width="320" height="160" src="img/azmi-logo.png">

# azmi

## Description

Azure Managed Identity tool - **azmi** - simplifies authentication to Azure resources inside Azure Linux VMs. Using VM with assigned Managed Identity you can easily authenticate against Azure services like Key Vault, Storage Account, etc.

## Examples

```bash
# get token from Azure infrastructure
azmi gettoken

# download blob from a storage account container and save to a file
azmi getblob --blob $BLOB_URL --file $FILE

# upload file as a blob to storage account container
azmi setblob --file $FILE --container $CONTAINER_URL
```

## Download

To download executable / package, use following commands:
- executable
```bash
curl https://azmideb.blob.core.windows.net/azmi-deb/azmi  > ./azmi
chmod +x azmi
ls azmi -l
```
P.S. Running azmi executable does not require root privilege.

- Debian package
```bash
curl https://azmideb.blob.core.windows.net/azmi-deb/azmi.deb > ./azmi.deb
ls azmi.deb -l
sudo dpkg -i ./azmi.deb
```

## How it works
All Azure authentication is completly transparent VM user or for a running script. There is no need to keep any secrets in the code or on the system, or to rotate and distribute them.

Azmi is utilizing managed identities to autheticate against Azure AD and obtain access token. This token is then sent to specified resource together with request for specific action (read/write data).

![azmi - how it works](img/azmi-explanation.png)

For other azmi commands (i.e. setblob) authentication works the same way. The only difference is with request being sent to target resource.

Azmi is not working accross different AAD tenants.

Read more:
- [Managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
 
## Pipeline statuses

- Package build [![Build status](https://skype.visualstudio.com/SCC/_apis/build/status/SE-UP/azmi/build%20-%20azmi)](https://skype.visualstudio.com/SCC/_build/latest?definitionId=8166)
- Integration tests [![Build status](https://skype.visualstudio.com/SCC/_apis/build/status/SE-UP/azmi/Integration%20-%20azmi)](https://skype.visualstudio.com/SCC/_build/latest?definitionId=8091)

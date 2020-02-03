# azmitool
Azure Managed Identity tool

## Description

Simplifies authentication in Azure VM. Using VM with assigned Managed Identity you can easily authenticate against Azure services like Key Vault, Storage Account, etc.

## Examples

```bash
# get token from Azure infrastructure
azmi gettoken

# download blob from a storage account container and save to a file
azmi getblob --blob $BLOB_URL --file $FILE

# upload file as a blob to storage account container
azmi setblob --file $FILE --container $CONTAINER_URL
```

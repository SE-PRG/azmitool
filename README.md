# azmitool
Azure Managed Identity tool

## Description

Simplifies authentication in Azure VM. Using VM with assigned Managed Identity you can easily authenticate against Azure services like Key Vault, Storage Account, etc.

## Examples

```bash
# read key vault secret
azmitool getsecret https://mykey.vault.azure.net/secrets/mysecret

# download blob from a storage account container and save to a file
azmitool getblob --blob $BLOB --file $FILE

# upload file as a blob to storage account container
azmitool setblob --file $FILE --container $CONTAINER
```

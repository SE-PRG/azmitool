# azmitool
Azure Managed Identity tool

## Description

Simplifies authentication in Azure VM. Using VM with assigned Managed Identity you can easily authenticate against Azure services like Key Vault, Storage Account, etc.

## Examples

```bash
# read key vault secret
azmitool getsecret https://mykey.vault.azure.net/secrets/mysecret

# upload blob to storage account, using variables
azmitool setblob $FILE $BLOB
```

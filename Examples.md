Below you may find some examples how to use `azmi` tool and their explanations.

## Basic commands
```bash
# list all commands that azmi supports
azmi --help

# get details on specific command
azmi gettoken --help

# obtain token for Azure storage
azmi gettoken --endpoint storage

# obtain management token and parse it as JWT
azmi gettoken --jwt-format
```

## Storage commands

Read more about [Azure storage accounts here](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview).

```bash
# list all blobs in container
azmi listblobs --container $CONTAINER

# count how many blobs starts with given prefix
azmi listblobs -c $CONTAINER --prefix $PREFIX | wc -l


# read single blob from storage account and save it to file
azmi getblob --blob $CONTAINER/$BLOB --file $FILE

# read a blob using specified managed identity
azmi getblob -b $$BLOBURL --file $FILE --identity $ID

# download blob only if newer than existing file
azmi getblob -b $BLOBURL -f $FILE --if-newer

# download blob and delete if afterwards
azmi getblob -b $BLOBURL -f $FILE --delete-after-copy


# download all blobs and save them in given directory
azmi getblobs --container $CONTAINER --directory $DOWNLOAD_DIR

# download blobs starting with given prefix
azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --prefix $PREFIX

# download blobs, but exclude ones matching given regular expression
azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --exclude $EXCLUDE


# upload file to storage account container
azmi setblob -f $UPLOADFILE --container $CONTAINER

# upload file and specify exact uploaded blob URL
azmi setblob -f $UPLOADFILE --blob $BLOBURL

# upload file even if exact blob already exists
azmi setblob -f $UPLOADFILE --container $CONTAINER --force
```

### Comments
- To be able to read data from storage blob, managed identity would need one of [blob specific RBAC roles](https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-rbac-portal#rbac-roles-for-blobs-and-queues), for example:
  - Storage Blob Data Reader
  - Storage Blob Data Contributor
- If file or blob in destination exists, following happens:
  - `getblob` is overwriting file in destination
  - `setblob` fails if blob with same name exists, override it with `--force`
- `getblobs` will output one line for each blob operation and one more for overall result
- Commands `listblobs` and `getblobs` return upto 5,000 blobs filtered on Azure API side by argument `--prefix`.
Filtering with `--exclude` though providing more flexibility with regex, is only client side filtering.
This means it operates on server filtered set which can be already topped to first 5,000 blobs.
If a container has more than 5,000 blobs, , it is required to use `--prefix`, otherwise results might be inconclusive.

## Key Vault Secret commands

```bash
azmi getsecret --secret-identifier ${KV_NA}/secrets/buriedSecret
azmi getsecret --secret-identifier ${KV_RO}/secrets/readMyPassword --identity $identity
```

### Comments
Command `getsecret` is currently being built.
It may have some changes or new features in near future.

## Complex examples

Each `azmi` command is doing only one operation against related Azure resource.
If you need operation that will do more actions just use `azmi` multiple times!

### Example 1

Following example is replacing secrets in predefined placeholders with actual values.
It is calling three `azmi` commands: `getblob`, `getsecret` and `setblob`

```bash
#!/bin/bash

cont="https://myaccount.blob.core.windows.net/mycontainer"
KV="https://myKV.vault.azure.net/secrets"
templateFile="passwords.template"
outputFile="passwords.output"
pattern='\$\{(.+)\}' # regex for replacement placeholder, as ${name}

# get template file from blob
azmi getblob --blob $cont/$templateFile --file $templateFile
while read line; do
  if [[ $line =~ $pattern ]] ; then
    placeholder="${BASH_REMATCH[0]}"
    secretName="${BASH_REMATCH[1]}"
    # get secret from key vault
    secretValue=`azmi getsecret --secret "$KV/$secretName"`
    echo ${line/$placeholder/$secretValue} >> $outputFile
  fi
done < templateFile
# save updated file to blob
azmi setblob --blob $cont/$outputFile --file $outputFile
rm $outputFile
```

**Note:** This is just example. It is not recommended to keep secrets in a file!


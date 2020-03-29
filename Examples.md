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

# download blobs, but exclude ones matching given regex
azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --exclude $EXCLUDE


# upload file to storage account container
azmi setblob -f $UPLOADFILE --container $CONTAINER

# upload file and specify exact uploaded blob URL
azmi setblob -f $UPLOADFILE --blob $BLOBURL

# upload file even if exact blob already exists
azmi setblob -f $UPLOADFILE --container $CONTAINER --force
```

### Comments
- TODO: access rights data reader
- `getblob` is overwriting file in destination
- `setblob` fails if blob with same name exists, override it with `--force`
- TODO: `--prefix` vs `--exclude` in `listblobs` and `getblobs`
- `getblobs` will output one line for each blob operation and one more for overall result

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


```bash
#!/bin/bash

cont="https://myaccount.blob.core.windows.net/mycontainer"
KV="https://myKV.vault.azure.net/secrets"

azmi getblob --blob $cont/passwords.template --file passwords.input
while read servername; do
   secret=`azmi getsecret --secret "$KV/$servername"`
   echo "$servername : $secret" >> passwords.output
done < passwords.input
azmi setblob --blob $cont/passwords.output --file passwords.output
rm passwords.output
```

**Note:** This is just example. It is not recommended to keep secrets in local file!


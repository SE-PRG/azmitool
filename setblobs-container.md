Since azmi [version 0.5](../changelog.md), azmi sub-command `setblob` is not supporting parameter `--container` anymore. You must use `--blob` to specify destination.

## Not supported
```
azmi setblob --container $CONTAINER --file $FILE
```

## Supported
```
azmi setblob --blob $BLOB --file $FILE
```

## How to switch to supported version

In reality blob url consists of container and blob paths. Solution is to update your commands like this:
```
# azmi setblob --container $CONTAINER --file $FILE
BLOB="$CONT/$FILE"
azmi setblob --blob $BLOB --file $FILE
```

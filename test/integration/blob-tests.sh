#!/bin/bash

# AzMiTool Integration tests
# It requires Bash Testing Framework
#
#   this file is part of set of files!
#


#
# setup variables
#

identity=$3
STORAGEACCOUNTNAME=$2
CONTAINER_NA="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-na"
CONTAINER_RO="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-ro"
CONTAINER_RW="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-rw"
CONTAINER_LB="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-ls"

BLOB_NA="file1"
BLOB_RO="file1"
DOWNLOAD_FILE="download.txt"
DOWNLOAD_DIR="./Download"
# prepare test upload file
DATE1=$(date +%s)   # used in file name
DATE2=$(date +%s%N) # used in file content
UPLOADFILE="upload$DATE1.txt"
echo "$DATE2" > "$UPLOADFILE"
UPLOAD_DIR="./Upload"


#
# storage subcommands testing
#

testing class "listblobs"
BC=5 # blob count
test "listblobs basic" assert.Success "azmi listblobs --container $CONTAINER_LB"
test "listblobs finds $BC blobs" assert.Equals "azmi listblobs --container $CONTAINER_LB | wc -l" $BC
test "listblobs finds 0 blobs with absolute paths (absolute-paths flag disabled)" assert.Equals "azmi listblobs --container $CONTAINER_LB | grep 'https://' | wc -l" 0
test "listblobs finds $BC blobs (absolute-paths flag enabled)" assert.Equals "azmi listblobs --container $CONTAINER_LB --absolute-paths | grep 'https://' | wc -l" $BC
BC=3; PREFIX="server1"
test "listblobs finds $BC blobs with prefix $PREFIX" assert.Equals "azmi listblobs -c $CONTAINER_LB --prefix $PREFIX | wc -l" $BC
BC=2; PREFIX="server2"
test "listblobs finds $BC blobs with prefix $PREFIX" assert.Equals "azmi listblobs -c $CONTAINER_LB --prefix $PREFIX | wc -l" $BC
test "listblobs finds $BC blobs with prefix $PREFIX (absolute-paths flag enabled)" assert.Equals "azmi listblobs -c $CONTAINER_LB -a --prefix $PREFIX | grep 'https://' | wc -l" $BC
BC=0; PREFIX="notExisting"
test "listblobs finds $BC blobs with prefix $PREFIX" assert.Equals "azmi listblobs -c $CONTAINER_LB --prefix $PREFIX | wc -l" $BC
BC=3; EXCLUDE="server2"
test "listblobs finds $BC blobs excluding $EXCLUDE" assert.Equals "azmi listblobs -c $CONTAINER_LB --exclude $EXCLUDE | wc -l" $BC
test "listblobs finds $BC blobs excluding $EXCLUDE (absolute-paths flag enabled)" assert.Equals "azmi listblobs -c $CONTAINER_LB --absolute-paths --exclude $EXCLUDE | grep 'https://' | wc -l" $BC
BC=1; EXCLUDE1="file1"; EXCLUDE2="file2"
test "listblobs finds $BC blobs with multiple excludes" assert.Equals "azmi listblobs -c $CONTAINER_LB --exclude $EXCLUDE1 --exclude $EXCLUDE2 | wc -l" $BC


testing class "getblob"
test "getblob fails on NA container" assert.Fail "azmi getblob --blob $CONTAINER_NA/$BLOB_NA --file $DOWNLOAD_FILE"
test "getblob OK on RO container" assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file $DOWNLOAD_FILE"
test "getblob fails to delete from RO container" assert.Fail "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file $DOWNLOAD_FILE --delete-after-copy"

testing class "getblob identity"
test "getblob OK on RO container using right identity"        assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity $identity"
test "getblob fails on RO container using foreign identity"      assert.Fail "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity $identity_foreign"
test "getblob fails on RO container using non-existing identity" assert.Fail "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity non-existing"


testing class "getblobs"
BC=5; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs" assert.Equals "azmi getblobs --container $CONTAINER_LB --directory $DOWNLOAD_DIR | grep Success | wc -l" $BC
# there is one extra line for summary
BC=3; PREFIX="server1"; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs with prefix $PREFIX" assert.Equals "azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --prefix $PREFIX | grep Success | wc -l" $BC
BC=0; PREFIX="notExisting"; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs with prefix $PREFIX" assert.Equals "azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --prefix $PREFIX | wc -l" $BC
BC=3; EXCLUDE="server2"; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs excluding $EXCLUDE" assert.Equals "azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --exclude $EXCLUDE | grep Success | wc -l" $BC
BC=1; EXCLUDE1="file1"; EXCLUDE2="file2"; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs with multiple excludes" assert.Equals "azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --exclude $EXCLUDE1 --exclude $EXCLUDE2 | grep Success | wc -l" $BC


testing class "setblob"
test "setblob fails on NA container" assert.Fail "azmi setblob --file $UPLOADFILE --blob ${CONTAINER_NA}/${UPLOADFILE}"
test "setblob fails on RO container" assert.Fail "azmi setblob --file $UPLOADFILE --blob ${CONTAINER_RO}/${UPLOADFILE}"
test "setblob OK on RW container" assert.Success "azmi setblob --file $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE}"

testing class "setblob force"
test "setblob fails to overwrite blob" assert.Fail "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE}"
test "setblob overwrites blob with force" assert.Success "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE} --force"


testing class "setblobs"
mkdir -p $UPLOAD_DIR && rm -rf $UPLOAD_DIR/*
# zero files
test "prepare for setblobs tests" assert.Success "azmi getblobs -c $CONTAINER_RW -d $DOWNLOAD_DIR --delete-after-copy"
test "setblobs OK with 0 files on RO container" assert.Success "azmi setblobs --directory $UPLOAD_DIR --container $CONTAINER_RO"
test "setblobs fails on non-existing directory" assert.Fail "azmi setblobs -d nonexisting -c $CONTAINER_RO"
# one file
echo "$DATE2" > "$UPLOAD_DIR/file1.txt"
test "setblobs OK with 1 file and identity" assert.Success "azmi setblobs -d $UPLOAD_DIR -c $CONTAINER_RW --identity $identity"
test "setblobs fails on RO container" assert.Fail "azmi setblobs -d $UPLOAD_DIR -c $CONTAINER_RO"
# two files
echo "$DATE2" > "$UPLOAD_DIR/file2.txt"
test "setblobs fails with 2 files without force" assert.Fail "azmi setblobs -d $UPLOAD_DIR -c $CONTAINER_RW"
test "setblobs OK with 2 files and force" assert.Success "azmi setblobs -d $UPLOAD_DIR -c $CONTAINER_RW --force"
# three files and subdirectory
mkdir -p "$UPLOAD_DIR/subdirectory" && echo "$DATE2" > "$UPLOAD_DIR/subdirectory/file3.txt"
test "setblobs OK with subdirectory" assert.Equals "azmi setblobs -d $UPLOAD_DIR -c $CONTAINER_RW --force | wc -l" 3
test "setblobs excludes a file" assert.Equals "azmi setblobs -d $UPLOAD_DIR -c $CONTAINER_RW --force --exclude file2 | wc -l" 2
rm -rf $DOWNLOAD_DIR
test "setblobs and getblobs give same files" assert.Success "azmi getblobs -c $CONTAINER_RW -d $DOWNLOAD_DIR --delete-after-copy && diff -r $UPLOAD_DIR $DOWNLOAD_DIR"


# mixed commands tests
testing class "SHA256"
test "setblob SHA256 upload" assert.Success "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE} --force"
test "getblob SHA256 download" assert.Success "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file $DOWNLOAD_FILE"
test "SHA256 same contents" assert.Success "diff $UPLOADFILE $DOWNLOAD_FILE"
UPLOADFILE_SHA256=$(sha256sum "$UPLOADFILE" | awk '{ print $1 }')
DOWNLOADFILE_SHA256=$(sha256sum $DOWNLOAD_FILE | awk '{ print $1 }')
test "SHA256 same checksums" assert.Success "[ $UPLOADFILE_SHA256 = $DOWNLOADFILE_SHA256 ]"

# noname tests make no sense without --container argument
#testing class "noname"
#test "prepare tmp file" assert.Success "rm -f /tmp/${UPLOADFILE} && echo sometext > /tmp/${UPLOADFILE}"
#test "upload tmp file" assert.Success "azmi setblob -f /tmp/${UPLOADFILE} --container ${CONTAINER_RW}"
#test "there is no noname folder" assert.Fail "azmi getblob -f /dev/null -b ${CONTAINER_RW}//tmp/${UPLOADFILE}"

testing class "if-newer"
SKIPMSG="Skipped. Blob is not newer than file."
test "setblob if-newer upload" assert.Equals "azmi setblob -f $UPLOADFILE -b ${CONTAINER_RW}/${UPLOADFILE}"
touch "$UPLOADFILE"
test "getblob skips if not newer" assert.Equals "azmi getblob -b ${CONTAINER_RW}/${UPLOADFILE} -f $UPLOADFILE --if-newer" "$SKIPMSG"
test "setblob updates blob" assert.Success "azmi setblob -f $UPLOADFILE -b ${CONTAINER_RW}/${UPLOADFILE} --force"
sleep 1
test "getblob downloads if newer" assert.Equals "azmi getblob -b ${CONTAINER_RW}/${UPLOADFILE} -f $UPLOADFILE --if-newer" "Success"
rm -rf $DOWNLOAD_FILE
test "getblob downloads newer than nonexisting" assert.Equals "azmi getblob -b ${CONTAINER_RW}/${UPLOADFILE} -f $DOWNLOAD_FILE --if-newer" "Success"

testing class "delete-after-copy"
test "setblob delete-after-copy upload" assert.Success "azmi setblob --file $UPLOADFILE -b ${CONTAINER_RW}/${UPLOADFILE} --force"
test "getblob remove blob with delete-after-copy" assert.Success "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file $DOWNLOAD_FILE --delete-after-copy"
test "getblob fails with deleted file" assert.Fail "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file $DOWNLOAD_FILE"

#
#  Clean up actions
#

rm "$UPLOADFILE"
rm -rf $DOWNLOAD_DIR

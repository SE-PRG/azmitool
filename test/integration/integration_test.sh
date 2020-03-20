#!/bin/bash

# AzMiTool Integration tests
# It requires Bash Testing Framework


#
# setup variables
#

# this script is called from testing framework script, therefore arguments are shifted
STORAGEACCOUNTNAME=$2
identity=$3


export DEBIAN_FRONTEND=noninteractive
PACKAGENAME=azmi
PACKAGEFILE=/tmp/azmiX.deb
declare -a subCommands=("gettoken" "getblob" "getblobs" "setblob" "listblobs")
identity_foreign=017dc05c-4d12-4ac2-b5f8-5e239dc8bc54

# calculated variables


#
# start testing
#

testing start "$PACKAGENAME"
testing class "package"
test "Install fake package should fail" assert.Fail "apt --assume-yes install somenonexistingpackage"

# dependencies installed?
test "Check all dependencies are installed" assert.Success "dpkg -s libstdc++6"

test "Install $PACKAGENAME package from file" assert.Success "dpkg -i $PACKAGEFILE"
test "Verify azmi binary exists and is executable" assert.Success "[ -x /usr/bin/azmi ]"

testing class "help"
test "Fail if no arguments are provided" assert.Fail "azmi"
test "Print help and return success status" assert.Success "azmi --help"

for subCommand in "${subCommands[@]}"
do
  test "Print help for $subCommand" assert.Success "azmi $subCommand --help"
  test "$subCommand verbose option" assert.Success "azmi $subCommand --help | grep verbose"
  test "Fail $subCommand with wrong args" assert.Fail "azmi $subCommand blahblah"
done


testing class "gettoken"

test "gettoken basic" assert.Success "azmi gettoken"
test "gettoken in JWT format" assert.Success "azmi gettoken --jwt-format | grep typ | grep JWT"
test "gettoken fails with wrong args" assert.Fail "azmi gettoken blahblah"

#
# storage subcommands testing
#

CONTAINER_NA="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-no-access"
CONTAINER_RO="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-r"
CONTAINER_RW="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-rw"
CONTAINER_LB="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-listblobs"

BLOB_NA="restricted_access_blob.txt"
BLOB_RO="read_only_blob.txt"
DOWNLOAD_FILE="download.txt"
DOWNLOAD_DIR="./Download"
# prepare test upload file
DATE1=$(date +%s)   # used in file name
DATE2=$(date +%s%N) # used in file content
UPLOADFILE="upload$DATE1.txt"
echo "$DATE2" > "$UPLOADFILE"


testing class "listblobs"
BC=5 # blob count
test "listblobs basic" assert.Success "azmi listblobs --container $CONTAINER_LB"
test "listblobs finds $BC blobs" assert.Equals "azmi listblobs --container $CONTAINER_LB | wc -l" $BC
BC=3; PREFIX="neu-pre"
test "listblobs finds $BC blobs with prefix $PREFIX" assert.Equals "azmi listblobs -c $CONTAINER_LB --prefix $PREFIX | wc -l" $BC
BC=1; PREFIX="neu-pre-show-me-only"
test "listblobs finds $BC blobs with prefix $PREFIX" assert.Equals "azmi listblobs -c $CONTAINER_LB --prefix $PREFIX | wc -l" $BC
BC=0; PREFIX="noBlobsShouldDownload"
test "listblobs finds $BC blobs with prefix $PREFIX" assert.Equals "azmi listblobs -c $CONTAINER_LB --prefix $PREFIX | wc -l" $BC


testing class "getblob"
test "getblob fails on NA container" assert.Fail "azmi getblob --blob $CONTAINER_NA/$BLOB_NA --file $DOWNLOAD_FILE"
test "getblob OK on RO container" assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file $DOWNLOAD_FILE"

testing class "getblob identity"
test "getblob OK on RO container using right identity"        assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity $identity"
test "getblob fails on RO container using foreign identity"      assert.Fail "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity $identity_foreign"
test "getblob fails on RO container using non-existing identity" assert.Fail "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity non-existing"


testing class "getblobs"
BC=5; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs" assert.Equals "azmi getblobs --container $CONTAINER_LB --directory $DOWNLOAD_DIR | grep Success | wc -l" $((BC+1))
# there is one extra line for summary
BC=3; PREFIX="neu-pre"; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs with prefix $PREFIX" assert.Equals "azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --prefix $PREFIX | grep Success | wc -l" $((BC+1))
BC=0; PREFIX="noBlobsShouldDownload"; rm -rf $DOWNLOAD_DIR
test "getblobs downloads $BC blobs with prefix $PREFIX" assert.Equals "azmi getblobs -c $CONTAINER_LB -d $DOWNLOAD_DIR --prefix $PREFIX | wc -l" $BC


testing class "setblob"
test "setblob fails on NA container" assert.Fail "azmi setblob --file $UPLOADFILE --container $CONTAINER_NA"
test "setblob fails on RO container" assert.Fail "azmi setblob --file $UPLOADFILE --container $CONTAINER_RO"
test "setblob OK on RW container" assert.Success "azmi setblob --file $UPLOADFILE --container $CONTAINER_RW"

testing class "setblob force"
test "setblob fails to overwrite on container" assert.Fail "azmi setblob -f $UPLOADFILE --container $CONTAINER_RW"
test "setblob fails to overwrite on blob" assert.Fail "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE}"
test "setblob overwrites blob on container" assert.Success "azmi setblob -f $UPLOADFILE --container $CONTAINER_RW --force"
test "setblob overwrites blob on blob" assert.Success "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE} --force"


# TODO: Add here setblobs tests

# mixed commands tests
testing class "SHA256"
test "setblob SHA256 upload" assert.Success "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE} --force"
test "getblob SHA256 download" assert.Success "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file $DOWNLOAD_FILE"
test "SHA256 same contents" assert.Success "diff $UPLOADFILE $DOWNLOAD_FILE"
UPLOADFILE_SHA256=$(sha256sum "$UPLOADFILE" | awk '{ print $1 }')
DOWNLOADFILE_SHA256=$(sha256sum $DOWNLOAD_FILE | awk '{ print $1 }')
test "SHA256 same checksums" assert.Success "[ $UPLOADFILE_SHA256 = $DOWNLOADFILE_SHA256 ]"

testing class "noname"
test "prepare tmp file" assert.Success "rm -f /tmp/${UPLOADFILE} && echo sometext > /tmp/${UPLOADFILE}"
test "upload tmp file" assert.Success "azmi setblob -f /tmp/${UPLOADFILE} --container ${CONTAINER_RW}"
test "there is no noname folder" assert.Fail "azmi getblob -f /dev/null -b ${CONTAINER_RW}//tmp/${UPLOADFILE}"

testing class "if-newer"
SKIPMSG="Skipped. Blob is not newer than file."
test "setblob prepares the file" assert.Equals "azmi setblob -f $UPLOADFILE -c $CONTAINER_RW"
touch "$UPLOADFILE"
test "getblob skips if not newer" assert.Equals "azmi getblob -b ${CONTAINER_RW}/${UPLOADFILE} -f $UPLOADFILE --if-newer" "$SKIPMSG"
test "setblob updates blob" assert.Success "azmi setblob -f $UPLOADFILE -c $CONTAINER_RW --force"
sleep 1
test "getblob downloads if newer" assert.Equals "azmi getblob -b ${CONTAINER_RW}/${UPLOADFILE} -f $UPLOADFILE --if-newer" "Success"
rm -rf $DOWNLOAD_FILE
test "getblob downloads newer than nonexisting" assert.Equals "azmi getblob -b ${CONTAINER_RW}/${UPLOADFILE} -f $DOWNLOAD_FILE --if-newer" "Success"

# uninstalling
testing class "package"
test "Uninstall packages" assert.Success "apt purge $PACKAGENAME -y"
test "Verify azmi binary does not exist anymore" assert.Fail "[ -f /usr/bin/azmi ]"

#
#  Clean up actions
#

rm "$UPLOADFILE"
rm -rf $DOWNLOAD_DIR

#################################
# display some diagnostic data
################################
echo -e "\n=============="
echo "Test running at '$(hostname)' host"

testing end

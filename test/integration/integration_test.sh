#!/bin/bash

# AzMiTool Integration tests
# It requires Bash Testing Framework


#
# setup variables
#

export DEBIAN_FRONTEND=noninteractive
PACKAGENAME=azmi
PACKAGEFILE=/tmp/azmiX.deb
STORAGEACCOUNTNAME=azmitest
declare -a subCommands=("gettoken" "getblob" "getblobs" "setblob" "listblobs")
identity=354800af-354e-42e0-906b-5b96e02c4e1c
identity_foreign=017dc05c-4d12-4ac2-b5f8-5e239dc8bc54

# calculated variables
CONTAINER_NA="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-no-access"
CONTAINER_RO="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-r"
CONTAINER_RW="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-rw"
CONTAINER_LB="https://${STORAGEACCOUNTNAME}.blob.core.windows.net/azmi-itest-listblobs"

# prepare test upload file
DATE1=$(date +%s)   # used in file name
DATE2=$(date +%s%N) # used in file content
UPLOADFILE="upload$DATE1.txt"
echo "$DATE2" > "$UPLOADFILE"

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

test "get access token" assert.Success "azmi gettoken"
test "get access token in JWT format" assert.Success "azmi gettoken --jwt-format | grep typ | grep JWT"


#
# storage subcommands testing
#

BLOB_NA="restricted_access_blob.txt"
BLOB_RO="read_only_blob.txt"
DOWNLOAD_FILE="download.txt"

testing class "getblob"
test "getblob fails on NA container" assert.Fail "azmi getblob --blob $CONTAINER_NA/$BLOB_NA --file $DOWNLOAD_FILE"
test "getblob OK on RO container" assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file $DOWNLOAD_FILE"

testing class "getblob identity"
test "getblob OK on RO container using right identity"        assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity $identity"
test "getblob fails on RO container using foreign identity"      assert.Fail "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity $identity_foreign"
test "getblob fails on RO container using non-existing identity" assert.Fail "azmi getblob --blob $CONTAINER_RO/$BLOB_RO --file download.txt --identity non-existing"

testing class "setblob"
test "setblob fails on NA container" assert.Fail "azmi setblob --file $UPLOADFILE --container $CONTAINER_NA"
test "setblob fails on RO container" assert.Fail "azmi setblob --file $UPLOADFILE --container $CONTAINER_RO"
test "setblob OK on RW container" assert.Success "azmi setblob --file $UPLOADFILE --container $CONTAINER_RW"

testing class "setblob force"
test "setblob fails to overwrite on container" assert.Fail "azmi setblob -f $UPLOADFILE --container $CONTAINER_RW"
test "setblob fails to overwrite on blob" assert.Fail "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE}"
test "setblob overwrites blob on container" assert.Success "azmi setblob -f $UPLOADFILE --container $CONTAINER_RW --force"
test "setblob overwrites blob on blob" assert.Success "azmi setblob -f $UPLOADFILE --blob ${CONTAINER_RW}/${UPLOADFILE} --force"

testing class "SHA256"
# it is using file uploaded in previous step
test "getblob SHA256 download" assert.Success "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file $DOWNLOAD_FILE"
test "SHA256 same contents" assert.Success "diff $UPLOADFILE $DOWNLOAD_FILE"
UPLOADFILE_SHA256=$(sha256sum "$UPLOADFILE" | awk '{ print $1 }')
DOWNLOADFILE_SHA256=$(sha256sum $DOWNLOAD_FILE | awk '{ print $1 }')
test "SHA256 same checksums" assert.Success "[ $UPLOADFILE_SHA256 = $DOWNLOADFILE_SHA256 ]"

testing class "noname"
test "prepare tmp file" assert.Success "rm -f /tmp/${UPLOADFILE} && echo sometext > /tmp/${UPLOADFILE}"
test "upload tmp file" assert.Success "azmi setblob -f /tmp/${UPLOADFILE} --container ${CONTAINER_RW}"
test "there is no noname folder" assert.Fail "azmi getblob -f /dev/null -b ${CONTAINER_RW}//tmp/${UPLOADFILE}"


# TODO: IGOR TAGGED: PROCEED FROM HERE

testing class "listblobs"
### list-blobs container
# Role(s):    Storage Blob Data Contributor
# Profile(s): bt-seu-test-id (obj. ID: d1c05b65-ccf9-47bd-870d-4e44d209ee7a), kotipoiss-identity (obj. ID: ccb781af-a4eb-4ecc-b183-cef74b3cc717)
test "List all blobs in listblobs container" assert.Success "azmi listblobs --container $CONTAINER_LB"
EXPECTED_BLOB_COUNT=5
test "There should be $EXPECTED_BLOB_COUNT listed blobs in listblobs container" assert.Equals "azmi listblobs --container $CONTAINER_LB | wc -l" $EXPECTED_BLOB_COUNT
# listing with an optional --prefix
EXPECTED_BLOB_COUNT=3; PREFIX="neu-pre"
test "There should be $EXPECTED_BLOB_COUNT listed blobs with prefix '$PREFIX' in listblobs container" assert.Equals "azmi listblobs --container $CONTAINER_LB --prefix $PREFIX | wc -l" $EXPECTED_BLOB_COUNT
EXPECTED_BLOB_COUNT=1; PREFIX="neu-pre-show-me-only"
test "There should be $EXPECTED_BLOB_COUNT listed blob with prefix '$PREFIX' in listblobs container" assert.Equals "azmi listblobs --container $CONTAINER_LB --prefix $PREFIX | wc -l" $EXPECTED_BLOB_COUNT
EXPECTED_BLOB_COUNT=0; PREFIX="noBlobsShouldDownload"
test "There should be $EXPECTED_BLOB_COUNT listed blob with prefix '$PREFIX' in listblobs container" assert.Equals "azmi listblobs --container $CONTAINER_LB --prefix $PREFIX | wc -l" $EXPECTED_BLOB_COUNT

# getblobs subcommand
testing class "getblobs"
DOWNLOAD_DIR="./Download"; EXPECTED_BLOB_COUNT=5; EXPECTED_SUCCESSES=6 # last one is summary
rm -rf $DOWNLOAD_DIR
test "We should successfully download $EXPECTED_BLOB_COUNT blobs from listblobs container" assert.Equals "azmi getblobs --container $CONTAINER_LB --directory $DOWNLOAD_DIR | grep Success | wc -l" $EXPECTED_SUCCESSES

EXPECTED_BLOB_COUNT=3; PREFIX="neu-pre"; EXPECTED_SUCCESSES=4
rm -rf $DOWNLOAD_DIR
test "We should successfully download $EXPECTED_BLOB_COUNT blobs with prefix '$PREFIX' from listblobs container" assert.Equals "azmi getblobs --container $CONTAINER_LB --directory $DOWNLOAD_DIR --prefix $PREFIX | grep Success | wc -l" $EXPECTED_SUCCESSES

EXPECTED_BLOB_COUNT=0; PREFIX="noBlobsShouldDownload"; EXPECTED_ROWS=0
rm -rf $DOWNLOAD_DIR
test "We should successfully download $EXPECTED_BLOB_COUNT blobs with prefix '$PREFIX' from listblobs container" assert.Equals "azmi getblobs --container $CONTAINER_LB --directory $DOWNLOAD_DIR --prefix $PREFIX | wc -l" $EXPECTED_ROWS

# testing setblob-byblob 
testing class "setblob-byblob"
test "Upload tmp file by blob" assert.Success "azmi setblob -f /tmp/${UPLOADFILE} --blob ${CONTAINER_RW}/byblob/${UPLOADFILE}"


# --if-newer option
testing class "--if-newer option"
test "Should skip: Download blob and write to file only if difference has been spotted (--if-newer option)" assert.Equals \
  "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file /tmp/${UPLOADFILE} --if-newer" "Skipped. Blob is not newer than file."
sleep 1s
test "Alter blob's modification time" assert.Success "azmi setblob --file /tmp/${UPLOADFILE} --blob ${CONTAINER_RW}/${UPLOADFILE} --force"
test "Download blob and write to file only if difference has been spotted (--if-newer option)" assert.Equals \
  "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file /tmp/${UPLOADFILE} --if-newer" "Success"
TIMESTAMP=$(date "+%Y%m%d_%H%M%S") # e.g. 20200107_144102
test "Download blob and write to file which does not exist yet (--if-newer option)" assert.Equals \
  "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file /tmp/unique-file.${TIMESTAMP} --if-newer" "Success"

# uninstalling
testing class "package"
test "Uninstall packages" assert.Success "apt purge $PACKAGENAME -y"

test "Verify azmi binary does not exist anymore" assert.Fail "[ -f /usr/bin/azmi ]"

#
#  Clean up actions
#

rm "$UPLOADFILE"

#################################
# display some diagnostic data
################################
echo -e "\n=============="
echo "Test running at '$(hostname)' host"

testing end

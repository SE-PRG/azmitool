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


### no-access container ###
BLOB="restricted_access_blob.txt"
test "Fails to read from NA container" assert.Fail "azmi getblob --blob $CONTAINER_NA/$BLOB --file download.txt"
test "Fails to save to NA container" assert.Fail "azmi setblob --file $UPLOADFILE --container $CONTAINER_NA"

### read-only container ###
# Role(s):    Storage Blob Data Reader
# Profile(s): bt-seu-test-id (obj. ID: d1c05b65-ccf9-47bd-870d-4e44d209ee7a), kotipoiss-identity (obj. ID: ccb781af-a4eb-4ecc-b183-cef74b3cc717)
BLOB="read_only_blob.txt"
test "Read blob contents from RO container" assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB --file download.txt"
test "Fails to save to RO container" assert.Fail "azmi setblob --file download.txt --container $CONTAINER_RO"
# test --identity options
test "Read blob from RO container using right identity"                 assert.Success "azmi getblob --blob $CONTAINER_RO/$BLOB --file download.txt --identity $identity"
test "Fails to read blob from RO container using foreign identity"      assert.Fail    "azmi getblob --blob $CONTAINER_RO/$BLOB --file download.txt --identity $identity_foreign"
test "Fails to read blob from RO container using non-existing identity" assert.Fail    "azmi getblob --blob $CONTAINER_RO/$BLOB --file download.txt --identity non-existing"

### read-write container ###
# Role(s):    Storage Blob Data Contributor
# Profile(s): bt-seu-test-id (obj. ID: d1c05b65-ccf9-47bd-870d-4e44d209ee7a), kotipoiss-identity (obj. ID: ccb781af-a4eb-4ecc-b183-cef74b3cc717)
test "Save file contents   to RW container" assert.Success "azmi setblob --file $UPLOADFILE --container $CONTAINER_RW"
DOWNLOADED_BLOB="azmi_itest_downloaded.txt"
test "Read blob contents from RW container" assert.Success "azmi getblob --blob ${CONTAINER_RW}/${UPLOADFILE} --file $DOWNLOADED_BLOB"
test "Blobs have to have same contents" assert.Success "diff $UPLOADFILE $DOWNLOADED_BLOB"
UPLOADFILE_SHA256=$(sha256sum $UPLOADFILE | awk '{ print $1 }')
DOWNLOADED_BLOB_SHA256=$(sha256sum $DOWNLOADED_BLOB | awk '{ print $1 }')
test "Blobs have to have equal SHA256 checksums" assert.Success "[ $UPLOADFILE_SHA256 = $DOWNLOADED_BLOB_SHA256 ]"

# there should be no <noname> folder in Azure
testing class "noname"
test "Prepare tmp file" assert.Success "rm -f /tmp/${RANDOM_BLOB_TO_STORE} && echo sometext > /tmp/${RANDOM_BLOB_TO_STORE}"
test "Upload tmp file" assert.Success "azmi setblob -f /tmp/${RANDOM_BLOB_TO_STORE} --container ${CONTAINER_RW}"
test "There is no noname folder after upload" assert.Fail "azmi getblob -f /dev/null -b ${CONTAINER_RW}//tmp/${RANDOM_BLOB_TO_STORE}"

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
test "Upload tmp file by blob" assert.Success "azmi setblob -f /tmp/${RANDOM_BLOB_TO_STORE} --blob ${CONTAINER_RW}/byblob/${RANDOM_BLOB_TO_STORE}"

# --force option
testing class "--force option"
test "Fails to attempt to overwrite existing blob using --container option" assert.Fail "azmi setblob -f /tmp/${RANDOM_BLOB_TO_STORE} --container ${CONTAINER_RW}"
test "Fails to attempt to overwrite existing blob using --blob option" assert.Fail "azmi setblob -f /tmp/${RANDOM_BLOB_TO_STORE} --blob ${CONTAINER_RW}/byblob/${RANDOM_BLOB_TO_STORE}"
test "Overwrite existing blob using --container option" assert.Success "azmi setblob -f /tmp/${RANDOM_BLOB_TO_STORE} --container ${CONTAINER_RW} --force"
test "Overwrite existing blob using --blob option" assert.Success "azmi setblob -f /tmp/${RANDOM_BLOB_TO_STORE} --blob ${CONTAINER_RW}/byblob/${RANDOM_BLOB_TO_STORE} --force"

# --if-newer option
testing class "--if-newer option"
test "Should skip: Download blob and write to file only if difference has been spotted (--if-newer option)" assert.Equals \
  "azmi getblob --blob ${CONTAINER_RW}/${RANDOM_BLOB_TO_STORE} --file /tmp/${RANDOM_BLOB_TO_STORE} --if-newer" "Skipped. Blob is not newer than file."
sleep 1s
test "Alter blob's modification time" assert.Success "azmi setblob --file /tmp/${RANDOM_BLOB_TO_STORE} --blob ${CONTAINER_RW}/${RANDOM_BLOB_TO_STORE} --force"
test "Download blob and write to file only if difference has been spotted (--if-newer option)" assert.Equals \
  "azmi getblob --blob ${CONTAINER_RW}/${RANDOM_BLOB_TO_STORE} --file /tmp/${RANDOM_BLOB_TO_STORE} --if-newer" "Success"
TIMESTAMP=`date "+%Y%m%d_%H%M%S"` # e.g. 20200107_144102
test "Download blob and write to file which does not exist yet (--if-newer option)" assert.Equals \
  "azmi getblob --blob ${CONTAINER_RW}/${RANDOM_BLOB_TO_STORE} --file /tmp/unique-file.${TIMESTAMP} --if-newer" "Success"

# uninstalling
testing class "package"
test "Uninstall packages" assert.Success "apt purge $PACKAGENAME -y"

test "Verify azmi binary does not exist anymore" assert.Fail "[ -f /usr/bin/azmi ]"

#
#  Clean up actions
#

rm $UPLOADFILE

#################################
# display some diagnostic data
################################
echo -e "\n=============="
echo "Test running at '`hostname`' host"

testing end

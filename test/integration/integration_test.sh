#!/bin/bash

# AzMiTool Integration tests
# It requires Bash Testing Framework

# setup variables
export DEBIAN_FRONTEND=noninteractive
PACKAGENAME=azmi
PACKAGEFILE=/tmp/azmiX.deb

# define function(s)
function install_azmi() { # installs/upgrades package from file if exists; otherwise APT repository
  if $(dpkg-query --show $PACKAGENAME > /dev/null 2>&1); then
    EVENT="Upgrade"
  else
    EVENT="Install"
  fi

  if [ -f $PACKAGEFILE ]; then
      test "$EVENT $PACKAGENAME package from file" assert.Success "dpkg -i $PACKAGEFILE"
  else
      test "$EVENT $PACKAGENAME package from repository" assert.Success "apt --assume-yes install $PACKAGENAME"
  fi  
  dpkg-query --showformat='  - ${Package} ${Version} installed\n' --show $PACKAGENAME
}

# start testing
testing start "$PACKAGENAME"
testing class "package"
test "Install fake package should fail" assert.Fail "apt --assume-yes install somenonexistingpackage"

# dependencies installed?
test "Check all dependencies are installed" assert.Success "dpkg -s libstdc++6"

install_azmi
test "Verify azmi binary exists and is executable" assert.Success "[ -x /usr/bin/azmi ]"


testing class "help"
test "Should fail if no arguments are provided" assert.Fail "azmi"
test "Print help and return success status" assert.Success "azmi --help"

test "Print help for gettoken" assert.Success "azmi gettoken --help"
test "Fail gettoken with wrong args" assert.Fail "azmi gettoken blahblah"

test "Print help for getblob" assert.Success "azmi getblob --help"
test "Fail getblob with wrong args" assert.Fail "azmi getblob blahblah"

test "Print help for setblob" assert.Success "azmi setblob --help"
test "Fail setblob with wrong args" assert.Fail "azmi setblob blahblah"
# TODO Automate above using list of supported subcommands


testing class "application"

test "Authenticate to Azure using a managed identity and get access token" assert.Success "azmi gettoken"

# URL structure
# https://azmitest.blob.core.windows.net/azmi-itest-rw/azmi_itest_2020-02-03_15:53:38.txt
# https://azmitest.blob.core.windows.net/azmi-itest-rw//tmp/azmi_itest_20200206_091521.txt
#         storage account URL           |containerName|          blob name

### no-access container ###
CONTAINER_URL="https://azmitest.blob.core.windows.net/azmi-itest-no-access"
BLOB="restricted_access_blob.txt"
test "Should fail: Read blob contents from restricted Azure storage container" assert.Fail "azmi getblob --blob $CONTAINER_URL/$BLOB --file download.txt"
date > upload.txt # generate unique file contents
test "Should fail: Save file contents to restricted Azure storage container" assert.Fail "azmi setblob --file upload.txt --container $CONTAINER_URL"
rm upload.txt

### read-only container ###
# Role(s):    Storage Blob Data Reader
# Profile(s): bt-seu-test-id (obj. ID: d1c05b65-ccf9-47bd-870d-4e44d209ee7a), kotipoiss-identity (obj. ID: ccb781af-a4eb-4ecc-b183-cef74b3cc717)
CONTAINER_URL="https://azmitest.blob.core.windows.net/azmi-itest-r"
BLOB="read_only_blob.txt"
test "Read blob contents from read-only Azure storage container" assert.Success "azmi getblob --blob $CONTAINER_URL/$BLOB --file download.txt"
test "Should fail: Save file contents to read-only Azure storage container" assert.Fail "azmi setblob --file download.txt --container $CONTAINER_URL"
# test --identity options
test "Read blob contents from read-only Azure storage container using right identity"                     assert.Success "azmi getblob --blob $CONTAINER_URL/$BLOB --file download.txt --identity 354800af-354e-42e0-906b-5b96e02c4e1c"
test "Should fail: Read blob contents from read-only Azure storage container using foreign identity"      assert.Fail    "azmi getblob --blob $CONTAINER_URL/$BLOB --file download.txt --identity 017dc05c-4d12-4ac2-b5f8-5e239dc8bc54"
test "Should fail: Read blob contents from read-only Azure storage container using non-existing identity" assert.Fail    "azmi getblob --blob $CONTAINER_URL/$BLOB --file download.txt --identity non-existing"

### read-write container ###
# Role(s):    Storage Blob Data Contributor
# Profile(s): bt-seu-test-id (obj. ID: d1c05b65-ccf9-47bd-870d-4e44d209ee7a), kotipoiss-identity (obj. ID: ccb781af-a4eb-4ecc-b183-cef74b3cc717)
CONTAINER_URL="https://azmitest.blob.core.windows.net/azmi-itest-rw"
TIMESTAMP=`date "+%Y%m%d_%H%M%S"` # e.g. 20200107_144102
RANDOM_BLOB_TO_STORE="azmi_itest_${TIMESTAMP}.txt"
CHARS="012345689abcdefghiklmnopqrstuvwxyz"
test "Generate random blob (file) contents" assert.Success "for i in {1..32}; do echo -n \"\${CHARS:RANDOM%\${#CHARS}:1}\"; done > $RANDOM_BLOB_TO_STORE"
test "Save file contents   to read-write Azure storage container" assert.Success "azmi setblob --file $RANDOM_BLOB_TO_STORE --container $CONTAINER_URL"
DOWNLOADED_BLOB="azmi_itest_downloaded.txt"
test "Read blob contents from read-write Azure storage container" assert.Success "azmi getblob --blob ${CONTAINER_URL}/${RANDOM_BLOB_TO_STORE} --file $DOWNLOADED_BLOB"
test "Blobs have to have same contents" assert.Success "diff $RANDOM_BLOB_TO_STORE $DOWNLOADED_BLOB"
RANDOM_BLOB_TO_STORE_SHA256=$(sha256sum $RANDOM_BLOB_TO_STORE | awk '{ print $1 }')
DOWNLOADED_BLOB_SHA256=$(sha256sum $DOWNLOADED_BLOB | awk '{ print $1 }')
test "Blobs have to have equal SHA256 checksums" assert.Success "[ $RANDOM_BLOB_TO_STORE_SHA256 = $DOWNLOADED_BLOB_SHA256 ]"

# there should be no <noname> folder in Azure
testing class "noname"
test "Prepare tmp file" assert.Success "rm -f /tmp/${RANDOM_BLOB_TO_STORE} && echo sometext > /tmp/${RANDOM_BLOB_TO_STORE}"
test "Upload tmp file" assert.Success "azmi setblob -f /tmp/${RANDOM_BLOB_TO_STORE} --container ${CONTAINER_URL}"
test "There is no noname folder after upload" assert.Fail "azmi getblob -f /dev/null -b ${CONTAINER_URL}//tmp/${RANDOM_BLOB_TO_STORE}"

### list-blobs container
# Role(s):    Storage Blob Data Contributor
# Profile(s): bt-seu-test-id (obj. ID: d1c05b65-ccf9-47bd-870d-4e44d209ee7a), kotipoiss-identity (obj. ID: ccb781af-a4eb-4ecc-b183-cef74b3cc717)
CONTAINER_URL="https://azmitest.blob.core.windows.net/azmi-itest-listblobs"
test "List all blobs in listblobs container" assert.Success "azmi listblobs --container $CONTAINER_URL"
EXPECTED_BLOB_COUNT=5
test "There should be $EXPECTED_BLOB_COUNT listed blobs in listblobs container" assert.Equals "azmi listblobs --container $CONTAINER_URL | wc -l" $EXPECTED_BLOB_COUNT
# listing with an optional --prefix
EXPECTED_BLOB_COUNT=3; PREFIX="neu-pre"
test "There should be $EXPECTED_BLOB_COUNT listed blobs with prefix '$PREFIX' in listblobs container" assert.Equals "azmi listblobs --container $CONTAINER_URL --prefix $PREFIX | wc -l" $EXPECTED_BLOB_COUNT
EXPECTED_BLOB_COUNT=1; PREFIX="neu-pre-show-me-only"
test "There should be $EXPECTED_BLOB_COUNT listed blob with prefix '$PREFIX' in listblobs container" assert.Equals "azmi listblobs --container $CONTAINER_URL --prefix $PREFIX | wc -l" $EXPECTED_BLOB_COUNT

# it should support verbose option for commands
testing class "verbose"
test "gettoken verbose option" assert.Success "azmi gettoken --help | grep verbose"
test "getblob verbose option" assert.Success "azmi getblob --help | grep verbose"
test "setblob verbose option" assert.Success "azmi setblob --help | grep verbose"

# uninstalling
testing class "package"
test "Uninstall packages" assert.Success "apt purge $PACKAGENAME -y"

test "Verify azmi binary does not exist anymore" assert.Fail "[ -f /usr/bin/azmi ]"

#################################
# display some diagnostic data
################################
echo -e "\n=============="
echo "Test running at '`hostname`' host"

testing end

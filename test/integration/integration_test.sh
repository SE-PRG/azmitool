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
test "Should fail if no arguments are provided" asser.Fail "azmi"
test "Print help and return success status" assert.Success "azmi --help"
test "Print help for gettoken" assert.Success "azmi gettoken --help"
test "Fail gettoken with wrong args" assert.Fail "azmi gettoken blahblah"
test "Print help for setblob" assert.Success "azmi setblob --help"
test "Fail setblob with wrong args" assert.Fail "azmi setblob blahblah"
# TODO Automate above using list of supported subcommands


testing class "application"

### read-only container ###
# read access granted to 'kotipoiss'
CONTAINER_URL="https://azmitest.blob.core.windows.net/azmi-itest-r"
BLOB="read_only_blob.txt"
test "Read blob contents from read-only Azure storage container" assert.Success "azmi getblob --blob $CONTAINER_URL/$BLOB --file download.txt"
test "Save file contents   to read-only Azure storage container" assert.Fail "azmi setblob --file download.txt --container $CONTAINER_URL"

### write-only container ###
# write access granted to 'kotipoiss'
CONTAINER_URL="https://azmitest.blob.core.windows.net/azmi-itest-w"
BLOB="write_only_blob.txt"
test "Read blob contents from write-only Azure storage container" assert.Fail "azmi getblob --blob $CONTAINER_URL/$BLOB --file download.txt"
test "Save file contents   to write-only Azure storage container" assert.Success "azmi setblob --file download.txt --container $CONTAINER_URL"

### read-write container ###
# read-write access granted to 'kotipoiss'
CONTAINER_URL="https://azmitest.blob.core.windows.net/azmi-itest-rw"
TIMESTAMP=`date "+%Y-%m-%d_%H:%M:%S"` # e.g. 2020-01-07_14:41:02
RANDOM_BLOB_TO_STORE="azmi_itest_${TIMESTAMP}.txt"
CHARS='012345689abcdefghiklmnopqrstuvwxyz'
test "Generate random blob (file) contents" assert.Success "for i in {1..32}; do echo -n \"\${CHARS:RANDOM%\${#CHARS}:1}\"; done > $RANDOM_BLOB_TO_STORE"
test "Save file contents   to read-write Azure storage container" assert.Success "azmi setblob --file $RANDOM_BLOB_TO_STORE --container $CONTAINER_URL"
DOWNLOADED_BLOB="azmi_integration_test_downloaded.txt"
test "Read blob contents from write-only Azure storage container" assert.Success "azmi getblob --blob ${CONTAINER_URL}/${RANDOM_BLOB_TO_STORE} --file $DOWNLOADED_BLOB"
test "Blobs have to have same contents" assert.Success "diff $RANDOM_BLOB_TO_STORE $DOWNLOADED_BLOB"
RANDOM_BLOB_TO_STORE_SHA256=$(sha256sum $RANDOM_BLOB_TO_STORE | awk '{ print $1 }')
DOWNLOADED_BLOB_SHA256=$(sha256sum $DOWNLOADED_BLOB | awk '{ print $1 }')
test "Blobs have to have equal SHA256 checksums" assert.Success "[ $RANDOM_BLOB_TO_STORE_SHA256 = $DOWNLOADED_BLOB_SHA256 ]"

# uninstalling
testing class "package"
test "Uninstall packages" assert.Success "apt purge $PACKAGENAME -y"

test "Verify azmi binary does not exist anymore" assert.Fail "[ -f /usr/bin/azmi ]"

#################################
# display some diagnostic data
################################
echo -e "\n=============="
echo "Test running at '`hostname`' host"
echo "Available $PACKAGENAME packages:"
apt-cache madison $PACKAGENAME

testing end

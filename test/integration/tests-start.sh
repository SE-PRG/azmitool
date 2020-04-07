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

#!/bin/bash

# AzMiTool Integration tests
# It requires Bash Testing Framework
#
#   this file is part of set of files!
#


#
# setup variables
#

# this script is called from testing framework script, therefore arguments are shifted
STORAGEACCOUNTNAME=$2
identity=$3
KV_BASENAME=$4
# do repeat this variables definition later in the script for clarity


# fixed variables
declare -a subCommands=("gettoken" "getblob" "getblobs" "setblob" "listblobs" "getsecret" "getcertificate")
identity_foreign=d8e2f047-99b7-48e8-89d1-0e9b6e0b2464


#
# start testing
#

testing start "azmi"

testing class "help"
test "Fail if no arguments are provided" assert.Fail "azmi"
test "Print help and return success status" assert.Success "azmi --help"

for subCommand in "${subCommands[@]}"
do
  test "Print help for $subCommand" assert.Success "azmi $subCommand --help"
  test "$subCommand verbose option" assert.Success "azmi $subCommand --help | grep verbose"
  test "Fail $subCommand with wrong args" assert.Fail "azmi $subCommand blahblah"
done


#
# gettoken testing
#

testing class "gettoken"

test "gettoken basic" assert.Success "azmi gettoken"
test "gettoken with identity" assert.Success "azmi gettoken --identity $identity"
test "gettoken in JWT format" assert.Success "azmi gettoken --jwt-format | grep typ | grep JWT"
test "gettoken fails with wrong args" assert.Fail "azmi gettoken blahblah"

testing class "gettoken endpoints"
test "gettoken management endpoint" assert.Success "azmi gettoken --endpoint management"
test "gettoken storage endpoint" assert.Success "azmi gettoken --endpoint storage"
test "gettoken keyvault endpoint" assert.Success "azmi gettoken --endpoint keyvault"
test "gettoken unknown endpoint" assert.Fail "azmi gettoken --endpoint unknown"
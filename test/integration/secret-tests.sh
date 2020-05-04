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
KV_BASENAME=$4
KV_NA="https://${KV_BASENAME}-na.vault.azure.net"
KV_RO="https://${KV_BASENAME}-ro.vault.azure.net"
SECRET="secret1"

#
# secret subcommands testing
#

testing class "getsecret"
test "getsecret fails on NA KeyVault" assert.Fail "azmi getsecret --secret ${KV_NA}/secrets/${SECRET}"
test "getsecret OK on RO KV" assert.Equals "azmi getsecret --secret ${KV_RO}/secrets/${SECRET} --identity $identity" "version2"
test "getsecret OK on RO KV with relative path" assert.Success "azmi getsecret --secret ${KV_RO}/secrets/${SECRET} --file download.txt --identity $identity && grep LikeThat download.txt"
test "getsecret OK on RO KV with absolute path" assert.Success "azmi getsecret --secret ${KV_RO}/secrets/${SECRET} -f /var/tmp/download.txt --identity $identity && grep LikeThat /var/tmp/download.txt"
test "getsecret OK on RO specific version of secret" assert.Equals "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword/6f7c24526c4d489594ca27a85edf6176 --identity $identity" "LikeThatSpecifically"
test "getsecret fails on non-existing specific version of secret" assert.Fail "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword/xxxxxxxVersionDoesNotExistxxxxxx --identity $identity"

test "getsecret fails on missing secret" assert.Fail "azmi getsecret --secret ${KV_RO}/secrets/iDoNotExist --identity $identity"
test "getsecret fails on invalid URL #1" assert.Fail "azmi getsecret --secret ${KV_RO}"
test "getsecret fails on invalid URL #2" assert.Fail "azmi getsecret --secret ${KV_RO}/"
test "getsecret fails on invalid URL #3" assert.Fail "azmi getsecret --secret http://azmi-itest-r.vault.azure.net/secrets/ReadPassword"   # http protocol
test "getsecret fails on invalid URL #4" assert.Fail "azmi getsecret --secret https:\\\azmi-itest-r.vault.azure.net/secrets/ReadPassword" # backslashes
test "getsecret fails on invalid URL #5" assert.Fail "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword/6f7c24526c4d489594ca27a85edf6176/iAmTooLong" # too long URL

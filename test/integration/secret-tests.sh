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
KV_NA="https://${KV_BASENAME}-no-access.vault.azure.net"
KV_RO="https://${KV_BASENAME}-r.vault.azure.net"


#
# secret subcommands testing
#

testing class "getsecret"
test "getsecret fails on existing but foreign secret" assert.Fail "azmi getsecret --secret ${KV_NA}/secrets/buriedSecret"
test "getsecret OK on RO secret" assert.Equals "azmi getsecret --secret ${KV_RO}/secrets/readMyPassword --identity $identity" "LikeThat"

test "getsecret fails on missing secret" assert.Fail "azmi getsecret --secret ${KV_RO}/secrets/iDoNotExist --identity $identity"
test "getsecret fails on invalid URL #1" assert.Fail "azmi getsecret --secret ${KV_RO}"
test "getsecret fails on invalid URL #2" assert.Fail "azmi getsecret --secret ${KV_RO}/"
test "getsecret fails on invalid URL #3" assert.Fail "azmi getsecret --secret http://azmi-itest-r.vault.azure.net/secrets/readMyPassword"   # http protocol
test "getsecret fails on invalid URL #4" assert.Fail "azmi getsecret --secret https:\\\azmi-itest-r.vault.azure.net/secrets/readMyPassword" # backslashes

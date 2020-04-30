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
test "getsecret OK on RO latest secret" assert.Equals "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword --identity $identity" "LikeThat"
test "getsecret OK on RO latest secret save to file - relative path" assert.Success "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword --file download.txt --identity $identity && grep LikeThat download.txt"
test "getsecret the same but absolute path" assert.Success "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword -f /var/tmp/download.txt --identity $identity && grep LikeThat /var/tmp/download.txt"
test "getsecret OK on RO specific version of secret" assert.Equals "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword/6f7c24526c4d489594ca27a85edf6176 --identity $identity" "LikeThatSpecifically"
test "getsecret fails on non-existing specific version of secret" assert.Fail "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword/xxxxxxxVersionDoesNotExistxxxxxx --identity $identity"

test "getsecret fails on missing secret" assert.Fail "azmi getsecret --secret ${KV_RO}/secrets/iDoNotExist --identity $identity"
test "getsecret fails on invalid URL #1" assert.Fail "azmi getsecret --secret ${KV_RO}"
test "getsecret fails on invalid URL #2" assert.Fail "azmi getsecret --secret ${KV_RO}/"
test "getsecret fails on invalid URL #3" assert.Fail "azmi getsecret --secret http://azmi-itest-r.vault.azure.net/secrets/ReadPassword"   # http protocol
test "getsecret fails on invalid URL #4" assert.Fail "azmi getsecret --secret https:\\\azmi-itest-r.vault.azure.net/secrets/ReadPassword" # backslashes
test "getsecret fails on invalid URL #5" assert.Fail "azmi getsecret --secret ${KV_RO}/secrets/ReadPassword/6f7c24526c4d489594ca27a85edf6176/iAmTooLong" # too long URL

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
# certificate subcommands testing
#

test "getcertificate fails on existing but foreign certificate" assert.Fail "azmi getcertificate --certificate ${KV_NA}/certificates/buriedCertificate"
test "getcertificate OK on RO latest certificate" assert.Success "azmi getcertificate --certificate ${KV_RO}/certificates/readThisCertificate --identity $identity | grep MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCdJzK88AQzWXVO"
test "getcertificate OK on RO specific version of certificate" assert.Success "azmi getcertificate --certificate ${KV_RO}/certificates/readThisCertificate/103a7355c6094bc78307b2db7b85b3c2 --identity $identity | grep MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCvga/z9gy4RG0S"
test "getcertificate fails on non-existing specific version of certificate" assert.Fail "azmi getcertificate --certificate ${KV_RO}/certificates/readThisCertificate/xxxxxxxVersionDoesNotExistxxxxxx --identity $identity"

test "getcertificate fails on missing certificate" assert.Fail "azmi getcertificate --certificate ${KV_RO}/certificates/iDoNotExist --identity $identity"
test "getcertificate fails on invalid URL #1" assert.Fail "azmi getcertificate --certificate ${KV_RO}"
test "getcertificate fails on invalid URL #2" assert.Fail "azmi getcertificate --certificate ${KV_RO}/"
test "getcertificate fails on invalid URL #3" assert.Fail "azmi getcertificate --certificate http://azmi-itest-r.vault.azure.net/certificates/readThisCertificate"   # http protocol
test "getcertificate fails on invalid URL #4" assert.Fail "azmi getcertificate --certificate https:\\\azmi-itest-r.vault.azure.net/certificates/readThisCertificate" # backslashes
test "getcertificate fails on invalid URL #5" assert.Fail "azmi getcertificate --certificate ${KV_RO}/certificates/readThisCertificate/103a7355c6094bc78307b2db7b85b3c2/iAmTooLong" # too long URL
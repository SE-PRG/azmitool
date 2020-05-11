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

#
# Internal variables
#
PEMCERT="pem-cert"
PFXCERT="pfx-cert"


#
# certificate subcommands testing
#

testing class "getcertificate PEM"
test "getcertificate fails on NA KV and PEM cert" assert.Fail "azmi getcertificate --certificate ${KV_NA}/certificates/${PEMCERT}"
test "getcertificate OK on RO KV and PEM certificate" assert.Success "azmi getcertificate -c ${KV_RO}/certificates/${PEMCERT} --identity $identity"
test "getcertificate OK on RO KV and relative path" assert.Success "azmi getcertificate -c ${KV_RO}/certificates/${PEMCERT} --file download.txt --identity $identity"
test "getcertificate OK on RO KV and absolute path" assert.Success "azmi getcertificate -c ${KV_RO}/certificates/${PEMCERT} -f /var/tmp/download.txt --identity $identity"
test "getcertificate OK on RO KV and PEM cert version" assert.Success "azmi getcertificate -c ${KV_RO}/certificates/${PEMCERT}/103a7355c6094bc78307b2db7b85b3c2 --identity $identity"

testing class "getcertificate PFX"
test "getcertificate fails on NA KV and PFX cert" assert.Fail "azmi getcertificate -c ${KV_NA}/certificates/${PFXCERT}"
test "getcertificate OK on RO KV and PFX certificate" assert.Success "azmi getcertificate -c ${KV_RO}/certificates/${PFXCERT} --identity $identity"
test "getcertificate OK on RO KV and PFX cert version" assert.Success "azmi getcertificate -c ${KV_RO}/certificates/${PFXCERT}/035f5dc633a445bb801f760f46286785 --identity $identity"

test "getcertificate fails on non-existing specific version of certificate" assert.Fail "azmi getcertificate --certificate ${KV_RO}/certificates/readThisCertificate/xxxxxxxVersionDoesNotExistxxxxxx --identity $identity"
test "getcertificate fails on missing certificate" assert.Fail "azmi getcertificate --certificate ${KV_RO}/certificates/iDoNotExist --identity $identity"

testing class "getcertificate url"
test "getcertificate fails on invalid URL #1" assert.Fail "azmi getcertificate --certificate ${KV_RO}"
test "getcertificate fails on invalid URL #2" assert.Fail "azmi getcertificate --certificate ${KV_RO}/"
test "getcertificate fails on invalid URL #3" assert.Fail "azmi getcertificate --certificate http://azmi-itest-r.vault.azure.net/certificates/readThisCertificate"   # http protocol
test "getcertificate fails on invalid URL #4" assert.Fail "azmi getcertificate --certificate https:\\\azmi-itest-r.vault.azure.net/certificates/readThisCertificate" # backslashes
test "getcertificate fails on invalid URL #5" assert.Fail "azmi getcertificate --certificate ${KV_RO}/certificates/readThisCertificate/103a7355c6094bc78307b2db7b85b3c2/iAmTooLong" # too long URL
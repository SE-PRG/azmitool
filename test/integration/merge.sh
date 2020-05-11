#!/bin/bash

#
# this script is used to merge all test files into single file in proper order
#   it must be executed from the same folder where it is located
#   the same process is used during integration testing in ADO pipelines
#   generated file can be used for manual testing
#   example: ./btf.sh ./azmi.8RnvZ.sh $storage $identity $keyvault
#



# list of test files
files="
01-common.sh
blob-tests.sh
secret-tests.sh
certificate-tests.sh
99-tests-end.sh
"

# prepare test script
temp_file=$(mktemp /tmp/azmi.XXXXX.sh)

for script in $files
do
    echo "$script"
    cat "$script" >> "$temp_file"
done

echo "$temp_file"

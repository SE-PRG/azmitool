#!/bin/bash

#
# this script is used to in integration pipeline to execute tests using BTF.sh
#   BTF.sh stands for Bash testing Framework
#   it is also using merge.sh to create single testing file
#

dotnet publish ./src/azmi-commandline/azmi-commandline.csproj --configuration Release --self-contained true /p:PublishSingleFile=true --runtime linux-x64

exePath=$(cd ./src/azmi-commandline/bin/Release/netcoreapp3.0/linux-x64/publish || exit; pwd)
PATH="$exePath:$PATH"
export DOTNET_BUNDLE_EXTRACT_BASE_DIR="$HOME/cache_dotnet_bundle_extract"

printf  "\n=================\n"
echo "Integration testing azmi v.$(azmi --version)"

cd ./test/integration/ || exit
chmod +x  merge.sh
#./merge.sh
#testFile=$(ls /tmp/azmi* -1t | head -1)
testFile=$(./merge.sh)
ls -l $testFile

wget --quiet https://azmideb.blob.core.windows.net/azmi-deb/dev/btf.sh
chmod +x btf.sh

printf  "\n=================\n"
./btf.sh "$testFile" "$@"
#$(STORAGE_ACCOUNT_NAME) $(IDENTITY_CLIENT_ID) $(KEY_VAULTS_BASE_NAME) $(OLD_SECRET_ID) $(OLD_CERTIFICATE_ID)




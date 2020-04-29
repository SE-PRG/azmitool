#!/bin/bash

REPEAT=$1

echo 'azmi - build executable'
dotnet build src/azmi-commandline/azmi-commandline.csproj

exePath=$(cd ./src/azmi-commandline/bin/Debug/netcoreapp3.0 || exit; pwd)
PATH="$PATH:$exePath"


echo "azmi getblob - performance testing, repeat count: $(REPEAT)"
echo "VM size: $(curl -H Metadata:true "http://169.254.169.254/metadata/instance/compute/vmSize?api-version=2018-10-01&format=text")"


printf  "\n=================\n"
echo "azmi --help"
time seq "$REPEAT" | azmi --help > /dev/null


printf  "\n=================\n"
echo "azmi gettoken"
time seq "$REPEAT" | azmi gettoken > /dev/null

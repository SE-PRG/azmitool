#!/bin/bash

echo 'azmi - build executable'
dotnet build src/azmi-commandline/azmi-commandline.csproj

exePath=$(cd ./src/azmi-commandline/bin/Debug/netcoreapp3.0; pwd)
PATH="$PATH:$exePath"


echo "azmi getblob - performance testing, repeat count: $(REPEAT)"

printf  "\n=================\n"
echo "azmi --help"

time for i in {1..$(REPEAT)}; do azmi --help > /dev/null; done


printf  "\n=================\n"
echo "azmi gettoken"

time for i in {1..$(REPEAT)}; do azmi gettoken > /dev/null; done
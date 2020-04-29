#!/bin/bash

echo 'azmi - build executable'
dotnet build src/azmi-commandline/azmi-commandline.csproj

exePath=$(cd ./src/azmi-commandline/bin/Debug/netcoreapp3.0 || exit; pwd)
PATH="$PATH:$exePath"

echo "azmi getblobs - performance testing, repeat count: 1"
azmi --version
CONT="https://azmitest5.blob.core.windows.net/azmi-pt"



printf  "\n=================\n"
echo "azmi listblobs -c  $CONT"
time azmi listblobs -c  $CONT | wc -l



printf  "\n=================\n"
echo "rclone ls pt:azmi-pt"
time rclone ls pt:azmi-pt | wc -l



rm -rf download1
mkdir download1
printf  "\n=================\n"
echo "azmi getblobs -c $CONT -d download1"
time azmi getblobs -c $CONT -d download1 > /dev/null
echo "disk usage: $(du download1)"



rm -rf download2
mkdir download2
printf  "\n=================\n"
echo "rclone copy pt:azmi-pt ./download2"
time rclone copy pt:azmi-pt ./download2
echo "disk usage: $(du download2)"

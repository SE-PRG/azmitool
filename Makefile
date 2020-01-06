VERSION=`head -1 debian/changelog |sed 's/.*(\(.*\)).*/\1/'`

all: azmi

azmi:
	dotnet publish ./src/azmi-commandline/azmi-commandline.csproj --configuration Release --self-contained true /p:PublishSingleFile=true --runtime linux-x64

deb:
	dpkg-buildpackage --unsigned-source --unsigned-changes -rfakeroot --post-clean

.PHONY: azmi deb

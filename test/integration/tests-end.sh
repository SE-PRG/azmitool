# AzMiTool Integration tests
# It requires Bash Testing Framework
#
#   this file is part of set of files!
#


# uninstalling
testing class "package"
test "Uninstall packages" assert.Success "apt purge $PACKAGENAME -y"
test "Verify azmi binary does not exist anymore" assert.Fail "[ -f /usr/bin/azmi ]"


#################################
# display some diagnostic data
################################
echo -e "\n=============="
echo "Test running at '$(hostname)' host"

testing end

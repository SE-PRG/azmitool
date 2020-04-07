
# uninstalling
testing class "package"
test "Uninstall packages" assert.Success "apt purge $PACKAGENAME -y"
test "Verify azmi binary does not exist anymore" assert.Fail "[ -f /usr/bin/azmi ]"

#
#  Clean up actions
#

rm "$UPLOADFILE"
rm -rf $DOWNLOAD_DIR

#################################
# display some diagnostic data
################################
echo -e "\n=============="
echo "Test running at '$(hostname)' host"

testing end

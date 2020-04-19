function Test-AzmiStorageAccountName ($Name) {
    return Test-AzmiDNSName ($Name + '.blob.core.windows.net')
}
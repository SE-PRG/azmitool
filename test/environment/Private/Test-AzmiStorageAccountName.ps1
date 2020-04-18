function Test-AzmiStorageAccountName {
    return Test-AzmiDNSName ($Name + '.blob.core.windows.net')
}
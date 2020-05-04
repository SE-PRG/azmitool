function Test-AzmiKeyVaultBaseName ($Name) {
    if (((Test-AzmiDNSName ($Name + '-na.vault.azure.net')) -eq 'ok') -and
        ((Test-AzmiDNSName ($Name + '-ro.vault.azure.net')) -eq 'ok')) {
            return 'ok'
    } else {
        return 'not ok'
    }
}
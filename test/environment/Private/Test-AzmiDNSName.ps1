function Test-AzmiDNSName($Name) {
    try {
        if ([System.Net.Dns]::Resolve($Name)) {
            return 'not ok'
        } else {
            return 'ok'
        }
    } catch {
        return 'ok'
    }
}
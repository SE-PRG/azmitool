function Write-AzmiVerbose {

    param(
        [string]$Message,
        [string]$CommandName = $null,
        [string]$Format = $null
    )

    if (!$CommandName) {
        $CommandName = Get-Variable -Name CommandName -Scope 1 -ValueOnly -ea 0
    }

    if ($Format) {
        Write-Verbose "$(Get-Date -f $Format) $CommandName $Message"
    } else {
        Write-Verbose "$(Get-Date -f T)   $CommandName`: $Message"
    }
}

function Get-AzmiRandomDigits {

    param (
        [Parameter(Mandatory=$true,Position=0)]
        [int]$Count
    )

    $NumbersArray = 1..$count | % {
        0..9 | Get-Random
    }

    return ($NumbersArray -join '')

}

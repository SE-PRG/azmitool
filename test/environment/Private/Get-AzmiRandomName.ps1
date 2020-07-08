function Get-AzmiRandomName {

    param(
        [Parameter(Mandatory=$true,Position=0)]
        [string]$TestFunctionName
    )

    $CommandName = $MyInvocation.MyCommand.Name
    $RandomDigits = 1;
    $RandomAttempt =1
    $IsFree = $false

    $TestFunc = (Get-Item "function:$TestFunctionName").ScriptBlock
    if (!$TestFunc) {
        throw "Get-AzmiRandomName cannot find test function $TestFunctionName"
        # TODO: Add custom Azmi exception
    }

    do {
        $Name = 'azmitest' + (Get-AzmiRandomDigits $RandomDigits)
        Write-AzmiVerbose "Trying $Name"
        if ($TestFunc.Invoke($Name) -eq 'ok') {
            Write-AzmiVerbose "Free!"
            $IsFree = $true
        } else {
            Write-AzmiVerbose "Already existing"
            # we try once 1 digit number, twice 2 digit number, etc.
            if ($RandomAttempt -ge $RandomDigits) {
                $RandomAttempt = 1
                $RandomDigits++
            } else {
                $RandomAttempt++
            }
        }
    } until ($IsFree)
    return $Name
}

function Test-AzmiSubscription {

    param (
        [Parameter(Mandatory,Position=0,HelpMessage="Run Get-AzSubscription to see subscription IDs")]
        [ValidatePattern('[0-9a-f\-]{36}')]
        [string]$SubscriptionID
    )

    # initialize
    $CommandName = $MyInvocation.MyCommand.Name
    Write-AzmiVerbose "Starting" -Format G


    #
    # start of command
    #

    $Ctx = Get-AzContext -ea 0

    if (!$Ctx) {
        throw "Please login to required Azure (Login-AzAccount)"
    }

    if ($Ctx.Subscription.Id -ne $SubscriptionID) {
        Write-AzmiVerbose $CommandName "Trying to set subscription $SubscriptionID"
        $Ctx = Set-AzContext -SubscriptionId $SubscriptionID -ErrorAction Stop
    }

    Write-AzmiVerbose "Azure subscription found: $($Ctx.Subscription.Name)"

    # return value
    $Ctx

    # end of command
    Write-AzmiVerbose "Finished" -Format G
}


function Test-ServiceStandbyWait {
    param(
        [Parameter(Mandatory=$true)][string]$AtsRoot,
        [Parameter(Mandatory=$true)][string]$Service,
        [Parameter(Mandatory=$true)][datetime]$ProcessStarted
    )
    $console = Join-Path $AtsRoot "log/$Service.console.log"
    if (!(Test-Path -LiteralPath $console)) { return $false }
    # Startup archives the preceding process's console. Also verify the timestamp so copied
    # or stale logs cannot turn a hung startup into a successful standby result.
    $lines = @(Get-Content -LiteralPath $console -Encoding UTF8 -Tail 1000)
    $waiting = $false
    foreach ($line in $lines) {
        if ($line -match ('DISCOVERY service=' + [regex]::Escape($Service) + ' state=STANDBY_WAITING')) {
            if ($line -notmatch '^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})') { continue }
            $loggedAt = [datetime]::ParseExact($Matches[1], 'yyyy-MM-dd HH:mm:ss', [Globalization.CultureInfo]::InvariantCulture)
            $waiting = $loggedAt -ge $ProcessStarted.AddSeconds(-1)
        } elseif ($line -match 'DISCOVERY state=(REGISTERED|FAILED)' -or
                  $line -match '"component":"DISCOVERY","state":"(REGISTERED|FAILED)"' -or
                  $line -match '"component":"(FRAMEWORK|SERVICE_HOST)","state":"(STARTING|STARTED|FAILED)"') {
            $waiting = $false
        }
    }
    return $waiting
}

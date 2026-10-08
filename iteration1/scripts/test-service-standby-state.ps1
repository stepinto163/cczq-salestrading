$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'service-standby-state.ps1')
$fixture = Join-Path $PSScriptRoot ('../runtime/standby-control-checks/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path "$fixture/log" -Force | Out-Null
$started = [datetime]::Now.AddSeconds(-2)
$timestamp = [datetime]::Now.ToString('yyyy-MM-dd HH:mm:ss')
$waiting = "$timestamp,001 INFO ficc.startup - DISCOVERY service=QuoteSvr state=STANDBY_WAITING target=localhost"
function Assert-State([bool]$Expected, [string]$Scenario) {
    $actual = Test-ServiceStandbyWait -AtsRoot $fixture -Service QuoteSvr -ProcessStarted $started
    if ($actual -ne $Expected) { throw "FAIL $Scenario" }
    Write-Host "PASS $Scenario"
}
Assert-State $false 'missing log is not standby'
$waiting | Set-Content "$fixture/log/QuoteSvr.console.log" -Encoding UTF8
Assert-State $true 'fresh waiting standby has no listener requirement'
Assert-State $true 'status observation does not consume the waiting state'
Add-Content "$fixture/log/QuoteSvr.console.log" 'INFO DISCOVERY state=REGISTERED node=/MDTService/test/test'
Assert-State $false 'claim acquired means startup, not waiting'
$waiting | Set-Content "$fixture/log/QuoteSvr.console.log" -Encoding UTF8
Add-Content "$fixture/log/QuoteSvr.console.log" 'ERROR DISCOVERY state=FAILED serviceStopRequested=true'
Assert-State $false 'discovery failure cannot report healthy standby'
$waiting | Set-Content "$fixture/log/QuoteSvr.console.log" -Encoding UTF8
$started = [datetime]::Now.AddMinutes(1)
Assert-State $false 'previous process log cannot validate a new process'

$ErrorActionPreference = 'Stop'
$iterationRoot = Split-Path $PSScriptRoot -Parent
$wrapper = Join-Path $PSScriptRoot 'product.ps1'
$fixture = Join-Path $iterationRoot ('runtime/build-wrapper-checks/' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path "$fixture/scripts", "$fixture/stc/QuoteSvr/javalib", "$fixture/stc/QuoteSvr/control" | Out-Null
'Service,Enabled,Port', 'QuoteSvr,true,1' | Set-Content "$fixture/scripts/serverlist.csv"
'environment-settings-must-survive' | Set-Content "$fixture/stc/QuoteSvr/control/fixture.ini"
'original-library' | Set-Content "$fixture/stc/QuoteSvr/javalib/original.jar"
'original-application' | Set-Content "$fixture/stc/QuoteSvr/cgws-ficc-saletrading-quotesvr-old.jar"
$manager = @'
param([string]$Action, [string]$Service)
$ErrorActionPreference = 'Stop'
$fixtureRoot = Split-Path $PSScriptRoot -Parent
Add-Content "$fixtureRoot/calls.txt" "$Action $Service"
if ($Action -eq 'start' -and (Test-Path "$fixtureRoot/fail-once")) {
    Remove-Item -LiteralPath "$fixtureRoot/fail-once"
    exit 9
}
if ($Action -eq 'stop' -and (Test-Path "$fixtureRoot/fail-stop")) { exit 8 }
exit 0
'@
$manager | Set-Content "$fixture/scripts/service.ps1" -Encoding UTF8
function Assert([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw "FAIL $Message" }
    Write-Host "PASS $Message"
}
function Invoke-Deployment([string]$LogName) {
    $arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"' + $wrapper + '"'),
        '-Action', 'deploy', '-Target', 'quotesvr', '-NoBuild', '-AtsRoot', ('"' + $fixture + '"'))
    $process = Start-Process powershell.exe -ArgumentList $arguments -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput "$fixture/$LogName.log" -RedirectStandardError "$fixture/$LogName.stderr.log"
    $null = $process.Handle
    if (!$process.WaitForExit(30000)) {
        Stop-Process -InputObject $process -Force -ErrorAction SilentlyContinue
        throw 'Deployment fixture timed out; its wrapper process was stopped.'
    }
    $process.WaitForExit()
    return $process.ExitCode
}
'fail next start' | Set-Content "$fixture/fail-once"
Assert ((Invoke-Deployment 'rollback') -ne 0) 'failed startup returns a failing exit code'
Assert ((Get-Content "$fixture/stc/QuoteSvr/javalib/original.jar" -Raw).Trim() -eq 'original-library') 'previous libraries restored'
Assert ((Get-Content "$fixture/stc/QuoteSvr/cgws-ficc-saletrading-quotesvr-old.jar" -Raw).Trim() -eq 'original-application') 'previous application restored'
Assert (((Get-Content "$fixture/calls.txt") -join ',') -eq 'stop QuoteSvr,start QuoteSvr,stop QuoteSvr,start QuoteSvr') 'failed start triggers stop, restore and previous-version start'
'cannot stop' | Set-Content "$fixture/fail-stop"
Assert ((Invoke-Deployment 'stop-failure') -ne 0) 'stop failure returns a failing exit code'
Assert (Test-Path "$fixture/stc/QuoteSvr/javalib/original.jar") 'stop failure leaves installed libraries intact'
Remove-Item -LiteralPath "$fixture/fail-stop"
Assert ((Invoke-Deployment 'success') -eq 0) 'successful deployment completes'
Assert (!(Test-Path "$fixture/stc/QuoteSvr/javalib/original.jar")) 'stale libraries are not mixed with the new package'
Assert ((Get-Content "$fixture/stc/QuoteSvr/control/fixture.ini" -Raw).Trim() -eq 'environment-settings-must-survive') 'environment configuration survives deployment and recovery'
Assert (@(Get-ChildItem "$fixture/stc/QuoteSvr" -Filter '*.jar').Count -eq 1) 'only the new application JAR remains at the service root'
Write-Host "Fixture reports: $fixture (no real service was stopped by these checks)"

param([string]$ExecutablePath)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
if (!$ExecutablePath) {
    $ExecutablePath = Join-Path $taskRoot 'client/bin/Debug/net472/Ficc.SalesTradingClient.exe'
}
$exe = (Resolve-Path -LiteralPath $ExecutablePath).Path
$testArgs='--autotest-ui "'+$taskRoot+'/runtime/ui-smoke.json"'
$p=Start-Process -FilePath $exe -ArgumentList $testArgs -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/ui-smoke.log" -RedirectStandardError "$taskRoot/runtime/ui-smoke.err"
$null=$p.Handle
if(!$p.WaitForExit(30000)){Stop-Process -Id $p.Id;throw 'UI functional check stopped after 30 seconds'}
Get-Content "$taskRoot/runtime/ui-smoke.log" -Tail 12
Get-Content "$taskRoot/runtime/ui-smoke.err" -Tail 12
if($p.ExitCode -ne 0){throw "UI functional check exit $($p.ExitCode)"}

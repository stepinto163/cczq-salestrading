param([string]$ExecutablePath)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
if (!$ExecutablePath) {
    $ExecutablePath = Join-Path $taskRoot 'client/bin/Debug/net472/Ficc.SalesTradingClient.exe'
}
$exe = (Resolve-Path -LiteralPath $ExecutablePath).Path
$testArgs='--autotest "'+$taskRoot+'/runtime/csharp-gateway-smoke.json"'
$p=Start-Process -FilePath $exe -ArgumentList $testArgs -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/csharp-gateway-smoke.log" -RedirectStandardError "$taskRoot/runtime/csharp-gateway-smoke.err"
$null=$p.Handle
if(!$p.WaitForExit(30000)){Stop-Process -Id $p.Id;throw 'C# functional test stopped after 30 seconds'}
Get-Content "$taskRoot/runtime/csharp-gateway-smoke.log" -Tail 20
Get-Content "$taskRoot/runtime/csharp-gateway-smoke.err" -Tail 16
if($p.ExitCode -ne 0){throw "C# functional test exit $($p.ExitCode)"}

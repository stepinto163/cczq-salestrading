param([string]$ExecutablePath)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
if(!$ExecutablePath){$ExecutablePath=Join-Path $taskRoot 'client/bin/Debug/net472/Ficc.SalesTradingClient.exe'}
$exe=(Resolve-Path -LiteralPath $ExecutablePath).Path
$testArgs='--autotest-price "'+$taskRoot+'/runtime/price-multibond-smoke.json"'
$p=Start-Process -FilePath $exe -ArgumentList $testArgs -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/price-multibond-smoke.log" -RedirectStandardError "$taskRoot/runtime/price-multibond-smoke.err"
$null=$p.Handle
if(!$p.WaitForExit(30000)){Stop-Process -Id $p.Id;throw 'Multi-bond UI check stopped after 30 seconds'}
Get-Content "$taskRoot/runtime/price-multibond-smoke.log" -Encoding UTF8
Get-Content "$taskRoot/runtime/price-multibond-smoke.err" -Encoding UTF8
if($p.ExitCode -ne 0){throw "Multi-bond UI check exit $($p.ExitCode)"}

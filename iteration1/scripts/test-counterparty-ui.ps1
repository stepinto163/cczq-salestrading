param([string]$ExecutablePath)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
if(!$ExecutablePath){$ExecutablePath=Join-Path $taskRoot 'client/bin/Counterparty/Ficc.QuotePrototype.exe'}
$exe=(Resolve-Path -LiteralPath $ExecutablePath).Path
$testArgs='--autotest-counterparty "'+$taskRoot+'/runtime/counterparty-ui-smoke.json"'
$p=Start-Process -FilePath $exe -ArgumentList $testArgs -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/counterparty-ui-smoke.log" -RedirectStandardError "$taskRoot/runtime/counterparty-ui-smoke.err"
$null=$p.Handle
if(!$p.WaitForExit(30000)){Stop-Process -Id $p.Id;throw 'Counterparty UI check stopped after 30 seconds'}
Get-Content "$taskRoot/runtime/counterparty-ui-smoke.log" -Encoding UTF8
Get-Content "$taskRoot/runtime/counterparty-ui-smoke.err" -Encoding UTF8
if($p.ExitCode -ne 0){throw "Counterparty UI check exit $($p.ExitCode)"}

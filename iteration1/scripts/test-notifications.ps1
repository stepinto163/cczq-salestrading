$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$exe=Join-Path $taskRoot 'client/bin/Debug/net472/Ficc.SalesTradingClient.exe'
$arguments='--autotest-notifications "'+$taskRoot+'/runtime/notification-smoke.json"'
$p=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/notification-smoke.log" -RedirectStandardError "$taskRoot/runtime/notification-smoke.err"
$null=$p.Handle
if(!$p.WaitForExit(30000)){Stop-Process -Id $p.Id;throw 'Reminder test stopped after 30 seconds'}
Get-Content "$taskRoot/runtime/notification-smoke.log" -Tail 24
Get-Content "$taskRoot/runtime/notification-smoke.err" -Tail 12
if($p.ExitCode -ne 0){throw "Reminder test failed: $($p.ExitCode)"}

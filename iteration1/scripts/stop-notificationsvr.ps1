$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$pidFile=Join-Path $taskRoot 'runtime/notificationsvr.pid'
if(!(Test-Path $pidFile)){Write-Output 'No recorded NotificationSvr process.';exit 0}
$targetId=[int](Get-Content $pidFile)
$process=Get-CimInstance Win32_Process -Filter "ProcessId=$targetId"
if($process){if($process.Name -ne 'java.exe' -or $process.CommandLine -notmatch 'ficc.notification.NotificationMain' -or !$process.CommandLine.Contains($taskRoot)){throw 'PID does not belong to this project NotificationSvr.'};Stop-Process -Id $targetId}
Remove-Item -LiteralPath $pidFile
Write-Output 'Project NotificationSvr stopped.'

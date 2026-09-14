$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$pidFile=Join-Path $taskRoot 'runtime/quotesvr.pid'
if(!(Test-Path $pidFile)){Write-Output 'No QuoteSvr PID recorded.';exit 0}
$recordedId=[int](Get-Content $pidFile)
$process=Get-CimInstance Win32_Process -Filter "ProcessId=$recordedId"
if($process){
    if($process.Name -ne 'java.exe' -or $process.CommandLine -notlike '*ficc.quote.Main*' -or !$process.CommandLine.Contains($taskRoot)){throw 'PID does not identify this project QuoteSvr.'}
    Stop-Process -Id $recordedId
}
Remove-Item -LiteralPath $pidFile
Write-Output 'Project QuoteSvr stopped.'

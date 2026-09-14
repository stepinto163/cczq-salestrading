$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$serverPath = Join-Path $taskRoot 'web-client/server.mjs'
$pidFile = Join-Path $taskRoot 'runtime/web-prototype.pid'
if (-not (Test-Path -LiteralPath $pidFile)) {
    Write-Output 'No web prototype PID recorded.'
    exit 0
}
$recordedProcessId = [int](Get-Content -LiteralPath $pidFile)
$recordedProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$recordedProcessId"
if ($recordedProcess) {
    if ($recordedProcess.Name -ne 'node.exe' -or -not $recordedProcess.CommandLine.Contains($serverPath)) {
        throw 'Recorded PID is not this web prototype; no process was stopped.'
    }
    Stop-Process -Id $recordedProcessId
}
Remove-Item -LiteralPath $pidFile
Write-Output 'Web prototype stopped.'

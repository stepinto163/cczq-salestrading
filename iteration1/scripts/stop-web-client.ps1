$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$serverPath = Join-Path $taskRoot 'web-client/server.mjs'
$pidFile = Join-Path $taskRoot 'runtime/web-client.pid'
if (-not (Test-Path -LiteralPath $pidFile)) {
    $ownedProcesses = @(Get-CimInstance Win32_Process -Filter "Name='node.exe'" |
        Where-Object { $_.CommandLine -and $_.CommandLine.Contains($serverPath) })
    if ($ownedProcesses.Count -gt 1) { throw 'Multiple project web servers found; inspect before stopping.' }
    if ($ownedProcesses.Count -eq 0) {
        Write-Output 'No project web client process found.'
        exit 0
    }
    $ownedProcesses[0].ProcessId | Set-Content -LiteralPath $pidFile
}
$recordedProcessId = [int](Get-Content -LiteralPath $pidFile)
$recordedProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$recordedProcessId"
if ($recordedProcess) {
    if ($recordedProcess.Name -ne 'node.exe' -or -not $recordedProcess.CommandLine.Contains($serverPath)) {
        throw 'Recorded PID is not this web client; no process was stopped.'
    }
    Stop-Process -Id $recordedProcessId
}
Remove-Item -LiteralPath $pidFile
Write-Output 'Web client stopped.'

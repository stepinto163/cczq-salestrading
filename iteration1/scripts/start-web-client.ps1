$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$serverPath = Join-Path $taskRoot 'web-client/server.mjs'
$pidFile = Join-Path $taskRoot 'runtime/web-client.pid'
if (-not (Test-Path -LiteralPath $pidFile)) {
    $ownedProcesses = @(Get-CimInstance Win32_Process -Filter "Name='node.exe'" |
        Where-Object { $_.CommandLine -and $_.CommandLine.Contains($serverPath) })
    if ($ownedProcesses.Count -gt 1) { throw 'Multiple project web servers found; inspect before starting another.' }
    if ($ownedProcesses.Count -eq 1) {
        $ownedProcesses[0].ProcessId | Set-Content -LiteralPath $pidFile
    }
}
if (Test-Path -LiteralPath $pidFile) {
    $priorProcessId = [int](Get-Content -LiteralPath $pidFile)
    $priorProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$priorProcessId"
    if ($priorProcess -and $priorProcess.CommandLine.Contains($serverPath)) {
        Write-Output 'Web client is already running: http://127.0.0.1:8088'
        exit 0
    }
    if ($priorProcess) { throw 'Recorded PID belongs to another process; check runtime/web-client.pid.' }
}
$nodePath = (Get-Command node).Source
if (Get-NetTCPConnection -LocalPort 8088 -State Listen -ErrorAction SilentlyContinue) {
    throw 'Port 8088 is already in use; no additional web server was started.'
}
$process = Start-Process -FilePath $nodePath -ArgumentList "--max-old-space-size=128 `"$serverPath`"" -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/web-client.log" -RedirectStandardError "$taskRoot/runtime/web-client.err"
$process.Id | Set-Content -LiteralPath $pidFile
Write-Output "Web client started (PID $($process.Id)): http://127.0.0.1:8088"

$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$serverPath = Join-Path $taskRoot 'web-client/server.mjs'
$pidFile = Join-Path $taskRoot 'runtime/web-prototype.pid'
if (Test-Path -LiteralPath $pidFile) {
    $priorProcessId = [int](Get-Content -LiteralPath $pidFile)
    $priorProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$priorProcessId"
    if ($priorProcess -and $priorProcess.CommandLine.Contains($serverPath)) {
        Write-Output 'Web prototype is already running: http://127.0.0.1:8088'
        exit 0
    }
    if ($priorProcess) { throw 'Recorded PID belongs to another process; check runtime/web-prototype.pid.' }
}
$nodePath = (Get-Command node).Source
$process = Start-Process -FilePath $nodePath -ArgumentList "--max-old-space-size=128 `"$serverPath`"" -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/web-prototype.log" -RedirectStandardError "$taskRoot/runtime/web-prototype.err"
$process.Id | Set-Content -LiteralPath $pidFile
Write-Output "Web prototype started (PID $($process.Id)): http://127.0.0.1:8088"

$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$pidFile = Join-Path $taskRoot 'runtime/zookeeper.pid'
if (!(Test-Path -LiteralPath $pidFile)) {
    Write-Output 'No project ZooKeeper PID recorded.'
    exit 0
}
$recordedId = [int](Get-Content -LiteralPath $pidFile)
$process = Get-CimInstance Win32_Process -Filter "ProcessId=$recordedId"
if ($process) {
    if ($process.Name -ne 'java.exe' -or $process.CommandLine -notlike '*org.apache.zookeeper.server.quorum.QuorumPeerMain*' -or !$process.CommandLine.Contains($taskRoot)) {
        throw 'Recorded PID does not identify this project ZooKeeper.'
    }
    Stop-Process -Id $recordedId
}
Remove-Item -LiteralPath $pidFile
Write-Output 'Project ZooKeeper stopped. Registry data is preserved.'

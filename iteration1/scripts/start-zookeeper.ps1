$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$installation = Join-Path $taskRoot 'runtime/tools/apache-zookeeper-3.8.6-bin'
$stateDirectory = Join-Path $taskRoot 'runtime/zookeeper'
$pidFile = Join-Path $taskRoot 'runtime/zookeeper.pid'
$port = 12181

if (!(Test-Path -LiteralPath "$installation/lib/zookeeper-3.8.6.jar")) {
    throw 'Run setup-zookeeper.ps1 first.'
}
if (Test-Path -LiteralPath $pidFile) {
    $priorId = [int](Get-Content -LiteralPath $pidFile)
    if (Get-Process -Id $priorId -ErrorAction SilentlyContinue) {
        throw 'Recorded ZooKeeper process is still running.'
    }
}
if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) {
    throw "Port $port is already in use. No existing service was changed."
}

New-Item -ItemType Directory -Force -Path "$stateDirectory/data" | Out-Null
$dataDirectory = (Join-Path $stateDirectory 'data').Replace('\', '/')
$config = Join-Path $stateDirectory 'zoo.cfg'
@"
tickTime=2000
dataDir=$dataDirectory
clientPort=$port
clientPortAddress=127.0.0.1
maxClientCnxns=30
admin.enableServer=false
4lw.commands.whitelist=ruok,stat
autopurge.snapRetainCount=3
autopurge.purgeInterval=1
"@ | Set-Content -LiteralPath $config -Encoding ASCII

$arguments = '-Xms32m -Xmx128m -XX:ActiveProcessorCount=2 -Dzookeeper.nio.numSelectorThreads=1 -Dzookeeper.nio.numWorkerThreads=2 -Dzookeeper.preAllocSize=1024 -Dfile.encoding=UTF-8 -cp "' + $installation + '/conf;' + $installation + '/lib/*" org.apache.zookeeper.server.quorum.QuorumPeerMain "' + $config + '"'
$process = Start-Process java -ArgumentList $arguments -WorkingDirectory $stateDirectory -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/zookeeper.log" -RedirectStandardError "$taskRoot/runtime/zookeeper.err"
$process.Id | Set-Content -LiteralPath $pidFile
$deadline = [DateTime]::UtcNow.AddSeconds(15)
while ([DateTime]::UtcNow -lt $deadline) {
    $process.Refresh()
    if ($process.HasExited) {
        throw 'ZooKeeper exited during startup. See runtime/zookeeper.err.'
    }
    $connection = [Net.Sockets.TcpClient]::new()
    try {
        $connection.Connect('127.0.0.1', $port)
        $stream = $connection.GetStream()
        $stream.ReadTimeout = 1000
        $command = [Text.Encoding]::ASCII.GetBytes('ruok')
        $stream.Write($command, 0, $command.Length)
        $buffer = New-Object byte[] 4
        $length = $stream.Read($buffer, 0, $buffer.Length)
        if ([Text.Encoding]::ASCII.GetString($buffer, 0, $length) -eq 'imok') {
            Write-Output "ZooKeeper READY at 127.0.0.1:$port; PID $($process.Id); maximum Java heap 128 MB."
            exit 0
        }
    } catch {
        Start-Sleep -Milliseconds 200
    } finally {
        $connection.Dispose()
    }
}
Stop-Process -Id $process.Id
throw 'ZooKeeper startup exceeded 15 seconds and was stopped.'

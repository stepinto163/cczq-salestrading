$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$gatewayPath = Join-Path $taskRoot 'GW'
$runtimePath = Join-Path $taskRoot 'runtime'
$pidFile = Join-Path $runtimePath 'gateway.pid'

if (Test-Path -LiteralPath $pidFile) {
    $priorId = [int](Get-Content -LiteralPath $pidFile)
    if (Get-Process -Id $priorId -ErrorAction SilentlyContinue) {
        throw 'Recorded GW process is still running. Run stop-gateway.ps1 first.'
    }
}
foreach ($requiredPath in @('javalib/gw-0.1.1-SNAPSHOT.jar', 'config/spring-config.xml', 'control/ATSConfig.ini', 'control/server.lic')) {
    if (!(Test-Path -LiteralPath (Join-Path $gatewayPath $requiredPath))) {
        throw "Standard GW deployment is missing $requiredPath under $gatewayPath."
    }
}
$arguments = '-Xms32m -Xmx192m -XX:ActiveProcessorCount=2 -Dfile.encoding=UTF-8 -cp "' + $gatewayPath + '/javalib/*" GW'
$previousAtsRoot = $env:ATS_ROOT
try {
    $env:ATS_ROOT = $gatewayPath
    $process = Start-Process java -ArgumentList $arguments -WorkingDirectory $gatewayPath -WindowStyle Hidden -PassThru -RedirectStandardOutput "$runtimePath/gateway.log" -RedirectStandardError "$runtimePath/gateway.err"
    $process.Id | Set-Content -LiteralPath $pidFile
    Write-Output "Standard GW startup requested; PID $($process.Id). Check runtime/gateway.log and GW/logs for service connections."
} finally {
    $env:ATS_ROOT = $previousAtsRoot
}
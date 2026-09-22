param([string]$Name = 'service-discovery')
$ErrorActionPreference = 'Stop'
if ($Name -notmatch '^[a-z0-9-]+$') {
    throw 'Report name may contain lowercase letters, numbers and hyphens only.'
}
$taskRoot = Split-Path $PSScriptRoot -Parent
$classpath = & (Join-Path $PSScriptRoot 'foundation-classpath.ps1')
$probe = Join-Path $taskRoot 'cgws-ficc-saletrading-common/src/test/java/RegistryProbe.java'
$registryReport = Join-Path $taskRoot "runtime/$Name-registry.json"
$arguments = '-Xmx96m -XX:ActiveProcessorCount=2 -cp "' + $classpath + '" "' + $probe + '" 127.0.0.1:12181 "' + $registryReport + '"'
$process = Start-Process java -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/$Name-registry.log" -RedirectStandardError "$taskRoot/runtime/$Name-registry.err"
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id
    throw 'Registry read-only probe exceeded 15 seconds.'
}
if ($process.ExitCode -ne 0) {
    throw "Registry probe failed. See runtime/$Name-registry.err."
}

$executable = Join-Path $taskRoot 'client/bin/Discovery/Ficc.SalesTradingClient.exe'
$report = Join-Path $taskRoot "runtime/$Name-client.json"
$process = Start-Process $executable -ArgumentList ('--probe-services "' + $report + '"') -WorkingDirectory (Split-Path $executable) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/$Name-client.log" -RedirectStandardError "$taskRoot/runtime/$Name-client.err"
$null = $process.Handle
if (!$process.WaitForExit(20000)) {
    Stop-Process -Id $process.Id
    throw 'C# read-only service probe exceeded 20 seconds.'
}
Get-Content -LiteralPath $registryReport
Get-Content -LiteralPath $report
if ($process.ExitCode -ne 0) {
    throw 'C# service probe failed.'
}

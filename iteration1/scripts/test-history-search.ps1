param([string]$Configuration = '')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
if (!$Configuration) { $Configuration = Join-Path $taskRoot 'runtime/quotesvr.local.properties' }
$Configuration = (Resolve-Path -LiteralPath $Configuration).Path
$classpath = & (Join-Path $PSScriptRoot 'server-classpath.ps1') -Server quotesvr
$testClasses = Join-Path $taskRoot 'cgws-ficc-saletrading-quotesvr/target/test-classes'
$java = Join-Path $taskRoot 'runtime/tools/jdk8/jdk8u504-b01/bin/java.exe'
if (!(Test-Path -LiteralPath $java)) { $java = (Get-Command java).Source }
$output = Join-Path $taskRoot ('runtime/history-search-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output | Out-Null
$previousAtsRoot = $env:ATS_ROOT
try {
    $env:ATS_ROOT = Join-Path $taskRoot 'cgws-ficc-saletrading-quotesvr'
    $arguments = '-Xms32m -Xmx128m -XX:ActiveProcessorCount=2 -Dfile.encoding=UTF-8 -cp "' +
        $testClasses + ';' + $classpath + '" ficc.quote.HistorySearchSmoke "' + $Configuration + '"'
    $process = Start-Process -FilePath $java -ArgumentList $arguments -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput "$output/stdout.log" -RedirectStandardError "$output/stderr.log"
    $null = $process.Handle
    if (!$process.WaitForExit(30000)) {
        Stop-Process -Id $process.Id
        throw "History functional test exceeded 30 seconds. Logs: $output"
    }
    Get-Content "$output/stdout.log" -Tail 30
    Get-Content "$output/stderr.log" -Tail 15
    if ($process.ExitCode -ne 0) { throw "History functional test failed. Logs: $output" }
    Write-Output "History functional test passed. Logs: $output"
} finally {
    $env:ATS_ROOT = $previousAtsRoot
}

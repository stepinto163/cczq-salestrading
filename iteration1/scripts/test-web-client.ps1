$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$testPath = Join-Path $taskRoot 'web-client/tests/integration.mjs'
$nodePath = (Get-Command node).Source
$process = Start-Process -FilePath $nodePath -ArgumentList "--max-old-space-size=128 `"$testPath`"" -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/web-functional.log" -RedirectStandardError "$taskRoot/runtime/web-functional.err"
$processHandle = $process.Handle
# External watchdog: two clients, two quotes, no load generation.
if (-not $process.WaitForExit(75000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Web functional test stopped by the 75-second watchdog. Inspect logs and created quote IDs before retrying.'
}
Get-Content -LiteralPath "$taskRoot/runtime/web-functional.log" -Encoding UTF8
if ($process.ExitCode -ne 0) {
    Get-Content -LiteralPath "$taskRoot/runtime/web-functional.err" -Encoding UTF8
    throw "Web functional test failed (exit $($process.ExitCode))."
}

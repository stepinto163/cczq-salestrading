$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$testPath = Join-Path $taskRoot 'web-client/tests/requirements-dom.mjs'
$nodePath = (Get-Command node).Source
$process = Start-Process -FilePath $nodePath -ArgumentList "--max-old-space-size=160 `"$testPath`"" -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/requirements-dom.log" -RedirectStandardError "$taskRoot/runtime/requirements-dom.err"
$processHandle = $process.Handle
if (-not $process.WaitForExit(35000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Original page DOM test stopped by the 35-second watchdog. Inspect the report before retrying.'
}
Get-Content -LiteralPath "$taskRoot/runtime/requirements-dom.log" -Encoding UTF8
if ($process.ExitCode -ne 0) {
    Get-Content -LiteralPath "$taskRoot/runtime/requirements-dom.err" -Encoding UTF8
    throw 'Original page DOM test failed.'
}

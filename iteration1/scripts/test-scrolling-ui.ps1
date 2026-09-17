param([string]$ExecutablePath)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$executable = if ($ExecutablePath) { (Resolve-Path -LiteralPath $ExecutablePath).Path } else { Join-Path $taskRoot 'client/bin/Debug/net472/Ficc.SalesTradingClient.exe' }
$resultFile = Join-Path $taskRoot 'runtime/scrolling-smoke.json'
$process = Start-Process -FilePath $executable -ArgumentList "--autotest-scroll `"$resultFile`"" -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/scrolling-smoke.log" -RedirectStandardError "$taskRoot/runtime/scrolling-smoke.err"
$processHandle = $process.Handle
if (-not $process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Scrolling functional test stopped after 15 seconds.'
}
if ($process.ExitCode -ne 0) {
    Get-Content "$taskRoot/runtime/scrolling-smoke.err"
    throw 'Scrolling functional test failed.'
}
Get-Content -LiteralPath $resultFile -Encoding UTF8

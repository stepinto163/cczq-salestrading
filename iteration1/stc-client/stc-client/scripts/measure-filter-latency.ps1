param(
    [Parameter(Mandatory=$true)][string]$StcConfig,
    [Parameter(Mandatory=$true)][string]$ReportDirectory,
    [switch]$Profile
)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $clientRoot 'runtime/ui-test-client'
$runner = Join-Path $runtime 'SalesTradingUiTests.exe'
if (!$env:STC_UI_USER -or !$env:STC_UI_PASSWORD) { throw 'Set STC_UI_USER and STC_UI_PASSWORD.' }
if (Get-Process SalesTradingUiTests,UiScenarioWorker -ErrorAction SilentlyContinue) { throw 'Another UI test is already running.' }
$ReportDirectory = [IO.Path]::GetFullPath($ReportDirectory)
New-Item -ItemType Directory -Force -Path $ReportDirectory | Out-Null
$arguments = @('--cases', ('"' + (Join-Path $clientRoot 'tests/ui/benchmarks') + '"'),
    '--run', 'filter-latency-500', '--config', ('"' + $StcConfig + '"'), '--report', ('"' + $ReportDirectory + '"'))
$previousProfile = $env:STC_UI_PROFILE
$env:STC_UI_PROFILE = if ($Profile) { '1' } else { '0' }
try {
    $process = Start-Process -FilePath $runner -WorkingDirectory $runtime -ArgumentList $arguments -PassThru -WindowStyle Hidden
} finally {
    $env:STC_UI_PROFILE = $previousProfile
}
$handle = $process.Handle
$deadline = [DateTime]::UtcNow.AddSeconds(180)
Write-Host "Started bounded test: runner PID $($process.Id); report $ReportDirectory"
while (!$process.WaitForExit(1000)) {
    $workers = @(Get-CimInstance Win32_Process -Filter "Name='UiScenarioWorker.exe'" |
        Where-Object { $_.ParentProcessId -eq $process.Id })
    $overMemory = @($workers | Where-Object { [long]$_.WorkingSetSize -gt 768MB }).Count -gt 0
    if ([DateTime]::UtcNow -gt $deadline -or $overMemory) {
        $reason = if ($overMemory) { 'Worker exceeded 768 MB' } else { '180 second watchdog expired' }
        foreach ($worker in $workers) { Stop-Process -Id $worker.ProcessId -Force -ErrorAction SilentlyContinue }
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        $reason | Set-Content (Join-Path $ReportDirectory 'watchdog.txt')
        throw "$reason; inspect owned-quotes.json for unfinished cleanup. No other STC was stopped."
    }
}
$process.WaitForExit()
$process.Refresh()
if (Test-Path (Join-Path $ReportDirectory 'summary.json')) {
    $results = Get-Content (Join-Path $ReportDirectory 'summary.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($result in $results) {
        Write-Host "$($result.caseId): $($result.status); cleanup=$($result.cleanupPassed)"
    }
}
exit $process.ExitCode

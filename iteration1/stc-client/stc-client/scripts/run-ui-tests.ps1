param(
    [string]$Cases = 'all',
    [string]$StcConfig,
    [ValidateRange(0, 60000)][int]$OperationIntervalMilliseconds = 2000,
    [string]$ReportDirectory
)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $clientRoot 'runtime/ui-test-client'
if (!$StcConfig) { $StcConfig = Join-Path $runtime 'STC.exe.config' }
if (!$ReportDirectory) { $ReportDirectory = Join-Path $runtime ('ui-test-reports/' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff')) }
$arguments = @('--cases', ('"' + (Join-Path $clientRoot 'tests/ui/cases') + '"'), '--run', $Cases,
    '--interval-ms', $OperationIntervalMilliseconds.ToString(), '--config', ('"' + $StcConfig + '"'), '--report', ('"' + $ReportDirectory + '"'))
$process = Start-Process -FilePath (Join-Path $runtime 'SalesTradingUiTests.exe') -WorkingDirectory $runtime -ArgumentList $arguments -PassThru -WindowStyle Hidden
Write-Host "Report: $ReportDirectory"
$null = $process.Handle
while (!$process.WaitForExit(1000)) { }
$process.WaitForExit()
if (Test-Path (Join-Path $ReportDirectory 'summary.json')) {
    Get-Content (Join-Path $ReportDirectory 'summary.json') -Raw -Encoding UTF8 | ConvertFrom-Json | Select-Object caseId, status, cleanupPassed
}
exit $process.ExitCode

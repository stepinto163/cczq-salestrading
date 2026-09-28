param(
    [string]$Cases = 'all',
    [string]$StcConfig,
    [string]$ReportDirectory
)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $clientRoot 'runtime/ui-test-client'
if (!$StcConfig) { $StcConfig = Join-Path $runtime 'STC.exe.config' }
if (!$ReportDirectory) { $ReportDirectory = Join-Path $runtime ('ui-test-reports/' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff')) }
if (!$env:STC_UI_USER -or !$env:STC_UI_PASSWORD) { throw 'Set STC_UI_USER and STC_UI_PASSWORD to the dedicated test account.' }
$arguments = @('--cases', ('"' + (Join-Path $clientRoot 'tests/ui/cases') + '"'), '--run', $Cases,
    '--config', ('"' + $StcConfig + '"'), '--report', ('"' + $ReportDirectory + '"'))
$process = Start-Process -FilePath (Join-Path $runtime 'SalesTradingUiTests.exe') -WorkingDirectory $runtime -ArgumentList $arguments -PassThru -WindowStyle Hidden
Write-Host "Report: $ReportDirectory"
$null = $process.Handle
while (!$process.WaitForExit(1000)) { }
$process.WaitForExit()
if (Test-Path (Join-Path $ReportDirectory 'summary.json')) {
    Get-Content (Join-Path $ReportDirectory 'summary.json') -Raw -Encoding UTF8 | ConvertFrom-Json | Select-Object caseId, status, cleanupPassed
}
exit $process.ExitCode

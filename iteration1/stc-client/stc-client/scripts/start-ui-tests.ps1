param([switch]$Build)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if ($Build) { & (Join-Path $PSScriptRoot 'build-ui-tests.ps1') }
$runtime = Join-Path $clientRoot 'runtime/ui-test-client'
$executable = Join-Path $runtime 'SalesTradingUiTests.exe'
if (!(Test-Path -LiteralPath $executable)) { throw 'Build first: .\build-ui-tests.ps1' }
$cases = Join-Path $clientRoot 'tests/ui/cases'
# The user requests a visible test-management window.
Start-Process -FilePath $executable -WorkingDirectory $runtime -ArgumentList @('--cases', ('"' + $cases + '"')) | Out-Null

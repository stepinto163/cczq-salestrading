param([string]$TestRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$TestRoot) { $TestRoot = Join-Path $clientRoot 'runtime/ui-test-client' }
$source = Join-Path $clientRoot 'tests/ui'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$output = Join-Path $TestRoot 'QTradeTestConfigurationChecks.exe'
& $compiler /nologo /target:exe "/out:$output" /r:System.dll /r:System.Core.dll "/r:$TestRoot/bin/gateway-api.dll" "$source/CaseDefinition.cs" "$source/QTradeTestConfiguration.cs" "$source/QTradeTestConfigurationChecks.cs"
if ($LASTEXITCODE -ne 0) { throw 'Configuration check compilation failed.' }
Copy-Item -LiteralPath "$TestRoot/SalesTradingUiTests.exe.config" -Destination "$output.config" -Force
& $output "$source/cases" $TestRoot
if ($LASTEXITCODE -ne 0) { throw 'QTrade test configuration checks failed.' }

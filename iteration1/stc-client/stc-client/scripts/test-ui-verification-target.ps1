param([string]$TestRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$TestRoot) { $TestRoot = Join-Path $clientRoot 'runtime/ui-test-client' }
$source = Join-Path $clientRoot 'tests/ui'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll', '/r:System.Core.dll', "/r:$TestRoot/UiScenarioWorker.exe", "/r:$TestRoot/bin/gateway-api.dll")
foreach ($name in @('WindowsBase', 'PresentationFramework', 'PresentationCore', 'System.Xaml')) { $references += "/r:$framework/$name.dll" }
$output = Join-Path $TestRoot 'UiVerificationTargetCheck.exe'
& $compiler /nologo /target:exe "/out:$output" $references "$source/OperationInterval.cs" "$source/UiVerificationTargetCheck.cs"
if ($LASTEXITCODE -ne 0) { throw 'Verification target check compilation failed.' }
Copy-Item -LiteralPath "$TestRoot/SalesTradingUiTests.exe.config" -Destination "$output.config" -Force
$process = Start-Process $output -ArgumentList ('"' + $TestRoot + '"') -WindowStyle Hidden -PassThru -RedirectStandardOutput "$TestRoot/verification-target-check.stdout.log" -RedirectStandardError "$TestRoot/verification-target-check.stderr.log"
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Verification target check exceeded the 15-second watchdog.'
}
Get-Content "$TestRoot/verification-target-check.stdout.log"
if ($process.ExitCode -ne 0) {
    Get-Content "$TestRoot/verification-target-check.stderr.log"
    throw 'Verification target check failed.'
}

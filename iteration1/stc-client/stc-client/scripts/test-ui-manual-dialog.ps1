param([string]$TestRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$TestRoot) { $TestRoot = Join-Path $clientRoot 'runtime/ui-test-client' }
$TestRoot = (Resolve-Path $TestRoot).Path
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll', '/r:System.Core.dll', "/r:$TestRoot/UiScenarioWorker.exe", "/r:$TestRoot/bin/STC.SalesTrading.Plugin.dll", "/r:$TestRoot/bin/gateway-api.dll")
foreach ($name in @('WindowsBase', 'PresentationFramework', 'PresentationCore', 'System.Xaml')) { $references += "/r:$framework/$name.dll" }
$executable = Join-Path $TestRoot 'ManualDialogCheck.exe'
& $compiler /nologo /target:exe "/out:$executable" $references "$clientRoot/tests/ui/ManualDialogCheck.cs"
if ($LASTEXITCODE -ne 0) { throw 'Dialog check compilation failed.' }
Copy-Item -LiteralPath "$TestRoot/SalesTradingUiTests.exe.config" -Destination "$executable.config" -Force
$output = Join-Path $clientRoot 'runtime/manual-dialog-check'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$process = Start-Process -FilePath $executable -WorkingDirectory $output -PassThru -WindowStyle Hidden -RedirectStandardOutput "$output/stdout.log" -RedirectStandardError "$output/stderr.log"
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Dialog check exceeded the 15-second watchdog.'
}
Get-Content "$output/stdout.log"
if ($process.ExitCode -ne 0 -or !(Select-String -LiteralPath "$output/stdout.log" -Pattern '^PASS:' -Quiet)) {
    Get-Content "$output/stderr.log"
    throw "Dialog check failed: $($process.ExitCode)"
}

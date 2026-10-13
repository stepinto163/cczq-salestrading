param([switch]$NativeUi, [switch]$SalesTrading, [switch]$Merged)
if ($SalesTrading) { $Merged = $true }
if ($Merged) {
    $NativeUi = $true
}
if ($SalesTrading) {
    $NativeUi = $true
}
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$outputRoot = Join-Path $clientRoot 'runtime/native-client'
if ($Merged) {
    & (Join-Path $PSScriptRoot 'run-ui-tests.ps1') -StcConfig (Join-Path $outputRoot 'STC.exe.config') -Cases '01-quote-lifecycle,03-pending-push,05-settlement-update,06-invalid-price-recovery,07-management-filters,08-market-filters,14-watchlist-notification-read'
    exit $LASTEXITCODE
}
if (Get-Process -Name 'NativeStartupCheck', 'NativeSalesTradingCheck', 'MergedSalesTradingCheck', 'LocalIntegrationChecks', 'STC' -ErrorAction SilentlyContinue) {
    throw 'Close the native STC client or existing integration test before running this check.'
}
$existingLogLines = @{}
Get-ChildItem (Join-Path $outputRoot 'logs') -Filter 'STC_*.LOG' -ErrorAction SilentlyContinue | ForEach-Object {
    $existingLogLines[$_.FullName] = @(Get-Content -LiteralPath $_.FullName).Count
}
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (!$compiler) {
    throw 'Roslyn C# compiler was not found.'
}
& (Join-Path $PSScriptRoot 'start-native-client.ps1') -Mode Local -PrepareOnly
$testName = if ($NativeUi) { 'NativeStartupCheck' } else { 'LocalIntegrationChecks' }
$resultFile = if ($Merged) { 'merged-salestrading-result.txt' } elseif ($SalesTrading) { 'native-salestrading-result.txt' } else { 'native-startup-result.txt' }
$resultPath = Join-Path $outputRoot $resultFile
if ($NativeUi -and (Test-Path -LiteralPath $resultPath)) {
    Remove-Item -LiteralPath $resultPath
}
$testExe = Join-Path $outputRoot "$testName.exe"
$references = @(
    '/r:System.dll', '/r:System.Core.dll', '/r:System.Configuration.dll',
    "/r:$outputRoot/bin/common.netapi.dll", "/r:$outputRoot/bin/gateway-api.dll"
)
if ($NativeUi) {
    $framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
    $references += @("/r:$outputRoot/STC.exe", "/r:$outputRoot/bin/STC.Frame.dll", "/r:$outputRoot/bin/STC.Common.dll", "/r:$outputRoot/bin/STC.SalesTrading.Plugin.dll")
    if ($NativeUi) {
        $references += "/r:$outputRoot/bin/CommunityToolkit.Mvvm.dll"
        $references += "/r:$framework/Facades/netstandard.dll"
    }
    foreach ($dll in @('WindowsBase.dll', 'PresentationFramework.dll', 'PresentationCore.dll', 'System.Xaml.dll')) {
        $references += "/r:$framework/$dll"
    }
}
& $compiler /nologo /target:exe "/out:$testExe" $references (Join-Path $clientRoot "tests/$testName.cs")
if ($LASTEXITCODE -ne 0) {
    throw 'Test compilation failed.'
}
Copy-Item -LiteralPath (Join-Path $outputRoot 'STC.exe.config') -Destination "$testExe.config" -Force
$stdout = Join-Path $outputRoot "$testName.stdout.log"
$stderr = Join-Path $outputRoot "$testName.stderr.log"
$process = Start-Process -FilePath $testExe -WorkingDirectory $outputRoot -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$null = $process.Handle
# A separate PowerShell process supervises the bounded test; never raise load automatically.
if (!$process.WaitForExit(45000)) {
    Stop-Process -Id $process.Id -Force
    throw "$testName exceeded 45 seconds and was terminated. See $stdout and $stderr."
}
$process.WaitForExit()
Get-Content -LiteralPath $stdout -Tail 12
Get-Content -LiteralPath $stderr -Tail 8
if ($NativeUi) {
    $result = Get-Content -LiteralPath $resultPath -Encoding UTF8
    $result | Select-Object -Last 8
    if ($result -match '^FAIL|^CLEANUP_FAILED' -or (($SalesTrading -or $Merged) -and $result -notcontains 'RESULT PASS')) {
        throw "Functional checks failed. See $resultPath."
    }
}
if ($process.ExitCode -ne 0) {
    throw "$testName failed with exit code $($process.ExitCode)."
}
if ($NativeUi) {
    Get-ChildItem (Join-Path $outputRoot 'logs') -Filter 'STC_*.LOG' | ForEach-Object {
        $previousLineCount = [int]$existingLogLines[$_.FullName]
        $newLines = @(Get-Content -LiteralPath $_.FullName | Select-Object -Skip $previousLineCount)
        if ($newLines -match '\bERROR\b|LOCAL_UNSUPPORTED') {
            throw "Native startup logged an error. See $($_.FullName)."
        }
    }
    Write-Host 'PASS: startup log contains no new errors or unsupported service calls.'
}

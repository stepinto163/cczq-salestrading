param([string]$OutputRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$OutputRoot) { $OutputRoot = Join-Path $clientRoot 'runtime/native-client' }
if (!$env:PARSER_TEST_USER -or !$env:PARSER_TEST_PASSWORD) {
    throw 'Set PARSER_TEST_USER and PARSER_TEST_PASSWORD to a dedicated local test account.'
}
$testName = 'ParserLiveEndToEndCheck'
if (Get-Process -Name $testName -ErrorAction SilentlyContinue) { throw 'Test already running.' }
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll','/r:System.Core.dll','/r:System.Configuration.dll',"/r:$framework/Facades/netstandard.dll", "/r:$OutputRoot/STC.exe")
foreach ($dll in @('common.netapi','gateway-api','STC.Frame','STC.Common','STC.SalesTrading.Plugin','CommunityToolkit.Mvvm')) {
    $references += "/r:$OutputRoot/bin/$dll.dll"
}
foreach ($dll in @('WindowsBase','PresentationFramework','PresentationCore','System.Xaml')) { $references += "/r:$framework/$dll.dll" }
& $compiler /nologo /target:exe "/out:$OutputRoot/$testName.exe" $references "$clientRoot/tests/$testName.cs"
if ($LASTEXITCODE -ne 0) { throw 'Live parser test compilation failed.' }
Copy-Item -LiteralPath "$OutputRoot/STC.exe.config" -Destination "$OutputRoot/$testName.exe.config" -Force
$process = Start-Process -FilePath "$OutputRoot/$testName.exe" -WorkingDirectory $OutputRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$OutputRoot/$testName.stdout.log" -RedirectStandardError "$OutputRoot/$testName.stderr.log"
$null = $process.Handle
$deadline = [DateTime]::UtcNow.AddSeconds(150)
while (!$process.WaitForExit(1000)) {
    if ([DateTime]::UtcNow -gt $deadline) {
        Stop-Process -Id $process.Id -Force
        throw 'Live parser test exceeded 150 seconds. Inspect logs and cleanup any test quotes before retrying.'
    }
}
Get-Content "$OutputRoot/parser-e2e-result.txt" -Encoding UTF8 -ErrorAction SilentlyContinue
if ($process.ExitCode -ne 0) { throw "Live parser test failed: exit $($process.ExitCode). See client logs." }

param([Parameter(Mandatory=$true)][string]$OutputRoot, [Parameter(Mandatory=$true)][string]$ClientConfig, [switch]$SmokeOnly)
$ErrorActionPreference = 'Stop'
if (!$env:FICC_TEST_USER -or !$env:FICC_TEST_PASSWORD) { throw 'Set FICC_TEST_USER and FICC_TEST_PASSWORD.' }
$clientRoot = Split-Path $PSScriptRoot -Parent
$outputRoot = (Resolve-Path -LiteralPath $OutputRoot).Path
$testName = 'SidebarFilterChecks'
if (Get-Process -Name $testName -ErrorAction SilentlyContinue) { throw 'Sidebar checks are already running.' }
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll','/r:System.Core.dll',"/r:$framework/Facades/netstandard.dll")
foreach ($name in @('WindowsBase','PresentationFramework','PresentationCore','System.Xaml')) { $references += "/r:$framework/$name.dll" }
foreach ($name in @('gateway-api','common.netapi','STC.Common','STC.SalesTrading.Plugin','CommunityToolkit.Mvvm')) { $references += "/r:$outputRoot/bin/$name.dll" }
$testExe = Join-Path $outputRoot "$testName.exe"
& $compiler /nologo /target:exe "/out:$testExe" $references "$clientRoot/tests/$testName.cs"
if ($LASTEXITCODE -ne 0) { throw 'Sidebar test compilation failed.' }
Copy-Item -LiteralPath $ClientConfig -Destination "$testExe.config" -Force
$arguments = if ($SmokeOnly) { '--smoke' } else { '--full' }
$process = Start-Process -FilePath $testExe -ArgumentList $arguments -WorkingDirectory $outputRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$outputRoot/$testName.stdout.log" -RedirectStandardError "$outputRoot/$testName.stderr.log"
$null = $process.Handle
$deadline = [DateTime]::UtcNow.AddSeconds(180)
while (!$process.WaitForExit(1000)) {
    if ([DateTime]::UtcNow -gt $deadline) {
        Stop-Process -Id $process.Id -Force
        throw 'Sidebar tests exceeded the 180-second watchdog.'
    }
}
Get-Content -LiteralPath "$outputRoot/$testName.stdout.log" -Encoding UTF8
Get-Content -LiteralPath "$outputRoot/$testName.stderr.log" -Encoding UTF8
if ($process.ExitCode -ne 0) { throw 'Sidebar tests failed.' }

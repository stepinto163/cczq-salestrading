param(
    [Parameter(Mandatory=$true)][string]$OutputRoot,
    [Parameter(Mandatory=$true)][string]$ProtocolFixture,
    [Parameter(Mandatory=$true)][string]$ClientConfig
)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$outputRoot = (Resolve-Path -LiteralPath $OutputRoot).Path
$fixture = (Resolve-Path -LiteralPath $ProtocolFixture).Path
$testName = 'ProtocolContractChecks'
if (Get-Process -Name $testName -ErrorAction SilentlyContinue) {
    throw 'A Protocol contract check is already running.'
}
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (!$compiler) { throw 'Roslyn C# compiler was not found.' }
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll', '/r:System.Core.dll', "/r:$framework/Facades/netstandard.dll")
foreach ($name in @('WindowsBase', 'PresentationFramework', 'PresentationCore', 'System.Xaml')) {
    $references += "/r:$framework/$name.dll"
}
foreach ($name in @('gateway-api', 'common.netapi', 'STC.Common', 'STC.SalesTrading.Plugin', 'CommunityToolkit.Mvvm')) {
    $references += "/r:$outputRoot/bin/$name.dll"
}
$testExe = Join-Path $outputRoot "$testName.exe"
& $compiler /nologo /target:exe "/out:$testExe" $references (Join-Path $clientRoot "tests/$testName.cs")
if ($LASTEXITCODE -ne 0) { throw 'Protocol contract test compilation failed.' }
$config = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration><startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" /></startup>
<runtime><assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"><probing privatePath="bin" /></assemblyBinding></runtime>
</configuration>
'@
$testConfiguration = New-Object System.Xml.XmlDocument
$testConfiguration.LoadXml($config)
$hostConfiguration = New-Object System.Xml.XmlDocument
$hostConfiguration.Load((Resolve-Path -LiteralPath $ClientConfig).Path)
if ($hostConfiguration.configuration.runtime) {
    $runtime = $testConfiguration.ImportNode($hostConfiguration.configuration.runtime, $true)
    $null = $testConfiguration.configuration.ReplaceChild($runtime, $testConfiguration.configuration.runtime)
}
$testConfiguration.Save("$testExe.config")
$stdout = Join-Path $outputRoot "$testName.stdout.log"
$stderr = Join-Path $outputRoot "$testName.stderr.log"
$process = Start-Process -FilePath $testExe -ArgumentList @('"' + $fixture + '"') -WorkingDirectory $outputRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$null = $process.Handle
if (!$process.WaitForExit(30000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Protocol contract checks exceeded the 30-second watchdog.'
}
$process.WaitForExit()
Get-Content -LiteralPath $stdout -Encoding UTF8
Get-Content -LiteralPath $stderr -Encoding UTF8
if ($process.ExitCode -ne 0) { throw "Protocol contract checks failed. See $stderr" }

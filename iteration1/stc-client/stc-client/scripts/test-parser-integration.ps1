param([string]$OutputRoot, [string]$ClientConfig)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$OutputRoot) {
    $OutputRoot = Join-Path $clientRoot 'runtime/native-client'
}
$testName = 'ParserIntegrationChecks'
$testExe = Join-Path $outputRoot "$testName.exe"
if (Get-Process -Name $testName -ErrorAction SilentlyContinue) {
    throw 'An existing parser integration check is already running.'
}
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (!$compiler) {
    throw 'Roslyn C# compiler was not found.'
}
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll', '/r:System.Core.dll', "/r:$framework/Facades/netstandard.dll")
foreach ($name in @('WindowsBase', 'PresentationFramework', 'PresentationCore', 'System.Xaml')) {
    $references += "/r:$framework/$name.dll"
}
foreach ($name in @('gateway-api', 'common.netapi', 'STC.Common', 'STC.SalesTrading.Plugin', 'CommunityToolkit.Mvvm')) {
    $references += "/r:$outputRoot/bin/$name.dll"
}
& $compiler /nologo /target:exe "/out:$testExe" $references (Join-Path $clientRoot "tests/$testName.cs")
if ($LASTEXITCODE -ne 0) {
    throw 'Parser integration test compilation failed.'
}
$config = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration><startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" /></startup>
<runtime><assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"><probing privatePath="bin" /></assemblyBinding></runtime>
</configuration>
'@
if ($ClientConfig) {
    $testConfiguration = New-Object System.Xml.XmlDocument
    $testConfiguration.LoadXml($config)
    $hostConfiguration = New-Object System.Xml.XmlDocument
    $hostConfiguration.Load((Resolve-Path -LiteralPath $ClientConfig).Path)
    if ($hostConfiguration.configuration.runtime) {
        $runtime = $testConfiguration.ImportNode($hostConfiguration.configuration.runtime, $true)
        $null = $testConfiguration.configuration.ReplaceChild($runtime, $testConfiguration.configuration.runtime)
    }
    $config = $testConfiguration.OuterXml
}
[IO.File]::WriteAllText("$testExe.config", $config)
$stdout = Join-Path $outputRoot "$testName.stdout.log"
$stderr = Join-Path $outputRoot "$testName.stderr.log"
$process = Start-Process -FilePath $testExe -WorkingDirectory $outputRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$null = $process.Handle
if (!$process.WaitForExit(30000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Parser integration checks exceeded the 30-second watchdog.'
}
$process.WaitForExit()
Get-Content -LiteralPath $stdout -Encoding UTF8
Get-Content -LiteralPath $stderr -Encoding UTF8
if ($process.ExitCode -ne 0) {
    throw "Parser integration checks failed. See $stderr"
}
$logFiles = Get-ChildItem (Join-Path $outputRoot 'parser-check-logs') -File
$logText = ($logFiles | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 }) -join "`n"
if ($logText -notmatch 'REQ stc.parser.bond.*LOG-CHECK' -or $logText -notmatch 'REP stc.parser.bond.*Datas') {
    throw 'Actual parser DPS request/reply log files were not written.'
}
Write-Host 'PASS actual DPS log file contains parser request and full response.'

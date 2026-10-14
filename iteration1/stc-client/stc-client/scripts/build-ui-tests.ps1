param([string]$OutputRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$OutputRoot) { $OutputRoot = Join-Path $clientRoot 'runtime/ui-test-client' }
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
$source = Join-Path $clientRoot 'tests/ui'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (!$compiler) { throw 'Visual Studio Build Tools (Roslyn) is required to build the tests.' }
$native = Join-Path $clientRoot 'runtime/native-client'
if (!(Test-Path "$native/STC.exe")) { throw 'Build the STC client first: build-native-client.ps1' }
New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
foreach ($folder in @('bin', 'control', 'config')) {
    if (Test-Path "$native/$folder") { Copy-Item -LiteralPath "$native/$folder" -Destination $OutputRoot -Recurse -Force }
}
Copy-Item -LiteralPath "$native/STC.exe", "$native/STC.exe.config" -Destination $OutputRoot -Force
$msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild "$clientRoot/STC.SalesTrading.Plugin/STC.SalesTrading.Plugin.csproj" /t:Build /m:1 /nr:false /v:minimal /p:Configuration=Release /p:BuildProjectReferences=false /p:TargetFrameworkVersion=v4.7.2 "/p:OutputPath=$OutputRoot/bin/" "/p:CustomAfterMicrosoftCommonTargets=$PSScriptRoot/NativeClientBuild.targets" *> "$OutputRoot/plugin-build.log"
if ($LASTEXITCODE -ne 0) { Get-Content "$OutputRoot/plugin-build.log" -Tail 20; throw 'Product plugin build failed.' }
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$common = @('/r:System.dll', '/r:System.Core.dll', "/r:$OutputRoot/bin/gateway-api.dll")
$worker = $common + @('/r:System.Configuration.dll', "/r:$OutputRoot/STC.exe", "/r:$framework/Facades/netstandard.dll")
foreach ($name in @('WindowsBase', 'PresentationFramework', 'PresentationCore', 'System.Xaml')) { $worker += "/r:$framework/$name.dll" }
foreach ($name in @('common.netapi', 'STC.Common', 'STC.Frame', 'STC.SalesTrading.Plugin', 'CommunityToolkit.Mvvm')) { $worker += "/r:$OutputRoot/bin/$name.dll" }
& $compiler /nologo /debug:full /target:exe "/out:$OutputRoot/UiScenarioWorker.exe" $worker "$source/CaseDefinition.cs" "$source/QTradeTestConfiguration.cs" "$source/OperationInterval.cs" "$source/UiScenarioWorker.cs" "$source/UserNoticeAutomation.cs" "$source/DemoCaptionWindow.cs" "$source/ControlHighlight.cs" "$source/UiRegressionActions.cs" "$source/UiQuoteWorkflowActions.cs" "$source/UiNegotiationActions.cs" "$source/UiQTradePublishActions.cs" "$source/UiRealtimeActions.cs" "$source/UiFilterLatencyActions.cs" "$source/UiPendingToolbarActions.cs" "$source/UiGroupManagementActions.cs" "$source/UiPageCoverageActions.cs"
if ($LASTEXITCODE -ne 0) { throw 'UI worker compilation failed.' }
$runner = $common + @('/r:System.Windows.Forms.dll', '/r:System.Drawing.dll', '/r:System.Xml.dll', '/r:System.Xml.Linq.dll')
& $compiler /nologo /debug:full /target:winexe "/out:$OutputRoot/SalesTradingUiTests.exe" $runner "$source/CaseDefinition.cs" "$source/QTradeTestConfiguration.cs" "$source/OperationInterval.cs" "$source/CaseEditor.cs" "$source/SuiteRunner.cs" "$source/ObserverSession.cs" "$source/RunnerWindow.cs" "$source/PlaybackWindow.cs"
if ($LASTEXITCODE -ne 0) { throw 'UI runner compilation failed.' }
$config = '<?xml version="1.0" encoding="utf-8"?><configuration><startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2"/></startup><runtime><assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"><probing privatePath="bin"/></assemblyBinding></runtime></configuration>'
[IO.File]::WriteAllText("$OutputRoot/SalesTradingUiTests.exe.config", $config)
Write-Host "Built: $OutputRoot/SalesTradingUiTests.exe"

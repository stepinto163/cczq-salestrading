param(
    [string]$MSBuild,
    [string]$TargetFramework = 'v4.7.2'
)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$outputRoot = Join-Path $clientRoot 'runtime/native-client'
$binaryRoot = Join-Path $outputRoot 'bin'
New-Item -ItemType Directory -Force -Path $binaryRoot | Out-Null

if (!$MSBuild) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    if (!(Test-Path -LiteralPath $vswhere)) {
        throw 'MSBuild is required. Supply -MSBuild or install Visual Studio Build Tools with WPF support.'
    }
    $MSBuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (!$MSBuild -or !(Test-Path -LiteralPath $MSBuild)) {
    throw 'MSBuild could not be located.'
}

$restoreLog = Join-Path $outputRoot 'restore.log'
& $MSBuild (Join-Path $clientRoot 'STC.SalesTrading.Plugin/STC.SalesTrading.Plugin.csproj') `
    /t:Restore /m:1 /nr:false /v:minimal "/p:TargetFrameworkVersion=$TargetFramework" /p:NuGetAudit=false *> $restoreLog
if ($LASTEXITCODE -ne 0) {
    Get-Content -LiteralPath $restoreLog -Tail 20
    throw "Local NuGet restore failed. See $restoreLog"
}

# Build outputs are isolated from the supplied STC binaries and any running installation.
Get-ChildItem (Join-Path $clientRoot 'client/bin') -File | ForEach-Object {
    if ($_.Name -notin @('common.netapi.dll', 'STC.Common.dll', 'STC.Frame.dll')) {
        Copy-Item -LiteralPath $_.FullName -Destination $binaryRoot -Force
    }
}
$buildTargets = Join-Path $PSScriptRoot 'NativeClientBuild.targets'
$projects = @('netapi/netapi/common.netapi.csproj', 'STC.Common/STC.Common.csproj', 'STC.Frame/STC.Frame.csproj', 'STC.SalesTrading.Plugin/STC.SalesTrading.Plugin.csproj', 'STC/STC.csproj')
foreach ($project in $projects) {
    $logName = [IO.Path]::GetFileNameWithoutExtension($project) + '-build.log'
    $logPath = Join-Path $outputRoot $logName
    Write-Host "Building $project"
    & $MSBuild (Join-Path $clientRoot $project) /t:Build /m:1 /nr:false /v:minimal `
        /p:Configuration=Release /p:BuildProjectReferences=false `
        "/p:TargetFrameworkVersion=$TargetFramework" "/p:OutputPath=$binaryRoot/" `
        "/p:CustomAfterMicrosoftCommonTargets=$buildTargets" *> $logPath
    if ($LASTEXITCODE -ne 0) {
        Get-Content -LiteralPath $logPath -Tail 20
        throw "Build failed: $project. See $logPath"
    }
}
Copy-Item -LiteralPath (Join-Path $binaryRoot 'STC.exe') -Destination $outputRoot -Force
$controlRoot = Join-Path $outputRoot 'control'
New-Item -ItemType Directory -Force -Path $controlRoot | Out-Null
Copy-Item -LiteralPath (Join-Path $clientRoot 'client/control/public.key') -Destination $controlRoot -Force
Write-Host "Built native STC client: $outputRoot/STC.exe"
Write-Host "Framework for this isolated build: $TargetFramework. Source project targets are unchanged."

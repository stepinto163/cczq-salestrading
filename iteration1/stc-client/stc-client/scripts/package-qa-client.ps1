param(
    [string]$GatewayHost,
    [ValidateRange(1, 65535)]
    [int]$GatewayPort,
    [switch]$Build
)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$buildRoot = Join-Path $clientRoot 'runtime/native-client'
$packageRoot = Join-Path $clientRoot 'runtime/qa-client'
if ($Build) {
    & (Join-Path $PSScriptRoot 'build-native-client.ps1')
}
if (!(Test-Path -LiteralPath (Join-Path $buildRoot 'STC.exe'))) {
    throw 'Build the client first, or specify -Build.'
}
$activeClient = Get-Process -Name STC -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -eq (Join-Path $packageRoot 'STC.exe')
}
if ($activeClient) {
    throw 'Close the QA client before updating its package.'
}
New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null
foreach ($directory in @('bin', 'control', 'logs')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot $directory) | Out-Null
}
Get-ChildItem (Join-Path $buildRoot 'bin') -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $packageRoot 'bin') -Force
}
Copy-Item -LiteralPath (Join-Path $buildRoot 'STC.exe') -Destination $packageRoot -Force
Copy-Item -LiteralPath (Join-Path $clientRoot 'client/control/public.key') -Destination (Join-Path $packageRoot 'control') -Force
# STC reads updater metadata even when automatic updating is disabled.
Copy-Item -LiteralPath (Join-Path $clientRoot 'client/AutoUpdateService.xml') -Destination $packageRoot -Force
$config = New-Object System.Xml.XmlDocument
$config.Load((Join-Path $clientRoot 'control/QA/STC.exe.config'))
function Set-QASetting([string]$key, [string]$value) {
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='$key']")
    if (!$setting) {
        $setting = $config.CreateElement('add')
        $setting.SetAttribute('key', $key)
        $null = $config.configuration.appSettings.AppendChild($setting)
    }
    $setting.SetAttribute('value', $value)
}
Set-QASetting 'Ficc.IntegrationMode' 'stc'
Set-QASetting 'ApiKey' '1'
Set-QASetting 'StartDps' '2'
Set-QASetting 'ClientUpdateType' '3'
Set-QASetting 'LoginEnv' 'QA'
Set-QASetting 'LogLevel' 'info'
Set-QASetting 'LogPath' './logs/STC_'
Set-QASetting 'DefaultPlugin' '230-销交平台'
Set-QASetting 'DefaultFocusPlugin' '销交平台'
# Preserve the tested binding redirects and framework settings for this build.
$binaryConfig = New-Object System.Xml.XmlDocument
$binaryConfig.Load((Join-Path $clientRoot 'client/STC.exe.config'))
foreach ($sectionName in @('runtime', 'startup')) {
    $replacement = $config.ImportNode($binaryConfig.configuration.SelectSingleNode($sectionName), $true)
    $old = $config.configuration.SelectSingleNode($sectionName)
    if ($old) {
        $null = $config.configuration.ReplaceChild($replacement, $old)
    } else {
        $null = $config.configuration.AppendChild($replacement)
    }
}
foreach ($server in $config.SelectNodes('/configuration/ServerGroupCfg/ServerGroup/Server')) {
    if ($GatewayHost) { $server.SetAttribute('DpsHost', $GatewayHost) }
    if ($PSBoundParameters.ContainsKey('GatewayPort')) { $server.SetAttribute('DpsPort', "$GatewayPort") }
    $server.SetAttribute('Host', $server.DpsHost)
    $server.SetAttribute('Port', $server.DpsPort)
    Write-Host "QA GW: $($server.DpsHost):$($server.DpsPort)"
}
$config.Save((Join-Path $packageRoot 'STC.exe.config'))
@'
@echo off
cd /d "%~dp0"
start "" "STC.exe"
'@ | Set-Content -LiteralPath (Join-Path $packageRoot 'start-qa.cmd') -Encoding ASCII
Write-Host "QA client: $packageRoot"
Write-Host 'Real STC credentials are required. No simulator or local DPS relay is enabled.'

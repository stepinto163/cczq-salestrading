param(
    [ValidateSet('Local', 'Stc')]
    [string]$Mode = 'Local',
    [string]$StcConfig,
    [string]$OutputRoot,
    [int]$GatewayPort = 18600,
    [switch]$Build,
    [switch]$PrepareOnly
)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$OutputRoot) { $OutputRoot = Join-Path $clientRoot 'runtime/native-client' }
if ($Build) {
    & (Join-Path $PSScriptRoot 'build-native-client.ps1') -OutputRoot $OutputRoot
}
$executable = Join-Path $outputRoot 'STC.exe'
if (!(Test-Path -LiteralPath $executable)) {
    throw 'Build the native client first: .\scripts\build-native-client.ps1'
}
$config = New-Object System.Xml.XmlDocument
if ($Mode -eq 'Stc') {
    if (!$StcConfig -or !(Test-Path -LiteralPath $StcConfig)) {
        throw 'STC mode requires -StcConfig pointing to your environment STC.exe.config.'
    }
    $config.Load((Resolve-Path -LiteralPath $StcConfig).Path)
} else {
    $config.Load((Join-Path $clientRoot 'client/STC.exe.config'))
}
function Set-AppSetting([string]$name, [string]$value) {
    $element = $config.SelectSingleNode("/configuration/appSettings/add[@key='$name']")
    if (!$element) {
        $element = $config.CreateElement('add')
        $element.SetAttribute('key', $name)
        $config.configuration.appSettings.AppendChild($element) | Out-Null
    }
    $element.SetAttribute('value', $value)
}
Set-AppSetting 'Ficc.IntegrationMode' $Mode.ToLowerInvariant()
if ($Mode -eq 'Local') {
    Set-AppSetting 'ApiKey' '1'
    Set-AppSetting 'StartDps' '2'
    Set-AppSetting 'IsGzip' '0'
    Set-AppSetting 'ClientUpdateType' '3'
    Set-AppSetting 'LoginEnv' 'LOCAL'
    Set-AppSetting 'AppName' 'stc'
    Set-AppSetting 'IsSingle' 'true'
    Set-AppSetting 'LogLevel' 'info'
    Set-AppSetting 'LogPath' './logs/STC_'
    $servers = $config.SelectSingleNode('/configuration/ServerGroupCfg/ServerGroup')
    $servers.RemoveAll()
    $server = $config.CreateElement('Server')
    $values = @{ Id='1'; Name='LOCAL FICC'; Host='127.0.0.1'; Port="$GatewayPort"; Protocol='TCP'; DpsHost='127.0.0.1'; DpsPort="$GatewayPort"; DpsProtocol='TCP'; UpdateHost='127.0.0.1'; UpdatePort='1'; UpdateProtocol='http' }
    foreach ($key in $values.Keys) {
        $server.SetAttribute($key, $values[$key])
    }
    $servers.AppendChild($server) | Out-Null
    $fixtureRoot = Join-Path $outputRoot 'config'
    New-Item -ItemType Directory -Force -Path $fixtureRoot | Out-Null
    Copy-Item -LiteralPath (Join-Path $clientRoot 'control/LOCAL/local-stc-fixtures.json') -Destination $fixtureRoot -Force
}
$config.Save((Join-Path $outputRoot 'STC.exe.config'))
New-Item -ItemType Directory -Force -Path (Join-Path $outputRoot 'logs') | Out-Null
Write-Host "Native client mode: $Mode"
if ($PrepareOnly) {
    Write-Host "Configuration prepared: $outputRoot/STC.exe.config"
    return
}
if ($Mode -eq 'Local') {
    $socket = New-Object System.Net.Sockets.TcpClient
    try {
        $connection = $socket.BeginConnect('127.0.0.1', $GatewayPort, $null, $null)
        if (!$connection.AsyncWaitHandle.WaitOne(1500)) {
            throw 'Local GW is not listening. Start the existing PostgreSQL, ZooKeeper, QuoteSvr, NotificationSvr and standard GW first.'
        }
        $socket.EndConnect($connection)
    } finally {
        $socket.Dispose()
    }
}
# User explicitly requested an interactive native client.
$process = Start-Process -FilePath $executable -WorkingDirectory $outputRoot -PassThru
Write-Host "STC client started: PID $($process.Id)."
if ($Mode -eq 'Local') {
    Write-Host 'Local accounts: trader1 / trader2; simulator password: trader1.'
}

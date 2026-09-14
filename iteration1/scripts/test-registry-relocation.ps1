$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$configuration = Join-Path $taskRoot 'cczq-app-salestrading-quotesvr/control/ATSConfig.ini'
$original = [IO.File]::ReadAllText($configuration)
$encoding = New-Object Text.UTF8Encoding($false)
$notificationPid = [int](Get-Content "$taskRoot/runtime/notificationsvr.pid")
$gatewayPid = [int](Get-Content "$taskRoot/runtime/gateway.pid")
$notificationLog = Join-Path $taskRoot 'runtime/notificationsvr.log'
$initialSyncCount = @(Select-String -LiteralPath $notificationLog -Pattern 'source synchronized').Count

if ($original -notmatch 'SERVER.QuoteSvr.Host=127.0.0.1:18680') {
    throw 'Relocation check expects the normal QuoteSvr port 18680.'
}
if (Get-NetTCPConnection -LocalPort 18681 -State Listen -ErrorAction SilentlyContinue) {
    throw 'Temporary test port 18681 is already in use.'
}

try {
    & powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot/stop-quotesvr.ps1"
    if ($LASTEXITCODE -ne 0) { throw 'QuoteSvr stop failed.' }
    [IO.File]::WriteAllText($configuration, $original.Replace('127.0.0.1:18680', '127.0.0.1:18681'), $encoding)
    & powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot/start-quotesvr.ps1"
    if ($LASTEXITCODE -ne 0) { throw 'Temporary QuoteSvr start failed.' }

    $deadline = [DateTime]::UtcNow.AddSeconds(50)
    $recovered = $false
    while ([DateTime]::UtcNow -lt $deadline) {
        $syncCount = @(Select-String -LiteralPath $notificationLog -Pattern 'source synchronized').Count
        $newConnection = Select-String -LiteralPath $notificationLog -Pattern 'GwClient connected to Host:127.0.0.1,Port:18681'
        if ($syncCount -gt $initialSyncCount -and $newConnection) {
            $recovered = $true
            break
        }
        Start-Sleep -Milliseconds 500
    }
    if (!$recovered) { throw 'NotificationSvr did not rediscover and synchronize within 50 seconds.' }

    # Allow the old ephemeral instance to expire before checking request routing.
    Start-Sleep -Seconds 3
    & powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot/test-service-discovery.ps1" -Name 'service-relocated'
    if ($LASTEXITCODE -ne 0) { throw 'Service calls failed after relocation.' }
    if ([int](Get-Content "$taskRoot/runtime/notificationsvr.pid") -ne $notificationPid -or
        [int](Get-Content "$taskRoot/runtime/gateway.pid") -ne $gatewayPid) {
        throw 'A consuming service was restarted during relocation.'
    }
    @{ result='PASS'; quotePort=18681; notificationPid=$notificationPid; gatewayPid=$gatewayPid; quotesCreated=0 } |
        ConvertTo-Json | Set-Content "$taskRoot/runtime/service-relocation.json" -Encoding UTF8
} finally {
    & powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot/stop-quotesvr.ps1"
    [IO.File]::WriteAllText($configuration, $original, $encoding)
    & powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot/start-quotesvr.ps1"
    Write-Output 'Normal QuoteSvr configuration restored to port 18680.'
}

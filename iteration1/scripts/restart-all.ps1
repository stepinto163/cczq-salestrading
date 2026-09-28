[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch]$IncludeInfrastructure
)

$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$runtimePath = Join-Path $taskRoot 'runtime'
$powershellPath = Join-Path $PSHOME 'powershell.exe'
if (-not (Test-Path -LiteralPath $powershellPath)) {
    $powershellPath = (Get-Command powershell.exe -ErrorAction Stop).Source
}

function Invoke-ProjectScript {
    param([string]$Name)

    Write-Host "[restart] $Name"
    # Isolate scripts that use exit, and apply execution policy only to this child process.
    & $powershellPath -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot $Name)
    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed (exit $LASTEXITCODE). See $runtimePath for service logs."
    }
}

function Find-ProjectJava {
    param([string]$MainClass)

    @(Get-CimInstance Win32_Process -Filter "Name='java.exe'" | Where-Object {
        $_.CommandLine -and $_.CommandLine.Contains($taskRoot) -and
        $_.CommandLine -match ('\s' + [regex]::Escape($MainClass) + '(\s|$)')
    })
}

function Repair-ServicePid {
    param([string]$Name, [string]$MainClass)

    $ownedProcesses = @(Find-ProjectJava $MainClass)
    if ($ownedProcesses.Count -gt 1) {
        throw "Multiple project $Name processes found. Resolve duplicates before restarting."
    }
    $pidFile = Join-Path $runtimePath "$Name.pid"
    if (Test-Path -LiteralPath $pidFile) {
        $recordedProcessId = [int](Get-Content -LiteralPath $pidFile)
        $recordedProcess = Get-Process -Id $recordedProcessId -ErrorAction SilentlyContinue
        if ($recordedProcess -and $recordedProcessId -notin $ownedProcesses.ProcessId) {
            throw "The $Name PID file points to another process. No process was stopped."
        }
    }
    if ($ownedProcesses.Count -eq 1) {
        $ownedProcesses[0].ProcessId | Set-Content -LiteralPath $pidFile
    }
}

function Wait-ServiceReady {
    param([string]$Name, [string[]]$RequiredLogText)

    $serviceProcessId = [int](Get-Content -LiteralPath (Join-Path $runtimePath "$Name.pid"))
    $logPath = Join-Path $runtimePath "$Name.log"
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    Write-Host "[restart] Waiting for $Name (PID $serviceProcessId)..."
    while ([DateTime]::UtcNow -lt $deadline) {
        if (-not (Get-Process -Id $serviceProcessId -ErrorAction SilentlyContinue)) {
            throw "$Name exited. Inspect $logPath and $runtimePath/$Name.err."
        }
        $logText = if (Test-Path -LiteralPath $logPath) {
            Get-Content -LiteralPath $logPath -Encoding UTF8 -Tail 400
        } else { @() }
        $logJoined = $logText -join "`n"
        $missing = @($RequiredLogText | Where-Object { -not $logJoined.Contains($_) })
        $listeners = @(Get-NetTCPConnection -State Listen -OwningProcess $serviceProcessId -ErrorAction SilentlyContinue)
        if ($missing.Count -eq 0 -and $listeners.Count -gt 0) {
            Write-Host "[restart] $Name ready; listening ports: $(($listeners.LocalPort | Sort-Object -Unique) -join ', ')"
            return
        }
        Start-Sleep -Milliseconds 500
    }
    throw "$Name was not ready within 45 seconds. Inspect $logPath and $runtimePath/$Name.err; later components were not started."
}

$scope = 'QuoteSvr, NotificationSvr, GW and web server'
if ($IncludeInfrastructure) {
    $scope += ', project PostgreSQL and ZooKeeper'
}
if (-not $PSCmdlet.ShouldProcess($taskRoot, "Restart $scope using existing configuration")) {
    return
}

# Check the existing local deployment before closing any application.
foreach ($requiredPath in @(
    (Join-Path $taskRoot 'cgws-ficc-saletrading-quotesvr/control/server.lic'),
    (Join-Path $taskRoot 'cgws-ficc-saletrading-quotesvr/target/cgws-ficc-saletrading-quotesvr-0.1.0-SNAPSHOT.jar'),
    (Join-Path $taskRoot 'cgws-ficc-saletrading-notificationsvr/target/cgws-ficc-saletrading-notificationsvr-0.1.0-SNAPSHOT.jar'),
    (Join-Path $taskRoot 'cgws-ficc-saletrading-common/target/cgws-ficc-saletrading-common-0.1.0-SNAPSHOT.jar'),
    (Join-Path $taskRoot 'GW/javalib/gw-0.1.1-SNAPSHOT.jar')
)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required file missing: $requiredPath. Build/setup this deployment first."
    }
}
foreach ($command in @('java', 'node')) {
    Get-Command $command -ErrorAction Stop | Out-Null
}

$lockPath = Join-Path $runtimePath 'restart-all.lock'
$restartLock = $null
try {
    try {
        $restartLock = [IO.File]::Open($lockPath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    } catch {
        throw 'Another restart is in progress, or the restart lock is inaccessible.'
    }
    Repair-ServicePid 'quotesvr' 'ficc.quote.Main'
    Repair-ServicePid 'notificationsvr' 'ficc.notification.NotificationMain'
    Repair-ServicePid 'gateway' 'GW'

    Invoke-ProjectScript 'stop-web-client.ps1'
    Invoke-ProjectScript 'stop-gateway.ps1'
    Invoke-ProjectScript 'stop-notificationsvr.ps1'
    Invoke-ProjectScript 'stop-quotesvr.ps1'

    if ($IncludeInfrastructure) {
        Invoke-ProjectScript 'stop-zookeeper.ps1'
        Invoke-ProjectScript 'stop-postgres.ps1'
        Invoke-ProjectScript 'start-postgres.ps1'
        Invoke-ProjectScript 'start-zookeeper.ps1'
    }

    Invoke-ProjectScript 'start-quotesvr.ps1'
    Wait-ServiceReady 'quotesvr' @('QuoteSvr READY')
    Invoke-ProjectScript 'start-notificationsvr.ps1'
    Wait-ServiceReady 'notificationsvr' @('NotificationSvr READY', 'source synchronized')
    Invoke-ProjectScript 'start-gateway.ps1'
    Wait-ServiceReady 'gateway' @()
    Invoke-ProjectScript 'start-web-client.ps1'
    Wait-ServiceReady 'web-client' @('Web client:')
    $response = Invoke-WebRequest -Uri 'http://127.0.0.1:8088/' -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -ne 200) { throw 'Web client HTTP check failed.' }

    Write-Host '[restart] Complete. Web http://127.0.0.1:8088/. Start stc-client through its own launcher.'
    Write-Host '[restart] Refresh the browser and log in again. No autotest was started.'
} finally {
    if ($restartLock) { $restartLock.Dispose() }
}

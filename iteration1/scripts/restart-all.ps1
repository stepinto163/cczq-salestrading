[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch]$IncludeInfrastructure
)

$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$runtimePath = Join-Path $taskRoot 'runtime'
$clientPath = Join-Path $taskRoot 'client/bin/Debug/net472/Ficc.SalesTradingClient.exe'
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

$scope = 'QuoteSvr, NotificationSvr, GW, C# client and web server'
if ($IncludeInfrastructure) {
    $scope += ', project PostgreSQL and ZooKeeper'
}
if (-not $PSCmdlet.ShouldProcess($taskRoot, "Restart $scope using existing configuration")) {
    return
}

# Check the existing local deployment before closing any application.
foreach ($requiredPath in @(
    $clientPath,
    (Join-Path $taskRoot 'cczq-app-salestrading-quotesvr/control/server.lic'),
    (Join-Path $taskRoot 'cczq-app-salestrading-quotesvr/target/cczq-app-salestrading-quotesvr.jar'),
    (Join-Path $taskRoot 'cczq-app-salestrading-notificationsvr/target/cczq-app-salestrading-notificationsvr.jar'),
    (Join-Path $taskRoot 'cczq-app-salestrading-common/target/cczq-app-salestrading-common.jar'),
    (Join-Path $taskRoot 'runtime/foundation-build/lib/gw-local.jar')
)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required file missing: $requiredPath. Build/setup this deployment first."
    }
}
foreach ($command in @('java', 'javac', 'node')) {
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

    $clients = @(Get-CimInstance Win32_Process -Filter "Name='Ficc.SalesTradingClient.exe'" | Where-Object {
        $_.ExecutablePath -and [IO.Path]::GetFullPath($_.ExecutablePath) -eq [IO.Path]::GetFullPath($clientPath)
    })
    foreach ($client in $clients) {
        $clientProcess = Get-Process -Id $client.ProcessId -ErrorAction SilentlyContinue
        if (-not $clientProcess) { continue }
        Write-Host "[restart] Closing C# client (PID $($client.ProcessId))..."
        if (-not $clientProcess.CloseMainWindow() -or -not $clientProcess.WaitForExit(8000)) {
            throw 'The C# client did not close. Finish any pending dialog and close it, then rerun this script.'
        }
    }

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

    # This is the interactive client the user requested; background services use hidden windows.
    $clientProcess = Start-Process -FilePath $clientPath -WorkingDirectory (Split-Path $clientPath -Parent) -PassThru
    if ($clientProcess.WaitForExit(1000)) {
        throw "C# client exited immediately. Inspect $runtimePath/client-logs."
    }
    Write-Host "[restart] Complete. C# client PID $($clientProcess.Id); web http://127.0.0.1:8088/"
    Write-Host '[restart] Refresh the browser and log in again. No autotest was started.'
} finally {
    if ($restartLock) { $restartLock.Dispose() }
}

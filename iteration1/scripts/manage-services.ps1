param(
    [string]$Action = 'show',
    [string]$Service = 'all',
    [string]$AtsRoot = (Join-Path $PSScriptRoot '../../../qa_env/ats_root')
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'resolve-command-name.ps1')
. (Join-Path $PSScriptRoot 'service-standby-state.ps1')
$Action = Resolve-CommandName $Action @('start', 'stop', 'restart', 'show', 'log') -Label 'action'
$atsRoot = (Resolve-Path -LiteralPath $AtsRoot).Path
$environmentRoot = Split-Path $atsRoot -Parent
$services = @(Import-Csv -LiteralPath "$atsRoot/scripts/serverlist.csv")
# Accept the original STC command form as well as the short service name.
$Service = ($Service -replace '\\', '/').TrimEnd('/').Split('/')[-1]
$Service = Resolve-CommandName $Service (@($services.Service) + @('all')) -Aliases @{
    'Registry' = 'ZooKeeper'
} -Label 'service'
$selected = @($services | Where-Object { $Service -eq 'all' -or $_.Service -eq $Service })
if ($selected.Count -eq 0) { throw "Unknown service: $Service. Run show.cmd for the service list." }

function Get-ServiceProcess($definition) {
    $pidFile = Join-Path $atsRoot "run/$($definition.Service).pid"
    if (!(Test-Path -LiteralPath $pidFile)) { return $null }
    $serviceProcessId = [int](Get-Content -LiteralPath $pidFile)
    $process = Get-CimInstance Win32_Process -Filter "ProcessId=$serviceProcessId"
    if (!$process) { return $null }
    $expectedPath = Join-Path $atsRoot "stc/$($definition.Service)"
    if ($definition.Service -eq 'ZooKeeper') { $expectedPath = Join-Path $atsRoot 'zookeeper' }
    $command = ([string]$process.CommandLine).Replace('/', '\')
    if ($process.Name -ne 'java.exe' -or $command.IndexOf($expectedPath.Replace('/', '\'), [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "PID $serviceProcessId does not belong to $($definition.Service) in this environment."
    }
    return $process
}

function Get-ServiceState($definition) {
    $process = Get-ServiceProcess $definition
    $listeners = @(Get-NetTCPConnection -LocalPort ([int]$definition.Port) -State Listen -ErrorAction SilentlyContinue)
    $state = 'STOPPED'
    if ($definition.Enabled -ne 'true') { $state = 'DISABLED' }
    if ($listeners.Count -gt 0) { $state = 'PORT CONFLICT' }
    if ($process) {
        $state = 'STARTING / NOT LISTENING'
        if (Test-ServiceStandbyWait -AtsRoot $atsRoot -Service $definition.Service -ProcessStarted $process.CreationDate) {
            $state = 'STANDBY / WAITING'
        }
        if ($listeners | Where-Object { $_.OwningProcess -eq $process.ProcessId }) { $state = 'LISTENING' }
    }
    [PSCustomObject]@{
        Service = $definition.Service
        State = $state
        PID = $(if ($process) { $process.ProcessId } else { '-' })
        Port = $definition.Port
        MemoryMB = $(if ($process) { [int]([long]$process.WorkingSetSize / 1MB) } else { 0 })
        Note = $definition.Note
    }
}

function Start-ServiceProcess($definition) {
    if ($definition.Enabled -ne 'true') {
        if ($Service -ne 'all') { throw "$($definition.Service): $($definition.Note)" }
        Write-Host "SKIPPED $($definition.Service): $($definition.Note)"
        return
    }
    $process = Get-ServiceProcess $definition
    $startedHere = !$process
    if (!$process) {
        & (Join-Path $environmentRoot 'start-service.ps1') -Service $definition.Service
    } else {
        Write-Host "ALREADY RUNNING $($definition.Service), PID $($process.ProcessId)"
    }
    $timeoutSeconds = 90
    if ($definition.StartupTimeoutSeconds) {
        $timeoutSeconds = [int]$definition.StartupTimeoutSeconds
        if ($timeoutSeconds -lt 1 -or $timeoutSeconds -gt 1800) {
            throw 'StartupTimeoutSeconds must be between 1 and 1800.'
        }
    }
    $deadline = [DateTime]::UtcNow.AddSeconds($timeoutSeconds)
    $nextProgress = [DateTime]::UtcNow.AddSeconds(30)
    do {
        $process = Get-ServiceProcess $definition
        if (!$process) { throw "$($definition.Service) exited. Read log/$($definition.Service).stderr.log and .console.log." }
        $listener = Get-NetTCPConnection -LocalPort ([int]$definition.Port) -State Listen -ErrorAction SilentlyContinue |
            Where-Object { $_.OwningProcess -eq $process.ProcessId }
        if ($listener) {
            Write-Host "LISTENING $($definition.Service), PID $($process.ProcessId), port $($definition.Port)"
            return
        }
        if (Test-ServiceStandbyWait -AtsRoot $atsRoot -Service $definition.Service -ProcessStarted $process.CreationDate) {
            Write-Host "STANDBY $($definition.Service), PID $($process.ProcessId), waiting for the service lease; business listener is intentionally closed."
            return
        }
        if ([DateTime]::UtcNow -ge $nextProgress) {
            $remainingSeconds = [int]($deadline - [DateTime]::UtcNow).TotalSeconds
            Write-Host "STARTING $($definition.Service), PID $($process.ProcessId), not listening yet; timeout in $remainingSeconds seconds. See initialization log."
            $nextProgress = [DateTime]::UtcNow.AddSeconds(30)
        }
        Start-Sleep -Milliseconds 500
    } while ([DateTime]::UtcNow -lt $deadline)
    if ($startedHere) {
        Stop-ServiceProcess $definition
    }
    throw "$($definition.Service) did not listen within $timeoutSeconds seconds. A process started by this command has been stopped. Inspect its logs before retrying."
}

function Stop-ServiceProcess($definition) {
    $process = Get-ServiceProcess $definition
    if (!$process) {
        Write-Host "ALREADY STOPPED $($definition.Service)"
        return
    }
    Stop-Process -Id $process.ProcessId -ErrorAction Stop
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    while (Get-Process -Id $process.ProcessId -ErrorAction SilentlyContinue) {
        if ([DateTime]::UtcNow -gt $deadline) { throw "Timed out stopping $($definition.Service)." }
        Start-Sleep -Milliseconds 200
    }
    Remove-Item -LiteralPath (Join-Path $atsRoot "run/$($definition.Service).pid")
    Write-Host "STOPPED $($definition.Service), PID $($process.ProcessId)"
}

if ($Action -eq 'show') {
    $selected | ForEach-Object { Get-ServiceState $_ } | Format-Table -AutoSize
    Write-Host 'LISTENING means the process owns its port; it does not confirm business data readiness.'
    exit 0
}
if ($Action -eq 'log') {
    if ($selected.Count -ne 1) { throw 'Choose one service, for example: log.cmd QuoteSvr' }
    $logFile = Join-Path $atsRoot "log/$Service.console.log"
    Get-Content -LiteralPath $logFile -Encoding UTF8 -Tail 80 -Wait
    exit 0
}
# Only one lifecycle command can change this environment at a time.
$lockPath = Join-Path $atsRoot 'run/process-control.lock'
$lock = $null
try {
    try {
        $lock = [IO.File]::Open($lockPath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    } catch [IO.IOException] {
        throw 'Another process-control command is running for this environment. Wait for it to finish.'
    }
    if ($Action -in @('stop', 'restart')) {
        $reverse = @($selected)
        [Array]::Reverse($reverse)
        foreach ($definition in $reverse) { Stop-ServiceProcess $definition }
    }
    if ($Action -in @('start', 'restart')) {
        if ($Service -eq 'all' -and !(Get-NetTCPConnection -LocalPort 55432 -State Listen -ErrorAction SilentlyContinue)) {
            & (Join-Path (Split-Path $environmentRoot -Parent) 'ficc/iteration1/scripts/start-postgres.ps1')
        }
        $startFailures = @()
        foreach ($definition in $selected) {
            try {
                Start-ServiceProcess $definition
            } catch {
                if ($Service -ne 'all') { throw }
                $startFailures += "$($definition.Service): $($_.Exception.Message)"
                Write-Warning $startFailures[-1]
            }
        }
        if ($startFailures.Count -gt 0) {
            throw ("Environment startup incomplete:`n" + ($startFailures -join "`n"))
        }
    }
} finally {
    if ($lock) { $lock.Dispose() }
}
Write-Host 'Shared PostgreSQL and STC client are not stopped by these commands.'

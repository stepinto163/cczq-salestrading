param(
    [string]$Action = 'status',
    [string]$StcConfig,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'resolve-command-name.ps1')
$Action = Resolve-CommandName $Action @('start', 'stop', 'restart', 'status') -Label 'action'
$iterationRoot = Split-Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path $iterationRoot -Parent
$clientRoot = Join-Path $iterationRoot 'stc-client/stc-client'
$outputRoot = Join-Path $clientRoot 'runtime/native-client'
$executable = [IO.Path]::GetFullPath((Join-Path $outputRoot 'STC.exe'))
if (!$StcConfig) {
    $StcConfig = Join-Path (Split-Path $repositoryRoot -Parent) 'qa_env/stc-client/STC.exe.config'
}

function Get-ClientProcesses {
    # Match the complete executable path, not every STC.exe on this machine.
    Get-CimInstance Win32_Process -Filter "Name='STC.exe'" | Where-Object {
        $_.ExecutablePath -and [string]::Equals(
            [IO.Path]::GetFullPath($_.ExecutablePath), $executable,
            [StringComparison]::OrdinalIgnoreCase)
    }
}

function Show-ClientStatus {
    $clients = @(Get-ClientProcesses)
    if ($clients.Count -eq 0) {
        Write-Host "STOPPED STC client: $executable"
        return
    }
    foreach ($client in $clients) {
        Write-Host "RUNNING STC client, PID $($client.ProcessId): $executable"
    }
    Write-Host 'RUNNING indicates a process only; it does not confirm login or backend readiness.'
}

function Stop-Client {
    foreach ($client in @(Get-ClientProcesses)) {
        $process = Get-Process -Id $client.ProcessId -ErrorAction SilentlyContinue
        if (!$process) {
            continue
        }
        if ($Force) {
            # Explicit opt-in; normal stop first requests a graceful window close.
            Stop-Process -InputObject $process -ErrorAction Stop
        } else {
            $null = $process.CloseMainWindow()
            if (!$process.WaitForExit(10000)) {
                throw "STC client PID $($process.Id) is still open. Complete any close prompt, or explicitly use -Force."
            }
        }
        if (!$process.WaitForExit(5000)) {
            throw "Timed out stopping STC client PID $($process.Id)."
        }
        Write-Host "STOPPED STC client, PID $($process.Id)"
    }
}

function Start-Client {
    if (@(Get-ClientProcesses).Count -gt 0) {
        Write-Host 'ALREADY RUNNING; a second client was not launched.'
        Show-ClientStatus
        return
    }
    & (Join-Path $clientRoot 'scripts/start-native-client.ps1') -Mode Stc -StcConfig $StcConfig
    Start-Sleep -Seconds 2
    if (@(Get-ClientProcesses).Count -eq 0) {
        throw "STC client exited during startup. Inspect $outputRoot/logs."
    }
    Show-ClientStatus
}

if ($Action -eq 'status') {
    Show-ClientStatus
    exit 0
}
if (!(Test-Path -LiteralPath $executable)) {
    throw "Client is not built. Run $clientRoot/scripts/build-native-client.ps1 first."
}
if ($Action -in @('start', 'restart')) {
    # Validate before restart closes the current application.
    $configuration = New-Object System.Xml.XmlDocument
    $configuration.Load([IO.Path]::GetFullPath($StcConfig))
}
$lock = $null
try {
    try {
        $lock = [IO.File]::Open((Join-Path $outputRoot 'client-control.lock'),
            [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    } catch [IO.IOException] {
        throw 'Another STC client management command is running.'
    }
    if ($Action -in @('stop', 'restart')) {
        Stop-Client
    }
    if ($Action -in @('start', 'restart')) {
        Start-Client
    } elseif ($Action -eq 'stop') {
        Show-ClientStatus
    }
} finally {
    if ($lock) {
        $lock.Dispose()
    }
}

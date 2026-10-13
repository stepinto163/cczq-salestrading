param(
    [string]$Action = 'status',
    [string]$StcConfig,
    [switch]$QTrade,
    [string]$QTradeUser = 'adrian'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'resolve-command-name.ps1')
$Action = Resolve-CommandName $Action @('start', 'stop', 'restart', 'status', 'build', 'report') -Label 'action'
$iterationRoot = Split-Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path $iterationRoot -Parent
$clientRoot = Join-Path $iterationRoot 'stc-client/stc-client'
$runtime = Join-Path $clientRoot 'runtime/ui-test-client'
$executable = [IO.Path]::GetFullPath((Join-Path $runtime 'SalesTradingUiTests.exe'))
$workerExecutable = [IO.Path]::GetFullPath((Join-Path $runtime 'UiScenarioWorker.exe'))
$reports = Join-Path $runtime 'ui-test-reports'
if (!$StcConfig) {
    $StcConfig = Join-Path (Split-Path $repositoryRoot -Parent) 'qa_env/stc-client/STC.exe.config'
}

function Get-TestProcesses {
    Get-CimInstance Win32_Process -Filter "Name='SalesTradingUiTests.exe' OR Name='UiScenarioWorker.exe'" |
        Where-Object {
            $_.ExecutablePath -and (
                [string]::Equals($_.ExecutablePath, $executable, [StringComparison]::OrdinalIgnoreCase) -or
                [string]::Equals($_.ExecutablePath, $workerExecutable, [StringComparison]::OrdinalIgnoreCase))
        }
}

function Show-TestStatus {
    $processes = @(Get-TestProcesses)
    if ($processes.Count -eq 0) {
        Write-Host "STOPPED UI test runner: $executable"
    }
    foreach ($process in $processes) {
        Write-Host "RUNNING $($process.Name), PID $($process.ProcessId)"
    }
    Write-Host 'Process status does not indicate test success or backend readiness.'
    Write-Host "Reports: $reports"
}

function Start-TestRunner {
    if (@(Get-TestProcesses).Count -gt 0) {
        Write-Host 'ALREADY RUNNING; a second runner was not launched.'
        if ($QTrade) {
            Write-Host 'Use autotest.cmd restart -QTrade to apply QTrade settings to the existing runner.'
        }
        Show-TestStatus
        return
    }
    $previousEnvironment = @{}
    try {
        if ($QTrade) {
            # 复用部署生成的共享凭据，不要求手工创建文件或输入 token。
            $authPath = Join-Path (Split-Path $repositoryRoot -Parent) 'qa_env/ats_root/control/QtIntakeSvrAuth.properties'
            $auth = @{}
            foreach ($line in Get-Content -LiteralPath $authPath -Encoding UTF8) {
                if ($line -match '^\s*([^#!\s][^=]*)=(.*)$') {
                    $auth[$matches[1].Trim()] = $matches[2].Trim()
                }
            }
            if ([string]::IsNullOrWhiteSpace($auth['intake.token'])) {
                throw "Missing intake.token in shared configuration: $authPath"
            }
            foreach ($name in @('STC_UI_QTRADE_PUBLISH', 'STC_UI_INTAKE_TOKEN', 'STC_UI_USER')) {
                $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
            }
            $env:STC_UI_QTRADE_PUBLISH = '1'
            $env:STC_UI_INTAKE_TOKEN = $auth['intake.token']
            $env:STC_UI_USER = $QTradeUser
            Write-Host "QTrade publishing tests enabled; receiver login: $QTradeUser. Selecting cases 24-26 sends real test intentions."
        }
        & (Join-Path $clientRoot 'scripts/start-ui-tests.ps1') -StcConfig $StcConfig
    } finally {
        foreach ($name in $previousEnvironment.Keys) {
            [Environment]::SetEnvironmentVariable($name, $previousEnvironment[$name], 'Process')
        }
    }
    Start-Sleep -Seconds 2
    if (@(Get-TestProcesses).Count -eq 0) {
        throw "UI test runner exited during startup: $executable"
    }
    Show-TestStatus
}

function Stop-TestRunner {
    $processes = @(Get-TestProcesses)
    $runners = @($processes | Where-Object { $_.Name -eq 'SalesTradingUiTests.exe' })
    if (@($runners | Where-Object { $_.CommandLine -match '--run\s' }).Count -gt 0) {
        throw 'A command-line suite is running. Wait for its report; this command closes the interactive test runner only.'
    }
    foreach ($runner in $runners) {
        $process = Get-Process -Id $runner.ProcessId -ErrorAction SilentlyContinue
        if (!$process) {
            continue
        }
        Write-Host "STOPPING UI test runner PID $($process.Id); requesting stop and cleanup."
        # During a case, FormClosing cancels the close and requests cleanup.
        # Retry the close after cleanup; never kill the worker or normal STC.
        $deadline = [DateTime]::UtcNow.AddSeconds(45)
        do {
            $null = $process.CloseMainWindow()
            if ($process.WaitForExit(1000)) {
                break
            }
            $process.Refresh()
        } while ([DateTime]::UtcNow -lt $deadline)
        if (!$process.HasExited) {
            throw 'Runner is still open. Check its cleanup status or close prompt; no process was forcibly terminated.'
        }
    }
    if (@(Get-TestProcesses).Count -gt 0) {
        throw 'A test worker is still running. Inspect its report and cleanup status; it was not forcibly terminated.'
    }
    Show-TestStatus
}

if ($Action -eq 'status') {
    Show-TestStatus
    exit 0
}
if ($Action -eq 'report') {
    $latest = $null
    if (Test-Path -LiteralPath $reports) {
        $latest = Get-ChildItem -LiteralPath $reports -Directory | Sort-Object Name -Descending | Select-Object -First 1
    }
    if (!$latest) {
        Write-Host "No reports yet: $reports"
        exit 0
    }
    Write-Host "Latest report: $($latest.FullName)"
    $summary = Join-Path $latest.FullName 'summary.json'
    if (Test-Path -LiteralPath $summary) {
        $results = Get-Content -LiteralPath $summary -Raw -Encoding UTF8 | ConvertFrom-Json
        $results | Select-Object caseId, status, cleanupPassed | Format-Table -AutoSize
    } else {
        Write-Host 'Summary is not available yet; the run may be active or incomplete.'
    }
    exit 0
}

if ($Action -in @('start', 'restart')) {
    if (!(Test-Path -LiteralPath $executable)) {
        throw 'UI tests are not built. Run autotest.cmd build first.'
    }
    # Validate before closing a currently open runner.
    $StcConfig = [IO.Path]::GetFullPath($StcConfig)
    $configuration = New-Object System.Xml.XmlDocument
    $configuration.Load($StcConfig)
}
New-Item -ItemType Directory -Path $runtime -Force | Out-Null
$lock = $null
try {
    try {
        $lock = [IO.File]::Open((Join-Path $runtime 'ui-test-control.lock'),
            [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    } catch [IO.IOException] {
        throw 'Another UI test management command is running.'
    }
    if ($Action -eq 'build') {
        if (@(Get-TestProcesses).Count -gt 0) {
            throw 'Close the UI test runner and finish cleanup before building.'
        }
        & (Join-Path $clientRoot 'scripts/build-ui-tests.ps1')
    }
    if ($Action -in @('stop', 'restart')) {
        Stop-TestRunner
    }
    if ($Action -in @('start', 'restart')) {
        Start-TestRunner
    }
} finally {
    if ($lock) {
        $lock.Dispose()
    }
}

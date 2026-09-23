param([switch]$CheckOnly)

$ErrorActionPreference = 'Stop'
$iteration = Split-Path $PSScriptRoot -Parent
$run = (Get-Content "$iteration/runtime/qtintake-prl-current.txt" -Raw).Trim()
$settings = Get-Content "$run/run.json" -Raw | ConvertFrom-Json
$java = "$iteration/runtime/tools/jdk8/jdk8u504-b01/bin/java.exe"
$psql = "$iteration/runtime/tools/postgresql/pgsql/bin/psql.exe"
$intakeFolder = "$run/stc/QtIntake"
$quoteFolder = "$run/stc/QuoteSvr"
$intakeJar = 'cgws-ficc-saletrading-qtintake-0.1.0-SNAPSHOT.jar'
$quoteJar = 'cgws-ficc-saletrading-quotesvr-0.1.0-SNAPSHOT.jar'
$ownedProcesses = @()
$oldAtsRoot = $env:ATS_ROOT
$oldPgPassword = $env:PGPASSWORD
$mutex = New-Object Threading.Mutex($false, 'Local\FiccQtIntakePrlTest')
$locked = $false

function Get-FixReceiptStatus([string]$line) {
    if ($line -notmatch 'FIX RECEIVE .*body=.*\|35=([^|]+)\|') { return }
    $messageType = $matches[1]
    if ($messageType -in @('0', '1', '2', '4', '5', 'A')) { return }
    if ($messageType -ne 'S') {
        return "OTHER MESSAGE - FIX type=$messageType; not the expected quote. Still waiting."
    }
    $security = '(missing)'
    if ($line -match '\|48=([^|]+)\|') { $security = $matches[1] }
    if ($security -in @('240210', '240210.IB')) {
        return 'EXPECTED QUOTE RECEIVED - 240210.IB, FIX type=S. QuoteSvr acceptance is not yet confirmed.'
    }
    return "OTHER QUOTE RECEIVED - security=$security, FIX type=S; expected 240210.IB."
}

function Start-TestService($folder, $jar, $name) {
    $process = Start-Process -FilePath $java -WorkingDirectory $folder -WindowStyle Hidden -PassThru `
        -ArgumentList @('-Xms32m', '-Xmx192m', '-XX:ActiveProcessorCount=2', '-Dfile.encoding=UTF-8', '-jar', $jar) `
        -RedirectStandardOutput "$script:output/$name.log" -RedirectStandardError "$script:output/$name.err"
    # Independent watchdog: closing this console must not leave the test receiver running.
    $ticks = $process.StartTime.ToUniversalTime().Ticks
    $watchdog = "Start-Sleep -Seconds 310; `$p = Get-Process -Id $($process.Id) -ErrorAction SilentlyContinue; if (`$p -and `$p.StartTime.ToUniversalTime().Ticks -eq $ticks) { Stop-Process -Id `$p.Id }"
    $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($watchdog))
    Start-Process powershell.exe -WindowStyle Hidden -ArgumentList @('-NoProfile', '-EncodedCommand', $encoded) | Out-Null
    return $process
}

try {
    $locked = $mutex.WaitOne(0)
    if (-not $locked) { throw 'Another PRL test is already running.' }
    foreach ($path in @($java, $psql, "$intakeFolder/$intakeJar", "$quoteFolder/$quoteJar", "$run/control/QtIntakeAuth.properties")) {
        if (-not (Test-Path -LiteralPath $path)) { throw "Missing test file: $path" }
    }
    if ($settings.userSchema -notmatch '^ficc_qtprl_\d{8}_\d{6}_users$') {
        throw 'Refusing to modify a user table outside the dedicated local test schema.'
    }
    $otherReceiver = Get-CimInstance Win32_Process | Where-Object {
        $_.Name -eq 'java.exe' -and $_.CommandLine -match 'saletrading-qtintake.*\.jar|cn\.com\.greatwall\.qtrade\.receiver\.PrlFixReceiver'
    }
    if ($otherReceiver) { throw 'A QTrade receiver is already running. Stop it before this test.' }
    $database = @{}
    Get-Content "$iteration/runtime/quotesvr.local.properties" | ForEach-Object {
        if ($_ -match '^(db\.[^=]+)=(.*)$') { $database[$matches[1]] = $matches[2] }
    }
    if ($database['db.url'] -ne 'jdbc:postgresql://127.0.0.1:55432/ficc_quote') {
        throw 'This script only supports the prepared local PostgreSQL test database.'
    }
    $env:PGPASSWORD = $database['db.password']
    $probe = "SELECT count(*) FROM $($settings.userSchema).stc_tbook_users;"
    $null = $probe | & $psql -X -h 127.0.0.1 -p 55432 -U $database['db.user'] -d ficc_quote -v ON_ERROR_STOP=1 -At
    if ($LASTEXITCODE -ne 0) { throw 'Local test user table is unavailable.' }
    if (-not (Get-NetTCPConnection -LocalPort 12181 -State Listen -ErrorAction SilentlyContinue)) {
        throw 'Local ZooKeeper is not running on port 12181.'
    }
    if ($CheckOnly) {
        Write-Host 'Preflight passed. No FIX connection started.'
        return
    }

    $script:output = "$run/manual-$(Get-Date -Format yyyyMMdd-HHmmss)"
    New-Item -ItemType Directory -Path $output | Out-Null
    $env:ATS_ROOT = $run
    $listener = Get-NetTCPConnection -LocalPort 18680 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($listener) {
        $expectedPid = [int](Get-Content "$run/quotesvr.pid" -Raw).Trim()
        $processInfo = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)"
        if ($listener.OwningProcess -ne $expectedPid -or $processInfo.CommandLine -notlike '*saletrading-quotesvr*.jar*') {
            throw 'Port 18680 is occupied by a service other than the prepared local QuoteSvr.'
        }
        Write-Host 'Using the already running local test QuoteSvr.'
    } else {
        $quote = Start-TestService $quoteFolder $quoteJar 'quotesvr'
        $ownedProcesses += $quote
        Set-Content "$run/quotesvr.pid" $quote.Id
    }
    # Skip old payloads: only report messages received during this run.
    $messageLogOffsets = @{}
    foreach ($file in @(Get-ChildItem "$run/log" -Filter 'QtIntake-dps*' -File -ErrorAction SilentlyContinue)) {
        $messageLogOffsets[$file.FullName] = @(Get-Content $file.FullName -Encoding UTF8).Count
    }
    $receiver = Start-TestService $intakeFolder $intakeJar 'qtintake'
    $ownedProcesses += $receiver
    $deadline = [DateTime]::UtcNow.AddMinutes(5)
    $ready = $false
    $mappedAccounts = @{}
    $consoleLineOffset = 0
    Write-Host "Logs: $output"
    Write-Host 'Waiting for FIX login and local QuoteSvr connection. Timeout: 5 minutes.'
    while ([DateTime]::UtcNow -lt $deadline) {
        $receiver.Refresh()
        if ($receiver.HasExited) { throw "QtIntake exited. See $output/qtintake.err" }
        $lines = @(Get-Content "$output/qtintake.log" -ErrorAction SilentlyContinue)
        $fixState = $lines | Where-Object { $_ -match 'FIX state=(CONNECTED|DISCONNECTED|FAILED)' } | Select-Object -Last 1
        $backendState = $lines | Where-Object { $_ -match 'Host:127\.0\.0\.1,Port:18680:CONNECTION_' } | Select-Object -Last 1
        $connected = $fixState -match 'FIX state=CONNECTED ' -and $backendState -match 'CONNECTION_LOGIN_SUCCESS'
        if ($connected -and -not $ready) {
            Write-Host 'READY - send ONE quote for 240210.IB now.' -ForegroundColor Green
            $ready = $true
        } elseif (-not $connected -and $ready) {
            Write-Host 'Connection lost. HOLD sending until READY appears again.' -ForegroundColor Yellow
            $ready = $false
        }
        foreach ($file in @(Get-ChildItem "$run/log" -Filter 'QtIntake-dps*' -File -ErrorAction SilentlyContinue)) {
            $messageLines = @(Get-Content $file.FullName -Encoding UTF8)
            $offset = [int]$messageLogOffsets[$file.FullName]
            if ($offset -gt $messageLines.Count) { $offset = 0 }
            for ($index = $offset; $index -lt $messageLines.Count; $index++) {
                $status = Get-FixReceiptStatus $messageLines[$index]
                if ($status) {
                    $color = if ($status.StartsWith('EXPECTED QUOTE RECEIVED')) { 'Green' } else { 'Yellow' }
                    Write-Host $status -ForegroundColor $color
                    Add-Content "$output/message-status.txt" "$(Get-Date -Format HH:mm:ss) $status"
                }
            }
            $messageLogOffsets[$file.FullName] = $messageLines.Count
        }
        for ($index = $consoleLineOffset; $index -lt $lines.Count; $index++) {
            if ($lines[$index] -match 'FIX state=BUFFERED') {
                Write-Host 'QUOTE BUFFERED - saved locally; waiting for QuoteSvr response.' -ForegroundColor Cyan
            } elseif ($lines[$index] -match 'FIX state=REJECTED') {
                Write-Host 'QUOTE REJECTED DURING MAPPING - see QtIntake log and inbox/rejected.' -ForegroundColor Red
            }
        }
        $consoleLineOffset = $lines.Count
        # Only the disposable local STC-user fixture is changed. Production owner lookup remains unchanged.
        foreach ($file in @(Get-ChildItem "$intakeFolder/data/inbox/pending" -Filter '*.json' -ErrorAction SilentlyContinue)) {
            $record = Get-Content $file.FullName -Raw | ConvertFrom-Json
            $account = [string]$record.request.intake.sourceUserId
            if ($account -and -not $mappedAccounts.ContainsKey($account)) {
                $sql = "INSERT INTO $($settings.userSchema).stc_tbook_users(userid, qtAccount, enable) VALUES ('U-001', :'account', 1) ON CONFLICT (qtAccount) DO NOTHING;"
                $null = $sql | & $psql -X -h 127.0.0.1 -p 55432 -U $database['db.user'] -d ficc_quote -v ON_ERROR_STOP=1 -v "account=$account"
                if ($LASTEXITCODE -ne 0) { throw 'Could not populate the local STC-user fixture.' }
                $mappedAccounts[$account] = $true
                Write-Host 'Received quote; mapped its QTrade account to local simulated user U-001.'
            }
        }
        $result = $lines | Where-Object { $_ -match 'FORWARD state=(SENT|REJECTED)' } | Select-Object -Last 1
        if ($result) {
            if ($result -match 'FORWARD state=SENT') {
                Write-Host 'QUOTESVR ACCEPTED - quote creation succeeded.' -ForegroundColor Green
            } else {
                Write-Host 'QUOTESVR REJECTED - quote creation failed; see the reply below.' -ForegroundColor Red
            }
            Write-Host $result
            Set-Content "$output/result.txt" $result
            Write-Host "Request and reply: $intakeFolder/data/inbox (sent or rejected folder)"
            break
        }
        Start-Sleep -Seconds 1
    }
    if (-not $result) {
        Write-Host 'TIMEOUT - no completed quote forwarding. Share the log folder for diagnosis.' -ForegroundColor Yellow
        Set-Content "$output/result.txt" 'TIMEOUT: no completed quote forwarding.'
    }
} finally {
    foreach ($process in $ownedProcesses) {
        $process.Refresh()
        if (-not $process.HasExited) { Stop-Process -Id $process.Id -ErrorAction SilentlyContinue }
    }
    $env:ATS_ROOT = $oldAtsRoot
    $env:PGPASSWORD = $oldPgPassword
    if ($locked) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}

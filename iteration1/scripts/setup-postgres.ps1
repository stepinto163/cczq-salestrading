param([switch]$Resume)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$runtimePath = Join-Path $taskRoot 'runtime'
$pgBin = Join-Path $runtimePath 'tools/postgresql/pgsql/bin'
$dataPath = Join-Path $runtimePath 'postgres'
$localConfig = Join-Path $runtimePath 'quotesvr.local.properties'
if ((Test-Path -LiteralPath (Join-Path $dataPath 'PG_VERSION')) -and !$Resume) { throw 'Project cluster already exists; use start-postgres.ps1.' }
New-Item -ItemType Directory -Force -Path $runtimePath | Out-Null
function New-LocalPassword {
    $bytes = New-Object byte[] 24
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes); return [BitConverter]::ToString($bytes).Replace('-','').ToLowerInvariant() } finally { $rng.Dispose() }
}
$adminPassword = New-LocalPassword
$appPassword = New-LocalPassword
$pwFile = Join-Path $runtimePath 'init-password.tmp'
$sqlFile = Join-Path $runtimePath 'provision.tmp.sql'
try {
    if ($Resume) {
        if (Test-Path -LiteralPath $localConfig) { throw 'Provisioning already complete.' }
        $adminPassword = [IO.File]::ReadAllText($pwFile).Trim()
    } else {
    [IO.File]::WriteAllText($pwFile,$adminPassword,[Text.Encoding]::ASCII)
    & (Join-Path $pgBin 'initdb.exe') -D $dataPath -U ficc_admin --encoding=UTF8 --locale=C --auth-host=scram-sha-256 --auth-local=scram-sha-256 --pwfile=$pwFile > (Join-Path $runtimePath 'initdb.log') 2>&1
    if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL initdb failed; see runtime/initdb.log.' }
    Add-Content -LiteralPath (Join-Path $dataPath 'postgresql.conf') -Encoding ASCII -Value @"
listen_addresses = '127.0.0.1'
port = 55432
max_connections = 16
shared_buffers = '32MB'
work_mem = '2MB'
maintenance_work_mem = '16MB'
max_worker_processes = 2
max_parallel_workers = 0
autovacuum_max_workers = 1
timezone = 'Asia/Shanghai'
"@
    $pgArgs = '-D "' + $dataPath + '" -l "' + (Join-Path $runtimePath 'postgres.log') + '" -w -t 15 start'
    $pgStart = Start-Process -FilePath (Join-Path $pgBin 'pg_ctl.exe') -ArgumentList $pgArgs -WindowStyle Hidden -PassThru
    if (!$pgStart.WaitForExit(20000)) { throw 'pg_ctl did not complete within 20 seconds.' }
    if ($pgStart.ExitCode -ne 0) { throw 'Project PostgreSQL start failed; see runtime/postgres.log.' }
    }
    [IO.File]::WriteAllText($sqlFile,"CREATE ROLE ficc_quote LOGIN PASSWORD '$appPassword';`nCREATE DATABASE ficc_quote OWNER ficc_quote;`n",[Text.Encoding]::ASCII)
    $previousPgPassword = $env:PGPASSWORD
    $env:PGPASSWORD = $adminPassword
    try {
        & (Join-Path $pgBin 'psql.exe') -X -h 127.0.0.1 -p 55432 -U ficc_admin -d postgres -v ON_ERROR_STOP=1 -f $sqlFile > (Join-Path $runtimePath 'provision.log') 2>&1
        if ($LASTEXITCODE -ne 0) { throw 'Database provisioning failed; see local provision.log.' }
    } finally { $env:PGPASSWORD = $previousPgPassword }
    [IO.File]::WriteAllText((Join-Path $runtimePath 'postgres-admin.local.properties'),"host=127.0.0.1`nport=55432`nuser=ficc_admin`npassword=$adminPassword`n",[Text.Encoding]::UTF8)
    [IO.File]::WriteAllText($localConfig,"db.url=jdbc:postgresql://127.0.0.1:55432/ficc_quote`ndb.user=ficc_quote`ndb.password=$appPassword`ndb.schema=ficc_demo`nlisten.port=18680`n",(New-Object Text.UTF8Encoding($false)))
    Write-Output 'Project PostgreSQL ready: 127.0.0.1:55432 / ficc_quote. Credentials saved locally; not printed.'
} finally {
    foreach ($tempPath in @($pwFile,$sqlFile)) { if (Test-Path -LiteralPath $tempPath) { Remove-Item -LiteralPath $tempPath } }
}

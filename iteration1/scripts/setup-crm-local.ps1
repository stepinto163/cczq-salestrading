$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$runtimePath = Join-Path $taskRoot 'runtime'
$admin = Get-Content (Join-Path $runtimePath 'postgres-admin.local.properties') -Raw | ConvertFrom-StringData
if ($admin.host -ne '127.0.0.1' -or $admin.port -ne '55432') {
    throw 'CRM fixture setup is restricted to the local project PostgreSQL instance.'
}
$psql = Join-Path $runtimePath 'tools/postgresql/pgsql/bin/psql.exe'
$previousPassword = $env:PGPASSWORD
$env:PGPASSWORD = $admin.password
try {
    $exists = & $psql -X -h 127.0.0.1 -p 55432 -U $admin.user -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='ficc_ibor'"
    if ($LASTEXITCODE -ne 0) { throw 'Cannot connect to local PostgreSQL.' }
    if ($exists -ne '1') {
        & $psql -X -h 127.0.0.1 -p 55432 -U $admin.user -d postgres -v ON_ERROR_STOP=1 -c 'CREATE DATABASE ficc_ibor OWNER ficc_quote'
        if ($LASTEXITCODE -ne 0) { throw 'CRM database creation failed.' }
    }
    & $psql -X -h 127.0.0.1 -p 55432 -U $admin.user -d ficc_ibor -v ON_ERROR_STOP=1 -f (Join-Path $taskRoot 'sql/fixtures/crm-local.sql')
    if ($LASTEXITCODE -ne 0) { throw 'CRM fixture setup failed.' }
    & $psql -X -h 127.0.0.1 -p 55432 -U $admin.user -d ficc_ibor -v ON_ERROR_STOP=1 -c 'GRANT USAGE ON SCHEMA public TO ficc_quote; GRANT SELECT ON public.crm_org, public.crm_contact, public.crm_contact_org_rel TO ficc_quote;'
    if ($LASTEXITCODE -ne 0) { throw 'CRM read grants failed.' }
} finally {
    $env:PGPASSWORD = $previousPassword
}
$database = Get-Content (Join-Path $runtimePath 'quotesvr.local.properties') -Raw | ConvertFrom-StringData
$configuration = "crm.mode=postgresql`ncrm.jdbcUrl=jdbc:postgresql://127.0.0.1:55432/ficc_ibor`ncrm.schema=public`ncrm.username=ficc_quote`ncrm.password=$($database['db.password'])`n"
[IO.File]::WriteAllText((Join-Path $runtimePath 'crm.local.properties'), $configuration, [Text.UTF8Encoding]::new($false))
Write-Output 'CRM fixture ready: local ficc_ibor / public. Credentials are in runtime/crm.local.properties.'

$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$dataPath = Join-Path $taskRoot 'runtime/postgres'
$pgBin = Join-Path $taskRoot 'runtime/tools/postgresql/pgsql/bin'
if (!(Test-Path -LiteralPath (Join-Path $dataPath 'PG_VERSION'))) { throw 'Run setup-postgres.ps1 first.' }
& (Join-Path $pgBin 'pg_ctl.exe') -D $dataPath status | Out-Null
if ($LASTEXITCODE -eq 0) { Write-Output 'Project PostgreSQL is running.'; exit 0 }
$pgArgs = '-D "' + $dataPath + '" -l "' + (Join-Path $taskRoot 'runtime/postgres.log') + '" -w -t 15 start'
$pgStart = Start-Process -FilePath (Join-Path $pgBin 'pg_ctl.exe') -ArgumentList $pgArgs -WindowStyle Hidden -PassThru
if (!$pgStart.WaitForExit(20000)) { throw 'pg_ctl did not complete within 20 seconds.' }
if ($pgStart.ExitCode -ne 0) { throw 'PostgreSQL start failed.' }

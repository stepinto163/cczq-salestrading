$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$dataPath=Join-Path $taskRoot 'runtime/postgres'
$pgCtl=Join-Path $taskRoot 'runtime/tools/postgresql/pgsql/bin/pg_ctl.exe'
if(!(Test-Path -LiteralPath (Join-Path $dataPath 'PG_VERSION'))){throw 'Project PostgreSQL data directory was not found.'}
& $pgCtl -D $dataPath status | Out-Null
if($LASTEXITCODE -ne 0){Write-Output 'Project PostgreSQL is already stopped.';exit 0}
# Stop only this dedicated cluster; fast mode rolls back unfinished transactions cleanly.
$arguments='-D "'+$dataPath+'" -m fast -w -t 15 stop'
$p=Start-Process -FilePath $pgCtl -ArgumentList $arguments -WindowStyle Hidden -PassThru
$null=$p.Handle
if(!$p.WaitForExit(20000)){throw 'PostgreSQL shutdown did not complete within 20 seconds.'}
if($p.ExitCode -ne 0){throw 'Project PostgreSQL shutdown failed.'}
Write-Output 'Project PostgreSQL stopped.'

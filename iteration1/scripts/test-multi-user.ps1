param([string]$ExecutablePath)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
if(!$ExecutablePath){$ExecutablePath=Join-Path $taskRoot 'client/bin/MultiUser/Ficc.QuotePrototype.exe'}
$exe=(Resolve-Path -LiteralPath $ExecutablePath).Path
$runDirectory=Join-Path $taskRoot ('runtime/multi-user-runs/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$testArgs='--autotest-multi-user-panel "'+$runDirectory+'/summary.json"'
$process=Start-Process -FilePath $exe -ArgumentList $testArgs -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$runDirectory/process.log" -RedirectStandardError "$runDirectory/process.err"
$null=$process.Handle
if(!$process.WaitForExit(35000)){Stop-Process -Id $process.Id;throw "Scenario supervisor stopped after 35 seconds; records: $runDirectory"}
Get-Content "$runDirectory/process.err" -Tail 12 -Encoding UTF8
$result=Get-Content "$runDirectory/summary.json" -Raw -Encoding UTF8 | ConvertFrom-Json
$result | Select-Object result,checks,quotesCreated,failedStep,error,messageLogDirectory
Write-Output "Scenario report: $runDirectory/summary.json"
if($process.ExitCode -ne 0){throw 'Multi-user scenario failed; inspect the report.'}

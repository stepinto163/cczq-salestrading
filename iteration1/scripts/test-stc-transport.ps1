$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$classpath= & (Join-Path $PSScriptRoot 'foundation-classpath.ps1')
$testOut=Join-Path $taskRoot 'runtime/compat-test-classes'
New-Item -ItemType Directory -Path $testOut -Force | Out-Null
& javac '-J-Xmx128m' '-J-XX:ActiveProcessorCount=2' '-J-Duser.language=en' -proc:none -encoding UTF-8 -cp $classpath -d $testOut "$taskRoot/compat/tests/StcTransportSmoke.java"
if($LASTEXITCODE -ne 0){throw 'Transport test compilation failed'}
$javaArgs='-Xms24m -Xmx96m -XX:ActiveProcessorCount=2 -Dfile.encoding=UTF-8 -cp "'+$testOut+';'+$classpath+'" StcTransportSmoke "'+$taskRoot+'/runtime/stc-transport-smoke.json"'
$p=Start-Process java -ArgumentList $javaArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/stc-transport-smoke.log" -RedirectStandardError "$taskRoot/runtime/stc-transport-smoke.err"
$null=$p.Handle
if(!$p.WaitForExit(30000)){Stop-Process -Id $p.Id;throw 'DPS functional test stopped after 30 seconds'}
Get-Content "$taskRoot/runtime/stc-transport-smoke.log" -Tail 18
Get-Content "$taskRoot/runtime/stc-transport-smoke.err" -Tail 8
if($p.ExitCode -ne 0){throw "Transport test exit $($p.ExitCode)"}

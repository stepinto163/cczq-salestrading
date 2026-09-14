$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$gwPath=Join-Path $taskRoot 'gateway'
$pidFile=Join-Path $taskRoot 'runtime/gateway.pid'
if(Test-Path $pidFile){$priorId=[int](Get-Content $pidFile);if(Get-Process -Id $priorId -ErrorAction SilentlyContinue){throw 'Recorded GW process is still running.'}}
$classpath="$taskRoot/runtime/foundation-build/lib/gw-local.jar;"+(& (Join-Path $PSScriptRoot 'foundation-classpath.ps1'))
New-Item -ItemType Directory -Force "$gwPath/out" | Out-Null
& javac '-J-Xmx128m' '-J-XX:ActiveProcessorCount=2' -proc:none -encoding UTF-8 -cp $classpath -d "$gwPath/out" "$gwPath/src/ficc/gateway/FiccSecurityCheck.java"
if($LASTEXITCODE -ne 0){throw 'GW extension compilation failed'}
if(!(Test-Path "$gwPath/control/server.lic")){Copy-Item -LiteralPath "$taskRoot/cczq-app-salestrading-quotesvr/control/server.lic" -Destination "$gwPath/control/server.lic"}
$arguments='-Xms32m -Xmx192m -XX:ActiveProcessorCount=2 -Dfile.encoding=UTF-8 -cp "'+$gwPath+'/out;'+$classpath+'" GW'
$oldAtsRoot=$env:ATS_ROOT
try{
    $env:ATS_ROOT=$gwPath
    $p=Start-Process java -ArgumentList $arguments -WorkingDirectory $gwPath -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/gateway.log" -RedirectStandardError "$taskRoot/runtime/gateway.err"
    $p.Id | Set-Content "$taskRoot/runtime/gateway.pid"
    Write-Output "GW startup requested; PID $($p.Id)."
}finally{$env:ATS_ROOT=$oldAtsRoot}

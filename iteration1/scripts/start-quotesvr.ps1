$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$serverPath=Join-Path $taskRoot 'cgws-ficc-saletrading-quotesvr'
$pidFile=Join-Path $taskRoot 'runtime/quotesvr.pid'
if(Test-Path $pidFile){$priorId=[int](Get-Content $pidFile);if(Get-Process -Id $priorId -ErrorAction SilentlyContinue){throw 'Recorded QuoteSvr process is still running.'}}
if(!(Test-Path "$serverPath/control/server.lic")){throw 'A valid STC server.lic is required under cgws-ficc-saletrading-quotesvr/control.'}
$classpath= & (Join-Path $PSScriptRoot 'server-classpath.ps1') -Server quotesvr
$arguments='-Xms48m -Xmx256m -XX:ActiveProcessorCount=2 -Dfile.encoding=UTF-8 -cp "'+$classpath+'" ficc.quote.Main'
$oldAtsRoot=$env:ATS_ROOT
try{
    $env:ATS_ROOT=$serverPath
    $p=Start-Process java -ArgumentList $arguments -WorkingDirectory $serverPath -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/quotesvr.log" -RedirectStandardError "$taskRoot/runtime/quotesvr.err"
    $p.Id | Set-Content $pidFile
    Write-Output "QuoteSvr startup requested; PID $($p.Id). Inspect runtime/quotesvr.log for READY."
}finally{$env:ATS_ROOT=$oldAtsRoot}

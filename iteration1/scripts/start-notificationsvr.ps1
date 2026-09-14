$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$serverPath=Join-Path $taskRoot 'cczq-app-salestrading-notificationsvr'
$pidFile=Join-Path $taskRoot 'runtime/notificationsvr.pid'
if(Test-Path $pidFile){$priorId=[int](Get-Content $pidFile);if(Get-Process -Id $priorId -ErrorAction SilentlyContinue){throw 'Recorded NotificationSvr process is still running.'}}
if(!(Test-Path "$serverPath/control/server.lic")){Copy-Item -LiteralPath "$taskRoot/cczq-app-salestrading-quotesvr/control/server.lic" -Destination "$serverPath/control/server.lic"}
$classpath= & (Join-Path $PSScriptRoot 'server-classpath.ps1') -Server notificationsvr
$arguments='-Xms24m -Xmx160m -XX:ActiveProcessorCount=2 -Dfile.encoding=UTF-8 -cp "'+$classpath+'" ficc.notification.NotificationMain'
$oldAtsRoot=$env:ATS_ROOT
try{
    $env:ATS_ROOT=$serverPath
    $p=Start-Process java -ArgumentList $arguments -WorkingDirectory $serverPath -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/notificationsvr.log" -RedirectStandardError "$taskRoot/runtime/notificationsvr.err"
    $p.Id | Set-Content $pidFile
    Write-Output "NotificationSvr startup requested; PID $($p.Id)."
}finally{$env:ATS_ROOT=$oldAtsRoot}

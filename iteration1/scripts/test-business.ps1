$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$classpath= & (Join-Path $PSScriptRoot 'server-classpath.ps1') -Server quotesvr
$testOut=Join-Path $taskRoot ('runtime/business-test-classes/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $testOut | Out-Null
$files=@(Get-ChildItem "$taskRoot/cgws-ficc-saletrading-quotesvr/src/test/java" -Filter '*.java' -Recurse | Select-Object -ExpandProperty FullName)
& javac '-J-Xmx192m' '-J-XX:ActiveProcessorCount=2' '-J-Duser.language=en' -proc:none -source 8 -target 8 -encoding UTF-8 -cp $classpath -d $testOut @files
if($LASTEXITCODE -ne 0){throw 'Business compile failed'}
$javaArgs='-Xms32m -Xmx128m -XX:ActiveProcessorCount=2 -cp "'+$testOut+';'+$classpath+'" ficc.quote.BusinessSmoke "'+$taskRoot+'/runtime/quotesvr.local.properties" "'+$taskRoot+'/runtime/business-smoke.json"'
$previousAtsRoot = $env:ATS_ROOT
try {
    $env:ATS_ROOT = Join-Path $taskRoot 'cgws-ficc-saletrading-quotesvr'
    $p=Start-Process java -ArgumentList $javaArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskRoot/runtime/business-smoke.log" -RedirectStandardError "$taskRoot/runtime/business-smoke.err"
} finally {
    $env:ATS_ROOT = $previousAtsRoot
}
$null=$p.Handle
if(!$p.WaitForExit(30000)){Stop-Process -Id $p.Id;throw 'Functional test stopped after 30 seconds'}
Get-Content "$taskRoot/runtime/business-smoke.log" -Tail 8
Get-Content "$taskRoot/runtime/business-smoke.err" -Tail 8
if(!(Test-Path "$taskRoot/runtime/business-smoke.json") -or $p.ExitCode -ne 0){throw "Functional test exit $($p.ExitCode)"}

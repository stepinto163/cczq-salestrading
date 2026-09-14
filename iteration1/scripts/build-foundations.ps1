# Resolve the STC foundation classpath from the internal Maven repository into
# runtime/foundation-classpath.txt, for machines without the frozen JAR set.
param([string]$Settings='E:\Program Files\apache-maven-3.9.16\conf\settings_yanxin.xml')
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$outFile=Join-Path $taskRoot 'runtime/foundation-classpath.txt'
New-Item -ItemType Directory -Force -Path (Join-Path $taskRoot 'runtime') | Out-Null
$previousProxy=$env:http_proxy,$env:https_proxy
$env:http_proxy='';$env:https_proxy=''
try{
    & mvn.cmd -s $Settings -q -f (Join-Path $taskRoot 'pom.xml') -pl stc-foundation-runtime -am `
        dependency:build-classpath "-Dmdep.outputFile=$outFile" -DincludeScope=runtime
    if($LASTEXITCODE -ne 0){throw 'Maven dependency:build-classpath failed.'}
    Write-Output "Foundation classpath written: $outFile"
}finally{
    $env:http_proxy=$previousProxy[0];$env:https_proxy=$previousProxy[1]
}

param([Parameter(Mandatory=$true)][ValidateSet('quotesvr','notificationsvr')][string]$Server)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$serverJar=Join-Path $taskRoot "cczq-app-salestrading-$Server/target/cczq-app-salestrading-$Server.jar"
$commonJar=Join-Path $taskRoot 'cczq-app-salestrading-common/target/cczq-app-salestrading-common.jar'
foreach($path in @($serverJar,$commonJar)){if(!(Test-Path -LiteralPath $path)){throw "Missing $path. Run scripts/build-server.ps1."}}
$serverJar+';'+$commonJar+';'+(& (Join-Path $PSScriptRoot 'foundation-classpath.ps1'))

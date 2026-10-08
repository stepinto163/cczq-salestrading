param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('quotesvr', 'notificationsvr', 'crmsvr', 'qtintakesvr', 'parsersvr')]
    [string]$Server
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$artifact = if ($Server -eq 'parsersvr') { 'cgws-ficc-parsersvr' } else { "cgws-ficc-saletrading-$Server" }
$target = Join-Path $taskRoot "$artifact/target"
$serverJar = Join-Path $target "$artifact-0.1.0-SNAPSHOT.jar"
$library = Join-Path $target 'javalib'
if (!(Test-Path -LiteralPath $serverJar) -or !(Test-Path -LiteralPath $library)) {
    throw "Missing standard Maven output for $Server. Run scripts/build-server.ps1 first."
}
"$serverJar;$library/*"

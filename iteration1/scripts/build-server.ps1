param(
    [ValidateSet('all', 'common', 'quotesvr', 'notificationsvr')][string]$Module = 'all',
    [ValidateSet('compat', 'stc')][string]$BuildMode = 'compat'
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
foreach ($service in @('quotesvr', 'notificationsvr')) {
    $pidFile = Join-Path $taskRoot "runtime/$service.pid"
    if (Test-Path -LiteralPath $pidFile) {
        $serviceId = [int](Get-Content -LiteralPath $pidFile)
        if (Get-Process -Id $serviceId -ErrorAction SilentlyContinue) {
            throw "Stop $service before rebuilding its shared library."
        }
    }
}
$mavenCommand = Get-Command mvn.cmd, mvn -ErrorAction SilentlyContinue | Select-Object -First 1
$maven = if ($mavenCommand) { $mavenCommand.Source } else {
    Join-Path $taskRoot 'runtime/tools/apache-maven-3.9.16/bin/mvn.cmd'
}
if (!(Test-Path -LiteralPath $maven)) { throw 'Maven 3.9.x is required.' }
$pomName = if ($BuildMode -eq 'stc') { 'pom.xml' } else { 'pom_compat.xml' }
$modules = @('parent-pom', 'common')
if ($Module -eq 'all') { $modules += @('quotesvr', 'notificationsvr') }
elseif ($Module -ne 'common') { $modules += $Module }
$previousMavenOptions = $env:MAVEN_OPTS
try {
    if (!$env:MAVEN_OPTS) { $env:MAVEN_OPTS = '-Xmx256m -XX:ActiveProcessorCount=2' }
    foreach ($name in $modules) {
        $pom = Join-Path $taskRoot "cgws-ficc-saletrading-$name/$pomName"
        & $maven -B -ntp -f $pom install
        if ($LASTEXITCODE -ne 0) { throw "$name Maven build failed." }
    }
} finally {
    $env:MAVEN_OPTS = $previousMavenOptions
}

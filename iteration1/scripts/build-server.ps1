param(
    [ValidateSet('all', 'parent-pom', 'common', 'quotesvr', 'notificationsvr', 'crmsvr', 'qtintakesvr')]
    [string]$Module = 'all',
    [string]$SettingsFile = '',
    [switch]$Clean
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$mavenCommand = Get-Command mvn.cmd, mvn -ErrorAction SilentlyContinue | Select-Object -First 1
$maven = if ($mavenCommand) { $mavenCommand.Source } else {
    Join-Path $taskRoot 'runtime/tools/apache-maven-3.9.16/bin/mvn.cmd'
}
if (!(Test-Path -LiteralPath $maven)) { throw 'Maven 3.9.x is required.' }
$modules = @('parent-pom')
if ($Module -ne 'parent-pom') { $modules += 'common' }
if ($Module -eq 'all') { $modules += @('quotesvr', 'notificationsvr', 'crmsvr', 'qtintakesvr') }
elseif ($Module -notin @('parent-pom', 'common')) { $modules += $Module }
$mavenArguments = @('-B', '-ntp')
if ($SettingsFile) { $mavenArguments += @('-s', (Resolve-Path -LiteralPath $SettingsFile).Path) }
$previousMavenOptions = $env:MAVEN_OPTS
try {
    if (!$env:MAVEN_OPTS) { $env:MAVEN_OPTS = '-Xmx256m -XX:ActiveProcessorCount=2' }
    foreach ($name in $modules) {
        $pom = Join-Path $taskRoot "cgws-ficc-saletrading-$name/pom.xml"
        $goals = @('install')
        if ($Clean) { $goals = @('clean', 'install') }
        & $maven @mavenArguments -f $pom @goals
        if ($LASTEXITCODE -ne 0) { throw "$name Maven build failed." }
    }
} finally {
    $env:MAVEN_OPTS = $previousMavenOptions
}

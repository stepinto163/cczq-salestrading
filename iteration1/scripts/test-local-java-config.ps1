param([string]$AtsRoot)
$ErrorActionPreference = 'Stop'
$iterationRoot = Split-Path $PSScriptRoot -Parent
if (!$AtsRoot) {
    $AtsRoot = Join-Path (Split-Path (Split-Path $iterationRoot -Parent) -Parent) 'qa_env/ats_root'
}
$AtsRoot = (Resolve-Path -LiteralPath $AtsRoot).Path
$jdk = Join-Path $iterationRoot 'runtime/tools/jdk8/jdk8u504-b01'
if (!(Test-Path "$jdk/bin/javac.exe")) { $jdk = $env:JAVA_HOME }
if (!$jdk -or !(Test-Path "$jdk/bin/javac.exe")) { throw 'Java 8 JDK is required.' }
$checkRoot = Join-Path $iterationRoot ('runtime/local-config-check-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $checkRoot -Force | Out-Null
$source = Join-Path $PSScriptRoot 'tests/java/LocalServiceConfigurationCheck.java'
& "$jdk/bin/javac.exe" -encoding UTF-8 -cp "$AtsRoot/stc/QuoteSvr/javalib/*" -d $checkRoot $source
if ($LASTEXITCODE -ne 0) { throw 'Configuration check compilation failed.' }
$previousAtsRoot = $env:ATS_ROOT
try {
    $env:ATS_ROOT = $AtsRoot
    $services = [ordered]@{
        'cgws-ficc-saletrading-quotesvr' = 'QuoteSvr'
        'cgws-ficc-saletrading-notificationsvr' = 'NotificationSvr'
        'cgws-ficc-saletrading-crmsvr' = 'CRMSvr'
        'cgws-ficc-saletrading-qtintakesvr' = 'QtIntakeSvr'
        'cgws-ficc-parsersvr' = 'ParserSvr'
    }
    foreach ($project in $services.Keys) {
        $service = $services[$project]
        $projectRoot = Join-Path $iterationRoot $project
        $classpath = "$checkRoot;$projectRoot/target/classes;$AtsRoot/stc/$service/javalib/*"
        $stdout = Join-Path $checkRoot "$service.out.log"
        $stderr = Join-Path $checkRoot "$service.err.log"
        $process = Start-Process -FilePath "$jdk/bin/java.exe" -WorkingDirectory $projectRoot `
            -ArgumentList @('-Xmx128m', '-XX:ActiveProcessorCount=2', '-Dfile.encoding=UTF-8', '-cp', "`"$classpath`"", 'LocalServiceConfigurationCheck', $service) `
            -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        $null = $process.Handle
        if (!$process.WaitForExit(15000)) {
            Stop-Process -Id $process.Id -Force
            throw "$service configuration check exceeded its 15-second watchdog."
        }
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            Get-Content -LiteralPath $stderr -Encoding UTF8 -Tail 20
            throw "$service configuration check failed. See $checkRoot"
        }
        Get-Content -LiteralPath $stdout -Encoding UTF8 | Select-String '^PASS '
    }
    Write-Host "Evidence: $checkRoot"
    Write-Host 'Scope: configuration/SDK checks only; no server launch, FIX logon or database writes.'
} finally {
    $env:ATS_ROOT = $previousAtsRoot
}

$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$foundation = & (Join-Path $PSScriptRoot 'server-classpath.ps1') -Server quotesvr
$common = Join-Path $taskRoot 'cgws-ficc-saletrading-common/target'
$classpath = "$common/test-classes;$common/cgws-ficc-saletrading-common-0.1.0-SNAPSHOT.jar;$foundation"
foreach ($service in @('QuoteSvr', 'NotificationSvr')) {
    $module = if ($service -eq 'QuoteSvr') { 'quotesvr' } else { 'notificationsvr' }
    $configuration = Join-Path $taskRoot "cgws-ficc-saletrading-$module/control/$service/log4j.ini"
    $runId = [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $reportPrefix = Join-Path $taskRoot "runtime/component-failure-$module-$runId"
    $arguments = '-Xmx64m -XX:ActiveProcessorCount=1 -cp "' + $classpath + '" ficc.common.ComponentFailureProbe "' + $configuration + '" "' + $reportPrefix + '.startup.log"'
    $process = Start-Process java -ArgumentList $arguments -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$reportPrefix.stdout.log" -RedirectStandardError "$reportPrefix.stderr.log"
    $null = $process.Handle
    if (!$process.WaitForExit(10000)) {
        Stop-Process -Id $process.Id
        throw "$service failure probe exceeded its ten-second watchdog."
    }
    if ($process.ExitCode -eq 0) { throw "$service configuration failure was swallowed." }
    foreach ($file in @("$reportPrefix.stdout.log", "$reportPrefix.startup.log")) {
        $content = [IO.File]::ReadAllText($file)
        foreach ($expected in @('"component":"CRM"', '"state":"STARTING"', '"state":"FAILED"', 'crm.jdbcUrl is required', 'PostgresCrmClient.java:')) {
            if (!$content.Contains($expected)) { throw "$file does not contain $expected" }
        }
        if ($content.Contains('"state":"INITIALIZED"')) { throw "$service incorrectly reported initialization success." }
    }
    Write-Output "PASS $service config: STARTING/FAILED and exception stack in console and file; process exit=$($process.ExitCode)"
}

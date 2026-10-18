param(
    [Parameter(Mandatory = $true)][string]$Configuration,
    [string]$ReferenceRoot,
    [string]$ReportDirectory
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $taskRoot 'cgws-ficc-saletrading-notificationsvr'
$configurationPath = (Resolve-Path -LiteralPath $Configuration).Path
if (!$ReferenceRoot) { $ReferenceRoot = Join-Path $taskRoot 'cgws-ficc-saletrading-quotesvr' }
if (!$ReportDirectory) { $ReportDirectory = Join-Path $taskRoot ('runtime/notification-tests/' + (Get-Date -Format 'yyyyMMdd-HHmmss')) }
$reportPath = [IO.Path]::GetFullPath($ReportDirectory)
New-Item -ItemType Directory -Path $reportPath -Force | Out-Null
$mavenCommand = Get-Command mvn.cmd, mvn -ErrorAction SilentlyContinue | Select-Object -First 1
$maven = if ($mavenCommand) { $mavenCommand.Source } else { Join-Path $taskRoot 'runtime/tools/apache-maven-3.9.16/bin/mvn.cmd' }
$previousRoot = $env:ATS_ROOT
$previousOptions = $env:MAVEN_OPTS
try {
    $env:MAVEN_OPTS = '-Xmx256m -XX:ActiveProcessorCount=2'
    $env:ATS_ROOT = [IO.Path]::GetFullPath($ReferenceRoot)
    & $maven -q -f "$project/pom.xml" test-compile dependency:copy-dependencies '-DincludeScope=test' '-DoutputDirectory=target/test-libraries'
    if ($LASTEXITCODE -ne 0) { throw 'Notification test compilation failed.' }
    foreach ($name in @('NotificationProtocolSmoke', 'NotificationScenarioMatrix')) {
        $classPath = "$project/target/test-classes;$project/target/classes;$project/target/test-libraries/*"
        $arguments = @('-Xmx160m', '-XX:ActiveProcessorCount=2', '-Dfile.encoding=UTF-8', '-cp', ('"' + $classPath + '"'),
            "ficc.notification.$name", ('"' + $configurationPath + '"'))
        if ($name -eq 'NotificationProtocolSmoke') { $arguments += ('"' + (Join-Path $reportPath 'protocol-fixtures.json') + '"') }
        $stdout = Join-Path $reportPath "$name.log"
        $stderr = Join-Path $reportPath "$name.err"
        # 仅少量功能夹具，独立进程/临时 schema；超时绝不自动增加负载或重试。
        $process = Start-Process java -ArgumentList $arguments -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        try {
            $null = $process.Handle
            if (!$process.WaitForExit(30000)) {
                $process.Kill()
                throw "$name exceeded its 30-second watchdog. Inspect $reportPath; an interrupted test may leave its temporary schema."
            }
            $process.WaitForExit()
            if ($process.ExitCode -ne 0) {
                Get-Content -LiteralPath $stderr -Encoding UTF8
                throw "$name failed. See $stdout"
            }
            Get-Content -LiteralPath $stdout -Encoding UTF8 | Select-Object -Last 1
        } finally {
            if ($process) { $process.Dispose() }
        }
    }
    Write-Host "Reports: $reportPath"
} finally {
    $env:ATS_ROOT = $previousRoot
    $env:MAVEN_OPTS = $previousOptions
}

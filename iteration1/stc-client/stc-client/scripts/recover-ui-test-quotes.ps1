param(
    [Parameter(Mandatory=$true)][string]$TestRoot,
    [Parameter(Mandatory=$true)][string]$CaseReport,
    [string]$User = 'test2',
    [string]$Password = '123456'
)
$ErrorActionPreference = 'Stop'
$runtime = [IO.Path]::GetFullPath($TestRoot)
$source = [IO.Path]::GetFullPath($CaseReport)
$caseFile = Join-Path $source 'case.json'
$manifest = Join-Path $source 'owned-quotes.json'
if (!(Test-Path -LiteralPath $caseFile) -or !(Test-Path -LiteralPath $manifest)) {
    throw 'Recovery requires the original case.json and owned-quotes.json.'
}
# 同一测试安装目录只允许一个运行器；不覆盖原失败报告。
$runLock = [IO.File]::Open((Join-Path $runtime 'ui-tests.lock'), 'OpenOrCreate', 'ReadWrite', 'None')
try {
    $output = Join-Path $source ('quote-recovery-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    New-Item -ItemType Directory -Path $output | Out-Null
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = Join-Path $runtime 'UiScenarioWorker.exe'
    $startInfo.WorkingDirectory = $runtime
    $startInfo.Arguments = '"' + $caseFile + '" "' + $output + '" "' + $manifest + '"'
    $startInfo.UseShellExecute = $false
    $startInfo.EnvironmentVariables['STC_UI_USER'] = $User
    $startInfo.EnvironmentVariables['STC_UI_PASSWORD'] = $Password
    $startInfo.EnvironmentVariables['STC_UI_REALTIME_ROOT'] = $output
    $startInfo.EnvironmentVariables.Remove('STC_UI_REALTIME_ROLE')
    $process = [Diagnostics.Process]::Start($startInfo)
    if (!$process.WaitForExit(60000)) {
        $process.Kill()
        throw "Quote recovery timed out; inspect $output"
    }
    $result = Get-Content -LiteralPath (Join-Path $output 'result.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($process.ExitCode -ne 0 -or !$result.cleanupPassed) { throw "Quote recovery failed: $output" }
    Write-Host "Recovered listed quotes only. Original result remains failed. Report: $output"
} finally {
    $runLock.Dispose()
}

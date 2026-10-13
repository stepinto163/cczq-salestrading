param([string]$TestRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$TestRoot) { $TestRoot = Join-Path $clientRoot 'runtime/ui-test-client' }
$TestRoot = (Resolve-Path $TestRoot).Path
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$executable = Join-Path $TestRoot 'RunnerSelectionCheck.exe'
& $compiler /nologo /target:exe "/out:$executable" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll "/r:$TestRoot/SalesTradingUiTests.exe" "$clientRoot/tests/ui/RunnerSelectionCheck.cs"
if ($LASTEXITCODE -ne 0) { throw 'Runner selection check compilation failed.' }
Copy-Item -LiteralPath "$TestRoot/SalesTradingUiTests.exe.config" -Destination "$executable.config" -Force
$output = Join-Path $clientRoot 'runtime/runner-selection-check'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$process = Start-Process -FilePath $executable -WorkingDirectory $output -PassThru -WindowStyle Hidden -RedirectStandardOutput "$output/stdout.log" -RedirectStandardError "$output/stderr.log"
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Runner selection check exceeded the 15-second watchdog.'
}
Get-Content "$output/stdout.log"
if ($process.ExitCode -ne 0 -or !(Select-String -LiteralPath "$output/stdout.log" -Pattern '^PASS:' -Quiet)) {
    Get-Content "$output/stderr.log"
    throw "Runner selection check failed: $($process.ExitCode)"
}

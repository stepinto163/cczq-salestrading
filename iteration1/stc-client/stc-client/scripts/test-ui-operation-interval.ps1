$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$source = Join-Path $clientRoot 'tests/ui'
$output = Join-Path $clientRoot 'runtime/operation-interval-check'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (!$compiler) { throw 'Roslyn compiler is required.' }
$executable = Join-Path $output 'OperationIntervalCheck.exe'
& $compiler /nologo /target:exe "/out:$executable" /r:System.dll /r:System.Core.dll "$source/OperationInterval.cs" "$source/OperationIntervalCheck.cs"
if ($LASTEXITCODE -ne 0) { throw 'Interval check compilation failed.' }
$process = Start-Process -FilePath $executable -PassThru -WindowStyle Hidden -RedirectStandardOutput "$output/stdout.log" -RedirectStandardError "$output/stderr.log"
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Interval check exceeded the 15-second watchdog.'
}
Get-Content "$output/stdout.log"
if ($process.ExitCode -ne 0) {
    Get-Content "$output/stderr.log"
    throw "Interval check failed: $($process.ExitCode)"
}

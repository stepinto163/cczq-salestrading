$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$source = Join-Path $clientRoot 'tests/ui'
$output = Join-Path $clientRoot 'runtime/demo-caption-check'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll', '/r:System.Core.dll', '/r:System.Xaml.dll')
foreach ($name in @('WindowsBase', 'PresentationFramework', 'PresentationCore')) { $references += "/r:$framework/$name.dll" }
$executable = Join-Path $output 'DemoCaptionCheck.exe'
& $compiler /nologo /target:exe "/out:$executable" $references "$source/DemoCaptionWindow.cs" "$source/DesktopPresentation.cs" "$source/DemoCaptionCheck.cs"
if ($LASTEXITCODE -ne 0) { throw 'Caption check compilation failed.' }
$process = Start-Process -FilePath $executable -WorkingDirectory $output -PassThru -WindowStyle Hidden -RedirectStandardOutput "$output/stdout.log" -RedirectStandardError "$output/stderr.log"
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Caption check exceeded the 15-second watchdog.'
}
Get-Content "$output/stdout.log"
if ($process.ExitCode -ne 0 -or !(Select-String -LiteralPath "$output/stdout.log" -Pattern '^PASS:' -Quiet)) {
    Get-Content "$output/stderr.log"
    throw "Caption check failed: $($process.ExitCode)"
}

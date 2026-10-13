param([string]$OutputRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$OutputRoot) { $OutputRoot = Join-Path $clientRoot 'runtime/ui-test-client' }
$source = Join-Path $clientRoot 'tests/ui'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll', '/r:System.Core.dll', "/r:$OutputRoot/STC.exe", "/r:$OutputRoot/bin/STC.Frame.dll")
foreach ($name in @('WindowsBase', 'PresentationFramework', 'PresentationCore', 'System.Xaml')) { $references += "/r:$framework/$name.dll" }
$executable = Join-Path $OutputRoot 'UserNoticeAutomationCheck.exe'
& $compiler /nologo /target:exe "/out:$executable" $references "$source/UserNoticeAutomation.cs" "$source/UserNoticeAutomationCheck.cs"
if ($LASTEXITCODE -ne 0) { throw 'Notice check compilation failed.' }
$configuration = New-Object System.Xml.XmlDocument
$configuration.Load("$OutputRoot/STC.exe.config")
$configuration.SelectSingleNode("/configuration/appSettings/add[@key='IsSingle']").SetAttribute('value', 'false')
$configuration.Save("$executable.config")
$process = Start-Process -FilePath $executable -WorkingDirectory $OutputRoot -PassThru -WindowStyle Hidden -RedirectStandardOutput "$OutputRoot/notice-check.stdout.log" -RedirectStandardError "$OutputRoot/notice-check.stderr.log"
$null = $process.Handle
if (!$process.WaitForExit(15000)) {
    Stop-Process -Id $process.Id -Force
    throw 'Notice check exceeded the 15-second watchdog.'
}
Get-Content "$OutputRoot/notice-check.stdout.log"
if ($process.ExitCode -ne 0 -or !(Select-String -LiteralPath "$OutputRoot/notice-check.stdout.log" -Pattern '^PASS:' -Quiet)) {
    Get-Content "$OutputRoot/notice-check.stderr.log"
    throw "Notice check failed: $($process.ExitCode)"
}

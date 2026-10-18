param([Parameter(Mandatory=$true)][string]$BuildRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
$buildRoot = [IO.Path]::GetFullPath($BuildRoot)
$binaryRoot = Join-Path $buildRoot 'bin'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (!$compiler) { throw 'Visual Studio Roslyn compiler is required.' }
$framework = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2'
$references = @('/r:System.dll', '/r:System.Core.dll', '/r:System.Configuration.dll', "/r:$framework/Facades/netstandard.dll")
foreach ($name in @('gateway-api', 'common.netapi', 'STC.Common', 'STC.Frame', 'STC.SalesTrading.Plugin', 'CommunityToolkit.Mvvm')) {
    $references += "/r:$binaryRoot/$name.dll"
}
foreach ($name in @('PresentationFramework', 'PresentationCore', 'WindowsBase', 'System.Xaml')) {
    $references += "/r:$framework/$name.dll"
}
$testExe = Join-Path $binaryRoot 'SalesTrading.IntegrationTests.exe'
& $compiler /nologo /target:exe /debug /langversion:latest "/out:$testExe" $references "$clientRoot/tests/integration/Program.cs"
if ($LASTEXITCODE -ne 0) { throw 'Integration test compilation failed.' }
# 与实际客户端使用相同程序集绑定，避免测试进程加载不同版本的框架依赖。
Copy-Item -LiteralPath (Join-Path $buildRoot 'STC.exe.config') -Destination ($testExe + '.config') -Force
$stdout = Join-Path $buildRoot 'integration-tests.log'
$stderr = Join-Path $buildRoot 'integration-tests-error.log'
# 功能测试独立进程，固定 30 秒看门狗；不是性能压力测试。
$startInfo = New-Object System.Diagnostics.ProcessStartInfo
$startInfo.FileName = $testExe
$startInfo.WorkingDirectory = $binaryRoot
$startInfo.UseShellExecute = $false
$startInfo.CreateNoWindow = $true
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
$process = New-Object System.Diagnostics.Process
$process.StartInfo = $startInfo
[void]$process.Start()
$outputTask = $process.StandardOutput.ReadToEndAsync()
$errorTask = $process.StandardError.ReadToEndAsync()
if (!$process.WaitForExit(30000)) {
    $process.Kill()
    throw "Integration tests exceeded 30 seconds. See $stdout"
}
$outputTask.GetAwaiter().GetResult() | Set-Content -LiteralPath $stdout -Encoding UTF8
$errorTask.GetAwaiter().GetResult() | Set-Content -LiteralPath $stderr -Encoding UTF8
$exitCode = $process.ExitCode
$process.Dispose()
Get-Content -LiteralPath $stdout -Encoding UTF8
if ($exitCode -ne 0) {
    Get-Content -LiteralPath $stderr -Encoding UTF8
    throw "Integration tests failed ($exitCode). See $stdout"
}

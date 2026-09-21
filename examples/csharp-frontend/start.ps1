$ErrorActionPreference = 'Stop'
$executable = Join-Path $PSScriptRoot 'bin\Debug\net472\Ficc.FrontendLearning.exe'
$updatedExecutable = Join-Path $PSScriptRoot 'bin\Updated\net472\Ficc.FrontendLearning.exe'
if ((Test-Path -LiteralPath $updatedExecutable) -and
    (!(Test-Path -LiteralPath $executable) -or
    (Get-Item -LiteralPath $updatedExecutable).LastWriteTimeUtc -gt (Get-Item -LiteralPath $executable).LastWriteTimeUtc)) {
    $executable = $updatedExecutable
}
if (-not (Test-Path -LiteralPath $executable)) {
    throw 'Build first: dotnet build FrontendLearning.csproj -m:1'
}
Start-Process -FilePath $executable -WorkingDirectory $PSScriptRoot

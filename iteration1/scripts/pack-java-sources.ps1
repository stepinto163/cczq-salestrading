param(
    [string]$ProjectRoot = (Split-Path $PSScriptRoot -Parent),
    [string]$Output
)

$ErrorActionPreference = 'Stop'
foreach ($command in @('python', 'git')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "$command must be available on PATH."
    }
}
$arguments = @((Join-Path $PSScriptRoot 'pack_java_sources.py'), '--project-root', $ProjectRoot)
if ($Output) { $arguments += @('--output', $Output) }
& python @arguments
if ($LASTEXITCODE -ne 0) { throw 'Java source packaging failed; see the error above.' }

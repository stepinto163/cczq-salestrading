param(
    [string]$Repository = (Join-Path $PSScriptRoot '../..'),
    [string]$Output
)

$ErrorActionPreference = 'Stop'
if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
    throw 'Python 3 and Git must be available on PATH.'
}
$arguments = @((Join-Path $PSScriptRoot 'pack_source_changes.py'), '--repository', $Repository)
if ($Output) { $arguments += @('--output', $Output) }
& python @arguments
if ($LASTEXITCODE -ne 0) { throw 'Source packaging failed; see the error above.' }

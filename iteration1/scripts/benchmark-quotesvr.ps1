[CmdletBinding()]
param(
    [string]$Config,
    [ValidateSet('spread', 'concentrated')]
    [string]$Distribution = 'spread',
    [switch]$BuildOnly,
    [switch]$SkipBuild
)
$ErrorActionPreference = 'Stop'
$arguments = @((Join-Path $PSScriptRoot 'benchmark_quotesvr.py'), '--distribution', $Distribution)
if ($Config) { $arguments += @('--config', $Config) }
if ($BuildOnly) { $arguments += '--build-only' }
if ($SkipBuild) { $arguments += '--skip-build' }
python @arguments
exit $LASTEXITCODE

$ErrorActionPreference = 'Stop'
. (Join-Path (Split-Path $PSScriptRoot -Parent) 'resolve-command-name.ps1')

$choices = @('QuoteSvr', 'QtIntakeSvr', 'CRMSvr', 'CalSvr', 'client', 'all')
$aliases = @{ 'stc-client' = 'client' }
foreach ($example in @(
    @('qUo', 'QuoteSvr'), @('QT', 'QtIntakeSvr'), @('Cr', 'CRMSvr'),
    @('CL', 'client'), @('STC-C', 'client'), @('ALL', 'all'), @('QuoteSvr', 'QuoteSvr')
)) {
    $actual = Resolve-CommandName $example[0] $choices -Aliases $aliases
    if ($actual -ne $example[1]) { throw "Wrong resolution: $($example[0]) -> $actual" }
}
foreach ($invalid in @('q', 'c', 'missing', '*', '')) {
    $rejected = $false
    try { $null = Resolve-CommandName $invalid $choices -Aliases $aliases }
    catch { $rejected = $true }
    if (!$rejected) { throw "Unsafe target accepted: '$invalid'" }
}
$actions = @('start', 'stop', 'restart', 'status', 'build', 'report')
foreach ($invalid in @('st', 'sta', 're')) {
    $rejected = $false
    try { $null = Resolve-CommandName $invalid $actions }
    catch {
        if ($_.Exception.Message -notlike 'Ambiguous*') { throw }
        $rejected = $true
    }
    if (!$rejected) { throw "Ambiguous action accepted: $invalid" }
}
if ((Resolve-CommandName 'STAT' $actions) -ne 'status') { throw 'Status prefix failed.' }
if ((Resolve-CommandName 'STO' $actions) -ne 'stop') { throw 'Stop prefix failed.' }
if ((Resolve-CommandName 'REP' $actions) -ne 'report') { throw 'Report prefix failed.' }
# An exact command must win even when another command extends its name.
if ((Resolve-CommandName 'START' @('start', 'startup')) -ne 'start') { throw 'Exact match precedence failed.' }
# Aliases for the same target must not introduce false ambiguity.
if ((Resolve-CommandName 'not' @('notificationsvr') -Aliases @{ 'notification' = 'notificationsvr' }) -ne 'notificationsvr') {
    throw 'Alias deduplication failed.'
}
Write-Host 'PASS: case-insensitive prefixes, aliases, exact matches and ambiguity rejection.'

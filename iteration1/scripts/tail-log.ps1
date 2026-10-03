param(
    [Parameter(Position = 0, Mandatory = $true)]
    [string]$Target,
    [string]$Kind = 'console',
    [string]$AtsRoot,
    [ValidateRange(1, 10000)]
    [int]$Lines = 80,
    [switch]$Here,
    [switch]$Once
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'resolve-command-name.ps1')
$Kind = Resolve-CommandName $Kind @('console', 'error', 'app') -Label 'log kind'
$repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
if (!$AtsRoot) {
    $AtsRoot = if ($env:ATS_ROOT) { $env:ATS_ROOT } else {
        Join-Path (Split-Path $repositoryRoot -Parent) 'qa_env/ats_root'
    }
}
$AtsRoot = [IO.Path]::GetFullPath($AtsRoot)
$Target = ($Target -replace '\\', '/').TrimEnd('/').Split('/')[-1]
$services = @(Import-Csv -LiteralPath (Join-Path $AtsRoot 'scripts/serverlist.csv'))
$Target = Resolve-CommandName $Target (@($services.Service) + @('client')) -Aliases @{
    'Registry' = 'ZooKeeper'
    'stc-client' = 'client'
} -Label 'log target'

if ($Target -in @('client', 'stc-client')) {
    $Target = 'client'
    $runtime = Join-Path $repositoryRoot 'iteration1/stc-client/stc-client/runtime/native-client'
    $configuration = New-Object System.Xml.XmlDocument
    $configuration.Load((Join-Path $runtime 'STC.exe.config'))
    $setting = $configuration.SelectSingleNode('/configuration/appSettings/add[@key="LogPath"]')
    if (!$setting -or !$setting.value) { throw 'STC.exe.config does not define LogPath.' }
    $prefix = [string]$setting.value
    if (![IO.Path]::IsPathRooted($prefix)) { $prefix = Join-Path $runtime $prefix }
    $directory = Split-Path $prefix -Parent
    $pattern = (Split-Path $prefix -Leaf) + '*.LOG'
    $latest = Get-ChildItem -LiteralPath $directory -Filter $pattern -File |
        Sort-Object Name -Descending | Select-Object -First 1
    if (!$latest) { throw "No STC client log found under $directory ($pattern)." }
    $logFile = $latest.FullName
} else {
    $service = @($services | Where-Object { $_.Service -eq $Target })
    if ($service.Count -ne 1) {
        throw "Unknown target '$Target'. Use client or: $($services.Service -join ', ')"
    }
    $Target = $service[0].Service
    $suffix = switch ($Kind) {
        'console' { '.console.log' }
        'error' { '.stderr.log' }
        'app' { '.log' }
    }
    $logFile = Join-Path $AtsRoot ('log/' + $Target + $suffix)
}
if (!(Test-Path -LiteralPath $logFile -PathType Leaf)) {
    throw "Log file does not exist: $logFile. Start the service first or select another -Kind."
}

if (!$Here -and !$Once) {
    # This command explicitly opens an interactive console for the user.
    $arguments = @('-NoProfile', '-NoExit', '-ExecutionPolicy', 'Bypass', '-File',
        ('"' + $PSCommandPath + '"'), '-Target', $Target, '-Kind', $Kind,
        '-AtsRoot', ('"' + $AtsRoot + '"'), '-Lines', $Lines, '-Here')
    $process = Start-Process -FilePath 'powershell.exe' -ArgumentList $arguments -WindowStyle Normal -PassThru
    Write-Host "Opened log console PID $($process.Id): $logFile"
    exit 0
}

if (!$Once) { $Host.UI.RawUI.WindowTitle = "FICC log - $Target - $Kind" }
Write-Host "Log: $logFile"
if ($Once) {
    Get-Content -LiteralPath $logFile -Encoding UTF8 -Tail $Lines
} else {
    Write-Host 'Following log. Ctrl+C stops following; closing this console does not stop the service/client.'
    Write-Host 'If the log is rotated or the client starts a new daily file, run log.cmd again.'
    Get-Content -LiteralPath $logFile -Encoding UTF8 -Tail $Lines -Wait
}

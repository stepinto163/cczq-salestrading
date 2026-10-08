[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$Action,
    [Parameter(Position=0)]
    [string]$Target,
    [switch]$NoBuild,
    [switch]$Plan,
    [string]$SettingsFile,
    [string]$AtsRoot,
    [string]$ClientConfig
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'resolve-command-name.ps1')
$Action = Resolve-CommandName $Action @('build', 'deploy') -Label 'action'
$iterationRoot = Split-Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path $iterationRoot -Parent
$clientSource = Join-Path $iterationRoot 'stc-client/stc-client'
$buildRoot = Join-Path $iterationRoot 'runtime/builds'
$services = [ordered]@{
    quotesvr = 'QuoteSvr'
    notificationsvr = 'NotificationSvr'
    crmsvr = 'CRMSvr'
    qtintakesvr = 'QtIntakeSvr'
    parsersvr = 'ParserSvr'
}
$allowed = @('parent-pom', 'common') + @($services.Keys) + @('client', 'all')
if (!$Target) {
    Write-Host 'Usage: .\build <target> | .\deploy <target> [-NoBuild] [-Plan]'
    Write-Host ('Targets: ' + ($allowed -join ', '))
    Write-Host 'build: Maven clean install / client MSBuild; deploy: build, replace binaries, restart.'
    Write-Host 'Optional: -SettingsFile <Maven settings> -AtsRoot <local ATS_ROOT> -ClientConfig <STC.exe.config>'
    exit 0
}
$Target = Resolve-CommandName $Target $allowed -Aliases @{
    'notifications' = 'notificationsvr'
    'notification' = 'notificationsvr'
    'stc-client' = 'client'
} -Label 'target'
if ($Action -eq 'build' -and $NoBuild) { throw '-NoBuild is only valid for deploy.' }
if ($Action -eq 'deploy' -and $Target -in @('parent-pom', 'common')) {
    throw "$Target is a Maven library, not a process. Deploy the service that consumes it."
}
if (!$AtsRoot) {
    $AtsRoot = if ($env:ATS_ROOT) { $env:ATS_ROOT } else { Join-Path (Split-Path $repositoryRoot -Parent) 'qa_env/ats_root' }
}
$AtsRoot = [IO.Path]::GetFullPath($AtsRoot)
if (!$ClientConfig) { $ClientConfig = Join-Path (Split-Path $AtsRoot -Parent) 'stc-client/STC.exe.config' }
if (!$SettingsFile) { $SettingsFile = Join-Path $iterationRoot 'build/qa-maven/settings-local.xml' }
$targets = if ($Target -eq 'all') { @($services.Keys) + @('client') } else { @($Target) }
$javaTargets = @($targets | Where-Object { $_ -ne 'client' })
$stamp = (Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 6)
$runRoot = Join-Path $buildRoot $stamp

function Assert-Within([string]$Path, [string]$Root) {
    $absolute = [IO.Path]::GetFullPath($Path)
    $boundary = [IO.Path]::GetFullPath($Root).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    if (!$absolute.StartsWith($boundary, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the expected directory: $absolute"
    }
    # Refuse link traversal when moving deployed binaries or preparing a backup.
    $current = $absolute
    while ($current.Length -ge $boundary.TrimEnd('\').Length) {
        if (Test-Path -LiteralPath $current) {
            $item = Get-Item -LiteralPath $current -Force
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Deployment path contains a link: $current" }
        }
        $parent = Split-Path $current -Parent
        if (!$parent -or $parent -eq $current) { break }
        $current = $parent
    }
    return $absolute
}

function Invoke-Script([string]$Path, [object[]]$Arguments) {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Path @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Command failed ($LASTEXITCODE): $Path $($Arguments -join ' ')" }
}

function Assert-Environment {
    if ($targets -contains 'client') {
        $xml = New-Object System.Xml.XmlDocument
        $xml.Load([IO.Path]::GetFullPath($ClientConfig))
    }
    foreach ($name in $targets | Where-Object { $services.Contains($_) }) {
        $service = $services[$name]
        $manager = Join-Path $AtsRoot 'scripts/service.ps1'
        if (!(Test-Path -LiteralPath $manager)) { throw "Local service manager is missing: $manager" }
        $definition = @(Import-Csv (Join-Path $AtsRoot 'scripts/serverlist.csv') | Where-Object { $_.Service -eq $service })
        if ($definition.Count -ne 1 -or $definition[0].Enabled -ne 'true') { throw "$service must be configured and enabled in serverlist.csv." }
        $directory = Assert-Within (Join-Path $AtsRoot "stc/$service") $AtsRoot
        if (!(Test-Path -LiteralPath $directory -PathType Container)) { throw "Service is not installed: $directory" }
    }
}

function Write-Package([string]$Name) {
    $directory = Join-Path $runRoot $Name
    $payload = Join-Path $directory 'payload'
    New-Item -ItemType Directory -Force -Path $payload | Out-Null
    $application = $null
    if ($Name -ne 'client') {
        $projectName = if ($Name -eq 'parsersvr') { 'cgws-ficc-parsersvr' } else { "cgws-ficc-saletrading-$Name" }
        $project = Join-Path $iterationRoot $projectName
        [xml]$pom = Get-Content (Join-Path $project 'pom.xml') -Raw
        $version = if ($pom.project.version) { [string]$pom.project.version } else { [string]$pom.project.parent.version }
        $application = "$($pom.project.artifactId)-$version.jar"
        $jar = Join-Path $project "target/$application"
        if ($Name -eq 'parent-pom') {
            Copy-Item -LiteralPath (Join-Path $project 'pom.xml') -Destination $payload
        } else {
            if (!(Test-Path -LiteralPath $jar)) { throw "Maven did not produce expected JAR: $jar" }
            Copy-Item -LiteralPath $jar -Destination $payload
            if ($services.Contains($Name)) {
                $dependencies = Join-Path $project 'target/javalib'
                if (@(Get-ChildItem -LiteralPath $dependencies -Filter '*.jar').Count -eq 0) { throw "Missing runtime dependencies: $dependencies" }
                Copy-Item -LiteralPath $dependencies -Destination $payload -Recurse
                Test-JavaPackage $Name $jar $dependencies
            }
        }
    } else {
        if (!(Test-Path (Join-Path $payload 'STC.exe'))) { throw 'Client build did not produce STC.exe.' }
    }
    $files = @(Get-ChildItem -LiteralPath $payload -Recurse -File | ForEach-Object {
        [PSCustomObject]@{
            path = $_.FullName.Substring($payload.Length + 1).Replace('\', '/')
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        }
    })
    $package = [ordered]@{
        target = $Name
        builtAt = (Get-Date).ToString('o')
        application = $application
        payload = $payload
        files = $files
    }
    $manifest = Join-Path $directory 'package.json'
    $package | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifest -Encoding UTF8
    Write-Host "PACKAGED $Name -> $manifest"
    return $manifest
}

function Test-JavaPackage([string]$Name, [string]$Jar, [string]$Dependencies) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($Jar)
    try {
        if ($Name -eq 'parsersvr') {
            if (!$archive.GetEntry('cgws/ficc/parser/parse/QuoteMessageParser.class')) {
                throw 'ParserSvr package must contain the Java parser engine.'
            }
            if (@(Get-ChildItem -LiteralPath $Dependencies -Filter 'cgws-ficc-parser-*.jar').Count) {
                throw 'ParserSvr must not depend on a standalone parser engine JAR.'
            }
        }
        $reader = New-Object IO.StreamReader($archive.GetEntry('META-INF/MANIFEST.MF').Open())
        try { $manifest = ($reader.ReadToEnd() -replace '\r?\n ', '') } finally { $reader.Dispose() }
        if ($manifest -notmatch ('(?m)^Main-Class: ' + [regex]::Escape($services[$Name]) + '\r?$')) { throw "Unexpected main class in $Jar" }
        if ($manifest -match '(?m)^Class-Path: ([^\r\n]+)') {
            foreach ($dependency in ($Matches[1] -split ' ' | Where-Object { $_ })) {
                if (!(Test-Path -LiteralPath (Join-Path $Dependencies $dependency))) { throw "Missing dependency: $dependency" }
            }
        }
    } finally { $archive.Dispose() }
}

function Read-Package([string]$Manifest, [string]$Name) {
    $package = Get-Content -LiteralPath $Manifest -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($package.target -ne $Name -or @($package.files).Count -eq 0) { throw "Invalid package manifest: $Manifest" }
    $null = Assert-Within $package.payload $buildRoot
    if (@(Get-ChildItem -LiteralPath $package.payload -Recurse -File).Count -ne @($package.files).Count) {
        throw "Build artifact file list changed: $Manifest. Run .\build $Name again."
    }
    foreach ($file in $package.files) {
        $path = Assert-Within (Join-Path $package.payload $file.path) $package.payload
        if (!(Test-Path -LiteralPath $path) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.sha256) {
            throw "Build artifact missing or changed: $path. Run .\build $Name again."
        }
    }
    return $package
}

function Move-Checked([string]$Source, [string]$Destination, [string]$Boundary) {
    $null = Assert-Within $Source $Boundary
    $null = Assert-Within $Destination $Boundary
    Move-Item -LiteralPath $Source -Destination $Destination
}

function Deploy-Package([string]$Name, $Package) {
    $client = $Name -eq 'client'
    $service = if ($client) { 'STC-client' } else { $services[$Name] }
    $destination = if ($client) { Join-Path $clientSource 'runtime/native-client' } else { Join-Path $AtsRoot "stc/$service" }
    $destination = [IO.Path]::GetFullPath($destination)
    # STC recursively discovers plugin DLLs. Staging and backups must be outside its installation.
    $operationBoundary = if ($client) { Split-Path $destination -Parent } else { $destination }
    $deploymentRoot = if ($client) { Join-Path $operationBoundary 'native-client-deployment-backups' } else { Join-Path $destination '.deploy' }
    $stage = Assert-Within (Join-Path $deploymentRoot "$stamp/new") $operationBoundary
    $backup = Assert-Within (Join-Path $deploymentRoot "$stamp/previous") $operationBoundary
    New-Item -ItemType Directory -Force -Path $stage, $backup | Out-Null
    if ($client) {
        Copy-Item -LiteralPath (Join-Path $Package.payload 'bin'), (Join-Path $Package.payload 'STC.exe') -Destination $stage -Recurse
    } else {
        Copy-Item -LiteralPath (Join-Path $Package.payload 'javalib') -Destination $stage -Recurse
        # CRM/Parser use the standard javalib-only entrypoint; the other local services
        # retain their existing root application JAR layout.
        $jarDirectory = if ($Name -in @('crmsvr', 'parsersvr')) { Join-Path $stage 'javalib' } else { $stage }
        Copy-Item -LiteralPath (Join-Path $Package.payload $Package.application) -Destination $jarDirectory
    }
    $manager = if ($client) { Join-Path $PSScriptRoot 'manage-stc-client.ps1' } else { Join-Path $AtsRoot 'scripts/service.ps1' }
    $stopArguments = if ($client) { @('-Action', 'stop') } else { @('-Action', 'stop', '-Service', $service) }
    $startArguments = if ($client) { @('-Action', 'start', '-StcConfig', $ClientConfig) } else { @('-Action', 'start', '-Service', $service) }
    $oldItems = if ($client) { @('bin', 'STC.exe') } else {
        @('javalib') + @(Get-ChildItem -LiteralPath $destination -Filter "cgws-ficc-saletrading-$Name-*.jar" -File | Select-Object -ExpandProperty Name)
    }
    $moved = New-Object 'System.Collections.Generic.List[string]'
    $installed = New-Object 'System.Collections.Generic.List[string]'
    Write-Host "DEPLOY $Name -> $destination (environment configuration preserved)"
    # A stop failure leaves the existing installation intact.
    Invoke-Script $manager $stopArguments
    if ($client -and (Test-Path -LiteralPath (Join-Path $destination '.deploy'))) {
        Move-Checked (Join-Path $destination '.deploy') (Join-Path $deploymentRoot "legacy-$stamp") $operationBoundary
    }
    try {
        foreach ($item in $oldItems) {
            $path = Join-Path $destination $item
            if (Test-Path -LiteralPath $path) {
                Move-Checked $path (Join-Path $backup $item) $operationBoundary
                $moved.Add($item)
            }
        }
        foreach ($item in Get-ChildItem -LiteralPath $stage) {
            Move-Checked $item.FullName (Join-Path $destination $item.Name) $operationBoundary
            $installed.Add($item.Name)
        }
        Invoke-Script $manager $startArguments
        Write-Host "DEPLOYED $Name; previous binaries: $backup"
        Write-Host 'Startup check verifies process/port only; it is not an end-to-end business test.'
    } catch {
        $deploymentError = $_
        Write-Warning "Deployment failed; restoring $service from $backup"
        try {
            Invoke-Script $manager $stopArguments
            $failed = Assert-Within (Join-Path $deploymentRoot "$stamp/failed") $operationBoundary
            New-Item -ItemType Directory -Force -Path $failed | Out-Null
            foreach ($item in $installed) {
                if (Test-Path -LiteralPath (Join-Path $destination $item)) {
                    Move-Checked (Join-Path $destination $item) (Join-Path $failed $item) $operationBoundary
                }
            }
            foreach ($item in $moved) { Move-Checked (Join-Path $backup $item) (Join-Path $destination $item) $operationBoundary }
            Invoke-Script $manager $startArguments
            Write-Warning 'Previous binaries restored and startup checked.'
        } catch {
            throw "Deployment failed: $deploymentError. Recovery also failed: $_. Backup: $backup"
        }
        throw $deploymentError
    }
}

if ($Action -eq 'deploy') { Assert-Environment }
Write-Host "ACTION=$Action TARGETS=$($targets -join ',') ATS_ROOT=$AtsRoot"
if ($Plan) {
    if (!$NoBuild -and $javaTargets.Count -gt 0) {
        Write-Host "Maven settings: $SettingsFile"
        Write-Host "Java: build-server.ps1 -Clean -Module $Target (parent/common first, tests included)"
    }
    if (!$NoBuild -and $targets -contains 'client') {
        Write-Host 'Client: build-native-client.ps1 -OutputRoot <isolated package directory>'
    }
    if ($Action -eq 'deploy') { Write-Host "Deploy $($targets -join ',') using existing lifecycle scripts; NoBuild=$NoBuild" }
    exit 0
}
New-Item -ItemType Directory -Force -Path $runRoot | Out-Null
$lock = $null
$transcribing = $false
$previousJavaHome = $env:JAVA_HOME
$previousPath = $env:PATH
try {
    $lock = [IO.File]::Open((Join-Path $buildRoot 'build-deploy.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    Start-Transcript -LiteralPath (Join-Path $runRoot "$Action.log") | Out-Null
    $transcribing = $true
    $packages = @{}
    if (!$NoBuild) {
        if ($javaTargets.Count -gt 0) {
            $localJdk = Join-Path $iterationRoot 'runtime/tools/jdk8/jdk8u504-b01'
            if (!$env:JAVA_HOME -and (Test-Path "$localJdk/bin/java.exe")) { $env:JAVA_HOME = $localJdk }
            if ($env:JAVA_HOME) { $env:PATH = (Join-Path $env:JAVA_HOME 'bin') + ';' + $env:PATH }
            $javaModule = if ($Target -eq 'all') { 'all' } else { $Target }
            Invoke-Script (Join-Path $PSScriptRoot 'build-server.ps1') @('-Module', $javaModule, '-SettingsFile', $SettingsFile, '-Clean')
        }
        if ($targets -contains 'client') {
            Invoke-Script (Join-Path $clientSource 'scripts/build-native-client.ps1') @('-OutputRoot', (Join-Path $runRoot 'client/payload'))
        }
        foreach ($name in $targets) { $packages[$name] = Write-Package $name }
        # Publish only after every selected build and package check has succeeded.
        foreach ($name in $targets) {
            $packages[$name] | Set-Content -LiteralPath (Join-Path $buildRoot "latest-$name.txt") -Encoding UTF8
        }
    } else {
        foreach ($name in $targets) {
            $pointer = Join-Path $buildRoot "latest-$name.txt"
            if (!(Test-Path -LiteralPath $pointer)) { throw "No successful packaged build for $name. Run .\build $name first." }
            $packages[$name] = (Get-Content -LiteralPath $pointer -Raw -Encoding UTF8).Trim()
        }
    }
    if ($Action -eq 'deploy') {
        $validated = @{}
        foreach ($name in $targets) { $validated[$name] = Read-Package $packages[$name] $name }
        foreach ($name in $targets) { Deploy-Package $name $validated[$name] }
    }
    Write-Host "SUCCESS $Action $Target. Log: $runRoot/$Action.log"
} finally {
    $env:JAVA_HOME = $previousJavaHome
    $env:PATH = $previousPath
    if ($transcribing) { Stop-Transcript | Out-Null }
    if ($lock) { $lock.Dispose() }
}

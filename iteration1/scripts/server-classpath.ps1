param([Parameter(Mandatory=$true)][ValidateSet('quotesvr', 'notificationsvr')][string]$Server)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$artifact = "cgws-ficc-saletrading-$Server"
$target = Join-Path $taskRoot "$artifact/target-compat"
$serverJar = Join-Path $target "$artifact-0.1.0-SNAPSHOT.jar"
$package = Join-Path $target "$artifact-0.1.0-SNAPSHOT-dist.zip"
foreach ($file in @($serverJar, $package)) {
    if (!(Test-Path -LiteralPath $file)) { throw "Missing $file. Run scripts/build-server.ps1 first." }
}
# Use the exact ordered Class-Path in the Maven JAR manifest.
# Extract only flat dependency JARs from our service distribution.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$library = Join-Path $target 'lib'
New-Item -ItemType Directory -Force -Path $library | Out-Null
$archive = [IO.Compression.ZipFile]::OpenRead($package)
try {
    $prefix = "$artifact/lib/"
    foreach ($entry in $archive.Entries) {
        if (!$entry.FullName.StartsWith($prefix) -or !$entry.FullName.EndsWith('.jar')) { continue }
        $name = $entry.FullName.Substring($prefix.Length)
        if ([IO.Path]::GetFileName($name) -ne $name) { throw 'Unexpected nested dependency path in distribution.' }
        $destination = Join-Path $library $name
        if (!(Test-Path -LiteralPath $destination) -or (Get-Item -LiteralPath $destination).LastWriteTimeUtc -lt (Get-Item -LiteralPath $package).LastWriteTimeUtc) {
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destination, $true)
        }
    }
} finally {
    $archive.Dispose()
}
$serverJar

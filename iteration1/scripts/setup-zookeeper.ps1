$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$version = '3.8.6'
$toolsDirectory = Join-Path $taskRoot 'runtime/tools'
$distribution = "apache-zookeeper-$version-bin"
$archive = Join-Path $toolsDirectory "$distribution.tar.gz"
$checksumFile = "$archive.sha512"
# downloads.apache.org only carries the current release; older ones live in the archive.
$baseUrls = @(
    "https://archive.apache.org/dist/zookeeper/zookeeper-$version",
    "https://downloads.apache.org/zookeeper/zookeeper-$version"
)
$baseUrl = $null
foreach ($candidate in $baseUrls) {
    $probeUrl = "$candidate/$distribution.tar.gz.sha512"
    try {
        $request = [Net.HttpWebRequest]::Create($probeUrl)
        $request.Method = 'HEAD'
        $request.Timeout = 15000
        $response = $request.GetResponse()
        $response.Close()
        $baseUrl = $candidate
        break
    } catch {
        Write-Output "ZooKeeper mirror unavailable: $probeUrl"
    }
}
if (!$baseUrl) {
    throw 'No reachable ZooKeeper download mirror.'
}

New-Item -ItemType Directory -Force -Path $toolsDirectory | Out-Null
foreach ($download in @(@("$baseUrl/$distribution.tar.gz", $archive), @("$baseUrl/$distribution.tar.gz.sha512", $checksumFile))) {
    if (!(Test-Path -LiteralPath $download[1])) {
        & python (Join-Path $PSScriptRoot 'download-file.py') $download[0] $download[1]
        if ($LASTEXITCODE -ne 0) {
            throw "ZooKeeper download failed: $($download[0])"
        }
    }
}

$checksumText = Get-Content -LiteralPath $checksumFile -Raw
$expected = [regex]::Match($checksumText, '(?i)\b[0-9a-f]{128}\b').Value
$actual = (Get-FileHash -LiteralPath $archive -Algorithm SHA512).Hash
if (!$expected -or $expected -ine $actual) {
    throw 'ZooKeeper archive SHA-512 mismatch. The archive has not been extracted.'
}

$installation = Join-Path $toolsDirectory $distribution
if (!(Test-Path -LiteralPath (Join-Path $installation "lib/zookeeper-$version.jar"))) {
    & tar -xzf $archive -C $toolsDirectory
    if ($LASTEXITCODE -ne 0) {
        throw 'ZooKeeper extraction failed.'
    }
}

Write-Output "Verified and installed ZooKeeper $version at $installation"

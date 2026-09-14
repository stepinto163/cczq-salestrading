param([string]$Jdk8Home = 'F:\dev\jdk8-temurin')

# Builds the sun.misc BASE64 shims that the STC/CEDA stack still needs on JDK 9+.
# The classes must be compiled by a JDK 8 javac (package sun.misc exists there) and
# are injected at run time with:
#   java --patch-module jdk.unsupported=<this directory>\classes ...

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$output = Join-Path $here 'classes'
$javac = Join-Path $Jdk8Home 'bin/javac.exe'
if (!(Test-Path -LiteralPath $javac)) {
    throw "JDK 8 javac not found at $javac. Pass -Jdk8Home with a JDK 8 installation."
}

New-Item -ItemType Directory -Force -Path $output | Out-Null
& $javac -Xprefer:source -nowarn -d $output `
    (Join-Path $here 'sun/misc/BASE64Encoder.java') `
    (Join-Path $here 'sun/misc/BASE64Decoder.java')
if ($LASTEXITCODE -ne 0) {
    throw 'shim compilation failed'
}
Write-Output "Shim classes written to $output"
Write-Output "Run with: java --patch-module jdk.unsupported=$output ..."

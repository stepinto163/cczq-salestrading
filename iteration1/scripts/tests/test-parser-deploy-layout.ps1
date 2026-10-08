$ErrorActionPreference = 'Stop'
$source = Join-Path (Split-Path $PSScriptRoot -Parent) 'product.ps1'
$tokens = $null
$errors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile($source, [ref]$tokens, [ref]$errors)
if ($errors.Count) { throw ($errors | Out-String) }
foreach ($name in @('Assert-Within', 'Move-Checked', 'Deploy-Package')) {
    $definition = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name }, $true)
    . ([scriptblock]::Create($definition.Extent.Text))
}
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('ficc-parser-deploy-' + [Guid]::NewGuid().ToString('N'))
$AtsRoot = Join-Path $testRoot 'ats_root'
$destination = Join-Path $AtsRoot 'stc/ParserSvr'
$payload = Join-Path $testRoot 'payload'
$services = @{ parsersvr = 'ParserSvr' }
$ClientConfig = 'unused'
$script:failStart = $false
function Invoke-Script([string]$Path, [object[]]$Arguments) {
    if ($script:failStart -and $Arguments -contains 'start') {
        $script:failStart = $false
        throw 'controlled startup failure'
    }
}
New-Item -ItemType Directory -Force -Path "$destination/javalib", "$destination/config", "$destination/data/dictionary", "$payload/javalib" | Out-Null
Set-Content -LiteralPath "$destination/javalib/cgws-ficc-parser-1.0.0-SNAPSHOT.jar" -Value 'old engine'
Set-Content -LiteralPath "$destination/javalib/sc-app-stc-parsersvr-local.jar" -Value 'old service'
Set-Content -LiteralPath "$destination/config/database.xml" -Value 'preserved configuration'
Set-Content -LiteralPath "$destination/data/dictionary/reference.txt" -Value 'preserved dictionary'
Set-Content -LiteralPath "$destination/ParseLib.dll" -Value 'preserved native library'
Set-Content -LiteralPath "$payload/javalib/dependency.jar" -Value 'dependency'
$application = 'cgws-ficc-parsersvr-0.1.0-SNAPSHOT.jar'
Set-Content -LiteralPath (Join-Path $payload $application) -Value 'merged service and engine'
$package = [pscustomobject]@{ payload=$payload; application=$application }
$stamp = 'first'
Deploy-Package 'parsersvr' $package
if (!(Test-Path -LiteralPath "$destination/javalib/$application")) { throw 'Merged service must be installed in javalib' }
if (Test-Path -LiteralPath "$destination/javalib/cgws-ficc-parser-1.0.0-SNAPSHOT.jar") { throw 'Old engine JAR was retained' }
if (Test-Path -LiteralPath "$destination/javalib/sc-app-stc-parsersvr-local.jar") { throw 'Old service JAR was retained' }
if ((Get-Content -LiteralPath "$destination/config/database.xml") -ne 'preserved configuration') { throw 'Configuration changed' }
if ((Get-Content -LiteralPath "$destination/ParseLib.dll") -ne 'preserved native library') { throw 'Native library changed' }
if ((Get-Content -LiteralPath "$destination/data/dictionary/reference.txt") -ne 'preserved dictionary') { throw 'Dictionary changed' }
Set-Content -LiteralPath (Join-Path $payload $application) -Value 'broken service'
$stamp = 'second'
$script:failStart = $true
try {
    Deploy-Package 'parsersvr' $package
    throw 'Expected deployment failure'
} catch {
    if ($_.ToString() -notmatch 'controlled startup failure') { throw }
}
if ((Get-Content -LiteralPath "$destination/javalib/$application") -ne 'merged service and engine') { throw 'Rollback did not restore merged service' }
Write-Output "PASS ParserSvr merged-JAR deployment, old-JAR removal, configuration/native data preservation and rollback. Fixtures: $testRoot"

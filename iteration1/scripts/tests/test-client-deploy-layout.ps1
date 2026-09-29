$ErrorActionPreference = 'Stop'
$source = Join-Path (Split-Path $PSScriptRoot -Parent) 'product.ps1'
$tokens = $null
$errors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile($source, [ref]$tokens, [ref]$errors)
if ($errors.Count) { throw ($errors | Out-String) }
# Load only filesystem deployment functions, never the command entry point or real process manager.
foreach ($name in @('Assert-Within', 'Move-Checked', 'Deploy-Package')) {
    $definition = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name }, $true)
    $scriptDirectoryLiteral = "'" + (Split-Path $source -Parent).Replace("'", "''") + "'"
    $definitionText = $definition.Extent.Text.Replace('$PSScriptRoot', $scriptDirectoryLiteral)
    . ([scriptblock]::Create($definitionText))
}
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('ficc-client-deploy-' + [Guid]::NewGuid().ToString('N'))
$clientSource = Join-Path $testRoot 'source'
$destination = Join-Path $clientSource 'runtime/native-client'
$payload = Join-Path $testRoot 'payload'
$ClientConfig = 'unused-test-config'
$services = @{}
$script:failStart = $false
function Invoke-Script([string]$Path, [object[]]$Arguments) {
    if ($script:failStart -and $Arguments -contains 'start') {
        $script:failStart = $false
        throw 'controlled startup failure'
    }
}
New-Item -ItemType Directory -Force -Path "$destination/bin", "$destination/.deploy/old/previous/bin", "$payload/bin" | Out-Null
Set-Content -LiteralPath "$destination/bin/plugin.dll" -Value 'old'
Set-Content -LiteralPath "$destination/.deploy/old/previous/bin/plugin.dll" -Value 'ancient'
Set-Content -LiteralPath "$destination/STC.exe" -Value 'old'
Set-Content -LiteralPath "$payload/bin/plugin.dll" -Value 'new'
Set-Content -LiteralPath "$payload/STC.exe" -Value 'new'
$stamp = 'first'
Deploy-Package 'client' ([pscustomobject]@{ payload=$payload })
$plugins = @(Get-ChildItem -LiteralPath $destination -Recurse -Filter '*.dll')
if ($plugins.Count -ne 1 -or (Get-Content -LiteralPath $plugins[0].FullName) -ne 'new') { throw 'Plugin scan sees old or staged assemblies' }
if (!(Test-Path -LiteralPath "$clientSource/runtime/native-client-deployment-backups/legacy-first/old/previous/bin/plugin.dll")) { throw 'Legacy backup was not preserved outside the installation' }
Set-Content -LiteralPath "$payload/bin/plugin.dll" -Value 'broken'
$stamp = 'second'
$script:failStart = $true
try { Deploy-Package 'client' ([pscustomobject]@{ payload=$payload }); throw 'Expected deployment failure' }
catch { if ($_.ToString() -notmatch 'controlled startup failure') { throw } }
if ((Get-Content -LiteralPath "$destination/bin/plugin.dll") -ne 'new') { throw 'Rollback failed to restore the working binary' }
if (@(Get-ChildItem -LiteralPath $destination -Recurse -Filter '*.dll').Count -ne 1) { throw 'Rollback left duplicate discoverable DLLs' }
Write-Output "PASS successful deployment, legacy backup migration, rollback and plugin-scan isolation. Fixtures: $testRoot"

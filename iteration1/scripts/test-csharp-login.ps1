param(
    [Parameter(Mandatory = $true)][string]$AssemblyPath
)
$ErrorActionPreference = 'Stop'
$resolvedAssembly = (Resolve-Path -LiteralPath $AssemblyPath).Path
$libraryDirectory = Split-Path $resolvedAssembly -Parent
[void][Reflection.Assembly]::LoadFrom((Join-Path $libraryDirectory 'gateway-api.dll'))
$assembly = [Reflection.Assembly]::LoadFrom($resolvedAssembly)
$clientType = $assembly.GetType('Ficc.SalesTrading.DpsClient', $true)
$method = $clientType.GetMethod('ValidateLoginReply', [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $method) {
    throw 'Build the updated C# application before running this test.'
}
$cases = @(
    @{ Json='{"Code":"0001","sid":"valid-stc-session","userid":"STC-42"}'; Accepted=$true },
    @{ Json='{"Code":"0001","sid":"SIM-valid-session","userid":"U-001"}'; Accepted=$true },
    @{ Json='{"Code":"0001","sid":null,"userid":null}'; Accepted=$false },
    @{ Json='{"Code":"0001"}'; Accepted=$false },
    @{ Json='{"Code":"0001","sid":"null"}'; Accepted=$false },
    @{ Json='{"Code":"0001","sid":" NULL "}'; Accepted=$false },
    @{ Json='{"Code":"0000","sid":" "}'; Accepted=$false },
    @{ Json='{"Code":"5006","Msg":"user not exsit","sid":"irrelevant"}'; Accepted=$false }
)
$checks = 0
foreach ($case in $cases) {
    $reply = [Newtonsoft.Json.Linq.JObject]::Parse($case.Json)
    $accepted = $true
    $arguments = New-Object object[] 1
    $arguments[0] = $reply
    try {
        [void]$method.Invoke($null, $arguments)
    } catch {
        if ($_.Exception.ToString() -notmatch 'InvalidOperationException') {
            throw
        }
        $accepted = $false
    }
    if ($accepted -ne $case.Accepted) {
        throw "Login validation case $checks failed."
    }
    $checks++
}
Write-Output "PASS: $checks login-reply checks; no network connections or quote requests."

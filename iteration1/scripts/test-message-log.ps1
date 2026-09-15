param(
    [string]$ClientDirectory = (Join-Path $PSScriptRoot '../client/bin/FilterPerformance')
)

$ErrorActionPreference = 'Stop'
$clientFolder = (Resolve-Path $ClientDirectory).Path
# This DPS DLL embeds Newtonsoft.Json; there is no separate Newtonsoft assembly.
[void][Reflection.Assembly]::LoadFrom((Join-Path $clientFolder 'gateway-api.dll'))
$clientAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $clientFolder 'Ficc.SalesTradingClient.exe'))
$logType = $clientAssembly.GetType('Ficc.SalesTrading.MessageLog', $true)
$redactMethod = $logType.GetMethod('Redact', [Reflection.BindingFlags]'NonPublic,Static')
$original = [Newtonsoft.Json.Linq.JToken]::Parse(@'
{
  "Password": "fake-secret",
  "quotes": [
    {
      "quoteId": "Q-1",
      "quantity": 1000,
      "nested": {"accessToken": "fake-token", "label": "retained", "empty": null}
    },
    null
  ],
  "authorization": {"value": "fake-auth"},
  "passwd": "fake-password",
  "REFRESHTOKEN": "fake-refresh"
}
'@)
$expected = [Newtonsoft.Json.Linq.JToken]::Parse(@'
{
  "Password": "[REDACTED]",
  "quotes": [
    {
      "quoteId": "Q-1",
      "quantity": 1000,
      "nested": {"accessToken": "[REDACTED]", "label": "retained", "empty": null}
    },
    null
  ],
  "authorization": "[REDACTED]",
  "passwd": "[REDACTED]",
  "REFRESHTOKEN": "[REDACTED]"
}
'@)
$before = $original.ToString()
$result = $redactMethod.Invoke($null, [object[]]@(,$original))
if (-not [Newtonsoft.Json.Linq.JToken]::DeepEquals($result, $expected)) {
    throw 'Nested log redaction failed'
}
if ($original.ToString() -ne $before) {
    throw 'Redaction mutated the business message'
}
if ($null -ne $redactMethod.Invoke($null, [object[]]@(,$null))) {
    throw 'Null message redaction failed'
}
Write-Output 'PASS: nested fields, arrays, nulls, all sensitive keys, case-insensitive masking, unchanged source'

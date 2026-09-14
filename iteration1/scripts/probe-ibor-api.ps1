<#
.SYNOPSIS
    Read-only observability probe for the deployed IBOR (固收交易管理系统) HTTP surface.

.DESCRIPTION
    Replaces "SSH into the QA host and tail server logs" as the primary way to prove
    what the deployed service is doing.  The bastion used by this environment grants
    publickey authentication but refuses remote command execution AND TCP forwarding
    (`channel 1: open failed: administratively prohibited: port forwarding is disabled`),
    and port 22 is filtered on the STC hosts themselves.  The deployed system, however,
    exposes a fully scriptable HTTP surface, so verification is done at the API contract
    level instead:

      * public OpenAPI document  -> the authoritative contract our client must match
      * /auth/init               -> RSA public key used to encrypt the login payload
      * /auth/code               -> captcha endpoint, proves the service is live
      * /auth/login (empty body) -> negative control: the endpoint exists and enforces auth
      * /mon/login/log           -> login audit trail (token-gated), the exact evidence we
                                    previously wanted out of the server logs

    Everything here is read-only and issues no state-changing request.

.NOTES
    No environment address is hard-coded: pass it in, or keep it in the gitignored
    local configuration.  Never commit real addresses or credentials.

.EXAMPLE
    ./probe-ibor-api.ps1 -BaseUrl 'http://<ibor-host>/ibor-api'
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    # Optional path for the machine-readable evidence blob (kept out of git).
    [string]$OutFile,

    [int]$TimeoutSec = 8
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$BaseUrl = $BaseUrl.TrimEnd('/')

# The bastion/proxy hop is irrelevant here; go straight at the service.
$results = [System.Collections.Generic.List[object]]::new()

function Invoke-Probe {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [string]$Body,
        [int[]]$Expect,
        # Business codes to accept.  This gateway answers HTTP 200 for application-level
        # errors and puts the real verdict in the JSON `code` field, so asserting on the
        # HTTP status alone is not enough.
        [int[]]$ExpectBodyCode,
        # Pass when the body reports a deliberate rejection (code != 200).
        [switch]$ExpectReject
    )

    $url = "$BaseUrl$Path"
    $record = [ordered]@{
        name    = $Name
        method  = $Method
        path    = $Path
        url     = $url
        status  = 'error'
        http    = $null
        bizCode = $null
        ms      = $null
        detail  = ''
    }

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $reqArgs = @{
            Uri             = $url
            Method          = $Method
            TimeoutSec      = $TimeoutSec
            UseBasicParsing = $true
            Headers         = @{ Accept = 'application/json' }
        }
        if ($Body) {
            $reqArgs.Body        = $Body
            $reqArgs.ContentType = 'application/json'
        }

        # Invoke-WebRequest throws on 4xx/5xx; we WANT those responses, so read them off
        # the exception rather than treating them as transport failures.
        try {
            $resp = Invoke-WebRequest @reqArgs
            $code = [int]$resp.StatusCode
            $text = $resp.Content
        }
        catch [System.Net.WebException] {
            $web = $_.Exception.Response
            if (-not $web) { throw }
            $code = [int]$web.StatusCode
            $reader = New-Object System.IO.StreamReader($web.GetResponseStream())
            $text = $reader.ReadToEnd()
            $reader.Dispose()
        }

        $sw.Stop()
        $record.http = $code
        $record.ms   = $sw.ElapsedMilliseconds

        # Envelope: {"code":<int>,"msg":"...","data":...}
        try {
            $env0 = $text | ConvertFrom-Json
            if ($env0.PSObject.Properties.Name -contains 'code') { $record.bizCode = [int]$env0.code }
        }
        catch { }

        $ok = $false
        if ($ExpectReject) {
            $ok = ($null -ne $record.bizCode) -and ($record.bizCode -ne 200)
        }
        elseif ($ExpectBodyCode) {
            $ok = ($null -ne $record.bizCode) -and ($ExpectBodyCode -contains $record.bizCode)
        }
        elseif ($Expect) {
            $ok = ($Expect -contains $code)
        }
        $record.status = if ($ok) { 'pass' } else { 'fail' }

        # Pull out the bits that make the result reproducible in a report.
        $summary = $text
        if ($summary.Length -gt 220) { $summary = $summary.Substring(0, 220) + '...' }
        $record.detail = ($summary -replace '\s+', ' ').Trim()

        switch ($Name) {
            'openapi' {
                try {
                    $doc = $text | ConvertFrom-Json
                    $n = @($doc.paths.PSObject.Properties).Count
                    $srv = ($doc.servers | ForEach-Object { $_.url }) -join ','
                    $record.detail = "openapi=$($doc.openapi) paths=$n server=$srv"
                }
                catch { }
            }
            'auth-code' {
                try {
                    $doc = $text | ConvertFrom-Json
                    $d = $doc.data
                    $record.detail = "code=$($doc.code) captchaEnabled=$($d.captchaEnabled) uuid=$($d.uuid) imgLen=$($d.img.Length)"
                }
                catch { }
            }
            'auth-init' {
                try {
                    $doc = $text | ConvertFrom-Json
                    $d = $doc.data
                    $record.detail = "code=$($doc.code) dataType=$($d.GetType().Name) dataLen=$("$d".Length)"
                }
                catch { }
            }
        }
    }
    catch {
        $sw.Stop()
        $record.status = 'error'
        $record.ms     = $sw.ElapsedMilliseconds
        $record.detail = $_.Exception.Message
    }

    $script:results.Add([pscustomobject]$record)
}

Write-Host "IBOR API observability probe" -ForegroundColor Cyan
Write-Host "base: $BaseUrl" -ForegroundColor DarkGray
Write-Host ''

# 1. Contract: the authoritative API surface our client is validated against.
Invoke-Probe -Name 'openapi'   -Method GET  -Path '/v3/api-docs'   -Expect 200
# 2. Public key for the encrypted login payload (VITE_GLOB_ENABLE_ENCRYPT=true).
Invoke-Probe -Name 'auth-init' -Method GET  -Path '/auth/init'     -ExpectBodyCode 200
# 3. Captcha: proves the app is live and serving real business endpoints.
Invoke-Probe -Name 'auth-code' -Method GET  -Path '/auth/code'     -ExpectBodyCode 200
# 4. Negative control: the login endpoint exists and rejects an unauthenticated call.
Invoke-Probe -Name 'login-neg' -Method POST -Path '/auth/login'    -Body '{}' -ExpectReject
# 5. The login audit trail - token-gated, so code 401 here means "reachable, needs a token".
Invoke-Probe -Name 'login-log' -Method GET  -Path '/mon/login/log' -ExpectBodyCode 401

$results | Format-Table -AutoSize name, method, path, status, http, bizCode, ms, detail

$pass = @($results | Where-Object status -eq 'pass').Count
$total = $results.Count
Write-Host ''
if ($pass -eq $total) {
    Write-Host "RESULT: $pass/$total checks passed - deployed service reachable at API level." -ForegroundColor Green
}
else {
    Write-Host "RESULT: $pass/$total checks passed." -ForegroundColor Yellow
}

if ($OutFile) {
    $payload = [ordered]@{
        baseUrl    = $BaseUrl
        probedAt   = (Get-Date).ToString('o')
        passed     = $pass
        total      = $total
        checks     = $results
    }
    $payload | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $OutFile -Encoding UTF8
    Write-Host "evidence written: $OutFile" -ForegroundColor DarkGray
}

if ($pass -ne $total) { exit 1 }

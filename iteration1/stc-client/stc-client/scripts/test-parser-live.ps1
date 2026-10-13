param([string]$OutputRoot)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path $PSScriptRoot -Parent
if (!$OutputRoot) { $OutputRoot = Join-Path $clientRoot 'runtime/native-client' }
# 复用可管理、带清理和看门狗的产品工作流，保留旧账号环境变量兼容性。
$previousUser = $env:STC_UI_USER
$previousPassword = $env:STC_UI_PASSWORD
try {
    if ($env:PARSER_TEST_USER) { $env:STC_UI_USER = $env:PARSER_TEST_USER }
    if ($env:PARSER_TEST_PASSWORD) { $env:STC_UI_PASSWORD = $env:PARSER_TEST_PASSWORD }
    & (Join-Path $PSScriptRoot 'run-ui-tests.ps1') -StcConfig (Join-Path $OutputRoot 'STC.exe.config') -Cases '01-quote-lifecycle,02-precise-crm,09-multiline-partial-confirm,10-semicolon-quotes,14-watchlist-notification-read,16-enter-multiline,23-complex-parser'
} finally {
    $env:STC_UI_USER = $previousUser
    $env:STC_UI_PASSWORD = $previousPassword
}
exit $LASTEXITCODE

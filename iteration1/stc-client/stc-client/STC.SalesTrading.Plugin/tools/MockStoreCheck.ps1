$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$pluginDir = if ($env:MOCK_STORE_DLL_DIR) { $env:MOCK_STORE_DLL_DIR } else { Join-Path $repoRoot 'client\plugin' }
$bin = Join-Path $repoRoot 'client\bin'

[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $args)
    $name = ($args.Name -split ',')[0]
    foreach ($dir in @($pluginDir, $bin)) {
        $path = Join-Path $dir ($name + '.dll')
        if (Test-Path -LiteralPath $path) { return [System.Reflection.Assembly]::LoadFrom($path) }
    }
    return $null
})

[System.Reflection.Assembly]::LoadFrom((Join-Path $pluginDir 'STC.SalesTrading.Plugin.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'STC.Common.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'STC.Frame.dll')) | Out-Null

$store = New-Object STC.SalesTrading.Plugin.Mock.MockSalesTradingStore
$quote = New-Object STC.SalesTrading.Plugin.Mock.MockQuoteService -ArgumentList $store
$market = New-Object STC.SalesTrading.Plugin.Mock.MockMarketCenterService -ArgumentList (New-Object STC.SalesTrading.Plugin.Mock.MockRequestOptions), $store
$mgmt = New-Object STC.SalesTrading.Plugin.Mock.MockQuoteMgmtService -ArgumentList (New-Object STC.SalesTrading.Plugin.Mock.MockRequestOptions), $store
$ct = [System.Threading.CancellationToken]::None
$probe = New-Object STC.SalesTrading.Plugin.Mock.MockQuoteEventProbe
$eventHandler = $probe.Handler
$market.SubscribeQuoteEvents($eventHandler)

$pending = @($quote.LoadPendingRowsAsync($ct).Result)
if ($pending.Count -eq 0) { throw '待确认报价种子为空' }
$first = $pending[0]
$ids = New-Object 'System.Collections.Generic.List[string]'
$ids.Add($first.RecognitionId)
$null = $quote.ConfirmQuotesAsync($ids, $ct).GetAwaiter().GetResult()
$remaining = @($quote.LoadPendingRowsAsync($ct).Result)
if ($remaining.Count -ne ($pending.Count - 1)) { throw '确认后待确认数量未减少' }
$events = $probe.Snapshot()
if (-not ($events | Where-Object { $_.Kind -eq [STC.SalesTrading.Plugin.Abstractions.Models.QuoteEventKind]::New -and $_.QuoteId -eq ('mock-quote-' + $first.RecognitionId) })) { throw '确认报价未发布 New 推送事件' }

$marketQuery = New-Object STC.SalesTrading.Plugin.Abstractions.Queries.MarketBoardQuery
$marketQuery.Market = if ($first.Name -match '国债|国开|农发|进出') { [STC.SalesTrading.Plugin.Abstractions.Models.BondMarket]::Rate } else { [STC.SalesTrading.Plugin.Abstractions.Models.BondMarket]::Credit }
$board = @($market.QueryBoardRowsAsync($marketQuery, $ct).Result.Items)
if (-not ($board | Where-Object { $_.QuoteId -eq ('mock-quote-' + $first.RecognitionId) })) { throw '确认后行情中心未出现报价' }

$todayQuery = New-Object STC.SalesTrading.Plugin.Abstractions.Queries.QuoteMgmtQuery
$todayQuery.Today = $true
$today = @($mgmt.QueryTodayRowsAsync($todayQuery, $ct).Result.Items)
$confirmed = $today | Where-Object { $_.QuoteId -eq ('mock-quote-' + $first.RecognitionId) }
if (-not $confirmed) { throw '确认后今日报价未出现报价' }
$ids.Clear()
$ids.Add($confirmed.QuoteId)
$null = $mgmt.DeleteQuotesAsync($ids, $false, $ct).GetAwaiter().GetResult()
$todayAfterDelete = @($mgmt.QueryTodayRowsAsync($todayQuery, $ct).Result.Items)
if ($todayAfterDelete | Where-Object { $_.QuoteId -eq $confirmed.QuoteId }) { throw '删除今日报价后刷新仍恢复' }
$events = $probe.Snapshot()
if (-not ($events | Where-Object { $_.Kind -eq [STC.SalesTrading.Plugin.Abstractions.Models.QuoteEventKind]::Revoke -and $_.QuoteId -eq $confirmed.QuoteId })) { throw '删除报价未发布 Revoke 推送事件' }

$intent = New-Object STC.SalesTrading.Plugin.Abstractions.Models.BoardIntentRow
$intent.IntentId = 'mock-check-intent'
$intent.Content = 'bid 验收意向'
$intent.Status = [STC.SalesTrading.Plugin.Abstractions.Models.IntentStatus]::Active
$intentRows = New-Object 'System.Collections.Generic.List[STC.SalesTrading.Plugin.Abstractions.Models.BoardIntentRow]'
$intentRows.Add($intent)
$null = $quote.SubmitIntentQuotesAsync($intentRows, $ct).GetAwaiter().GetResult()
$intents = @($market.LoadIntentRowsAsync([datetime]::Today.AddDays(-1), [datetime]::Today.AddDays(1), $ct).Result)
if (-not ($intents | Where-Object { $_.IntentId -eq $intent.IntentId })) { throw '转意向后刷新未找到意向' }
$ids.Clear()
$ids.Add($intent.IntentId)
$null = $market.SetIntentStatusAsync($ids, [STC.SalesTrading.Plugin.Abstractions.Models.IntentStatus]::Revoked, $ct).GetAwaiter().GetResult()
$updated = @($market.LoadIntentRowsAsync([datetime]::Today.AddDays(-1), [datetime]::Today.AddDays(1), $ct).Result | Where-Object { $_.IntentId -eq $intent.IntentId })
if ($updated.Count -ne 1 -or $updated[0].Status -ne [STC.SalesTrading.Plugin.Abstractions.Models.IntentStatus]::Revoked) { throw '意向状态修改后刷新不一致' }
$market.UnsubscribeQuoteEvents($eventHandler)

Write-Host 'MOCK STORE PASS'

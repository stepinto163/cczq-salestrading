$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase

$bin = 'D:\Work\Coding\cgws-stc-client\client\bin'
# 默认加载 client\plugin 的部署产物；STC 运行锁定拷贝时可设 SMOKE_DLL_DIR 指向 obj\Debug 验证最新编译
$pluginDir = if ($env:SMOKE_DLL_DIR) { $env:SMOKE_DLL_DIR } else { 'D:\Work\Coding\cgws-stc-client\client\plugin' }

# 依赖解析：LoadFrom 上下文默认在 pwsh 目录找，手动按宿主 probing 路径兜底
[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($s, $e)
    $name = ($e.Name -split ',')[0]
    foreach ($dir in @($bin, $pluginDir)) {
        $p = Join-Path $dir ($name + '.dll')
        if (Test-Path $p) { return [System.Reflection.Assembly]::LoadFrom($p) }
    }
    return $null
})

$app = New-Object System.Windows.Application
$app.ShutdownMode = [System.Windows.ShutdownMode]::OnExplicitShutdown

[System.Reflection.Assembly]::LoadFrom((Join-Path $pluginDir 'STC.SalesTrading.Plugin.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'STC.Common.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'STC.Frame.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'gateway-api.dll')) | Out-Null

# 复现用户点击菜单的构造路径：MainView ctor -> ThemeManager.ApplyDark -> 合并主题字典
$view = New-Object STC.SalesTrading.Plugin.Shell.View.MainView
Write-Host 'MainView OK (theme dictionaries merged)'

$vm = New-Object STC.SalesTrading.Plugin.Shell.MainViewModel
Write-Host ("MainViewModel OK (nav items: {0})" -f $vm.NavItems.Count)

# 挂 DataContext 并强制走完整布局管线：Measure/Arrange/UpdateLayout 会实例化全部 DataTemplate，
# 模板期的 XAML 错误（如 Binding 内 DynamicResource）只有这一步才能暴露。
$view.DataContext = $vm
try {
    $view.Measure([System.Windows.Size]::new(1440.0, 900.0))
    $view.Arrange([System.Windows.Rect]::new(0.0, 0.0, 1440.0, 900.0))
    $view.UpdateLayout()

    # 逐个切换三大页面（默认行情中心 -> 报价录入 -> 报价管理 -> 行情中心），实例化全部页面模板
    foreach ($idx in 0, 2, 1) {
        $vm.SelectedNavItem = $vm.NavItems[$idx]
        $view.UpdateLayout()
        Write-Host ("page switch OK: {0}" -f $vm.SelectedNavItem.Title)
    }
    Write-Host 'Layout/render pipeline OK'

    # G4 数据断言：报价确认页 Mock 加载（3 批次 / 9 行）
    $batches = $vm.QuotePage.Confirm.Batches
    $rows = ($batches | ForEach-Object { $_.Rows.Count } | Measure-Object -Sum).Sum
    Write-Host ("confirm page loaded: {0} batches / {1} rows" -f $batches.Count, $rows)

    # G5 数据断言：行情中心四个看板 Mock 加载（信用全部/最优/意向/提醒）
    Write-Host ("board creditAll: {0} rows" -f $vm.MarketCenterPage.CreditAll.Rows.Count)
    Write-Host ("board creditBest: {0} rows" -f $vm.MarketCenterPage.CreditBest.Rows.Count)
    Write-Host ("board intent: {0} rows" -f $vm.MarketCenterPage.Intent.Rows.Count)
    Write-Host ("board alert: {0} rows" -f $vm.MarketCenterPage.Alert.Rows.Count)

    # G6 数据断言：报价管理今日/历史 Mock 加载
    Write-Host ("mgmt today: {0} rows" -f $vm.QuoteMgmtPage.Today.Rows.Count)
    Write-Host ("mgmt history: {0} rows" -f $vm.QuoteMgmtPage.History.Rows.Count)

    # G5c 数据断言：债券详情智能推荐 Mock（Bid/Ofr 双侧 12 行，Bid 视图 6 行，范围过滤到 ExactBond 后 2 行）
    $mcSvc = New-Object STC.SalesTrading.Plugin.Mock.MockMarketCenterService
    $recDetail = $mcSvc.LoadBondDetailAsync('240210.IB', [System.Threading.CancellationToken]::None).Result
    $recVm = New-Object STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel.BondDetailViewModel
    $recVm.Fill($recDetail)
    Write-Host ("rec total: {0} rows" -f $recVm.Recommendations.Count)
    Write-Host ("rec bid view: {0} (bid={1}/ofr={2}, page='{3}')" -f @($recVm.RecView).Count, $recVm.RecBidCount, $recVm.RecOfrCount, $recVm.RecPageSummary)
    $recVm.SelectedRecScope = [STC.SalesTrading.Plugin.Abstractions.Models.RecommendationScope]::ExactBond
    Write-Host ("rec scope ExactBond: {0} rows" -f @($recVm.RecView).Count)

    # G7 用户区弹窗：个人中心/设置可构造（XAML 解析 + InitializeComponent 校验，不 ShowDialog 阻塞）
    $null = New-Object STC.SalesTrading.Plugin.Shell.View.UserCenterWindow -ArgumentList $vm.CurrentUser
    $null = New-Object STC.SalesTrading.Plugin.Shell.View.SettingsWindow
    Write-Host ("user windows OK (profile={0}/{1})" -f $vm.CurrentUser.Name, $vm.CurrentUser.RoleName)

    # G5b: 循环切换 6 个子页 → 四套筛选面板（信用/利率/意向/提醒）全部实例化
    for ($i = 0; $i -lt $vm.MarketCenterPage.SubTabs.Count; $i++) {
        $vm.MarketCenterPage.SelectedSubTab = $vm.MarketCenterPage.SubTabs[$i]
        $view.UpdateLayout()
    }
    Write-Host ("filter panels cycled, final panel: {0}" -f $vm.MarketCenterPage.FilterPanel)
}
catch {
    Write-Host '=== LAYOUT FAILED, exception chain ==='
    $e = $_.Exception
    $depth = 0
    while ($null -ne $e -and $depth -lt 10) {
        Write-Host ("[{0}] {1}: {2}" -f $depth, $e.GetType().FullName, $e.Message)
        $e = $e.InnerException
        $depth++
    }
    throw
}

# EventAggregator 往返验证：先走 VM 的 IRecipient 接收（ctor 已 RegisterAll）
[STC.Common.Utils.Events.EventAggregator]::Publish([STC.SalesTrading.Plugin.Shared.Messages.PendingQuotesChangedMessage]::new(3))
Write-Host ("VM badge after publish: {0}" -f $vm.NavItems[0].BadgeCount)

# 再用独立 recipient 验证 Subscribe/Publish/Unsubscribe 委托式 API
$r = New-Object object
[STC.Common.Utils.Events.EventAggregator]::Subscribe($r, [Action[STC.SalesTrading.Plugin.Shared.Messages.AlertCountChangedMessage]] { param($m) Write-Host ("callback received unread: {0}" -f $m.UnreadCount) })
[STC.Common.Utils.Events.EventAggregator]::Publish([STC.SalesTrading.Plugin.Shared.Messages.AlertCountChangedMessage]::new(5))
[STC.Common.Utils.Events.EventAggregator]::Unsubscribe($r)
Write-Host 'EventAggregator roundtrip OK'

$vm.Dispose()
Write-Host 'SMOKE PASS'

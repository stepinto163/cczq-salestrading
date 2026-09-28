$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase

$bin = 'D:\Work\Coding\cgws-stc-client\client\bin'
$pluginDir = if ($env:SMOKE_DLL_DIR) { $env:SMOKE_DLL_DIR } else { 'D:\Work\Coding\cgws-stc-client\client\plugin' }
[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($s, $e)
    $name = ($e.Name -split ',')[0]
    foreach ($dir in @($bin, $pluginDir)) {
        $p = Join-Path $dir ($name + '.dll')
        if (Test-Path $p) { return [System.Reflection.Assembly]::LoadFrom($p) }
    }
    return $null
})

$outDir = Join-Path $PSScriptRoot 'captures'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$app = New-Object System.Windows.Application
$app.ShutdownMode = [System.Windows.ShutdownMode]::OnExplicitShutdown

[System.Reflection.Assembly]::LoadFrom((Join-Path $pluginDir 'STC.SalesTrading.Plugin.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'STC.Common.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'STC.Frame.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'gateway-api.dll')) | Out-Null

$view = New-Object STC.SalesTrading.Plugin.Shell.View.MainView
$vm = New-Object STC.SalesTrading.Plugin.Shell.MainViewModel
$view.DataContext = $vm

$view.Measure([System.Windows.Size]::new(1440.0, 900.0))
$view.Arrange([System.Windows.Rect]::new(0.0, 0.0, 1440.0, 900.0))
$view.UpdateLayout()

function Snap([string]$name) {
    $view.UpdateLayout()
    $rtb = New-Object System.Windows.Media.Imaging.RenderTargetBitmap(1440, 900, 96.0, 96.0, [System.Windows.Media.PixelFormats]::Pbgra32)
    $rtb.Render($view)
    $enc = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $enc.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($rtb))
    $fs = [IO.File]::Create((Join-Path $outDir ($name + '.png')))
    $enc.Save($fs); $fs.Close()
    Write-Host "snap: $name"
}

function SwitchPage([int]$idx) { $vm.SelectedNavItem = $vm.NavItems[$idx]; $view.UpdateLayout() }
function SwitchSub([object]$pageVm, [int]$idx) { $pageVm.SelectedSubTab = $pageVm.SubTabs[$idx]; $view.UpdateLayout() }

# 行情中心（默认页）：6 子页逐个截图（筛选面板随之切换 4 套）
SwitchPage 1
for ($i = 0; $i -lt $vm.MarketCenterPage.SubTabs.Count; $i++) {
    SwitchSub $vm.MarketCenterPage $i
    Snap ("market-{0}" -f $vm.MarketCenterPage.SelectedSubTab.Title)
}

# 报价录入：3 子页
SwitchPage 0
for ($i = 0; $i -lt $vm.QuotePage.SubTabs.Count; $i++) {
    SwitchSub $vm.QuotePage $i
    Snap ("quote-{0}" -f $vm.QuotePage.SelectedSubTab.Title)
}

# 报价管理：2 子页
SwitchPage 2
for ($i = 0; $i -lt $vm.QuoteMgmtPage.SubTabs.Count; $i++) {
    SwitchSub $vm.QuoteMgmtPage $i
    Snap ("mgmt-{0}" -f $vm.QuoteMgmtPage.SelectedSubTab.Title)
}

$vm.Dispose()
Write-Host 'CAPTURE DONE'

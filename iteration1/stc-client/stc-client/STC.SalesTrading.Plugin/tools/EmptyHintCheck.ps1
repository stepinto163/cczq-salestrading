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
$app = New-Object System.Windows.Application
$app.ShutdownMode = [System.Windows.ShutdownMode]::OnExplicitShutdown
[System.Reflection.Assembly]::LoadFrom((Join-Path $pluginDir 'STC.SalesTrading.Plugin.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom((Join-Path $bin 'STC.Common.dll')) | Out-Null

# 真实窗口：验证空态由 DataGrid 自身 Background 触发器呈现，不创建窗口级 Adorner。
$grid = New-Object System.Windows.Controls.DataGrid
$grid.Style = $null
# 经主题字典解析 StcDataGrid（含 Items.Count 空态触发器）
$theme = New-Object System.Windows.Controls.Grid
[STC.SalesTrading.Plugin.Themes.ThemeManager]::ApplyDark($theme)
$grid.Style = $theme.FindResource('StcDataGrid')

$win = New-Object System.Windows.Window
$win.WindowStyle = [System.Windows.WindowStyle]::None
$win.AllowsTransparency = $True
$win.Background = [System.Windows.Media.Brushes]::Transparent
$win.Width = 400; $win.Height = 200
$win.ShowInTaskbar = $False
$win.Content = $grid
$win.Show()
$luCount = 0
$grid.Add_LayoutUpdated({ $script:luCount++ })
# flush dispatcher，确保样式触发器完成求值
$frame = New-Object System.Windows.Threading.DispatcherFrame
[System.Windows.Threading.Dispatcher]::CurrentDispatcher.BeginInvoke([System.Windows.Threading.DispatcherPriority]::Background,
    [Action]{ $frame.Continue = $false })
[System.Windows.Threading.Dispatcher]::PushFrame($frame)
$grid.UpdateLayout()

# 断言 1：空表时 Background 切换为包含“暂无数据”的 VisualBrush
$layer = [System.Windows.Documents.AdornerLayer]::GetAdornerLayer($grid)
if ($null -eq $layer) { throw 'no adorner layer' }
$emptyBackground = $grid.Background
$emptyVisual = if ($emptyBackground -is [System.Windows.Media.VisualBrush]) { $emptyBackground.Visual } else { $null }
$emptyText = if ($emptyVisual -is [System.Windows.Controls.TextBlock]) { $emptyVisual.Text } else { $null }
$emptyForeground = if ($emptyVisual -is [System.Windows.Controls.TextBlock] -and $emptyVisual.Foreground -is [System.Windows.Media.SolidColorBrush]) {
    $emptyVisual.Foreground.Color.ToString()
} else { $null }
$emptyAdorners = $layer.GetAdorners($grid)
$emptyBackgroundType = if ($null -eq $emptyBackground) { 'null' } else { $emptyBackground.GetType().Name }
Write-Host ("empty grid: background={0} text='{1}' foreground={2} adorners={3} (LU after load = {4})" -f `
    $emptyBackgroundType, $emptyText, $emptyForeground, ($(if ($emptyAdorners) { $emptyAdorners.Count } else { 0 })), $luCount)
if ($emptyBackground -isnot [System.Windows.Media.VisualBrush] -or $emptyText -ne '暂无数据') {
    throw 'empty grid background is not the expected VisualBrush'
}
if ($emptyForeground -ne '#FFFFFFFF') { throw 'empty grid foreground is not the expected white color' }
if ($emptyAdorners) { throw 'empty grid must not create adorners' }

# 断言 2：填入数据后恢复普通背景
$grid.ItemsSource = @('a', 'b', 'c')
$frame2 = New-Object System.Windows.Threading.DispatcherFrame
[System.Windows.Threading.Dispatcher]::CurrentDispatcher.BeginInvoke([System.Windows.Threading.DispatcherPriority]::Render,
    [Action]{ $frame2.Continue = $false }) | Out-Null
[System.Windows.Threading.Dispatcher]::PushFrame($frame2)
$grid.UpdateLayout()
$filledBackground = $grid.Background
$filledBackgroundType = if ($null -eq $filledBackground) { 'null' } else { $filledBackground.GetType().Name }
Write-Host ("after fill: background={0}" -f $filledBackgroundType)
if ($filledBackground -is [System.Windows.Media.VisualBrush]) { throw 'filled grid still shows empty background' }

# 断言 3：清空后重新显示 VisualBrush，且仍无 Adorner
$grid.ItemsSource = $null
$grid.UpdateLayout()
$clearedBackground = $grid.Background
$clearedAdorners = $layer.GetAdorners($grid)
$clearedBackgroundType = if ($null -eq $clearedBackground) { 'null' } else { $clearedBackground.GetType().Name }
Write-Host ("after clear: background={0} adorners={1}" -f `
    $clearedBackgroundType, ($(if ($clearedAdorners) { $clearedAdorners.Count } else { 0 })))
if ($clearedBackground -isnot [System.Windows.Media.VisualBrush]) { throw 'cleared grid did not restore empty background' }
if ($clearedAdorners) { throw 'cleared grid must not create adorners' }

$win.Close()
Write-Host 'EMPTY HINT CHECK DONE'

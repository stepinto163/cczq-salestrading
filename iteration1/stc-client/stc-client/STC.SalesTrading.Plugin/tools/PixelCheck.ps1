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

# 主题挂到一个根元素上，子控件经资源树解析样式
$root = New-Object System.Windows.Controls.Grid
[STC.SalesTrading.Plugin.Themes.ThemeManager]::ApplyDark($root)

function Test-Toggle([string]$styleKey, [string]$text, [bool]$checked) {
    $tb = New-Object System.Windows.Controls.Primitives.ToggleButton
    $tb.Content = $text
    $tb.Style = $root.FindResource($styleKey)
    $tb.IsChecked = $checked
    $root.Children.Add($tb) | Out-Null
    $tb.Measure([System.Windows.Size]::new(300.0, 80.0))
    $size = $tb.DesiredSize
    $tb.Arrange([System.Windows.Rect]::new(0.0, 0.0, $size.Width, $size.Height))
    $tb.UpdateLayout()
    $w = [int][Math]::Ceiling($size.Width); $h = [int][Math]::Ceiling($size.Height)
    $rtb = New-Object System.Windows.Media.Imaging.RenderTargetBitmap($w, $h, 96.0, 96.0, [System.Windows.Media.PixelFormats]::Pbgra32)
    $rtb.Render($tb)
    $bytes = New-Object byte[] ($w * $h * 4)
    $rtb.CopyPixels($bytes, ($w * 4), 0)
    # 采样顶 padding/边框行（y=1，避开文字）的水平 20% 处：A R G B
    $x = [int][Math]::Floor($w * 0.2); $y = 1; $i = ($y * $w + $x) * 4
    $argb = 'A={0:X2} RGB=#{1:X2}{2:X2}{3:X2}' -f $bytes[$i+3], $bytes[$i+2], $bytes[$i+1], $bytes[$i]
    Write-Host ('{0,-20} checked={1,-5} {2,3}x{3,-2}  {4}' -f $styleKey, $checked, $w, $h, $argb)
    $root.Children.Remove($tb) | Out-Null
}

Write-Host '=== 筛选 tag：未选中=实心灰 #21262D / 选中=20% 金底（RGB 应呈暖金调） ==='
foreach ($k in 'StcTagNeutral','StcTagBid','StcTagOfr') {
    Test-Toggle $k '荐' $false
    Test-Toggle $k '荐' $true
}

Write-Host '=== 行内/工具栏旗标：彩色 chip，未选中 opacity .35（A≈0x0D）/ 选中 opacity 1（A≈0x1F） ==='
foreach ($k in 'StcFlagUrgent','StcFlagRecommend','StcFlagWhole','StcFlagOco','StcFlagPacking','StcToolbarFlagUrgent','StcToolbarFlagRecommend','StcToolbarFlagWhole','StcToolbarFlagOco','StcToolbarFlagPacking') {
    Test-Toggle $k '急' $false
    Test-Toggle $k '急' $true
}
foreach ($k in 'StcButtonGreen','StcButtonRed','StcButton','StcButtonPrimary') {
    $b = New-Object System.Windows.Controls.Button
    $b.Content = '+1k'
    $b.Style = $root.FindResource($k)
    $root.Children.Add($b) | Out-Null
    $b.Measure([System.Windows.Size]::new(300.0, 80.0))
    Write-Host ('{0,-20} height={1}' -f $k, [int][Math]::Ceiling($b.DesiredSize.Height))
    $root.Children.Remove($b) | Out-Null
}
Write-Host 'PIXEL CHECK DONE'

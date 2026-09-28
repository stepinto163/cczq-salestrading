using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 报价录入模块浮窗（原型 0910）：
    /// 遮罩 .quote-entry-overlay{background:rgba(0,0,0,.68);padding:20px 居中} +
    /// 外壳 .quote-entry-shell{width:min(1970px,96vw);height:min(840px,92vh);bg #0d1117;border 1px #30363d}。
    /// 遮罩用「窗口铺满宿主 + 内部半透明黑 Border」实现（见 <see cref="OverlayBackdrop"/>）。
    /// DataContext 为独立新建的 QuoteViewModel。
    /// </summary>
    public partial class QuoteEntryOverlayWindow : Window
    {
        /// <summary>原型 quote-entry-shell 的基准尺寸。</summary>
        private const double ShellWidth = 1970d;
        private const double ShellHeight = 840d;

        /// <summary>原型 .quote-entry-overlay 的内距（padding:20px）。</summary>
        private const double OverlayPadding = 20d;

        /// <summary>原型 96vw / 92vh。</summary>
        private const double ShellWidthRatio = 0.96d;
        private const double ShellHeightRatio = 0.92d;

        private readonly QuoteViewModel _viewModel;

        public QuoteEntryOverlayWindow(QuoteViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            ContentView.DataContext = _viewModel;
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        /// <summary>铺满宿主，并按 96vw/92vh（含 20px 内距）收敛外壳尺寸。</summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyHostGeometry();
        }

        /// <summary>对齐宿主尺寸与外壳尺寸；在 ShowDialog 之前调用可避免窗口先出现在默认位置再跳动。</summary>
        private void ApplyHostGeometry()
        {
            try
            {
                OverlayBackdrop.CoverOwner(this);

                var host = OverlayBackdrop.OwnerSize(this);
                var availableWidth = Math.Max(320d, host.Width - OverlayPadding * 2d);
                var availableHeight = Math.Max(240d, host.Height - OverlayPadding * 2d);

                Shell.Width = Math.Min(ShellWidth, Math.Min(availableWidth, host.Width * ShellWidthRatio));
                Shell.Height = Math.Min(ShellHeight, Math.Min(availableHeight, host.Height * ShellHeightRatio));
            }
            catch (Exception)
            {
                // 尺寸收敛失败不影响浮窗可用性，保留 XAML 基准尺寸。
            }
        }

        /// <summary>打开浮窗（模态）。任何失败以 toast 呈现，避免"点击没反应"。</summary>
        public static void Show(Window owner, QuoteViewModel viewModel)
        {
            try
            {
                var w = new QuoteEntryOverlayWindow(viewModel);
                var host = owner ?? Application.Current?.MainWindow;
                if (host != null) w.Owner = host;
                w.ApplyHostGeometry();
                w.ShowDialog();
            }
            catch (Exception ex)
            {
                ToastService.Show("打开报价录入失败：" + ex.Message, ToastKind.Error);
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _viewModel.Dispose();
        }

        /// <summary>遮罩空白处点击关闭（原型 .quote-entry-overlay 点击遮罩关闭）。</summary>
        private void OnBackdropMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

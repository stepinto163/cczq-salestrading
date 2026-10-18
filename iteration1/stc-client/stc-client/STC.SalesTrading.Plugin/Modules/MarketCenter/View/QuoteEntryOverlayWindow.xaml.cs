using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 报价录入模块窗（原型 0910）：普通模态窗口（无遮罩浮层，居中于宿主、可拖动标题栏），
    /// 承载报价录入整模块（报价确认/精确报价/模糊报价三子页）。
    /// DataContext 为独立新建的 QuoteViewModel；窗口尺寸按宿主 96%/92% 收敛。
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
            Closed += OnClosed;
            _active = this;
        }

        /// <summary>按宿主 96%/92% 收敛窗口尺寸（基准 1970×840）；在 ShowDialog 之前调用避免跳动。</summary>
        private void ApplyHostGeometry()
        {
            try
            {
                var host = Owner != null ? new Size(Owner.ActualWidth, Owner.ActualHeight) : SystemParameters.WorkArea.Size;
                var availableWidth = Math.Max(320d, host.Width - OverlayPadding * 2d);
                var availableHeight = Math.Max(240d, host.Height - OverlayPadding * 2d);

                Width = Math.Min(ShellWidth, Math.Min(availableWidth, host.Width * ShellWidthRatio));
                Height = Math.Min(ShellHeight, Math.Min(availableHeight, host.Height * ShellHeightRatio));
            }
            catch (Exception)
            {
                // 尺寸收敛失败不影响浮窗可用性，保留 XAML 基准尺寸。
            }
        }

        /// <summary>标题栏拖动。</summary>
        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            try { DragMove(); }
            catch (Exception) { }
        }

        private static QuoteEntryOverlayWindow _active;

        /// <summary>tab 关闭时关闭已打开的浮窗（VM 随 Closed 释放）。
        /// 浮窗持有本 tab 的 QuoteViewModel/服务引用，tab 关闭后继续开着会走已释放的服务。</summary>
        public static void CloseActive()
        {
            _active?.Close();
        }

        /// <summary>
        /// 打开窗口（**非模态、全局唯一**）：已打开时仅激活置前，不重复创建；
        /// 每次打开都新建独立 QuoteViewModel（旧窗口关闭时已释放自己的 VM）。
        /// 任何失败以 toast 呈现，避免"点击没反应"。
        /// </summary>
        public static void Show(Window owner, QuoteViewModel viewModel)
        {
            try
            {
                if (_active != null)
                {
                    if (_active.WindowState == WindowState.Minimized) _active.WindowState = WindowState.Normal;
                    _active.Activate();
                    return;
                }

                var w = new QuoteEntryOverlayWindow(viewModel);
                var host = owner ?? Application.Current?.MainWindow;
                if (host != null) w.Owner = host;
                w.WindowStartupLocation = host != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
                w.ApplyHostGeometry();
                w.Show();
            }
            catch (Exception ex)
            {
                ToastService.Show("打开报价录入失败：" + ex.Message, ToastKind.Error);
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (ReferenceEquals(_active, this)) _active = null;
            _viewModel.Dispose();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

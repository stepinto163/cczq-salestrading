using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using Vm = STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 关注债券管理弹窗（原型 showFavBondModal）。普通模态窗口（无遮罩浮层，Esc/✕ 关闭）；
    /// 保存时同步后端并回写 <see cref="FilterProfileService"/> 会话缓存，取消不落库。
    /// 删除非空分组经 <see cref="ConfirmDialog"/> 二次确认（原型 _fbmConfirm）。
    /// </summary>
    public partial class FavBondManagerWindow
    {
        /// <summary>结果表空态骨架行数（原型：无结果时画 12 行 32px 空行）。</summary>
        private const int SkeletonRows = 12;

        private readonly Vm.FavBondManagerViewModel _vm;

        private FavBondManagerWindow(FilterProfileService.MarketKind market, IMarketCenterService lookupService)
        {
            InitializeComponent();
            _vm = new Vm.FavBondManagerViewModel(market, lookupService);
            DataContext = _vm;

            var rows = new List<int>(SkeletonRows);
            for (int i = 0; i < SkeletonRows; i++)
            {
                rows.Add(i);
            }
            EmptySkeleton.ItemsSource = rows;
            UpdateSkeleton();
        }

        /// <summary>弹窗入口（普通模态，居中于宿主）。lookupService 用于识别回填（可空，空则不回填）。</summary>
        public static void Show(Window owner, FilterProfileService.MarketKind market,
            IMarketCenterService lookupService = null)
        {
            var dlg = new FavBondManagerWindow(market, lookupService);
            dlg.Owner = owner;
            dlg.ShowDialog();
        }


        /// <summary>
        /// 滚轮穿透兜底（实机反馈：目录区滚动无效）。PreviewMouseWheel 隧道阶段判断：
        /// 光标下若没有可滚的内部 ScrollViewer（输入框内容不满 / 悬停在 ComboBox 上会被吃轮），
        /// 转给弹窗主滚动区（目录/结果表）滚动并标记已处理；内部滚动区可滚时不动，让默认路由接管。
        /// </summary>
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 从事件源向上找最近的 ScrollViewer：可滚则交还默认路由
            var node = e.OriginalSource as System.Windows.DependencyObject;
            while (node != null && !(node is ScrollViewer))
            {
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }
            if (node is ScrollViewer inner && inner.ScrollableHeight > 0)
            {
                return;
            }

            var main = MainScrollViewer();
            if (main == null || main.ScrollableHeight <= 0)
            {
                return;
            }

            double lines = SystemParameters.WheelScrollLines > 0 ? SystemParameters.WheelScrollLines : 3;
            main.ScrollToVerticalOffset(main.VerticalOffset - e.Delta / 120.0 * lines * 16.0);
            e.Handled = true;
        }

        /// <summary>弹窗主滚动区（目录 / 识别结果表）。</summary>
        private ScrollViewer MainScrollViewer() => ResultScroll;

        // ============ 标题栏 / 关闭（普通弹窗，无遮罩；与 QuoteEntryDialog 同款） ============

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            try { DragMove(); }
            catch (Exception) { }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>Esc 关闭（原遮罩版为点空白关闭）。</summary>
        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        // ---- 顶部按钮 ----

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            // 新建行未确认的内容一并提交
            CommitNewGroup();
            // 后台保存（逐组网关请求，同步等会冻结 UI 最坏到请求超时）：
            // 按钮禁用防重复提交；成功才关窗，失败恢复按钮留窗重试
            var button = (System.Windows.Controls.Button)sender;
            button.IsEnabled = false;
            button.Content = "保存中…";
            try
            {
                await System.Threading.Tasks.Task.Run(() => _vm.Save());
                Shared.Services.FilterProfileService.RaiseDataChanged();   // 回 UI 线程再发（订阅方改集合）
                ToastService.Show("关注债券分组已保存（已同步后端）");
                Close();
            }
            catch (Exception ex)
            {
                ToastService.Show("分组保存失败：" + ex.Message, ToastKind.Error);
                button.IsEnabled = true;
                button.Content = "保存";
            }
        }

        // ---- 分组列表 ----

        private void OnGroupListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_vm.SelectedGroup != null)
            {
                _vm.SelectedGroup.IsEditing = true;
            }
        }

        private void OnRenameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitRename(sender as TextBox);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelRename(sender as TextBox);
                e.Handled = true;
            }
        }

        private void OnRenameLostFocus(object sender, RoutedEventArgs e)
        {
            CommitRename(sender as TextBox);
        }

        private static void CommitRename(TextBox box)
        {
            if (box?.DataContext is FavBondGroup g && g.IsEditing)
            {
                var name = (g.Name ?? string.Empty).Trim();
                g.Name = name.Length > 0 ? name : g.Name;
                g.IsEditing = false;
            }
        }

        private static void CancelRename(TextBox box)
        {
            if (box?.DataContext is FavBondGroup g)
            {
                g.IsEditing = false;
            }
        }

        private void OnDeleteGroupClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is FavBondGroup g)
            {
                // 原型 _fbmDeleteGroup：非空分组需二次确认
                if (g.Bonds.Count > 0
                    && !ConfirmDialog.Show(this, "分组中有关注的债券，是否确认删除？"))
                {
                    return;
                }
                _vm.DeleteGroup(g);
            }
        }

        // ---- 新建分组行 ----

        private void OnStartNewGroupClick(object sender, RoutedEventArgs e)
        {
            _vm.StartNewGroup();
            NewGroupButton.Visibility = Visibility.Collapsed;
            NewGroupInput.Visibility = Visibility.Visible;
            NewGroupInput.Focus();
        }

        private void OnNewGroupKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitNewGroup();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _vm.NewGroupName = string.Empty;
                CommitNewGroup();
                e.Handled = true;
            }
        }

        private void OnNewGroupLostFocus(object sender, RoutedEventArgs e)
        {
            CommitNewGroup();
        }

        private void CommitNewGroup()
        {
            if (NewGroupInput.Visibility != Visibility.Visible)
            {
                return;
            }
            _vm.NewGroupName = NewGroupInput.Text;
            _vm.ConfirmNewGroup();
            NewGroupInput.Text = string.Empty;
            NewGroupInput.Visibility = Visibility.Collapsed;
            NewGroupButton.Visibility = Visibility.Visible;
        }

        // ---- 识别 / 结果表 ----

        private async void OnBondInputKeyDown(object sender, KeyEventArgs e)
        {
            // 原型：Enter（非 Shift）= 识别
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                await RunIdentifyAsync();
                e.Handled = true;
            }
        }

        private async void OnIdentifyClick(object sender, RoutedEventArgs e)
        {
            await RunIdentifyAsync();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _vm.ClearInput();
            BondInputBox.Clear();
            UpdateSkeleton();
        }

        /// <summary>识别（原型 _fbmIdentify）：按钮「识别中…」防重入，完成后刷新骨架占位。</summary>
        private async System.Threading.Tasks.Task RunIdentifyAsync()
        {
            if (!IdentifyButton.IsEnabled)
            {
                return;
            }
            var originalContent = IdentifyButton.Content;
            IdentifyButton.IsEnabled = false;
            IdentifyButton.Content = "识别中...";
            try
            {
                await _vm.IdentifyAsync();
            }
            catch (System.Exception ex)
            {
                ToastService.Show("识别服务异常：" + ex.Message, ToastKind.Error);
            }
            finally
            {
                IdentifyButton.Content = originalContent;
                IdentifyButton.IsEnabled = true;
                UpdateSkeleton();
            }
        }

        private void OnRowCheckChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is FavBondEntry entry)
            {
                var checkBox = (CheckBox)sender;
                _vm.AutoAdd(entry, checkBox.IsChecked == true);
            }
        }

        private void OnToggleAllChecked(object sender, RoutedEventArgs e)
        {
            _vm.ToggleAll(true);
        }

        private void OnToggleAllUnchecked(object sender, RoutedEventArgs e)
        {
            _vm.ToggleAll(false);
        }

        private void OnRemoveBondClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is FavBondEntry entry)
            {
                _vm.RemoveBond(entry);
            }
        }

        /// <summary>无结果时显示 12 行空骨架（原型空表占位），有结果时隐藏。</summary>
        private void UpdateSkeleton()
        {
            EmptySkeleton.Visibility = _vm.Results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

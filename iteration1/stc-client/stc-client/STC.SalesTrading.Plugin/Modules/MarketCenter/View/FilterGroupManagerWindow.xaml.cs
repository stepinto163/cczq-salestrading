using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using Vm = STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 筛选分组管理弹窗（原型 showFilterGroupModal/_fgmRender）。普通模态窗口（无遮罩浮层，Esc/✕ 关闭）；
    /// 保存时同步后端并回写 <see cref="FilterProfileService"/> 会话缓存。
    /// 右栏目录用 code-behind 动态构建（勾选态 = 当前激活子分组的内容，切换子分组后需刷新）；
    /// 自定义条件行（文本/下拉/区间）整行排列，添加按钮为原型 tb-btn-primary 红底。
    /// </summary>
    public partial class FilterGroupManagerWindow
    {
        private readonly Vm.FilterGroupManagerViewModel _vm;

        /// <summary>目录组的显示包装：记录组 + 每个候选值的勾选 Button 引用，便于刷新勾选态。</summary>
        private sealed class CatalogGroupView
        {
            public CatalogConditionGroup Group;
            public UIElement Root;
            public readonly List<KeyValuePair<string, Button>> CheckButtons = new List<KeyValuePair<string, Button>>();
        }

        /// <summary>目录分区的显示包装（与模型 CatalogSection 解耦，避免名字冲突）。</summary>
        private sealed class SectionView
        {
            public readonly List<CatalogGroupView> Groups = new List<CatalogGroupView>();
        }

        private readonly List<SectionView> _sectionViews = new List<SectionView>();

        private FilterGroupManagerWindow(FilterProfileService.MarketKind market)
        {
            InitializeComponent();
            _vm = new Vm.FilterGroupManagerViewModel(market);
            DataContext = _vm;
            TitleText.Text = _vm.MarketTitle; // 标题按市场（信用/利率筛选分组管理）
            BuildCatalog();
        }

        /// <summary>弹窗入口。merged（提醒合并页）原型无独立存储，暂按信用口径（待确认）。</summary>
        public static void Show(Window owner, FilterProfileService.MarketKind market)
        {
            var dlg = new FilterGroupManagerWindow(market);
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
        private ScrollViewer MainScrollViewer() => CatalogScroll;

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

        // ============ 右栏目录构建（原型 _fgmRender 的目录部分） ============

        private void BuildCatalog()
        {
            CatalogHost.Items.Clear();
            _sectionViews.Clear();
            foreach (var sec in _vm.Catalog)
            {
                var sectionPanel = new StackPanel { Margin = new Thickness(0, 4, 0, 6) };
                sectionPanel.Children.Add(new TextBlock
                {
                    Text = sec.Title,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x58, 0xA6, 0xFF)),
                    Margin = new Thickness(0, 6, 0, 2),
                });
                var divider = new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.FromRgb(0x21, 0x26, 0x2D)),
                    Margin = new Thickness(0, 0, 0, 6),
                };
                sectionPanel.Children.Add(divider);

                var sectionView = new SectionView();
                foreach (var cg in sec.Groups)
                {
                    var groupView = BuildConditionGroup(cg);
                    sectionPanel.Children.Add(groupView.Root);
                    sectionView.Groups.Add(groupView);
                }
                CatalogHost.Items.Add(sectionPanel);
                _sectionViews.Add(sectionView);
            }
            RefreshChecks();
        }

        private CatalogGroupView BuildConditionGroup(CatalogConditionGroup cg)
        {
            // .fgm-condition-group{margin-bottom:8px} + .disabled{opacity:.5}
            var outer = new StackPanel { Margin = new Thickness(0, 0, 0, 8), Opacity = cg.Enabled ? 1d : 0.5d };
            if (!cg.Enabled)
            {
                ToolTipService.SetToolTip(outer, cg.DisabledReason);
            }
            outer.Children.Add(new TextBlock
            {
                Text = cg.Label,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x7D, 0x85, 0x90)),
                Margin = new Thickness(0, 0, 0, 4),
            });

            var itemsPanel = new WrapPanel { };
            var view = new CatalogGroupView { Group = cg };
            bool chipsAdded = false;

            // 原型：仅 tag 型组渲染候选 chips（`(fg.type==='select'?[]:fg.items)`）
            if (cg.Kind == CatalogConditionGroup.ConditionKind.Tags)
            {
                foreach (var item in cg.Items)
                {
                    var btn = new Button
                    {
                        Content = item.Label,
                        Style = (Style)FindResource("CatalogCheckButton"),
                        Margin = new Thickness(0, 0, 4, 4),
                        Tag = item,
                    };
                    var capturedGroup = cg;
                    var capturedItem = item;
                    btn.Click += (s, e) => OnCatalogCheckClick(capturedGroup, capturedItem);
                    if (!item.Enabled || !cg.Enabled)
                    {
                        btn.IsEnabled = false;
                        btn.Opacity = 0.55;
                    }
                    itemsPanel.Children.Add(btn);
                    view.CheckButtons.Add(new KeyValuePair<string, Button>(item.Value, btn));
                }
                outer.Children.Add(itemsPanel);
                chipsAdded = true;
            }

            // 自定义条件行：原型 .fbm-custom-condition{width:100%} 整行排列（非 chip 流内）
            if (cg.Kind == CatalogConditionGroup.ConditionKind.Text)
            {
                var input = BuildSmallInput(rightAlign: false);
                input.Tag = "输入" + cg.Label; // StcTextBox 模板的 Tag 占位

                var addBtn = BuildAddButton();
                var capturedGroup = cg;
                var capturedInput = input;
                addBtn.Click += (s, e) =>
                {
                    if (_vm.AddCustomText(capturedGroup, capturedInput.Text))
                    {
                        capturedInput.Text = string.Empty;
                        RefreshChecks();
                    }
                };
                var row = new Grid { Margin = new Thickness(0, 4, 0, 0) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(input, 0);
                Grid.SetColumn(addBtn, 1);
                row.Children.Add(input);
                row.Children.Add(addBtn);
                if (!cg.Enabled)
                {
                    input.IsEnabled = false;
                    addBtn.IsEnabled = false;
                }
                outer.Children.Add(row);
            }

            if (cg.Kind == CatalogConditionGroup.ConditionKind.Select)
            {
                var comboStyle = new Style(typeof(ComboBox), (Style)FindResource("StcComboBox"));
                comboStyle.Setters.Add(new Setter(FontSizeProperty, 11d));
                comboStyle.Setters.Add(new Setter(HeightProperty, 22d));
                comboStyle.Setters.Add(new Setter(MinWidthProperty, 96d));
                var combo = new ComboBox { Style = comboStyle };
                // 原型首项为「请选择」占位（<option value="">请选择</option>）
                combo.Items.Add(new ComboBoxItem { Content = "请选择" });
                foreach (var item in cg.Items)
                {
                    combo.Items.Add(new ComboBoxItem { Content = item.Label, IsEnabled = item.Enabled });
                }
                combo.SelectedIndex = 0;
                var addBtn = BuildAddButton();
                var capturedGroup = cg;
                var capturedCombo = combo;
                addBtn.Click += (s, e) =>
                {
                    var selected = capturedCombo.SelectedItem as ComboBoxItem;
                    if (selected == null || ReferenceEquals(selected, capturedCombo.Items[0]))
                    {
                        return;
                    }
                    if (_vm.AddCustomText(capturedGroup, selected.Content?.ToString()))
                    {
                        capturedCombo.SelectedIndex = 0;
                        RefreshChecks();
                    }
                };
                var row = new Grid { Margin = new Thickness(0, 4, 0, 0) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(combo, 0);
                Grid.SetColumn(addBtn, 1);
                row.Children.Add(combo);
                row.Children.Add(addBtn);
                if (!cg.Enabled)
                {
                    combo.IsEnabled = false;
                    addBtn.IsEnabled = false;
                }
                outer.Children.Add(row);
            }

            if (cg.HasRange)
            {
                var minBox = BuildSmallInput();
                minBox.Tag = "最小";
                var maxBox = BuildSmallInput();
                maxBox.Tag = "最大";
                var sep = new TextBlock { Text = "-", Foreground = new SolidColorBrush(Color.FromRgb(0x48, 0x4F, 0x58)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 2, 0) };
                ComboBox unitBox = null;
                if (cg.HasUnitSelect)
                {
                    var unitStyle = new Style(typeof(ComboBox), (Style)FindResource("StcComboBox"));
                    unitStyle.Setters.Add(new Setter(FontSizeProperty, 11d));
                    unitStyle.Setters.Add(new Setter(HeightProperty, 22d));
                    unitStyle.Setters.Add(new Setter(WidthProperty, 52d));
                    unitBox = new ComboBox { Style = unitStyle, Margin = new Thickness(4, 0, 0, 0) };
                    foreach (var u in new[] { "Y", "M", "D" })
                    {
                        unitBox.Items.Add(new ComboBoxItem { Content = u });
                    }
                    unitBox.SelectedIndex = 0;
                }
                var addBtn = BuildAddButton();
                var capturedGroup = cg;
                var capturedMin = minBox;
                var capturedMax = maxBox;
                var capturedUnit = unitBox;
                addBtn.Click += (s, e) =>
                {
                    var unit = capturedUnit != null && capturedUnit.SelectedItem is ComboBoxItem ui ? ui.Content?.ToString() : null;
                    if (_vm.AddCustomRange(capturedGroup, capturedMin.Text, capturedMax.Text, unit))
                    {
                        capturedMin.Text = string.Empty;
                        capturedMax.Text = string.Empty;
                        RefreshChecks();
                    }
                };
                var row = new Grid { Margin = new Thickness(0, 4, 0, 0) };
                // 原型：min/max 都是 flex:1 的 .tb-input，单位下拉与按钮固定宽
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(minBox, 0);
                Grid.SetColumn(sep, 1);
                Grid.SetColumn(maxBox, 2);
                row.Children.Add(minBox);
                row.Children.Add(sep);
                row.Children.Add(maxBox);
                if (unitBox != null)
                {
                    Grid.SetColumn(unitBox, 3);
                    row.Children.Add(unitBox);
                    Grid.SetColumn(addBtn, 4);
                }
                else
                {
                    Grid.SetColumn(addBtn, 3);
                }
                row.Children.Add(addBtn);
                if (!cg.Enabled)
                {
                    minBox.IsEnabled = false;
                    maxBox.IsEnabled = false;
                    if (unitBox != null) unitBox.IsEnabled = false;
                    addBtn.IsEnabled = false;
                }
                outer.Children.Add(row);
            }

            if (!chipsAdded)
            {
                outer.Children.Add(itemsPanel);
            }
            view.Root = outer;
            return view;
        }

        /// <summary>目录输入行文本框：主题 StcTextBox（深色 + Tag 占位），覆盖为原型 11px/22px 小尺寸。</summary>
        private TextBox BuildSmallInput(bool rightAlign = true)
        {
            var style = new Style(typeof(TextBox), (Style)FindResource("StcTextBox"));
            style.Setters.Add(new Setter(FontSizeProperty, 11d));
            style.Setters.Add(new Setter(HeightProperty, 22d));
            style.Setters.Add(new Setter(PaddingProperty, new Thickness(4, 0, 4, 0)));
            if (rightAlign)
            {
                style.Setters.Add(new Setter(TextBox.TextAlignmentProperty, TextAlignment.Right));
            }
            return new TextBox { Style = style };
        }

        /// <summary>目录「添加」按钮：原型 tb-btn-primary 红底白字（hover #dc2626）。</summary>
        private static Button BuildAddButton()
        {
            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border), "Bd");
            factory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            factory.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Button.PaddingProperty));
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(presenter);
            template.VisualTree = factory;
            template.Triggers.Add(new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true,
                Setters = { new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)), "Bd") },
            });

            return new Button
            {
                Content = "添加",
                FontSize = 11,
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(4, 0, 0, 0),
                Cursor = Cursors.Hand,
                Background = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Template = template,
            };
        }

        /// <summary>按当前激活子分组刷新目录勾选态与识别结果 chips（原型：active class）。</summary>
        private void RefreshChecks()
        {
            foreach (var sectionView in _sectionViews)
            {
                foreach (var cg in sectionView.Groups)
                {
                    foreach (var pair in cg.CheckButtons)
                    {
                        var active = _vm.IsConditionActive(cg.Group, pair.Key);   // Key = item.Value（条件落库值）
                        // 原型 .fbm-check.checked{background:rgba(59,130,246,.08);color:#79c0ff;border-color:rgba(59,130,246,.4)}
                        pair.Value.Background = active
                            ? new SolidColorBrush(Color.FromArgb(0x14, 0x3B, 0x82, 0xF6))
                            : new SolidColorBrush(Color.FromRgb(0x16, 0x1B, 0x22));
                        pair.Value.BorderBrush = active
                            ? new SolidColorBrush(Color.FromArgb(0x66, 0x3B, 0x82, 0xF6))
                            : new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D));
                        pair.Value.Foreground = active
                            ? new SolidColorBrush(Color.FromRgb(0x79, 0xC0, 0xFF))
                            : new SolidColorBrush(Color.FromRgb(0xC9, 0xD1, 0xD9));
                    }
                }
            }
            RefreshMatchedChecks();
        }

        /// <summary>识别结果 chips 的勾选态（原型 matched chip 也按 active 子分组着色）。</summary>
        private void RefreshMatchedChecks()
        {
            if (MatchedList == null)
            {
                return;
            }
            for (int i = 0; i < MatchedList.Items.Count; i++)
            {
                if (MatchedList.ItemContainerGenerator.ContainerFromIndex(i) is ContentPresenter presenter)
                {
                    var btn = FindVisualChild<Button>(presenter);
                    if (btn?.Content is string token)
                    {
                        var group = FindCatalogGroupForValue(token);
                        var active = group != null && _vm.IsConditionActive(group, token);
                        btn.BorderBrush = active
                            ? new SolidColorBrush(Color.FromArgb(0x66, 0x3B, 0x82, 0xF6))
                            : new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D));
                        btn.Background = active
                            ? new SolidColorBrush(Color.FromArgb(0x14, 0x3B, 0x82, 0xF6))
                            : new SolidColorBrush(Color.FromRgb(0x16, 0x1B, 0x22));
                        btn.Foreground = active
                            ? new SolidColorBrush(Color.FromRgb(0x79, 0xC0, 0xFF))
                            : new SolidColorBrush(Color.FromRgb(0xC9, 0xD1, 0xD9));
                    }
                }
            }
        }

        /// <summary>token 在目录中所属的启用组（识别结果点击/勾选态共用）。</summary>
        private CatalogConditionGroup FindCatalogGroupForValue(string token)
        {
            foreach (var sec in _vm.Catalog)
            {
                foreach (var cg in sec.Groups)
                {
                    if (cg.Enabled && cg.Kind == CatalogConditionGroup.ConditionKind.Tags &&
                        cg.Items.Any(it => it.Enabled && string.Equals(it.Label, token, StringComparison.OrdinalIgnoreCase)))
                    {
                        return cg;
                    }
                }
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject start) where T : DependencyObject
        {
            if (start == null)
            {
                return null;
            }
            int count = VisualTreeHelper.GetChildrenCount(start);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);
                if (child is T match)
                {
                    return match;
                }
                var deep = FindVisualChild<T>(child);
                if (deep != null)
                {
                    return deep;
                }
            }
            return null;
        }

        private void OnCatalogCheckClick(CatalogConditionGroup group, Shared.ViewModel.CatalogItem item)
        {
            _vm.ToggleCatalogCondition(group, item);
            RefreshChecks();
        }

        private void OnMatchedTokenClick(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Content is string token)
            {
                var group = FindCatalogGroupForValue(token);
                if (group != null)
                {
                    _vm.ToggleCatalogCondition(group, token);
                    RefreshChecks();
                }
            }
        }

        // ============ 顶部按钮 / 分组列表 ============

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            CommitNewGroup();
            // 后台保存（条件编译含字段目录网络读取 + 逐组网关请求）：
            // 按钮禁用防重复提交；成功才关窗，失败恢复按钮留窗重试
            var button = (System.Windows.Controls.Button)sender;
            button.IsEnabled = false;
            button.Content = "保存中…";
            try
            {
                await System.Threading.Tasks.Task.Run(() => _vm.Save());
                Shared.Services.FilterProfileService.RaiseDataChanged();   // 回 UI 线程再发（订阅方改集合）
                ToastService.Show("筛选分组已保存（已同步后端）");
                Close();
            }
            catch (Exception ex)
            {
                ToastService.Show("分组保存失败：" + ex.Message, ToastKind.Error);
                button.IsEnabled = true;
                button.Content = "保存";
            }
        }

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
                if (sender is TextBox box && box.DataContext is FilterNamedGroup g)
                {
                    g.IsEditing = false;
                }
                e.Handled = true;
            }
        }

        private void OnRenameLostFocus(object sender, RoutedEventArgs e)
        {
            CommitRename(sender as TextBox);
        }

        private static void CommitRename(TextBox box)
        {
            if (box?.DataContext is FilterNamedGroup g && g.IsEditing)
            {
                var name = (g.Name ?? string.Empty).Trim();
                g.Name = name.Length > 0 ? name : g.Name;
                g.IsEditing = false;
            }
        }

        private void OnDeleteGroupClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is FilterNamedGroup g)
            {
                // 原型 _fgmDeleteGroup：非空分组需二次确认
                if (g.ConditionCount > 0
                    && !ConfirmDialog.Show(this, "分组中有已选条件，是否确认删除？"))
                {
                    return;
                }
                _vm.DeleteGroup(g);
                RefreshChecks();
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

        // ---- 子分组卡片 ----

        private void OnSubgroupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 切换分组时 ItemsSource 重建会短暂清空选择（-1），忽略以免把 VM 拉回无选中
            if (SubgroupList.SelectedIndex >= 0)
            {
                _vm.SelectedSubgroupIndex = SubgroupList.SelectedIndex;
            }
            RefreshChecks();
        }

        /// <summary>子分组点击显式选中：默认「点容器切换」与 SelectedIndex 脱节（点已选变取消、
        /// 点空白选到别的卡——实测探针 POST 证据），子分组语义要求恒有一张激活卡，这里直接定选中。
        /// ✕ 删除按钮豁免（否则吞掉它的 Click）。</summary>
        private void OnSubgroupListPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as System.Windows.DependencyObject;
            for (var n = source; n != null; n = System.Windows.Media.VisualTreeHelper.GetParent(n))
            {
                if (n is System.Windows.Controls.Button) return;
            }
            var container = source;
            while (container != null && !(container is System.Windows.Controls.ListBoxItem))
            {
                container = System.Windows.Media.VisualTreeHelper.GetParent(container);
            }
            if (container is System.Windows.Controls.ListBoxItem clicked)
            {
                var index = SubgroupList.ItemContainerGenerator.IndexFromContainer(clicked);
                if (index >= 0)
                {
                    SubgroupList.SelectedIndex = index;
                    _vm.SelectedSubgroupIndex = index;
                    RefreshChecks();
                }
                e.Handled = true;   // 拦掉默认容器切换，选中状态完全由这里管
            }
        }

        private void OnAddSubgroupClick(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedGroup == null)
            {
                ToastService.Show("请先选择分组");
                return;
            }
            _vm.AddSubgroup();
            SubgroupList.SelectedIndex = _vm.SelectedSubgroupIndex;
            RefreshChecks();
        }

        private void OnDeleteSubgroupClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is FilterSubgroup sg)
            {
                // 原型 _fgmDeleteSubgroup：非空删除二次确认（深色确认框，替代系统 MessageBox）
                if (sg.Conditions.Count > 0
                    && !ConfirmDialog.Show(this, "子分组中有已选条件，是否确认删除？"))
                {
                    return;
                }
                _vm.DeleteSubgroup(sg);
                RefreshChecks();
            }
        }

        private void OnRemoveConditionClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var condition = btn.DataContext as FilterCondition;
            var subgroup = FindAncestorSubgroup(btn);
            if (condition != null && subgroup != null)
            {
                _vm.RemoveCondition(subgroup, condition);
                RefreshChecks();
            }
        }

        /// <summary>chip 的 ✕ 事件源在卡片 DataTemplate 里，向上找 ListBoxItem 的 DataContext（= FilterSubgroup）。</summary>
        private static FilterSubgroup FindAncestorSubgroup(System.Windows.DependencyObject start)
        {
            var d = start;
            while (d != null && !(d is System.Windows.Controls.ListBoxItem))
            {
                d = System.Windows.Media.VisualTreeHelper.GetParent(d);
            }
            return (d as System.Windows.Controls.ListBoxItem)?.DataContext as FilterSubgroup;
        }

        // ---- 识别 ----

        private void OnConditionInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                RunMatch();
                e.Handled = true;
            }
        }

        private void OnMatchClick(object sender, RoutedEventArgs e)
        {
            RunMatch();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _vm.ClearConditionInput();
            ConditionInputBox.Clear();
            MatchedPanel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 识别（原型 _fgmMatch）：命中条件**自动加入激活子分组**（identity 去重），
        /// 识别结果区展示命中值；toast「已识别 N 个条件」/「未匹配到条件」。
        /// </summary>
        private void RunMatch()
        {
            int added = _vm.Match();
            MatchedPanel.Visibility = _vm.MatchedTokens.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            RefreshChecks();
            if (added > 0)
            {
                ToastService.Show("已识别 " + added + " 个条件", ToastKind.Success);
            }
            else if (_vm.MatchedTokens.Count == 0)
            {
                ToastService.Show("未匹配到条件");
            }
        }
    }
}

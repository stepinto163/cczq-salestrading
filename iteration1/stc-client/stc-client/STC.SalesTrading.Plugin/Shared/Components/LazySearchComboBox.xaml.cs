using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 公共懒加载搜索下拉（参考 stc-client-lite LazySearchComboBox 定位）：
    /// 输入防抖 → <see cref="SearchCommand"/>(<see cref="LazySearchArgs"/>) 取数（业务在命令实现方，控件零业务概念）→
    /// 候选 DataGrid（列按 <see cref="ColumnMembers"/> 配置）+ 页码步进分页（args.HasMore，适配无总数后端）+
    /// 键盘导航（↑↓/Enter/Esc）+ 清空按钮 + 关闭时名称/Id 一致性还原。
    /// 选中/清空一律显式 UpdateSource 推源（DataGrid 模板列编辑管道吞自动推源，实测踩坑）。
    /// 水印由 StcTextBox 模板按 Tag 渲染（Placeholder 绑 Tag）。
    /// </summary>
    public partial class LazySearchComboBox : UserControl
    {
        public LazySearchComboBox()
        {
            InitializeComponent();
            // TextBox 的类处理器可能先处理方向键；候选导航仍需接收该按键。
            InputBox.AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(OnInputKeyDown), true);
            Candidates = new ObservableCollection<object>();
            CandidateGrid.ItemsSource = Candidates;
            InputBox.GotKeyboardFocus += (s, e) => UpdateChrome();
            InputBox.LostKeyboardFocus += (s, e) => UpdateChrome();
            _debounce.Tick += OnDebounceTick;
            Loaded += (s, e) => UpdateChrome();
        }

        // ============ 依赖属性 ============

        public static readonly DependencyProperty SearchCommandProperty = DependencyProperty.Register(
            nameof(SearchCommand), typeof(LazySearchCommand), typeof(LazySearchComboBox), new PropertyMetadata(null));

        /// <summary>取数命令：控件传入 <see cref="LazySearchArgs"/>（Text/PageIndex/PageSize/Parameter），实现方回填 Items/HasMore。</summary>
        public LazySearchCommand SearchCommand
        {
            get { return (LazySearchCommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }

        public static readonly DependencyProperty SearchParameterProperty = DependencyProperty.Register(
            nameof(SearchParameter), typeof(object), typeof(LazySearchComboBox),
            new PropertyMetadata(null, (sender, args) => ((LazySearchComboBox)sender).InvalidateCandidates()));

        /// <summary>业务附加参数（原样进 args.Parameter，语义由命令实现方约定）。</summary>
        public object SearchParameter
        {
            get { return GetValue(SearchParameterProperty); }
            set { SetValue(SearchParameterProperty, value); }
        }

        public static readonly DependencyProperty ColumnMembersProperty = DependencyProperty.Register(
            nameof(ColumnMembers), typeof(string), typeof(LazySearchComboBox),
            new PropertyMetadata(null, (s, e) => ((LazySearchComboBox)s).RebuildColumns()));

        /// <summary>候选列（逗号分隔的行对象属性名，如 "Name,SubText"）。</summary>
        public string ColumnMembers
        {
            get { return (string)GetValue(ColumnMembersProperty); }
            set { SetValue(ColumnMembersProperty, value); }
        }

        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register(
            nameof(DisplayMember), typeof(string), typeof(LazySearchComboBox), new PropertyMetadata(null));

        /// <summary>选中后回填输入框的属性名。</summary>
        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }
            set { SetValue(DisplayMemberProperty, value); }
        }

        public static readonly DependencyProperty ValueMemberProperty = DependencyProperty.Register(
            nameof(ValueMember), typeof(string), typeof(LazySearchComboBox), new PropertyMetadata(null));

        /// <summary>SelectedValue 对应的属性名。</summary>
        public string ValueMember
        {
            get { return (string)GetValue(ValueMemberProperty); }
            set { SetValue(ValueMemberProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(LazySearchComboBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) => ((LazySearchComboBox)s).OnTextChanged(e)));

        /// <summary>显示名称（TwoWay；写回经显式推源）。</summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(string), typeof(LazySearchComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>选中 Id（TwoWay；写回经显式推源）。</summary>
        public string SelectedValue
        {
            get { return (string)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(LazySearchComboBox), new PropertyMetadata(null));

        /// <summary>水印/占位文案（经 Tag 交由 StcTextBox 模板渲染）。</summary>
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen), typeof(bool), typeof(LazySearchComboBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) => ((LazySearchComboBox)s).OnDropDownOpenChanged()));

        /// <summary>候选浮层开关。</summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
            nameof(PageSize), typeof(int), typeof(LazySearchComboBox), new PropertyMetadata(50));

        /// <summary>每页条数。</summary>
        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        public static readonly DependencyProperty SelectionCommandProperty = DependencyProperty.Register(
            nameof(SelectionCommand), typeof(ICommand), typeof(LazySearchComboBox));

        public ICommand SelectionCommand
        {
            get { return (ICommand)GetValue(SelectionCommandProperty); }
            set { SetValue(SelectionCommandProperty, value); }
        }

        // ============ 状态 ============

        /// <summary>候选行（任意类型，列按 ColumnMembers 反射取值）。</summary>
        public ObservableCollection<object> Candidates { get; }

        private int _pageIndex = 1;
        private bool _hasMore;
        private int _searchSeq;
        private readonly DispatcherTimer _debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        private bool _restoring;

        // ============ 输入与交互 ============

        /// <summary>外部回填 Text（VM/代码回填行数据）：同步输入框与清空按钮；
        /// 不触发搜索、不清 Id（调用方成对回填 Text+SelectedValue）。</summary>
        private void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_restoring) return;
            var value = e.NewValue as string ?? string.Empty;
            if (InputBox.Text == value) return;
            _restoring = true;
            try
            {
                InputBox.Text = value;
                UpdateChrome();
            }
            finally
            {
                _restoring = false;
            }
        }

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_restoring) return;
            // 名称变更：清 Id（显式推源；语义——名称改了 Id 不能还是旧的）
            InvalidateCandidates();
            PushSelection(null);
            Text = InputBox.Text;
            PushText();
            UpdateChrome();
            _debounce.Stop();
            _debounce.Start();
        }

        private void InvalidateCandidates()
        {
            _searchSeq++;
            Candidates?.Clear();
            _debounce.Stop();
            if (IsDropDownOpen) _debounce.Start();
        }

        private void OnDebounceTick(object sender, EventArgs e)
        {
            _debounce.Stop();
            if (!IsDropDownOpen) IsDropDownOpen = true;
            else _ = SearchAsync(1);
        }

        private void OnDropDownOpenChanged()
        {
            if (!IsDropDownOpen)
            {
                RestoreConsistencyOnClose();
                return;
            }
            if (!InputBox.IsFocused) InputBox.Focus();
            _pageIndex = 1;
            _ = SearchAsync(1);
        }

        private void OnPrevPageClick(object sender, RoutedEventArgs e)
        {
            if (_pageIndex > 1)
            {
                _pageIndex--;
                _ = SearchAsync(_pageIndex);
            }
        }

        private void OnNextPageClick(object sender, RoutedEventArgs e)
        {
            if (_hasMore)
            {
                _pageIndex++;
                _ = SearchAsync(_pageIndex);
            }
        }

        /// <summary>键盘导航挂 KeyUp（参考实现同款）：Down/Up 移动选中行，Enter 确认在 KeyDown。</summary>
        private void OnInputKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && CandidateGrid.Items.Count > 0)
            {
                CandidateGrid.SelectedIndex = System.Math.Min(CandidateGrid.SelectedIndex + 1, CandidateGrid.Items.Count - 1);
                CandidateGrid.ScrollIntoView(CandidateGrid.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Up && CandidateGrid.SelectedIndex > 0)
            {
                CandidateGrid.SelectedIndex -= 1;
                CandidateGrid.ScrollIntoView(CandidateGrid.SelectedItem);
                e.Handled = true;
            }
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _restoring = true;
            try
            {
                InputBox.Text = string.Empty;
                Text = string.Empty;
                PushText();
                PushSelection(null);
                _searchSeq++;
                Candidates.Clear();
                UpdateChrome();
            }
            finally
            {
                _restoring = false;
            }
            InputBox.Focus();
        }

        private void OnCandidateClicked(object sender, MouseButtonEventArgs e)
        {
            var row = FindParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row != null)
            {
                ApplySelection(row.DataContext);
                e.Handled = true;
            }
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && !IsDropDownOpen)
            {
                // 已关闭候选时，方向键明确重新打开；不依赖上一次输入防抖是否仍在计时。
                _debounce.Stop();
                IsDropDownOpen = true;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && IsDropDownOpen && CandidateGrid.SelectedItem != null)
            {
                ApplySelection(CandidateGrid.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        // ============ 选中 / 清空 / 还原 ============

        /// <summary>选中候选：名称与 Id 统一回填，显式推源（编辑管道吞自动推源）。</summary>
        private void ApplySelection(object item)
        {
            _restoring = true;
            try
            {
                InputBox.Text = GetMemberValue(item, DisplayMember) as string ?? string.Empty;
                PushSelection(GetMemberValue(item, ValueMember) as string);
                Text = InputBox.Text;
                PushText();
                if (SelectionCommand?.CanExecute(item) == true) SelectionCommand.Execute(item);
                UpdateChrome();
            }
            finally
            {
                _restoring = false;
            }
            IsDropDownOpen = false;
        }

        /// <summary>Popup 关闭后的一致性还原：
        /// 文本空 → 清 Id；有选中且文本与显示名不一致（手改）→ 还原显示名，保证名称/Id 不错位。</summary>
        private void RestoreConsistencyOnClose()
        {
            _debounce.Stop();
            if (_restoring) return;
            _restoring = true;
            try
            {
                if (string.IsNullOrEmpty(InputBox.Text))
                {
                    if (SelectedValue != null) PushSelection(null);
                }
                else if (SelectedValue != null && SelectedTextName != null && InputBox.Text != SelectedTextName)
                {
                    InputBox.Text = SelectedTextName; // 手改名称但 Id 仍是旧值 → 还原显示
                }
            }
            finally
            {
                _restoring = false;
            }
        }

        private string SelectedTextName
        {
            get
            {
                var match = Candidates.FirstOrDefault(o => Equals(GetMemberValue(o, ValueMember), SelectedValue));
                return match == null ? null : GetMemberValue(match, DisplayMember) as string;
            }
        }

        // ============ 搜索 ============

        private async System.Threading.Tasks.Task SearchAsync(int pageIndex)
        {
            var command = SearchCommand;
            if (command == null) return;

            var seq = ++_searchSeq;
            var args = new LazySearchArgs
            {
                Text = InputBox.Text,
                PageIndex = pageIndex,
                PageSize = PageSize,
                Parameter = SearchParameter,
            };
            try
            {
                await command.ExecuteAsync(args);
            }
            catch (Exception)
            {
                return; // 命令实现方应自行容错提示；控件保底不让异常抛进 UI 线程
            }
            if (seq != _searchSeq) return; // 已发起更新的搜索，丢弃旧结果

            _pageIndex = pageIndex;
            _hasMore = args.HasMore;
            Candidates.Clear();
            foreach (var item in args.Items ?? new object[0]) Candidates.Add(item);
            // 后端 CRM 查询无 totalCount（仅 hasMore）：末页以「末页」标记
            PageText.Text = "第 " + pageIndex + " 页 · 本页 " + Candidates.Count + " 条"
                + (_hasMore ? string.Empty : " · 末页");
            NextPageButton.IsEnabled = _hasMore;
            PrevPageButton.IsEnabled = pageIndex > 1;
            if (Candidates.Count > 0) CandidateGrid.SelectedIndex = 0;
        }

        // ============ 列构建 / 显式推源 / 视觉 ============

        /// <summary>按 ColumnMembers 动态生成候选列：首列主文本（亮色，*宽+截断），其余辅助文本（暗色，Auto 宽）。
        /// 文本前景取主题画刷的解析值（弹层内 DynamicResource 继承不可靠，实测踩坑）。</summary>
        private void RebuildColumns()
        {
            CandidateGrid.Columns.Clear();
            if (string.IsNullOrEmpty(ColumnMembers)) return;
            var primary = GetBrush("TextPrimaryBrush", System.Windows.Media.Brushes.White);
            var secondary = GetBrush("TextMutedBrush", System.Windows.Media.Brushes.Gray);
            var members = ColumnMembers.Split(',').Select(m => m.Trim()).Where(m => m.Length > 0).ToList();
            for (int i = 0; i < members.Count; i++)
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.ForegroundProperty, i == 0 ? primary : secondary));
                style.Setters.Add(new Setter(TextBlock.FontSizeProperty, i == 0 ? 12.0 : 11.0));
                style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
                DataGridLength width;
                if (i == 0)
                {
                    // 主文本列：按内容完整显示（机构简称不可截断）
                    width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }
                else
                {
                    // 辅助列：占满剩余空间，超长省略号 + Tooltip 全名
                    style.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
                    style.Setters.Add(new Setter(FrameworkElement.ToolTipProperty,
                        new Binding(members[i]) { Mode = BindingMode.OneWay }));
                    width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
                CandidateGrid.Columns.Add(new DataGridTextColumn
                {
                    Binding = new Binding(members[i]) { Mode = BindingMode.OneWay },
                    Width = width,
                    IsReadOnly = true,
                    ElementStyle = style,
                });
            }
        }

        private static System.Windows.Media.Brush GetBrush(string key, System.Windows.Media.Brush fallback)
        {
            return Application.Current != null
                ? Application.Current.TryFindResource(key) as System.Windows.Media.Brush ?? fallback
                : fallback;
        }

        private static object GetMemberValue(object item, string member)
        {
            if (item == null || string.IsNullOrEmpty(member)) return null;
            return item.GetType().GetProperty(member)?.GetValue(item);
        }

        private void PushSelection(string id)
        {
            SelectedValue = id;
            GetBindingExpression(SelectedValueProperty)?.UpdateSource();
        }

        private void PushText()
        {
            GetBindingExpression(TextProperty)?.UpdateSource();
        }

        /// <summary>水印与清空按钮状态（一体式外框内，按钮不额外占宽）。</summary>
        private void UpdateChrome()
        {
            var hasText = !string.IsNullOrEmpty(InputBox.Text);
            ClearButton.Visibility = hasText ? Visibility.Visible : Visibility.Collapsed;
            WaterText.Visibility = hasText || InputBox.IsKeyboardFocused
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null)
            {
                if (node is T match) return match;
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }
            return null;
        }
    }
}

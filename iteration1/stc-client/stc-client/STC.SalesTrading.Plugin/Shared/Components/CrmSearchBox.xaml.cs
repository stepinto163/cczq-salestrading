using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// CRM 下拉联想选择框（方案 §4.0.1 复用组件）：
    /// 输入防抖 300ms → <see cref="Directory"/>.SearchAsync(kind, text, institutionId)；
    /// 选中一条同时回填 Id + 标准名称；清空/手改输入即清 Id。
    /// 替换位置：报价确认页/精确报价页表格机构与交易员两列、两页工具栏批量应用框、
    /// 新报价/编辑报价弹窗（组合根注入 <see cref="Directory"/>，绑定方式见 XAML 注释）。
    /// </summary>
    public partial class CrmSearchBox : UserControl
    {
        /// <summary>CRM 目录服务（组合根装配时注入；未注入时联想为空、仅可手输名称）。</summary>
        public static IBaseDirectory Directory { get; set; }

        /// <summary>联想防抖间隔（方案 §4.0.1：300ms）。</summary>
        private static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(300);

        public CrmSearchBox()
        {
            InitializeComponent();
            Loaded += (s2, e2) => UpdateClearVisibility();
            _debounce = new DispatcherTimer { Interval = Debounce };
            _debounce.Tick += (s, e) =>
            {
                _debounce.Stop();
                _ = SearchAsync();
            };
        }

        #region 依赖属性

        public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
            nameof(Kind), typeof(string), typeof(CrmSearchBox), new PropertyMetadata("INSTITUTION"));

        public static readonly DependencyProperty InstitutionIdProperty = DependencyProperty.Register(
            nameof(InstitutionId), typeof(string), typeof(CrmSearchBox),
            new PropertyMetadata(null, (s, e) => ((CrmSearchBox)s).OnFilterChanged()));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(CrmSearchBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) => ((CrmSearchBox)s).OnTextChanged()));

        public static readonly DependencyProperty SelectedIdProperty = DependencyProperty.Register(
            nameof(SelectedId), typeof(string), typeof(CrmSearchBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(CrmSearchBox), new PropertyMetadata(null));

        /// <summary>联想类别：INSTITUTION / CONTACT。</summary>
        public string Kind
        {
            get { return (string)GetValue(KindProperty); }
            set { SetValue(KindProperty, value); }
        }

        /// <summary>仅 Kind=CONTACT：限定联系人所属机构（更换机构后由调用方联动）。</summary>
        public string InstitutionId
        {
            get { return (string)GetValue(InstitutionIdProperty); }
            set { SetValue(InstitutionIdProperty, value); }
        }

        /// <summary>输入/显示文本（双向：选中回填标准名称写回业务侧）。</summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>选中的 CRM Id（双向）；null = 未选定（提交前置校验拦截）。</summary>
        public string SelectedId
        {
            get { return (string)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        /// <summary>占位文案。</summary>
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        #endregion

        /// <summary>当前候选（XAML 绑定）。</summary>
        public ObservableCollection<CrmDirectoryEntry> Candidates { get; } = new ObservableCollection<CrmDirectoryEntry>();

        private readonly DispatcherTimer _debounce;
        private CancellationTokenSource _searchCts;
        private bool _syncingText;

        /// <summary>用户编辑输入：程序回填（选中）不触发；否则先清 Id 再联想（规范语义：名称改了 Id 不能还是旧的）。</summary>
        private void OnTextChanged()
        {
            if (_syncingText) return;
            if (string.IsNullOrEmpty(Text)) PushSource(null);
            UpdateClearVisibility();
            _debounce.Stop();
            _debounce.Start();
        }

        /// <summary>显式推源：DataGrid 模板列内 DP 的自动推源会被编辑管道吞掉（实测踩坑），
        /// 选中/清空时手动 UpdateSource 保证 TwoWay 写回可靠。</summary>
        private void PushSource(string id)
        {
            SelectedId = id;
            GetBindingExpression(SelectedIdProperty)?.UpdateSource();
        }

        /// <summary>清空按钮显隐（有文本时显示）。</summary>
        private void UpdateClearVisibility()
        {
            if (ClearButton == null) return;
            ClearButton.Visibility = string.IsNullOrEmpty(InputBox.Text)
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnFilterChanged()
        {
            // 机构限定变化：重新联想（联系人列表跟随机构）
            if (Kind != "CONTACT" || InputBox.IsKeyboardFocusWithin) return;
            _debounce.Stop();
            _debounce.Start();
        }

        private async System.Threading.Tasks.Task SearchAsync()
        {
            var service = Directory;
            if (service == null) return;

            _searchCts?.Cancel();
            var cts = new CancellationTokenSource();
            _searchCts = cts;
            try
            {
                var result = await service.SearchAsync(Kind, Text, Kind == "CONTACT" ? InstitutionId : null, cts.Token);
                if (cts.IsCancellationRequested) return;

                Candidates.Clear();
                foreach (var entry in result) Candidates.Add(entry);
                SuggestPopup.IsOpen = Candidates.Count > 0 && InputBox.IsKeyboardFocused;
                if (CandidateList.Items.Count > 0) CandidateList.SelectedIndex = 0;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                SuggestPopup.IsOpen = false;
                Services.ToastService.Show("CRM 联想失败：" + error.Message, ToastKind.Error);
            }
        }

        /// <summary>键盘：↓/↑ 候选移动、Enter 选中候选、Esc 关闭浮层。</summary>
        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && CandidateList.Items.Count > 0)
            {
                SuggestPopup.IsOpen = true;
                var next = System.Math.Min(CandidateList.SelectedIndex + 1, CandidateList.Items.Count - 1);
                CandidateList.SelectedIndex = next;
                CandidateList.ScrollIntoView(CandidateList.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Up && CandidateList.Items.Count > 0)
            {
                var prev = System.Math.Max(CandidateList.SelectedIndex - 1, 0);
                CandidateList.SelectedIndex = prev;
                CandidateList.ScrollIntoView(CandidateList.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && SuggestPopup.IsOpen && CandidateList.SelectedItem is CrmDirectoryEntry entry)
            {
                Apply(entry);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                SuggestPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void OnCandidateClicked(object sender, MouseButtonEventArgs e)
        {
            if (CandidateList.SelectedItem is CrmDirectoryEntry entry) Apply(entry);
        }

        /// <summary>选中：回填标准名称与 Id，关浮层（焦点仍在输入框，便于继续 Tab 到下一列）。</summary>
        private void Apply(CrmDirectoryEntry entry)
        {
            _syncingText = true;
            try
            {
                Text = entry.Name;
                PushSource(entry.Id);
                UpdateClearVisibility();
            }
            finally
            {
                _syncingText = false;
            }
            SuggestPopup.IsOpen = false;
        }

        /// <summary>清空：名称与 Id 一起清（显式推源）。</summary>
        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            InputBox.Text = string.Empty;
            PushSource(null);
            UpdateClearVisibility();
            InputBox.Focus();
        }
    }
}

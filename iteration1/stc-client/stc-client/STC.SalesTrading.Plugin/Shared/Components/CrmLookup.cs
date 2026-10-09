using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using STC.SalesTrading.Plugin.Abstractions;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// CRM 自动补全控件：延迟触发搜索，用请求版本过滤过时应答，支持鼠标与键盘显式选择。
    /// 显示文字与业务身份分开；自由文本不代表匹配成功，选择命令携带机构和联系人 ID。
    /// Directory 从父级继承，InstitutionId 限定联系人搜索范围；无机构时允许跨机构搜索。
    /// </summary>
    public sealed class CrmLookup : TextBox
    {
        public static readonly DependencyProperty DirectoryProperty = DependencyProperty.RegisterAttached(
            "Directory", typeof(ICrmDirectory), typeof(CrmLookup),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static void SetDirectory(DependencyObject target, ICrmDirectory value) => target.SetValue(DirectoryProperty, value);
        public static ICrmDirectory GetDirectory(DependencyObject target) => (ICrmDirectory)target.GetValue(DirectoryProperty);

        public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
            nameof(Kind), typeof(string), typeof(CrmLookup), new PropertyMetadata("INSTITUTION"));
        public string Kind { get => (string)GetValue(KindProperty); set => SetValue(KindProperty, value); }

        public static readonly DependencyProperty InstitutionIdProperty = DependencyProperty.Register(
            nameof(InstitutionId), typeof(string), typeof(CrmLookup), new PropertyMetadata(null, ScopeChanged));
        public string InstitutionId { get => (string)GetValue(InstitutionIdProperty); set => SetValue(InstitutionIdProperty, value); }

        public static readonly DependencyProperty SelectionCommandProperty = DependencyProperty.Register(
            nameof(SelectionCommand), typeof(ICommand), typeof(CrmLookup));
        public ICommand SelectionCommand { get => (ICommand)GetValue(SelectionCommandProperty); set => SetValue(SelectionCommandProperty, value); }

        private readonly DispatcherTimer debounce;
        private readonly Popup popup;
        private readonly ListBox suggestions;
        private readonly TextBlock status;
        private long requestVersion;
        private bool selecting;

        public CrmLookup()
        {
            suggestions = new ListBox
            {
                DisplayMemberPath = nameof(CrmChoice.DisplayName),
                MaxHeight = 220,
                Background = new SolidColorBrush(Color.FromRgb(29, 36, 45)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            status = new TextBlock { Margin = new Thickness(8), Foreground = Brushes.LightGray, TextWrapping = TextWrapping.Wrap };
            var panel = new StackPanel();
            panel.Children.Add(status);
            panel.Children.Add(suggestions);
            popup = new Popup
            {
                PlacementTarget = this,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                Child = new Border
                {
                    MinWidth = 300,
                    MaxWidth = 500,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.SlateGray,
                    Background = suggestions.Background,
                    Child = panel
                }
            };
            debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            debounce.Tick += Search;
            TextChanged += (sender, args) => ScheduleSearch();
            GotKeyboardFocus += (sender, args) => ScheduleSearch();
            PreviewKeyDown += HandleKey;
            suggestions.PreviewMouseLeftButtonUp += (sender, args) =>
            {
                var item = ItemsControl.ContainerFromElement(suggestions, args.OriginalSource as DependencyObject) as ListBoxItem;
                if (item?.Content is CrmChoice choice)
                {
                    Select(choice);
                    args.Handled = true;
                }
            };
            LostKeyboardFocus += (sender, args) => Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsKeyboardFocusWithin && !suggestions.IsKeyboardFocusWithin) Close();
            }));
            Unloaded += (sender, args) => Close();
            DataContextChanged += (sender, args) => Close();
        }

        private static void ScopeChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            ((CrmLookup)target).ScheduleSearch();
        }

        private void ScheduleSearch()
        {
            Close();
            if (!selecting && IsKeyboardFocusWithin && !string.IsNullOrWhiteSpace(Text)) debounce.Start();
        }

        private async void Search(object sender, EventArgs args)
        {
            debounce.Stop();
            long version = ++requestVersion;
            suggestions.ItemsSource = null;
            status.Text = "正在搜索…";
            status.Visibility = Visibility.Visible;
            popup.IsOpen = true;
            try
            {
                var directory = GetDirectory(this);
                if (directory == null) throw new InvalidOperationException("CRM 查询服务未就绪");
                var matches = await directory.SearchAsync(Kind, Text.Trim(), InstitutionId);
                if (version != requestVersion || !IsKeyboardFocusWithin) return;
                suggestions.ItemsSource = matches;
                status.Text = matches.Count == 0 ? "没有匹配结果" : "请选择匹配项";
                status.Visibility = matches.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception error)
            {
                if (version != requestVersion || !IsKeyboardFocusWithin) return;
                status.Text = "查询失败：" + error.Message + "（重新输入或按 ↓ 重试）";
            }
        }

        private void HandleKey(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                Close();
                args.Handled = true;
            }
            else if (args.Key == Key.Down || args.Key == Key.Up)
            {
                if (!popup.IsOpen || suggestions.Items.Count == 0) ScheduleSearch();
                else
                {
                    int change = args.Key == Key.Down ? 1 : -1;
                    suggestions.SelectedIndex = Math.Max(0, Math.Min(suggestions.Items.Count - 1, suggestions.SelectedIndex + change));
                    suggestions.ScrollIntoView(suggestions.SelectedItem);
                }
                args.Handled = true;
            }
            else if (args.Key == Key.Enter && popup.IsOpen && suggestions.SelectedItem is CrmChoice choice)
            {
                Select(choice);
                args.Handled = true;
            }
        }

        private void Select(CrmChoice choice)
        {
            var command = SelectionCommand;
            if (command == null || !command.CanExecute(choice)) return;
            selecting = true;
            try
            {
                // The command updates both display names and IDs atomically in the model.
                command.Execute(choice);
                GetBindingExpression(TextProperty)?.UpdateTarget();
                Focus();
                CaretIndex = Text.Length;
            }
            finally
            {
                selecting = false;
                Close();
            }
        }

        private void Close()
        {
            requestVersion++;
            debounce.Stop();
            popup.IsOpen = false;
            suggestions.ItemsSource = null;
        }
    }
}

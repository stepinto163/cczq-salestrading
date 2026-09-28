using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 搜索联想组件。Suggestions 为全量候选，输入时组件内做包含过滤；
    /// 选中候选后写回 Text 并触发 SuggestionSelected 事件。
    /// 组件级过滤仅作骨架演示：真实场景候选集由服务端/模块 VM 提供（本组件不感知业务）。
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(SearchBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(SearchBox), new PropertyMetadata("搜索"));

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register(
            nameof(Suggestions), typeof(IEnumerable), typeof(SearchBox), new PropertyMetadata(null));

        public SearchBox()
        {
            InitializeComponent();
            PART_TextBox.TextChanged += OnTextChanged;
            PART_TextBox.PreviewKeyDown += OnPreviewKeyDown;
            PART_List.SelectionChanged += OnSuggestionSelected;
            PART_TextBox.LostFocus += OnLostFocus;
        }

        /// <summary>当前输入文本（双向）。</summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>占位提示。</summary>
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        /// <summary>全量候选集合（字符串）。</summary>
        public IEnumerable Suggestions
        {
            get { return (IEnumerable)GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
        }

        /// <summary>候选被选中（参数为选中文本）。</summary>
        public event Action<string> SuggestionSelected;

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            PART_Watermark.Visibility = string.IsNullOrEmpty(Text) ? Visibility.Visible : Visibility.Collapsed;

            var keyword = (Text ?? string.Empty).Trim();
            var source = Suggestions as IEnumerable<string>;
            var filtered = (source ?? Enumerable.Empty<string>())
                .Where(s => s != null && (keyword.Length == 0 || s.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            PART_List.ItemsSource = filtered;
            PART_Popup.IsOpen = filtered.Count > 0 && !string.IsNullOrEmpty(keyword);
            if (filtered.Count > 0) PART_List.SelectedIndex = -1;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!PART_Popup.IsOpen)
            {
                if (e.Key == Key.Down) OpenIfHasItems();
                return;
            }

            switch (e.Key)
            {
                case Key.Down:
                    if (PART_List.Items.Count > 0)
                        PART_List.SelectedIndex = Math.Min(PART_List.SelectedIndex + 1, PART_List.Items.Count - 1);
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (PART_List.Items.Count > 0)
                        PART_List.SelectedIndex = Math.Max(PART_List.SelectedIndex - 1, 0);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (PART_List.SelectedIndex >= 0)
                        ApplySelection((string)PART_List.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    PART_Popup.IsOpen = false;
                    break;
            }
        }

        private void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (PART_List.SelectedItem is string selected)
            {
                ApplySelection(selected);
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // 点击候选浮层时 Popup 自身处理焦点，文本框失焦不立即关闭；
            // 点击组件外区域时关闭浮层。
            if (!PART_List.IsKeyboardFocusWithin && !PART_TextBox.IsKeyboardFocusWithin)
            {
                PART_Popup.IsOpen = false;
            }
        }

        private void OpenIfHasItems()
        {
            if (PART_List.Items.Count > 0)
            {
                PART_Popup.IsOpen = true;
                PART_List.SelectedIndex = 0;
            }
        }

        private void ApplySelection(string value)
        {
            Text = value;
            PART_Popup.IsOpen = false;
            SuggestionSelected?.Invoke(value);
        }
    }
}

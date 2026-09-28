using System;
using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>筛选面板「标签 + 下拉」行（原三面板手写同构行的公共件）。</summary>
    public partial class FilterComboRow : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(FilterComboRow), new PropertyMetadata(null));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items), typeof(string), typeof(FilterComboRow),
            new PropertyMetadata(null, OnItemsChanged));

        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            nameof(Values), typeof(string[]), typeof(FilterComboRow), new PropertyMetadata(null));

        public static readonly DependencyProperty RowMarginProperty = DependencyProperty.Register(
            nameof(RowMargin), typeof(Thickness), typeof(FilterComboRow),
            new PropertyMetadata(new Thickness(0, 0, 0, 8)));

        public FilterComboRow()
        {
            InitializeComponent();
        }

        /// <summary>行首标签文案。</summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>下拉项文案，逗号分隔；首项即默认选中（原型 select 默认第一项）。</summary>
        public string Items
        {
            get { return (string)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        /// <summary>拆分后的下拉项（供 ItemsSource 绑定）。</summary>
        public string[] Values
        {
            get { return (string[])GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        /// <summary>行外距（默认 "0,0,0,8"；区块末行传 "0"）。</summary>
        public Thickness RowMargin
        {
            get { return (Thickness)GetValue(RowMarginProperty); }
            set { SetValue(RowMarginProperty, value); }
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var row = (FilterComboRow)d;
            var items = e.NewValue as string;
            row.Values = string.IsNullOrEmpty(items) ? Array.Empty<string>() : items.Split(',');
        }
    }
}

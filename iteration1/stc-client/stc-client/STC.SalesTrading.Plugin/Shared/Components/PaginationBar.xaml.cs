using System;
using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 统一分页条（原型 rec-pagination 同款）：左侧总数/页码摘要，右侧 上一页/页码/下一页。
    /// 换页 = 组件钳制后改 <see cref="CurrentPage"/>（TwoWay 绑定），VM 在属性变化回调里触发换页；
    /// 组件不依赖命令。边界：首页禁用上一页、末页禁用下一页，加载中（IsLoading）两者均禁用。
    /// </summary>
    public partial class PaginationBar : UserControl
    {
        public static readonly DependencyProperty TotalCountProperty = DependencyProperty.Register(
            nameof(TotalCount), typeof(int), typeof(PaginationBar),
            new PropertyMetadata(0, OnCountChanged));

        public static readonly DependencyProperty TotalPagesProperty = DependencyProperty.Register(
            nameof(TotalPages), typeof(int), typeof(PaginationBar),
            new PropertyMetadata(1, OnCountChanged));

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
            nameof(CurrentPage), typeof(int), typeof(PaginationBar),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCountChanged));

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading), typeof(bool), typeof(PaginationBar),
            new PropertyMetadata(false, OnCountChanged));

        public PaginationBar()
        {
            InitializeComponent();
            Refresh();
        }

        /// <summary>总行数（摘要）。</summary>
        public int TotalCount
        {
            get { return (int)GetValue(TotalCountProperty); }
            set { SetValue(TotalCountProperty, value); }
        }

        /// <summary>总页数（VM 按 ceil(TotalCount/PageSize) 计算）。</summary>
        public int TotalPages
        {
            get { return (int)GetValue(TotalPagesProperty); }
            set { SetValue(TotalPagesProperty, value); }
        }

        /// <summary>当前页（1-based；TwoWay——组件换页写入，VM 换页回调更新）。</summary>
        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        /// <summary>补拉/重查进行中（禁用两个按钮）。</summary>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaginationBar)d).Refresh();
        }

        private void Refresh()
        {
            var totalPages = TotalPages < 1 ? 1 : TotalPages;
            var page = CurrentPage < 1 ? 1 : (CurrentPage > totalPages ? totalPages : CurrentPage);
            if (page != CurrentPage) CurrentPage = page;

            SummaryText.Text = "共 " + TotalCount + " 条 · 第 " + page + "/" + totalPages + " 页";
            PageText.Text = page.ToString();
            PrevButton.IsEnabled = page > 1 && !IsLoading;
            NextButton.IsEnabled = page < totalPages && !IsLoading;
        }

        private void OnPrevClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage > 1) CurrentPage--;
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage < TotalPages) CurrentPage++;
        }
    }
}

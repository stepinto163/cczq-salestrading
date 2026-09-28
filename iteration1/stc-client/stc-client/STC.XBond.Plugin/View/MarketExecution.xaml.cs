using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using DataGrid = System.Windows.Controls.DataGrid;
using UserControl = System.Windows.Controls.UserControl;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// MarketExecution.xaml 的交互逻辑
    /// </summary>
    public partial class MarketExecution : UserControl
    {
        private MarketExecutionViewModel _marketExecutionViewModel = new MarketExecutionViewModel();

        public MarketExecution()
        {
            InitializeComponent();
        }

        private void MarketExecution_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _marketExecutionViewModel;

            marketExecutionNew.Loaded += (s, ex) =>
            {
                var scrollViewer = GetScrollViewer(marketExecutionNew);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                }
            };
        }

        #region 滚动条
        // 获取 DataGrid 的 ScrollViewer
        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj == null) return null;

            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        // 处理滚动事件
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset == e.ExtentHeight - e.ViewportHeight)
            {
                if (marketExecutionNew.Items.Count > 0)
                {
                    // 滚动到最底部
                    var lastItem = marketExecutionNew.Items[marketExecutionNew.Items.Count - 1];
                    if (lastItem != null)
                    {
                        string date = DateTime.Now.ToString("yyyy-MM-dd");
                        MarketExecutionsItem list = _marketExecutionViewModel.FilteredMarketExecutions.View.Cast<MarketExecutionsItem>().Where(o => (o.Time ?? o.TimeStamp).StartsWith(date)).OrderBy(o => o.TimeStamp).FirstOrDefault();
                        if (list != null)
                        {
                            _marketExecutionViewModel.GetRocksDbDataByItem(list);
                        }

                    }
                }
            }
            if (e.VerticalOffset == 0)
            {
                MarketExecutionProcesser.Processer._isScrollToEnd = false;
            }
        }
        #endregion

        public static readonly DependencyProperty TitleHeightProperty = DependencyProperty.Register("TitleHeight", typeof(int),
            typeof(MarketExecution), new PropertyMetadata(24, onTitleHeightPropertyChange));

        public static readonly DependencyProperty SecurtityIDProperty = DependencyProperty.Register("SecurtityID", typeof(string),
            typeof(MarketExecution), new PropertyMetadata("", new PropertyChangedCallback(OnSecurtityIDPropertyChange)));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string),
            typeof(MarketExecution), new PropertyMetadata("", new PropertyChangedCallback(OnTitlePropertyChange)));

        public static readonly DependencyProperty SearchTextVisibilityProperty = DependencyProperty.Register("SearchTextVisibility", typeof(Visibility),
            typeof(MarketExecution), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnSearchTextVisibilityPropertyChange)));

        public static readonly DependencyProperty PopupButtonVisibilityProperty = DependencyProperty.Register("PopupButtonVisibility", typeof(Visibility),
           typeof(MarketExecution), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnPopupButtonVisibilityPropertyChange)));

        static void OnSecurtityIDPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MarketExecution source = (MarketExecution)sender;
            if (source.DataContext != null && source.DataContext is MarketExecutionViewModel)
            {
                MarketExecutionViewModel viewModel = (MarketExecutionViewModel)source.DataContext;
                viewModel.SearchText = args.NewValue.ToString();
            }
        }

        private static void onTitleHeightPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MarketExecution source = (MarketExecution)sender;
            int height = CommonUtils.ConvertToInt(args.NewValue.ToString());
            source.Title_Part.Height = new GridLength( height < 24 ? 24 : height);
        }


        static void OnTitlePropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MarketExecution source = (MarketExecution)sender;
            source.textBlock.Text = args.NewValue.ToString();
        }

        static void OnSearchTextVisibilityPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MarketExecution source = (MarketExecution)sender;
            source.Search_TextBox.Visibility = (Visibility)args.NewValue;
        }

        static void OnPopupButtonVisibilityPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MarketExecution source = (MarketExecution)sender;
            source.PopupBtn.Visibility = (Visibility)args.NewValue;
        }

        public int TitleHeigh
        {
            get { return (int)GetValue(TitleHeightProperty); }
            set { SetValue(TitleHeightProperty, value); }
        }

        public string SecurtityID
        {
            get { return (string)GetValue(SecurtityIDProperty); }
            set { SetValue(SecurtityIDProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public Visibility SearchTextVisibility
        {
            get { return (Visibility)GetValue(SearchTextVisibilityProperty); }
            set { SetValue(SearchTextVisibilityProperty, value); }
        }

        public Visibility PopupButtonVisibility
        {
            get { return (Visibility)GetValue(PopupButtonVisibilityProperty); }
            set { SetValue(PopupButtonVisibilityProperty, value); }
        }
    }
}

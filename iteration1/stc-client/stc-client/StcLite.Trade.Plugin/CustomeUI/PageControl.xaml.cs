using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BDB.Trade.Plugin.CustomeUI
{
    /// <summary>
    /// PageControl.xaml 的交互逻辑
    /// </summary>
    public partial class PageControl : UserControl, INotifyPropertyChanged
    {
        #region 事件
        /// <summary>
        /// 分页事件
        /// </summary>
        public event EventHandler<PageChangedEventArgs> PageChanged;
        #endregion
        public static readonly DependencyProperty TotalCountProperty = DependencyProperty.Register("TotalCount", typeof(int),
            typeof(PageControl), new PropertyMetadata(0, new PropertyChangedCallback(OnTotalCountPropertyChange)));
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageControl));


        public int TotalCount
        {
            get
            {
                return (int)GetValue(TotalCountProperty);
            }
            set
            {
                SetValue(TotalCountProperty, value);
                this.RecordCount = value;
                RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
            }
        }

        private static void OnTotalCountPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            PageControl source = (PageControl)sender;

            var newValue = CommonUtils.ConvertToInt(args.NewValue.ToString());
            //if (source.TotalCount != newValue)
            //{
                source.TotalCount = newValue;
            //}

            //if (source.TotalCount!= newValue)
            //{
            //    source.SearchTermTextBox.Text = newValue;
            //}
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPageNum
        {
            get { return (int)GetValue(CurrentPageNumProperty); }
            set { SetValue(CurrentPageNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPageNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageNumProperty =
            DependencyProperty.Register("CurrentPageNum", typeof(int), typeof(PageControl), new PropertyMetadata(0, new PropertyChangedCallback(OnCurrentPageNumPropertyChange)));

        private static void OnCurrentPageNumPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            PageControl source = (PageControl)d;

            var newValue = CommonUtils.ConvertToInt(e.NewValue.ToString());

            if (source.Page != newValue)
            {
                source.Page = newValue;
            }
        }

        #region 变量
        private ObservableCollection<PageControlItemModel> _collection = new ObservableCollection<PageControlItemModel>();
        private List<PageControlItemModel> _list = null;
        #endregion

        #region 属性
        private int _FontSize = 12;
        /// <summary>
        /// 文字字体大小
        /// </summary>
        public int FontSize
        {
            get { return _FontSize; }
            set
            {
                _FontSize = value;
                OnPropertyChanged("FontSize");

                CalcPageNumList(); //计算页码
            }
        }
        #endregion

        #region 分页相关属性
        private int _PageCount = 1;
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount
        {
            get { return _PageCount; }
            set
            {
                _PageCount = value;
                OnPropertyChanged("PageCount");
            }
        }

        private int _Page = 1;
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page
        {
            get { return _Page; }
            set
            {
                _Page = value;
                OnPropertyChanged("Page");

                CalcPageNumList(); //计算页码
            }
        }

        private int _RecordCount = 0;
        /// <summary>
        /// 记录总数
        /// </summary>
        public int RecordCount
        {
            get { return _RecordCount; }
            set
            {
                _RecordCount = value;
                OnPropertyChanged("RecordCount");

                CalcPageNumList(); //计算页码
            }
        }

        private int _PageSize = CommonConstants.pageCount;
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize
        {
            get { return _PageSize; }
            set
            {
                _PageSize = value;
                OnPropertyChanged("PageSize");

                CalcPageNumList(); //计算页码
            }
        }

        private int _ContinuousCount = 3;
        /// <summary>
        /// 当前页码右边连续页码数
        /// </summary>
        public int ContinuousCount
        {
            get { return _ContinuousCount; }
            set
            {
                _ContinuousCount = value;
                OnPropertyChanged("_ContinuousCount");

                CalcPageNumList(); //计算页码
            }
        }

        #endregion

        #region 构造函数
        public PageControl()
        {
            InitializeComponent();

            this.itemsControl.ItemsSource = _collection;
        }
        #endregion

        #region 单击页码事件
        private void btnNum_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            PageControlItemModel itemModel = btn.CommandParameter as PageControlItemModel;
            if (itemModel.Page != Page)
            {
                Page = itemModel.Page;
                CalcPageNumList();

                if (PageChanged != null)
                {
                    PageChangedEventArgs args = new PageChangedEventArgs(itemModel.Page);
                    PageChanged(sender, args);
                }
            }
        }
        #endregion

        #region 计算页码
        /// <summary>
        /// 计算页码
        /// </summary>
        private void CalcPageNumList()
        {
            PageCount = (RecordCount - 1) / PageSize + 1; //计算总页数PageCount

            _list = new List<PageControlItemModel>();

            //第一页
            PageControlItemModel item = new PageControlItemModel(1, Page);
            _list.Add(item);

            //当前页码连续页码
            for (int i = Page - ContinuousCount; i <= Page + ContinuousCount; i++)
            {
                if (i > 0 && i < PageCount)
                {
                    item = new PageControlItemModel(i, Page);
                    if (!_list.Exists(a => a.Page == item.Page))
                    {
                        _list.Add(item);
                    }
                }
            }

            //最后一页
            item = new PageControlItemModel(PageCount, Page);
            if (!_list.Exists(a => a.Page == item.Page))
            {
                _list.Add(item);
            }

            for (int i = _list.Count - 1; i > 0; i--)
            {
                if (_list[i].Page - _list[i - 1].Page > 1)
                {
                    _list.Insert(i, new PageControlItemModel(0, Page, 2));
                }
            }

            //上一页下一页
            if (Page == 1)
            {
                this.btnPrePage.IsEnabled = false;
                this.btnPrePage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88dddddd"));
            }
            else
            {
                this.btnPrePage.IsEnabled = true;
                this.btnPrePage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff"));
            }
            if (Page == PageCount)
            {
                this.btnNextPage.IsEnabled = false;
                this.btnNextPage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88dddddd"));
            }
            else
            {
                this.btnNextPage.IsEnabled = true;
                this.btnNextPage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff"));
            }

            _collection.Clear();
            _list.ForEach(a => { _collection.Add(a); });
        }
        #endregion

        #region 上一页
        private void btnPrePage_Click(object sender, RoutedEventArgs e)
        {
            int prePage = Page - 1;
            if (prePage < 1) prePage = 1;
            if (prePage != Page)
            {
                Page = prePage;
                CalcPageNumList();

                if (PageChanged != null)
                {
                    PageChangedEventArgs args = new PageChangedEventArgs(prePage);
                    PageChanged(sender, args);
                }
            }
        }
        #endregion

        #region 下一页
        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            int nextPage = Page + 1;
            if (nextPage > PageCount) nextPage = PageCount;
            if (nextPage != Page)
            {
                Page = nextPage;
                CalcPageNumList();

                if (PageChanged != null)
                {
                    PageChangedEventArgs args = new PageChangedEventArgs(nextPage);
                    PageChanged(sender, args);
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged接口
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

    }

    #region 分页控件Item Model
    /// <summary>
    /// 分页控件Item Model
    /// </summary>
    public class PageControlItemModel : INotifyPropertyChanged
    {
        private int _Type = 1;
        /// <summary>
        /// 类型(1数字 2省略号)
        /// </summary>
        public int Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                OnPropertyChanged("Type");

                if (_Type == 1)
                {
                    NumVisible = Visibility.Visible;
                    OmitVisible = Visibility.Collapsed;
                }
                else
                {
                    NumVisible = Visibility.Collapsed;
                    OmitVisible = Visibility.Visible;
                }
            }
        }

        private bool _IsCurrentPage;
        /// <summary>
        /// 是否当前页码
        /// </summary>
        public bool IsCurrentPage
        {
            get { return _IsCurrentPage; }
            set
            {
                _IsCurrentPage = value;
                OnPropertyChanged("IsCurrentPage");

                if (_IsCurrentPage)
                {
                    CurrentPageColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E63034"));
                }
                else
                {
                    CurrentPageColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));
                }
            }
        }

        private SolidColorBrush _CurrentPageColor = new SolidColorBrush(Colors.White);
        /// <summary>
        /// 当前页码颜色
        /// </summary>
        public SolidColorBrush CurrentPageColor
        {
            get { return _CurrentPageColor; }
            set
            {
                _CurrentPageColor = value;
                OnPropertyChanged("CurrentPageColor");
            }
        }

        private int _Page;
        /// <summary>
        /// 页码
        /// </summary>
        public int Page
        {
            get { return _Page; }
            set
            {
                _Page = value;
                OnPropertyChanged("Page");
            }
        }

        private Visibility _NumVisible = Visibility.Visible;
        /// <summary>
        /// 数字可见
        /// </summary>
        public Visibility NumVisible
        {
            get { return _NumVisible; }
            set
            {
                _NumVisible = value;
                OnPropertyChanged("NumVisible");
            }
        }

        private Visibility _OmitVisible = Visibility.Collapsed;
        /// <summary>
        /// 省略号可见
        /// </summary>
        public Visibility OmitVisible
        {
            get { return _OmitVisible; }
            set
            {
                _OmitVisible = value;
                OnPropertyChanged("OmitVisible");
            }
        }

        /// <summary>
        /// 分页控件Item Model
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="currentPage">当前页码</param>
        /// <param name="type">类型(1数字 2省略号)</param>
        public PageControlItemModel(int page, int currentPage, int type = 1)
        {
            Type = type;
            Page = page;
            IsCurrentPage = page == currentPage;
        }

        #region INotifyPropertyChanged接口
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

    }
    #endregion

    #region 分页事件参数
    /// <summary>
    /// 分页事件参数
    /// </summary>
    public class PageChangedEventArgs : EventArgs
    {
        private int _Page = 1;
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page
        {
            get
            {
                return _Page;
            }
        }

        /// <summary>
        /// 分页事件参数
        /// </summary>
        /// <param name="page">当前页码</param>
        public PageChangedEventArgs(int page)
        {
            _Page = page;
        }
    }
    #endregion
}

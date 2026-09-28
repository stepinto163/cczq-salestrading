using STC.Common.Utils;
using STC.Position.Plugin.Command;
using STC.Position.Plugin.ViewModel;
using System;
using System.Collections.Generic;
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

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// HistoryPositionOrderDetail.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryPositionOrderDetail : UserControl
    {
        HistoryPositionOrderDetailViewModel _viewModel = new HistoryPositionOrderDetailViewModel();
        #region 定义 SecurityId
        public string SecurityId
        {
            get { return (string)GetValue(SecurityIdProperty); }
            set 
            { 
                SetValue(SecurityIdProperty, value); 
            }
        }

        public static readonly DependencyProperty SecurityIdProperty = DependencyProperty.Register("SecurityId", typeof(string),
        typeof(HistoryPositionOrderDetail), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnSecurityIdPropertyChange)));

        public static void OnSecurityIdPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            HistoryPositionOrderDetail source = (HistoryPositionOrderDetail)sender;

            if (args.NewValue == null)
            {
                return;
            }

            //source.txt_Securityid.Text = args.NewValue.ToString();
            source._viewModel.InSecurityId = args.NewValue.ToString();
        }

        #endregion

        #region 定义 DealTimeBegin,DealTimeEnd
        public string DealTimeBegin
        {
            get { return (string)GetValue(DealTimeBeginProperty); }
            set
            {
                SetValue(DealTimeBeginProperty, value);
            }
        }

        public static readonly DependencyProperty DealTimeBeginProperty = DependencyProperty.Register("DealTimeBegin", typeof(string),
        typeof(HistoryPositionOrderDetail), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDealTimeBeginPropertyChange)));

        public static void OnDealTimeBeginPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            HistoryPositionOrderDetail source = (HistoryPositionOrderDetail)sender;

            if (args.NewValue == null)
            {
                return;
            }

            source._viewModel.DealTimeBegin = args.NewValue.ToString();
        }


        public string DealTimeEnd
        {
            get { return (string)GetValue(DealTimeEndProperty); }
            set
            {
                SetValue(DealTimeEndProperty, value);
            }
        }

        public static readonly DependencyProperty DealTimeEndProperty = DependencyProperty.Register("DealTimeEnd", typeof(string),
        typeof(HistoryPositionOrderDetail), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDealTimeEndPropertyChange)));

        public static void OnDealTimeEndPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            HistoryPositionOrderDetail source = (HistoryPositionOrderDetail)sender;

            if (args.NewValue == null)
            {
                return;
            }

            source._viewModel.DealTimeEnd = args.NewValue.ToString();
        }


        public string BookId
        {
            get { return (string)GetValue(BookIdProperty); }
            set
            {
                SetValue(BookIdProperty, value);
            }
        }

        public static readonly DependencyProperty BookIdProperty = DependencyProperty.Register("BookId", typeof(string),
        typeof(HistoryPositionOrderDetail), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnBookIdPropertyChange)));

        public static void OnBookIdPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            HistoryPositionOrderDetail source = (HistoryPositionOrderDetail)sender;

            if (args.NewValue == null)
            {
                return;
            }

            source._viewModel.BookId = args.NewValue.ToString();
        }


        #endregion

        /// <summary>
        /// 清空数据
        /// </summary>
        public ICommand ClearData { get { return new ComRelayCommand(ClearDataCommand); } }


        /// <summary>
        /// 查询数据
        /// </summary>
        public ICommand QueryData { get { return new ComRelayCommand(QueryDataCommand); } }

        /// <summary>
        /// 查询数据
        /// </summary>
        public void QueryDataCommand()
        {
            (this.DataContext as HistoryPositionOrderDetailViewModel).EMSIndex = 0;
            (this.DataContext as HistoryPositionOrderDetailViewModel).QueryData();
        }

        private void ClearDataCommand()
        {
            (this.DataContext as HistoryPositionOrderDetailViewModel).ClearData();
        }


        public HistoryPositionOrderDetail()
        {
            InitializeComponent();
            this.DataContext = _viewModel;
            (this.DataContext as HistoryPositionOrderDetailViewModel).HistoryView = this;
            (this.DataContext as HistoryPositionOrderDetailViewModel).EMSPageControl = this.EMSPageControl;
        }


        public void ShowEMSMask(bool flag)
        {
            positionloadMask.Dispatcher.Invoke(() =>
            {
                positionloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void emsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void EMSPageControl_PageChanged(object sender, PageChangedEventArgs e)
        {
            if (e != null)
            {
                (this.DataContext as HistoryPositionOrderDetailViewModel).EMSIndex = e.Page - 1;
                //_historyOrderViewModel.clear();
                (this.DataContext as HistoryPositionOrderDetailViewModel).QueryData();
            }
        }
        public void loadWindows() 
        {
            if (_viewModel != null) 
            {
                this.RefreshBtn.Visibility = Visibility.Visible;
            }
        }
    }
}

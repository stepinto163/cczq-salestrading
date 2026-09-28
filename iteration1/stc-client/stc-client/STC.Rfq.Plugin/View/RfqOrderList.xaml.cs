using STC.Common;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Rfq.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Rfq.Plugin.View
{
    /// <summary>
    /// HistoryPositionOrderDetail.xaml 的交互逻辑
    /// </summary>
    public partial class RfqOrderList : UserControl
    {
        RfqOrderListViewModel _viewModel = new RfqOrderListViewModel();

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
        typeof(RfqOrderList), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnSecurityIdPropertyChange)));

        public static void OnSecurityIdPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            RfqOrderList source = (RfqOrderList)sender;

            if (args.NewValue == null)
            {
                return;
            }

            //source.txt_Securityid.Text = args.NewValue.ToString();
            source._viewModel.InSecurityId = args.NewValue.ToString();
        }

        #endregion

        #region 定义 DealTimeBegin,DealTimeEnd
        ////public string DealTimeBegin
        ////{
        ////    get { return (string)GetValue(DealTimeBeginProperty); }
        ////    set
        ////    {
        ////        SetValue(DealTimeBeginProperty, value);
        ////    }
        ////}

        ////public static readonly DependencyProperty DealTimeBeginProperty = DependencyProperty.Register("DealTimeBegin", typeof(string),
        ////typeof(RfqOrderList), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDealTimeBeginPropertyChange)));

        ////public static void OnDealTimeBeginPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        ////{
        ////    RfqOrderList source = (RfqOrderList)sender;

        ////    if (args.NewValue == null)
        ////    {
        ////        return;
        ////    }

        ////    source._viewModel.DealTimeBegin = args.NewValue.ToString();
        ////}


        ////public string DealTimeEnd
        ////{
        ////    get { return (string)GetValue(DealTimeEndProperty); }
        ////    set
        ////    {
        ////        SetValue(DealTimeEndProperty, value);
        ////    }
        ////}

        ////public static readonly DependencyProperty DealTimeEndProperty = DependencyProperty.Register("DealTimeEnd", typeof(string),
        ////typeof(RfqOrderList), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDealTimeEndPropertyChange)));

        ////public static void OnDealTimeEndPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        ////{
        ////    RfqOrderList source = (RfqOrderList)sender;

        ////    if (args.NewValue == null)
        ////    {
        ////        return;
        ////    }

        ////    source._viewModel.DealTimeEnd = args.NewValue.ToString();
        ////}


        ////public string BookId
        ////{
        ////    get { return (string)GetValue(BookIdProperty); }
        ////    set
        ////    {
        ////        SetValue(BookIdProperty, value);
        ////    }
        ////}

        ////public static readonly DependencyProperty BookIdProperty = DependencyProperty.Register("BookId", typeof(string),
        ////typeof(RfqOrderList), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnBookIdPropertyChange)));

        ////public static void OnBookIdPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        ////{
        ////    RfqOrderList source = (RfqOrderList)sender;

        ////    if (args.NewValue == null)
        ////    {
        ////        return;
        ////    }

        ////    source._viewModel.BookId = args.NewValue.ToString();
        ////}


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
            //(this.DataContext as RfqOrderListViewModel).EMSIndex = 0;
            //(this.DataContext as RfqOrderListViewModel).QueryData();
        }

        private void ClearDataCommand()
        {
            (this.DataContext as RfqOrderListViewModel).ClearData();
        }


        public RfqOrderList()
        {
            InitializeComponent();
            this.DataContext = _viewModel;
            (this.DataContext as RfqOrderListViewModel).HistoryView = this;
            //(this.DataContext as RfqOrderListViewModel).EMSPageControl = this.EMSPageControl;
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

        private void LstReplayStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (this.DataContext != null)
                {
                    List<string> sel = new List<string>();
                    foreach (ListBoxItem item in listBox.SelectedItems)
                    {
                        sel.Add(item.Tag.ToString());
                        
                    }

                    (this.DataContext as RfqOrderListViewModel).LstSelectStatus = sel;
                }

            }
        }

        private void Search_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {

        }

        //private void EMSPageControl_PageChanged(object sender, PageChangedEventArgs e)
        //{
        //    if (e != null)
        //    {
        //        (this.DataContext as RfqOrderListViewModel).EMSIndex = e.Page - 1;
        //        //_historyOrderViewModel.clear();
        //        (this.DataContext as RfqOrderListViewModel).QueryData();
        //    }
        //}
    }
}

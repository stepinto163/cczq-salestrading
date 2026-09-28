using IDC.Frame.Utils;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Component;
using STC.Common.Handler.PT;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.History {
    /// <summary>
    /// OrderWin.xaml 的交互逻辑
    /// </summary>
    public partial class DeleteOrderWin : UserControl, INotifyPropertyChanged {


        private const string _dateFormat = "yyyy-MM-dd";
        private const int PageSize = 40;
        GrExecOrderQueryReq oldRequest = new GrExecOrderQueryReq() { pageSize = PageSize, pageNum = 1, ownOrder = "N" };
        GrExecOrderQueryReq newRequest = new GrExecOrderQueryReq() { pageSize = PageSize, pageNum = 1, ownOrder = "N" };

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<TransOrderServerData> orderInfoModels = new ObservableCollection<TransOrderServerData>();
        /// <summary>
        /// 数据绑定模型
        /// </summary>
        public ObservableCollection<TransOrderServerData> OrderInfoModels {
            get { return orderInfoModels; }
            set {
                if (orderInfoModels != value) {
                    orderInfoModels = value;
                    NotifyPropertyChanged("OrderInfoModels");
                }
            }
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        private string currentPage = "1";
        /// <summary>
        /// 开始日期变化是否刷新数据
        /// </summary>
        bool _isReflashRro = true;
        /// <summary>
        /// 结束日期变化是否刷新数据
        /// </summary>
        bool _isReflashRTo = true;

        public DeleteOrderWin() {
            InitializeComponent();
            dpFrom.SelectedDate = DateTimeEx.ServerTime;
            dpTo.SelectedDate = DateTimeEx.ServerTime;
            string today = DateTime.Today.ToString(_dateFormat);
            oldRequest.updateTimeBegin = newRequest.updateTimeBegin = dpFrom.SelectedDate.HasValue ? dpFrom.SelectedDate.Value.ToString(_dateFormat) : today;
            oldRequest.updateTimeEnd = newRequest.updateTimeEnd = dpTo.SelectedDate.HasValue ? dpTo.SelectedDate.Value.ToString(_dateFormat) : today;
            pageControl.Page = 1;
            pageControl.PageCount = 1;
            QueryOrder();
            this.pageControl.PageChanged += PageControl_PageChanged; ;
            this.dpFrom.SelectedDateChanged += DpFrom_SelectedDateChanged;
            this.dpTo.SelectedDateChanged += DpTo_SelectedDateChanged;

        }

        private void PageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            currentPage = this.pageControl.Page.ToString();
            int current = Convert.ToInt32(currentPage);
            newRequest.pageNum = current;
            //pageControl.tb_CurrentPage.Text = Convert.ToInt32(currentPage).ToString();
            QueryOrder();
        }

        private void DpTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue) {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value) {
                    _isReflashRro = false;
                    dpFrom.SelectedDate = dpTo.SelectedDate.Value;
                }
            }
            currentPage = "1";
            string today = DateTime.Today.ToString(_dateFormat);
            newRequest.updateTimeEnd = dpTo.SelectedDate.HasValue ? dpTo.SelectedDate.Value.ToString(_dateFormat) : today;
        }

        private void DpFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue) {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value) {
                    _isReflashRTo = false;
                    dpTo.SelectedDate = dpFrom.SelectedDate.Value;
                }
            }
            currentPage = "1";
            string today = DateTime.Today.ToString(_dateFormat);
            newRequest.updateTimeBegin = dpFrom.SelectedDate.HasValue ? dpFrom.SelectedDate.Value.ToString(_dateFormat) : today;
        }

        //private void PageControl_CurrentPageChangeEvent(object sender, UI.CustomUI.CurrentPageChangeEventArgs e) {
        //    currentPage = this.pageControl.CurrentPage.ToString();
        //    int current = e.CurrentPage;
        //    newRequest.pageNum = current;
        //    pageControl.tb_CurrentPage.Text = Convert.ToInt32(currentPage).ToString();
        //    QueryOrder();
        //}

        private void OrderWin_Loaded(object sender, RoutedEventArgs e) {
           
        }

        #region 方法
        /// <summary>
        /// 查询订单
        /// </summary>
        private  void QueryOrder() {
            Task.Factory.StartNew(() =>
            {
                List<TransOrderServerData> orderDatas = new List<TransOrderServerData>();
                //总条数
                int count = 0;
                //查询消息
                newRequest.pageNum = Convert.ToInt32(currentPage)-1;
                newRequest.pageSize = PageSize;
                var  hisOrderReplyPo =  GRHandler.Instance.ExecOrderDelQuery(newRequest);
                if (hisOrderReplyPo != null && "0000".Equals(hisOrderReplyPo.Code, StringComparison.Ordinal) && hisOrderReplyPo.Datas != null && hisOrderReplyPo.Datas.Any()) {
                    count = hisOrderReplyPo.totalSize;
                    TransOrderHelper.Instance.ConvertBaseDataToTransData(orderDatas, hisOrderReplyPo.Datas);
                    //foreach (var item in hisOrderReplyPo.Datas)
                    //{
                    //    OrderDetailPo orderDetailPo = new OrderDetailPo();
                    //    orderDetailPo.OrderID = item.OrderID;
                    //    orderDetailPo.OrdStatus = item.OrdStatus;
                    //    orderDetailPo.ExecID = item.ExecID;
                    //    orderDetailPo.seq = Convert.ToDecimal(item.Sequence);
                    //    orderDetailPo.timeToMaturity = item.TimeToMaturity;
                    //    orderDetailPo.SecurityID = item.SecurityID;
                    //    orderDetailPo.goodsShortName = item.Symbol;
                    //    orderDetailPo.Side = item.Side;
                    //    orderDetailPo.fandian = item.FanDian;
                    //    orderDetailPo.price = Convert.ToDecimal(item.Price);
                    //    orderDetailPo.SettlType = item.SettlType;
                    //    orderDetailPo.SettlType2 = item.SettlType2;
                    //    orderDetailPo.dealdate = item.SettlDate;
                    //    orderDetailPo.DealDate2 = item.SettlDate2;
                    //    orderDetailPo.qty = Convert.ToDecimal(item.LastQty);
                    //    orderDetailPo.TraderName = item.TraderName;
                    //    orderDetailPo.InstitutionName = item.InstitutionName;
                    //    orderDetailPo.PartySubName = item.PartySubName;
                    //    orderDetailPo.onlinea = item.OnlineA;
                    //    orderDetailPo.onlineb = item.OnlineB;
                    //    orderDetailPo.CounterPartyTraderName = item.CounterPartyTraderName;
                    //    orderDetailPo.InstitutionName = item.CounterPartyInstitutionName;
                    //    orderDetailPo.CounterPartySubName = item.CounterPartySubName;
                    //    orderDetailPo.createTime = item.TransactTime;
                    //    orderDetailPo.updateTime = item.UpdateTime;
                    //    orderDetailPo.InternalFlag = item.InternalFlag;
                    //    orderDetailPo.closeby = item.CloseBy;
                    //    orderDetailPo.PartySubID = item.PartySubID;
                    //    orderDetailPo.CounterPartySubID = item.CounterPartySubID;
                    //    //orderDetailPo.seq = item.TimeToMaturity;
                    //    //orderDetailPo.seq = item.TimeToMaturity;
                    //    //orderDetailPo.seq = item.TimeToMaturity;
                    //    //orderDetailPo.seq = item.TimeToMaturity;
                    //    orderDetailPos.Add(orderDetailPo);

                    //}
                    OrderInfoModels = new ObservableCollection<TransOrderServerData>(orderDatas);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //waterMark.Width = this.wd_delOrder.ActualWidth + 120;
                        //pageControl.Page = 1;
                        pageControl.Page = newRequest.pageNum+1;
                        pageControl.PageCount = count <= 0 ? 1 : (count % Convert.ToInt32(PageSize) == 0 ? count / Convert.ToInt32(PageSize) : count / Convert.ToInt32(PageSize) + 1);
                        pageControl.PageSize = newRequest.pageSize;
                        pageControl.RecordCount = count;
                        dgRecords.Visibility = Visibility.Visible;
                        dgRecords.DataContext = OrderInfoModels;
                    }));
                }
                else {
                    OrderInfoModels = new ObservableCollection<TransOrderServerData>();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //waterMark.Width = this.wd_delOrder.ActualWidth + 120;
                        oldRequest.pageNum = 0;
                        pageControl.Page = 1;
                        pageControl.PageCount = 1;
                        ///pageControl.tb_CurrentPage.Text = Convert.ToInt32(currentPage).ToString();
                        //pageControl.LastPage = 1;
                        //pageControl.RefalshPage();
                        pageControl.PageSize = newRequest.pageSize;
                        pageControl.RecordCount = 0;
                        dgRecords.Visibility = Visibility.Visible;
                        dgRecords.DataContext = OrderInfoModels;
                    }));
                }
            });
        }


        public async Task<GrExecOrdersReplyPo> GetExecOrderHistRecordData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecOrderHistRecordQuery(req);
            });

            return reply;
        }

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            //this.Close();
        }

        private void BackGround_MouseMove(object sender, MouseEventArgs e) {
            //if (e.LeftButton == MouseButtonState.Pressed) {
            //    this.DragMove();
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            currentPage = "1";
            QueryOrder();
        }

        private void wd_delOrder_SizeChanged(object sender, SizeChangedEventArgs e) {
            //waterMark.Width = this.wd_delOrder.ActualWidth + 120;
        }

        private void wd_delOrder_Loaded(object sender, RoutedEventArgs e)
        {
            UIElement uiElement = (UIElement)this.Content;
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(uiElement);
            adornerLayer.Add(new WatermarkAdorner(uiElement, LocalEnv.UserInfo.FullName));
        }
    }
}

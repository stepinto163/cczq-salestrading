using System;
using System.Windows.Media;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using STC.Common.Utils;
using System.Windows.Input;
using STC.Common.Command;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.Inner;
using STC.Common.View.Component;
using STC.Common.Model.Drp;
using STC.Common.ViewModel.Drp.Utils;
using com.gateway.log;
using STC.Common.Model.Admin;
using System.Collections.Generic;
using STC.Common.Handler.Admin;
using System.Windows.Threading;
using System.Windows.Data;
using System.ComponentModel;
using System.Threading;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Data;

namespace STC.Common.ViewModel.Drp
{
    /// <summary>
    /// Tray Notification Window ViewModel
    /// </summary>
    public class DRPNotificationViewModel : ViewModelBase
    {
        #region 构造函数

        public bool IsSetFocus = false;

        private string id;
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    id = value;
                    this.OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string title;
        /// <summary>
        /// 自定义标题
        /// </summary>
        public string Title 
        {
            get
            {
                return title;
            }
            set
            {
                if (title != value)
                {
                    title = value;
                    this.OnPropertyChanged(nameof(Title));
                }
            }
        }

        private string message;
        /// <summary>
        /// 原始消息
        /// </summary>
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (message != value)
                {
                    message = value;
                    this.OnPropertyChanged(nameof(Message));
                }
            }
        }

        private string detail;
        /// <summary>
        /// DRPDetail
        /// </summary>
        public string Detail
        {
            get
            {
                return detail;
            }
            set
            {
                if (detail != value)
                {
                    detail = value;
                    this.OnPropertyChanged(nameof(Detail));
                }
            }
        }

        private string contenttype;
        /// <summary>
        /// 消息类型
        /// </summary>
        public string ContentType
        {
            get
            {
                return contenttype;
            }
            set
            {
                if (contenttype != value)
                {
                    contenttype = value;
                    this.OnPropertyChanged(nameof(ContentType));
                }
            }
        }


        private bool isshow;
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow
        {
            get
            {
                return isshow;
            }
            set
            {
                if (isshow != value)
                {
                    isshow = value;
                    this.OnPropertyChanged(nameof(IsShow));
                }
            }
        }

        private string delflag;
        /// <summary>
        /// 是否删除
        /// </summary>
        public string DelFlag
        {
            get
            {
                return delflag;
            }
            set
            {
                if (delflag != value)
                {
                    delflag = value;
                    this.OnPropertyChanged(nameof(DelFlag));
                }
            }
        }


        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
        /// <summary>
        /// 边框颜色，激活和失去焦点后变化
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }


        private string symbol;
        public string Symbol { get { return symbol; } set { symbol = value; this.OnPropertyChanged(); } }

        
        private string side;
        /// <summary>
        /// 买卖方向
        /// </summary>
        public string Side { get { return side; } set { side = value; this.OnPropertyChanged(); } }

        private string wholeFlag;

        /// <summary>
        /// 整量
        /// </summary>
        public string WholeFlag { get { return wholeFlag; } set { wholeFlag = value; this.OnPropertyChanged(); } }


        private string price;
        /// <summary>
        /// 收益率
        /// </summary>
        public string Price
        {
            get { return price; }
            set
            {
                price = value;
                this.OnPropertyChanged();
            }
        }


        private string pointprice;
        /// <summary>
        /// 建议收益率
        /// </summary>
        public string PointPrice
        {
            get { return pointprice; }
            set
            {
                pointprice = value;
                this.OnPropertyChanged();
            }
        }


        private string firmType;
        /// <summary>
        /// 整量
        /// </summary>
        public string FirmType { get { return firmType; } set { firmType = value; this.OnPropertyChanged(); } }

        private string amount;
        /// <summary>
        /// 量
        /// </summary>
        public string Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                this.OnPropertyChanged();
            }
        }

        private string pointAmount;
        /// <summary>
        /// 建议量
        /// </summary>
        public string PointAmount
        {
            get
            {
                return pointAmount;
            }
            set
            {
                pointAmount = value;
                this.OnPropertyChanged();
            }
        }


        private string orderStatus;
        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus { get { return orderStatus; } set { orderStatus = value; } }


        private string settle;
        /// <summary>
        /// Settle
        /// </summary>
        public string Settle { get { return settle; } set { settle = value; this.OnPropertyChanged(); } }

        private string showSettle;
        /// <summary>
        /// 用于显示的清算速度
        /// </summary>
        public string ShowSettle { get { return showSettle; } set { showSettle = value; this.OnPropertyChanged(); } }


        private string showSourceSettle;
        /// <summary>
        /// 用于显示的清算速度,转换后
        /// </summary>
        public string ShowSourceSettle { get { return showSourceSettle; } set { showSourceSettle = value; this.OnPropertyChanged(); } }



        public string settldate;
        /// <summary>
        /// 交易日
        /// </summary>
        public string SettlDate { get { return settldate; } set { settldate = value; this.OnPropertyChanged(); } }

        private Visibility _wholeVisibility = Visibility.Collapsed;
        public Visibility WholeVisibility
        {
            get
            {
                return _wholeVisibility;
            }

            set
            {
                _wholeVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private string netPrice;
        /// <summary>
        /// 净价
        /// </summary>
        public string NetPrice { get { return netPrice; } set { netPrice = value; this.OnPropertyChanged(); } }



        private string securityid;
        /// <summary>
        /// 债券ID
        /// </summary>
        public string SecurityID { get { return securityid; } set { securityid = value; this.OnPropertyChanged(); } }


        private string timeInForce;
        public string TimeInForce { get { return timeInForce; } set { timeInForce = value; this.OnPropertyChanged(); } }


        private string iocTimeOut;
        /// <summary>
        /// 超时时间
        /// </summary>
        public string IocTimeOut { get { return iocTimeOut; } set { iocTimeOut = value; this.OnPropertyChanged(); } }

        private string validuntiltime;
        public string ValidUntilTime { get { return validuntiltime; } set { validuntiltime = value; this.OnPropertyChanged(); } }


        private string quoteid;
        /// <summary>
        /// 询价ID
        /// </summary>
        public string QuoteID { get { return quoteid; } set { quoteid = value; this.OnPropertyChanged(); } }


        private string orderId;
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get { return orderId; } set { orderId = value; this.OnPropertyChanged(); } }

        private string actiontype;
        public string ActionType { get { return actiontype; } set { actiontype = value; this.OnPropertyChanged(); } }


        private string ioccount;
        /// <summary>
        /// 回价个数
        /// </summary>
        public string IocCount { get { return ioccount; } set { ioccount = value; this.OnPropertyChanged(); } }


        private string _showside;
        /// <summary>
        /// 交易方向，用于显示
        /// </summary>
        public string ShowSide
        {
            get
            {
                return _showside;
            }

            set
            {
                _showside = value;

                this.OnPropertyChanged();
            }
        }

        private string _ShowPrice;
        /// <summary>
        /// 收益率 用于显示
        /// </summary>
        public string ShowPrice
        {
            get
            {
                return _ShowPrice;
            }

            set
            {
                _ShowPrice = value;
                this.OnPropertyChanged();
            }
        }


        private DRPDetail drpdetail;
        /// <summary>
        /// 原始 JSON
        /// </summary>
        public DRPDetail DrpDetail
        {
            get { return drpdetail; }
            set
            {
                if (value != null)
                {
                    drpdetail = value;
                    this.OnPropertyChanged(nameof(DrpDetail));
                }
                
            }
        }

        private DRPInfo2Model drpinfo2model;
        /// <summary>
        /// Info2
        /// </summary>
        public DRPInfo2Model DrpInfo2Model
        {
            get { return drpinfo2model; }
            set
            {
                if (value != null)
                {
                    drpinfo2model = value;
                    this.OnPropertyChanged(nameof(DrpInfo2Model));
                }
            }
        }

        private ObservableCollection<DRPListModel> _gridDatas;

        public ObservableCollection<DRPListModel> GridDatas
        {
            get
            {
                if (_gridDatas == null)
                    _gridDatas = new ObservableCollection<DRPListModel>();

                return _gridDatas;
            }

            set
            {
                if (value != null)
                {
                    _gridDatas = value;
                    this.OnPropertyChanged(nameof(GridDatas));
                }
            }
        }

        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }

        private DRPListModel selectDrp;

        public DRPListModel SelectDrp
        {
            get { return selectDrp; }
            set
            {
                if (value != null)
                {
                    selectDrp = value;
                    this.OnPropertyChanged(nameof(SelectDrp));
                }
            }
        }

        private decimal _leavesqtytotal = decimal.Zero;
        /// <summary>
        /// 待确认量
        /// </summary>
        public decimal LeavesQtyTotal
        {
            get
            {
                return _leavesqtytotal;
            }
            set
            {
                _leavesqtytotal = value;
                this.OnPropertyChanged();
            }
        }

        private string _showleavesqtytotal = "--";
        /// <summary>
        /// 待确认量
        /// </summary>
        public string ShowLeavesQtyTotal
        {
            get
            {
                return _showleavesqtytotal;
            }
            set
            {
                _showleavesqtytotal = value;
                this.OnPropertyChanged();
            }
        }

        private string _showbestpriceqtytip = "";
        /// <summary>
        /// Tool Tip BestPrice And BestQty
        /// </summary>
        public string ShowBestPriceQtyTip
        {
            get
            {
                return _showbestpriceqtytip;
            }
            set
            {
                _showbestpriceqtytip = value;
                this.OnPropertyChanged();
            }
        }


        #region IRS
        /// <summary>
        /// 是否显示 IRS模式, IRS时不可见 现券时可见
        /// </summary>
        private Visibility isShowIRS = Visibility.Visible;
        public Visibility IsShowIRS
        {
            get { return isShowIRS; }
            set
            {
                isShowIRS = value;
                this.OnPropertyChanged("IsShowIRS");
            }
        }

        /// <summary>
        /// 是否是交易所债券
        /// </summary>
        private bool isShowSHE = false;
        public bool IsShowSHE
        {
            get { return isShowSHE; }
            set
            {
                isShowSHE = value;
                this.OnPropertyChanged("IsShowIRS");
            }
        }
        #endregion

        #endregion

        public ICommand OpenDrpProcDiagCommand { get; set; }
        public ICommand CloseSelfCommand { get; set; }

        //Modi:cyc Date:2023-03-24 Func:增加倒计时
        System.Threading.Timer descTimer;

        private object Updatelock = new object();

        /// <summary>
        /// 实时更新最优价和最优量
        /// </summary>
        private SendPriceModelAdapter _sendPriceAdapter = null;

        private SendPriceItem sendPriceItem = null;

        public SendPriceItem SendPriceItem
        {
            get { return sendPriceItem; }
            set
            {
                if (value != null)
                {
                    sendPriceItem = value;
                    this.OnPropertyChanged(nameof(SendPriceItem));
                }
            }
        }

        public DRPNotificationViewModel()
        {
            this.OpenDrpProcDiagCommand = new DelegateCommand(new Action<object>(OnOpenDrpProcDiagCommand));
            this.CloseSelfCommand = new ComRelayCommand(OnCloseSelfCommand);
            StartTimeTick();

            _sendPriceAdapter = new SendPriceModelAdapter();

            this.Mv.Source = GridDatas;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="side"></param>
        public void SetSortDescriptions(string side)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //排序 ShowSide 1 Bid Buy asc  2 Ofr  desc
                this.Mv.SortDescriptions.Add(new SortDescription("EnabledBtn", ListSortDirection.Descending));
                this.Mv.SortDescriptions.Add(new SortDescription("Price", side == "1" ? ListSortDirection.Descending : ListSortDirection.Ascending));
                this.Mv.SortDescriptions.Add(new SortDescription("TransactTime", ListSortDirection.Descending));
                this.Mv.View.Refresh();

                
                _sendPriceAdapter.SetFilterParam(false, this.settle, "");
                SendPriceItem = new SendPriceItem();
                SendPriceItem.Security = this.SecurityID;
                _sendPriceAdapter.Init(SendPriceItem);
                // 订阅报价
                _sendPriceAdapter.OnMDPrice += _sendPriceAdapter_OnMDPrice;
            });
        }

        private void _sendPriceAdapter_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (obj.SecurityID == SendPriceItem.Security)
            {
                this.ShowBestPriceQtyTip = $"{SendPriceItem.BestBidShort} {SendPriceItem.BestBidShowAmount} | {SendPriceItem.BestOfrShort} {SendPriceItem.BestOfrShowAmount}";
            }
        }

        /// <summary>
        /// 报价或议价
        /// </summary>
        /// <param name="inparam"></param>
        public void OnReplyCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    string errorMsg = string.Empty;
                    bool NeedPrompt = false;
                    string info1 = "1";

                    decimal d = 0;
                    if (IsShowIRS == Visibility.Collapsed)
                    {
                        // IRS
                        if (decimal.TryParse(rf.PointAmount, out d) && d % 100 != 0)
                        {
                            CustomMessageBox.Error(string.Format("确认量必须是100的整数倍，当前输入值：{0}", rf.PointAmount));
                            return;
                        }
                    }
                    else 
                    {
                        if (!IsShowSHE)
                        {
                            if (decimal.TryParse(rf.PointAmount, out d) && d % 100 != 0)
                            {
                                string ErrorTxt = "确认量必须是100的整数倍";
                                CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                                return;
                            }
                        }
                        else
                        {
                            if (decimal.TryParse(rf.PointAmount, out d) && d % 1 != 0)
                            {
                                string ErrorTxt = "确认量必须是1的整数倍";
                                CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                                return;
                            }
                        }
                    }

                    if (!IsShowSHE)
                    {
                        if (d > 10000000 || d < 1)
                        {
                            string ErrorTxt = "确认量输入范围 [1，10000000] ";
                            CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                            return;
                        }
                    }
                    else
                    {
                        if (d > 100000000000 || d < 1)
                        {
                            string ErrorTxt = "确认量输入范围 [1，100000000000] ";
                            CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                            return;
                        }
                    }


                    if (decimal.TryParse(rf.PointPrice, out d))
                    {
                        if (d > 9999 || d < -100)
                        {
                            string ErrorTxt = "价格输入范围 [-100，9999]";
                            CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                            return;
                        }
                    }


                    //if (Amount != "--")
                    //{
                    if (decimal.TryParse(rf.PointAmount, out d))
                    {
                        if (d > decimal.Parse(rf.Amount))
                        {
                            string ErrorTxt = "确认量需要小于等于回价总量";
                            CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                            return;
                        }
                        if (this.ShowLeavesQtyTotal != "--" && (string.IsNullOrEmpty(this.ShowLeavesQtyTotal)))
                        {
                            if (d > decimal.Parse(this.ShowLeavesQtyTotal))
                            {
                                string ErrorTxt = "确认量需要小于等于报价总量";
                                CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                                return;
                            }
                        }
                    }
                    //}

                    if (CommonUtils.ConvertToDecimal(rf.Price) == CommonUtils.ConvertToDecimal(rf.PointPrice))
                    {
                        info1 = "2";
                    }
                    else
                    {
                        info1 = "1";
                    }

                    string dm = info1 == "2" ? "成交意向发送" : "议价意向发送";

                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        PResult result = null;
                        if (info1 == "1")
                        {
                            //1-议价
                            Common.Model.Fix.Message.OrderCancelRequest rs = DRPTools.GetCancelOrder(rf);
                            rs.Info1 = info1;
                            result = InnerHandler.Instance.DRPCancelOrder(rs);
                        }
                        else
                        {
                            //2-成交
                            NewOrderSingle rs = DRPTools.GetNewOrderSingle(rf); //报价类型（1-议价，2-成交，3-拒绝）
                            rs.Info1 = info1;
                            result = InnerHandler.Instance.DRPOrder(rs);
                        }

                        if (result != null && result.Code == "0000")
                        {
                            NeedPrompt = false;
                        }
                        else
                        {
                            NeedPrompt = true;
                            if (result != null && !string.IsNullOrEmpty(result.Msg))
                            {
                                errorMsg = result.Msg;
                            }
                            else
                            {
                                errorMsg = "服务器接收数据失败！";
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (NeedPrompt)
                            {
                                CustomMessageBox.Warning($"定向回价失败！\r\n原因:{errorMsg}", "定向回价");
                                if (result == null)
                                {
                                    return;
                                }

                                if (result.Code != "0000")
                                {
                                    return;
                                }
                            }
                            //CommonUtils.Prompt2Second("定向回价", $"{dm}成功.");
                        });
                    });
                }
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        /// <param name="inparam"></param>
        public void OnRefuseCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                string errorMsg = string.Empty;
                bool NeedPrompt = false;
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        Common.Model.Fix.Message.OrderCancelRequest rs = DRPTools.GetRejectOrder(rf);
                        rs.Info1 = "3"; //报价类型（1-议价，2-成交，3-拒绝）
                        
                        PResult result = InnerHandler.Instance.DRPCancelOrder(rs);
                        if (result != null && result.Code == "0000")
                        {
                            NeedPrompt = false;
                        }
                        else
                        {
                            NeedPrompt = true;
                            if (result != null && !string.IsNullOrEmpty(result.Msg))
                            {
                                errorMsg = result.Msg;
                            }
                            else
                            {
                                errorMsg = "服务器接收数据失败！";
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (NeedPrompt)
                            {
                                CustomMessageBox.Warning($"定向回价失败！\r\n原因:{errorMsg}", "定向回价");

                                if (result == null)
                                {
                                    return;
                                }

                                if (result.Code != "0000")
                                {
                                    return;
                                }
                            }

                            //CommonUtils.Prompt2Second("定向回价", $"拒绝意向发送成功.", _myWindow);
                        });
                    });

                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }


        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="inparam"></param>
        private void OnOpenDrpProcDiagCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                TMessageContent detail = Common.Handler.Drp.DrpHandler.Instance.GetContent(this.QuoteID);
                if (detail != null)
                { 
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "DRPMsgPo", Info1 = "TTbookSystemMessage", Msg = this.Message });
                }
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }


        private void OnCloseSelfCommand()
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    ThreadPoolHelper.InvokeThreadPool((o) =>
            //    {
            //        try
            //        {
            //            TBatchMsgReq batchMsgReq = new TBatchMsgReq();
            //            batchMsgReq.delflag = "1";
            //            batchMsgReq.readflag = "1";
            //            batchMsgReq.id = new List<string>();

            //            batchMsgReq.id.Add(this.id);

            //            PResult result = AdminHandler.Instance.BatchUpdateSysMsg(batchMsgReq);

            //            if (result == null)
            //            {
            //                return;
            //            }

            //            if (result.Code != "0000")
            //            {
            //                return;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            LogHelper.Error(ex);
            //        }
            //    });
            //});
        }

        private void Init()
        {
            GridDatas.Clear();
            IsSetFocus = false;
        }

        /// <summary>
        /// 更新GridData数据
        /// </summary>
        /// <param name="dr"></param>
        public void UpdateGridDataData(ObservableCollection<DRPListModel> dr)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                foreach (DRPListModel dg in dr)
                {
                    DRPTools.UpdateGridData(this.GridDatas, dg);
                }
              
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 自动选择最优价
        /// </summary>
        /// <param name="dr"></param>
        public void SetFirstBestPriceSelected()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                bool isFind = false;
                foreach (DRPListModel dg in this.Mv.View)
                {
                    if (dg.EnabledBtn)
                    {
                        this.SelectDrp = dg;
                        isFind = true;
                        break;
                    }
                }

                if (!isFind && this.GridDatas.Count > 0)
                {
                    if (ShowSide == "1")
                    {
                        this.SelectDrp = this.GridDatas.OrderByDescending(a => a.EnabledBtn).ThenByDescending(o => o.Price).ThenByDescending(k => k.TransactTime).First();
                    }
                    else
                    {
                        this.SelectDrp = this.GridDatas.OrderByDescending(a => a.EnabledBtn).ThenBy(o => o.Price).ThenByDescending(k => k.TransactTime).First();
                    }
                }

                IsSetFocus = false;
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 报价或议价
        /// </summary>
        /// <param name="inparam"></param>
        public void SetClipboard(object inparam)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    string side = rf.Side == "1" ? "TKN" : "GVN";
                    string txt = string.Empty;
                    if (!string.IsNullOrEmpty(rf.RemainSeconds) && rf.RemainSeconds != "--" && rf.RemainSeconds != "0")
                        txt = $"{this.securityid} {side} {rf.Price} {rf.Amount}万 是否OK？({rf.RemainSeconds}s 内回复有效)";
                    else
                        txt = $"{this.securityid} {side} {rf.Price} {rf.Amount}万 是否OK？";


                    ClipboardHelper.CopyToClipboard(txt);
                    CommonUtils.Prompt2Second("提示", "已复制");
                }
            });
        }


        #region 定向报价倒计时

        private void StartTimeTick()
        {
            this.descTimer = new System.Threading.Timer(new TimerCallback(DescTimer_Tick), null, 100, 500);
        }

        /// <summary>
        /// 更新剩余时间
        /// </summary>
        private void DescTimer_Tick(object sender)
        {
            try
            {
                UpdatMyOrdersTimeRemains();
            }
            catch (Exception ex)
            {

            }
        }


        public void UpdatMyOrdersTimeRemains()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (Updatelock)
                {
                    foreach (DRPListModel myOrder in this.GridDatas)
                    {
                        if (!string.IsNullOrEmpty(myOrder.ValidUntilTime))
                        {
                            if (!string.IsNullOrEmpty(myOrder.IocTimeOut) && myOrder.EnabledBtn)
                            {
                                int res = 0;
                                DateTime transactTime = CommonUtils.ConvertToDateTime(myOrder.ValidUntilTime.ToString());
                                TimeSpan diffTime = transactTime - DateTimeEx.GetNow();
                                res = Convert.ToInt32(diffTime.TotalSeconds);
                                myOrder.RemainSeconds = res < 0 ? "0" : res.ToString();
                            }
                            else
                            {
                                myOrder.RemainSeconds = "--";
                                myOrder.EnabledBtn = false;
                            }
                        }
                    }
                }
            });
        }

        #endregion

        #region Dispose
        ~DRPNotificationViewModel()
        {
            this.descTimer = null;
            LogHandler.Info("DRPNotificationViewModel Dispose");
            this.Dispose();
        }
        #endregion
    }
}

using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Inner;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Drp.Utils;
using STC.Common.View.Order;
using STC.Common.ViewModel.Drp.Utils;
using STC.XBond.Plugin.Command;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel.TradeBookHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel.Drp
{
    /// <summary>
    /// PriceMatchView的数据内容
    /// </summary>
    public class DirectionalReturnPriceViewModel : ViewModelBase
    {
        private MsgPo _currentMsg = null;
        private string CurrentMsgType = "2";
        private AsyncThreadQueue<List<TTbookSystemMessage>> _msgTask = null;
        private string CurrentMsgId = string.Empty;
        private object _lockObj = new object();
        private readonly string FIRM_END_TXT = "已匹配到相关价格如下：";
        private readonly string NONFIRM_END_TXT = "已匹配到订单 （{0}笔），请确认firm量";
        private readonly string FIRM = "firm";
        private readonly string NONFIRM = "nonfirm";
        private readonly string ERROR_CHANNEL = "填写渠道数量错误";
        private readonly string ERROR_AMOUNT = "填写确认数量错误";
        private Window _myWindow;
        private bool _isFirm = false;
        private DRPDetail _priceMatchDetail = null;

        #region 界面属性

        private bool isAllChecked;
        public bool IsAllChecked { get { return isAllChecked; } set { isAllChecked = value; this.OnPropertyChanged(); this.OnCheckAll(); } }

        private ObservableCollection<DRPListModel> _gridDatas;

        public ObservableCollection<DRPListModel> GridDatas
        {
            get
            {
                if (_gridDatas == null)
                    _gridDatas = new ObservableCollection<DRPListModel>();

                return _gridDatas;
            }
        }

        private string _contentStart;
        public string ContentStart
        {
            get
            {
                return _contentStart;
            }

            set
            {
                _contentStart = value;

                this.OnPropertyChanged();
            }
        }

        private string _firmType;
        public string FirmType
        {
            get
            {
                return _firmType;
            }

            set
            {
                _firmType = value;

                this.OnPropertyChanged();
            }
        }


        private string _showside;
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


        private string _remark;
        public string Remark
        {
            get
            {
                return _remark;
            }

            set
            {
                _remark = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _cancelVisibility = Visibility.Visible;
        public Visibility CancelVisibility
        {
            get
            {
                return _cancelVisibility;
            }

            set
            {
                _cancelVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _confirmVisibility = Visibility.Collapsed;
        public Visibility ConfirmVisibility
        {
            get
            {
                return _confirmVisibility;
            }

            set
            {
                _confirmVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _remarkVisibility = Visibility.Collapsed;
        public Visibility RemarkVisibility
        {
            get
            {
                return _remarkVisibility;
            }

            set
            {
                _remarkVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private string _totalAmount;
        public string TotalAmount
        {
            get
            {
                return _totalAmount;
            }

            set
            {
                _totalAmount = value;

                this.OnPropertyChanged();
            }
        }

        private string _customAmount;
        public string CustomAmount
        {
            get
            {
                return _customAmount;
            }

            set
            {
                _customAmount = value;

                this.OnPropertyChanged();
            }
        }


        private Visibility _remarkOtherVisibility = Visibility.Collapsed;
        public Visibility RemarkOtherVisibility
        {
            get
            {
                return _remarkOtherVisibility;
            }

            set
            {
                _remarkOtherVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _errorVisibility = Visibility.Collapsed;
        public Visibility ErrorVisibility
        {
            get
            {
                return _errorVisibility;
            }

            set
            {
                _errorVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private string _errorTxt;
        public string ErrorTxt
        {
            get
            {
                return _errorTxt;
            }

            set
            {
                _errorTxt = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _checkAmountErrorVisibility = Visibility.Collapsed;
        public Visibility CheckAmountErrorVisibility
        {
            get
            {
                return _checkAmountErrorVisibility;
            }

            set
            {
                _checkAmountErrorVisibility = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 当点价总量>报价总量时为red， 否则为white
        /// </summary>
        private string _warningColor = "red";

        public string WarningColor
        {
            get
            {
                return _warningColor;
            }
            set
            {
                _warningColor = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 报价总量
        /// </summary>
        private decimal _matchTotal = decimal.Zero;
        public decimal MatchTotal
        {
            get
            {
                return _matchTotal;
            }

            set
            {
                _matchTotal = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _pointTotal = decimal.Zero;
        public decimal PointTotal
        {
            get
            {
                return _pointTotal;
            }
            set
            {
                _pointTotal = value;
                this.OnPropertyChanged();
            }
        }

        private string _outChannelNum;
        public string OutChannelNum
        {
            get
            {
                return _outChannelNum;
            }

            set
            {
                _outChannelNum = value;

                this.OnPropertyChanged();
            }
        }

        private string _randomNum;
        public string RandomNum
        {
            get
            {
                return _randomNum;
            }

            set
            {
                _randomNum = value;

                this.OnPropertyChanged();
            }
        }

        private bool _checkOutChannel;
        public bool CheckOutChannel
        {
            get
            {
                return _checkOutChannel;
            }

            set
            {
                _checkOutChannel = value;

                this.OnPropertyChanged();
            }
        }


        private string _checkOutChannelTag;
        public string CheckOutChannelTag
        {
            get
            {
                return _checkOutChannelTag;
            }

            set
            {
                _checkOutChannelTag = value;

                this.OnPropertyChanged();
            }
        }

        private string _outChannel;
        public string OutChannel
        {
            get
            {
                return _outChannel;
            }

            set
            {
                _outChannel = value;

                this.OnPropertyChanged();
            }
        }


        private bool _checkOut = true;
        public bool CheckOut
        {
            get
            {
                return _checkOut;
            }

            set
            {
                _checkOut = value;

                this.OnPropertyChanged();
            }
        }

        private bool _sendPriceEnable = true;
        public bool SendPriceEnable
        {
            get
            {
                return _sendPriceEnable;
            }

            set
            {
                _sendPriceEnable = value;

                this.OnPropertyChanged();
            }
        }


        private string _matchId;
        private string _matchVenue;
        private List<PriceMatchParam> _priceMatchParams;
        private List<DRPValue> _priceValues;

        public ICommand CloseCommand { get; set; }
        public ICommand AddAmountCommand { get; set; }
        public ICommand DownAmountCommand { get; set; }
        public ICommand ReplyCommand { get; set; }
        public ICommand RefuseCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand BatchRefusedCommand { get; set; }
        public ICommand BatchConfirmCommand { get; set; }

        #endregion

        #region 构造函数
        public DirectionalReturnPriceViewModel()
        {
            ReplyCommand = new DelegateCommand(new Action<object>(OnReplyCommand));
            RefuseCommand = new DelegateCommand(new Action<object>(OnRefuseCommand));
            CancelCommand = new RelayCommand(CancelClick);
            BatchRefusedCommand = new RelayCommand(OnBatchRefusedCommand);
            BatchConfirmCommand = new RelayCommand(OnBatchConfirmCommand);
            CloseCommand = new RelayCommand(CloseClick);
            AddAmountCommand = new DelegateCommand(new Action<object>(OnAddAmountCommand));
            DownAmountCommand = new DelegateCommand(new Action<object>(OnDownAmountCommand));
        }


        /// <summary>
        /// 初始化界面 订阅系统参数消息数据
        /// </summary>
        /// <param name="ownWindow"></param>
        /// <param name="msg"></param>
        public void Init(Window ownWindow, MsgPo msg)
        {
            if (_msgTask == null)
                _msgTask = new AsyncThreadQueue<List<TTbookSystemMessage>>(Process);
            else
                _msgTask.Clear();

            _myWindow = ownWindow;

            InitUI(msg, false);

            AdminHandler.Instance.Init(OnAdminMsg);
        }

        #region 参数消息数据处理
        private void InitUI(MsgPo msg, bool isRealtime)
        {
            _currentMsg = msg;
            int orderNum = 0;
            List<DRPValue> priceValues = null;

            if (msg != null)
            {
                if (!string.IsNullOrEmpty(msg.Detail))
                {
                    decimal amount = 0;
                    _priceMatchDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(msg.Detail);
                    priceValues = _priceMatchDetail.iocOrderDqList;
                    int index = 1;
                    foreach (DRPValue pv in priceValues)
                    {

                        DRPListModel dg = DRPTools.PriceValueToGridData(pv);
                        if (_priceMatchDetail.order != null)
                        {
                            Brush brush = null;
                            //1 BID BUY   2 Ofr SELL
                            if (_priceMatchDetail.order.side.ToUpper().Equals("BUY") || _priceMatchDetail.order.side.Equals("1"))
                            {
                                brush = (Brush)_myWindow.FindResource("ColorBrush.Orange7");
                                dg.Side = "1";
                            }
                            else
                            {
                                dg.Side = "2";
                                brush = (Brush)_myWindow.FindResource("ColorBrush.Blue8");
                            }

                            dg.PriceBrush = brush;

                        }

                        dg.EnabledBtn = CheckVenuePermission(dg.Venue, _priceMatchDetail.order.symbol);

                        dg.Symbol = _priceMatchDetail.order.symbol;

                        if (dg.OrderStatus == "New" || dg.OrderStatus == "PartiallyFilled" || dg.OrderStatus == "Filled")
                        {
                            dg.ForegroundBrush = (Brush)_myWindow.FindResource("DefaultGridForground");
                        }
                        else
                        { 
                            dg.ForegroundBrush = (Brush)_myWindow.FindResource("ColorBrush.Gray14");
                        }
                            
                        dg.Sort = index.ToString();
                        index++;


                        UpdateGridData(dg);

                        //if (!pv.isForbit)
                        //{
                        amount += pv.qty;
                        //}
                    }
                    TotalAmount = _priceMatchDetail.order != null ? _priceMatchDetail.order.unOpenQty.ToString() : string.Empty;//amount.ToString();// string.Format("{0}万", amount.ToString());
                    orderNum = priceValues.Count;

                    ComputeAmount();

                }

                // 修改标题显示内容
                CurrentMsgId = msg.Id;
                if (msg.Content != null)
                {
                    this.ContentStart = _priceMatchDetail.order.symbol;
                    this.TotalAmount = $"{_priceMatchDetail.order.unOpenQty.ToString()}万";
                    if (_priceMatchDetail.order.firmFlag == "1")
                    {
                        this.FirmType = $"(Firm)";    // firm
                    }
                    else
                    {
                        this.FirmType = $"(NonFirm)";  // nonfirm
                    }

                    //1 BID BUY   2 Ofr SELL
                    this.ShowSide = (_priceMatchDetail.order.side.ToUpper().Equals("BUY") || _priceMatchDetail.order.side.Equals("1")) ? "1" : "2";
                    this.ShowPrice = _priceMatchDetail.order.price.ToString();
                }
            }
        }


        /// <summary>
        /// 接收系统参数消息数据
        /// </summary>
        /// <param name="sysMsg"></param>
        private void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            _msgTask.Enqueue(sysMsg);
        }

        /// <summary>
        /// 处理系统参数消息数据
        /// </summary>
        /// <param name="sysMsg"></param>
        private void Process(List<TTbookSystemMessage> sysMsg)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lockObj)
                {
                    if (sysMsg != null && sysMsg.Count > 0 && _currentMsg != null && CurrentMsgId.Equals(sysMsg[0].id))
                    {
                        GridDatas.Clear();
                        foreach (TTbookSystemMessage msg in sysMsg)
                        {
                            if (msg != null)
                            {
                                // 加入删除逻辑
                                string isdel = msg.delflag ?? "0";
                                if (isdel == "1")
                                {
                                    if (msg.id == CurrentMsgId)
                                    {
                                        CloseClick();
                                        break;
                                    }
                                }
                            }

                            if (msg.content != null && msg.type.Equals(CurrentMsgType) && (!string.IsNullOrEmpty(CurrentMsgId) && msg.id.Equals(CurrentMsgId)))
                            {
                                InitUI(ObjectConvert.SystemMsgToShowMsg(msg, _myWindow), true);
                            }
                        }
                    }
                }
            }));
        }

        #endregion


        #region UI
        private void OnCheckAll()
        {
            foreach (DRPListModel gridData in GridDatas)
            {
                if (gridData.EnabledBtn)
                {
                    gridData.IsChecked = IsAllChecked;
                }
            }
        }

       
        public void ChangeAmount(string amount)
        {
            DataGrid dgData = _myWindow.FindName("dgData") as DataGrid;
            if (dgData != null)
            {
                DRPListModel gridData = dgData.SelectedItem as DRPListModel;
                if (gridData != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(amount, out d))
                    {
                        gridData.PointAmount = d.ToString();

                        ComputeAmount();
                    }
                }
            }
        }

        public void ComputeAmount()
        {
            string tipTxt = string.Empty;
            decimal oldTotal = 0;
            decimal pointTotal = 0;
            int index = 0;
            foreach (DRPListModel gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    index++;
                    decimal d = 0;
                    if (decimal.TryParse(gridData.Amount, out d))
                        oldTotal += d;
                    if (decimal.TryParse(gridData.PointAmount, out d))
                        pointTotal += d;
                }
            }

            if (_priceMatchDetail != null && _priceMatchDetail.order != null)
                oldTotal = _priceMatchDetail.order.qty;

            if (pointTotal > oldTotal)
            {
                this.WarningColor = "red";
            }
            else
            {
                this.WarningColor = "white";
            }

            //ErrorTxt = string.Format("报价总量:{0}万,点价总量:{1}万", oldTotal, pointTotal);
            MatchTotal = oldTotal;
            PointTotal = pointTotal;

            CheckAmountErrorVisibility = Visibility.Visible;
            ErrorVisibility = Visibility.Collapsed;

            isAllChecked = index == GridDatas.Count;

            this.OnPropertyChanged("IsAllChecked");


        }

        private void ConfirmClick()
        {
            CancelClick();
        }

        public void Clear()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void CancelClick()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
            _myWindow.Close();
        }

        #endregion


        #region Command Proceduce

        /// <summary>
        /// 取消并关闭窗口
        /// </summary>
        private void CloseClick()
        {
            //if (_isFirm)
            {
                AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
                _msgTask.Dispose();
                _myWindow.Close();

                return;
            }

            //nonfirm的价格匹配 忽略调用TBookSvr的删除接口
            if (_currentMsg.MessageContent != null && !string.IsNullOrEmpty(_currentMsg.MessageContent.contentType) && _currentMsg.MessageContent.contentType.Equals(AdminConsts.PriceMatch)
                && _currentMsg.Content.ToUpper().Contains("NONFIRM"))
            {
                DRPDetail priceMatchDetail = null;
                try
                {
                    priceMatchDetail = MsgHelper.Deserialize<DRPDetail>(_currentMsg.MessageContent.detail);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }

                if (priceMatchDetail != null && priceMatchDetail.order != null && !string.IsNullOrEmpty(priceMatchDetail.order.orderId))
                {
                    NewOrderSingle order = new NewOrderSingle();
                    order.AlgoName = "CROSS";
                    order.StrategyOrderId = priceMatchDetail.order.orderId;

                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            PResult result = TBookHandler.Instance.updateOrder(order);

                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    });
                }
            }
            else
            {

                TBatchMsgReq batchMsgReq = new TBatchMsgReq();
                batchMsgReq.delflag = "1";
                batchMsgReq.readflag = "1";
                batchMsgReq.id = new List<string>();

                batchMsgReq.id.Add(_currentMsg.Id);

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        PResult result = AdminHandler.Instance.BatchUpdateSysMsg(batchMsgReq);

                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                });
            }

            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Dispose();
            _myWindow.Close();
        }


        /// <summary>
        /// 增加报量
        /// </summary>
        public void OnAddAmountCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(rf.PointAmount, out d))
                    {
                        rf.PointAmount = (d + 1000).ToString();

                        ComputeAmount();
                    }
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 减少报量
        /// </summary>
        public void OnDownAmountCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(rf.PointAmount, out d))
                    {
                        if ((d - 1000) >= 0)
                        {
                            rf.PointAmount = (d - 1000).ToString();
                        }

                        ComputeAmount();
                    }
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 回应(成交或议价)
        /// </summary>
        /// <param name="inparam"></param>
        private void OnReplyCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    string errorMsg = string.Empty;
                    bool NeedPrompt = false;
                    string info1 = "1";

                    if (rf.Price == rf.PointPrice && rf.Amount == rf.PointAmount)
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

                        //run = new Run(errorMsg);
                        //txtRun.Add(run);
                        //CustomMessageBox.ShowDialog(
                        //    result != null && result.Code == "0000" ? CustomMessageBox.EnumNotifyType.Info : CustomMessageBox.EnumNotifyType.Error,
                        //    txtRun, null, MyWindow);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (NeedPrompt)
                            {
                                CustomMessageBox.Warning($"定向回价失败！\r\n 原因:{errorMsg}",  "定向回价");
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
        private void OnRefuseCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                string errorMsg = string.Empty;
                bool NeedPrompt = false;
                DRPListModel  rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {

                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        Common.Model.Fix.Message.OrderCancelRequest rs = DRPTools.GetCancelOrder(rf);
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
                                CustomMessageBox.Warning( $"定向回价失败！\r\n 原因:{errorMsg}.", "定向回价");

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
        /// 批量拒绝
        /// </summary>
        private void OnBatchRefusedCommand()
        {
            CustomMessageBox.Warning("开发中");
        }

        /// <summary>
        /// 批量确认
        /// </summary>
        private void OnBatchConfirmCommand()
        {
            CustomMessageBox.Warning("开发中");
            return;

            _matchVenue = null;
            if (_priceMatchParams != null)
                _priceMatchParams.Clear();
            if (_priceValues != null)
                _priceValues.Clear();


            ErrorVisibility = Visibility.Collapsed;
            CheckAmountErrorVisibility = Visibility.Visible;
            ErrorTxt = string.Empty;

            if (!CheckParam())
            {
                ErrorVisibility = Visibility.Visible;
                CheckAmountErrorVisibility = Visibility.Collapsed;
                ErrorTxt = "输入量必须是1000的整数倍";

                return;
            }
            
            if (PointTotal == 0)
            {
                CustomMessageBox.Warning("没有选择报价");
                return;
            }

            ////_priceMatchParams = SendPointOrder();

            ////if (IsChangeOriOrder())
            ////{
            ////    if (PointTotal < MatchTotal)
            ////    {
            ////        // 1.弹框提示
            ////        if(CustomMessageBox.Question("是否修改已存在的订单?", "编辑订单", _myWindow))
            ////        {
            ////            // 2 改单
            ////            EditOrder(_priceMatchDetail.order.orderId, PointTotal);
            ////            return;
            ////        }
            ////    }
            ////    else if (PointTotal >= MatchTotal)
            ////    {
            ////        // 1.弹框提示
            ////        if (CustomMessageBox.Question("是否撤销已存在的订单?", "删除订单",_myWindow))
            ////        {
            ////            // 2.撤单
            ////            DeleteOrder(_priceMatchDetail.order.orderId);
            ////        }
            ////    }
            ////}

            ////NotifyCloseStateDel();

        }

        #endregion

        #endregion

        #region  Proc
        private void AsynUpdateOrder(object order)
        {
            PResult result = TBookHandler.Instance.updateOrder(order as NewOrderSingle);
        }

        private List<PriceMatchParam> SendPointOrder()
        {
            List<PriceMatchParam> priceMatchParams = new List<PriceMatchParam>();
            foreach (DRPListModel gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    PriceMatchParam priceMatchParam = new PriceMatchParam();

                    priceMatchParam.VenueTypeGW = gridData.VenueTypeGW;
                    priceMatchParam.Symbol = gridData.Symbol;
                    priceMatchParam.Side = (gridData.Side == "1") ? "2" : "1";
                    priceMatchParam.Price = gridData.Price;
                    priceMatchParam.CustomFirmAmount = gridData.PointAmount;
                    //priceMatchParam.NetPrice = GetNetPrice(drv);
                    //priceMatchParam.Terminal = GetSource(drv);
                    priceMatchParam.WholeFlag = gridData.WholeFlag;
                    priceMatchParam.IsFirm = (gridData.FirmType.Equals("1") || gridData.FirmType == "firm");
                    priceMatchParam.Source = ComboSourceUtil.Instance.FindEnum(gridData.Source, "venues_cn");
                    priceMatchParam.Id = gridData.Id;
                    //priceMatchParam.TrsactTime = gridData.tr
                    //priceMatchParam.Text = GetOrderTip(drv);
                    priceMatchParam.TrsactTime = gridData.TransactTime;
                    priceMatchParam.QuoteType = gridData.QuoteType;
                    priceMatchParam.DeliveryType = gridData.DeliveryType;
                    priceMatchParam.ClearingMethod = gridData.ClearingMethod;
                    priceMatchParam.SettlType = ComboSourceUtil.Instance.FindEnum(gridData.Settle, "settletype_revert");
                    //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    //    priceMatchParam.OperationType = "3";
                    //else
                    //    priceMatchParam.QuoteType = GetQuoteType(drv);
                    if (_priceMatchDetail != null && _priceMatchDetail.order != null)
                    {
                        //priceMatchParam.Info4 = _priceMatchDetail.order.Info4;//用于意向单的Intentionid
                        priceMatchParam.AccountID = _priceMatchDetail.order.userID;
                    }

                    priceMatchParams.Add(priceMatchParam);
                }
            }

            return priceMatchParams;
        }

        private bool CheckParam()
        {
            bool result = true;
            foreach (DRPListModel gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    if (!string.IsNullOrEmpty(gridData.PointAmount))
                    {
                        decimal d = 0;
                        if (decimal.TryParse(gridData.PointAmount, out d) && d % 1000 != 0)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private bool Check()
        {
            bool result = true;
            ErrorVisibility = Visibility.Collapsed;
            decimal custom = 0;
            if (!_isFirm)
            {
                decimal total = 0;
                if (decimal.TryParse(_totalAmount, out total) && decimal.TryParse(_customAmount, out custom) && custom < 0)//total < custom)
                {
                    ErrorVisibility = Visibility.Visible;
                    ErrorTxt = ERROR_AMOUNT;
                    result = false;
                }

                if (decimal.TryParse(_customAmount, out total) && (total % 100) > 0)
                {
                    ErrorVisibility = Visibility.Visible;
                    ErrorTxt = string.Format("量必须是100的整数倍，当前输入值：{0}", _customAmount);
                    result = false;
                }
            }
            else
            {
                decimal tem = 0;
                if (decimal.TryParse(RandomNum, out custom) && decimal.TryParse(OutChannelNum, out tem) && custom > tem)
                {
                    ErrorVisibility = Visibility.Visible;
                    ErrorTxt = ERROR_CHANNEL;
                    result = false;
                }
            }

            return result;
        }

        private void UpdateGridData(DRPListModel gd)
        {
            bool isContain = false;
            List<DRPListModel> selectData = GridDatas.Where(delegate (DRPListModel gridData)
            {
                if (gridData.Id.Equals(gd.Id))
                    return true;
                else
                    return false;
            }).ToList();

            isContain = selectData.Count > 0;

            if (!isContain)
                GridDatas.Add(gd);
            else
                DRPTools.UpdatePriceValue(selectData[0], gd);
        }


        private string SetGoodsShortname(string goodcode)
        {
            string goodsType = string.Empty;
            List<BondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(goodcode, 1);
            if (bondsInfoPos != null && bondsInfoPos.Count > 0)
                goodsType = bondsInfoPos[0].SECURITY_TYPE;

            return goodsType;
        }

       

        private bool CheckVenuePermission(string venue, string security)
        {
            //if (!string.IsNullOrEmpty(venue) && ((venue.ToUpper() == "MANUAL") || (venue.ToUpper() == "人工") || (venue.ToUpper() == "内部")))
            //    return true;

            string goodsType = CommonUtils.SetGoodsShortname(security);

            List<Venue> venues = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, string.Empty);

            bool result = false;
            foreach (Venue ve in venues)
            {
                if (ve.venue.ToUpper() == venue.ToUpper())
                {
                    result = true;

                    break;
                }
            }

            return result;
        }

        //to do ...
        /// <summary>
        /// 处理表格双击事件， 弹出点价界面
        /// </summary>
        /// <param name="selectItem"></param>
        public void OnDoubleClick(object selectItem)
        {
            return;
            DRPListModel selectedGridData = selectItem as DRPListModel;

            if (selectedGridData == null)
                return;

            _matchVenue = selectedGridData.Venue;
            _matchId = selectedGridData.Id;

            if (_priceMatchParams != null)
                _priceMatchParams.Clear();
            if (_priceValues != null)
                _priceValues.Clear();

            if (_priceMatchDetail != null && _priceMatchDetail.iocOrderDqList != null)
            {
                if (!CheckVenuePermission(_matchVenue, _priceMatchDetail.order.symbol))
                    return;

                _priceValues = _priceMatchDetail.iocOrderDqList.Where(delegate (DRPValue pv)
                {
                    if (pv.orderId.Equals(_matchId))
                        return true;
                    else
                        return false;
                }).ToList();
            }

            if (IsChangeOriOrder())
            {
                decimal currentPointAmount = selectedGridData.PointAmount.ConvertToDecimal();
                if (currentPointAmount < MatchTotal)
                {
                    // 1.弹框提示
                    if (CustomMessageBox.Question("是否修改已存在的订单?", "编辑订单", _myWindow))
                    {
                        // 2 改单
                        EditOrder(_priceMatchDetail.order.orderId, currentPointAmount);
                        return;
                    }
                }
                else if (currentPointAmount >= MatchTotal)
                {
                    // 1.弹框提示
                    if (CustomMessageBox.Question("是否撤销已存在的订单?", "删除订单", _myWindow))
                    {
                        // 2.撤单
                        DeleteOrder(_priceMatchDetail.order.orderId);
                    }
                }
            }

            NotifyCloseStateDel();
        }


        public void CheckBoxAllValueChange()
        {
            IsAllChecked = !isAllChecked;
            OnCheckAll();

            ComputeAmount();
        }


        #endregion


        #region 废弃代码
        private bool IsChangeOriOrder()
        {
            UserSetting us = SystemParamHandler.Instance.GetUserSetting();
            if (us != null)
            {
                return us.inf3 == "0" ? false : true;
            }
            return true;
        }

        private void DeleteOrder(string strategyOrderID)
        {
            Common.Model.TBook.OrderCancelRequest cancelPo = new Common.Model.TBook.OrderCancelRequest();

            DataRow dr = MyOrderProcesser.Processer.GetOrderById(strategyOrderID);
            if (dr != null)
            {
                cancelPo.AlgoName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            }

            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
            cancelPo.OrderID = strategyOrderID;

            ThreadPoolHelper.InvokeThreadPool((o) => {
                TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        //this.SelectedItem_MyOrders[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                    }
                    else
                    {
                        CommonUtils.Prompt2Second("删除订单", $"删除订单失败, 原因:{reply.Msg}.");
                    }
                }
            });
        }

        private void NotifyCloseStateDel()
        {
            if (string.IsNullOrEmpty(_matchId))
            {
                QuickOrderHandler.Instance.BatchQuickOrder(_priceMatchParams);
                CloseClick();
            }
            else
            {
                CloseClick();

                if (_priceValues != null && _priceValues.Count > 0)
                {
                    DRPValue pv = _priceValues[0];

                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPointPriceWinKey(pv.orderId)))
                    {

                        SendPointPriceView sendPointPriceView =
                        new SendPointPriceView(_priceMatchDetail.order.symbol, !_priceMatchDetail.order.symbol.ToUpper().Equals("BUY"), pv.price.ToString(), pv.qty.ToString(),
                            pv.settlDate, pv.netPrice.ToString(), pv.venue, pv.wholeflag, pv.firmFlag == "1" ? true : false, pv.orderId, pv.transactTime, null , null, null, "", "", pv.venueGW, null, ""); ;


                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointPriceWinKey(pv.orderId), sendPointPriceView);

                    }
                }
            }
        }

        private void EditOrder(string strategyOrderID, decimal pointAmount)
        {
            if (string.IsNullOrEmpty(strategyOrderID))
            {
                return;
            }

            DataRow myorderDr = MyOrderProcesser.Processer.GetOrderById(strategyOrderID);
            if (myorderDr == null)
            {
                return;
            }

            if (WindowManager.Instance.IsExist($"EditOrder-{strategyOrderID}"))
            {
                Window editWin = WindowManager.Instance._UniqueKeyMapWindowDic[$"EditOrder-{strategyOrderID}"];
                if (editWin != null && editWin is SendPriceViewLite)
                {
                    SendPriceViewLite sendPriceViewLite = editWin as SendPriceViewLite;
                    sendPriceViewLite.Closed -= SendPriceViewLite_Closed;
                    sendPriceViewLite.Closed += SendPriceViewLite_Closed;
                }
                return;
            }

            Common.Model.Fix.Message.NewOrderSingle singleOrder = GetNewOrderSingle(myorderDr);
            PriceMatchParam priceMatchParam = new PriceMatchParam();
            

            priceMatchParam.CustomFirmAmount = (singleOrder.OrderQty.ConvertToDecimal() - pointAmount).ToString(); // OrderQty 订单量
            priceMatchParam.OrderId = singleOrder.StrategyOrderId;// 订单号
            priceMatchParam.RandomNum = singleOrder.VenuesRdmNum; // 渠道类型默认，填写随机几家渠道
            priceMatchParam.Side = singleOrder.Side; // 方向
            priceMatchParam.Symbol = singleOrder.SecurityID; // 代码
            priceMatchParam.SymbolName = singleOrder.Symbol; // 名称
            priceMatchParam.WholeFlag = singleOrder.WholeFlag; // 整量标志
            priceMatchParam.NetPrice = singleOrder.NetPrice; // 净价
            priceMatchParam.Price = singleOrder.Price; // 收益率
            priceMatchParam.SettlType = singleOrder.SettlType; // 清算速度
            priceMatchParam.NonfirmTime = singleOrder.FirmToNonTime; // nonfirmTime;
            priceMatchParam.CrossVenues = singleOrder.CrossVenues;// 撮合渠道
            priceMatchParam.VenuesSelectF = singleOrder.VenuesSelectF;//渠道类型  不填写1,默认2，指定3 
            priceMatchParam.SelfSelectVenues = singleOrder.SelfSelectVenues;//指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC

            priceMatchParam.InstitutionID = singleOrder.InstitutionID; //客盘机构id
            priceMatchParam.InstitutionName = singleOrder.InstitutionName;//客盘机构名字
            priceMatchParam.TraderID = singleOrder.TraderID; // 客盘交易员id
            priceMatchParam.TraderName = singleOrder.TraderName;//客盘交易员名字
            priceMatchParam.Terminal = singleOrder.Terminal;
            priceMatchParam.Info1 = singleOrder.Info1;
            priceMatchParam.Info3 = singleOrder.Info3;//用于IDB单子
            priceMatchParam.Info4 = singleOrder.Info4;//用于意向单的Intentionid
            priceMatchParam.AccountID = singleOrder.AccountID;


            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.OldOrder = singleOrder;

            SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
            // bid;
            if (singleOrder.Side == "1")
            {
                //sendPriceView.OfrBtn.IsEnabled = false;
                sendPriceView.SetButtonEnable("OfrBtn", false);
            }
            // ofr
            else if (singleOrder.Side == "2")
            {
                // sendPriceView.BidBtn.IsEnabled = false;
                sendPriceView.SetButtonEnable("BidBtn", false);
            }

            if (singleOrder.FirmFlag == "1")
            {
                //sendPriceView.AmountTxt.IsEnabled = false;
                //sendPriceView.btnOne.IsEnabled = sendPriceView.btnThree.IsEnabled = sendPriceView.btnFive.IsEnabled = false;
            }

            sendPriceView.SetButtonEnable("Search_TextBox", false);
            sendPriceView.SetButtonEnable("btnXBond", false);

            //sendPriceView.datePicker.IsEnabled = false;
            //sendPriceView.cbSettle.IsEnabled = false;
            //sendPriceView.lbOne.IsEnabled = sendPriceView.lbOther.IsEnabled = false;
            //sendPriceView.lbSpotOne.IsEnabled = sendPriceView.lbSpotOther.IsEnabled = false;

            //sendPriceView.lbAddOneNotLimit.IsEnabled = sendPriceView.lbAddOneToday.IsEnabled = sendPriceView.lbAddOneTomorrow.IsEnabled = false;

            //sendPriceView.Show();
            sendPriceView.Closed -= SendPriceViewLite_Closed;
            sendPriceView.Closed += SendPriceViewLite_Closed;
            WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{singleOrder.StrategyOrderId}", sendPriceView);
        }

        private void SendPriceViewLite_Closed(object sender, EventArgs e)
        {
            NotifyCloseStateDel();
        }

        private NewOrderSingle GetNewOrderSingle(DataRow dr)
        {
            string strategyCodeID = CommonUtils.ConvertToString(dr[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(dr[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            string qty = CommonUtils.ConvertToString(dr[MyOrderProcesser.QTY_ORDER_FIELD]);
            string exeqty = CommonUtils.ConvertToString(dr[MyOrderProcesser.EXECQTY_ORDER_FIELD]);
            string price = CommonUtils.ConvertToString(dr[MyOrderProcesser.PRICE_ORDER_FIELD]);
            string netPrice = CommonUtils.ConvertToString(dr[MyOrderProcesser.NETPRICE_ORDER_FIELD]);
            string settlType = CommonUtils.ConvertToString(dr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            string side = CommonUtils.ConvertToString(dr[MyOrderProcesser.SIDE_ORDER_FIELD]);
            string wholeflag = CommonUtils.ConvertToString(dr[MyOrderProcesser.WHOLE_FLAG_FIELD]);
            string firmFlag = CommonUtils.ConvertToString(dr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            string crossVenues = CommonUtils.ConvertToString(dr[MyOrderProcesser.CROSS_VENUES_FIELD]);
            string firmToNonTime = CommonUtils.ConvertToString(dr[MyOrderProcesser.NONFIRM_TIME_FIELD]);
            string randomNum = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            string venuesSelectF = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENUES_SELECT_F_FIELD]);
            string selfSelectVenues = CommonUtils.ConvertToString(dr[MyOrderProcesser.SELF_SELECT_VENUES_FIELD]);


            string venues = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENEUES_ORDER_FIELD]);
            string venueTypeGw = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENUETYPEGW_FIELD]);
            string algoName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            string quantID = CommonUtils.ConvertToString(dr[MyOrderProcesser.QUANTID_FIELD]);
            string timeInForce = CommonUtils.ConvertToString(dr[MyOrderProcesser.TIMEINFORCE_FIELD]);
            string ordStatus = CommonUtils.ConvertToString(dr[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD]);
            string bizType = CommonUtils.ConvertToString(dr[MyOrderProcesser.BIZTYPE_FIELD]);
            string bizTypeID = CommonUtils.ConvertToString(dr[MyOrderProcesser.BIZTYPEID_FIELD]);
            string quoteID = CommonUtils.ConvertToString(dr[MyOrderProcesser.QUOTEID_FIELD]);
            string roleID = CommonUtils.ConvertToString(dr[MyOrderProcesser.ROLEID_FIELD]);
            string roleName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ROLENAME_FIELD]);
            string symbol = CommonUtils.ConvertToString(dr[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
            string userId = CommonUtils.ConvertToString(dr[MyOrderProcesser.USERID_FIELD]);
            string institutionID = CommonUtils.ConvertToString(dr[MyOrderProcesser.INSTITUTIONID_FIELD]);
            string institutionName = CommonUtils.ConvertToString(dr[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
            string traderID = CommonUtils.ConvertToString(dr[MyOrderProcesser.TRADERID_FIELD]);
            string traderName = CommonUtils.ConvertToString(dr[MyOrderProcesser.TRADERNAME_FIELD]);
            string showTrader = CommonUtils.ConvertToString(dr[MyOrderProcesser.SHOW_TRADER_FIELD]);
            string ternimal = CommonUtils.ConvertToString(dr[MyOrderProcesser.TERMINAL_FIELD]);
            string info1 = CommonUtils.ConvertToString(dr[MyOrderProcesser.INFO1_FIELD]);
            string info3 = CommonUtils.ConvertToString(dr[MyOrderProcesser.INFO3_FIELD]);
            string validUntiltime = CommonUtils.ConvertToString(dr[MyOrderProcesser.VALIDUNTILTIME_FIELD]);
            string intentionid = CommonUtils.ConvertToString(dr[MyOrderProcesser.INTENTIONID_FIELD]);

            string marketindicator = CommonUtils.ConvertToString(dr[MyOrderProcesser.MARKETINDICATOR_FIELD]);
            string accountid = CommonUtils.ConvertToString(dr[MyOrderProcesser.AccountID_Field]);
            string accountName = CommonUtils.ConvertToString(dr[MyOrderProcesser.AccountName_Field]);

            Common.Model.Fix.Message.NewOrderSingle sendPricePo = new Common.Model.Fix.Message.NewOrderSingle();

            // BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            sendPricePo.SecurityType = CommonUtils.ConvertToString(dr[MyOrderProcesser.SECURITY_TYPE]);

            sendPricePo.BizType = bizType;
            sendPricePo.BizTypeID = bizTypeID;
            sendPricePo.QuoteID = quoteID;
            sendPricePo.RoleID = roleID;
            sendPricePo.RoleName = roleName;
            sendPricePo.Symbol = symbol;
            sendPricePo.UserID = userId;
            sendPricePo.OrdStatus = ordStatus;
            sendPricePo.StrategyOrderId = strategyCodeID;

            sendPricePo.AlgoName = algoName;
            sendPricePo.VenueTypeGW = venueTypeGw;
            sendPricePo.Venues = venues;
            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.MarketIndicator = "4";
            sendPricePo.OrdType = "2";
            sendPricePo.OrderQty = (CommonUtils.ConvertToDecimal(qty) - CommonUtils.ConvertToDecimal(exeqty)).ToString();
            sendPricePo.Price = price;
            sendPricePo.NetPrice = netPrice;
            sendPricePo.SecurityID = securityID;

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = settlType;

            sendPricePo.Side = side;

            sendPricePo.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = timeInForce;
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = firmFlag;

            sendPricePo.CrossVenues = crossVenues;

            sendPricePo.WholeFlag = wholeflag;

            sendPricePo.QuantID = quantID;

            sendPricePo.FirmToNonTime = firmToNonTime;

            sendPricePo.VenuesRdmNum = randomNum;

            sendPricePo.VenuesSelectF = venuesSelectF;

            sendPricePo.SelfSelectVenues = selfSelectVenues;

            sendPricePo.InstitutionID = institutionID; //客盘机构id
            sendPricePo.InstitutionName = institutionName;//客盘机构名字
            sendPricePo.TraderID = traderID; // 客盘交易员id
            sendPricePo.TraderName = traderName;//客盘交易员名字
            sendPricePo.ShowTrader = showTrader;// 是否显示经济商

            //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");
            sendPricePo.Terminal = ternimal;
            sendPricePo.Info1 = info1;
            sendPricePo.Info3 = info3;
            sendPricePo.ValidUntilTime = validUntiltime;
            sendPricePo.Info4 = intentionid;
            sendPricePo.MarketIndicator = marketindicator;
            sendPricePo.AccountID = accountid;
            sendPricePo.AccountName = accountName;

            return sendPricePo;
        }

        #endregion
    }
}

using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Order;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.View.Order;
using STC.XBond.Plugin.ViewModel;
using STC.XBond.Plugin.ViewModel.TradeBookHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// PriceMatchView的数据内容
    /// </summary>
    public class PriceMatchViewModel : ViewModelBase
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
        private PriceMatchDetail _priceMatchDetail = null;

        #region 界面属性

        private bool isAllChecked;
        public bool IsAllChecked { get { return isAllChecked; } set { isAllChecked = value;  this.OnPropertyChanged(); this.OnCheckAll(); } }

        private ObservableCollection<GridData> _gridDatas;

        public ObservableCollection<GridData> GridDatas
        {
            get
            {
                if (_gridDatas == null)
                    _gridDatas = new ObservableCollection<GridData>();

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

        private string _contentEnd;
        public string ContentEnd
        {
            get
            {
                return _contentEnd;
            }

            set
            {
                _contentEnd = value;

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

        private int _rowHeight = 0;
        public int RowHeight
        {
            get
            {
                return _rowHeight;
            }

            set
            {
                _rowHeight = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _firmBorder = Visibility.Collapsed;
        public Visibility FirmBorder
        {
            get
            {
                return _firmBorder;
            }

            set
            {
                _firmBorder = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _contentEndVisibility = Visibility.Collapsed;
        public Visibility ContentEndVisibility
        {
            get
            {
                return _contentEndVisibility;
            }

            set
            {
                _contentEndVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _nonFirmBorder = Visibility.Collapsed;
        public Visibility NonFirmBorder
        {
            get
            {
                return _nonFirmBorder;
            }

            set
            {
                _nonFirmBorder = value;
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
        private List<PriceValue> _priceValues;
        #endregion

        #region 命令事件

        public ICommand CancelCommand { get { return new RelayCommand(CancelClick); } }
        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }
        public ICommand OkCommand { get { return new RelayCommand(OKClick); } }
        public ICommand ConfirmCommand { get { return new RelayCommand(ConfirmClick); } }

        public ICommand AddCommand { get { return new RelayCommand(AddClick); } }
        public ICommand DownCommand { get { return new RelayCommand(DownClick); } }

        private void OnCheckAll()
        {
            foreach (GridData gridData in GridDatas)
            {
                if (gridData.EnabledBtn)
                {
                    gridData.IsChecked = IsAllChecked;
                }
            }
        }

        public void AddClick()
        {
            DataGrid dgData = _myWindow.FindName("dgData") as DataGrid;
            if (dgData != null)
            {
                GridData gridData = dgData.SelectedItem as GridData;
                if (gridData != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(gridData.PointAmount, out d))
                    {
                        gridData.PointAmount = (d + 1000).ToString();

                        ComputeAmount();
                    }
                }
            }
        }

        public void ChangeAmount(string amount)
        {
            DataGrid dgData = _myWindow.FindName("dgData") as DataGrid;
            if (dgData != null)
            {
                GridData gridData = dgData.SelectedItem as GridData;
                if (gridData != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(amount, out d))
                    {
                        gridData.PointAmount = amount;

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
            foreach (GridData gridData in GridDatas)
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
                oldTotal = _priceMatchDetail.order.Qty;
            
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

        public void DownClick()
        {
            DataGrid dgData = _myWindow.FindName("dgData") as DataGrid;
            if (dgData != null)
            {
                GridData gridData = dgData.SelectedItem as GridData;
                if (gridData != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(gridData.PointAmount, out d) && (d - 1000) > 0)
                    {
                        gridData.PointAmount = (d - 1000).ToString();

                        ComputeAmount();
                    }
                }
            }
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

        private void CancelClick()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
            _myWindow.Close();
        }

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
                PriceMatchDetail priceMatchDetail = null;
                try
                {
                    priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(_currentMsg.MessageContent.detail);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }

                if (priceMatchDetail != null && priceMatchDetail.order != null && !string.IsNullOrEmpty(priceMatchDetail.order.OrderId))
                {
                    NewOrderSingle order = new NewOrderSingle();
                    order.AlgoName = "CROSS";
                    order.StrategyOrderId = priceMatchDetail.order.OrderId;

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

        private List<PriceMatchParam> SendPointOrder()
        {
            List<PriceMatchParam> priceMatchParams = new List<PriceMatchParam>();
            foreach (GridData gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    PriceMatchParam priceMatchParam = new PriceMatchParam();

                    priceMatchParam.VenueTypeGW = gridData.VenueTypeGW;
                    priceMatchParam.Symbol = gridData.Symbol;
                    priceMatchParam.Side = (gridData.Side=="1")?"2":"1";
                    priceMatchParam.Price = gridData.Price;
                    priceMatchParam.CustomFirmAmount = gridData.PointAmount;
                    //priceMatchParam.NetPrice = GetNetPrice(drv);
                    //priceMatchParam.Terminal = GetSource(drv);
                    priceMatchParam.WholeFlag = gridData.WholeFlag;
                    priceMatchParam.IsFirm = (gridData.FirmType.Equals("1") || gridData.FirmType == "firm");
                    priceMatchParam.Source = gridData.Venue;//ComboSourceUtil.Instance.FindEnum(gridData.Source, "venues_cn");
                    priceMatchParam.Id = gridData.Id;
                    //priceMatchParam.TrsactTime = gridData.tr
                    //priceMatchParam.Text = GetOrderTip(drv);
                    priceMatchParam.TrsactTime = gridData.TrsactTime;
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
                        priceMatchParam.Info4 = _priceMatchDetail.order.Info4;//用于意向单的Intentionid
                        priceMatchParam.AccountID = _priceMatchDetail.order.AccountID;
                    }

                    priceMatchParams.Add(priceMatchParam);
                }
            }

            return priceMatchParams;
        }

        private int CheckParam()
        {
            int result = 0;
            foreach (GridData gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    if (!string.IsNullOrEmpty(gridData.PointAmount))
                    {
                        decimal d = 0;
                        if (decimal.TryParse(gridData.PointAmount, out d))
                        {
                            if (d % 100 != 0)
                            {
                                result = 1;
                                break;
                            }
                        }
                        else
                        {
                            result = 2;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private void OKClick()
        {
            _matchId = null;
            _matchVenue = null;
            if (_priceMatchParams != null)
                _priceMatchParams.Clear();
            if (_priceValues != null)
                _priceValues.Clear();


            ErrorVisibility = Visibility.Collapsed;
            CheckAmountErrorVisibility = Visibility.Visible;
            ErrorTxt = string.Empty;

            int chechQtyResult = CheckParam();
            if (chechQtyResult == 1)
            {
                ErrorVisibility = Visibility.Visible;
                CheckAmountErrorVisibility = Visibility.Collapsed;
                ErrorTxt = "输入量必须是100的整数倍";

                return;
            }
            else if (chechQtyResult == 2)
            {
                ErrorVisibility = Visibility.Visible;
                CheckAmountErrorVisibility = Visibility.Collapsed;
                ErrorTxt = "输入量非数字";

                return;
            }
            
            if (PointTotal == 0)
            {
                Common.View.Component.CustomMessageBox.Warning("没有选择报价");
                return;
            }

            _priceMatchParams = SendPointOrder();

            if (IsChangeOriOrder())
            {
                if (PointTotal < MatchTotal)
                {
                    // 1.弹框提示
                    if(Common.View.Component.CustomMessageBox.Question("是否修改已存在的订单?", "编辑订单", _myWindow))
                    {
                        // 2 改单
                        EditOrder(_priceMatchDetail.order.OrderId, PointTotal);
                        return;
                    }
                }
                else if (PointTotal >= MatchTotal)
                {
                    // 1.弹框提示
                    if (Common.View.Component.CustomMessageBox.Question("是否撤销已存在的订单?", "删除订单",_myWindow))
                    {
                        // 2.撤单
                        DeleteOrder(_priceMatchDetail.order.OrderId);
                    }
                }
            }

            NotifyCloseStateDel();

           /* QuickOrderHandler.Instance.BatchQuickOrder(priceMatchParams);


            //if (!Check())
            //    return;

            //PriceMatchDetail priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(_currentMsg.Detail);

            //if (_isFirm)
            //{
            //    UpdateOrder(priceMatchDetail);

            //    //this.CloseClick();
            //}
            //else
            //{
            //    UpdatePrice(priceMatchDetail);

            //    //CancelClick();
            //}
            CancelClick();
            //this.CloseClick();*/
        }

        private void UpdatePrice(PriceMatchDetail priceMatchDetail)
        {
            PriceMatchParam priceMatchParam = new PriceMatchParam();
            priceMatchParam.CustomFirmAmount = CustomAmount;
            priceMatchParam.IsFirm = _isFirm;
            priceMatchParam.OrderId = priceMatchDetail.order.OrderId;
            //priceMatchParam.RandomNum = RandomNum;
            priceMatchParam.Side = priceMatchDetail.order.Side;
            priceMatchParam.Symbol = priceMatchDetail.order.Symbol;
            priceMatchParam.SymbolName = priceMatchDetail.order.SymbolName;
            priceMatchParam.WholeFlag = priceMatchDetail.order.WholeFlag;
            priceMatchParam.NetPrice = priceMatchDetail.order.NetPrice.ToString();
            priceMatchParam.Price = priceMatchDetail.order.Price.ToString();
            priceMatchParam.SettlType = priceMatchDetail.order.SettlType;
            priceMatchParam.OperationType = "2";
            priceMatchParam.Info3 = priceMatchDetail.order != null ? priceMatchDetail.order.Info3 : null;

            if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchSendPriceWinKey(CurrentMsgId)))
            {
                SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPriceMatchSendPriceWinKey(CurrentMsgId), sendPriceView);
                //sendPriceView.Show();
            }
        }

        private void UpdateOrder(PriceMatchDetail priceMatchDetail)
        {
            NewOrderSingle order = new NewOrderSingle();
            order.AlgoName = "CROSS";
            order.StrategyOrderId = priceMatchDetail.order.OrderId;
            string matchVenues = string.Empty;
            foreach (PriceValue pv in priceMatchDetail.Prices)
            {
                if (!pv.isForbit)
                {
                    string venueGW = pv.VenueGW;
                    string tem = string.Format("{0}-{1},", venueGW.ToUpper().Equals("CDH") ? "INNER" : venueGW, pv.Venue);

                    if (!string.IsNullOrEmpty(CheckOutChannelTag))
                    {
                        if (CheckOut)
                        {
                            if (!CheckOutChannelTag.Contains(pv.Venue) && !matchVenues.Contains(tem))//过滤默认渠道
                                matchVenues += tem;
                        }
                        else
                        {
                            if (!matchVenues.Contains(tem))//过滤重复渠道
                                matchVenues += tem;
                        }
                    }
                    else
                    {
                        matchVenues += !matchVenues.Contains(tem) ? tem : string.Empty;
                    }
                }
            }
            if (!string.IsNullOrEmpty(matchVenues))
                order.SelfSelectVenues = matchVenues.Substring(0, matchVenues.Length - 1);
            else
                order.SelfSelectVenues = string.Empty;
            
            order.VenuesSelectF = CheckOut?"2":"4";
            order.VenuesRdmNum = string.IsNullOrEmpty(order.SelfSelectVenues)?string.Empty:RandomNum;
            order.Info3 = priceMatchDetail.order != null ? priceMatchDetail.order.Info3 : null;
            order.Info5 = string.Format(CheckOut ? "随机{0}家渠道+默认渠道": "随机{0}家渠道", RandomNum);

            ThreadPoolHelper.InvokeThreadPool(AsynUpdateOrder, order);
        }

        private void AsynUpdateOrder(object order)
        {
            PResult result = TBookHandler.Instance.updateOrder(order as NewOrderSingle);
        }

        #endregion

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

        private void UpdateGridData(GridData gd)
        {
            bool isContain = false;
            List<GridData> selectData = GridDatas.Where(delegate (GridData gridData)
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
                ObjectConvert.UpdatePriceValue(selectData[0], gd);
        }

        private void InitUI(MsgPo msg, bool isRealtime)
        {
            _currentMsg = msg;
            int orderNum = 0;
            List<PriceValue> priceValues = null;
            PriceMatchDetail priceMatchDetail = null;
            if (msg != null)
            {
                if (!string.IsNullOrEmpty(msg.Detail))
                {
                    decimal amount = 0;
                    _priceMatchDetail = priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(msg.Detail);
                    priceValues = priceMatchDetail.Prices;
                    int index = 1;
                    foreach (PriceValue pv in priceValues)
                    {

                        GridData dg = ObjectConvert.PriceValueToGridData(pv);

                        if (!CommonUtils.IsBrokerSecurity(_priceMatchDetail.order.Symbol))
                        {
                            dg.Settle = dg.DealSettle = "--";
                        }

                        if (priceMatchDetail.order != null)
                        {
                            Brush brush = null;
                            if (priceMatchDetail.order.Side.ToUpper().Equals("SELL") || priceMatchDetail.order.Side.Equals("2"))
                            {
                                brush = (Brush)_myWindow.FindResource("ColorBrush.Orange7");
                                dg.Side = "2";
                            }
                            else
                            {
                                dg.Side = "1";
                                brush = (Brush)_myWindow.FindResource("ColorBrush.Blue8");
                            }
                            dg.PriceBrush = brush;
                            
                        }

                        dg.EnabledBtn = CheckVenuePermission(dg.Venue, _priceMatchDetail.order.Symbol);

                        dg.Symbol = _priceMatchDetail.order.Symbol;

                        dg.Sort = index.ToString();
                        index++;

                        UpdateGridData(dg);

                        if (!pv.isForbit)
                        {
                            amount += pv.Qty;
                        }
                    }
                    TotalAmount = priceMatchDetail.order != null ? priceMatchDetail.order.UnOpenQty.ToString() : string.Empty;//amount.ToString();// string.Format("{0}万", amount.ToString());
                    orderNum = priceValues.Count;

                    ComputeAmount();

                }
                CurrentMsgId = msg.Id;
                string[] contentArr = null;
                if (msg.Content != null)
                {
                    if (msg.Content.ToUpper().Contains("NONFIRM"))
                    {
                        FirmBorder = Visibility.Collapsed;
                        NonFirmBorder = Visibility.Visible;

                        FirmType = NONFIRM;
                        ContentEnd = string.Format(NONFIRM_END_TXT, orderNum);
                        ContentEndVisibility = Visibility.Visible;
                        contentArr = msg.Content.Split(new string[] { NONFIRM }, StringSplitOptions.RemoveEmptyEntries);

                    }
                    else
                    {
                        FirmBorder = Visibility.Visible;
                        NonFirmBorder = Visibility.Collapsed;

                        ContentEnd = FIRM_END_TXT;
                        ContentEndVisibility = Visibility.Collapsed;

                        FirmType = FIRM;
                        _isFirm = true;

                        //Button btn = this._myWindow.FindName("btnCancel") as Button;
                        //Grid.SetColumnSpan(btn, 2);

                        ConfirmVisibility = Visibility.Collapsed;
                        //Button btnSendPrice = this._myWindow.FindName("btnSendPrice") as Button;
                        //btnSendPrice.Visibility = Visibility.Collapsed;
                        //CancelVisibility = Visibility.Collapsed;

                        contentArr = msg.Content.Split(new string[] { FIRM }, StringSplitOptions.RemoveEmptyEntries);

                        int outChannelCount = 0;
                        StringBuilder sbName = new StringBuilder();
                        if (priceValues != null)
                        {
                            foreach (PriceValue pv in priceValues)
                            {
                                string allChanel = sbName.ToString();
                                string showName = ComboSourceUtil.Instance.FindEnum(pv.Venue, "venues");
                                if (!pv.VenueGW.ToUpper().Equals("INNER") && !string.IsNullOrEmpty(showName) && !pv.isForbit
                                    && (string.IsNullOrEmpty(allChanel) || !allChanel.Contains(showName)))//展示匹配到的外部渠道数
                                    outChannelCount++;

                                sbName.Append(showName);
                            }
                        }

                        OutChannelNum = outChannelCount.ToString();
                        RandomNum = outChannelCount > 0 ? "1" : string.Empty;
                        string goodsType = priceMatchDetail != null ? SetGoodsShortname(priceMatchDetail.order.Symbol) : "";
                        List<Venue> venues = SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "1");
                        if (venues != null && venues.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            StringBuilder sbTag = new StringBuilder();
                            foreach (Venue uv in venues)
                            {
                                sb.Append(uv.name + ",");
                                sbTag.Append(uv.venue + ",");
                            }
                            string name = sb.ToString();
                            string venue = sbTag.ToString();
                            if (!string.IsNullOrEmpty(name))
                                OutChannel = name.Substring(0, name.Length - 1);
                            if (!string.IsNullOrEmpty(venue))
                                CheckOutChannelTag = venue.Substring(0, venue.Length - 1);
                        }
                    }

                    if (contentArr != null && contentArr.Length > 0)
                    {
                        string remaingingTime = string.Empty;
                        string symbol = string.Empty;
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(msg.MessageContent.securityid);
                        if (bondsInfoPo != null)
                        {
                            remaingingTime = bondsInfoPo.RemaingingTime + " ";
                            //symbol = bondsInfoPo.Short_Name + " ";
                        }
                        ContentStart = $"{remaingingTime}{symbol}{contentArr[0]}";
                    }

                    if (_isFirm && priceMatchDetail != null && priceMatchDetail.order != null)
                    {
                        if (!string.IsNullOrEmpty(priceMatchDetail.order.WholeFlag))
                        {
                            //if (priceMatchDetail.order.WholeFlag.Equals("1") 
                            //    || (!priceMatchDetail.order.WholeFlag.Equals("1")
                            //        && ((priceMatchDetail.order.CrossVenues.Equals("2") && string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues))
                            //                || (priceMatchDetail.order.CrossVenues.Equals("3")
                            //                    && (!string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues) 
                            //                    && priceMatchDetail.order.SelfSelectVenues.ToUpper().Equals("ODM-ODM"))))))

                            if (!priceMatchDetail.order.WholeFlag.Equals("1")
                                && ((priceMatchDetail.order.CrossVenues.Equals("2") && string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues))
                                        || (priceMatchDetail.order.CrossVenues.Equals("3")
                                            && (!string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues)
                                            && priceMatchDetail.order.SelfSelectVenues.ToUpper().Equals("ODM-ODM")))))
                            {
                                string str = "说明：本订单（{0}）不允许发到外部渠道";
                                NonFirmBorder = FirmBorder = Visibility.Collapsed;
                                RemarkVisibility = Visibility.Visible;
                                RowHeight = 70;
                                if (priceMatchDetail.order.WholeFlag.Equals("1"))
                                    Remark = string.Format(str, "整量");
                                else if (!priceMatchDetail.order.WholeFlag.Equals("1") && priceMatchDetail.order.CrossVenues.Equals("2")
                                    && string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues))
                                    Remark = string.Format(str, "仅内部");
                                else if (!priceMatchDetail.order.WholeFlag.Equals("1") && priceMatchDetail.order.CrossVenues.Equals("3")
                                        && (!string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues) && priceMatchDetail.order.SelfSelectVenues.ToUpper().Equals("ODM-ODM")))
                                {
                                    Remark = string.Format(str, "仅内部+X-bond");
                                    RemarkOtherVisibility = Visibility.Visible;
                                }
                                ConfirmVisibility = Visibility.Visible;
                                CancelVisibility = Visibility.Collapsed;

                                //Button btnSendPrice = this._myWindow.FindName("btnSendPrice") as Button;
                                //btnSendPrice.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }

            if (isRealtime)
            {
                if (priceMatchDetail == null || (priceMatchDetail != null && priceMatchDetail.order != null && priceMatchDetail.order.OrderStatus.Equals("2"))
                    || (priceMatchDetail != null && priceMatchDetail.Prices != null && priceMatchDetail.Prices.Count == 0))
                    SendPriceEnable = false;
                else
                    SendPriceEnable = true;
            }
        }

        private string SetGoodsShortname(string goodcode)
        {
            string goodsType = string.Empty;
            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(goodcode);
            if (bondsInfoPo != null)
                goodsType = bondsInfoPo.SECURITY_TYPE;

            return goodsType;
        }

        private List<ListBoxItemData> VenueToListBoxItemData(List<Venue> venues)
        {
            List<ListBoxItemData> result = null;

            if (venues != null && venues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (Venue uv in venues)
                {
                    ListBoxItemData itemData = new ListBoxItemData();
                    itemData.Code = uv.venue;
                    itemData.Name = uv.name;
                    itemData.Seq = uv.sn;
                    itemData.Level = uv.level;
                    itemData.Venuetypegw = uv.venuetypegw;

                    result.Add(itemData);

                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
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
                        foreach (TTbookSystemMessage msg in sysMsg)
                        {
                            if (msg.content != null && msg.type.Equals(CurrentMsgType) && (!string.IsNullOrEmpty(CurrentMsgId) && msg.id.Equals(CurrentMsgId)))
                            {
                                GridDatas.Clear();
                                InitUI(ObjectConvert.SystemMsgToShowMsg(msg, _myWindow), true);
                            }
                        }
                    }
                }
            }));
        }

        private bool CheckVenuePermission(string venue, string security)
        {
            //if (!string.IsNullOrEmpty(venue) && ((venue.ToUpper() == "MANUAL") || (venue.ToUpper() == "人工") || (venue.ToUpper() == "内部")))
            //    return true;

            string goodsType = CommonUtils.SetGoodsShortname(security);

            List<Venue> venues = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, string.Empty);

            bool result = false;
            if (venues != null)
            {
                foreach (Venue ve in venues)
                {
                    if (ve.venue.ToUpper() == venue.ToUpper())
                    {
                        result = true;

                        break;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// 处理表格双击事件， 弹出点价界面
        /// </summary>
        /// <param name="selectItem"></param>
        public void OnDoubleClick(object selectItem)
        {
            GridData selectedGridData = selectItem as STC.XBond.Plugin.CustomeUI.GridData;

            if (selectedGridData == null)
                return;

            _matchVenue = selectedGridData.Venue;
            _matchId = selectedGridData.Id;

            if (_priceMatchParams != null)
                _priceMatchParams.Clear();
            if (_priceValues != null)
                _priceValues.Clear();

            if (_priceMatchDetail != null && _priceMatchDetail.Prices != null)
            {
                if (!CheckVenuePermission(_matchVenue, _priceMatchDetail.order.Symbol))
                    return;

                _priceValues = _priceMatchDetail.Prices.Where(delegate (PriceValue pv)
                {
                    if (pv.Id.Equals(_matchId))
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
                    if (Common.View.Component.CustomMessageBox.Question("是否修改已存在的订单?", "编辑订单", _myWindow))
                    {
                        // 2 改单
                        EditOrder(_priceMatchDetail.order.OrderId, currentPointAmount);
                        return;
                    }
                }
                else if (currentPointAmount >= MatchTotal)
                {
                    // 1.弹框提示
                    if (Common.View.Component.CustomMessageBox.Question("是否撤销已存在的订单?", "删除订单", _myWindow))
                    {
                        // 2.撤单
                        DeleteOrder(_priceMatchDetail.order.OrderId);
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
                    PriceValue pv = _priceValues[0];

                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPointPriceWinKey(pv.Id)))
                    {
                        SendPointPriceView sendPointPriceView =
                        new SendPointPriceView(_priceMatchDetail.order.Symbol, !_priceMatchDetail.order.Side.ToUpper().Equals("BUY"), pv.Value.ToString(), pv.Qty.ToString(),
                            pv.SettlType, pv.NetPrice.ToString(), pv.Venue, pv.WholeFlag, pv.IsFirmFlag, pv.Id, $"{pv.Date} {pv.Time}", pv.DeliveryType, pv.ClearingMethod, null, "", "", pv.VenueGW, null, _priceMatchDetail.order.Info4, false);


                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointPriceWinKey(pv.Id), sendPointPriceView);

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

            priceMatchParam.PriceCondition = singleOrder.PriceCondition;

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

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(DataRow dr)
        {
            string strategyCodeID = CommonUtils.ConvertToString(dr[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(dr[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            string qty = CommonUtils.ConvertToString(dr[MyOrderProcesser.QTY_ORDER_FIELD]);
            string exeqty = CommonUtils.ConvertToString(dr[MyOrderProcesser.EXECQTY_ORDER_FIELD]);
            string price = CommonUtils.ConvertToString(dr[MyOrderProcesser.PRICE_ORDER_FIELD]);
            string netPrice = CommonUtils.ConvertToString(dr[MyOrderProcesser.NETPRICE_ORDER_FIELD]);
            string settlType = CommonUtils.ConvertToString(dr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            string side = CommonUtils.ConvertToString(dr[MyOrderProcesser.SIDE_ORDER_FIELD]);
            string wholeFlag = CommonUtils.ConvertToString(dr[MyOrderProcesser.WHOLE_FLAG_FIELD]);
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

            string priceCondition = CommonUtils.ConvertToString(dr[MyOrderProcesser.PriceCondition_Field]);

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

            sendPricePo.WholeFlag = wholeFlag;

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

            sendPricePo.PriceCondition = priceCondition;

            return sendPricePo;
        }
    }

    /// <summary>
    /// 价格匹配数据结构
    /// </summary>
    public class GridData : ViewModelBase
    {
        private bool isChecked;
        public bool IsChecked { get { return isChecked; } set { isChecked = value; this.OnPropertyChanged(); } }

        private bool enabledBtn;
        public bool EnabledBtn { get { return enabledBtn; } set { enabledBtn = value; this.OnPropertyChanged(); } }

        private string symbol;
        public string Symbol { get { return symbol; } set { symbol = value; this.OnPropertyChanged(); } }
        private string venueTypeGW;
        public string VenueTypeGW { get { return venueTypeGW; } set { venueTypeGW = value; this.OnPropertyChanged(); } }

        private string venue;
        public string Venue { get { return venue; } set { venue = value; this.OnPropertyChanged(); } }

        private string side;
        public string Side { get { return side; } set { side = value; this.OnPropertyChanged(); } }
        private string clearingMethod;
        public string ClearingMethod { get { return clearingMethod; } set { clearingMethod = value; this.OnPropertyChanged(); } }
        private string deliveryType;
        public string DeliveryType { get { return deliveryType; } set { deliveryType = value; this.OnPropertyChanged(); } }
        private string wholeFlag;
        public string WholeFlag { get { return wholeFlag; } set { wholeFlag = value; this.OnPropertyChanged(); } }
        private string trsactTime;
        public string TrsactTime { get { return trsactTime; } set { trsactTime = value; this.OnPropertyChanged(); } }
        private string quoteType;
        public string QuoteType { get { return quoteType; } set { quoteType = value; this.OnPropertyChanged(); } }


        private string orderStatus;
        public string OrderStatus { get { return orderStatus; } set { orderStatus = value; } }
        private string id;
        public string Id { get { return id; } set { id = value; this.OnPropertyChanged(); } }
        private string sort;
        public string Sort { get { return sort; } set { sort = value; this.OnPropertyChanged(); } }

        private Brush priceBrush;
        public Brush PriceBrush { get { return priceBrush; } set { priceBrush = value; this.OnPropertyChanged(); } }

        private string price;
        public string Price { get { return price; } set { price = value; this.OnPropertyChanged(); } }
        private string firmType;
        public string FirmType { get { return firmType; } set { firmType = value; this.OnPropertyChanged(); } }
        private string amount;
        public string Amount { get { return amount; } set { amount = value; this.OnPropertyChanged(); } }
        private string source;
        public string Source { get { return source; } set { source = value; this.OnPropertyChanged(); } }

        private string dealSettle;
        public string DealSettle { get { return dealSettle; } set { dealSettle = value; this.OnPropertyChanged(); } }

        private string settle;
        public string Settle { get { return settle; } set { settle = value; this.OnPropertyChanged(); } }

        private string pointAmount;
        public string PointAmount { get { return pointAmount; } set { pointAmount = value; this.OnPropertyChanged(); } }

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

        private string info4;
        public string Info4 { get { return info4; } set { info4 = value; this.OnPropertyChanged(); } }

        private string _partySubID;

        public string PartySubID
        {
            get { return _partySubID; }
            set 
            { 
                _partySubID = value;
                this.OnPropertyChanged();
            }
        }
    }
}

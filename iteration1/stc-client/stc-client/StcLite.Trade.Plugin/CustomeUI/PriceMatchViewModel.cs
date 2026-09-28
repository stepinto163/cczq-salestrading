using Ats.Foundation.Message;
using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.Model.Base;
using STC.Common.Model;
using BDB.Trade.Plugin.CustomUI;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.TBook;
using BDB.Trade.Plugin.View.Order;
using STC.Common.Handler.SystemParam;
using BDB.Trade.Plugin.ViewModel.Adapter;
using STC.Common.Data;

namespace BDB.Trade.Plugin.CustomeUI
{
    public class PriceMatchViewModel : BaseViewModel
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


        #endregion

        #region 命令事件

        public ICommand CancelCommand { get { return new ComRelayCommand(CancelClick); } }
        public ICommand CloseCommand { get { return new ComRelayCommand(CloseClick); } }
        public ICommand OkCommand { get { return new ComRelayCommand(OKClick); } }
        public ICommand ConfirmCommand { get { return new ComRelayCommand(ConfirmClick); } }

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

        private void OKClick()
        {
            if (!Check())
                return;

            PriceMatchDetail priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(_currentMsg.Detail);

            if (_isFirm)
            {
                UpdateOrder(priceMatchDetail);

                //this.CloseClick();
            }
            else
            {
                UpdatePrice(priceMatchDetail);

                //CancelClick();
            }
            CancelClick();
            //this.CloseClick();
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

            if (!WindowManager.Instance.IsExist(Utils.WindowKeyHelper.GetPriceMatchSendPriceWinKey(CurrentMsgId)))
            {
                SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
                WindowManager.Instance.OpenOnlyOneWindow(Utils.WindowKeyHelper.GetPriceMatchSendPriceWinKey(CurrentMsgId), sendPriceView);
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
                        if (priceMatchDetail.order != null)
                        {
                            Brush brush = null;
                            if (priceMatchDetail.order.Side.ToUpper().Equals("SELL") || priceMatchDetail.order.Side.Equals("2"))
                                brush = (Brush)_myWindow.FindResource("ColorBrush.Orange7");
                            else
                                brush = (Brush)_myWindow.FindResource("ColorBrush.Blue8");

                            dg.PriceBrush = brush;
                        }

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
                }
                CurrentMsgId = msg.Id;
                string[] contentArr = null;
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
                    contentArr = msg.Content.Split(new string[] { FIRM }, StringSplitOptions.RemoveEmptyEntries);

                    int outChannelCount = 0;
                    StringBuilder sbName = new StringBuilder();
                    foreach (PriceValue pv in priceValues)
                    {
                        string allChanel = sbName.ToString();
                        string showName = ComboSourceUtil.Instance.FindEnum(pv.Venue, "venues");
                        if (!pv.VenueGW.ToUpper().Equals("INNER") && !string.IsNullOrEmpty(showName) && !pv.isForbit
                            && (string.IsNullOrEmpty(allChanel) || !allChanel.Contains(showName)))//展示匹配到的外部渠道数
                            outChannelCount++;

                        sbName.Append(showName);
                    }

                    OutChannelNum = outChannelCount.ToString();
                    RandomNum = outChannelCount > 0 ? "1" : string.Empty;
                    string goodsType = SetGoodsShortname(priceMatchDetail.order.Symbol);
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
                    ContentStart = contentArr[0];
                }

                if (_isFirm && priceMatchDetail != null && priceMatchDetail.order != null)
                {
                    if (!string.IsNullOrEmpty(priceMatchDetail.order.WholeFlag))
                    {
                        if (priceMatchDetail.order.WholeFlag.Equals("1") 
                            || (!priceMatchDetail.order.WholeFlag.Equals("1")
                                && ((priceMatchDetail.order.CrossVenues.Equals("2") && string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues))
                                        || (priceMatchDetail.order.CrossVenues.Equals("3")
                                            && (!string.IsNullOrEmpty(priceMatchDetail.order.SelfSelectVenues) 
                                            && priceMatchDetail.order.SelfSelectVenues.ToUpper().Equals("ODM-ODM"))))))
                        {
                            string str = "说明：本订单（{0}）不允许发到外部渠道";
                            NonFirmBorder = FirmBorder = Visibility.Collapsed;
                            RemarkVisibility = Visibility.Visible;
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
        
        private void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            _msgTask.Enqueue(sysMsg);
        }

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
                            if (msg.content != null && msg.type.Equals(CurrentMsgType) && (!string.IsNullOrEmpty(CurrentMsgId) && msg.id.Equals(CurrentMsgId)))
                            {
                                InitUI(ObjectConvert.SystemMsgToShowMsg(msg, _myWindow), true);
                            }
                        }
                    }
                }
            }));
        }

        public void OnDoubleClick(object selectItem)
        {
            if (_isFirm)
            {
                GridData data = selectItem as GridData;
                if (data != null && _priceMatchDetail != null && _priceMatchDetail.Prices != null)
                {
                    List<PriceValue> priceValues = _priceMatchDetail.Prices.Where(delegate (PriceValue pv)
                    {
                        if (pv.Id.Equals(data.Id))
                            return true;
                        else
                            return false;
                    }).ToList();

                    if (priceValues != null && priceValues.Count > 0)
                    {
                        PriceValue pv = priceValues[0];

                        if (!WindowManager.Instance.IsExist(Utils.WindowKeyHelper.GetPointPriceWinKey(pv.Id)))
                        {

                            decimal tmpprice = 0;
                            if (!decimal.TryParse(pv.Value.ToString(), out tmpprice))
                            {
                                return;
                            }

                            SendPointPriceViewLite sendPointPriceView =  new SendPointPriceViewLite(_priceMatchDetail.order.Symbol, _priceMatchDetail.order.Side.ToUpper().Equals("BUY"), pv.Value.ToString(), pv.Qty.ToString(), pv.SettlType, pv.NetPrice.ToString(), pv.VenueGW, pv.WholeFlag, pv.IsFirmFlag);
                            // WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointPriceWinKey(pv.Id), sendPointPriceView);
                            sendPointPriceView.Show();
                        }
                    }
                }
            }
        }
    }

    public class GridData : BaseViewModel
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
    }
}

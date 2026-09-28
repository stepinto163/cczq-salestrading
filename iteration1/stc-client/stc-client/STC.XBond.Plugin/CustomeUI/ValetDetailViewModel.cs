using com.gateway.log;
using STC.Common.Handler.Cal;
using STC.Common.Model;
using STC.Common.Model.Cal;
using STC.Common.Model.EMS;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// ValetDetailView的数据内容
    /// </summary>
    public class ValetDetailViewModel : BaseViewModel
    {
        private string _commonStr = "--";
        private FrameworkElement _myOwn = null;
        private bool _isBankCode = true;
        private string _tradeDate = string.Empty;
        private string _settleType = string.Empty;

        #region 界面属性

        private ObservableCollection<ComboxItemData> _comboxDatas = new ObservableCollection<ComboxItemData>();
        public ObservableCollection<ComboxItemData> ComboxDatas
        {
            get
            {
                return _comboxDatas;
            }
        }

        private bool _priceCheck = false;
        public bool PriceCheck
        {
            get { return _priceCheck; }
            set
            {
                _priceCheck = value;

                if (value)
                {
                    RemarksCheck = false;
                }

                IdbRemarksCheck = value;

                this.OnPropertyChanged();
            }
        }

        private ComboxItemData _comboxSelectedItem;
        public ComboxItemData ComboxSelectedItem
        {
            get { return _comboxSelectedItem; }
            set
            {
                _comboxSelectedItem = value;

                this.OnPropertyChanged();

                if (PriceCheck && _comboxSelectedItem != null)
                {
                    IdbVisibility = Visibility.Visible;

                    IdbYield = ComboxSelectedItem.NoMDEntry.MDEntryPx;
                    IdbClientInstName = ComboxSelectedItem.NoMDEntry.InstitutionName;
                    
                    if (IdbExchangeTxtEnable)
                        IdbClientExchanger = ComboxSelectedItem.NoMDEntry.TraderName;

                    IdbClientQty = ComboxSelectedItem.NoMDEntry.MDEntrySize;

                    CalDate(ComboxSelectedItem.NoMDEntry.SettlType);
                }
                else
                    IdbVisibility = Visibility.Collapsed;
            }
        }

       /// <summary>
       /// 设置默认清算速度
       /// </summary>
       /// <param name="settleType"></param>
       /// <returns></returns>
        private int SetDefaultSettle(string settleType)
        {
            int result = 0;
            result = settleType.Equals("1") || settleType.Contains("+0") || settleType.Contains("+0") 
                ? 0 : settleType.Equals("2") || settleType.Contains("+1") || settleType.Contains("+1") ? 1 : 0;


            return result;
        }

        /// <summary>
        /// 根据清算速度计算日期
        /// </summary>
        /// <param name="settleType"></param>
        private void CalDate(string settleType)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (requst != null && result.Data != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                string year = result.Data.dealDate.Substring(0, 4);
                                string month = result.Data.dealDate.Substring(4, 2);
                                string day = result.Data.dealDate.Substring(6, 2);

                                IdbSelectDate = CommonUtils.ConvertToDateTime(year, month, day);

                                IdbSelectSetle = SetDefaultSettle(settleType);
                            }
                            catch (Exception e) { }
                        }));
                    }
                }
                catch (Exception e)
                {
                }
            });
        }

        private bool _comboxEnable = false;
        public bool ComboxEnable
        {
            get { return _comboxEnable; }
            set
            {
                _comboxEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool _remarksCheck = false;
        public bool RemarksCheck
        {
            get { return _remarksCheck; }
            set
            {
                _remarksCheck = value;

                if (value)
                {
                    PriceCheck = false;

                }
                this.OnPropertyChanged();


                if (value)
                {
                    if (_isBankCode)
                    {
                        ComboBox comboBox = _myOwn.FindName("cbSendPriceType") as ComboBox;
                        if (comboBox != null)
                        {
                            comboBox.SelectedIndex = 0;
                            if (SendPriceType == null)
                                SendPriceType = comboBox.Items[0] as ComboBoxItem;
                        }
                    }
                }
                else
                {
                    ComboBox comboBox = _myOwn.FindName("cbSendPriceType") as ComboBox;
                    if (comboBox != null)
                    {
                        comboBox.SelectedIndex = -1;
                    }
                }

                if (!_isBankCode)
                {
                    ExchangeTxtEnable = value;
                }
                else
                {
                    ComboBox comboBox = _myOwn.FindName("cbSendPriceType") as ComboBox;
                    if (comboBox != null)
                    {
                        if (value && comboBox.SelectedIndex < 3)
                            ExchangeTxtEnable = value;
                        else
                            ExchangeTxtEnable = false;
                    }
                }
            }
        }

        private bool _remarksTxtEnable = false;
        public bool RemarksTxtEnable
        {
            get { return _remarksTxtEnable; }
            set
            {
                _remarksTxtEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool _exchangeTxtEnable = false;
        public bool ExchangeTxtEnable
        {
            get { return _exchangeTxtEnable; }
            set
            {
                _exchangeTxtEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool _idbExchangeTxtEnable = false;
        public bool IdbExchangeTxtEnable
        {
            get { return _idbExchangeTxtEnable; }
            set
            {
                _idbExchangeTxtEnable = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _idbVisibility = Visibility.Collapsed;
        public Visibility IdbVisibility
        {
            get { return _idbVisibility; }
            set
            {
                _idbVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _IDBBorderVisibility = Visibility.Collapsed;
        public Visibility IDBBorderVisibility
        {
            get { return _IDBBorderVisibility; }
            set
            {
                _IDBBorderVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private string _clientInstName = string.Empty;
        public string ClientInstName
        {
            get { return _clientInstName; }
            set
            {
                _clientInstName = value;

                this.OnPropertyChanged();
            }
        }

        private string _yield = string.Empty;
        public string Yield
        {
            get { return _yield; }
            set
            {
                _yield = value;

                this.OnPropertyChanged();
            }
        }

        private string _clientExchanger = string.Empty;
        public string ClientExchanger
        {
            get { return _clientExchanger; }
            set
            {
                _clientExchanger = value;

                this.OnPropertyChanged();
            }
        }

        private string _idbClientInstName;
        public string IdbClientInstName
        {
            get
            {
                return _idbClientInstName;
            }

            set
            {
                _idbClientInstName = value;

                this.OnPropertyChanged();
            }
        }

        private string _idbYield;
        public string IdbYield
        {
            get
            {
                return _idbYield;
            }

            set
            {
                _idbYield = value;

                this.OnPropertyChanged();
            }
        }

        private string _idbClientExchanger;
        public string IdbClientExchanger
        {
            get
            {
                return _idbClientExchanger;
            }

            set
            {
                _idbClientExchanger = value;

                this.OnPropertyChanged();
            }
        }

        private bool _idbRemarksCheck = false;
        public bool IdbRemarksCheck
        {
            get
            {
                return _idbRemarksCheck;
            }

            set
            {
                _idbRemarksCheck = value;

                this.OnPropertyChanged();
                
                if (value)
                {
                    if (_isBankCode)
                    {
                        ComboBox comboBox = _myOwn.FindName("cbIdbSendPriceType") as ComboBox;
                        if (comboBox != null)
                        {
                            comboBox.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    ComboBox comboBox = _myOwn.FindName("cbIdbSendPriceType") as ComboBox;
                    if (comboBox != null)
                    {
                        comboBox.SelectedIndex = -1;
                    }
                }

                if (!_isBankCode)
                {
                    IdbExchangeTxtEnable = value;
                }
                else
                {
                    ComboBox comboBox = _myOwn.FindName("cbIdbSendPriceType") as ComboBox;
                    if (comboBox != null)
                    {
                        if (value && comboBox.SelectedIndex < 3)
                            IdbExchangeTxtEnable = value;
                        else
                            IdbExchangeTxtEnable = false;
                    }
                }
            }
        
        }


        private string _idbClientQty;
        public string IdbClientQty
        {
            get
            {
                return _idbClientQty;
            }

            set
            {
                _idbClientQty = value;

                this.OnPropertyChanged();
            }
        }

        private string _clientQty;
        public string ClientQty
        {
            get
            {
                return _clientQty;
            }

            set
            {
                _clientQty = value;

                this.OnPropertyChanged();
            }
        }

        private ComboBoxItem _sendPriceType;
        public ComboBoxItem SendPriceType
        {
            get
            {
                return _sendPriceType;
            }

            set
            {
                _sendPriceType = value;

                if (value != null && value.Tag != null)
                {
                    string tag = value.Tag as string;
                    if (!string.IsNullOrEmpty(tag) && (tag == "4" || tag == "50"))
                    {
                        ExchangeTxtEnable = false;
                        ClientExchanger = _commonStr;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ClientExchanger) && ClientExchanger == _commonStr)
                            ClientExchanger = string.Empty;

                        ExchangeTxtEnable = true;

                    }
                }
                this.OnPropertyChanged();
            }
        }

        private ComboBoxItem _idbSendPriceType;
        public ComboBoxItem IdbSendPriceType
        {
            get
            {
                return _idbSendPriceType;
            }

            set
            {
                _idbSendPriceType = value;

                if (value != null && value.Tag != null)
                {
                    string tag = value.Tag as string;
                    if (!string.IsNullOrEmpty(tag) && (tag == "4" || tag == "50"))
                    {
                        IdbExchangeTxtEnable = false;
                        IdbClientExchanger = _commonStr;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(IdbClientExchanger) && IdbClientExchanger == _commonStr)
                            IdbClientExchanger = string.Empty;

                        IdbExchangeTxtEnable = true;

                    }
                }
                this.OnPropertyChanged();
            }
        }

        private DateTime? _selectDate;
        public DateTime? SelectDate
        {
            get
            {
                return _selectDate;
            }

            set
            {
                _selectDate = value;

                this.OnPropertyChanged();
            }
        }

        private DateTime? _idbSelectDate;
        public DateTime? IdbSelectDate
        {
            get
            {
                return _idbSelectDate;
            }

            set
            {
                _idbSelectDate = value;

                this.OnPropertyChanged();
            }
        }

        private int _idbSelectSetle;
        public int IdbSelectSetle
        {
            get
            {
                return _idbSelectSetle;
            }

            set
            {
                _idbSelectSetle = value;

                this.OnPropertyChanged();
            }
        }

        private int _selectSetle;
        public int SelectSetle
        {
            get
            {
                return _selectSetle;
            }

            set
            {
                _selectSetle = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _dbPositionVisibility;
        public Visibility IdbPositionVisibility
        {
            get
            {
                return _dbPositionVisibility;
            }

            set
            {
                _dbPositionVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _positionVisibility;
        public Visibility PositionVisibility
        {
            get
            {
                return _positionVisibility;
            }

            set
            {
                _positionVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private string _postionTxt;
        public string PostionTxt
        {
            get
            {
                return _postionTxt;
            }

            set
            {
                _postionTxt = value;

                this.OnPropertyChanged();
            }
        }

        private string _idbPostionTxt;
        public string IdbPostionTxt
        {
            get
            {
                return _idbPostionTxt;
            }

            set
            {
                _idbPostionTxt = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _idbSendPriceTypeVisibility;
        public Visibility IdbSendPriceTypeVisibility
        {
            get
            {
                return _idbSendPriceTypeVisibility;
            }

            set
            {
                _idbSendPriceTypeVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _sendPriceTypeVisibility;
        public Visibility SendPriceTypeVisibility
        {
            get
            {
                return _sendPriceTypeVisibility;
            }

            set
            {
                _sendPriceTypeVisibility = value;

                this.OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// 初始化idb数据
        /// </summary>
        /// <param name="order"></param>
        private void InitIDB(ExecutionReport order)
        {
            if (order != null && !string.IsNullOrEmpty(order.TraderName) && !string.IsNullOrEmpty(order.InstitutionName))
            {
                PriceCheck = true;
                IdbVisibility = Visibility.Visible;
                IdbClientInstName = order.InstitutionName;
                IdbYield = order.Price;
                IdbClientExchanger = order.TraderName;
                IdbClientQty = order.OrderQty;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        string tradeDate = CommonUtils.CalTradeDate(order.Info6);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {

                            IdbSelectDate = new DateTime?(CommonUtils.ConvertToDateTime2(tradeDate));

                            if (!string.IsNullOrEmpty(order.Info6))//order.SettlType))
                            {
                                IdbSelectSetle = SetDefaultSettle(order.Info6);//order.SettlType);
                            }


                            if (order.SecurityID.EndsWith("IB"))//银行间债券
                            {
                                _isBankCode = true;
                                IdbSendPriceTypeVisibility = SendPriceTypeVisibility = SendPriceTypeVisibility = Visibility.Visible;
                                IdbPositionVisibility = PositionVisibility = Visibility.Collapsed;

                                ComboBox idbComboBox = _myOwn.FindName("cbIdbSendPriceType") as ComboBox;
                                if (idbComboBox != null)
                                {
                                    idbComboBox.SelectedIndex = 0;
                                }

                            }
                            else//交易所债券SH,SZ
                            {
                                _isBankCode = false;
                                SendPriceTypeVisibility = SendPriceTypeVisibility = Visibility.Collapsed;
                                IdbPositionVisibility = PositionVisibility = Visibility.Visible;
                                IdbSendPriceTypeVisibility = Visibility.Collapsed;
                            }


                            ComboxItemData itemData = new ComboxItemData();
                            string[] arr = order.TransactTime.Split(new string[] { " ", "." }, StringSplitOptions.RemoveEmptyEntries);
                            string showTime = arr != null && arr.Length > 1 ? arr[1] : order.TransactTime;
                            itemData.ShowTxt = string.Format(" {0} {1}万 {2} {3} {4} {5} {6}  ", order.Side.Equals("1") ? "Bid" : "Ofr", order.OrderQty, order.Price, "nonfirm",
                            order.InstitutionName, order.TraderName, showTime);

                            ComboBox idbCosmboBox = _myOwn.FindName("cbIDB") as ComboBox;
                            idbCosmboBox.Text = itemData.ShowTxt;
                        }));
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="own"></param>
        /// <param name="comboxData"></param>
        /// <param name="price"></param>
        /// <param name="goodsCode"></param>
        /// <param name="tradeDate"></param>
        /// <param name="settleType"></param>
        /// <param name="amount"></param>
        /// <param name="order"></param>
        public void Init(FrameworkElement own, object comboxData, string price, string goodsCode, string tradeDate, string settleType, string amount, ExecutionReport order = null)
        {
            _myOwn = own;

            ObservableCollection<ComboxItemData> data = comboxData as ObservableCollection<ComboxItemData>;
            if (data != null)
                _comboxDatas = data;
            
            IdbYield = Yield = price;

            _tradeDate = tradeDate;
            _settleType = settleType;

            IdbSelectDate = SelectDate = new DateTime?(CommonUtils.ConvertToDateTime2(_tradeDate));
            
            if (!string.IsNullOrEmpty(_settleType))
            {
                IdbSelectSetle = SelectSetle = SetDefaultSettle(settleType);
            }
            
            if (!string.IsNullOrEmpty(goodsCode))
            {
                if (goodsCode.EndsWith("IB"))//银行间债券
                {
                    _isBankCode = true;
                    IdbSendPriceTypeVisibility = SendPriceTypeVisibility = SendPriceTypeVisibility = Visibility.Visible;
                    IdbPositionVisibility = PositionVisibility = Visibility.Collapsed;
                    
                    ComboBox comboBox = _myOwn.FindName("cbSendPriceType") as ComboBox;
                    if (comboBox != null)
                    {
                        comboBox.SelectedIndex = 0;
                    }

                    ComboBox idbComboBox = _myOwn.FindName("cbIdbSendPriceType") as ComboBox;
                    if (idbComboBox != null)
                    {
                        idbComboBox.SelectedIndex = 0;
                    }

                }
                else//交易所债券SH,SZ
                {
                    _isBankCode = false;
                    SendPriceTypeVisibility = SendPriceTypeVisibility = Visibility.Collapsed;
                    IdbPositionVisibility = PositionVisibility = Visibility.Visible;
                    IdbSendPriceTypeVisibility = Visibility.Collapsed;
                }
            }

            IdbClientQty = ClientQty = amount;

            RemarksCheck = true;

            this.InitIDB(order);
        }

        /// <summary>
        /// 生成补单请求数据
        /// </summary>
        /// <returns></returns>
        public UpdateOrderReq GetValetInfo()
        {
            UpdateOrderReq result = new UpdateOrderReq();
            if (PriceCheck)
            {
                result.Info1 = IdbYield;

                result.InstitutionID = ComboxSelectedItem != null ? ComboxSelectedItem.NoMDEntry.CounterPartyID : string.Empty ;
                result.InstitutionName = IdbClientInstName;
                result.TraderID = ComboxSelectedItem != null ? ComboxSelectedItem.NoMDEntry.CounterPartySubID : string.Empty;
                result.TraderName = IdbClientExchanger;
                result.Info2 = IdbClientQty;
                result.Info5 = "IDB";

                if (IdbSendPriceTypeVisibility == Visibility.Visible)
                    result.TradeMethod = IdbSendPriceType == null ? string.Empty : IdbSendPriceType.Tag as string;
                else
                    result.TradeMethod = IdbPostionTxt;

                result.Info6 = GetSettle(IdbSelectDate, IdbSelectSetle);
            }
            else if (RemarksCheck)
            {
                result.Info1 = Yield;

                result.InstitutionName = ClientInstName;
                result.TraderName = ClientExchanger;
                result.Info2 = ClientQty;
                result.Info5 = "MANUAL";

                if (SendPriceTypeVisibility == Visibility.Visible)
                    result.TradeMethod = SendPriceType == null ? string.Empty : SendPriceType.Tag as string;
                else
                    result.TradeMethod = PostionTxt;

                result.Info6 = GetSettle(SelectDate, SelectSetle);
            }

            return result;
        }

        /// <summary>
        /// 根据选择的日期 和 选择的清算速度 计算出内部清算速度
        /// </summary>
        /// <param name="selectDate"></param>
        /// <param name="selectSetle"></param>
        /// <returns></returns>
        private string GetSettle(DateTime? selectDate, int selectSetle)
        {
            string result = string.Empty;
            if (selectDate != null && selectDate.HasValue)
            {
                if (SettleParseHelper.IsSpotSettle(selectDate, selectSetle))
                    result = SettleParseHelper.IsTomorrow(SelectDate, SelectSetle) ? SettleParseHelper.IsToday(SelectDate) ? "2" : "3" : "1";//SettleParseHelper.IsTomorrow(selectDate, selectSetle) ? "3" : "1";//2:明天+0转T+1
                else
                    result = selectDate.Value.ToString("MM.dd") + (selectSetle == 0 ? "+0" : "+1");
            }

            return result;
        }

        /// <summary>
        /// 检查输入数据是否完整
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            bool result = true;

            if (PriceCheck)
            {
                result = !string.IsNullOrEmpty(IdbClientInstName) && !string.IsNullOrEmpty(IdbYield) //&& !string.IsNullOrEmpty(IdbClientExchanger)
                    && !string.IsNullOrEmpty(IdbClientQty);
            }
            if (result && RemarksCheck)
            {
                result = !string.IsNullOrEmpty(ClientInstName) && !string.IsNullOrEmpty(Yield) //&& CheckClientExchanger()
                    && !string.IsNullOrEmpty(ClientQty) && _selectDate != null && _selectSetle >= 0;
            }
            if (!PriceCheck && !RemarksCheck)
                result = false;

            return result;
        }

        /// <summary>
        /// 检查数据发送类型
        /// </summary>
        /// <returns></returns>
        private bool CheckClientExchanger()
        {
            bool result = true;
            if (_sendPriceType != null && _sendPriceType.Tag != null)
            {
                string tag = _sendPriceType.Tag as string;
                if (!string.IsNullOrEmpty(tag) && (tag == "4" || tag == "50"))
                {
                    result = true;
                }
                else if (!string.IsNullOrEmpty(ClientExchanger))
                    result = true;
                else
                    result = false;
            }

            return result;
        }
    }
}

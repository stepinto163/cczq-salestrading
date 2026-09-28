using com.gateway.log;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;
using STC.Common.Model.FinQ;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.WorkBench.Plugin.ViewModel.Data
{
    public class IntentionMarketItem : ViewModelBase
    {
        private int _sn;

        public int Sn
        {
            get { return _sn; }
            set
            {
                _sn = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isChecked = false;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.OnPropertyChanged();
            }
        }

        private string _intentionId;

        public string IntentionId
        {
            get { return _intentionId; }
            set
            {
                _intentionId = value;
                this.OnPropertyChanged();
            }
        }

        private string _id;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                this.OnPropertyChanged();
            }
        }

        private string _quoteid;

        public string QuoteId
        {
            get { return _quoteid; }
            set
            {
                _quoteid = value;
                this.OnPropertyChanged();
            }
        }

        private string _inquiryKey;

        public string InquiryKey
        {
            get { return _inquiryKey; }
            set
            {
                _inquiryKey = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionId;

        public string InstitutionId
        {
            get { return _institutionId; }
            set
            {
                _institutionId = value;
                this.OnPropertyChanged();
            }
        }

        private string _securityId;

        public string SecurityId
        {
            get { return _securityId; }
            set
            {
                _securityId = value;
                this.OnPropertyChanged();
            }
        }

        private string _symbol;

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                this.OnPropertyChanged();
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                this.OnPropertyChanged();
            }
        }

        private string _userId;

        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                this.OnPropertyChanged();
            }
        }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionName;

        public string InstitutionName
        {
            get { return _institutionName; }
            set
            {
                _institutionName = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderId;

        public string TraderId
        {
            get { return _traderId; }
            set
            {
                _traderId = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderName;

        public string TraderName
        {
            get { return _traderName; }
            set
            {
                _traderName = value;
                this.OnPropertyChanged();
            }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                this.OnPropertyChanged();
            }
        }

        private string _side_Edit;

        public string Side_Edit
        {
            get { return _side_Edit; }
            set
            {
                _side_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _side;

        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price_Edit;

        public decimal? Price_Edit
        {
            get { return _price_Edit; }
            set
            {
                _price_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price;

        public decimal? Price
        {
            get { return _price; }
            set
            {
                _price = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _qty_Edit;

        public decimal? Qty_Edit
        {
            get { return _qty_Edit; }
            set
            {
                _qty_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _qty;

        public decimal? Qty
        {
            get { return _qty; }
            set
            {
                _qty = value;
                this.OnPropertyChanged();
            }
        }

        private string _settlType;

        public string SettlType
        {
            get { return _settlType; }
            set
            {
                _settlType = value;
                SettlType_Edit = value;
                ParseSettle(value);
                this.OnPropertyChanged();
            }
        }

        private string _settlType_edit;

        public string SettlType_Edit
        {
            get { return _settlType_edit; }
            set
            {
                _settlType_edit = value;
                this.OnPropertyChanged();
            }
        }

        //private DateTime _edit_settltype_date;

        //public DateTime edit_settltype_date
        //{
        //    get { return _edit_settltype_date; }
        //    set
        //    {
        //        _edit_settltype_date = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private int _edit_settletype = int.MinValue;

        //public int edit_settletype
        //{
        //    get { return _edit_settletype; }
        //    set
        //    {
        //        _edit_settletype = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        #region 标签
        // 急
        private string _HurryFlag_Edit = string.Empty;

        public string HurryFlag_Edit
        {
            get
            {
                return _HurryFlag_Edit;
            }
            set
            {
                _HurryFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _HurryFlag = string.Empty;

        public string HurryFlag
        {
            get
            {
                return _HurryFlag;
            }
            set
            {
                _HurryFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 荐
        private string _SuggestFlag_Edit = string.Empty;

        public string SuggestFlag_Edit
        {
            get
            {
                return _SuggestFlag_Edit;
            }
            set
            {
                _SuggestFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _SuggestFlag = string.Empty;

        public string SuggestFlag
        {
            get
            {
                return _SuggestFlag;
            }
            set
            {
                _SuggestFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 火
        private string _FireFlag_Edit = string.Empty;

        public string FireFlag_Edit
        {
            get
            {
                return _FireFlag_Edit;
            }
            set
            {
                _FireFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _FireFlag = string.Empty;

        public string FireFlag
        {
            get
            {
                return _FireFlag;
            }
            set
            {
                _FireFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 底价
        private string _FloorPrice_Edit = string.Empty;

        public string FloorPrice_Edit
        {
            get
            {
                return _FloorPrice_Edit;
            }
            set
            {
                _FloorPrice_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _FloorPrice = string.Empty;

        public string FloorPrice
        {
            get
            {
                return _FloorPrice;
            }
            set
            {
                _FloorPrice = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private string _PFlag_Edit = string.Empty;

        public string PFlag_Edit
        {
            get { return _PFlag_Edit; }
            set
            {
                _PFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 0：不选  1：*   2：P*   
        /// </summary>
        private string _PFlag = string.Empty;

        public string PFlag
        {
            get { return _PFlag; }
            set
            {
                _PFlag = value;
                this.OnPropertyChanged();
            }
        }

        ///// <summary>
        ///// *
        ///// </summary>
        //private string _isP;

        //public string IsP
        //{
        //    get { return _isP; }
        //    set
        //    {
        //        _isP = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private string _isP_Edit;

        //public string IsP_Edit
        //{
        //    get { return _isP_Edit; }
        //    set
        //    {
        //        _isP_Edit = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// P*
        ///// </summary>
        //private string _isPS;

        //public string IsPS
        //{
        //    get { return _isPS; }
        //    set
        //    {
        //        _isPS = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private string _isPS_Edit;

        //public string IsPS_Edit
        //{
        //    get { return _isPS_Edit; }
        //    set
        //    {
        //        _isPS_Edit = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        // oco, 打包

        private string _OCOFlag_Edit = string.Empty;

        public string OCOFlag_Edit
        {
            get
            {
                return _OCOFlag_Edit;
            }
            set
            {
                _OCOFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _OCOFlag = string.Empty;

        public string OCOFlag
        {
            get
            {
                return _OCOFlag;
            }
            set
            {
                _OCOFlag = value;
                this.OnPropertyChanged();
            }
        }

        private string _SourceTrader;

        public string SourceTrader
        {
            get { return _SourceTrader; }
            set
            {
                _SourceTrader = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// 发送方QQ号码
        /// </summary>
        private string _senderNumber;

        public string SenderNumber
        {
            get { return _senderNumber; }
            set
            {
                _senderNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _receiverNumber_edit;

        public string ReceiverNumber_Edit
        {
            get { return _receiverNumber_edit; }
            set
            {
                _receiverNumber_edit = value;
                this.OnPropertyChanged();
            }
        }

        private ToOption _receiver_edit;

        public ToOption Receiver_Edit
        {
            get { return _receiver_edit; }
            set
            {
                _receiver_edit = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 接收方QQ号码
        /// </summary>
        private string _receiverNumber;

        public string ReceiverNumber
        {
            get { return _receiverNumber; }
            set
            {
                _receiverNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _createTime;

        public string CreateTime
        {
            get { return _createTime; }
            set
            {
                _createTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _action;

        public string Action
        {
            get { return _action; }
            set
            {
                _action = value;
                this.OnPropertyChanged();
            }
        }

        private string _remark_Edit;

        public string Remark_Edit
        {
            get { return _remark_Edit; }
            set
            {
                _remark_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _remark;

        public string Remark
        {
            get { return _remark; }
            set
            {
                _remark = value;
                this.OnPropertyChanged();
            }
        }

        private string _optionType;

        public string OptionType
        {
            get { return _optionType; }
            set
            {
                _optionType = value;
                this.OnPropertyChanged();
            }
        }

        private string _clOrdID;

        public string ClOrdID
        {
            get { return _clOrdID; }
            set
            {
                _clOrdID = value;
                this.OnPropertyChanged();
            }
        }

        private string _execOrderId;

        public string ExecOrderId
        {
            get { return _execOrderId; }
            set
            {
                _execOrderId = value;
                this.OnPropertyChanged();
            }
        }

        private string _venue;

        public string Venue
        {
            get { return _venue; }
            set
            {
                _venue = value;
                this.OnPropertyChanged();
            }
        }

        private string _venueTypeGW;

        public string VenueTypeGW
        {
            get { return _venueTypeGW; }
            set
            {
                _venueTypeGW = value;
                this.OnPropertyChanged();
            }
        }

        private int _sendType;

        public int SendType
        {
            get { return _sendType; }
            set
            {
                _sendType = value;
                this.OnPropertyChanged();
            }
        }

        #region  清算速度 +1 +0 远
        private bool isSendReq = false;
        private const string YYYYMMDD = "yyyyMMdd";
        private readonly string ADD_ONE_NOT_LIMIT = "2,3";

        private string customSettleDate;
        public string CustomSettleDate
        {
            get { return customSettleDate; }
            set
            {
                if (value != null)
                {
                    customSettleDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string addOneShowText = "+1";
        public string AddOneShowText
        {
            get { return addOneShowText; }
            set
            {
                addOneShowText = value;
                OnPropertyChanged(nameof(AddOneShowText));
            }
        }

        /// <summary>
        /// 用于改变当点击 +1时，下拉菜单弹出，需要改变 +1 的上边框颜色，同时用于判断是否选中的 +1 ,用这个值改变默认的背景色
        /// </summary>
        private string addOneOpenedTag = "0";
        public string AddOneOpenedTag
        {
            get { return addOneOpenedTag; }
            set
            {
                addOneOpenedTag = value;
                OnPropertyChanged(nameof(AddOneOpenedTag));
                SettlType_Edit = GetSetttle();
            }
        }

        private bool _addOneNotLimitCheck = true;
        public bool AddOneNotLimitCheck
        {
            get
            {
                return _addOneNotLimitCheck;
            }

            set
            {
                bool oldVal = _addOneNotLimitCheck;
                _addOneNotLimitCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneTodayCheck = _addOneTomorrowCheck = false;
                    this.OnPropertyChanged("AddOneTodayCheck");
                    this.OnPropertyChanged("AddOneTomorrowCheck");
                }
            }
        }

        private bool _addOneTodayCheck = false;
        public bool AddOneTodayCheck
        {
            get
            {
                return _addOneTodayCheck;
            }

            set
            {
                bool oldVal = _addOneTodayCheck;
                _addOneTodayCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTomorrowCheck = false;
                    this.OnPropertyChanged("AddOneNotLimitCheck");
                    this.OnPropertyChanged("AddOneTomorrowCheck");
                }
            }
        }

        private bool _addOneTomorrowCheck = false;
        public bool AddOneTomorrowCheck
        {
            get
            {
                return _addOneTomorrowCheck;
            }

            set
            {
                bool oldVal = _addOneTomorrowCheck;
                _addOneTomorrowCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTodayCheck = false;
                    this.OnPropertyChanged("AddOneNotLimitCheck");
                    this.OnPropertyChanged("AddOneTodayCheck");
                }
            }
        }

        private int settleBoxIndex2 = 0;
        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int SettleBoxIndex2
        {
            get
            {
                return settleBoxIndex2;
            }

            set
            {
                if (settleBoxIndex2 != value)
                {
                    int oldIndex = settleBoxIndex;
                    settleBoxIndex2 = value;
                    if (settleBoxIndex2 >= 0)
                    {
                        AddOneOpenedTag = "1";
                    }

                    if (SettleBoxIndex2 == 0)
                    {
                        AddOneNotLimitCheck = true;
                        AddOneTodayCheck = false;
                        AddOneTomorrowCheck = false;

                    }
                    else if (SettleBoxIndex2 == 1)
                    {
                        AddOneNotLimitCheck = false;
                        AddOneTodayCheck = true;
                        AddOneTomorrowCheck = false;

                    }
                    else if (SettleBoxIndex2 == 2)
                    {
                        AddOneNotLimitCheck = false;
                        AddOneTodayCheck = false;
                        AddOneTomorrowCheck = true;
                    }

                    OnPropertyChanged();

                    SettlType_Edit = GetSetttle();
                }
            }
        }

        private int settleBoxIndex = -1;

        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int SettleBoxIndex
        {
            get
            {
                return settleBoxIndex;
            }

            set
            {
                if (settleBoxIndex != value)
                {
                    int oldIndex = settleBoxIndex;
                    settleBoxIndex = value;

                    this.AddOneOpenedTag = "0";

                    this.OnPropertyChanged();

                    SetOtherVisibile();

                    SettlType_Edit = GetSetttle();
                }

            }
        }

        private Visibility dateVisible = Visibility.Collapsed;
        public Visibility DateVisible
        {
            get { return dateVisible; }
            set
            {
                dateVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility settleTypeVisible = Visibility.Visible;
        public Visibility SettleTypeVisible
        {
            get { return dateVisible; }
            set
            {
                settleTypeVisible = value;
                this.OnPropertyChanged();
            }
        }

        private bool _SettleIsDropDownOpen = false;
        public bool SettleIsDropDownOpen
        {
            get
            {
                return _SettleIsDropDownOpen;
            }

            set
            {
                _SettleIsDropDownOpen = value;
                SettleBoxIndex = -1;
                AddOneOpenedTag = "1";
                this.OnPropertyChanged();

            }
        }

        /// <summary>
        /// 清算时间切换至其他
        /// </summary>
        private void SetOtherVisibile()
        {
            if (SettleBoxIndex == 0 || SettleBoxIndex == -1)
            {
                DateVisible = Visibility.Collapsed;
                SettleTypeVisible = Visibility.Collapsed;

            }
            else if (SettleBoxIndex == 1)
            {
                FwdComboBoxSelectIndex = 0;
                DateVisible = Visibility.Visible;
                SettleTypeVisible = Visibility.Visible;

                try
                {
                    if (string.IsNullOrEmpty(SettlType_Edit))
                    {
                        this.CalBond("4", true);
                    }
                    else
                    {
                        string settle = SettlType_Edit;
                        if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3,2") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                        {
                            this.CalBond("4", true);
                        }
                        else
                        {
                            // 恢复 Settle
                            string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr != null && arr.Length > 1)
                            {
                                if (arr[1].Equals("0"))
                                {
                                    fwdComboBoxSelectIndex = 0;
                                }
                                else if (arr[1].Equals("1"))
                                {
                                    fwdComboBoxSelectIndex = 1;
                                }
                                this.OnPropertyChanged("FwdComboBoxSelectIndex");
                            }
                        }
                    }
                }
                catch (Exception e) { }

            }
        }

        private void CalBond(string settleType, bool isTomorrow)
        {
            if (isSendReq)
                return;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (requst != null)
                    {
                        if (result.Data != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                try
                                {
                                    string year = result.Data.settlementDate.Substring(0, 4);
                                    string month = result.Data.settlementDate.Substring(4, 2);
                                    string day = result.Data.settlementDate.Substring(6, 2);

                                    FwdSelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                    CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                                }
                                catch (Exception e) { }

                                FwdComboBoxSelectIndex = 0;
                                SettlType_Edit = GetSetttle(); //"T+0";

                                isSendReq = false;
                            }));
                        }
                    }
                    else
                        isSendReq = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendReq = false;
                }
            });
        }


        /// <summary>
        /// 解析清算速度
        /// </summary>
        /// <param name="settle"></param>
        private void ParseSettle(string settle)
        {
            //SettleBoxIndex2 = 2;
            //AddOneShowText = "T+1";
            try
            {
                if (!string.IsNullOrEmpty(settle))
                {

                    if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                    {
                        SettleBoxIndex = -1;
                        AddOneNotLimitCheck = false;

                        if (settle.Equals(ADD_ONE_NOT_LIMIT))
                        {
                            SettleBoxIndex2 = 0;
                            AddOneNotLimitCheck = true;
                        }
                        else if (settle.Equals("3"))
                        {
                            SettleBoxIndex2 = 2;
                            AddOneTomorrowCheck = true;
                        }
                        else if (settle.Equals("2"))
                        {
                            SettleBoxIndex2 = 1;
                            AddOneTodayCheck = true;
                        }
                        else if (settle.Equals("+1"))
                        {
                            SettleBoxIndex2 = 0;
                            AddOneNotLimitCheck = true;
                        }
                        else if (settle.Equals("全"))
                        {
                            SettleBoxIndex2 = 0;
                            AddOneNotLimitCheck = true;
                        }
                    }
                    else
                    {
                        //_needCalPrice = false;
                        SettleBoxIndex = -1;
                        AddOneNotLimitCheck = false;
                        if (settle.Equals("1") || settle.Equals("T+0") || settle.Equals("+0"))//T+0
                        {
                            SettleBoxIndex = 0;
                            try
                            {
                                FwdSelectDate = DateTime.Now;
                                CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
                            }
                            catch (Exception e) { }
                            //_needCalPrice = true;
                            //ComboBoxSelectIndex = 0;
                            //SendPriceItem.SettleType = "T+0";
                        }
                        else if (settle.Equals("明天+1") || settle.Equals("4"))
                        {
                            //_needCalPrice = true;
                            this.CalDate("4", false);
                        }
                        else if (settle.Equals("远"))
                        {
                            SettleBoxIndex = 1;
                        }
                        else
                        {
                            string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            int month = 0;
                            int day = 0;
                            if (arr != null && arr.Length > 1)
                            {
                                SettleBoxIndex = 1;
                                string[] dtStr = arr[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                if (dtStr != null && dtStr.Length > 1
                                            && Int32.TryParse(dtStr[0], out month) && Int32.TryParse(dtStr[1], out day))
                                {
                                    DateTime dt = new DateTime(DateTime.Now.Year, month, day);
                                    if (dt < DateTime.Now)
                                    {
                                        dt = new DateTime(DateTime.Now.Year, month, day).AddYears(1);
                                    }

                                    try
                                    {
                                        FwdSelectDate = new DateTime?(dt);
                                        CustomSettleDate = dt.ToString("yyy-MM-dd");
                                    }
                                    catch (Exception e) { }
                                }
                                //_needCalPrice = true;
                                if (arr[1].Equals("0"))
                                {
                                    FwdComboBoxSelectIndex = 0;
                                    SettlType_Edit = GetSetttle(); //"T+0";
                                }
                                else if (arr[1].Equals("1"))
                                {
                                    FwdComboBoxSelectIndex = 1;
                                    SettlType_Edit = GetSetttle(); //"T+1";
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {

            }
            finally
            {

            }
        }

        private bool isSendCalDate = false;
        /// <summary>
        /// 计算清算时间其他模式下的日期
        /// </summary>
        /// <param name="settleType"></param>
        /// <param name="isTomorrow"></param>
        private void CalDate(string settleType, bool isTomorrow)
        {
            if (isSendCalDate)
                return;

            isSendCalDate = true;
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
                                FwdSelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                            }
                            catch (Exception e) { }

                            FwdComboBoxSelectIndex = isTomorrow ? 0 : 1;
                            SettlType_Edit = GetSetttle(); //isTomorrow ? "T+0" : "T+1";

                            isSendCalDate = false;
                        }));
                    }
                    else
                        isSendCalDate = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendCalDate = false;
                }
            });
        }

        private int fwdComboBoxSelectIndex = 0;

        public int FwdComboBoxSelectIndex
        {
            get
            {
                return fwdComboBoxSelectIndex;
            }
            set
            {
                int oldIndex = fwdComboBoxSelectIndex;

                fwdComboBoxSelectIndex = value;

                GetSettleTypeFromServer(GetSetttle());

                this.OnPropertyChanged();
            }
        }

        private int addOneSettleBoxIndex = 0;
        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int AddOneSettleBoxIndex
        {
            get
            {
                return addOneSettleBoxIndex;
            }

            set
            {
                int oldIndex = settleBoxIndex;
                addOneSettleBoxIndex = value;
                if (addOneSettleBoxIndex >= 0)
                {
                }

                if (addOneSettleBoxIndex == 0)
                {
                }
                else if (addOneSettleBoxIndex == 1)
                {
                }
                else if (addOneSettleBoxIndex == 2)
                {
                }

                OnPropertyChanged();
            }
        }

        private DateTime? fwdSelectDate;
        public DateTime? FwdSelectDate
        {
            get { return fwdSelectDate; }
            set
            {
                string oldDate = fwdSelectDate == null ? string.Empty : fwdSelectDate.Value.Date.ToString(YYYYMMDD);
                fwdSelectDate = value;
                string newDate = fwdSelectDate == null ? string.Empty : fwdSelectDate.Value.Date.ToString(YYYYMMDD);

                GetSettleTypeFromServer(GetSetttle());

                this.OnPropertyChanged();

                if (value != null && !oldDate.Equals(newDate))
                {
                }
            }
        }

        /// <summary>
        /// 获得清算速度
        /// </summary>
        /// <returns></returns>
        private string GetSetttle(bool isfilter = false)
        {
            string settle = string.Empty;

            //默认+1选择
            if (SettleBoxIndex == -1)
            {
                if (AddOneOpenedTag == "1")
                {
                    settle = AddOneNotLimitCheck ? ADD_ONE_NOT_LIMIT : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : ADD_ONE_NOT_LIMIT;
                }
            }
            else if (SettleBoxIndex == 0)
            {
                //选择+0
                settle = "1";
            }
            else
            {
                // 如果不是获取最优价
                if (!isfilter)
                {
                    if (FwdSelectDate != null && FwdSelectDate.HasValue)
                    {
                        settle = FwdSelectDate.Value.ToString("MM.dd") + (FwdComboBoxSelectIndex == 0 ? "+0" : "+1");
                    }
                }
                else
                {
                    settle = "4"; //远期
                }
            }

            return settle;
        }

        /// <summary>
        /// 计算正确的清算速度
        /// </summary>
        /// <param name="settleType">明天+0 ，6.15+0 </param>
        private void GetSettleTypeFromServer(string settleType)
        {
            if (isSendReq)
                return;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (requst != null && result != null)
                    {
                        if (result.Data != null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(result.Data.settlementType2))
                                {
                                    SettlType_Edit = result.Data.settlementType;
                                }
                                else
                                {
                                    SettlType_Edit = result.Data.settlementType2;
                                }
                            }
                            catch (Exception e) { }
                        }
                        isSendReq = false;
                    }
                    else
                        isSendReq = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendReq = false;
                }
            });
        }

        #endregion

        #region 清算速度逻辑
        //selectSettle:T+0:0, T+1:1
        public bool IsSpotSettle(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string now = DateTime.Now.ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                DateTime dtSelect = new DateTime(selectDate.Value.Year, selectDate.Value.Month, selectDate.Value.Day);

                int days = (dtSelect - dtNow).Days;

                if ((days == 0 && (selectSettle == 0 || selectSettle == 1)) || (days == 1 && selectSettle == 0))
                    result = true;
            }

            return result;
        }

        public bool IsTomorrow(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string now = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                string tem = DateTime.Now.ToString("yyyyMMdd");

                //明天+0=T+1
                if ((now.CompareTo(select) == 0 && selectSettle == 0) || (tem.CompareTo(select) == 0 && selectSettle == 1))
                    result = true;

            }

            return result;
        }

        public bool IsFwdSettle(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                TimeSpan ts = selectDate.Value - DateTime.Now;

                int days = ts.Days;

                if ((days == 1 || ts.TotalMinutes > 0) && selectSettle == 1)
                {
                    result = true;
                }
                else if ((days > 1) || (days == 1 && ts.TotalMinutes > 0))
                    result = true;
            }

            return result;
        }

        #endregion


        private string _percentDisplay;

        public string PercentDisplay
        {
            get { return _percentDisplay; }
            set
            {
                _percentDisplay = value;
                this.OnPropertyChanged();
            }
        }

        private string _priceType_Edit;

        public string PriceType_Edit
        {
            get { return _priceType_Edit; }
            set
            {
                _priceType_Edit = value;
                this.OnPropertyChanged();
                if (_priceType_Edit == "3" || _priceType_Edit == "7")
                {
                    PercentDisplay = "%";
                }
                else
                {
                    PercentDisplay = string.Empty;
                }
            }
        }

        // 价格类型
        private string _priceType;

        public string PriceType
        {
            get { return _priceType; }
            set
            {
                _priceType = value;
                this.OnPropertyChanged();
            }
        }

        #region 报价标准
        private ObservableCollection<KeyValuePair<string, string>> _priceType_Dic = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> PriceTypeDic
        {
            get
            {
                if (_priceType_Dic.Count == 0)
                {
                    _priceType_Dic.Add(new KeyValuePair<string, string>("1", "净价"));//["1"] = "净价";
                    //_priceType_Dic.Add(new KeyValuePair<string, string>("2", "全价"));//["2"] = "全价";
                    _priceType_Dic.Add(new KeyValuePair<string, string>("3", "到期"));//["3"] = "到期收益率";
                    _priceType_Dic.Add(new KeyValuePair<string, string>("7", "行权"));//["7"] = "行权收益率";
                }
                return _priceType_Dic;
            }
        }
        #endregion

        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; this.OnPropertyChanged(); }
        }

        private decimal _remaingingTime_convert;
        public decimal RemaingingTime_convert
        {
            get { return _remaingingTime_convert; }
            set { _remaingingTime_convert = value; this.OnPropertyChanged(); }
        }

        public IntentionQuoteItem intentionItem;

        /// <summary>
        /// 交易员 ID（STC userName)
        /// </summary>
        private string _trader_buyer;

        public string Trader_Buyer
        {
            get { return _trader_buyer; }
            set
            {
                _trader_buyer = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 交易员名称 (中文名称)
        /// </summary>
        private string _traderName_buyer;

        public string TraderName_Buyer
        {
            get { return _traderName_buyer; }
            set
            {
                _traderName_buyer = value;
                this.OnPropertyChanged();
            }
        }

        private string _groupKey;

        public string GroupKey
        {
            get { return _groupKey; }
            set
            {
                _groupKey = value;
                this.OnPropertyChanged();
            }
        }

        private string _type;

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                this.OnPropertyChanged();
            }
        }

        private object _tempSort;

        public object TempSort
        {
            get { return _tempSort; }
            set
            {
                _tempSort = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isEdit;

        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                _isEdit = value;
                this.OnPropertyChanged();
            }
        }

        private string _bondType;

        public string BondType
        {
            get { return _bondType; }
            set
            {
                _bondType = value;
                this.OnPropertyChanged();
            }
        }
    }
}

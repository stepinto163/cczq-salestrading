using netapi.Api;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STC.Common.Model.Fix.MessageDictionary.FieldsEnum;

namespace STC.PT.Plugin.Model
{
    public class PtExecordersData : BaseViewModel
    {
        private string _MarketIndicator;
        private string _ClOrdID;
        private string _ExecID;
        private string _Matchstatus;
        private string _Relatestatus;
        private string _Sendstatus;
        private string _Securityid;
        private string _Symbol;
        private string _Symboltype;
        private string _Tag1;
        private string _Venuetypegw;
        private string _Venues;
        private string _Side;
        private string _LastPx;
        private string _LastPx_Display;
        private string _NetPrice;
        private string _LastQty;
        private string _TradeDate;
        private string _ExecutionTime;
        private string _SettlType;
        private string _StrategyOrderId;
        private string _InstitutionName;
        private string _InstitutionID;
        private string _TraderName;
        private string _TraderID;
        private string _Seatnumber;
        private string _Quotemethod;
        private string _CounterPartyName;
        private string _CounterPartySubName;
        private string _PartySubName;
        private string _AccountId;
        private string _AccountName;
        private string _Venueexecid;
        private string _Venueid;
        private string _UpdateTime;
        private string _ShowDeleteBtn;
        private string _RemainingTime;
        private string _Approvalstatus;
        private int _MaturityDays;
        private bool _IsMe;
        private long _Sign;
        private long _Sign1;
        private long _Sign2;
        private long _Sign3;
        private string _Remark;
        private string _Yieldtoexecution;//行权收益率
        private string _FullPrice;//全价
        private string _Realcounterpartyname;//终端交易对手
        private string _TrdSubType;//发单方向
        private string _TrdType;//发单方式
        private decimal _SettlAmount;//结算金额？
        private string _BridgePaymentFixedRate;//DF费率
        private decimal _Yield2;//费后收益率
        private decimal _CleanPrice2;//费后净价
        private decimal _FullPrice2;//费后全价
        private string _TrdMatchId;//约定号
        public string TrdMatchId
        {
            get
            {
                return _TrdMatchId;
            }
            set
            {
                _TrdMatchId = value;
                this.OnPropertyChanged();
            }
        }
        private string _Couponrate;
        public string Couponrate
        {
            get
            {
                return _Couponrate;
            }
            set
            {
                _Couponrate = value;
                this.OnPropertyChanged();
            }
        }
        public decimal CleanPrice2
        {
            get
            {
                return _CleanPrice2;
            }
            set
            {
                _CleanPrice2 = value;
                this.OnPropertyChanged();
            }
        }
        public decimal FullPrice2
        {
            get
            {
                return _FullPrice2;
            }
            set
            {
                _FullPrice2 = value;
                this.OnPropertyChanged();
            }
        }
        public decimal Yield2
        {
            get
            {
                return _Yield2;
            }

            set
            {
                _Yield2 = value;

                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 费后行权收益率
        /// </summary>
        private decimal _Yieldtoexecution2;
        public decimal Yieldtoexecution2
        {
            get
            {
                return _Yieldtoexecution2;
            }

            set
            {
                _Yieldtoexecution2 = value;

                this.OnPropertyChanged();
            }
        }
        public string BridgePaymentFixedRate
        {
            get
            {
                return _BridgePaymentFixedRate;
            }

            set
            {
                _BridgePaymentFixedRate = value;

                this.OnPropertyChanged();
            }
        }
        public decimal SettlAmount
        {
            get
            {
                return _SettlAmount;
            }

            set
            {
                _SettlAmount = value;

                this.OnPropertyChanged();
            }
        }
        public string TrdType
        {
            get
            {
                return _TrdType;
            }

            set
            {
                _TrdType = value;

                this.OnPropertyChanged();
            }
        }
        public string TrdSubType
        {
            get
            {
                return _TrdSubType;
            }

            set
            {
                _TrdSubType = value;

                this.OnPropertyChanged();
            }
        }
        public string Realcounterpartyname
        {
            get
            {
                return _Realcounterpartyname;
            }

            set
            {
                _Realcounterpartyname = value;

                this.OnPropertyChanged();
            }
        }
        public string FullPrice
        {
            get
            {
                return _FullPrice;
            }

            set
            {
                _FullPrice = value;

                this.OnPropertyChanged();
            }
        }
        public string Yieldtoexecution
        {
            get
            {
                return _Yieldtoexecution;
            }

            set
            {
                _Yieldtoexecution = value;

                this.OnPropertyChanged();
            }
        }
        public string Remark
        {
            get
            {
                return _Remark;
            }

            set
            {
                _Remark = value;

                this.OnPropertyChanged();
            }
        }
        public string Quotemethod
        {
            get
            {
                return _Quotemethod;
            }

            set
            {
                _Quotemethod = value;

                this.OnPropertyChanged();
            }
        }
        public string Seatnumber
        {
            get
            {
                return _Seatnumber;
            }

            set
            {
                _Seatnumber = value;

                this.OnPropertyChanged();
            }
        }
        public string TraderID
        {
            get
            {
                return _TraderID;
            }

            set
            {
                _TraderID = value;

                this.OnPropertyChanged();
            }
        }
        public string TraderName
        {
            get
            {
                return _TraderName;
            }

            set
            {
                _TraderName = value;

                this.OnPropertyChanged();
            }
        }
        public string InstitutionID
        {
            get
            {
                return _InstitutionID;
            }

            set
            {
                _InstitutionID = value;

                this.OnPropertyChanged();
            }
        }
        public string InstitutionName
        {
            get
            {
                return _InstitutionName;
            }

            set
            {
                _InstitutionName = value;

                this.OnPropertyChanged();
            }
        }
        public string SettlType
        {
            get
            {
                return _SettlType;
            }

            set
            {
                _SettlType = value;

                this.OnPropertyChanged();
            }
        }
        public string LastQty
        {
            get
            {
                return _LastQty;
            }

            set
            {
                _LastQty = value;

                this.OnPropertyChanged();
            }
        }
        public string NetPrice
        {
            get
            {
                return _NetPrice;
            }

            set
            {
                _NetPrice = value;

                this.OnPropertyChanged();
            }
        }
        public string LastPx
        {
            get
            {
                return _LastPx;
            }

            set
            {
                _LastPx = value;

                this.OnPropertyChanged();
            }
        }
        public string Side
        {
            get
            {
                return _Side;
            }

            set
            {
                _Side = value;

                this.OnPropertyChanged();
            }
        }
        public string TradeDate
        {
            get
            {
                return _TradeDate;
            }

            set
            {
                _TradeDate = value;

                this.OnPropertyChanged();
            }
        }
        public DateTime TradeDated
        {
            get { return DateTime.ParseExact(TradeDate, "yyyyMMdd", CultureInfo.InvariantCulture); }
        }

        public string Symbol
        {
            get
            {
                return _Symbol;
            }

            set
            {
                _Symbol = value;

                this.OnPropertyChanged();
            }
        }
        public string SecurityID
        {
            get
            {
                return _Securityid;
            }

            set
            {
                _Securityid = value;

                this.OnPropertyChanged();
            }
        }
        public string ExecID
        {
            get
            {
                return _ExecID;
            }

            set
            {
                _ExecID = value;

                this.OnPropertyChanged();
            }
        }

        public string ClOrdID
        {
            get { return _ClOrdID; }
            set
            {
                _ClOrdID = value;

                this.OnPropertyChanged();
            }
        }
        public string Matchstatus
        {
            get
            {
                return _Matchstatus;
            }
            set
            {
                _Matchstatus = value;

                this.OnPropertyChanged();
            }
        }
        public string Relatestatus
        {
            get
            {
                return _Relatestatus;
            }
            set
            {
                _Relatestatus = value;

                this.OnPropertyChanged();
            }
        }
        public string Sendstatus
        {
            get
            {
                return _Sendstatus;
            }
            set
            {
                _Sendstatus = value;

                this.OnPropertyChanged();
            }
        }
        public string Symboltype
        {
            get
            {
                return _Symboltype;
            }
            set
            {
                _Symboltype = value;

                this.OnPropertyChanged();
            }
        }
        public string Tag1
        {
            get
            {
                return _Tag1;
            }
            set
            {
                _Tag1 = value;

                this.OnPropertyChanged();
            }
        }
        public string Venuetypegw
        {
            get
            {
                return _Venuetypegw;
            }
            set
            {
                _Venuetypegw = value;

                this.OnPropertyChanged();
            }
        }
        public string Venues
        {
            get
            {
                return _Venues;
            }
            set
            {
                _Venues = value;

                this.OnPropertyChanged();
            }
        }
        public string LastPx_Display
        {
            get
            {
                return _LastPx_Display;
            }
            set
            {
                _LastPx_Display = value;

                this.OnPropertyChanged();
            }
        }
        public string ExecutionTime
        {
            get
            {
                return _ExecutionTime;
            }
            set
            {
                _ExecutionTime = value;

                this.OnPropertyChanged();
            }
        }
        public string StrategyOrderId
        {
            get
            {
                return _StrategyOrderId;
            }
            set
            {
                _StrategyOrderId = value;

                this.OnPropertyChanged();
            }
        }
        private string _CounterPartyId;
        public string CounterPartyId
        {
            get
            {
                return _CounterPartyId;
            }
            set
            {
                _CounterPartyId = value;

                this.OnPropertyChanged();
            }
        }
        public string CounterPartyName
        {
            get
            {
                return _CounterPartyName;
            }
            set
            {
                _CounterPartyName = value;

                this.OnPropertyChanged();
            }
        }

        private string _Counterpartyfullname;
        public string Counterpartyfullname
        {
            get
            {
                return _Counterpartyfullname;
            }
            set
            {
                _Counterpartyfullname = value;

                this.OnPropertyChanged();
            }
        }
        private string _CounterPartySubId;
        public string CounterPartySubId
        {
            get
            {
                return _CounterPartySubId;
            }
            set
            {
                _CounterPartySubId = value;

                this.OnPropertyChanged();
            }
        }
        public string CounterPartySubName
        {
            get
            {
                return _CounterPartySubName;
            }
            set
            {
                _CounterPartySubName = value;

                this.OnPropertyChanged();
            }
        }
        private string _PartySubId;
        public string PartySubId
        {
            get 
            { 
                return _PartySubId;
            }
            set 
            {
                _PartySubId = value;
                this.OnPropertyChanged();
            }
        }
        public string PartySubName
        {
            get
            {
                return _PartySubName;
            }
            set
            {
                _PartySubName = value;

                this.OnPropertyChanged();
            }
        }
        public string AccountId
        {
            get
            {
                return _AccountId;
            }
            set
            {
                _AccountId = value;

                this.OnPropertyChanged();
            }
        }
        public string AccountName
        {
            get
            {
                return _AccountName;
            }
            set
            {
                _AccountName = value;

                this.OnPropertyChanged();
            }
        }
        public string Venueexecid
        {
            get { return _Venueexecid; }
            set
            {
                _Venueexecid = value;

                this.OnPropertyChanged();
            }
        }
        public string Venueid
        {
            get { return _Venueid; }
            set
            {
                _Venueid = value;

                this.OnPropertyChanged();
            }
        }
        public string UpdateTime
        {
            get { return _UpdateTime; }
            set
            {
                _UpdateTime = value;

                this.OnPropertyChanged();
            }
        }
        public string ShowDeleteBtn
        {
            get { return _ShowDeleteBtn; }
            set
            {
                _ShowDeleteBtn = value;

                this.OnPropertyChanged();
            }
        }
        public string RemainingTime
        {
            get { return _RemainingTime; }
            set
            {
                _RemainingTime = value;

                this.OnPropertyChanged();
            }
        }
        private decimal _RemaingingTime_convert;
        public decimal RemaingingTime_convert
        {
            get { return _RemaingingTime_convert; }
            set 
            { 
                _RemaingingTime_convert = value; 
                this.OnPropertyChanged(); 
            }
        }
        //1-新建 2-审批中 3-审批拒绝 4-审批通过 5-交易执行中 6-部分撤销 7-已撤销 8-交易终止 9-成交确认
        public string Approvalstatus
        {
            get { return _Approvalstatus; }
            set
            {
                _Approvalstatus = value;

                this.OnPropertyChanged();
            }
        }
        public int MaturityDays
        {
            get { return _MaturityDays; }
            set
            {
                _MaturityDays = value;

                this.OnPropertyChanged();
            }
        }
        public bool IsMe
        {
            get { return _IsMe; }
            set
            {
                _IsMe = value;

                this.OnPropertyChanged();
            }
        }
        #region sign
        public long Sign
        {
            get { return _Sign; }
            set
            {
                _Sign = value;

                _signDic = ParseSign("sign", value);
                //this.OnPropertyChanged();
            }
        }
        private Dictionary<string, string> _signDic;

        public Dictionary<string, string> SignDic
        {
            get { return _signDic; }
            set
            {
                if (_signDic != value)
                {
                    _signDic = value;
                }
            }
        }

        private long _sign_InnerBid;

        public long Sign_InnerBid
        {
            get { return _sign_InnerBid; }
            set
            {
                if (_sign_InnerBid != value || _signDic_InnerBid == null)
                {
                    _sign_InnerBid = value;
                    _signDic_InnerBid = ParseSign("sign", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerBid;

        public Dictionary<string, string> SignDic_InnerBid
        {
            get { return _signDic_InnerBid; }
            set
            {
                if (_signDic_InnerBid != value)
                {
                    _signDic_InnerBid = value;
                }
            }
        }

        private long _sign_InnerOfr;

        public long Sign_InnerOfr
        {
            get { return _sign_InnerOfr; }
            set
            {
                if (_sign_InnerOfr != value || _signDic_InnerOfr == null)
                {
                    _sign_InnerOfr = value;
                    _signDic_InnerOfr = ParseSign("sign", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr;

        public Dictionary<string, string> SignDic_InnerOfr
        {
            get { return _signDic_InnerOfr; }
            set
            {
                if (_signDic_InnerOfr != value)
                {
                    _signDic_InnerOfr = value;
                }
            }
        }
        #endregion
        private Dictionary<string, string> ParseSign(string signField, long sign)
        {
            Dictionary<string, QuoteTag> quoteTagDic = BaseHandler.Instance.GetQuoteTagDic();
            string a = CommonUtils.ToBinary(sign);
            if (!string.IsNullOrEmpty(a))
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, QuoteTag> pair in quoteTagDic)
                {
                    if (pair.Value.signField == signField)
                    {
                        dic[pair.Value.tagType] = a.Substring(pair.Value.tagTypeStartIndex, pair.Value.tagTypeLength);
                    }
                }
                return dic;
            }
            return null;
        }
        #region SIGN1
        public long Sign1
        {
            get { return _Sign1; }
            set
            {
                if (_Sign1 != value || _signDic1 == null)
                {
                    _Sign1 = value;
                    _signDic1 = ParseSign("sign1", value);
                }
            }
        }
        private Dictionary<string, string> _signDic1;

        public Dictionary<string, string> SignDic1
        {
            get { return _signDic1; }
            set
            {
                if (_signDic1 != value)
                {
                    _signDic1 = value;
                }
            }
        }
        private long _sign_InnerBid1;

        public long Sign_InnerBid1
        {
            get { return _sign_InnerBid1; }
            set
            {
                if (_sign_InnerBid1 != value || _signDic_InnerBid1 == null)
                {
                    _sign_InnerBid1 = value;
                    _signDic_InnerBid1 = ParseSign("sign1", value);
                }
            }
        }
        private Dictionary<string, string> _signDic_InnerBid1;

        public Dictionary<string, string> SignDic_InnerBid1
        {
            get { return _signDic_InnerBid1; }
            set
            {
                if (_signDic_InnerBid1 != value)
                {
                    _signDic_InnerBid1 = value;
                }
            }
        }
        private long _sign_InnerOfr1;

        public long Sign_InnerOfr1
        {
            get { return _sign_InnerOfr1; }
            set
            {
                if (_sign_InnerOfr1 != value || _signDic_InnerOfr1 == null)
                {
                    _sign_InnerOfr1 = value;
                    _signDic_InnerOfr1 = ParseSign("sign1", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr1;

        public Dictionary<string, string> SignDic_InnerOfr1
        {
            get { return _signDic_InnerOfr1; }
            set
            {
                if (_signDic_InnerOfr1 != value)
                {
                    _signDic_InnerOfr1 = value;
                }
            }
        }
        #endregion
        #region Sign2
        public long Sign2
        {
            get { return _Sign2; }
            set
            {
                if (_Sign2 != value || _signDic2 == null)
                {
                    _Sign2 = value;
                    _signDic2 = ParseSign("sign2", value);
                }
            }
        }

        private Dictionary<string, string> _signDic2;

        public Dictionary<string, string> SignDic2
        {
            get { return _signDic2; }
            set
            {
                if (_signDic2 != value)
                {
                    _signDic2 = value;
                }
            }
        }

        private long _sign_InnerBid2;

        public long Sign_InnerBid2
        {
            get { return _sign_InnerBid2; }
            set
            {
                if (_sign_InnerBid2 != value || _signDic_InnerBid2 == null)
                {
                    _sign_InnerBid2 = value;
                    _signDic_InnerBid2 = ParseSign("sign2", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerBid2;

        public Dictionary<string, string> SignDic_InnerBid2
        {
            get { return _signDic_InnerBid2; }
            set
            {
                if (_signDic_InnerBid2 != value)
                {
                    _signDic_InnerBid2 = value;
                }
            }
        }

        private long _sign_InnerOfr2;

        public long Sign_InnerOfr2
        {
            get { return _sign_InnerOfr2; }
            set
            {
                if (_sign_InnerOfr2 != value || _signDic_InnerOfr2 == null)
                {
                    _sign_InnerOfr2 = value;
                    _signDic_InnerOfr2 = ParseSign("sign2", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr2;

        public Dictionary<string, string> SignDic_InnerOfr2
        {
            get { return _signDic_InnerOfr2; }
            set
            {
                if (_signDic_InnerOfr2 != value)
                {
                    _signDic_InnerOfr2 = value;
                }
            }
        }
        #endregion
        #region sign3
        public long Sign3
        {
            get { return _Sign3; }
            set
            {
                if (_Sign3 != value || _signDic3 == null)
                {
                    _Sign3 = value;
                    _signDic3 = ParseSign("sign3", value);
                }
            }
        }

        private Dictionary<string, string> _signDic3;

        public Dictionary<string, string> SignDic3
        {
            get { return _signDic3; }
            set
            {
                if (_signDic3 != value)
                {
                    _signDic3 = value;
                }
            }
        }

        private long _sign_InnerBid3;

        public long Sign_InnerBid3
        {
            get { return _sign_InnerBid3; }
            set
            {
                if (_sign_InnerBid3 != value || _signDic_InnerBid3 == null)
                {
                    _sign_InnerBid3 = value;
                    _signDic_InnerBid3 = ParseSign("sign3", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerBid3;

        public Dictionary<string, string> SignDic_InnerBid3
        {
            get { return _signDic_InnerBid3; }
            set
            {
                if (_signDic_InnerBid3 != value)
                {
                    _signDic_InnerBid3 = value;
                }
            }
        }

        private long _sign_InnerOfr3;

        public long Sign_InnerOfr3
        {
            get { return _sign_InnerOfr3; }
            set
            {
                if (_sign_InnerOfr3 != value || _signDic_InnerOfr3 == null)
                {
                    _sign_InnerOfr3 = value;
                    _signDic_InnerOfr3 = ParseSign("sign3", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr3;

        public Dictionary<string, string> SignDic_InnerOfr3
        {
            get { return _signDic_InnerOfr3; }
            set
            {
                if (_signDic_InnerOfr3 != value)
                {
                    _signDic_InnerOfr3 = value;
                }
            }
        }
        #endregion
        private string _issuerCode;
        public string IssuerCode
        {
            get { return _issuerCode; }
            set { _issuerCode = value; this.OnPropertyChanged(); }
        }
        private string _issuerName;
        public string IssuerName
        {
            get { return _issuerName; }
            set { _issuerName = value; this.OnPropertyChanged(); }
        }
        private string _warranterCode;

        public string WarranterCode
        {
            get { return _warranterCode; }
            set { _warranterCode = value; this.OnPropertyChanged(); }
        }
        private string _warranterName;
        public string WarranterName
        {
            get { return _warranterName; }
            set { _warranterName = value; this.OnPropertyChanged(); }
        }
        private string _dayOffStr;

        public string DayOffStr
        {
            get { return _dayOffStr; }
            set
            {
                _dayOffStr = value;
                this.OnPropertyChanged();
            }
        }
        private string _ShowSendBtn;
        public string ShowSendBtn
        {
            get { return _ShowSendBtn; }
            set
            {
                _ShowSendBtn = value;

                this.OnPropertyChanged();
            }
        }

        private string _ShowSendBtn_FQ;
        public string ShowSendBtn_FQ
        {
            get { return _ShowSendBtn_FQ; }
            set
            {
                _ShowSendBtn_FQ = value;

                this.OnPropertyChanged();
            }
        }

        private string _ShowEditBtn;
        public string ShowEditBtn
        {
            get { return _ShowEditBtn; }
            set
            {
                _ShowEditBtn = value;

                this.OnPropertyChanged();
            }
        }
        private string _ShowHistoryBtn;
        public string ShowHistoryBtn
        {
            get { return _ShowHistoryBtn; }
            set
            {
                _ShowHistoryBtn = value;
                this.OnPropertyChanged();
            }
        }
        private string _Approvalorderid;
        public string Approvalorderid
        {
            get { return _Approvalorderid; }
            set { _Approvalorderid = value; this.OnPropertyChanged(); }
        }

        private string _settldate;
        public string Settldate
        {
            get { return _settldate; }
            set
            {
                _settldate = value;
                this.OnPropertyChanged();
            }
        }
        private string _Approvaltime;
        public string Approvaltime
        {
            get { return _Approvaltime; }
            set
            {
                _Approvaltime = value;
                this.OnPropertyChanged();
            }
        }
        private string _Transacttime;
        public string Transacttime
        {
            get { return _Transacttime; }
            set
            {
                _Transacttime = value;
                this.OnPropertyChanged();
            }
        }
        private string _Approvalterminal;
        public string Approvalterminal
        {
            get { return _Approvalterminal; }
            set
            {
                _Approvalterminal = value;
                this.OnPropertyChanged();
            }
        }
        public PtExecordersData Copy()
        {
            return new PtExecordersData
            {
                ClOrdID = this.ClOrdID,
                ExecID = this.ExecID,
                Matchstatus = this.Matchstatus,
                Relatestatus = this.Relatestatus,
                Sendstatus = this.Sendstatus,
                SecurityID = this.SecurityID,
                Symbol = this.Symbol,
                Symboltype = this.Symboltype,
                Tag1 = this.Tag1,
                Venuetypegw = this.Venuetypegw,
                Venues = this.Venues,
                Side = this.Side,
                LastPx = this.LastPx,
                LastPx_Display = this.LastPx_Display,
                NetPrice = this.NetPrice,
                LastQty = this.LastQty,
                TradeDate = this.TradeDate,
                ExecutionTime = this.ExecutionTime,
                SettlType = this.SettlType,
                StrategyOrderId = this.StrategyOrderId,
                InstitutionName = this.InstitutionName,
                InstitutionID = this.InstitutionID,
                TraderName = this.TraderName,
                TraderID = this.TraderID,
                Seatnumber = this.Seatnumber,
                Quotemethod = this.Quotemethod,
                CounterPartyId = this.CounterPartyId,
                CounterPartyName = this.CounterPartyName,
                CounterPartySubId = this.CounterPartySubId,
                CounterPartySubName = this.CounterPartySubName,
                PartySubName = this.PartySubName,
                AccountId = this.AccountId,
                AccountName = this.AccountName,
                Venueexecid = this.Venueexecid,
                Venueid = this.Venueid,
                UpdateTime = this.UpdateTime,
                ShowDeleteBtn = this.ShowDeleteBtn,
                RemainingTime = this.RemainingTime,
                Approvalorderid = this.Approvalorderid,
                Approvalstatus = this.Approvalstatus,
                Approvaltime = this.Approvaltime,
                Settldate = this.Settldate,
                MaturityDays = this.MaturityDays,
                IsMe = this.IsMe,
                Sign = this.Sign,
                Sign1 = this.Sign1,
                Sign2 = this.Sign2,
                Sign3 = this.Sign3,
                Remark = this.Remark,
                IssuerCode = this.IssuerCode,
                WarranterCode = this.WarranterCode,
                DayOffStr = this.DayOffStr,
                Createtime = this.Createtime,
                BusinessType = this.BusinessType,
                Bookid = this.Bookid,
                BookName = this.BookName,
                Username = this.Username,
                Approvalterminal = this.Approvalterminal,
                MarketIndicator = this.MarketIndicator,
                Offsetflag = this.Offsetflag,   
                CloseBy = this.CloseBy, 
                Operationtype = this.Operationtype, 
                Delflag = this.Delflag,
                Entrytype  = this.Entrytype,
                MatchQty = this.MatchQty,   
                Riskflag = this.Riskflag,
                RiskRemark = this.RiskRemark,
                AllocText = this.AllocText,
                Partymanagerid = this.Partymanagerid,
                Partymanagername = this.Partymanagername,
                Sseinstitutioncode = this.Sseinstitutioncode,
                Szedealercode = this.Szedealercode,
                Szetradingentitycode = this.Szetradingentitycode,
                Szetradercode = this.Szetradercode,
                Yieldtoexecution = this.Yieldtoexecution
            };
        }

        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                _IsChecked = value;
                this.OnPropertyChanged();
            }
        }
        private string _IsCurrentDate;
        public string IsCurrentDate
        {
            get { return _IsCurrentDate; }
            set
            {
                _IsCurrentDate = value;
                this.OnPropertyChanged();
            }
        }
        private string _createtime;
        /// <summary>
        /// 创建时间
        /// </summary>
        public string Createtime
        {
            get { return _createtime; }
            set
            {
                _createtime = value;
                this.OnPropertyChanged();
            }
        }

        private string _blocknum;
        /// <summary>
        /// 交易包号
        /// </summary>
        public string Blocknum
        {
            get { return _blocknum; }
            set
            {
                _blocknum = value;
                this.OnPropertyChanged();
            }
        }
        private string _businessType;
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType
        {
            get { return _businessType; }
            set
            {
                _businessType = value;
                this.OnPropertyChanged();
            }
        }
        private string _bookid;
        /// <summary>
        /// 组合id
        /// </summary>
        public string Bookid
        {
            get { return _bookid; }
            set
            {
                _bookid = value;
                this.OnPropertyChanged();
            }
        }
        private string _bookName;
        /// <summary>
        /// 组合名称
        /// </summary>
        public string BookName
        {
            get { return _bookName; }
            set
            {
                _bookName = value;
                this.OnPropertyChanged();
            }
        }
        private string _username;
        /// <summary>
        /// stc用户
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value;
                this.OnPropertyChanged();
            }

        }
        public string MarketIndicator
        {
            get { return _MarketIndicator; }
            set { _MarketIndicator = value;
                this.OnPropertyChanged();
            }

        }
        private string _Offsetflag;
        public string Offsetflag
        {
            get { return _Offsetflag; }
            set { _Offsetflag = value;
                this.OnPropertyChanged();
            }

        }

        public string _CloseBy;
        public string CloseBy
        {
            get
            {
                return _CloseBy;
            }
            set
            {
                _CloseBy = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 操作类型
        /// </summary>
        public string _Operationtype;
        public string Operationtype
        {
            get
            {
                return _Operationtype;
            }
            set
            {
                _Operationtype = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 状态
        /// </summary>
        public string _Delflag;
        public string Delflag
        {
            get
            {
                return _Delflag;
            }
            set
            {
                _Delflag = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 录入方式 0-自动录入 1-手工录入
        /// </summary>
        private string _Entrytype;
        public string Entrytype
        {
            get { return _Entrytype; }
            set {
                _Entrytype = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 已匹配个需量(万)
        /// </summary>
        private string _MatchQty;
        public string MatchQty
        {
            get { return _MatchQty; }
            set
            {
                _MatchQty = value;
                this.OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// 风控检查标记
        /// </summary>
        private string _Riskflag;
        public string Riskflag
        {
            get { return _Riskflag; }
            set
            {
                _Riskflag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 风控检查备注
        /// </summary>
        private string _RiskRemark;
        public string RiskRemark
        {
            get { return _RiskRemark; }
            set
            {
                _RiskRemark = value;
                this.OnPropertyChanged();
            }
        }
        private string _ID;
        public string ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                this.OnPropertyChanged();
            }
        }
        private string _ShowRelationBtn;
        public string ShowRelationBtn
        {
            get { return _ShowRelationBtn; }
            set
            {
                _ShowRelationBtn = value;
                this.OnPropertyChanged();
            }
        }

        private string _Matchdetail;//匹配详情
        public string Matchdetail
        {
            get
            {
                return _Matchdetail;
            }
            set
            {
                _Matchdetail = value;
                this.OnPropertyChanged();
                if (string.IsNullOrEmpty(_Matchdetail))
                {
                    matchdetailPos = new List<MatchdetailPo>();
                }
                else
                {
                    //List<TbookPtExecorders> data = BaseHelper.ConvertStcMessage<List<TbookPtExecorders>>(new string[] { netMsg.Topic }, netMsg);
                    matchdetailPos = BaseHelper.ConvertStcMessage<List<MatchdetailPo>>(new string[] {""}, _Matchdetail);
                }
            }
        }

        private List<MatchdetailPo> matchdetailPos;
        public List<MatchdetailPo> MatchdetailPos 
        {
            get { return matchdetailPos; }
            set { matchdetailPos = value; }
        }

        private string _MinChange;
        public string MinChange
        {
            get { return _MinChange; }
            set
            {
                _MinChange = value;
                this.OnPropertyChanged();
            }
        } 
        /// <summary>
        /// 0-含权债
        /// </summary>
        private string _IsExeCdcType = string.Empty;

        public string IsExeCdcType
        {
            get { return _IsExeCdcType; }
            set
            {
                _IsExeCdcType = value;
                this.OnPropertyChanged();
            }
        }
        #region 高亮字段定义

        private bool _CloseByIsHighlighted;
        public bool CloseByIsHighlighted
        {
            get { return _CloseByIsHighlighted; }
            set 
            { 
                _CloseByIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _DelflagIsHighlighted;
        public bool DelflagIsHighlighted
        {
            get { return _DelflagIsHighlighted; }
            set
            {
                _DelflagIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }

        private bool _SideIsHighlighted;
        public bool SideIsHighlighted
        {
            get { return _SideIsHighlighted; }
            set
            {
                _SideIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }

        private bool _PriceIsHighlighted;
        public bool PriceIsHighlighted
        {
            get { return _PriceIsHighlighted; }
            set
            {
                _PriceIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }

        private bool _BridgePFRateIsHighlighted;
        public bool BridgePFRateIsHighlighted
        {
            get { return _BridgePFRateIsHighlighted; }
            set
            {
                _BridgePFRateIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }

        private bool _CouponrateIsHighlighted;
        public bool CouponrateIsHighlighted
        {
            get { return _CouponrateIsHighlighted; }
            set
            {
                _CouponrateIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _Yield2IsHighlighted;
        public bool Yield2IsHighlighted
        {
            get { return _Yield2IsHighlighted; }
            set
            {
                _Yield2IsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _Yieldtoexe2IsHighlighted;
        public bool Yieldtoexe2IsHighlighted
        {
            get { return _Yieldtoexe2IsHighlighted; }
            set
            {
                _Yieldtoexe2IsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _CleanPrice2IsHighlighted;
        public bool CleanPrice2IsHighlighted
        {
            get { return _CleanPrice2IsHighlighted; }
            set
            {
                _CleanPrice2IsHighlighted = value;
                this.OnPropertyChanged();
            }
        }

        private bool _LastQtyIsHighlighted;
        public bool LastQtyIsHighlighted
        {
            get { return _LastQtyIsHighlighted; }
            set
            {
                _LastQtyIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _MatchQtyIsHighlighted;
        public bool MatchQtyIsHighlighted
        {
            get { return _MatchQtyIsHighlighted; }
            set
            {
                _MatchQtyIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _SettlAmountIsHighlighted;
        public bool SettlAmountIsHighlighted
        {
            get { return _SettlAmountIsHighlighted; }
            set
            {
                _SettlAmountIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _SettlTypeIsHighlighted;
        public bool SettlTypeIsHighlighted
        {
            get { return _SettlTypeIsHighlighted; }
            set
            {
                _SettlTypeIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _CounterPartyNameIsHighlighted;
        public bool CounterPartyNameIsHighlighted
        {
            get { return _CounterPartyNameIsHighlighted; }
            set
            {
                _CounterPartyNameIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _CounterPartySubNameIsHighlighted;
        public bool CounterPartySubNameIsHighlighted
        {
            get { return _CounterPartySubNameIsHighlighted; }
            set
            {
                _CounterPartySubNameIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _TrdMatchIdIsHighlighted;
        public bool TrdMatchIdIsHighlighted
        {
            get { return _TrdMatchIdIsHighlighted; }
            set
            {
                _TrdMatchIdIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _RealcounterIsHighlighted;
        public bool RealcounterIsHighlighted
        {
            get { return _RealcounterIsHighlighted; }
            set
            {
                _RealcounterIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _MatchStatusIsHighlighted;
        public bool MatchStatusIsHighlighted
        {
            get { return _MatchStatusIsHighlighted; }
            set
            {
                _MatchStatusIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _SendstatusIsHighlighted;
        public bool SendstatusIsHighlighted
        {
            get { return _SendstatusIsHighlighted; }
            set
            {
                _SendstatusIsHighlighted = value;
                this.OnPropertyChanged();
            }
        }
        private bool _IsUpdated;
        public bool IsUpdated
        {
            get { return _IsUpdated; }
            set
            {
                _IsUpdated = value;
                this.OnPropertyChanged();
            }
        }
        private string _DateFilter;
        public string DateFilter
        {
            get { return _DateFilter; }
            set
            {
                _DateFilter = value;
                this.OnPropertyChanged();
            }
        }

        public string _AllocText;
        public string AllocText
        {
            get
            {
                return _AllocText;
            }
            set
            {
                _AllocText = value;
                this.OnPropertyChanged();
            }
        }
        private string _Partymanagername;//投资经理
        public string Partymanagername
        {
            get
            {
                return _Partymanagername;
            }
            set
            {
                _Partymanagername = value;
                this.OnPropertyChanged();
            }
        }
        private string _Partymanagerid;//投资经理ID
        public string Partymanagerid
        {
            get
            {
                return _Partymanagerid;
            }
            set
            {
                _Partymanagerid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Sseinstitutioncode;//上交所机构代码
        public string Sseinstitutioncode
        {
            get
            {
                return _Sseinstitutioncode;
            }
            set
            {
                _Sseinstitutioncode = value;
                this.OnPropertyChanged();
            }
        }
        private string _Szedealercode;//深交所-交易商代码
        public string Szedealercode
        {
            get
            {
                return _Szedealercode;
            }
            set
            {
                _Szedealercode = value;
                this.OnPropertyChanged();
            }
        }
        private string _Szetradingentitycode;//深交所-交易主体代码
        public string Szetradingentitycode
        {
            get
            {
                return _Szetradingentitycode;
            }
            set
            {
                _Szetradingentitycode = value;
                this.OnPropertyChanged();
            }
        }
        private string _Szetradercode;//深交所-交易员代码
        public string Szetradercode
        {
            get
            {
                return _Szetradercode;
            }
            set
            {
                _Szetradercode = value;
                this.OnPropertyChanged();
            }
        }
        private string _Toqq;//QQ号
        public string Toqq
        {
            get
            {
                return _Toqq;
            }
            set
            {
                _Toqq = value;
                this.OnPropertyChanged();
            }
        }
        #endregion


    }



    public class MatchdetailPo
    {
        private string intentionId;
        public string IntentionId
        {
            get { return intentionId; }
            set { intentionId = value; }
        }
        private string partymanagerid;
        public string Partymanagerid
        {
            get { return partymanagerid; }
            set { partymanagerid = value; }
        }
        private string partymanagername;
        public string Partymanagername
        {
            get { return partymanagername; }
            set { partymanagername = value; }
        }
        private string quantity;
        public string Quantity { get { return quantity; } set { quantity = value; } }
    }
}

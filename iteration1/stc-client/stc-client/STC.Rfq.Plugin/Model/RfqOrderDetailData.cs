using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Rfq.Plugin.Model
{
    public class RfqOrderDetailData : ViewModelBase
    {
        private string _ClOrdID = string.Empty;
        /// <summary>
        /// 客户编号
        /// </summary>
        public string ClOrdID
        {
            get
            {
                return _ClOrdID;
            }
            set
            {
                if (_ClOrdID != value)
                {
                    _ClOrdID = value;
                    this.OnPropertyChanged(nameof(ClOrdID));
                }
            }
        }



        private string _securityid = string.Empty;
        /// <summary>
        /// 代码
        /// </summary>
        public string SecurityID
        {
            get
            {
                return _securityid;
            }
            set
            {
                if (_securityid != value)
                {
                    _securityid = value;
                    this.OnPropertyChanged(nameof(SecurityID));
                }
            }
        }

        private string _symbol = string.Empty;
        /// <summary>
        /// 简称
        /// </summary>
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged(nameof(Symbol));
                }
            }
        }

        private string _SecurityType = string.Empty;
        /// <summary>
        /// 类型
        /// </summary>
        public string SecurityType
        {
            get
            {
                return _SecurityType;
            }
            set
            {
                if (_SecurityType != value)
                {
                    _SecurityType = value;
                    this.OnPropertyChanged(nameof(SecurityType));
                }
            }
        }

        private string _marketindicator = string.Empty;
        /// <summary>
        /// 类型
        /// </summary>
        public string marketindicator
        {
            get
            {
                return _marketindicator;
            }
            set
            {
                if (_marketindicator != value)
                {
                    _marketindicator = value;
                    this.OnPropertyChanged(nameof(marketindicator));
                }
            }
        }


        private string _side = string.Empty;
        /// <summary>
        /// 方向 >0多 <0空 1 Bid 2 Ofr
        /// </summary>
        public string Side
        {
            get
            {
                return _side;
            }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged(nameof(Side));
                }
            }
        }

        private string _OrdStatus = string.Empty;
        /// <summary>
        /// 状态
        /// </summary>
        public string OrdStatus
        {
            get
            {
                return _OrdStatus;
            }
            set
            {
                if (_OrdStatus != value)
                {
                    _OrdStatus = value;
                    this.OnPropertyChanged(nameof(OrdStatus));
                }
            }
        }

        

        private string _settltype;
        /// <summary>
        /// 清算速度 1 +0  2,3+1 
        /// </summary>
        public string SettlType
        {
            get
            {
                return _settltype;
            }
            set
            {
                if (_settltype != value)
                {
                    _settltype = value;
                    this.OnPropertyChanged(nameof(SettlType));
                }
            }
        }

        private string _settldate;
        /// <summary>
        /// 清算日期
        /// </summary>
        public string SettlDate
        {
            get
            {
                return _settldate;
            }
            set
            {
                if (_settldate != value)
                {
                    _settldate = value;
                    this.OnPropertyChanged(nameof(SettlDate));
                }
            }
        }

        private string _veneues;
        /// <summary>
        /// 渠道
        /// </summary>
        public string Veneues
        {
            get
            {
                return _veneues;
            }
            set
            {
                if (_veneues != value)
                {
                    _veneues = value;
                    this.OnPropertyChanged(nameof(Veneues));
                }
            }
        }


        private string _counterpartysubid;
        /// <summary>
        /// 市场对手方机构名
        /// </summary>
        public string CounterPartySubID
        {
            get
            {
                return _counterpartysubid;
            }
            set
            {
                if (_counterpartysubid != value)
                {
                    _counterpartysubid = value;
                    this.OnPropertyChanged(nameof(CounterPartySubID));
                }
            }
        }


        private string _CounterPartyName;
        /// <summary>
        /// 市场对手方机构名
        /// </summary>
        public string CounterPartyName
        {
            get
            {
                return _CounterPartyName;
            }
            set
            {
                if (_CounterPartyName != value)
                {
                    _CounterPartyName = value;
                    this.OnPropertyChanged(nameof(CounterPartyName));
                }
            }
        }

        private string _CounterPartySubName;

        /// <summary>
        /// 市场对手方交易员名
        /// </summary>
        public string CounterPartySubName
        {
            get
            {
                return _CounterPartySubName;
            }
            set
            {
                if (_CounterPartySubName != value)
                {
                    _CounterPartySubName = value;
                    this.OnPropertyChanged(nameof(CounterPartySubName));
                }
            }
        }


        private string _partysubid;

        /// <summary>
        /// 本方ID
        /// </summary>
        public string PartySubID
        {
            get
            {
                return _partysubid;
            }
            set
            {
                if (_partysubid != value)
                {
                    _partysubid = value;
                    this.OnPropertyChanged(nameof(PartySubID));
                }
            }
        }

        private string _partysubname;

        /// <summary>
        /// 本方
        /// </summary>
        public string PartySubName
        {
            get
            {
                return _partysubname;
            }
            set
            {
                if (_partysubname != value)
                {
                    _partysubname = value;
                    this.OnPropertyChanged(nameof(PartySubName));
                }
            }
        }

        private string _NetPrice;
        /// <summary>
        /// 净价
        /// </summary>
        public string NetPrice 
        {
            get
            {
                return _NetPrice;
            }
            set
            {
                if (_NetPrice != value)
                {
                    _NetPrice = value;
                    this.OnPropertyChanged(nameof(NetPrice));
                }
            }
        }

        private string _Price;
        /// <summary>
        /// 回复价
        /// </summary>
        public string Price
        {
            get
            {
                return _Price;
            }
            set
            {
                if (_Price != value)
                {
                    _Price = value;
                    this.OnPropertyChanged(nameof(Price));
                }
            }
        }

        private string _Price2;
        /// <summary>
        /// 报价
        /// </summary>
        public string Price2
        {
            get
            {
                return _Price2;
            }
            set
            {
                if (_Price2 != value)
                {
                    _Price2 = value;
                    this.OnPropertyChanged(nameof(Price2));
                }
            }
        }

        private string _Yield;
        /// <summary>
        /// 收益率
        /// </summary>
        public string Yield
        {
            get
            {
                return _Yield;
            }
            set
            {
                if (_Yield != value)
                {
                    _Yield = value;
                    this.OnPropertyChanged(nameof(Yield));
                }
            }
        }



        private string _OrderQty = string.Empty;
        /// <summary>
        /// 回复量
        /// </summary>
        public string OrderQty
        {
            get
            {
                return _OrderQty;
            }
            set
            {
                if (_OrderQty != value)
                {
                    _OrderQty = value;
                    this.OnPropertyChanged(nameof(OrderQty));
                }
            }
        }

        private string _OrderQty2 = string.Empty;
        /// <summary>
        /// 请求量
        /// </summary>
        public string OrderQty2
        {
            get
            {
                return _OrderQty2;
            }
            set
            {
                if (_OrderQty2 != value)
                {
                    _OrderQty2 = value;
                    this.OnPropertyChanged(nameof(OrderQty2));
                }
            }
        }


        private string _UserID = string.Empty;
        /// <summary>
        ///   
        /// </summary>
        public string UserID
        {
            get
            {
                return _UserID;
            }
            set
            {
                if (_UserID != value)
                {
                    _UserID = value;
                    this.OnPropertyChanged(nameof(UserID));
                }
            }
        }



        private string _UserName = string.Empty;
        /// <summary>
        ///   
        /// </summary>
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                if (_UserName != value)
                {
                    _UserName = value;
                    this.OnPropertyChanged(nameof(UserName));
                }
            }
        }


        private string _tradeDate;
        public string TradeDate
        {
            get
            {
                return _tradeDate;
            }
            set
            {
                if (_tradeDate != value)
                {
                    _tradeDate = value;
                    this.OnPropertyChanged(nameof(TradeDate));
                }
            }
        }

        private string _status;
        /// <summary>
        /// 转换后的状态
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    this.OnPropertyChanged(nameof(Status));
                }
            }
        }


        private string _TransactTime;
        public string TransactTime
        {
            get
            {
                return _TransactTime;
            }
            set
            {
                if (_TransactTime != value)
                {
                    _TransactTime = value;
                    this.OnPropertyChanged(nameof(TransactTime));
                }
            }
        }

        private string _UpdateTime;

        public string UpdateTime
        {
            get { return _UpdateTime; }
            set
            {
                if (_UpdateTime != value)
                {
                    _UpdateTime = value;
                    this.OnPropertyChanged(nameof(UpdateTime));
                }
            }
        }

        private string _remark;

        public string Remark
        {
            get { return _remark; }
            set
            {
                if (_remark != value)
                {
                    _remark = value;
                    this.OnPropertyChanged(nameof(Remark));
                }
            }
        }

        private string _isselfrfq;
        /// <summary>
        /// 是本人的Rfq
        /// </summary>
        public string IsSelfRfq
        {
            get
            {
                return _isselfrfq;
            }
            set
            {
                if (_isselfrfq != value)
                {
                    _isselfrfq = value;
                    this.OnPropertyChanged(nameof(IsSelfRfq));
                }
            }
        }

        private string _createtime = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime
        {
            get
            {
                return _createtime;
            }
            set
            {
                if (_createtime != value)
                {
                    _createtime = value;
                    this.OnPropertyChanged(nameof(CreateTime));
                }
            }
        }

        private string _VenueTypeGw;

        public string VenueTypeGw
        {
            get { return _VenueTypeGw; }
            set
            {
                if (_VenueTypeGw != value)
                {
                    _VenueTypeGw = value;
                    this.OnPropertyChanged(nameof(VenueTypeGw));
                }
            }
        }

        private string _Venues;

        public string Venues
        {
            get { return _Venues; }
            set
            {
                if (_Venues != value)
                {
                    _Venues = value;
                    this.OnPropertyChanged(nameof(Venues));
                }
            }
        }

        private string _VenuesSelectF;

        public string VenuesSelectF
        {
            get { return _VenuesSelectF; }
            set
            {
                if (_VenuesSelectF != value)
                {
                    _VenuesSelectF = value;
                    this.OnPropertyChanged(nameof(VenuesSelectF));
                }
            }
        }

        private string _SelfSelectVenues;

        public string SelfSelectVenues
        {
            get { return _SelfSelectVenues; }
            set
            {
                if (_SelfSelectVenues != value)
                {
                    _SelfSelectVenues = value;
                    this.OnPropertyChanged(nameof(SelfSelectVenues));
                }
            }
        }

        private string _QuoteReqID = string.Empty;
        /// <summary>
        /// 报价请求编号
        /// </summary>
        public string QuoteReqID
        {
            get
            {
                return _QuoteReqID;
            }
            set
            {
                if (_QuoteReqID != value)
                {
                    _QuoteReqID = value;
                    this.OnPropertyChanged(nameof(QuoteReqID));
                }
            }
        }

        private string _QuoteStatus = string.Empty;
        /// <summary>
        /// 订单状态
        /// </summary>
        public string QuoteStatus
        {
            get
            {
                return _QuoteStatus;
            }
            set
            {
                if (_QuoteStatus != value)
                {
                    _QuoteStatus = value;
                    this.OnPropertyChanged(nameof(QuoteStatus));
                }
            }
        }

        /// <summary>
        /// 回复价格
        /// </summary>
        public string _makerprice;
        public string MakerPrice
        {
            get
            {
                return _makerprice;
            }
            set
            {
                if (_makerprice != value)
                {
                    _makerprice = value;
                    this.OnPropertyChanged(nameof(MakerPrice));
                }
            }
        }

        /// <summary>
        /// 回复量
        /// </summary>
        public string _makerorderqty;
        public string MakerOrderQty
        {
            get
            {
                return _makerorderqty;
            }
            set
            {
                if (_makerorderqty != value)
                {
                    _makerorderqty = value;
                    this.OnPropertyChanged(nameof(MakerOrderQty));
                }
            }
        }
        /// <summary>
        /// 报价类型
        /// </summary>
        public string _quotetype;
        public string QuoteType
        {
            get
            {
                return _quotetype;
            }
            set
            {
                if (_quotetype != value)
                {
                    _quotetype = value;
                    this.OnPropertyChanged(nameof(QuoteType));
                }
            }
        }

        /// <summary>
        /// 报价状态说明
        /// </summary>
        public string _quotetranstype;
        public string QuoteTransType
        {
            get
            {
                return _quotetranstype;
            }
            set
            {
                if (_quotetranstype != value)
                {
                    _quotetranstype = value;
                    this.OnPropertyChanged(nameof(QuoteTransType));
                }
            }
        }

        private string _quotestatusdesc = string.Empty;
        /// <summary>
        /// 报价状态说明
        /// </summary>
        public string QuoteStatusDesc
        {
            get
            {
                return _quotestatusdesc;
            }
            set
            {
                if (_quotestatusdesc != value)
                {
                    _quotestatusdesc = value;
                    this.OnPropertyChanged(nameof(QuoteStatusDesc));
                }
            }
        }

        /// <summary>
        /// 清算类型
        /// </summary>
        public string _clearingmethod;


        public string ClearingMethod
        {
            get
            {
                return _clearingmethod;
            }
            set
            {
                if (_clearingmethod != value)
                {
                    _clearingmethod = value;
                    this.OnPropertyChanged(nameof(ClearingMethod));
                }
            }
        }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string _deliverytype;

        public string DeliveryType
        {
            get
            {
                return _deliverytype;
            }
            set
            {
                if (_deliverytype != value)
                {
                    _deliverytype = value;
                    this.OnPropertyChanged(nameof(DeliveryType));
                }
            }
        }

        private string _Info1 = string.Empty;
        /// <summary>
        /// 报价状态说明
        /// </summary>
        public string Info1
        {
            get
            {
                return _Info1;
            }
            set
            {
                if (_Info1 != value)
                {
                    _Info1 = value;
                    this.OnPropertyChanged(nameof(Info1));
                }
            }
        }

        private string _Info2 = string.Empty;
        public string Info2
        {
            get
            {
                return _Info2;
            }
            set
            {
                if (_Info2 != value)
                {
                    _Info2 = value;
                    this.OnPropertyChanged(nameof(Info2));
                }
            }
        }

        private string _ValidUntilTime;
        public string ValidUntilTime
        {
            get
            {
                return _ValidUntilTime;
            }
            set
            {
                if (_ValidUntilTime != value)
                {
                    _ValidUntilTime = value;
                    this.OnPropertyChanged(nameof(ValidUntilTime));
                }
            }
        }

        public int _remainseconds;
        /// <summary>
        /// 剩余时间
        /// </summary>
        public int RemainSeconds
        {
            get
            {
                return _remainseconds;
            }
            set
            {
                if (_remainseconds != value)
                {
                    _remainseconds = value;
                    this.OnPropertyChanged(nameof(RemainSeconds));
                }
            }
        }


        public string _showremainseconds;
        /// <summary>
        /// 显示剩余时间
        /// </summary>
        public string ShowRemainSeconds
        {
            get
            {
                return _showremainseconds;
            }
            set
            {
                if (_showremainseconds != value)
                {
                    _showremainseconds = value;
                    this.OnPropertyChanged(nameof(ShowRemainSeconds));
                }
            }
        }

        public string _alerttime;
        /// <summary>
        /// 是否显示红色
        /// </summary>
        public string AlertTime
        {
            get
            {
                return _alerttime;
            }
            set
            {
                if (_alerttime != value)
                {
                    _alerttime = value;
                    this.OnPropertyChanged(nameof(AlertTime));
                }
            }
        }


        private string _QuoteID;
        /// <summary>
        /// 报价编号
        /// </summary>
        public string QuoteID
        {
            get
            {
                return _QuoteID;
            }
            set
            {
                if (_QuoteID != value)
                {
                    _QuoteID = value;
                    this.OnPropertyChanged(nameof(QuoteID));
                }
            }
        }


        private int _IsOutTime;
        /// <summary>
        /// RemainSeconds是否大于0
        /// </summary>
        public int IsOutTime
        {
            get
            {
                return _IsOutTime;
            }
            set
            {
                if (_IsOutTime != value)
                {
                    _IsOutTime = value;
                    this.OnPropertyChanged(nameof(IsOutTime));
                }
            }
        }

        private string _statusorder;
        /// <summary>
        /// 状态排序
        /// </summary>
        public string StatusOrder
        {
            get
            {
                return _statusorder;
            }
            set
            {
                if (_statusorder != value)
                {
                    _statusorder = value;
                    this.OnPropertyChanged(nameof(StatusOrder));
                }
            }
        }

        private string _bookid;
        public string BookID
        {
            get
            {
                return _bookid;
            }
            set
            {
                if (_bookid != value)
                {
                    _bookid = value;
                    this.OnPropertyChanged(nameof(BookID));
                }
            }
        }


        private bool iscanchangecancelorder;
        /// <summary>
        /// 是否能改价撤单
        /// </summary>
        public bool IsCanChangeCancelOrder
        {
            get
            {
                return iscanchangecancelorder;
            }
            set
            {
                if (iscanchangecancelorder != value)
                {
                    iscanchangecancelorder = value;
                    this.OnPropertyChanged(nameof(IsCanChangeCancelOrder));
                }
            }
        }

        private bool iscanreplyrefuseorder;
        /// <summary>
        /// 是否能回价拒绝
        /// </summary>
        public bool IsCanReplyRefuseOrder
        {
            get
            {
                return iscanreplyrefuseorder;
            }
            set
            {
                if (iscanreplyrefuseorder != value)
                {
                    iscanreplyrefuseorder = value;
                    this.OnPropertyChanged(nameof(IsCanReplyRefuseOrder));
                }
            }
        }


        /// <summary>
        /// RFQ策略具体内容
        /// </summary>
        private string _inf1 = string.Empty;
        public string inf1
        {
            get
            {
                return _inf1;
            }
            set
            {
                if (_inf1 != value)
                {
                    _inf1 = value;
                    this.OnPropertyChanged(nameof(inf1));
                }
            }
        }

        /// <summary>
        /// RFQ策略自动匹配传来的报价
        /// </summary>
        private string _inf2 = string.Empty;
        public string inf2
        {
            get
            {
                return _inf2;
            }
            set
            {
                if (_inf2 != value)
                {
                    _inf2 = value;
                    this.OnPropertyChanged(nameof(inf2));
                }
            }
        }
    }

    public class RfqOrderDetailDataInf2Model
    {
        public string quantity;
        public string price;
        public string QuoteReqID;
    }
}

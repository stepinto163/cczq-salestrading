using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class GrFeedBackViewModel : BaseViewModel
    {
        private string _MDReqID;
        public string MDReqID
        {
            get
            {
                return _MDReqID;
            }
            set
            {
                _MDReqID = value;
                this.OnPropertyChanged();
            }
        }

        private string _MsgId;
        public string MsgId
        {
            get
            {
                return _MsgId;
            }
            set
            {
                _MsgId = value;
                this.OnPropertyChanged();
            }
        }

        private string _StrategyOrderId;
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

        private string _BlockNum
;
        public string BlockNum

        {
            get
            {
                return _BlockNum;
            }
            set
            {
                _BlockNum = value;
                this.OnPropertyChanged();
            }
        }

        private string _SecurityID;
        public string SecurityID
        {
            get
            {
                return _SecurityID;
            }
            set
            {
                _SecurityID = value;
                this.OnPropertyChanged();
            }
        }


        private string _Symbol;
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

        private string _Price;
        public string Price
        {
            get
            {
                return _Price;
            }
            set
            {
                _Price = value;
                this.OnPropertyChanged();
            }
        }

        private string _PartyID;
        public string PartyID
        {
            get
            {
                return _PartyID;
            }
            set
            {
                _PartyID = value;
                this.OnPropertyChanged();
            }
        }

        private string _OrderQty;
        public string OrderQty
        {
            get
            {
                return _OrderQty;
            }
            set
            {
                _OrderQty = value;

                this.OnPropertyChanged();
            }
        }

        private string _LastQty;
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
        

        private string _LeavesQty;
        public string LeavesQty
        {
            get
            {
                return _LeavesQty;
            }
            set
            {
                _LeavesQty = value;
                this.OnPropertyChanged();
            }
        }

        

        private string _ClOrdID;
        public string ClOrdID
        {
            get
            {
                return _ClOrdID;
            }
            set
            {
                _ClOrdID = value;
                this.OnPropertyChanged();
            }
        }

        private string _QuoteID;
        public string QuoteID
        {
            get
            {
                return _QuoteID;
            }
            set
            {
                _QuoteID = value;
                this.OnPropertyChanged();
            }
        }


        private string _OrderID;
        public string OrderID
        {
            get
            {
                return _OrderID;
            }
            set
            {
                _OrderID = value;
                this.OnPropertyChanged();
            }
        }

        private string _ExecID;
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

        private string _TraderName;
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

        private string _Side;
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

        private string _CounterPartyInstitutionName;
        public string CounterPartyInstitutionName
        {
            get
            {
                return _CounterPartyInstitutionName;
            }
            set
            {
                _CounterPartyInstitutionName = value;
                this.OnPropertyChanged();
            }
        }

        private string _CounterPartyTraderName;
        public string CounterPartyTraderName
        {
            get
            {
                return _CounterPartyTraderName;
            }
            set
            {
                _CounterPartyTraderName = value;
                this.OnPropertyChanged();
            }
        }

        private string _ButtonVisible = "0";
        public string ButtonVisible
        {
            get
            {
                return _ButtonVisible;
            }
            set
            {
                _ButtonVisible = value;
                this.OnPropertyChanged();
            }
        }

        private string _DayPrice;

        public string DayPrice
        {
            get
            {
                return _DayPrice;
            }
            set
            {
                _DayPrice = value;
                this.OnPropertyChanged();
            }
        }


        private string _DayOrderQty;

        public string DayOrderQty
        {
            get
            {
                return _DayOrderQty;
            }
            set
            {
                _DayOrderQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _DaySettlType;

        public string DaySettlType
        {
            get
            {
                return _DaySettlType;
            }
            set
            {
                _DaySettlType = value;
                this.OnPropertyChanged();
            }
        }

        private string _DayComment;
        public string DayComment
        {
            get
            {
                return _DayComment;
            }
            set
            {
                _DayComment = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsMe = false;
        public bool IsMe
        {
            get
            {
                return _IsMe;
            }
            set
            {
                _IsMe = value;
                this.OnPropertyChanged();
            }
        }

        private string _OrdStatus;
        public string OrdStatus
        {
            get
            {
                return _OrdStatus;
            }
            set
            {
                _OrdStatus = value;
                AmountIsEnable = value == "2" ? "1" : "0";

                this.OnPropertyChanged();
            }
        }

        private string _StatusMsg;
        public string StatusMsg
        {
            get
            {
                return _StatusMsg;
            }
            set
            {
                _StatusMsg = value;
                this.OnPropertyChanged();
            }
        }

        private string _PartLastQty;
        public string PartLastQty
        {
            get
            {
                return _PartLastQty;
            }
            set
            {
                _PartLastQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _HandlInst;
        public string HandlInst
        {
            get
            {
                return _HandlInst;
            }
            set
            {
                _HandlInst = value;
                this.OnPropertyChanged();
            }
        }


        private string _DayHandlInst;
        public string DayHandlInst
        {
            get
            {
                return _DayHandlInst;
            }
            set
            {
                _DayHandlInst = value;
                this.OnPropertyChanged();
            }
        }


        private string _CommandType;
        public string CommandType
        {
            get
            {
                return _CommandType;
            }
            set
            {
                _CommandType = value;
                this.OnPropertyChanged();
            }
        }

        private string _CloseBy;
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

        private string _CloseById;
        public string CloseById
        {
            get
            {
                return _CloseById;
            }
            set
            {
                _CloseById = value;
                this.OnPropertyChanged();
            }
        }

        private string _UpdateTime;
        public string UpdateTime
        {
            get
            {
                return _UpdateTime;
            }
            set
            {
                _UpdateTime = value;
                this.OnPropertyChanged();
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
                _TransactTime = value;
                this.OnPropertyChanged();
            }
        }


        private string _ocoFlag;

        /// <summary>
        /// OCO标识
        /// </summary>
        public string ocoFlag
        {
            get
            {
                return _ocoFlag;
            }
            set
            {
                _ocoFlag = value;
                this.OnPropertyChanged();
            }
        }

        private bool _exchangesignFlag;

        /// <summary>
        /// 换券标识 true: 换券; false 不换券
        /// </summary>
        public bool ExchangeSignFlag
        {
            get
            {
                return _exchangesignFlag;
            }
            set
            {
                _exchangesignFlag = value;
                this.OnPropertyChanged();
            }
        }

        private string _bargainFlag;
        /// <summary>
        /// 议价标识 0:无星; 1: 1星; 2: 2星
        /// </summary>
        public string bargainFlag
        {
            get
            {
                return _bargainFlag;
            }
            set
            {
                _bargainFlag = value;
                this.OnPropertyChanged();
            }
        }

        private string _wholeFlag;

        /// <summary>
        /// 整量标志
        /// </summary>
        public string wholeFlag
        {
            get
            {
                return _wholeFlag;
            }
            set
            {
                _wholeFlag = value;
                this.OnPropertyChanged();
            }
        }


        private string _firmflag;
        /// <summary>
        /// 1 firm 0 NoFrim
        /// </summary>
        public string FirmFlag
        {
            get
            {
                return _firmflag;
            }
            set
            {
                _firmflag = value; this.OnPropertyChanged();
            }
        }


        private string _DelFlag;
        public string DelFlag
        {
            get
            {
                return _DelFlag;
            }
            set
            {
                _DelFlag = value;
                this.OnPropertyChanged();
            }
        }


        #region 我的订单被点价

        private string _QQMsg;
        public string QQMsg
        {
            get
            {
                return _QQMsg;
            }
            set
            {
                _QQMsg = value;
                this.OnPropertyChanged();
            }
        }

        private string _ManualEnable = "1";
        public string ManualEnable
        {
            get
            {
                return _ManualEnable;
            }
            set
            {
                _ManualEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _AutoEnable = "1";
        public string AutoEnable
        {
            get
            {
                return _AutoEnable;
            }
            set
            {
                _AutoEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _PartySubID;
        public string PartySubID
        {
            get
            {
                return _PartySubID;
            }
            set
            {
                _PartySubID = value;
                this.OnPropertyChanged();
            }
        }
        private string _PartyTraderId;
        public string PartyTraderId
        {
            get
            {
                return _PartyTraderId;
            }
            set
            {
                _PartyTraderId = value;
                this.OnPropertyChanged();
            }
        }

        private string _SettlType;
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

        private string _Comment;
        public string Comment
        {
            get
            {
                return _Comment;
            }
            set
            {
                _Comment = value;
                this.OnPropertyChanged();
            }
        }

        private string _PointOrderQty = string.Empty;
        public string PointOrderQty
        {
            get
            {
                return _PointOrderQty;
            }
            set
            {
                _PointOrderQty = value;

                if (AmountIsEnable == "1")
                {
                    if (Utils.CommonUtils.ConvertToDecimal(value) * AmountUnit > Utils.CommonUtils.ConvertToDecimal(OrderQty))
                    {
                        AddEnabled = "0";
                        SubtractionEnabled = "1";
                        ButtonIsEnable = "0";
                    }
                    else if (Utils.CommonUtils.ConvertToDecimal(value) * AmountUnit == Utils.CommonUtils.ConvertToDecimal(OrderQty))
                    {
                        AddEnabled = "0";
                        SubtractionEnabled = "1";
                        ButtonIsEnable = "1";
                    }
                    else if (Utils.CommonUtils.ConvertToDecimal(value) * AmountUnit < MinAmount)
                    {
                        AddEnabled = "1";
                        SubtractionEnabled = "0";
                        ButtonIsEnable = "0";
                    }
                    else if (Utils.CommonUtils.ConvertToDecimal(value) * AmountUnit == MinAmount)
                    {
                        AddEnabled = "1";
                        SubtractionEnabled = "0";
                        ButtonIsEnable = "1";
                    }
                    else
                    {
                        AddEnabled = "1";
                        SubtractionEnabled = "1";
                        ButtonIsEnable = "1";
                    }

                    //如果下单量等于最小量，则增加减少按钮为灰色
                    if (Utils.CommonUtils.ConvertToDecimal(value) * AmountUnit == MinAmount && MaxAmount == MinAmount)
                    {
                        AddEnabled = "0";
                        SubtractionEnabled = "0";
                        ButtonIsEnable = "1";
                    }
                }
                else
                {
                    AddEnabled = "0";
                    SubtractionEnabled = "0";
                    ButtonIsEnable = "0";
                }


                Amount = Utils.CommonUtils.ConvertToDecimal(value) * AmountUnit;

                _PointOrderQty = value;
                 this.OnPropertyChanged();
            }
        }

        private decimal amount = decimal.Zero;
        public decimal Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                this.OnPropertyChanged();
            }
        }
        private decimal maxAmount = decimal.MaxValue;
        public decimal MaxAmount
        {
            get { return maxAmount; }
            set
            {
                maxAmount = value;
                this.OnPropertyChanged();
            }
        }
        private string _SubtractionEnabled = "1";
        public string SubtractionEnabled
        {
            get { return _SubtractionEnabled; }
            set
            {
                _SubtractionEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private string _AddEnabled = "1";
        public string AddEnabled
        {
            get { return _AddEnabled; }
            set
            {
                _AddEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private string _TraderBQQ;
        public string TraderBQQ
        {
            get
            {
                return _TraderBQQ;
            }
            set
            {
                _TraderBQQ = value;
                this.OnPropertyChanged();
            }
        }


        private string _ButtonIsEnable;
        public string ButtonIsEnable
        {
            get
            {
                return _ButtonIsEnable;
            }
            set
            {
                _ButtonIsEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _RejectVisible;
        public string RejectVisible
        {
            get
            {
                return _RejectVisible;
            }
            set
            {
                _RejectVisible = value;
                this.OnPropertyChanged();
            }
        }


        private string _AmountIsEnable;
        public string AmountIsEnable
        {
            get
            {
                return _AmountIsEnable;
            }
            set
            {
                _AmountIsEnable = value;
                this.OnPropertyChanged();
            }
        }

        public decimal _MinAmount = 1000;
        public decimal MinAmount
        {
            get
            {
                return Utils.LocalEnv.UserInfo.CompanyId == "4" ? 500 : 1000;
            }
        }

        public decimal AmountUnit { get { return 1000; } }


        #endregion
    }
}

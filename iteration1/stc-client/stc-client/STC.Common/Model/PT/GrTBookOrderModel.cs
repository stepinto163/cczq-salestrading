using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class GrTBookOrderModel : BaseViewModel
    {
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

        private string _QuoteQty;
        public string QuoteQty
        {
            get
            {
                return _QuoteQty;
            }
            set
            {
                _QuoteQty = value;
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

        private string _CumQty;
        public string CumQty
        {
            get
            {
                return _CumQty;
            }
            set
            {
                _CumQty = value;
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

        private string _ocoFlag;

        /// <summary>
        /// OCO标识
        /// </summary>
        public string OCOFlag
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

        private string _exchangesignFlag;

        /// <summary>
        /// 换券标识 true: 换券; false 不换券
        /// </summary>
        public string ExchangeSignFlag
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
        public string BargainFlag
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
        public string WholeFlag
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

        private string _counterpartyTraderQQ;
        public string counterpartyTraderQQ
        {
            get
            {
                return _counterpartyTraderQQ;
            }
            set
            {
                _counterpartyTraderQQ = value;
                this.OnPropertyChanged();
            }
        }

        private string _counterPartySubQQ;
        public string counterPartySubQQ
        {
            get
            {
                return _counterPartySubQQ;
            }
            set
            {
                _counterPartySubQQ = value;
                this.OnPropertyChanged();
            }
        }


        private string _partySubQQ;
        public string partySubQQ
        {
            get
            {
                return _partySubQQ;
            }
            set
            {
                _partySubQQ = value;
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

        private string _Remark;
        /// <summary>
        /// Comment
        /// </summary>
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


        private string _Text;
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                this.OnPropertyChanged();
            }
        }

        private string _fandian;
        public string fandian
        {
            get
            {
                return _fandian;
            }
            set
            {
                _fandian = value;
                this.OnPropertyChanged();
            }
        }


        private string _IfRQO;
        public string IfRQO
        {
            get
            {
                return _IfRQO;
            }
            set
            {
                _IfRQO = value;
                this.OnPropertyChanged();
            }
        }

    }
}

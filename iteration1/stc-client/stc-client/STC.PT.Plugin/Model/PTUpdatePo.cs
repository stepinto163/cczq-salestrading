using System.Collections.ObjectModel;

namespace STC.PT.Plugin.Model
{
    public class PTUpdatePo:ViewModelBase
    {
        private string _ExecID;
        private string _TradeDate;
        private string _SecurityID;
        private string _Symbol;
        private string _Side;
        private string _LastPx;
        private string _NetPrice;
        private string _LastQty;
        private string _SettlType;
        private string _InstitutionName;
        private string _ClientInstID;
        private string _TraderName;
        private string _ClientID;
        private string _Seatnumber;
        private string _Quotemethod;
        private string _Remark;
        private ObservableCollection<PTUpdateDetailPo> _Details;
        public ObservableCollection<PTUpdateDetailPo> Details
        {
            get
            {
                return _Details;
            }

            set
            {
                _Details = value;

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
        public string ClientID
        {
            get
            {
                return _ClientID;
            }

            set
            {
                _ClientID = value;

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
        public string ClientInstID
        {
            get
            {
                return _ClientInstID;
            }

            set
            {
                _ClientInstID = value;

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
                return _SecurityID;
            }

            set
            {
                _SecurityID = value;

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

        private string _Approvalorderid;
        public string Approvalorderid
        {
            get
            {
                return _Approvalorderid;
            }

            set
            {
                _Approvalorderid = value;

                this.OnPropertyChanged();
            }
        }
        private string _Approvalstatus;
        public string Approvalstatus
        {
            get
            {
                return _Approvalstatus;
            }

            set
            {
                _Approvalstatus = value;

                this.OnPropertyChanged();
            }
        }
        private string _Updatetime;
        public string Updatetime
        {
            get
            {
                return _Updatetime;
            }

            set
            {
                _Updatetime = value;

                this.OnPropertyChanged();
            }
        }

        private string _BusinessType;
        public string BusinessType
        {
            get
            {
                return _BusinessType;
            }
            set
            {
                _BusinessType = value;
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

        private string _CounterPartyName;
        private string _CounterPartySubName;
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

        private string _counterpartyid;//对手机构id
        /// <summary>
        /// 对手机构id
        /// </summary>
        public string CounterPartyid
        {
            get { return _counterpartyid; }
            set { _counterpartyid = value;
                this.OnPropertyChanged();
            }
        }

        private string _counterpartysubid;//对手交易员id
        /// <summary>
        /// 对手交易员id
        /// </summary>
        public string CounterPartySubid
        {
            get { return _counterpartysubid; }
            set { _counterpartysubid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Approvalterminal;
        public string Approvalterminal
        {
            get { return _Approvalterminal; }
            set { _Approvalterminal = value;
                this.OnPropertyChanged();
            }
        }
    }
    public class PTUpdateDetailPo : ViewModelBase
    {
        private string _OldValue;
        private string _NewValue;
        public string OldValue
        {
            get
            {
                return _OldValue;
            }

            set
            {
                _OldValue = value;

                this.OnPropertyChanged();
            }
        }
        public string NewValue
        {
            get
            {
                return _NewValue;
            }

            set
            {
                _NewValue = value;

                this.OnPropertyChanged();
            }
        }
    }
}

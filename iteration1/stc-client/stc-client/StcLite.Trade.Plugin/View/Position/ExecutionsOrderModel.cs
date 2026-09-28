using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.View.Position
{
    public class ExecutionsOrderModel : BaseViewModel
    {
        /// <summary>
        /// 已确认订单
        /// </summary>
        public string Key
        {
            get
            {
                return _Key;
            }
            set
            {
                this._Key = value;
                this.OnPropertyChanged(nameof(Key));
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
                this._Side = value;
                this.OnPropertyChanged(nameof(Side));
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
                this._SecurityID = value;
                this.OnPropertyChanged(nameof(SecurityID));
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
                this._Symbol = value;
                this.OnPropertyChanged(nameof(Symbol));
            }
        }

        public string SecurityType
        {
            get
            {
                return _SecurityType;
            }
            set
            {
                this._SecurityType = value;
                this.OnPropertyChanged(nameof(SecurityType));
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
                this._LastPx_Display = value;
                this.OnPropertyChanged(nameof(LastPx_Display));
            }
        }

        public decimal LastPx
        {
            get
            {
                return _LastPx;
            }
            set
            {
                this._LastPx = value;
                this.OnPropertyChanged(nameof(LastPx));
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
                this.OnPropertyChanged(nameof(NetPrice));
            }
        }

        public decimal DirtyPrice
        {
            get
            {
                return _DirtyPrice;
            }

            set
            {
                _DirtyPrice = value;
                this.OnPropertyChanged(nameof(DirtyPrice));
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
                this.OnPropertyChanged(nameof(SettlType));
            }
        }


        public string SettlDate
        {
            get
            {
                return _SettlDate;
            }

            set
            {
                _SettlDate = value;
                this.OnPropertyChanged(nameof(SettlDate));
            }
        }

        public decimal CumQty
        {
            get
            {
                return _CumQty;
            }

            set
            {
                _CumQty = value;
                this.OnPropertyChanged(nameof(CumQty));
            }
        }

        public long LastQty
        {
            get
            {
                return _LastQty;
            }

            set
            {
                _LastQty = value;
                this.OnPropertyChanged(nameof(LastQty));
            }
        }

        public decimal DeliveryAmount
        {
            get
            {
                return _DeliveryAmount;
            }

            set
            {
                _DeliveryAmount = value;
                this.OnPropertyChanged(nameof(DeliveryAmount));
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
                this.OnPropertyChanged(nameof(CounterPartyName));
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
                this.OnPropertyChanged(nameof(CounterPartySubName));
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
                this.OnPropertyChanged(nameof(ExecID));
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
                this.OnPropertyChanged(nameof(Venues));
            }
        }

        public string PartyName
        {
            get
            {
                return _PartyName;
            }

            set
            {
                _PartyName = value;
                this.OnPropertyChanged(nameof(PartyName));
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
                this.OnPropertyChanged(nameof(PartySubName));
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
                this.OnPropertyChanged(nameof(ExecutionTime));
            }
        }

        public string StrategyOrderID
        {
            get
            {
                return _StrategyOrderID;
            }

            set
            {
                _StrategyOrderID = value;
                this.OnPropertyChanged(nameof(StrategyOrderID));
            }
        }

        public string RefOrderID
        {
            get
            {
                return _RefOrderID;
            }

            set
            {
                _RefOrderID = value;
                this.OnPropertyChanged(nameof(RefOrderID));
            }
        }

        public string SyncStatus
        {
            get
            {
                return _SyncStatus;
            }

            set
            {
                _SyncStatus = value;
                this.OnPropertyChanged(nameof(SyncStatus));
            }
        }

        public string ClOrdID
        {
            get
            {
                return _ClOrdID;
            }

            set
            {
                _ClOrdID = value;
                this.OnPropertyChanged(nameof(ClOrdID));
            }
        }

        public string Price
        {
            get
            {
                return _Price;
            }

            set
            {
                _Price = value;
                this.OnPropertyChanged(nameof(Price));
            }
        }

        public string OrderQty
        {
            get
            {
                return _OrderQty;
            }

            set
            {
                _OrderQty = value;
                this.OnPropertyChanged(nameof(OrderQty));
            }
        }

        public string OrderID
        {
            get
            {
                return _OrderID;
            }

            set
            {
                _OrderID = value;
                this.OnPropertyChanged(nameof(OrderID));
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
                this.OnPropertyChanged(nameof(TradeDate));
            }
        }

        public string UserName
        {
            get
            {
                return _UserName;
            }

            set
            {
                _UserName = value;
                this.OnPropertyChanged(nameof(UserName));
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
                this.OnPropertyChanged(nameof(InstitutionName));
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
                this.OnPropertyChanged(nameof(TraderName));
            }
        }

        public string Info6
        {
            get
            {
                return _Info6;
            }

            set
            {
                _Info6 = value;
                this.OnPropertyChanged(nameof(Info6));
            }
        }

        private string _Key;
        private string _Side;
        private string _SecurityID;
        private string _Symbol;
        private string _SecurityType;
        private string _LastPx_Display;
        private decimal _LastPx;
        private string _NetPrice;
        private decimal _DirtyPrice;
        private string _SettlDate;
        private decimal _CumQty;
        private long _LastQty;
        private string _SettlType;
        private decimal _DeliveryAmount;
        private string _CounterPartyName;
        private string _CounterPartySubName;
        private string _ExecID;
        private string _Venues;
        private string _PartyName;
        private string _PartySubName;
        private string _ExecutionTime;
        private string _StrategyOrderID;
        private string _RefOrderID;
        private string _SyncStatus;
        private string _ClOrdID;
        private string _Price;
        private string _OrderQty;
        private string _OrderID;
        private string _TradeDate;
        private string _UserName;
        private string _InstitutionName;
        private string _TraderName;
        private string _Info6;
    }
}

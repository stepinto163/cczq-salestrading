using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Drp.Component.Broker
{
    public class IntentionDetailViewModel : ViewModelBase
    {
        #region 字段
        private string _SecurityID;
        private string _Symbol;
        private string _TraderName;
        private string _Side;
        private string _CounterPartyInstitutionName;
        private string _CounterPartyTraderName;
        private string _Price;
        private string _OrderQty;
        private string _SettlType;
        private string _Comment;
        private string _OCOVisible;
        private string _PackageVisible;
        private string _SwitchVisible;
        private string _XVisible;
        private string _XXVisible;
        private string _DayPrice;
        private string _DayOrderQty;
        private string _DaySettlType;
        private string _DayComment;
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

        public string OCOVisible
        {
            get
            {
                return _OCOVisible;
            }
            set
            {
                _OCOVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string PackageVisible
        {
            get
            {
                return _PackageVisible;
            }
            set
            {
                _PackageVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string SwitchVisible
        {
            get
            {
                return _SwitchVisible;
            }
            set
            {
                _SwitchVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string XVisible
        {
            get
            {
                return _XVisible;
            }
            set
            {
                _XVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string XXVisible
        {
            get
            {
                return _XXVisible;
            }
            set
            {
                _XXVisible = value;
                this.OnPropertyChanged();
            }
        }

        private string _IOCOrderQty;
        public string IOCOrderQty
        {
            get
            {
                return _IOCOrderQty;
            }
            set
            {
                _IOCOrderQty = value;
                this.OnPropertyChanged();
            }
        }
        private string _LeaveOrderQty;
        public string LeaveOrderQty
        {
            get
            {
                return _LeaveOrderQty;
            }
            set
            {
                _LeaveOrderQty = value;
                this.OnPropertyChanged();
            }
        }
        private string _IOCPrice;
        public string IOCPrice
        {
            get
            {
                return _IOCPrice;
            }
            set
            {
                _IOCPrice = value;
                this.OnPropertyChanged();
            }
        }
        private string _Count;
        public string Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
                this.OnPropertyChanged();
            }
        }

        private string _NoMatchVisible;
        public string NoMatchVisible
        {
            get
            {
                return _NoMatchVisible;
            }
            set
            {
                _NoMatchVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string Info4;
        private string _BrokerAIsMe;
        public string BrokerAIsMe
        {
            get
            {
                return _BrokerAIsMe;
            }
            set
            {
                _BrokerAIsMe = value;
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
        #endregion
        public void BindingData(ExecutionReport po)
        {
            HandlInst = po.HandlInst;
            SecurityID = po.SecurityID;
            Symbol = po.Symbol;
            TraderName = po.PartySubName;
            Side = po.Side.Equals("1") ? "2" : "1";
            string counterTraderName = getDayOrderInfo(Info4, 7);
            if (!string.IsNullOrEmpty(counterTraderName))
            {
                CounterPartyInstitutionName = getDayOrderInfo(Info4, 5) + string.Format("({0})", counterTraderName);
            }
            else
            {
                CounterPartyInstitutionName = getDayOrderInfo(Info4, 5);
            }
            CounterPartyTraderName = getDayOrderInfo(Info4, 8);
            string CounterPartySubID = getDayOrderInfo(Info4, 20);
            if (!string.IsNullOrEmpty(CounterPartySubID) && CounterPartySubID.Equals(po.UserID))
            {
                BrokerAIsMe = "1";
            }
            Price = po.Price;
            OrderQty = getDayOrderInfo(Info4, 10);
            SettlType = getDayOrderInfo(Info4, 11);
            Comment = po.Remark;
            DayPrice = getDayOrderInfo(Info4, 9);
            DayOrderQty = getDayOrderInfo(Info4, 19);
            DaySettlType = getDayOrderInfo(Info4, 18);
            DayComment = getDayOrderInfo(Info4, 15);
            DayHandlInst = getDayOrderInfo(Info4, 16);
            string OCOFlag = getDayOrderInfo(Info4, 13);
            string ExchangeSignFlag = getDayOrderInfo(Info4, 14);
            string BargainFlag = getDayOrderInfo(Info4, 12);
            string wholeFlag = getDayOrderInfo(Info4, 17);
            if ("1".Equals(wholeFlag))
            {
                DayComment = "整量 " + DayComment;
            }
            if ("1".Equals(OCOFlag))
            {
                OCOVisible = "0";
            }
            else
            {
                OCOVisible = "1";
            }
            if ("2".Equals(OCOFlag))
            {
                PackageVisible = "0";
            }
            else
            {
                PackageVisible = "1";
            }
            if ("1".Equals(ExchangeSignFlag))
            {
                SwitchVisible = "0";
            }
            else
            {
                SwitchVisible = "1";
            }
            if ("1".Equals(BargainFlag))
            {
                XVisible = "0";
                XXVisible = "1";
            }
            else if ("2".Equals(BargainFlag))
            {
                XVisible = "1";
                XXVisible = "0";
            }
            else
            {
                XVisible = "1";
                XXVisible = "1";
            }
            if (("0").Equals(NoMatchVisible))
            {
                IOCOrderQty = po.OrderQty;
                LeaveOrderQty = po.LeavesQty;// CommonUtils.ConvertToString(CommonUtils.ConvertToLong(po.OrderQty) - CommonUtils.ConvertToLong(po.CumQty));
                IOCPrice = po.Price;
                Count = po.Info2;
            }
        }

        private string getDayOrderInfo(string info7, int index)
        {
            string result = "";
            if (!string.IsNullOrEmpty(info7))
            {
                string[] info7Array = info7.Split('|');
                if (info7Array.Count() >= (index + 1))
                {
                    result = "null".Equals(info7Array[index]) ? "" : info7Array[index];
                }
            }
            return result;
        }
    }
}

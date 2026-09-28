using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Drp.Component.Broker
{
    public class UnconfirmDetailViewModel : ViewModelBase
    {
        private string _CounterPartyInstitutionName;
        private string _CounterPartyTraderName; 
        private string _DayPrice;
        private string _DayOrderQty;
        private string _DaySettlType;
        private string _DayComment;
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
        public void BindingData(ExecutionReport po)
        {
            string counterTraderName = po.CounterPartyTraderName;
            if (!string.IsNullOrEmpty(counterTraderName))
            {
                CounterPartyInstitutionName = po.CounterPartyInstitutionName + string.Format("({0})", counterTraderName);
            }
            else
            {
                CounterPartyInstitutionName = po.CounterPartyInstitutionName;
            }
            CounterPartyTraderName = po.CounterPartySubName;
            if (po.CounterPartySubID.Equals(po.PartySubID))
            {
                BrokerAIsMe = "1";
            }
            ExecutionReport er = getDayOrderInfo(po.Tag1);
            DayPrice = er.Price;
            DayOrderQty = er.OrderQty;
            DaySettlType = er.SettlType;
            DayComment = er.Text;
            string wholeFlag = er.WholeFlag;
            if ("1".Equals(wholeFlag))
            {
                DayComment = "整量 " + DayComment;
            }
        }
        private ExecutionReport getDayOrderInfo(string tag1)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ExecutionReport>(tag1);
        }
    }
}

using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Repo.Plugin.Model
{
    public class DetailBondItem : ViewModelBase
    {
        private string _code;

        public string Code
        {
            get { return _code; }
            set {
                _code = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _couponAmount;

        public decimal CouponAmount
        {
            get { return _couponAmount; }
            set {
                _couponAmount = value;
                this.OnPropertyChanged();
            }
        }

        private string _bondStartDate;

        public string BondStartDate
        {
            get { return _bondStartDate; }
            set { _bondStartDate = value;
                this.OnPropertyChanged(); }
        }

        private string _dueSettlementDate;

        public string DueSettlementDate
        {
            get { return _dueSettlementDate; }
            set { _dueSettlementDate = value;
                this.OnPropertyChanged();
            }
        }

    }
}

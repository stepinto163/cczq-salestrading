using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.Common.Model.WorkTable
{
    public class AssignVenuesWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// 是否显示利率债部分
        /// </summary>
        private Visibility _IsShowInterestRateDebt = Visibility.Collapsed;
        public Visibility IsShowInterestRateDebt
        {
            get
            {
                return _IsShowInterestRateDebt;
            }
            set
            {
                if (_IsShowInterestRateDebt != value)
                {
                    _IsShowInterestRateDebt = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否显示信用债部分
        /// </summary>
        private Visibility _IsShowCreditDebt = Visibility.Collapsed;
        public Visibility IsShowCreditDebt
        {
            get
            {
                return _IsShowCreditDebt;
            }
            set
            {
                if (_IsShowCreditDebt != value)
                {
                    _IsShowCreditDebt = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}

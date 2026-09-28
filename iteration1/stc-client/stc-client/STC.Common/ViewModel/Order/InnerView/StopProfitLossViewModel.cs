using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class StopProfitLossViewModel : BaseViewModel
    {

        private bool isCheckStopProfit = false;

        public bool IsCheckStopProfit { get { return isCheckStopProfit; } set { isCheckStopProfit = value; this.OnPropertyChanged(); } }

        private bool isCheckStopLoss = false;

        public bool IsCheckStopLoss { get { return isCheckStopLoss; } set { isCheckStopLoss = value;  this.OnPropertyChanged(); } }

        private string stopProfitTxt;
        public string StopProfitTxt
        {
            get { return stopProfitTxt; }
            set { stopProfitTxt = value; this.OnPropertyChanged(); }
        }

        private string stopLossTxt; 
        public string StopLossTxt
        {
            get { return stopLossTxt; }
            set { stopLossTxt = value; this.OnPropertyChanged(); }
        }

        private int selectType;
        public int SelectType
        {
            get { return selectType; }
            set { selectType = value; this.OnPropertyChanged(); }
        }

        #region 命令事件

        public ICommand AddStopProfitCommand { get { return new ComRelayCommand(OnAddStopProfitClick); } }
        public ICommand DownStopProfitCommand { get { return new ComRelayCommand(OnDownStopProfitClick); } }

        public ICommand AddStopLossCommand { get { return new ComRelayCommand(OnAddStopLossClick); } }
        public ICommand DownStopLossCommand { get { return new ComRelayCommand(OnDownStopLossClick); } }

        #endregion


        private void OnAddStopProfitClick()
        {
            if (selectType == 0)
            {
                double amount = 0;
                if (double.TryParse(stopProfitTxt, out amount))
                {
                    amount += 0.5;

                    StopProfitTxt = amount.ToString();
                }
            }
            else if (selectType == 1)
            {
                decimal amount = 0;
                if (decimal.TryParse(stopProfitTxt, out amount))
                {
                    amount += 1;

                    StopProfitTxt = amount.ToString();
                }
            }
        }

        private void OnDownStopProfitClick()
        {
            if (selectType == 0)
            {
                double amount = 0;
                if (double.TryParse(stopProfitTxt, out amount))
                {
                    amount -= 0.5;

                    StopProfitTxt = amount.ToString();
                }
            }
            else if (selectType == 1)
            {
                decimal amount = 0;
                if (decimal.TryParse(stopProfitTxt, out amount))
                {
                    amount -= 1;
                    if (amount >= 0)
                        StopProfitTxt = amount.ToString();
                }
            }
        }

        private void OnAddStopLossClick()
        {
            if (selectType == 0)
            {
                double amount = 0;
                if (double.TryParse(stopLossTxt, out amount))
                {
                    amount += 0.5;

                    StopLossTxt = amount.ToString();
                }
            }
            else if (selectType == 1)
            {
                decimal amount = 0;
                if (decimal.TryParse(stopLossTxt, out amount))
                {
                    amount += 1;
                    if (amount >= 0)
                        StopLossTxt = amount.ToString();
                }
            }
        }

        private void OnDownStopLossClick()
        {
            if (selectType == 0)
            {
                double amount = 0;
                if (double.TryParse(stopLossTxt, out amount))
                {
                    amount -= 0.5;

                    StopLossTxt = amount.ToString();
                }
            }
            else if (selectType == 1)
            {
                decimal amount = 0;
                if (decimal.TryParse(stopLossTxt, out amount))
                {
                    amount -= 1;
                    if (amount >= 0)
                        StopLossTxt = amount.ToString();
                }
            }
        }
    }
}

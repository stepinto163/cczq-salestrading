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
    public class IceViewModel : BaseViewModel
    {

        private string totalTxt;
        public string TotalTxt
        {
            get { return totalTxt; }
            set { totalTxt = value; this.OnPropertyChanged(); }
        }

        private string oneTxt;
        public string OneTxt
        {
            get { return oneTxt; }
            set { oneTxt = value; this.OnPropertyChanged(); }
        }


        #region 命令事件

        public ICommand AddTotalCommand { get { return new ComRelayCommand(OnAddTotalClick); } }
        public ICommand DownTotalCommand { get { return new ComRelayCommand(OnDownTotalClick); } }

        public ICommand AddOneCommand { get { return new ComRelayCommand(OnAddOneClick); } }
        public ICommand DownOneCommand { get { return new ComRelayCommand(OnDownOneClick); } }

        #endregion

        private void OnAddTotalClick()
        {
            double amount = 0;
            if (double.TryParse(TotalTxt, out amount))
            {
                amount += 1;

                TotalTxt = amount.ToString();
            }
        }

        private void OnDownTotalClick()
        {
            double amount = 0;
            if (double.TryParse(TotalTxt, out amount))
            {
                amount -= 1;

                if (amount >= 0)
                    TotalTxt = amount.ToString();
            }
        }

        private void OnAddOneClick()
        {
            double amount = 0;
            if (double.TryParse(OneTxt, out amount))
            {
                amount += 1;

                OneTxt = amount.ToString();
            }
        }

        private void OnDownOneClick()
        {
            double amount = 0;
            if (double.TryParse(OneTxt, out amount))
            {
                amount -= 1;

                if (amount >= 0)
                    OneTxt = amount.ToString();
            }
        }
    }
}

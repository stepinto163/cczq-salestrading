using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class SniperViewModel : BaseViewModel
    {

        private bool _checkAll = true;
        public bool CheckAll
        {
            get
            {
                return _checkAll;
            }

            set
            {
                _checkAll = value;
                this.OnPropertyChanged();

            }
        }

        private bool _checkAllEnable = false;
        public bool CheckAllEnable
        {
            get
            {
                return _checkAllEnable;
            }

            set
            {
                _checkAllEnable = value;
                this.OnPropertyChanged();

            }
        }


        private bool _checkNotOut = true;
        public bool CheckNotOut
        {
            get
            {
                return _checkNotOut;
            }

            set
            {
                _checkNotOut = value;

                this.OnPropertyChanged();
            }
        }

        private bool _checkNotoutEnable = false;
        public bool CheckNotoutEnable
        {
            get
            {
                return _checkNotoutEnable;
            }

            set
            {
                _checkNotoutEnable = value;
                this.OnPropertyChanged();
            }
        }

    }
}

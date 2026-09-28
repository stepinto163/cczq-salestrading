using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.IRS.Plugin.ViewModel.Data
{
    public class CopyColumnItem : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set 
            {
                _isChecked = value;
                this.OnPropertyChanged();
            }
        }

    }
}

using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Data
{
    public class AccountUserItem : BaseViewModel
    {
        private string _id;

        public string Id
        {
            get { return _id; }
            set 
            { 
                _id = value;
                this.OnPropertyChanged();
            }
        }

        private string _accountName;

        public string AccountName
        {
            get { return _accountName; }
            set { 
                _accountName = value;
                this.OnPropertyChanged();
            }
        }

    }
}

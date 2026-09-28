using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.ViewModel.Data
{
    public class GroupItems : BaseViewModel
    {
        private bool _isChecked = false;
        private string _groupId = string.Empty;
        private string _groupName = string.Empty;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }

            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    this.OnPropertyChanged();
                }
               
            }
        }

        public string GroupId
        {
            get
            {
                return _groupId;
            }

            set
            {
                if (_groupId != value)
                {
                    _groupId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string GroupName
        {
            get
            {
                return _groupName;
            }

            set
            {
                if (_groupName != value)
                {
                    _groupName = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}

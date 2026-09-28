using STC.Common.Model.FinQ;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.ViewModel.Data
{
    public class GroupsData : ViewModelBase
    {

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


        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set 
            { 
                _groupName = value;
                this.OnPropertyChanged();
            }
        }

        // 组内成员
        public ToOption GroupsToOption;
    }
}

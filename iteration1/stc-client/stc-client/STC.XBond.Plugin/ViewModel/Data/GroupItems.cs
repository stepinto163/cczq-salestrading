using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Data
{
    /// <summary>
    /// 收藏功能组信息
    /// </summary>
    public class GroupItems : ViewModelBase
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

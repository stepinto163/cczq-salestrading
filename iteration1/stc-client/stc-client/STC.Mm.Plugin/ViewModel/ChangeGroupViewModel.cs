using STC.Common.Model.MM;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.Mm.Plugin.ViewModel
{
    public class ChangeGroupViewModel : ViewModelBase
    {
        public ICommand CancelBtnCommand { get; set; }

        public ICommand ConfirmBtnCommand { get; set; }

        public ChangeGroupViewModel(string groupid)
        {
            if (this.CancelBtnCommand == null)
                this.CancelBtnCommand = new RelayCommand<Window>(CloseWindow);

            if (this.ConfirmBtnCommand == null)
                this.ConfirmBtnCommand = new RelayCommand<Window>(Confirm);

            if (Utils.Data.MMGroupHandler.Instance.CacheSortedGroups != null)
            {
                foreach (MMGroupInfo pair in Utils.Data.MMGroupHandler.Instance.CacheSortedGroups)
                {
                    if(pair.inf1 != MMConstants.MM_GROPE_TYPE_UNIFY)
                    {
                        ChangeGroupInfo cgi = new ChangeGroupInfo() { GroupID = pair.id, GroupName = pair.groupname, IsCurrent = pair.id == groupid };
                        GroupList.Add(cgi);
                        if (pair.id == groupid)
                        {
                            this.SelectedGroup = cgi;
                        }
                    }
                }
            }
        }

        private void Confirm(Window win)
        {
            if (win != null)
            {
                win.DialogResult = true;
                win.Close();
            }
        }

        private void CloseWindow(Window win)
        {
            if (win != null)
            {
                win.DialogResult = false;
                win.Close();
            }
        }

        private ChangeGroupInfo _SelectedGroup;

        public ChangeGroupInfo SelectedGroup
        {
            get { return _SelectedGroup; }
            set {
                if (_SelectedGroup != value)
                {
                    _SelectedGroup = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<ChangeGroupInfo> _GroupList = new ObservableCollection<ChangeGroupInfo>();

        public ObservableCollection<ChangeGroupInfo> GroupList
        {
            get
            {
                return _GroupList;
            }
            set
            {
                _GroupList = value;
                this.OnPropertyChanged();
            }
        }
    }
    
    public class ChangeGroupInfo : ViewModelBase
    {
        private string _groupid;

        public string GroupID
        {
            get { return _groupid; }
            set { _groupid = value;
                this.OnPropertyChanged();
            }
        }

        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }

        private bool _isCurrent;

        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { _isCurrent = value;
                this.OnPropertyChanged();
            }
        }
    }
}

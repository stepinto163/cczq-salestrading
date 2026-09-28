using STC.Common.Model.MMshe;
using STC.Mmshe.Plugin.Command;
using STC.Mmshe.Plugin.Validation;
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

namespace STC.Mmshe.Plugin.ViewModel
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
            List<MMsheGroupInfo> list = Utils.Data.MMsheGroupHandler.Instance.CacheSortedGroups.ToList();
            List<MMsheGroupInfo> selectMMsheGroupInfoList = Utils.Data.MMsheGroupHandler.Instance.CacheSortedGroups.Where(o => o.id == groupid).ToList();
            MMsheGroupInfo selectMMsheGroup = null;
            if (selectMMsheGroupInfoList != null && selectMMsheGroupInfoList.Count() > 0) 
            {
                selectMMsheGroup = selectMMsheGroupInfoList[0];

                foreach (MMsheGroupInfo info in list)
                {
                    if (info.venuetypegw == selectMMsheGroup.venuetypegw )
                    {
                        ChangeGroupInfo cgi = new ChangeGroupInfo() { GroupID = info.id, GroupName = info.groupname, IsCurrent = info.id == groupid };
                        GroupList.Add(cgi);
                        if (info.id == groupid)
                        {
                            this.SelectedGroup = cgi;
                        }
                    }
                }
            }
            //Dictionary<string, string> dic = Utils.Data.MMsheGroupHandler.Instance.CacheSortedGroups.ToDictionary(o => o.id, o => o.groupname);
            //foreach(KeyValuePair<string, string> pair in dic)
            //{
            //    ChangeGroupInfo cgi = new ChangeGroupInfo() { GroupID = pair.Key, GroupName = pair.Value, IsCurrent = pair.Key == groupid };
            //    GroupList.Add(cgi);
            //    if (pair.Key == groupid)
            //    {
            //        this.SelectedGroup = cgi;
            //    }
            //}
           
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

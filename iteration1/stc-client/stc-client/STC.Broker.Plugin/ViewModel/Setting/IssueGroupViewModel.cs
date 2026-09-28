using STC.Common.Model;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.ViewModel.Setting
{
    public class IssueGroupViewModel : BaseViewModel
    {
        public IssueGroupViewModel(IssueGroup issueGroup)
        {
            GroupId = issueGroup.groupId;
            Priority = issueGroup.priority;
            GroupName = issueGroup.groupName;
        }

        public string GroupId { get; set; }


        public int Priority { get; set; }


        private string groupName;
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否显示发单管理中机构分组删除按钮
        /// </summary>
        public Visibility ShowInstDeleteBtnVisibility
        {
            get
            {
                return LocalEnv.ConfInfo.ShowInstDeleteBtn ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }
}

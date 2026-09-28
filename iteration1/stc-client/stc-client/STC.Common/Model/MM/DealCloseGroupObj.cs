using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class DealCloseGroupObj
    {
        private string groupId;//分组id
        private string groupName;//分组名字
        private bool dealCloseGroup;//出现成交当前用户当前分组全撤全停 false-不启用 true-启用
        private bool groupCheckEnable;//是否可编辑

        public string GroupId
        {
            get
            {
                return groupId;
            }

            set
            {
                groupId = value;
            }
        }

        public string GroupName
        {
            get
            {
                return groupName;
            }

            set
            {
                groupName = value;
            }
        }

        public bool GroupCheckEnable
        {
            get
            {
                return groupCheckEnable;
            }

            set
            {
                groupCheckEnable = value;
            }
        }

        public bool DealCloseGroup
        {
            get
            {
                return dealCloseGroup;
            }

            set
            {
                dealCloseGroup = value;
            }
        }
    }
}

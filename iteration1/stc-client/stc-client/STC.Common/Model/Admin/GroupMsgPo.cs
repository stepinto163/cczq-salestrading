using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class GroupMsgPo
    {
        public string IsCareGroup;//false不会显示主页；true显示在主页
        public string IsDynamicsGroup;//true动态组
        public string IsAllInstGroup;//true全机构适用
        public string Tag;//标签
    }
}

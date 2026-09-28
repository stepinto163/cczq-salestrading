using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public  class PowerSettingModel
    {
        /// <summary>
        /// 查看指定人成交权限
        /// 可以查看其他人成交记录的人员
        /// </summary>
        public List<UserRoleSettingDetail> QueryOrders { get; set; }

        /// <summary>
        /// 查看指定人成交权限
        /// 可以查看其他人成交记录的人员
        /// </summary>
        public List<UserRoleSettingDetail> UpdateOrders { get; set; }

        /// <summary>
        /// 开/闭市权限
        /// </summary>
        public List<UserRoleSettingDetail> OpenCloses { get; set; }

        /// <summary>
        /// 不可查看他人历史成交
        /// </summary>
        public List<UserRoleSettingDetail> onlyQueryOwnHistoryOrder { get; set; }


        /// <summary>
        /// //桥权限
        /// </summary>
        public List<UserRoleSettingDetail> BridgePermissions { get; set; }
        /// <summary>
        /// 其他人桥权限
        /// </summary>
        public List<UserRoleSettingDetail> OtherBridgePermissions { get; set; }
    }
}

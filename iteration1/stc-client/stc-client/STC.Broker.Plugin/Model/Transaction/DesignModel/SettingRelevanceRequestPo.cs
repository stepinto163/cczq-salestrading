using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{
    public class SettingRelevanceRequestPo
    {
        /// <summary>
        /// 操作类型：1-新增；2-删除
        /// </summary>
        public int operateType { get; set; }
        /// <summary>
        /// 机构id
        /// </summary>
        public string instId { get; set; }
        /// <summary>
        /// 机构名称
        /// </summary>
        public string instName { get; set; }
        /// <summary>
        /// 关联机构id
        /// </summary>
        public string associatedInstId { get; set; }
        /// <summary>
        /// 关联机构名称
        /// </summary>
        public string associatedInstName { get; set; }

        /// <summary>
        /// 关联机构名称
        /// </summary>
        public  List<string> deleteList { get; set; }
    }
}

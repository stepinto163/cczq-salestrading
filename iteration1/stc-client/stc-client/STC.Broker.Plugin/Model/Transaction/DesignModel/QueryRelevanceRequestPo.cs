using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{

    /// <summary>
    /// 关联机构查询
    /// </summary>
    public class QueryRelevanceRequestPo
    {
        /// <summary>
        /// 模糊匹配机构名称
        /// </summary>
        public string InstName { get; set; }
    }
}

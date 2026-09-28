using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class BondGridDataPo
    {
        /// <summary>
        /// 债券ID
        /// </summary>
        public string BondID { get; set; }
        /// <summary>
        /// 债券简称
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// 剩余时间
        /// 前端计算得出，用于债券剩余期限的展示使用。
        /// 后台订单中使用的剩余期限由后台自己维护。
        /// </summary>
        public string RemaingingTime { get; set; }
        /// <summary>
        /// 资产类型
        /// </summary>
        public string Goodstype { get; set; }
        /// <summary>
        /// 资产二级类型
        /// </summary>
        public string Subtype { get; set; }
        /// <summary>
        /// 流通市场
        /// </summary>
        public string ListedMarket { get; set; }
        /// <summary>
        /// 基础利率代码
        /// </summary>
        public string FRNIndexID { get; set; }
    }
}

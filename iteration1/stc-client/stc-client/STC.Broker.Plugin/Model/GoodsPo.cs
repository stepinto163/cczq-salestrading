using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model
{
    public class GoodsPo
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string GoodsCode;
        /// <summary>
        /// 债券简称
        /// </summary>
        public string GoodsShortname;

        /// <summary>
        /// 市场
        /// </summary>
        public string MarketIndicator;
        /// <summary>
        /// 债券ID
        /// </summary>
        public string Id;
		/// <summary>
        /// 排序
        /// </summary>
        public int Sorting;
        /// <summary>
        /// 状态：是否是枚举值  QQQQQQQQQQQQ
        /// </summary>
        public string Status;
        /// <summary>
        /// 债券类别
        /// 债券类型:1信用,2利率,11 ABS,12 NCD
        /// </summary>
        public string Goodstype;
        /// <summary>
        /// 债券子类别  二级类型
        /// </summary>
        public string Subtype;

        /// <summary>
        /// 基础利率代码
        /// </summary>
        public string FRNIndexID { get; set; }
    }
}

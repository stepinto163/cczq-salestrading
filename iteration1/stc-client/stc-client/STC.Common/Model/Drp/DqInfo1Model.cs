using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
   public  class DqInfo1Model
    {
        #region DQ回价详情字段
        /// <summary>
        /// 报价人
        /// </summary>
        public string CnPartySubName { get; set; }

        /// <summary>
        /// 报价方向
        /// </summary>
        public string CnBidOrOfr { get; set; }

        /// <summary>
        /// 原始价格
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 原始量
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// 原始整量标记
        /// </summary>
        public string Wholeflag { get; set; }

        #endregion

        #region 兼容字段，避免程序出错

        /// <summary>
        /// 报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}"
        /// </summary>
        public string ValidTimeType;

        /// <summary>
        /// 过期时间  yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string ValidUntilTime;

        #endregion
    }
}

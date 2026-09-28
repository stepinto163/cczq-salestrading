using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    /// <summary>
    /// 双边剔除行情
    /// </summary>
    public class MMFilterQuoteReq
    {
        /// <summary>
        /// 必填报价id "220317020401001802"
        /// </summary>
        public string quoteid;

        /// <summary>
        /// 必填方向 "0" 
        /// </summary>
        public string side;

        /// <summary>
        /// 必填债券id "147709.IB"
        /// </summary>
        public string securityid;

        /// <summary>
        /// 必填债券简称 ""
        /// </summary>
        public string symbol;

        /// <summary>
        /// 必填启用标记 1:启用，0:不启用 
        /// </summary>
        public string enable;

        /// <summary>
        /// 必填 1-人工
        /// </summary>
        public string type;

        /// <summary>
        /// 必填1：新增  2：修改   
        /// </summary>
        public string action;

        /// <summary>
        /// 机构六位码
        /// </summary>
        public string partycode6;

        /// <summary>
        /// 机构简称
        /// </summary>
        public string cnshortname;

        /// <summary>
        /// 行情时间
        /// </summary>
        public string transacttime;
    }
}

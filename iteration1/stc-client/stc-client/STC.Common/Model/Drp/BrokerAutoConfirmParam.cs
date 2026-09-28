using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    public class BrokerAutoConfirmParam
    {
        /// <summary>
        /// BrokerA 全部成交自动确认
        /// </summary>
        public string brokerAAutoConfirm_BrokerBFiledConfirm = "Y";
        /// <summary>
        /// BrokerA 部分成交自动确认
        /// </summary>
        public string brokerAAutoConfirm_BrokerBPartiallyFiledConfirm = "N";
        
        /// <summary>
        /// 我的客户发来意向是否需要弹窗
        /// </summary>
        public string brokerAShowBox_BrokerAFilledNew = "Y";

        /// <summary>
        /// A确认我的客户被点价是否需要弹窗
        /// </summary>
        public string brokerBShowBox_BrokerAFiledConfirm = "N";

        /// <summary>
        /// B部分确认我的客户被点价是否需要弹窗
        /// </summary>
        public string brokerBShowBox_BrokerBPartiallyFiledConfirm = "N";
    }
}

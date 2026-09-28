using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{
    public class PaymentInstRequestPo {
        /// <summary>
        /// 操作类型
        /// </summary>
        public string businessOperateType { get; set; }
        /// <summary>
        /// 机构名称
        /// </summary>
        public List<PaymentInstDetail> datas { get; set; }
    }

    public class PaymentInstDetail {
        /// <summary>
        /// 机构id
        /// </summary>
        public string instId { get; set; }
        /// <summary>
        /// 机构名称
        /// </summary>
        public string instName { get; set; }
        /// <summary>
        /// 机构全称
        /// </summary>
        public string instFullName { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 业务类型：1-代付费用机构；2-外发手工发送机构管理；3-外发加桥发送机构管理
        /// </summary>
        public int businessType { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string createby { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime { get; set; }
    }

}

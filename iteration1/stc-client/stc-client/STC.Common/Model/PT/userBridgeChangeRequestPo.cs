using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 桥 增删改 
    /// </summary>
    public class UserBridgeChangeRequestPo
    {
        /// <summary>
        /// 桥操作类型（0新增，1修改，2删除，3:查询桥）
        /// </summary>
        public string bridgeChangeType;
        /// <summary>
        /// 主键 后端生成，删除时候用
        /// </summary>
        public string bridgeId;
        /// <summary>
        /// brokerID
        /// </summary>
        public string partySubid;
        /// <summary>
        /// broker名称
        /// </summary>
        public string partySubname;
        /// <summary>
        /// 机构id
        /// </summary>
        public string bankInstitutionid;
        /// <summary>
        /// 机构名称
        /// </summary>
        public string bankInstitutionname;
        /// <summary>
        /// 交易员id
        /// </summary>
        public string bankTraderid;
        /// <summary>
        /// //交易员名称
        /// </summary>
        public string bankTradername;
        /// <summary>
        /// 联系方式
        /// </summary>
        public string contactType;
        /// <summary>
        /// 机构id-计费
        /// </summary>
        public string feeTraderid;
        /// <summary>
        /// 机构名称-计费
        /// </summary>
        public string feeTradername;
        /// <summary>
        /// 交易员ID-计费
        /// </summary>
        public string feeInstitutionid;
        /// <summary>
        /// 交易员名称-计费
        /// </summary>
        public string feeInstitutionname;
        /// <summary>
        /// //渠道：1:对话，2:请求,3:双边,4:xbond,5:ideal
        /// </summary>
        public string channel;
        /// <summary>
        /// 发给
        /// </summary>
        public string sendto;
        /// <summary>
        /// //备注
        /// </summary>
        public string remark;
    }
}

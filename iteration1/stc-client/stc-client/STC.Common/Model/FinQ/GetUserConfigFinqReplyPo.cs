using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.FinQ
{
    public class GetUserConfigFinqReplyPo : BaseReplyPo
    {
        public UserConfigFinq data;
    }

    public class UserConfigFinq
    {
        /// <summary>
        /// 1:国利 2：国际 3：中诚 4：平安 5：信唐 6：上田
        /// </summary>
        public Dictionary<string, List<SendIntentBrokers>> sendIntentBrokers;

        /// <summary>
        /// 常用联系人
        /// </summary>
        public List<UserConfig> sendIntentContacts;

        /// <summary>
        /// 对手方
        /// </summary>
        public List<UserConfig> sendIntentCounterparties;

        /// <summary>
        /// 自定义分组
        /// </summary>
        public List<SendIntentGroups> sendIntentGroups;

        /// <summary>
        /// QO群组
        /// </summary>
        public List<QQGroupConfig> sendIntentQQGroups;
    }

    /// <summary>
    /// broker
    /// </summary>
    public class SendIntentBrokers
    {
        public string qq;
        public string securityType;
        public string name;
    }

    /// <summary>
    /// 常用联系人
    /// </summary>
    public class UserConfig
    {
        // 企点 QQ
        public string qq;
        
        // 对外名称
        public string name;
    }

    public class SendIntentGroups
    {
        // 分组名称
        public string groupName;

        // 组内成员
        public List<UserConfig> members;
    }

    public class QQGroupConfig
    {
        // 群openid
        public string groupOpenID;

        // 群名称
        public string groupName;
    }

    public class ToOption
    {
        public string OptionType { get; set; }

        public string DispayName { get; set; }

        /// <summary>
        /// broker
        /// </summary>
        public SendIntentBrokers Broker { get; set; }

        public string BrokerCode { get; set; }

        /// <summary>
        /// 常用联系人，对手方
        /// </summary>
        public UserConfig Sender { get; set; }

        /// <summary>
        /// 自定义分组
        /// </summary>
        public SendIntentGroups Groups { get; set; }

        /// <summary>
        /// QQ群组
        /// </summary>
        public QQGroupConfig QQGroups { get; set; }

        /// <summary>
        /// 交易员
        /// </summary>
        public string TraderBuyer { get; set; }

        /// <summary>
        /// 交易员名称
        /// </summary>
        public string TraderName_Buyer { get; set; }

        /// <summary>
        /// 剩余期限
        /// </summary>
        public string RemainingTime { get; set; }

        /// <summary>
        /// 债券名称
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// 1:信用;2:利率债(其他);3：NCD;21：地方债
        /// </summary>
        public string BondType { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.TBook
{
    public class UserSetting
    {
        public List<Venue> systemVenue; // 系统渠道
        public List<UserVenue> userVenue;// 用户渠道偏好

        public string userQty;
        public string idbOrderTimeOut;
        public string userNofirmIOCTimeOut;
        public string validUntiTimeSetting;

        /// <summary>
        /// 1: cross
        /// 2: stp
        /// </summary>
        private string tradeType = "1";

        public string TradeType { get { return tradeType; } set { tradeType = value; } }

        public string inf1;
        public string inf2;
        public string inf3;// 订单匹配后更新原始订单

        public string priceCondition;
    }

    public class ConsistencyCheckPo
    {
        public string PointPriceConsistencyCheck;
        public string SendPriceConsistencyCheck;

        public string PointPriceQty;

        /// <summary>
        /// 格式:TPSC:CNEX:PTCN
        /// </summary>
        public string BrokerSort;

        /// <summary>
        /// ValidTime 默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
        /// </summary>
        public string DefaultValidTime;

        /// <summary>
        /// Rfq 时长 "1|{min}:{sec}
        /// </summary>
        public string RfqDefaultValidTime;

        /// <summary>
        /// 一创 默认组合账号设置
        /// </summary>
        public string AccountId;

        /// <summary>
        /// 默认类型 0(nonfirm)，未选中：1(firm)
        /// </summary>
        public string DefaultFirmType;

        /// <summary>
        /// 订阅该券同向 0 - 未订阅 1订阅
        /// </summary>
        public string SubscribeSyntropy;

        /// <summary>
        /// 订阅该券反向  0 - 未订阅 1订阅
        /// </summary>
        public string SubscribeReverse;

        /// <summary>
        /// 消息铃铛弹出样式 1 弹出 0 固定
        /// </summary>
        public string MessageBellPopupStyle;

        /// <summary>
        /// 在"悬浮"模式,支持“弹框”复选框，不选的话0：“只跳数字不弹框” 1：跳数字且弹框
        /// </summary>
        public string PopMsgMode;

        /// <summary>
        /// 手工单默认组合ID
        /// </summary>
        public string BookId;
        /// <summary>
        /// 手工单默认组合名称
        /// </summary>
        public string BookName;
    }
}

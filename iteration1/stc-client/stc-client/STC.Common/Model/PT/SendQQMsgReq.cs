using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class SendQQMsgReq
    {
        /**
          * 机构ID
          */
        public string institutionId;

        /**
         * 机构名称
         */
        public string institutionName;
        /**
         * 交易员ID
         */
        public string traderId;

        /**
         * 交易员名称
         */
        public string traderName;

        /**
         * 本方用户ID
         */
        public string partySubId;

        /**
         * 本方用户名称
         */
        public string partySubName;

        /**
         * 对手方用户ID
         */
        public string counterPartySubId;

        /**
         * 对手方用户名称
         */
        public string counterPartySubName;

        /**
         * 消息内容
         */
        public string message;

        /**
         * 发送类型
         * "1" ：发给本方交易员  "2"：发给对手方broker,催单， "3"：发送单条消息给本方交易员，追加消息
         */
        public string sendType;

        /**
         * 操作类型 为空，单纯发送消息，客户端传什么发什么
         * 47/48 要拼接消息
         * message + 换行 + 拼接的消息
         */
        public string commandType;

        /**
         * 成交单ID
         */
        public string execId;

        public string userName;
        /**
         * 是否追加消息 0：否 1：是
         */
        public string ifAppend;

        public string HideCounterpartySend { get; set; }
    }
}

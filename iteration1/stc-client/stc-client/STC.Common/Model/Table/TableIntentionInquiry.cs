using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class TableIntentionInquiry
    {
        public string id;

        // 部门
        public string biztypeid;

        public string inquiryKey;//??

        public string intentionId;

        public string securityId;

        public string side;

        public decimal? price;

        public decimal? qty;

        public string name;

        public string userName;

        public string userId;

        public string institutionId;

        public string institutionName;

        public string traderId;

        public string traderName;

        public string settlType;

        /**
         * 0:未发布 1：已发布 4:已撤销 -1:发布失败
         */
        public string status;
        /// <summary>
        /// 0:查询 1:新增-发布 2:新增-不发布了:修改-发布 4:修改-不 5:REF-发布 6:REF-不发布
        /// </summary>
        public string action;

        public string tags;

        public string info1;

        public string info2;

        public string info3;

        public string info4;

        public string info5;

        public string createTime;

        public string updateTime;

        /**
        * 发送方QQ号码
        */
        public string senderNumber;

        /**
         * 接收方QQ号码
         */
        public string receiverNumber;

        /// <summary>
        /// 接收方名称
        /// </summary>
        public string receiverName;

        /**
        * FQ报价ID
        */
        public string quoteId;

        /**
         * 格式为：yyyyMMdd
         */
        public string settlDate;

        /**
         * 0=无星, 1=*, 2=**
         */
        public string bargainFlag;

        /**
         * P*标记 N=无, Y=P*
         */
        public string quoteDividedFlag;

        /**
         * 0=无OCO, 1=OCO, 2=打包
         */
        public string ocoFlag;

        /**
         * 发布FQ返回信息
         */
        public string pubFQMsg;

        /**
         * 发布FQ返回的代码
         */
        public string pubFQCode;

        /// <summary>
        /// 群发信息
        /// </summary>
        public string text;

        /**
         * 群聊群号
         */
        public string receiverGroupOpenId;

        /**
         * 群聊群名称
         */
        public string receiverGroupName;

        /**
         * 批量私聊接收方QQ号码列表
         * 格式为：{"QQ号码1":"名称1", "QQ号码2":"名称2"}
         */
        public Dictionary<string, string> batchReceiverNumberMap;

        /**
         * 0:私聊 1：群聊 2：批量私聊  3：GVN,TNK  
         * 411:批量询价-私聊（broker)  receiverNumber receiverName
         * 412:批量询价-私聊（对手方)  receiverNumber receiverName
         * 413:批量询价-私聊（同事)    receiverNumber receiverName
         * 42:批量询价-群聊            receiverGroupOpenId   receiverGroupName
         * 43:批量询价-批量私聊        receiverGroupName   batchReceiverNumberMap
         */
        public int sendType;

        public string quoteType;

        public TableIntentionInquiry Clone(TableIntentionInquiry obj)
        {
            TableIntentionInquiry newObj = new TableIntentionInquiry();
            this.id = obj.id;
            this.biztypeid = obj.biztypeid;
            this.inquiryKey = obj.inquiryKey;
            this.intentionId = obj.intentionId;
            this.securityId = obj.securityId;
            this.side = obj.side;
            this.price = obj.price;
            this.qty = obj.qty;
            this.name = obj.name;
            this.userName = obj.userName;
            this.userId = obj.userId;
            this.institutionId = obj.institutionId;
            this.institutionName = obj.institutionName;
            this.traderId = obj.traderId;
            this.traderName = obj.traderName;
            this.settlType = obj.settlType;
            this.status = obj.status;
            this.action = obj.action;
            this.tags = obj.tags;
            this.info1 = obj.info1;
            this.info2 = obj.info2;
            this.info3 = obj.info3;
            this.info4 = obj.info4;
            this.info5 = obj.info5;
            this.createTime = obj.createTime;
            this.updateTime = obj.updateTime;
            this.senderNumber = obj.senderNumber;
            this.receiverNumber = obj.receiverNumber;
            this.receiverName = obj.receiverName;
            this.quoteId = obj.quoteId;
            this.settlDate = obj.settlDate;
            this.bargainFlag = obj.bargainFlag;
            this.quoteDividedFlag = obj.quoteDividedFlag;
            this.ocoFlag = obj.ocoFlag;
            this.pubFQMsg = obj.pubFQMsg;
            this.pubFQCode = obj.pubFQCode;
            this.text = obj.text;
            this.receiverGroupOpenId = obj.receiverGroupOpenId;
            this.receiverGroupName = obj.receiverGroupName;
            if (obj.batchReceiverNumberMap != null)
            {
                this.batchReceiverNumberMap = new Dictionary<string, string>();
                foreach(KeyValuePair<string, string> pair in obj.batchReceiverNumberMap)
                {
                    this.batchReceiverNumberMap[pair.Key] = pair.Value;
                }
            }
            this.sendType = obj.sendType;
            this.quoteType = obj.quoteType;
            this.clOrdID = obj.clOrdID;
            this.execOrderId = obj.execOrderId;
            this.venue = obj.venue;
            this.venueTypeGW = obj.venueTypeGW;
            return newObj;
        }

        public string venue;

        public string venueTypeGW;

        public string strategyOrderId;

        public string clOrdID;

        public string execOrderId;
    }
}

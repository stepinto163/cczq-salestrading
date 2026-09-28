using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class UpdateIntentionQuoteRequest
    {
        // 意向报价唯一编号
        public string intentionId;
        // 债券代码
        public string securityid;
        // 价格
        public decimal? price;
        // 全价
        public decimal? fullPrice;
        // 净价
        public decimal? cleanPrice;
        // 报量
        public decimal? qty;
        // 清算类型
        public string settlType;
        // 整量标识
        public string wholeFlag;
        // 交易员编号
        public string traderId;
        // 交易员名称
        public string traderName;
        // 经纪商编号
        public string brokerId;
        // 经纪商名称
        public string brokerName;
        // 机构编号
        public string institutionId;
        // 机构名称
        public string institutionName;
        // 方向
        public string side;

        public string userName;

        public string name;

        public string closeBy;
        public string closeByName;

        public string updateTime;

        public string info2;
        public string info5;
        public string inf1;

        public string VipCode;
        public string OrgType;
        public string IfSubBond;

        #region 标签，点差
        /**
         * 急
         */
        public string hurryFlag;
        /**
         * 荐
         */
        public string suggestFlag;
        /**
         * 火
         */
        public string fireFlag;
        /**
         * 底价
         */
        public string floorPrice;
        /**
         * 客户报价
         */
        public decimal? customerPrice;
        /**
         * 点差
         */
        public decimal? customerSpread;

        /**
         * 公开备注
         */
        public string publicRemarks;

        /**
         * 个人备注
         */
        public string personalRemarks;
        /**
         * 报价来源
         */
        public string channel;

        /// <summary>
        /// 1:oco 2:打包
        /// </summary>
        public string OcoFlag;

        ///// <summary>
        ///// 换券标识 true: 换券; false 不换券
        ///// </summary>
        //public bool ExchangeSignFlag;

        ///// <summary>
        ///// 值
        ///// </summary>
        //public string FanDian;

        ///// <summary>
        ///// 返点标签， 1 数字,2文本
        ///// </summary>
        //public string FanDianFlag;

        /// <summary>
        /// 暗盘标记 1暗盘，2明盘
        /// </summary>
        public string Handlinst;

        ///// <summary>
        ///// 3-收益率（到期） 7-行权 1-净价;2- 全价;
        ///// </summary>
        //public string PriceType;
        #endregion

        public string QuoteType;

        public string counterPartyID;

        public string counterPartyName;

        public string counterPartySubID;

        public string counterPartySubName;

        public string readFlag;

        /// <summary>
        /// BUYER_ROLE_TRADER：交易员（0100）
        /// BUYER_ROLE_MANAGER：投资经理（0011）
        /// </summary>
        public string roletype;

        public string clordid;

        public string partyTraderName; //交易员名称

        public string partyManagerName;//投资经理

        public string partyTrader; //交易员名称(username)

        public string partyManager;//投资经理(username)

        public string bookId; // 分组id
        public string bookName; // 分组名称
        public string accountID; // 分组账号id
        public string accountName; // 分组账号名称
        public string riskReTryFlag;//风控试算
        public string riskManualCk;//跳过风控校验
        public string actionFlag;
        public string relatedUsers;//对谁可见
        public string priceType;//价格类型
    }
}

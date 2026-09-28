using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class IntentionQuoteInfo
    {
        // 意向报价唯一编号
        public string intentionId;
        // 机构编号
        public string institutionId;
        // 机构名称
        public string institutionName;
        // 交易员编号
        public string traderId;
        // 交易员名称
        public string traderName;
        // 经纪商编号
        public string brokerId;
        // 经纪商名称
        public string brokerName;
        //恒生基金编号
        public String hsProductId;
        // QQ号码
        public string qqNumber;
        // 债券编号
        public string securityId;
        // 债券简称
        public string symbol;
        // 报价方向 1=买、2=卖
        public string side;
        // 价格
        public decimal? price;
        // 净价
        public decimal? cleanPrice;
        // 全价
        public decimal? fullPrice;
        // 报量
        public decimal? qty;
        
        // 成交量
        public decimal? executionqty;

        // 清算类型 1=T+0, 2=T+1, 3=Tm+0, 4=Tm+1, 99=其他
        public string settlType;
        // 原始文本 
        public string text;
        // 订单状态 ,0=待处理,1=已处理,2=已成交, 3=已忽略
        public string status;

        public string subStatus; //子状态

        ///// <summary>
        ///// 0:无询价  1：有询价
        ///// </summary>
        //public string inquiryStatus;
        // 创建时间
        public string createTime;
        // 更新时间
        public string updateTime;
        // 操作人
        public string closeBy;
        // 操作人中文名
        public string closeByName;
        // 我的订单号
        public string orderId;
        //
        public string type;
        // 用户id
        public string userId;
        // 用户名
        public string userName;
        // 用户昵称
        public string nickName;
        // 价格区间-起始价
        public decimal? startPrice;
        // 价格区间-结尾价
        public decimal? endPrice;
        // 簿记量
        public decimal? bookQty;
        // 剩余量
        public decimal? leftQty;
        // 盈亏值
        public decimal? profitLoss;
        // 报量完成度
        public string completeRate;
        // 运行时长
        public string runningTime;
        // 意向报价标签，逗号分割
        public string tagList;
        // 整量
        public string wholeFlag;
        // 识别trader
        public String parseTraderIdbkey;
        // 识别Company
        public String parseCompanyIdbkey;
        // type标志
        public string firmFlag;

        // clordid
        public string clordid;

        //投资组合
        public string accountID;
        public string accountName;

        public string vipCode;//"0001"（勾选）、"0010"(不勾选）
        public string orgType;//"0001"（银行配置户）、"0010"（券商）、"0011"（基金）、"0100"（资管）、"0101"（保险）、"0110"（其他）、"0111"（银行交易户）、"1000"（理财子公司）、"1001"（期货机构）、"1010"（信托）
        public string ifSubBond;//"BUY"(同向）,"SELL"（反向）,"ALL"（全选）
        public string validuntitime;
        public string inf1; // 与validUnitlTime配合使用
        public string name;
        //到期时间
        public String timeToMaturity;
        //到期日距离下个工作日天数
        public String timeToMaturityToWorkDay;

        //价格类型
        public string priceType;

        // 对应价格类型的价格
        public decimal? tempPrice;

        public string info2;
        public string info5;

        public string inf3;

        #region IRS
        public string BondID;
        //public string TimeToMaturity;
        public decimal? quantity;
        //原文
        public string original;
        //parseType = IRS
        public string parseType;
        #endregion

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

        ///// <summary>
        ///// 暗盘标记 1暗盘，2明盘
        ///// </summary>
        //public string Handlinst;

        ///// <summary>
        ///// 3-收益率（到期） 7-行权 1-净价;2- 全价;
        ///// </summary>
        //public string PriceType;
        #endregion

        public long sign;
        public long sign1;
        public long sign2;
        public long sign3;
        public long sign4;
        //期限（天）
        public decimal? termDays;
        //最新票面利率
        public decimal? couponRateCurrent;
        //中债估值
        public decimal? chinaBondValuation;
        //中债估值（复制）
        public string chinaBondValuationFull;
        //发行人代码
        public string issuerCode;
        //发行人名称
        public string issuerName;
        //担保人代码
        public string warranterCode;
        //担保人名称
        public string warranterName;

        //麦氏久期
        public decimal? macaulayDuration;
        //修正久期
        public decimal? modifiedDuration;
        //pvbp
        public decimal? pvbp;
        //凸性
        public decimal? convexity;

        //到期日距离下个工作日天数（休n/x）
        public string dayOffStr;

        //发行量
        public decimal? issueAmount;

        /**
         * 中证估值
         */
        public decimal? csiBondValuation;

        /**
         * 中证估值全价
         */
        public string csiBondValuationFull;

        // 债券金额
        public string outstandingAmount;

        /// <summary>
        /// INNER("1", "INNER"),
        /// CDH("2", "CDH");
        /// </summary>
        public string orderSource;

        /// <summary>
        /// 剩余期限
        /// </summary>
        public string tenor;

        public string roleid;
        public string rolename;

        /// <summary>
        /// INTENTION，DEMAND
        /// </summary>
        public string quotetype;

        public string handlinst;

        public string clordId;

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

        public string partyTraderName; //交易员名称

        public string partyManagerName;//投资经理

        public string partyTrader; //交易员名称(username)

        public string partyManager;//投资经理(username)

        public string bizType;

        #region 平安养老
        public string bookId;//分组id

        public string bookName;//分组名称
        #endregion

        public string relatedUsers;//对谁可见
        public string delflag;

        public decimal? Yieldtoexecution;//行权价
    }
}

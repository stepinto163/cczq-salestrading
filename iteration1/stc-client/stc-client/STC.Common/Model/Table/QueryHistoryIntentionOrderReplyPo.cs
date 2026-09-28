using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryHistoryIntentionOrderReplyPo : BaseReplyPo
    {
        public int totalSize { get; set; }
        public List<HistoryIntentionOrderReport> Datas;
    }

    public class HistoryIntentionOrderReport
    {
        /// <summary>
        /// 意向报价编号
        /// </summary>
        public string intentionId;
        /// <summary>
        /// 机构编号
        /// </summary>
        public string institutionId;
        /// <summary>
        /// 机构名称
        /// </summary>
        public string institutionName;
        /// <summary>
        /// 客户编号
        /// </summary>
        public string traderId;
        /// <summary>
        /// 客户名称
        /// </summary>
        public string traderName;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string name;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string userId;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName;

        /// <summary>
        /// QQ号码
        /// </summary>
        public string qqNumber;

        /// <summary>
        /// 债券编号
        /// </summary>
        public string securityId;

        /// <summary>
        /// 债券简称
        /// </summary>
        public string symbol;

        /// <summary>
        /// 方向
        /// </summary>
        public string side;

        /// <summary>
        /// 价格
        /// </summary>
        public decimal? price;

        /// <summary>
        /// 净价
        /// </summary>
        public decimal? cleanPrice;

        /// <summary>
        /// 全价
        /// </summary>
        public decimal? fullPrice;

        /// <summary>
        /// 报量
        /// </summary>
        public string qty;

        /// <summary>
        /// 清算类型
        /// </summary>
        public string settlType;

        /// <summary>
        /// 原始文本
        /// </summary>
        public string text;

        /// <summary>
        /// 意向报价状态
        /// </summary>
        public string status;

        /// <summary>
        /// 确定操作类型 1=普通确认,2=闪电确认,3=T/G确认,4=忽略,5=恢复
        /// </summary>
        public string type;

        /// <summary>
        /// 策略订单编号
        /// </summary>
        public string orderId;

        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime;

        /// <summary>
        /// 更新时间
        /// </summary>
        public string updateTime;

        /// <summary>
        /// 操作人
        /// </summary>
        public string closeBy;

        /// <summary>
        /// 是否整量
        /// </summary>
        public string wholeflag;

        /// <summary>
        /// 角色id
        /// </summary>
        public string roleid;

        /// <summary>
        /// 角色名称
        /// </summary>
        public string rolename;

        /// <summary>
        /// 本交易员Name
        /// </summary>
        public string partysubname;

        /// <summary>
        /// 识别交易员idbkey
        /// </summary>
        public string parseTraderIdbkey;

        /// <summary>
        /// 识别机构idbkey
        /// </summary>
        public string parseCompanyIdbkey;

        /// <summary>
        /// 投组账号id
        /// </summary>
        public string accountID;

        /// <summary>
        /// 投组账号名称
        /// </summary>
        public string accountName;

        /// <summary>
        /// VIP："0001"（勾选）、"0010"(不勾选）
        /// </summary>
        public string vipCode;

        /// <summary>
        /// 机构类型:"0001"（银行配置户）、"0010"（券商）、"0011"（基金）、"0100"（资管）、"0101"（保险）、"0110"（其他）、"0111"（银行交易户）、"1000"（理财子公司）、"1001"（期货机构）、"1010"（信托）
        /// </summary>
        public string orgType;

        /// <summary>
        /// 订阅该券同向/反向/全选："BID/OFR"(同向/反向）,"ALL"（全选）
        /// </summary>
        public string ifSubBond;

        /// <summary>
        /// 有效期
        /// </summary>
        public string validuntitime;

        /// <summary>
        /// 有效期扩展字段，编辑订单回显
        /// </summary>
        public string inf1;

        /// <summary>
        /// 标签
        /// </summary>
        public string inf2;

        /// <summary>
        /// oco, 打包
        /// </summary>
        public string ocoflag;

        /// <summary>
        /// 标签
        /// </summary>
        public long sign;

        /// <summary>
        /// 标签
        /// </summary>
        public long sign1;

        /// <summary>
        /// 标签
        /// </summary>
        public long sign2;

        /// <summary>
        /// 标签
        /// </summary>
        public long sign3;

        /// <summary>
        /// 标签
        /// </summary>
        public long sign4;

        /// <summary>
        /// 撤销类型："1"人工，"2"成交，"3"自动
        /// </summary>
        public string reftype;

        /// <summary>
        /// 
        /// </summary>
        public string firmflag;

        /// <summary>
        /// 动态期限的天数 3650,7300
        /// </summary>
        public decimal? termDays;

        /// <summary>
        /// 发行人代码
        /// </summary>
        public string issuerCode;

        /// <summary>
        /// 发行人名称
        /// </summary>
        public string issuerName;

        /// <summary>
        /// 担保人代码
        /// </summary>
        public string warranterCode;

        /// <summary>
        /// 担保人名称
        /// </summary>
        public string warranterName;

        /// <summary>
        /// 最新票面利率
        /// </summary>
        public decimal? couponRateCurrent;

        /// <summary>
        /// 中债估值
        /// </summary>
        public decimal? chinaBondValuation;

        /// <summary>
        /// 中债估值
        /// </summary>
        public string chinaBondValuationFull;

        /// <summary>
        /// 麦氏久期
        /// </summary>
        public decimal? macaulayDuration;

        /// <summary>
        /// 修正久期
        /// </summary>
        public decimal? modifiedDuration;

        /// <summary>
        /// 到期日距离下个工作日天数（休n/x）
        /// </summary>
        public string dayOffStr;

        /// <summary>
        /// 发行量
        /// </summary>
        public decimal? issueAmount;

        /// <summary>
        /// 操作人（中文）
        /// </summary>
        public string closeByName;

        /**
         * 中证估值
         */
        public decimal? csiBondValuation;

        /**
         * 中证估值全价
         */
        public string csiBondValuationFull;

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
        #endregion

        public string priceType;
    }

}

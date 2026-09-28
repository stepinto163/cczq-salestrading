using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class ConfirmIntentionQuoteRequest
    {
        // 意向报价唯一编号
        public string intentionId;
        // 确认类型,1=普通确认,2=闪电确认,3=T/G确认,4=忽略,5=恢复
        public string actionType;
        // stc用户
        public string userName;
        // 交易员编号
        public string partySubID;
        // 客户端订单编号
        public string clOrdId;
        // 风控检查标志，为空则风控检查,1=只检查风控禁止项,2=不进行风控检查
        public string riskManualCk;
        // 暗盘标记,1=暗盘, 2=明盘
        public string handlInst;
        // 是否整量,1=整量, 0=不整量
        public string wholeFlag;
        // 渠道管理,1=所有,2=内部,3=内部+XBOND
        public string corssVenues;
        // 渠道类型,不填写1,默认2,指定3,二档随机4
        public string venuesSelectF;
        // 渠道类型默认，填写随机几家渠道,例如：2(表示两家)
        public string venuesRdmNum;
        // 指定渠道，多家用逗号隔开,例如：SOR-CTTJ,SOR-TPSC
        public string selfSelectVenues;
        // 扩展字段
        public string inf1;
        public string inf2;
        public string info2;
        public string Info3;
        public string Info4;
        public string info5;
        public string Info6;

        public string accountID;
        public string accountName;

        public string vipCode;//"0001"（勾选）、"0010"(不勾选）
        public string orgType;//"0001"（银行配置户）、"0010"（券商）、"0011"（基金）、"0100"（资管）、"0101"（保险）、"0110"（其他）、"0111"（银行交易户）、"1000"（理财子公司）、"1001"（期货机构）、"1010"（信托）
        public string ifSubBond;//"BUY"(同向）,"SELL"（反向）,"ALL"（全选）
        public string validUntilTime;
        public string closeBy;
        public string closeByName;
        //默认填写 "3" //收益率
        public string priceType = "3";

        public string updateTime;

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

        public string quoteType;

        public string riskReTryFlag;//风控试算
        public string actionFlag;
        public string relatedUsers;//对谁可见
    }
}

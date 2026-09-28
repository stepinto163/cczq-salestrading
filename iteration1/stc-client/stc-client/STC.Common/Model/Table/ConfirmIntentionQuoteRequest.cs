using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class ConfirmIntentionQuoteRequest
    {
        // 意向报价唯一编号
        public string intentionId;
        // 替换报价编号
        public string replaceId;
        // stc用户
        public string userName;
        // 确认类型,1=普通确认,2=闪电确认,3=忽略,4=恢复,5=暂停,6=继续
        public string actionType;
        // 客户端订单编号
        public string clOrdId;
        // 交易平台
        public string venueTypeGw;
        // 交易渠道
        public string venues;
        // 订单类型
        public string orderType;
        //// 下单价
        //public string orderPrice;
        //// 下单量
        //public string orderQty;
        // 清算速度
        public string settlType;
        // 有效时间
        public string timeInForce;
        // 订单过期时间
        public string validUntilTime;
        // firm标记
        public string firmFlag;
        // 是否整量：1=整量 0=不整量
        public string wholeFlag;
        // 风控检查标志，为空则风控检查,1=只检查风控禁止项,2=不进行风控检查
        public string riskManualCk;
        // 暗盘标记,1=暗盘, 2=明盘
        public string handlInst;
        // 渠道管理,1=所有,2=内部,3=内部+XBOND
        public string crossVenues;
        // 渠道类型,不填写1,默认2,指定3,二档随机4
        public string venuesSelectF;
        // 指定渠道，多家用逗号隔开,例如：SOR-CTTJ,SOR-TPSC
        public string selfSelectVenues;
        // 渠道类型默认，填写随机几家渠道,例如：2(表示两家)
        public string venuesRdmNum;

        //// 交易员编号
        //public string partySubID;
        //// 扩展字段
        //public string Info1;

        // 价格
        public decimal? price;
        // 报量
        public decimal? qty;
        //// 清算类型
        //public string settlType;
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
        // 债券代码
        public string securityId;
        // 简称
        public string symbol;

        //默认填写 "3" //收益率
        public string priceType = "3";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class IntentionOrderInfo
    {
        // 意向报价唯一编号
        public string intentionId;
        // 策略订单编号
        public string orderid;
        // 客户订单编号
        public string clordid;
        // 交易平台
        public string venuetypegw;
        // 交易执行渠道
        public string venues;
        // 机构编号
        public string institutionId;
        // 机构名称
        public string institutionName;
        // 交易员编号
        public string traderId;
        // 交易员名称
        public string traderName;
        // 用户编号
        public string userid;
        // 用户名
        public string userName;
        // 债券编号
        public string securityId;
        // 债券简称
        public string symbol;
        // 报价方向 1=买、2=卖
        public string side;
        // 清算类型 1=T+0, 2=T+1, 3=Tm+0, 4=Tm+1, 99=其他
        public string settlType;
        // 价格
        public decimal? price;
        // 报量
        public decimal? orderqty;
        // 已经执行的量
        public decimal? openqty;
        // 已经撤销的量
        public decimal? cancelledqty;
        // 已经成交的量
        public decimal? executionqty;
        // 匹配交易量
        public decimal? matchqty;
        // 订单类型
        public string ordtype;
        // 订单状态
        public string orderstatus;
        // 交易日期
        public string tradedate;
        // 交割日期
        public string settldate;
        // 订单有效期
        public string validuntitime;
        // 有效时间
        public string timeinforce;
        // firm标记
        public string firmflag;
        // 整量标记
        public string wholeflag;
        // 风控检查标记
        public string riskmanualck;
        // 销售标记
        public string salesAgentflag;
        // 暗盘标记 1暗盘，2明盘
        public string handlinst;
        // 渠道类型： 不填写1 默认2 指定3 二档随机4
        public string venuesselectf;
        // 指定渠道，多家用逗号隔开 例如：SOR-CTTJ,SOR-TPSC
        public string selfselectvenues;
        // 扩展字段1
        public string inf1;
        // 扩展字段2
        public string inf2;
        // 扩展字段3
        public string inf3;
        // 扩展字段4
        public string inf4;
        // 扩展字段5
        public string inf5;
        // 创建时间
        public string createtime;
        // 更新时间
        public string updatetime;
        // 操作人
        public string closeBy;
    }
}

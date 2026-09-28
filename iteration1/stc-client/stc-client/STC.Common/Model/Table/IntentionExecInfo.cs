using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class IntentionExecInfo
    {
        // 意向报价唯一编号
        public string intentionId;
        // 关联标记
        public string relationFlag;// 强 1， 弱 0
        // 执行单号
        public string execid;
        // 策略订单编号
        public string strategyorderid;
        // 成交单号
        public string orderid;
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
        // 净价
        public decimal? netprice;
        // 全价
        public decimal? dirtyprice;
        // 量
        public decimal? qty;
        // 本方机构编号
        public string partyid;
        // 本方机构名称
        public string partyname;
        // 本方交易员编号
        public string partysubid;
        // 本方交易员名称
        public string partysubname;
        // 对手机构编号
        public string counterpartyid;
        // 对手机构名称
        public string counterpartyname;
        // 对手交易员编号
        public string counterpartysubid;
        // 对手交易员名称
        public string counterpartysubname;
        // 拒绝者
        public string rejecter;
        // 拒绝代码
        public string rejectercode;
        // 拒绝原因
        public string rejecterreason;
        // 执行状态
        public string exectype;
        // 订单类型
        public string ordtype;
        // 订单状态
        public string orderstatus;
        // 交易日期
        public string tradedate;
        // 交割日期
        public string settldate;
        // 业务发生时间
        public string transacttime;
        // 接收时间
        public string sendingtime;
        // 同步标记
        public string synchflag;
        // 有效时间
        public string timeinforce;
        // firm标记
        public string firmflag;
        // 整量标记
        public string wholeflag;
        // 销售标记
        public string salesAgentflag;
        //// 风控检查标记
        //public string riskmanualck;
        //// 销售标记
        //public string salesAgentflag;
        //// 暗盘标记 1暗盘，2明盘
        //public string handlinst;
        public decimal? profitLoss;
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
        // 备注
        public string remark;
        // 剩余量
        public decimal? remainQty;
        // 操作标记
        public string operation;
    }
}

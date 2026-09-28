using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class RelateIntentionReq
    {
        public List<RelateIntentionInfo> execList;
    }

    public class RelateIntentionInfo
    {
        public string intentionId;// Y   String 意向报价编号
        public string relationFlag;// 强 1， 弱 0
        public string execid;//     String 执行单号
        public string strategyorderid;//     String 策略订单编号
        public string orderid;//     String 成交单号
        public string venuetypegw;//     String 交易平台
        public string venues;//     String 交易执行渠道
        public string institutionid;//     String 机构编号
        public string institutionname;//     String 机构名称
        public string traderid;//     String 交易员编号
        public string tradername;//     String 交易员名称
        public string userid;//     String 用户编号
        public string username;//     String 用户名称
        public string securityid;//     String 债券编号
        public string symbol;//    String 债券简称
        public string side;//     String 方向
        public string settltype;//     String 清算类型
        public decimal? origordprice;// BigDecimal 原始报价
        public decimal? price;// Y   BigDecimal 价格
        public decimal? netprice;//     BigDecimal 净价
        public decimal? dirtyprice;//     BigDecimal 全价
        public decimal? qty;// Y   BigDecimal 量
        public string exectype;//     String 执行状态
        public string partyid;//     String 本方机构编号
        public string partyname;//     String 本方机构名称
        public string partysubid;//     String 本方交易员编号
        public string partysubname;//     String 本方交易员名称
        public string counterpartyid;//     String 对手机构编号
        public string counterpartyname;//     String 对手机构名称
        public string counterpartysubid;//     String 对手交易员编号
        public string counterpartysubname;//     String 对手交易员名称
        public string rejecter;//     String 拒绝者
        public string rejectercode;//     String 拒绝代码
        public string rejecterreason;//     String 拒绝原因
        public string ordtype;//     String 订单类型
        public string orderstatus;//     String 交易状态
        public string tradedate;//     String 交易日期
        public string settldate;//     String 交割日期
        public string transacttime;//     String 业务发生时间
        public string sendingtime;//     String 接收时间
        public string synchflag;//     String 同步标记
        public string timeinforce;//     String 有效时间
        public string firmflag;//     String firm标记
        public string wholeflag;//     String 整量标记
        public string salesAgentflag;//     String 销售标记
        public string remark;     // String 备注
        public string inf1;//     String 扩展字段1
        public string inf2;//     String 扩展字段2
        public string inf3;//     String 扩展字段3
        public string inf4;//     String 扩展字段4
        public string inf5;//     String 扩展字段5
        public string createtime;//     String 创建时间
        public string updatetime;//     String 更新时间
        public string closeby;//     String 操作人
        public string operation; // 操作标志
    }
}

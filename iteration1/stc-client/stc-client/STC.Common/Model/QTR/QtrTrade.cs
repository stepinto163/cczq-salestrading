using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.QTR
{
    /*
     * 市场成交
     */
    public class QtrTrade
    {
        //成交ID
        public string dealID;
        //"经纪商代码，取值：
        //1 = TPSC 国利
        //2 = CNEX 国际
        //3 = CCTB 中诚
        //4 = PTCN 平安
        //5 = CTTJ 信唐"
        public int companyID;
        //成交时间,格式：YYYYMMDD-HH:MM:SS
        public string dealTime;
        //成交状态：1-新增  2-修改  3-撤销
        public int method;
        //描述,如报价描述，成交描述
        public string description;
        //“键值+市场” 唯一标示一个债券
        public string bondKeyListedMarket;
        //债券代码
        public string bondCode;
        //剩余期限，如7.5Y
        public string timeToMaturity;
        //债券简称，比如：10国开09
        public string bondShortName;
        //债券类型【短融，中票...】, 【国债，央票...】
        public string bondType;
        //交易所：CIB = 银行间 SSE = 上交所 SZE = 深交所
        public string listedMarket;
        //买卖方向：GVN=given、TKN=taken、TRD=traded
        public string direction;
        //货币, 货币, USD/CNY…
        public string currency;
        //价格
        public decimal price;
        //报价类型：1=收益率 2=行权收益率 3=净价 4=全价
        public int quoteType;
        //净价，103.0941
        public decimal cleanPrice;
        //全价，108.0941
        public decimal dirtyPrice;
        //结算日
        public string settlementDate;
        //结算模式, 如DVP
        public string settlementMode;
        //结算金额
        public decimal settlementAmount;
        //交易日期
        public string tradedDate;
        //量，单位 万
        public decimal volume;
        //结算类型
        public string settlementType;
        //卖方机构id
        public string ofrInstId;
        //卖方机构名称
        public string ofrInstName;
        //卖方交易员id
        public string ofrTraderId;
        //卖方交易员名称
        public string ofrTraderName;
        //卖方经纪人名称
        public string ofrBrokerName;
        //买方机构id
        public string bidInstId;
        //买方机构名称
        public string bidInstName;
        //买方交易员id
        public string bidTraderId;
        //买方交易员名称
        public string bidTraderName;
        //买方经纪人名称
        public string bidBrokerName;
        //报文发送时间，格式：YYYYMMDD-HH:MM:SS.sss
        public string sendingTime;

        public string venues;

        public string stcText;

        public string seq;

        public string comment;

        //OrderID
        public string orderID;
        //本方交易员（QB账户名）
        public string deliverToSubID;
    }
}

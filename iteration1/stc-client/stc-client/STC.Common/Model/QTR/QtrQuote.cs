using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.QTR
{
    /*
     * 市场报价
     */
    public class QtrQuote
    {
        //报价id
        public string quoteID;
        //"经纪商代码，取值：
        //1 = TPSC 国利
        //2 = CNEX 国际
        //3 = CCTB 中诚
        //4 = PTCN 平安
        //5 = CTTJ 信唐"
        public string companyID;
        //报价状态：1-新增  2-修改  3-撤销
        public int method;
        //报价时间,格式：YYYYMMDD-HH:MM:SS.sss
        public string quoteTime;
        //更新时间,格式：YYYYMMDD-HH:MM:SS.sss
        public string updateTime;
        //市场：4=现券市场
        public int marketIndicator;
        //"债券类型：
        //BNC=利率债 BCO=信用债 NCD=存单二级"
        public string securityType;
        //债券代码，带后缀
        public string securityID;
        //债券简称
        public string symbol;
        //交易场所：CIB=银行间 SSE=上交所 SZE=深交所
        public string listedMarket;
        //方向：1=bid 2=ofr
        public int side;
        //"报价类型：
        //1=收益率 2=行权收益率 3=净价 4=全价"
        public int quoteType;
        //bid价格
        public decimal bidPrice;
        //bid数量（单位为万）
        public decimal bidSize;
        //"bid是否OCO：
        //0=否  1=OCO  2=OCO打包"
        public int bidOCOFlag;
        //bid报价备注
        public string bidDescription;
        //ofr价格
        public decimal ofrPrice;
        //ofr数量（单位为万）
        public decimal ofrSize;
        //"ofr是否OCO：
        //0=否 1=OCO 2=OCO打包"
        public int ofrOCOFlag;
        //ofr报价备注
        public string ofrDescription;
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
    }
}


using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryEmsOrderReplyPo : BaseReplyPo
    {
       public List<EmsOrderReport> Datas;
    }

    public class EmsOrderReport
    {
        public decimal? avgprice;
        public decimal? avgprice2;
        public string biztype;
        public string biztypeid;
        public decimal? blocknum;
        public string bookid;
        public decimal? cancelledqty;
        public decimal? cancelledqty2;
        public string clearingmethod;
        public string clordid;
        public string counterpartycode;
        public string counterpartyid;
        public string counterpartyname;
        public string counterpartysubid;
        public string counterpartysubname;
        public string createtime;
        public string currency;
        public string deliverytype;
        public string execflag;
        public string exectype;
        public string exectype2;
        public string exectypereason;
        public decimal? executionqty;
        public decimal? executionqty2;
        public string firmflag;
        public string handlinst;
        public string inf1;
        public string inf2;
        public string inf3;
        public string inf4;
        public string inf5;
        public string inf6;
        public string institutionid;
        public string institutionname;
        public string ioctimeout;
        public string ip;
        public string marketindicator;
        public decimal? matchqty;
        public string mdReqid;
        public decimal? netprice;
        public decimal? netprice2;
        public decimal? fullprice;
        public decimal? fullprice2;
        public string ocoflag;
        public decimal? openqty;
        public string orderid;
        public decimal? orderqty;
        public decimal? orderqty2;
        public string orderstatus;
        public string orderstatus2;
        public string ordtype;
        public string origordprice;
        public string partycode;
        public string partyid;
        public string partyname;
        public string partysubid;
        public string partysubname;
        public decimal? price;
        public decimal? price2;
        public string qmnumber;
        public string quantid;
        public string quoteid;
        public string quotestatusdesc;
        public string quotetranstype;
        public string quotetype;
        public string rejecter;
        public string rejectercode;
        public string rejecterreason;
        public string remark;
        public string roleid;
        public string rolename;
        public string rule;
        public string salesAgentflag;
        public string securityid;
        public string settltype;
        public string settlDate;
        public string showtrader;
        public string side;
        public int sn;
        public string strategyorderid;
        public string strategyordervenues;
        public string submitclientqty;
        public string symbol;
        public string symboltype;
        public string terminal;
        public string timeinforce;
        public string tradeMethod;
        public string tradedate;
        public string traderid;
        public string tradername;
        public string updatetime;
        public string userid;
        public string username;
        public string validuntitime;
        public string venueid;
        public string venues;
        public string venuetypegw;
        public string wholeflag;
        public decimal? yield;
        public decimal? yield2;
        public long sign;
        public long sign1;
        public long sign2;
        public long sign3;
        public long sign4;
        //期限（天）
        public decimal? termDays;
        public string optiontype;
        public string optionDate;

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

        /// <summary>
        /// 来源
        /// </summary>
        public string channel;

        /// <summary>
        /// 报价标准
        /// </summary>
        public string priceType;

        /// <summary>
        /// 风控信息
        /// </summary>
        public string riskMsg;
    }
}

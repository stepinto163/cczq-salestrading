using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class PtExecOrderSaveReq
    {
        public string securityid;
        public string symbol;
        public string symboltype;
        public string price;
        public string qty;
        public string netprice;
        public string side;
        public string counterpartyid;//对手机构id 发单机构ID
        public string counterpartyname;
        public string counterpartyfullname;
        public string counterpartysubid;//对手交易员id 对手方交易员
        public string counterpartysubname;
        public string counterpartycode;

        public string institutionid;
        public string institutionname;
        public string traderid;
        public string tradername;
        public string tag1;
        public string venuetypegw;//??
        public string venues;
        public string accountid;
        public string accountname;
        public string quotemethod;//报价方式（待定/发请求/收请求/发对话/收对话）
        public string remark;

        public string execid;//stc成交单号
        public string strategyorderid;//stc单号
        public string venueexecid;//外部成交单号


        public string matchstatus;
        public string relatestatus = "0";
        public string sendstatus;
        public string settltype;
        public string settldate;
        public string partyid; 
        public string partyname;
        public string partysubid;
        public string partysubname;
        public string username;
        public string userid;
        public string name;
        public string yield;
        public string seatnumber;//席位号
        public string tradedate;//交易日期
        public List<string> execids;
        public string businessType;//业务类别
        public string bookid;//组合id
        public string bookname;//组合名称
        public string transacttime;//成交时间
        public string comment;//备注
        public string offsetflag;//开平标志0-开1-平
        public string action;//4=导入
        public string exePrice;//行权收益率
        public string fullPrice;//全价
        public string realcounterpartyname;//终端交易对手
        public string trdSubType;//发单方向
        public string trdType;//发单方式
        public string bridgePaymentFixedRate;//DF费率

        public string couponrate;//过券费
        public string yield2;//费后收益率
        public string cleanPrice2;//费后净价
        public string fullPrice2;//费后全价
        public string exePrice2;//费后行权收益率
        public string trdMatchId;//约定号

        public string settlAmount;//结算金额？
        public string sseinstitutioncode;//上交所机构代码
        public string szedealercode;//深交所-交易商代码
        public string szetradingentitycode;//深交所-交易主体代码
        public string szetradercode;//深交所-交易员代码
        public string partymanagername;//投资经理
        public string partymanagerid;//投资经理ID
        public string RiskManualCk;

        public List<string> execIdList = new List<string>();//下发集合
        public bool isSend;//false-撤销下发 true-下发
        public string matchqty;
        public string allocText;

    }

    public class PtExecOrderBatchSaveReq 
    {
        public List<PtExecOrderSaveReq> content = new List<PtExecOrderSaveReq>();
    }
}

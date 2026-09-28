using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 查询风控检查记录 接口请求结果类
    /// </summary>
    public class RiskHistoryReplyPo : BaseReplyPo
    {
        public int totalSize;
        public List<RiskMsg> Datas;
    }
    public class RiskMsg
    {
        public string clordid;
        public string createtime;
        public string crossvenues;
        public string currency;
        public string firmflag;
        public string indicator;
        public string indicatorlimitvalue;
        public string indicatorsource;
        public string indicatorvalue;
        public string institutionid;
        public string institutionname;
        public string marketindicator;
        public string mdReqid;
        public string name;
        public decimal netprice;
        public string orderid;
        public string orderqty;
        public string ordtype;
        public string partyname;
        public string partysubid;
        public string partysubname;
        public string price;
        public string quantid;
        public string quoteid;
        public string remark;
        public string result;
        public string riskmanualck;
        public string rule;
        public string securityid;
        public string selfselectvenues;
        public string settltype;
        public string side;
        public string symbol;
        public string symboltype;
        public string terminal;
        public string timeinforce;
        public string tradedate;
        public string traderid;
        public string tradername;
        public string type;
        public string updatetime;
        public string userid;
        public string username;
        public string validuntitime;
        public string venues;
        public string venuesrdmnum;
        public string venuesselectf;
        public string venuetypegw;
        public string venueusersid;
        public string wholeflag;
        public decimal yield;
    }
}

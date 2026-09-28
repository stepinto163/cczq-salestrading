using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.TBook
{
    public class QueryOrderReq
    {
        public string orderid;
        public string strategyorderid;
        public string PartyName;
        public string securityid;
        public string dealTimeBegin;
        public string dealTimeEnd;
        public string orderStatus;
        public int pageSize;
        public int pageNum = 0;
        public string venues;
        public string venuetypegw;
        public string userName;
        public string firmflag;
        public string ordType;
        public string timeinforce;
        public string counterpartyname;
        public string synchflag;
        public string execid;
        public string symbol;
        public string strategyordervenues;
        public string symboltype;
        //合约种类: 国债期货=5，债券=4
        public string marketindicator;
        public string quantid;


        /// <summary>
        /// 业务时间
        /// </summary>
        public string transactTime;
        /// <summary>
        /// 1或不传=大于sortExecid的成交；2=小于sortExecid的成交
        /// </summary>
        public string sortRule;
        public string side;
    }
}

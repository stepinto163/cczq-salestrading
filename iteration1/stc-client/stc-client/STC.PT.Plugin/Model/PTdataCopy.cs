using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PT.Plugin.Model
{
    public class PTdataCopy
    {
        public string securityid;
        public string symbol;
        public string side;
        public string price;
        public string counterPartyName;
        public string counterPartySubName;
        public string qty;
        public decimal lastQty;
        public string partySubName;
        public string institutionName;//客户机构
        public string traderName;//客户
        public string quotemethod;//报价方式
        public string settletype;
        public string tradedate;
        public string seatNumber;
        public string comment;
        public bool isBank;
        public string qtyTxt;
        public bool isIRS;
        public string quoteWay;
        public string quoteWay1;
        public string quote;
        public string instname;
        public string remainTime;//剩余时间
        public string approvalstatus;//审批状态
        public string venues;//
        public string realcounterpartynameText;
        public string codeText;//<发单机构以及交易员（交易商代码，交易主体代码，交易员代码）>
        public string paymentRate;//费率厘
        public string netPrice;//净价
        public string trdMatchId;//约定号
        public string SHNumber;//奔放机构上证所席位号


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
  public  class TradeSum
    {
        public string bookID{ get; set; }
        public string bookName{ get; set; }
        public string bookType{ get; set; }
        public string accId { get; set; }//账户
        public string accName { get; set; }
        public string securityId{ get; set; }
        public string symbol{ get; set; }
        public string execId{ get; set; }//交易编号
        public string htexecId { get; set; }//衡泰交易编号
        public string venueExecId{ get; set; }//外部成交编号
        public string tradeDate{ get; set; }
        public string side{ get; set; }
        public string netPrice { get; set; }
        public string yield { get; set; }//到期收益率
        public string amount{ get; set; }//交易金额
        public string settleDate{ get; set; }
        public string checkResult{ get; set; }

        public string isEnter { get; set; }//是否补录
        public string comment { get; set; }//备注

    }
}

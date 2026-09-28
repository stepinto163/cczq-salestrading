using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Inner
{
    /// <summary>
    /// 人工可在渠道执行界面进行成交操作类
    /// </summary>
    public class InnerOrderReq
    {
        public string ClOrdID;
        public string LastQty;
        public string LastPx;
        public string ExecType;
        public string OrderID;
        public string CounterPartyID;
        public string CounterPartyName;
        public string CounterPartySubID;
        public string CounterPartySubName;
        public string Lastpartyname;
        public string Rejecterreason;

        public string OrderQty;
        public string StrategyOrderId;

        public string SettlType;
        public string TradeMethod;
    }
}

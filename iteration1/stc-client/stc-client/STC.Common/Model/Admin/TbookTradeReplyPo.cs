using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
   public class TbookTradeReplyPo : PResult
    {
        public TbookTradePo Datas;
        //public List<TtrdAccBalanceSecu> htDatas;
    }

    public class TbookTradePo
    {
        public List<TradeSum> tradeDatas;
        public List<TrptOtcTradeAll> htDatas;
        public string UpdateTime;
        public int SyncHtCount;

    }
}

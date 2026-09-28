using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Limit
{
    public class QueryDetailsReply : PResult
    {
        public List<BondDetailInfo> datas;
    }

    public class BondDetailInfo
    {
        public string dueSettleDate;//到期结算日
        public string execid;// 成交编号
        public string execidRecord;//成交意向编号
        public string firstSettleDate;//起息日
        public decimal haircut;
        public decimal haircutAmount;
        public int id;
        public decimal lastQty;
        public string orderId;//订单编号
        public string qtpAccount;//账号
        public string securityId;//债券代码
        public string stcClOrderId;//客户端订单编号
        public string symbol;//债券名称
        public decimal usedBondAmount;//质押券面额
        public string limitStatus;// U:占用;  F:冻结   R:释放
    }
}

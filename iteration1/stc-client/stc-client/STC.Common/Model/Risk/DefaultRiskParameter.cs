using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 债券缺省
    /// </summary>
    public class DefaultRiskParameter
    {
        public RiskParam interestRateRiskParam = new RiskParam();
        public RiskParam creditRiskParam = new RiskParam();

        public class RiskParam
        {
            public String qtyMin = "";//报量下限
            public String qtyMax = "";//报量上限
            public String bid;//债估值Bid
            public String ofr;//中债估值ofr
            public String marketBid;//市场最优Bid
            public String marketOfr;//市场最优ofr
            public String dropAwayFromBid;//倒挂Bid
            public String dropAwayFromOfr;//倒挂ofr
            public String bidCheck;//
            public String ofrCheck;//
        }

        //BDB Add
        public String qtyCheck; // 是否勾选 0-不勾选  1-勾选
        public String qtyMin;//报量下限
        public String qtyMax;//报量上限

        public String chinaBidCheck;//中债估值 是否勾选 0-不勾选  1-勾选
        public String chinaBid;//中债估值Bid偏离
        public String chinaOfrCheck;//中债估值ofr偏离 是否勾选 0-不勾选  1-勾选
        public String chinaOfr;//中债估值ofr偏离


        public String marketBidCheck;//市场最优Bid偏离 是否勾选 0-不勾选  1-勾选
        public String marketBid;//市场最优Bid偏离
        public String marketOfrCheck;//市场最优ofr偏离 是否勾选 0-不勾选  1-勾选
        public String marketOfr;//市场最优ofr偏离

        public String dropAwayFromBidCheck;//倒挂Bid 是否勾选 0-不勾选  1-勾选
        public String dropAwayFromBid;//倒挂Bid
        public String dropAwayFromOfrCheck;//倒挂ofr 是否勾选 0-不勾选  1-勾选
        public String dropAwayFromOfr;//倒挂ofr

        public String excutOrderBidCheck;//成交bid偏离 是否勾选 0-不勾选  1-勾选
        public String excutOrderBid;//成交bid偏离
        public String excutOrderOfrCheck;//成交ofr偏离 是否勾选 0-不勾选  1-勾选
        public String excutOrderOfr;//成交ofr偏离
    }
}

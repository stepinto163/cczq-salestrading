using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.TBook
{
    public class PriceCondition
    {
        public string CounterpartyBestOffer;//对手方最优报价
        public string LatestTransMarket;//市场最新成交价
        public string PriceInversion;//倒挂即撤单 -未勾选；1-勾选

        public string MorethanMarketBPCheck; //与市场最优倒挂 0 -未勾选；1-勾选
        public string MorethanMarketBP; //与市场最优倒挂BP数
    }
}

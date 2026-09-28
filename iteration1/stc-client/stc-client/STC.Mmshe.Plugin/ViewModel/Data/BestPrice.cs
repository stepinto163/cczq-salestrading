using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Mm.Plugin.ViewModel.Data
{
    /// <summary>
    /// 最优价格
    /// </summary>
    public class BestPrice
    {
        // 最优价格渠道
        public string BestDisplayVenues = string.Empty;
        // 最优价格具体渠道内容
        public List<string> BestVenues;
        // 最优价格对应报量总和
        public long MDEntrySize = 0;
        // 最优价格
        public decimal MDEntryPx = decimal.Zero;
        // 最优价格的具体清算速度
        public string SettlType = string.Empty;
    }


    public class BestPrice1
    {
        public string bestPx;
        public int bestQty;
        public string bestT0;
        public string bestYuan;
        public string isAllFirm;
        public string partyName;
    }

    public class BilateralBestPrice
    {
        public BestPrice1 BidBest;
        public BestPrice1 OfrBest;

        public string Valuation;

    }

    public class LastPriceParam
    {
        //最后成交价
        public string LastPrice;

        //上涨/下跌
        public string UpDown;

        //涨跌幅
        public string Bp;

        //最后成交方向
        public string Side;

        //成交量
        public string Qty;

        //渠道
        public string Source;
    }
}

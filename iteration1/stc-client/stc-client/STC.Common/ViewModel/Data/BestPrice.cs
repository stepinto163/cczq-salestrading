using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Data
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
        public string bestNetPx;
        public decimal bestQty;
        public string bestT1;
        public string bestT0;
        public string bestYuan;
        public string isAllFirm;
        public string partyName;
        public string quoteId;
        public string venue;
        public string priceType;
        public string text;
        public string bargain;
        public string settlType;
        public decimal T1Qty;//T1总量(单渠道+1量最大)
        // 角标判定（来自行情数据，而非 EMS 订单）：
        // hasMe：该方向是否有“本人”报价（行情行 IsMe=true）
        // hasOur：该方向是否有“本机构”报价（行情行 Source∈{IDB,QUANT,MANUAL}）
        public bool hasMe;
        public bool hasOur;
    }

    public class BilateralBestPrice
    {
        public BestPrice1 BidBest;
        public BestPrice1 OfrBest;

        public string Valuation;

        public string BidSettleTypes;
        public string OfrSettleTypes;
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
        //价差
        public string NetPriceSpread;
        //最后成交净价
        public string NetPrice;
    }
}

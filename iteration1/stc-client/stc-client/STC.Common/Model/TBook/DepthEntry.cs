using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.TBook
{
    /// <summary>
    ///  阶梯下单板 深度报价
    /// </summary>
    public class DepthEntry
    {
        public string SecurityId { get; set; }
        public int StrategyBuyQty { get; set; }
        public int ManualBuyQty { get; set; }
        public int BuyQty { get; set; }
        public int StrategySellQty { get; set; }
        public int ManualSellQty { get; set; }
        public int SellQty { get; set; }
        public string Price { get; set; }
        public bool Updated { get; set; }
        public string BuySell { get; set; }
        public decimal PriceDecimal { get; set; }
        public int Index { get; set; }
        public bool Show { get; set; }
    }
}

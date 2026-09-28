using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Chart.Plugin.Chart;
using STC.Chart.Plugin.FuncEx;
using STC.Common.Model.Chart;

namespace STC.Chart.Plugin.Data
{
    public class DealtPo : IComparer<DealtPo>
    {
        public string bondCode { get; set; }
        public int dealstatus { get; set; }
        public string brokerId { get; set; }
        public string dealprice { get; set; }
        public long UpdateDateTime { get; set; }
        public string InnerUpdateTime { get; set; }
        public string tradeId { get; set; }
        public string dealtype { get; set; }

        public string Broker { get; set; }
        public string Side { get; set; }
        public string Price { get; set; }
        public string TradeTime { get; set; }

        public ChartModel Change2ChartModel()
        {
            ChartModel chartModel = new ChartModel();
            chartModel.Price = this.dealprice.ToDecimal();
            chartModel.Volumn = 1;
            chartModel.Time = this.TradeTime;
            return chartModel;
        }

        public int Compare(DealtPo x, DealtPo y)
        {
            if (x == null || y == null 
                || string.IsNullOrEmpty(x.TradeTime) 
                || string.IsNullOrEmpty(y.TradeTime)) return 0;

            int cmp1 = y.TradeTime.CompareTo(x.TradeTime);

            return cmp1;
        }
    }
}

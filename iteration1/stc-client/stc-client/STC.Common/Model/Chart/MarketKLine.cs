using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Chart
{
    public class MarketKLine
    {
        // 昨收
        public string prev_close_yield { get; set; }

        // 成交笔数
        public decimal total_num_trades { get; set; }

        // 高
        public string high_yield { get; set; }

        // 低
        public string low_yield { get; set; }

        // 开
        public string open_yld { get; set; }

        // linux TimeStamp
        public string t { get; set; }

        // 是否有效
        public string vs { get; set; }

        // 交易时间
        public string TransactTime { get; set; }

        // 接收时间
        public string TimeStamp { get; set; }

        // 当前成交笔数
        public decimal volume { get; set; }

    }
}

using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    /// <summary>
    ///  成交统计
    /// </summary>
    public class TradeStatisticsModel
    {
        // 组合编号
        public string bookId { get; set; }
        // 组合名称
        public string bookName { get; set; }
        /// key   BookId + "#" + Securityid
        public string key { get; set; }
        // 合约代码
        public string securityId { get; set; }
        // 简称
        public string symbol { get; set; }
        // 最后更新时间
        public string lastUpdateTime { get; set; }
        // 买成交量（手）
        public decimal longLot { get; set; }
        // 卖成交量（手）
        public decimal shortLot { get; set; }
        // 净成交量（手）
        public decimal netLot { get; set; }
        // 总成交量（手）
        public decimal totalLot { get; set; }
        // 买成交量（元）
        public decimal longVolume { get; set; }
        // 卖成交量（元）
        public decimal shortVolume { get; set; }
        // 净成交量（元）
        public decimal netVolume { get; set; }
        // 总成交量（元）
        public decimal totalVolume { get; set; }
        // 成交均价（买）
        public decimal longAvgPrice { get; set; }
        // 成交均价（卖）
        public decimal shortAvgPrice { get; set; }
        // 总成交金额（元）
        public decimal totalAmt { get; set; }

        // 债券类型
        public string symbolType { get; set; }
        // 合约类型 4=债券，5=国债期货
        public string marketIndicator { get; set; }
    }
}

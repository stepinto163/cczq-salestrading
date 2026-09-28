using STC.SalesTrading.Plugin.Abstractions.Models;
using System;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>
    /// 右下角提醒弹窗的**单条提醒**（原型 0910 alert-item 两行结构）：
    /// 第一行 = 债券代码 + 简称 + 报价时间；第二行 = Bid 侧（方向/交易员/量/价格）+ VS + 价格类型标识 + Ofr 侧（价格/量/交易员/方向）。
    /// 已读/关闭语义改为在分组（<see cref="AlertPopupGroup"/>）层控制，条目本身不再持有关闭状态。
    /// </summary>
    public class AlertPopupItem
    {
        public AlertPopupItem(string alertKey, string typeLabel, AlertType alertType, string code, string name,
            string time, Action onView)
        {
            AlertKey = alertKey;
            TypeLabel = typeLabel;
            AlertType = alertType;
            Code = code;
            Name = name;
            Time = time;
            _onView = onView;
        }

        public string AlertKey { get; }

        public string TypeLabel { get; }

        /// <summary>提醒类型（徽标配色用）。</summary>
        public AlertType AlertType { get; }

        public string Code { get; }

        public string Name { get; }

        /// <summary>报价时间显示文本。</summary>
        public string Time { get; }

        /// <summary>Bid 侧报价交易员。</summary>
        public string BidTrader { get; set; }

        /// <summary>Bid 侧报量。</summary>
        public string BidVolume { get; set; }

        /// <summary>Bid 侧价格。</summary>
        public string BidPrice { get; set; }

        /// <summary>价格类型标识（净价「净」/全价「全」/行权「行」/到期「到」；默认空）。</summary>
        public string PriceTypeMark { get; set; }

        /// <summary>Ofr 侧价格。</summary>
        public string OfrPrice { get; set; }

        /// <summary>Ofr 侧报量。</summary>
        public string OfrVolume { get; set; }

        /// <summary>Ofr 侧报价交易员。</summary>
        public string OfrTrader { get; set; }

        private readonly Action _onView;

        /// <summary>「查看」回调（标记该弹窗 alertKey 并跳转提醒页定位）。</summary>
        public void InvokeView()
        {
            try { _onView?.Invoke(); }
            catch (Exception) { }
        }
    }
}

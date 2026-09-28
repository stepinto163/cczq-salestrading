using System.Collections.Generic;
using System.Collections.ObjectModel;
using STC.SalesTrading.Plugin.Shared.Converters;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>
    /// 底部状态栏分段的组装/更新帮助类：构造时按 key 声明分段（文案 + 主题画刷键 + 兜底色），
    /// 之后按 key 更新文案——取代各 VM 里按集合索引写 StatusSections[i].Text 的脆弱写法
    /// （插入/移动分段时索引会静默错位）。
    /// 用法：
    ///   _status.Add("total", "报价总数: 0").Add("bid", "Bid: 0", "BidBrush", "#F59E0B");
    ///   public ObservableCollection&lt;StatusBarSection&gt; StatusSections => _status.Sections;
    ///   _status.Set("total", "报价总数: " + n);
    /// </summary>
    public sealed class StatusSectionBag
    {
        private readonly ObservableCollection<StatusBarSection> _sections = new ObservableCollection<StatusBarSection>();
        private readonly Dictionary<string, StatusBarSection> _map = new Dictionary<string, StatusBarSection>();

        /// <summary>绑定给 StatusBar.Sections 的分段集合。</summary>
        public ObservableCollection<StatusBarSection> Sections { get { return _sections; } }

        /// <summary>追加一个分段；brushKey/fallbackHex 给定时按 ConverterBrushes 解析强调色。</summary>
        public StatusSectionBag Add(string key, string text, string brushKey = null, string fallbackHex = null)
        {
            var section = new StatusBarSection(text);
            if (brushKey != null)
            {
                section.Brush = ConverterBrushes.Resolve(brushKey, fallbackHex);
            }
            _sections.Add(section);
            _map.Add(key, section);
            return this;
        }

        /// <summary>按 key 更新分段文案；key 不存在时忽略（防止静默错位改错段）。</summary>
        public void Set(string key, string text)
        {
            StatusBarSection section;
            if (_map.TryGetValue(key, out section))
            {
                section.Text = text;
            }
        }
    }
}

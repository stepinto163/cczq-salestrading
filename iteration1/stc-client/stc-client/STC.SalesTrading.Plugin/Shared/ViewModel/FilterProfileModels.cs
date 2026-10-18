using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>
    /// 关注债券管理（原型 fbm）与筛选分组管理（原型 fgm）的数据模型。
    /// 数据由 <see cref="STC.SalesTrading.Plugin.Shared.Services.FilterProfileService"/> 会话内保存；
    /// 后端契约到位后由服务层落库（原型字段：favBondGroupsCredit/Rate、filterGroupsCredit/Rate）。
    /// </summary>

    /// <summary>
    /// 关注分组里的一只债券。识别未命中时 Name/Tenor 等留空（原型 found=false 行 0.4 透明度）。
    /// 实现 INPC：识别回填（异步）与勾选态变化要驱动结果行的半透明/绿底样式。
    /// </summary>
    public class FavBondEntry : INotifyPropertyChanged
    {
        private string _code = string.Empty;
        private string _name = string.Empty;
        private string _tenor = "—";
        private string _bondLib = "—";
        private string _valuation = "—";
        private bool _found;
        private BondMarket? _market;
        private bool _isChecked;

        public string Code { get { return _code; } set { if (_code != value) { _code = value; OnPropertyChanged(nameof(Code)); } } }
        /// <summary>债券简称。</summary>
        public string Name { get { return _name; } set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } } }
        /// <summary>剩余期限（原型表格列，识别接口待接时显示 —）。</summary>
        public string Tenor { get { return _tenor; } set { if (_tenor != value) { _tenor = value; OnPropertyChanged(nameof(Tenor)); } } }
        /// <summary>债券库（同上）。</summary>
        public string BondLib { get { return _bondLib; } set { if (_bondLib != value) { _bondLib = value; OnPropertyChanged(nameof(BondLib)); } } }
        /// <summary>中债估值（同上）。</summary>
        public string Valuation { get { return _valuation; } set { if (_valuation != value) { _valuation = value; OnPropertyChanged(nameof(Valuation)); } } }
        /// <summary>识别是否命中债券库；未命中行半透明展示。</summary>
        public bool Found { get { return _found; } set { if (_found != value) { _found = value; OnPropertyChanged(nameof(Found)); } } }
        /// <summary>命中债券的所属市场；未命中为 null（勾选入组时的市场校验对 null 放行）。</summary>
        public BondMarket? Market { get { return _market; } set { if (_market != value) { _market = value; OnPropertyChanged(nameof(Market)); } } }
        /// <summary>结果表勾选态（勾选 = 已加入当前选中分组）。</summary>
        public bool IsChecked { get { return _isChecked; } set { if (_isChecked != value) { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); } } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>关注债券分组（原型 fbm-group-item：双击重命名、hover 删除）。</summary>
    public class FavBondGroup : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private bool _isEditing;

        /// <summary>稳定标识（侧栏 chip 激活集 / 右键子菜单按 id 定位；保存整体替换时同名沿用旧 id）。</summary>
        public string Id { get; set; } = "g" + Guid.NewGuid().ToString("N").Substring(0, 12);

        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); } }
        }

        /// <summary>行内重命名编辑态（原型双击 → 行内 input）。</summary>
        public bool IsEditing
        {
            get { return _isEditing; }
            set { if (_isEditing != value) { _isEditing = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEditing))); } }
        }

        public ObservableCollection<FavBondEntry> Bonds { get; } = new ObservableCollection<FavBondEntry>();

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>一条筛选条件（原型 fgm-condition-chip：key+label+value 三元组，展示用 label:value）。</summary>
    public class FilterCondition
    {
        public FilterCondition(string key, string label, string value, string displayValue = null)
        {
            Key = key;
            Label = label;
            Value = value;
            DisplayValue = displayValue;
        }

        public string Key { get; }
        public string Label { get; }

        /// <summary>条件落库的值（后端代码；本地目录 value 为空时沿用 label）。</summary>
        public string Value { get; }

        /// <summary>chip 值部分的显示文本（点选目录时为中文 label；后端读回经字典翻译）。null 时显示 Value。</summary>
        public string DisplayValue { get; }

        /// <summary>
        /// 结构化协议条件（<see cref="Abstractions.Models.FilterConditionPayload"/>）：保存直接透传、读回直接显示。
        /// null 时保存走旧编码回退（EQ/IN）。区间/文本/布尔条件必须携带。
        /// </summary>
        public object Protocol { get; set; }

        /// <summary>chip 本体显示的值文本（DisplayValue 优先，回退落库值）。</summary>
        public string ValueText
        {
            get { return DisplayValue ?? Value; }
        }

        /// <summary>条件唯一标识（原型 marketGroupConditionIdentity）：同组内去重/反选用。</summary>
        public string Identity
        {
            get { return Key + "|" + Value; }
        }

        /// <summary>chip 显示文本（原型 chip 内 span）：如「剩余期限:1-3Y」。值部分优先显示中文。</summary>
        public string DisplayText
        {
            get { return Label + ":" + (DisplayValue ?? Value); }
        }
    }

    /// <summary>子分组（原型 .fgm-subgroup 卡片：一组条件 chip，卡片右上角 hover 删除）。</summary>
    public class FilterSubgroup
    {
        public ObservableCollection<FilterCondition> Conditions { get; } = new ObservableCollection<FilterCondition>();
    }

    /// <summary>筛选分组（原型 fgm 左侧列表项；条目数 = 展平的子分组条件总数）。</summary>
    public class FilterNamedGroup : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private bool _isEditing;

        /// <summary>稳定标识（侧栏 chip 激活集按 id 定位；保存整体替换时同名沿用旧 id）。</summary>
        public string Id { get; set; } = "f" + Guid.NewGuid().ToString("N").Substring(0, 12);

        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); } }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set { if (_isEditing != value) { _isEditing = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEditing))); } }
        }

        /// <summary>已挂接 Conditions 变化通知的子分组，避免重复订阅。</summary>
        private readonly System.Collections.Generic.HashSet<FilterSubgroup> _hooked = new System.Collections.Generic.HashSet<FilterSubgroup>();

        public FilterNamedGroup()
        {
            // 子分组集合与组内条件变化都要刷新分组名后的条件计数
            Subgroups.CollectionChanged += (s, e) =>
            {
                HookSubgroups();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionCount)));
            };
        }

        private void HookSubgroups()
        {
            foreach (var sg in Subgroups)
            {
                if (_hooked.Add(sg))
                {
                    sg.Conditions.CollectionChanged += (s2, e2) =>
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionCount)));
                }
            }
        }

        public ObservableCollection<FilterSubgroup> Subgroups { get; } = new ObservableCollection<FilterSubgroup>();

        /// <summary>条件总数（原型组名后的括号计数）。</summary>
        public int ConditionCount
        {
            get
            {
                HookSubgroups();
                int n = 0;
                foreach (var sg in Subgroups)
                {
                    n += sg.Conditions.Count;
                }
                return n;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>条件目录项（原型 .fbm-check）。</summary>
    public class CatalogItem
    {
        /// <summary>后端字典候选：显示 label、发送 value；本地目录 value 为空时沿用 label。</summary>
        public CatalogItem(string label, string value = null, bool enabled = true)
        {
            Label = label;
            Value = string.IsNullOrEmpty(value) ? label : value;
            Enabled = enabled;
        }

        public string Label { get; }

        /// <summary>条件落库的值（后端代码；本地静态目录 = Label）。</summary>
        public string Value { get; }

        /// <summary>是否可选（静态目录 "!前缀" = 禁用态半透明展示）。</summary>
        public bool Enabled { get; }
    }

    /// <summary>条件目录组（原型 .fgm-condition-group）：一组同 key 的可选条件 + 可选的文本/下拉/区间自定义行。</summary>
    public class CatalogConditionGroup
    {
        /// <summary>控件形态：Tags=多选 chip；Text=文本输入+添加；Select=下拉+添加；Range=最小-最大+添加。</summary>
        public enum ConditionKind { Tags, Text, Select, Range }

        public CatalogConditionGroup(string label, string key, ConditionKind kind)
        {
            Label = label;
            Key = key;
            Kind = kind;
        }

        public string Label { get; }
        public string Key { get; }
        public ConditionKind Kind { get; }

        /// <summary>Tags/Select 模式的候选值。</summary>
        public System.Collections.Generic.List<CatalogItem> Items { get; } = new System.Collections.Generic.List<CatalogItem>();

        /// <summary>是否带自定义区间行（原型 .fbm-custom-condition 的 min-max 形态；剩余期限 = 标签 + 区间混排）。</summary>
        public bool HasRange { get; set; }

        /// <summary>区间行是否带 Y/M/D 单位下拉（原型仅剩余期限 rt 有）。</summary>
        public bool HasUnitSelect { get; set; }

        public bool Enabled { get; set; } = true;

        public string DisabledReason { get; set; } = "当前筛选条件不可用";
    }

    /// <summary>条件目录分区（原型 .fbm-cond-section，标题 = 报价筛选/债券筛选/…）。</summary>
    public class CatalogSection
    {
        public CatalogSection(string title)
        {
            Title = title;
        }

        public string Title { get; }

        public System.Collections.Generic.List<CatalogConditionGroup> Groups { get; } = new System.Collections.Generic.List<CatalogConditionGroup>();
    }
}

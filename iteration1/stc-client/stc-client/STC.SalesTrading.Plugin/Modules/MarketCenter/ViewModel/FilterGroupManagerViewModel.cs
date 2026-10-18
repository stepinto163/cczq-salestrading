using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 筛选分组管理弹窗 VM（原型 showFilterGroupModal/_fgmRender）。
    /// 左列 = 分组列表（双击重命名/删除/+新建）+ 已选区（子分组卡片：条件 chip 可删、卡片 hover 删除、
    /// 底部「+ 新增分组」虚线按钮）；右侧 = 粘贴条件文本识别 + 条件目录（按分区展开、多选 toggle、
    /// 文本/下拉/区间「添加」行、不可用项置灰）。
    /// 保存整体替换回 <see cref="FilterProfileService"/>；侧栏 chip 与表格数据联动待后端契约。
    /// </summary>
    public class FilterGroupManagerViewModel : INotifyPropertyChanged
    {
        /// <summary>分组目录服务（组合根装配时注入；未注入回退静态内存 Mock）。</summary>
        public static IGroupDirectory Directory { get; set; }

        private readonly FilterProfileService.MarketKind _market;
        private readonly IGroupDirectory _directory;

        public FilterGroupManagerViewModel(FilterProfileService.MarketKind market)
        {
            _market = market;
            _directory = Directory ?? throw new System.InvalidOperationException("分组目录服务未装配（组合根初始化异常）");
            foreach (var g in _directory.GetFilterGroups(market))
            {
                Groups.Add(CloneGroup(g));
            }
            // 条件目录：优先后端字段目录按原型侧栏结构合成（GroupConditionCatalog：分区/顺序/形态 = 面板结构，
            // label/候选/置灰 = dictionary）；不可用时回退 dictionary 直构、再回退本地静态目录（Toast 指明原因）
            var catalogSections = GroupConditionCatalog.TryGet(market, _directory)?.ForMarket(market);
            if (catalogSections == null && GroupConditionCatalog.LastFailReason != null)
            {
                ToastService.Show("条件目录按本地数据回退：" + GroupConditionCatalog.LastFailReason, ToastKind.Error);
            }
            Catalog = catalogSections
                ?? BuildCatalog(market)
                ?? FilterProfileService.GetCatalog(market);
            if (Groups.Count > 0)
            {
                SelectedIndex = 0;
            }
        }

        public string MarketTitle
        {
            get { return (_market == FilterProfileService.MarketKind.Rate ? "利率" : "信用") + "筛选分组管理"; }
        }

        public ObservableCollection<FilterNamedGroup> Groups { get; } = new ObservableCollection<FilterNamedGroup>();

        private FilterNamedGroup _selectedGroup;
        public FilterNamedGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    // 原型 _fgmActiveSubgroup 的 normalize 语义：子分组为空时自动回落一张空卡
                    if (value != null && value.Subgroups.Count == 0)
                    {
                        value.Subgroups.Add(new FilterSubgroup());
                    }
                    _selectedSubgroupIndex = value != null && value.Subgroups.Count > 0 ? 0 : -1;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGroup)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubgroupIndex)));
                }
            }
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                SelectedGroup = (value >= 0 && value < Groups.Count) ? Groups[value] : null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
            }
        }

        private int _selectedSubgroupIndex = -1;
        /// <summary>当前激活的子分组卡片（原型 .fgm-subgroup.active，右栏 toggle 的目标）。</summary>
        public int SelectedSubgroupIndex
        {
            get { return _selectedSubgroupIndex; }
            set
            {
                if (_selectedSubgroupIndex != value)
                {
                    _selectedSubgroupIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubgroupIndex)));
                }
            }
        }

        public FilterSubgroup ActiveSubgroup
        {
            get
            {
                if (SelectedGroup == null || _selectedSubgroupIndex < 0 || _selectedSubgroupIndex >= SelectedGroup.Subgroups.Count)
                {
                    return null;
                }
                return SelectedGroup.Subgroups[_selectedSubgroupIndex];
            }
        }

        public List<CatalogSection> Catalog { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        // ---- 新建分组 ----
        public bool NewGroupVisible { get; private set; }
        public string NewGroupName { get; set; } = string.Empty;

        public void StartNewGroup()
        {
            NewGroupVisible = true;
            NewGroupName = string.Empty;
        }

        public FilterNamedGroup ConfirmNewGroup()
        {
            var name = (NewGroupName ?? string.Empty).Trim();
            NewGroupVisible = false;
            if (name.Length == 0)
            {
                return null;
            }
            if (Groups.Any(g => g.Name == name))
            {
                // 原型 _fbmConfirmNewGroup/_fgmConfirmNewGroup：重名 toast 后取消该行
                ToastService.Show("分组名称不能重复", ToastKind.Error);
                return null;
            }
            var g = new FilterNamedGroup { Name = name };
            Groups.Add(g);
            SelectedIndex = Groups.Count - 1;
            return g;
        }

        public void DeleteGroup(FilterNamedGroup group)
        {
            if (group == null)
            {
                return;
            }
            int idx = Groups.IndexOf(group);
            Groups.Remove(group);
            SelectedIndex = Groups.Count > 0 ? System.Math.Min(System.Math.Max(idx, 0), Groups.Count - 1) : -1;
        }

        // ---- 子分组（原型 .fgm-subgroup 卡片） ----

        /// <summary>「+ 新增分组」：给当前分组加一个空子分组并激活。</summary>
        public FilterSubgroup AddSubgroup()
        {
            if (SelectedGroup == null)
            {
                return null;
            }
            var sg = new FilterSubgroup();
            SelectedGroup.Subgroups.Add(sg);
            SelectedSubgroupIndex = SelectedGroup.Subgroups.Count - 1;
            return sg;
        }

        /// <summary>删除子分组；有条件时由窗口层先确认。删空后自动回落一张空卡（原型 normalize 语义）。</summary>
        public void DeleteSubgroup(FilterSubgroup subgroup)
        {
            if (SelectedGroup == null || subgroup == null)
            {
                return;
            }
            int idx = SelectedGroup.Subgroups.IndexOf(subgroup);
            SelectedGroup.Subgroups.Remove(subgroup);
            if (SelectedGroup.Subgroups.Count == 0)
            {
                SelectedGroup.Subgroups.Add(new FilterSubgroup());
                SelectedSubgroupIndex = 0;
            }
            else
            {
                SelectedSubgroupIndex = System.Math.Min(System.Math.Max(idx, 0), SelectedGroup.Subgroups.Count - 1);
            }
        }

        public void RemoveCondition(FilterSubgroup subgroup, FilterCondition condition)
        {
            subgroup?.Conditions.Remove(condition);
        }

        // ---- 右栏条件 toggle（原型 _fgmToggleCatalog） ----

        /// <summary>文本/识别 chip 场景：直接以文本作为条件值。</summary>
        public bool ToggleCatalogCondition(CatalogConditionGroup group, string value)
        {
            return ToggleCatalogCondition(group, new Shared.ViewModel.CatalogItem(value, value));
        }

        /// <summary>勾选/取消目录项 → 加入/移出激活子分组；无子分组时自动建一个。返回操作后的勾选态。
        /// 条件值取 item.Value（后端代码），chip 显示 label。</summary>
        public bool ToggleCatalogCondition(CatalogConditionGroup group, Shared.ViewModel.CatalogItem item)
        {
            if (group == null || item == null || !group.Enabled || !item.Enabled || string.IsNullOrEmpty(item.Value))
            {
                return false;
            }
            if (SelectedGroup == null)
            {
                return false;
            }
            var sg = ActiveSubgroup ?? AddSubgroup();
            if (sg == null)
            {
                return false;
            }
            // chip Value=目录项代码（H14 enumValues；无字典字段==label）、DisplayValue=中文显示——
            // Identity 全程按代码对齐：当场亮/读回亮/去重一致/服务端 canonical 校验通过（实测踩坑）
            var condition = new FilterCondition(group.Key, group.Label, item.Value, item.Label);
            var existing = sg.Conditions.FirstOrDefault(c => c.Identity == condition.Identity)
                ?? sg.Conditions.FirstOrDefault(c =>
                    c.Key == group.Key
                    && (c.Value == item.Label || (c.DisplayValue != null && c.DisplayValue == item.Label)));
            if (existing != null)
            {
                sg.Conditions.Remove(existing);
                return false;
            }
            sg.Conditions.Add(condition);
            return true;
        }

        /// <summary>目录项当前是否已在激活子分组中（按落库值匹配；兼容 label 直传）。</summary>
        public bool IsConditionActive(CatalogConditionGroup group, Shared.ViewModel.CatalogItem item)
        {
            var sg = ActiveSubgroup;
            if (sg == null || item == null) return false;
            var value = item.Value;
            return sg.Conditions.Any(c => c.Key == group.Key && (c.Value == value || c.Value == item.Label));
        }

        /// <summary>目录项当前是否已在激活子分组中（勾选态）。</summary>
        public bool IsConditionActive(CatalogConditionGroup group, string value)
        {
            var sg = ActiveSubgroup;
            if (sg == null)
            {
                return false;
            }
            var identity = group.Key + "|" + value;
            // DisplayValue 兜底：历史数据落库值与目录显示值不同文（旧编码/翻译差异）时仍能点亮
            return sg.Conditions.Any(c => c.Identity == identity || (c.DisplayValue != null && group.Key + "|" + c.DisplayValue == identity));
        }

        // ---- 自定义添加（原型 _fgmAddCatalogValue / _fgmAddCatalogRange） ----

        public bool AddCustomText(CatalogConditionGroup group, string text)
        {
            var value = (text ?? string.Empty).Trim();
            if (group == null || !group.Enabled)
            {
                return false;
            }
            if (value.Length == 0)
            {
                ToastService.Show("请输入或选择" + group.Label, ToastKind.Error);
                return false;
            }
            if (IsConditionActive(group, value))
            {
                return false;
            }
            var added = ToggleCatalogCondition(group, value);
            if (added)
            {
                ToastService.Show("已添加" + group.Label);
            }
            return added;
        }

        public bool AddCustomRange(CatalogConditionGroup group, string min, string max, string unit)
        {
            min = (min ?? string.Empty).Trim();
            max = (max ?? string.Empty).Trim();
            if (group == null || !group.Enabled)
            {
                return false;
            }
            if (min.Length == 0 && max.Length == 0)
            {
                ToastService.Show("请输入" + group.Label + "范围", ToastKind.Error);
                return false;
            }

            decimal? minVal = ParseRangeNumber(min);
            decimal? maxVal = ParseRangeNumber(max);
            if ((min.Length > 0 && !minVal.HasValue) || (max.Length > 0 && !maxVal.HasValue)
                || (minVal.HasValue && maxVal.HasValue && minVal.Value > maxVal.Value))
            {
                ToastService.Show(group.Label + "范围不合法", ToastKind.Error);
                return false;
            }

            // 原型 _fgmAddCatalogRange：剩余期限的 M/D 单位折算成年（/12、/365）
            if (unit == "M" || unit == "D")
            {
                decimal factor = unit == "M" ? 1m / 12m : 1m / 365m;
                minVal = minVal.HasValue ? minVal.Value * factor : (decimal?)null;
                maxVal = maxVal.HasValue ? maxVal.Value * factor : (decimal?)null;
            }

            var value = FormatRangeValue(minVal) + "-" + FormatRangeValue(maxVal);
            if (IsConditionActive(group, value))
            {
                return false;
            }
            var added = ToggleCatalogCondition(group, value);
            if (added)
            {
                ToastService.Show("已添加" + group.Label + "范围");
            }
            return added;
        }

        private static decimal? ParseRangeNumber(string text)
        {
            if (text.Length == 0)
            {
                return null;
            }
            decimal value;
            return decimal.TryParse(text, out value) ? value : (decimal?)null;
        }

        /// <summary>数值格式化：整数不带小数点，小数去尾零（1 → "1"，0.5 → "0.5"）。</summary>
        private static string FormatRangeValue(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("0.####") : string.Empty;
        }

        // ---- 文本识别（原型 _fgmMatch：MVP 用「与目录项 label 全等」匹配） ----

        public ObservableCollection<string> MatchedTokens { get; } = new ObservableCollection<string>();

        private string _conditionInput = string.Empty;
        /// <summary>粘贴条件文本。必须通知：右栏占位提示语的 DataTrigger 绑定此值（空=显示占位）。</summary>
        public string ConditionInput
        {
            get { return _conditionInput; }
            set { if (_conditionInput != value) { _conditionInput = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(ConditionInput))); } }
        }

        /// <summary>
        /// 前置要求已选分组（原型：「请先选择左侧分组」）；命中的目录条件**直接加入当前激活子分组**
        /// （identity 去重），并回填「识别结果」chips。返回新加入的条件数。
        /// </summary>
        public int Match()
        {
            MatchedTokens.Clear();
            if (string.IsNullOrWhiteSpace(ConditionInput))
            {
                return 0;
            }
            if (SelectedGroup == null)
            {
                ToastService.Show("请先选择左侧分组", ToastKind.Error);
                return 0;
            }

            var matched = new List<string>();
            int added = 0;
            foreach (var token in FavBondManagerViewModel.ParseTokens(ConditionInput))
            {
                foreach (var sec in Catalog)
                {
                    foreach (var cg in sec.Groups)
                    {
                        if (!cg.Enabled)
                        {
                            continue;
                        }
                        if (cg.Kind == CatalogConditionGroup.ConditionKind.Tags &&
                            cg.Items.Any(it => it.Enabled && string.Equals(it.Label, token, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            if (!matched.Contains(token))
                            {
                                matched.Add(token);
                            }
                            // 原型 _fgmMatch：匹配到的条件直接进激活子分组（Toggle 自带 identity 去重）
                            if (ToggleCatalogCondition(cg, token))
                            {
                                added++;
                            }
                            break;
                        }
                    }
                }
            }

            foreach (var token in matched)
            {
                MatchedTokens.Add(token);
            }
            return added;
        }

        public void ClearConditionInput()
        {
            ConditionInput = string.Empty;
            MatchedTokens.Clear();
        }

        /// <summary>保存：条件先经目录编译成结构化载荷（文本 CONTAINS/区间 BETWEEN/枚举 IN），整体替换回分组目录。
        /// 编译含字段目录网络读取、保存为逐组网关请求——窗口包 Task.Run 在线程池执行，UI 不冻结；
        /// 成功后由调用方回 UI 线程 RaiseDataChanged。</summary>
        public void Save()
        {
            // 无载荷的条件（点选/识别/手输）按当前目录编译；个别编译失败（如旧数据的代码值）
            // 留空走网关旧编码回退，不阻断整体保存
            var catalog = GroupConditionCatalog.TryGet(_market, _directory);
            if (catalog != null)
            {
                foreach (var group in Groups)
                {
                    foreach (var sg in group.Subgroups)
                    {
                        foreach (var c in sg.Conditions)
                        {
                            if (c.Protocol != null) continue;
                            try { c.Protocol = catalog.Compile(_market, c); }
                            catch { /* 留空：网关层回退 EQ/IN 旧编码 */ }
                        }
                    }
                }
            }
            _directory.SaveFilterGroups(_market, Groups);
        }

        /// <summary>
        /// 条件目录：优先后端字段目录（ficc.quote.dictionary.query，条件可落库的唯一事实源）；
        /// 网关不可用/无字段时回退本地静态目录（此时保存会被网关拦截提示）。
        /// 只渲染带枚举候选的字段（Tags 形态）；text/区间字段的录入交互二期接。
        /// </summary>
        private List<CatalogSection> BuildCatalog(FilterProfileService.MarketKind market)
        {
            System.Collections.Generic.IReadOnlyList<Abstractions.Models.FilterFieldEntry> fields;
            try
            {
                fields = _directory.GetFilterFields(market);
            }
            catch
            {
                return null;
            }
            if (fields == null || fields.Count == 0) return null;

            var sections = new Dictionary<string, CatalogSection>(System.StringComparer.Ordinal);
            foreach (var field in fields)
            {
                if (field.EnumValues == null || field.EnumValues.Count == 0) continue;   // text/区间字段二期接录入 UI

                var sectionTitle = string.IsNullOrEmpty(field.Group) ? "筛选条件" : field.Group;
                if (!sections.TryGetValue(sectionTitle, out var section))
                {
                    section = new CatalogSection(sectionTitle);
                    sections[sectionTitle] = section;
                }
                var group = new CatalogConditionGroup(field.Label, field.Field, CatalogConditionGroup.ConditionKind.Tags);
                foreach (var option in field.EnumValues)
                {
                    // 显示后端中文 label、落库后端 value 代码（对齐 reviewed-market-detail 的双轨做法）
                    if (!string.IsNullOrEmpty(option.Value)) group.Items.Add(new CatalogItem(option.Label, option.Value));
                }
                if (group.Items.Count > 0) section.Groups.Add(group);
            }
            return sections.Values.ToList();
        }

        private static FilterNamedGroup CloneGroup(FilterNamedGroup src)
        {
            var copy = new FilterNamedGroup { Name = src.Name, Id = src.Id, Revision = src.Revision };
            foreach (var sg in src.Subgroups)
            {
                var sgCopy = new FilterSubgroup();
                foreach (var c in sg.Conditions)
                {
                    sgCopy.Conditions.Add(new FilterCondition(c.Key, c.Label, c.Value, c.DisplayValue) { Protocol = c.Protocol });
                }
                copy.Subgroups.Add(sgCopy);
            }
            return copy;
        }
    }
}

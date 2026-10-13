using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 关注债券管理弹窗 VM（原型 showFavBondModal/_fbmRender 及 _fbmIdentify/_fbmAutoAdd/_fbmToggleAll）。
    /// 左列 = 分组列表（双击重命名、hover 删除、+ 新建）+ 选中分组的债券列表；
    /// 右侧 = 粘贴代码文本框（清空/识别）+ 识别结果表（勾选即加入/移出当前分组）。
    /// 识别回填：经 <see cref="IMarketCenterService.LookupBondAsync"/> 从服务端基础资料按代码精确查询
    /// ；未命中行 Found=false 半透明展示。
    /// </summary>
    public class FavBondManagerViewModel : INotifyPropertyChanged
    {
        private readonly FilterProfileService.MarketKind _market;
        private readonly List<FavBondGroup> originalGroups;
        private readonly IMarketCenterService _lookupService;

        public FavBondManagerViewModel(FilterProfileService.MarketKind market, IMarketCenterService lookupService = null)
        {
            _market = market;
            originalGroups = FilterProfileService.GetFavGroups(market).ToList();
            _lookupService = lookupService;
            // 深拷贝工作副本：取消不落库，保存才整体替换
            foreach (var g in FilterProfileService.GetFavGroups(market))
            {
                var copy = new FavBondGroup { Name = g.Name, Id = g.Id, Revision = g.Revision };
                foreach (var b in g.Bonds)
                {
                    copy.Bonds.Add(new FavBondEntry { Code = b.Code, Name = b.Name, Tenor = b.Tenor, BondLib = b.BondLib, Valuation = b.Valuation, Found = b.Found, Market = b.Market });
                }
                Groups.Add(copy);
            }
            if (Groups.Count > 0)
            {
                SelectedIndex = 0;
            }
        }

        public ObservableCollection<FavBondGroup> Groups { get; } = new ObservableCollection<FavBondGroup>();

        private FavBondGroup _selectedGroup;
        /// <summary>当前选中分组（左下列表与结果表勾选的目标）。</summary>
        public FavBondGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGroup)));
                    // 原型 _fbmGroupClick → _fbmSyncCheckState：切分组后按新分组回填结果表勾选框
                    SyncCheckStates();
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

        public event PropertyChangedEventHandler PropertyChanged;

        // ---- 新建分组（原型 fbmNewGroupRow：行内输入，Enter/失焦确认，Esc 取消） ----
        public bool NewGroupVisible { get; private set; }
        public string NewGroupName { get; set; } = string.Empty;

        public void StartNewGroup()
        {
            NewGroupVisible = true;
            NewGroupName = string.Empty;
        }

        /// <summary>确认新建；空名/重名返回 null（由调用方提示），成功返回新组。</summary>
        public FavBondGroup ConfirmNewGroup()
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
            var g = new FavBondGroup { Name = name };
            Groups.Add(g);
            SelectedIndex = Groups.Count - 1;
            return g;
        }

        public void DeleteGroup(FavBondGroup group)
        {
            if (group == null)
            {
                return;
            }
            int idx = Groups.IndexOf(group);
            Groups.Remove(group);
            SelectedIndex = Groups.Count > 0 ? System.Math.Min(System.Math.Max(idx, 0), Groups.Count - 1) : -1;
        }

        // ---- 识别（原型 _fbmIdentify → /parse；WPF 走 LookupBondAsync 本地 Mock 库） ----
        private string _bondInput = string.Empty;
        /// <summary>粘贴债券代码文本。必须通知：右侧占位提示语的 DataTrigger 绑定此值（空=显示占位）。</summary>
        public string BondInput
        {
            get { return _bondInput; }
            set { if (_bondInput != value) { _bondInput = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(BondInput))); } }
        }

        public ObservableCollection<FavBondEntry> Results { get; } = new ObservableCollection<FavBondEntry>();

        /// <summary>
        /// 解析粘贴文本（空格/逗号/顿号/分号分隔）→ 逐个查债券库回填 名称/期限/债券库/中债估值/市场。
        /// 未命中行 Found=false 半透明展示，仍可勾选入组（与原型一致）。
        /// 返回解析出的代码数；输入为空时返回 0 并提示。
        /// </summary>
        public async Task<int> IdentifyAsync()
        {
            var tokens = ParseTokens(BondInput);
            if (tokens.Count == 0)
            {
                ToastService.Show("请输入债券代码", ToastKind.Error);
                return 0;
            }

            Results.Clear();
            foreach (var code in tokens)
            {
                var entry = new FavBondEntry { Code = code };
                if (_lookupService != null)
                {
                    var hit = await _lookupService.LookupBondAsync(code, System.Threading.CancellationToken.None);
                    if (hit != null)
                    {
                        entry.Code = hit.Code ?? code;
                        entry.Found = hit.Found;
                        entry.Name = hit.Found ? (hit.Name ?? string.Empty) : string.Empty;
                        entry.Tenor = hit.Found ? (hit.Tenor ?? "—") : "—";
                        entry.BondLib = hit.Found ? (hit.BondLibrary ?? "—") : "—";
                        entry.Valuation = hit.Found ? (hit.Valuation ?? "—") : "—";
                        entry.Market = hit.Market;
                    }
                }

                // 勾选初值 = 是否已在当前选中分组
                entry.IsChecked = IsAdded(entry);
                Results.Add(entry);
            }

            return tokens.Count;
        }

        public void ClearInput()
        {
            BondInput = string.Empty;
            Results.Clear();
        }

        public static List<string> ParseTokens(string text)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }
            var sb = new StringBuilder();
            foreach (var ch in text)
            {
                if (char.IsWhiteSpace(ch) || ch == ',' || ch == '，' || ch == '、' || ch == ';' || ch == '；' || ch == '/')
                {
                    Flush(sb, list);
                }
                else
                {
                    sb.Append(ch);
                }
            }
            Flush(sb, list);
            return list;
        }

        private static void Flush(StringBuilder sb, List<string> list)
        {
            if (sb.Length > 0)
            {
                var token = sb.ToString().Trim();
                if (token.Length > 0 && !list.Contains(token))
                {
                    list.Add(token);
                }
                sb.Clear();
            }
        }

        // ---- 勾选 ↔ 当前分组（原型 _fbmAutoAdd / _fbmToggleAll / _fbmSyncCheckState） ----

        /// <summary>结果行是否已在选中分组中（表格勾选态）。</summary>
        public bool IsAdded(FavBondEntry entry)
        {
            return SelectedGroup != null && entry != null &&
                   SelectedGroup.Bonds.Any(b => string.Equals(b.Code, entry.Code, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 勾选/取消勾选 → 加入/移出当前分组。加入前做市场校验（原型 _fbmAutoAdd）：
        /// 已命中且市场与当前弹窗不符的债券拒绝入组并提示。
        /// </summary>
        public void AutoAdd(FavBondEntry entry, bool add)
        {
            if (SelectedGroup == null || entry == null)
            {
                return;
            }
            var existing = SelectedGroup.Bonds.FirstOrDefault(b => string.Equals(b.Code, entry.Code, System.StringComparison.OrdinalIgnoreCase));
            if (add && existing == null)
            {
                if (entry.Market.HasValue && entry.Market.Value != (BondMarket)_market)
                {
                    ToastService.Show(string.Format("该债券不属于当前{0}分组",
                        _market == FilterProfileService.MarketKind.Rate ? "利率债" : "信用债"), ToastKind.Error);
                    entry.IsChecked = false;
                    return;
                }
                SelectedGroup.Bonds.Add(new FavBondEntry { Code = entry.Code, Name = entry.Name, Tenor = entry.Tenor, BondLib = entry.BondLib, Valuation = entry.Valuation, Found = entry.Found, Market = entry.Market });
                entry.IsChecked = true;
            }
            else if (!add && existing != null)
            {
                SelectedGroup.Bonds.Remove(existing);
                entry.IsChecked = false;
            }
        }

        /// <summary>表头全选（原型 _fbmToggleAll）：全选 = 逐行加入；取消全选 = 从分组移除所有结果行代码。</summary>
        public void ToggleAll(bool checkAll)
        {
            if (SelectedGroup == null || Results.Count == 0)
            {
                return;
            }
            if (!checkAll)
            {
                var resultCodes = new HashSet<string>(Results.Select(r => r.Code), System.StringComparer.OrdinalIgnoreCase);
                for (int i = SelectedGroup.Bonds.Count - 1; i >= 0; i--)
                {
                    if (resultCodes.Contains(SelectedGroup.Bonds[i].Code))
                    {
                        var entry = Results.FirstOrDefault(r => string.Equals(r.Code, SelectedGroup.Bonds[i].Code, System.StringComparison.OrdinalIgnoreCase));
                        if (entry != null)
                        {
                            entry.IsChecked = false;
                        }
                        SelectedGroup.Bonds.RemoveAt(i);
                    }
                }
                return;
            }

            foreach (var entry in Results.Where(r => !r.IsChecked))
            {
                AutoAdd(entry, true);
            }
        }

        /// <summary>按当前选中分组回填结果表勾选框（原型 _fbmSyncCheckState：切分组/行变动后调用）。</summary>
        public void SyncCheckStates()
        {
            foreach (var entry in Results)
            {
                entry.IsChecked = IsAdded(entry);
            }
        }

        /// <summary>移除分组内债券（左下列表 X）。</summary>
        public void RemoveBond(FavBondEntry entry)
        {
            if (SelectedGroup?.Bonds.Remove(entry) == true)
            {
                SyncCheckStates();
            }
        }

        /// <summary>保存：整体替换回服务存储。</summary>
        public async System.Threading.Tasks.Task SaveAsync()
        {
            await FilterProfileService.SaveFavGroupsAsync(_market, Groups, originalGroups);
        }
    }
}

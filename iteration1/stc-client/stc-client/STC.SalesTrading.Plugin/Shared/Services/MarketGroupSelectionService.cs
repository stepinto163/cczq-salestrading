using System;
using System.Collections.Generic;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 侧栏分组 chip 的激活状态与「添加到分组」入口（原型 activeMarketGroupIds + contextFavorite 系列）。
    /// 语义（app.js rowMatchesActiveMarketGroups）：
    ///  - 激活的关注组内**任一**组包含该债券代码，且激活的筛选组内**任一**组条件命中，行才保留（两类之间 AND、类内 OR）；
    ///  - 分组按行自身市场取集合（信用行只看信用组）。
    /// 激活集随分组数据变化做失效清理（原型 applyMarketGroupState 的 validIds 修剪）。
    /// </summary>
    public static class MarketGroupSelectionService
    {
        private static readonly HashSet<string> _activeFavoriteIds = new HashSet<string>();
        private static readonly HashSet<string> _activeFilterIds = new HashSet<string>();

        static MarketGroupSelectionService()
        {
            // 管理弹窗保存（整体替换）后：修剪已不存在的激活 id，必要时触发刷新
            FilterProfileService.DataChanged += PruneAndNotify;
        }

        /// <summary>激活分组变化（选中/取消/失效清理）时通知看板重查、侧栏 chip 更新。</summary>
        public static event Action ActiveGroupsChanged;

        public static bool IsFavoriteActive(string groupId)
        {
            return _activeFavoriteIds.Contains(groupId);
        }

        public static bool IsFilterActive(string groupId)
        {
            return _activeFilterIds.Contains(groupId);
        }

        /// <summary>侧栏 chip 点击（原型 toggleMarketGroup）：再点取消。</summary>
        public static void Toggle(FavBondGroup group)
        {
            if (group == null) return;
            if (!_activeFavoriteIds.Remove(group.Id))
            {
                _activeFavoriteIds.Add(group.Id);
            }
            RaiseChanged();
        }

        /// <summary>chip 命令入口（chip 只带 id 与类别）。</summary>
        public static void Toggle(string groupId, bool isFavorite)
        {
            var ids = isFavorite ? _activeFavoriteIds : _activeFilterIds;
            if (!ids.Remove(groupId))
            {
                ids.Add(groupId);
            }
            RaiseChanged();
        }

        public static void Toggle(FilterNamedGroup group)
        {
            if (group == null) return;
            if (!_activeFilterIds.Remove(group.Id))
            {
                _activeFilterIds.Add(group.Id);
            }
            RaiseChanged();
        }

        /// <summary>取指定市场激活的分组 Id（关注组、筛选组）——查询下推时注入 filter（服务端过滤）。</summary>
        public static (System.Collections.Generic.List<string> favorites, System.Collections.Generic.List<string> filters) ActiveGroupIds(FilterProfileService.MarketKind market)
        {
            var favorites = new System.Collections.Generic.List<string>();
            var filters = new System.Collections.Generic.List<string>();
            foreach (var g in FilterProfileService.GetFavGroups(market))
            {
                if (_activeFavoriteIds.Contains(g.Id)) favorites.Add(g.Id);
            }
            foreach (var g in FilterProfileService.GetFilterGroups(market))
            {
                if (_activeFilterIds.Contains(g.Id)) filters.Add(g.Id);
            }
            return (favorites, filters);
        }

        /// <summary>
        /// 清除过滤（原型 clearBondFilterScope 第一段）：把指定市场的激活分组退掉；
        /// 不传市场 = 全部退掉。激活分组随分组数据变化做失效清理（原型 applyMarketGroupState 的 validIds 修剪）。
        /// </summary>
        public static void ClearActive(params FilterProfileService.MarketKind[] markets)
        {
            var changed = false;
            if (markets == null || markets.Length == 0)
            {
                changed = _activeFavoriteIds.Count > 0 || _activeFilterIds.Count > 0;
                _activeFavoriteIds.Clear();
                _activeFilterIds.Clear();
            }
            else
            {
                changed |= ClearScoped(_activeFavoriteIds, markets, FilterProfileService.GetFavGroups, g => g.Id);
                changed |= ClearScoped(_activeFilterIds, markets, FilterProfileService.GetFilterGroups, g => g.Id);
            }
            if (changed)
            {
                RaiseChanged();
            }
        }

        private static bool ClearScoped<T>(
            HashSet<string> ids,
            FilterProfileService.MarketKind[] markets,
            Func<FilterProfileService.MarketKind, System.Collections.ObjectModel.ObservableCollection<T>> source,
            Func<T, string> idOf)
        {
            var changed = false;
            foreach (var market in markets)
            {
                foreach (var group in source(market))
                {
                    changed |= ids.Remove(idOf(group));
                }
            }
            return changed;
        }

        /// <summary>原型 normalizeBondCode：去空白、大写、去掉交易所后缀。</summary>
        public static string NormalizeCode(string code)
        {
            var value = (code ?? string.Empty).Trim().ToUpperInvariant();
            if (value.EndsWith(".IB") || value.EndsWith(".SH") || value.EndsWith(".SZ") || value.EndsWith(".BJ"))
            {
                value = value.Substring(0, value.Length - 3);
            }
            return value;
        }

        // ============ 右键「添加到分组」（原型 addToContextFavorite / confirmNewGroup） ============

        /// <summary>把债券加入关注分组；已在组里返回 false（原型静默跳过）。</summary>
        public static bool AddBondToFavoriteGroup(FavBondGroup group, string code, string name)
        {
            if (group == null || string.IsNullOrWhiteSpace(code)) return false;
            var normalized = NormalizeCode(code);
            foreach (var bond in group.Bonds)
            {
                if (NormalizeCode(bond.Code) == normalized) return false;
            }
            group.Bonds.Add(new FavBondEntry { Code = code.Trim(), Name = name ?? string.Empty });
            FilterProfileService.RaiseDataChanged();
            return true;
        }

        /// <summary>按 id 在两个市场集合中查找关注分组（右键菜单/提醒看板行没有市场信息时用）。</summary>
        public static FavBondGroup FindFavoriteGroup(string groupId)
        {
            foreach (var kind in new[] { FilterProfileService.MarketKind.Credit, FilterProfileService.MarketKind.Rate })
            {
                foreach (var g in FilterProfileService.GetFavGroups(kind))
                {
                    if (g.Id == groupId) return g;
                }
            }
            return null;
        }

        private static void PruneAndNotify()
        {
            var changed = false;
            changed |= _activeFavoriteIds.RemoveWhere(id => FindFavoriteGroup(id) == null) > 0;
            changed |= _activeFilterIds.RemoveWhere(id => FindFilterGroup(id) == null) > 0;
            if (changed) RaiseChanged();
        }

        private static FilterNamedGroup FindFilterGroup(string groupId)
        {
            foreach (var kind in new[] { FilterProfileService.MarketKind.Credit, FilterProfileService.MarketKind.Rate })
            {
                foreach (var g in FilterProfileService.GetFilterGroups(kind))
                {
                    if (g.Id == groupId) return g;
                }
            }
            return null;
        }

        private static void RaiseChanged()
        {
            ActiveGroupsChanged?.Invoke();
        }
    }
}

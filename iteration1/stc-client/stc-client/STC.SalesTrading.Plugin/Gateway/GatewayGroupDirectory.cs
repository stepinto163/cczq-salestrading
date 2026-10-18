using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Gateway.Contracts.Dictionary;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using STC.SalesTrading.Plugin.Gateway.Contracts.Base;
using STC.SalesTrading.Plugin.Gateway.Contracts.Group;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// 分组目录网关实现（ficc.watchlist.* / ficc.filterGroup.*，scope=MARKET）。
    /// 后端按「一次一组」保存（H20/H23 完整替换该组成员）、按 Id 删除（H21/H24），
    /// UI 的整体替换语义在此做差量映射：传入集合逐组 upsert、服务端多余组删除。
    /// <para>已知限制（方案 §3.2.6 二期完善）：筛选条件 UI 模型是 key|value chip，无运算符与码表——
    /// 落库统一按 EQ（多值逗号分隔按 IN），显示 label 暂用字段名；条件编辑器上线后替换转换。</para>
    /// <para>组级 expectedRevision 缓存于读取时；弹窗为模态低频操作，保存同步等待（netapi 同步风格一致）。</para>
    /// </summary>
    internal sealed class GatewayGroupDirectory : IGroupDirectory
    {
        private readonly GatewayRequester requester;
        private readonly Dictionary<string, string> watchRevisions = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> filterRevisions = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<FilterProfileService.MarketKind, IReadOnlyList<FilterFieldEntry>> filterFieldCache =
            new Dictionary<FilterProfileService.MarketKind, IReadOnlyList<FilterFieldEntry>>();

        public GatewayGroupDirectory(GatewayRequester requester)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
        }

        public ObservableCollection<FavBondGroup> GetFavGroups(FilterProfileService.MarketKind market)
        {
            var data = QueryGroups(GatewayTopics.WatchlistQuery, ToMarketCode(market));
            var result = new ObservableCollection<FavBondGroup>();
            foreach (var w in data.Watchlists ?? new List<WatchlistDto>())
            {
                if (w == null || w.WatchlistId == null) continue;
                watchRevisions[w.WatchlistId] = w.Revision;
                var group = new FavBondGroup { Id = w.WatchlistId, Name = w.Name };
                foreach (var item in w.Securities ?? new List<WatchlistItem>())
                {
                    if (item == null || item.SecurityId == null) continue;
                    group.Bonds.Add(new FavBondEntry { Code = item.SecurityId, Found = true });
                }
                result.Add(group);
            }

            // 回写会话缓存（FilterProfileService 静态内存）：侧栏 chip / chip 过滤与弹窗同一数据源，
            // Save 内部触发 DataChanged 驱动侧栏重建
            FilterProfileService.SaveFavGroups(market, result);
            return result;
        }

        public ObservableCollection<FilterNamedGroup> GetFilterGroups(FilterProfileService.MarketKind market)
        {
            var data = QueryGroups(GatewayTopics.FilterGroupQuery, ToMarketCode(market));
            var result = new ObservableCollection<FilterNamedGroup>();

            // 条件显示翻译（field/value 代码 → dictionary.query 中文 label）；目录不可用时回退显示代码
            var fieldLabels = new Dictionary<string, string>(StringComparer.Ordinal);
            var valueLabels = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                foreach (var f in GetFilterFields(market))
                {
                    if (!string.IsNullOrEmpty(f.Label)) fieldLabels[f.Field] = f.Label;
                    foreach (var opt in f.EnumValues)
                    {
                        if (!string.IsNullOrEmpty(opt.Value) && !string.IsNullOrEmpty(opt.Label))
                        {
                            valueLabels[f.Field + "|" + opt.Value] = opt.Label;
                        }
                    }
                }
            }
            catch
            {
                // 目录不可用时 chip 显示落库代码
            }

            foreach (var g in data.FilterGroups ?? new List<FilterGroupDto>())
            {
                if (g == null || g.FilterGroupId == null) continue;
                filterRevisions[g.FilterGroupId] = g.Revision;
                var group = new FilterNamedGroup { Id = g.FilterGroupId, Name = g.Name };
                foreach (var subgroup in g.Subgroups ?? new List<FilterSubgroupDto>())
                {
                    if (subgroup == null) continue;
                    var ui = new FilterSubgroup();
                    foreach (var condition in subgroup.Conditions ?? new List<FilterGroupCondition>())
                    {
                        if (condition == null || condition.Field == null) continue;
                        var valueText = FormatConditionValue(condition.Value);
                        string fieldLabel;
                        fieldLabels.TryGetValue(condition.Field, out fieldLabel);
                        ui.Conditions.Add(new FilterCondition(
                            condition.Field,
                            fieldLabel ?? condition.Field,
                            valueText,
                            TranslateConditionValue(valueLabels, condition.Field, valueText)));
                    }
                    if (ui.Conditions.Count > 0) group.Subgroups.Add(ui);
                }
                result.Add(group);
            }

            // 回写会话缓存：同 GetFavGroups
            FilterProfileService.SaveFilterGroups(market, result);
            return result;
        }

        /// <summary>
        /// 筛选字段目录（ficc.quote.dictionary.query，H14）。条件落库 field/operator 的唯一事实源；
        /// 按 market 缓存（目录低频变化）。
        /// </summary>
        public IReadOnlyList<FilterFieldEntry> GetFilterFields(FilterProfileService.MarketKind market)
        {
            lock (filterFieldCache)
            {
                if (filterFieldCache.TryGetValue(market, out var cached)) return cached;
            }

            var data = requester.RequestAsync<DictionaryRequest, DictionaryData>(
                GatewayTopics.QuoteDictionaryQuery, new DictionaryRequest(),
                System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            var fields = (data.FilterFields ?? new List<Contracts.Dictionary.FilterFieldDto>())
                .Where(f => f != null && f.Field != null && f.Enabled != false)
                .Select(f =>
                {
                    var entry = new FilterFieldEntry
                    {
                        Field = f.Field,
                        Label = f.Label,
                        Type = f.Type,
                        Operators = f.Operators ?? new List<string>(),
                        EnumValues = (f.EnumValues ?? new List<Contracts.Dictionary.DictionaryOption>())
                            .Where(o => o != null)
                            .Select(o => new FilterEnumOption { Value = o.Value, Label = o.Label })
                            .ToList(),
                        Group = f.Group,
                    };
                    // 枚举候选为空的字段回退业务字典（远端同款：字典候选兜底）
                    if ((entry.EnumValues == null || entry.EnumValues.Count == 0)
                        && data.Dictionaries != null && data.Dictionaries.TryGetValue(f.Field, out var options))
                    {
                        entry.EnumValues = options.Where(o => o != null)
                            .Select(o => new FilterEnumOption { Value = o.Value, Label = o.Label }).ToList();
                    }
                    return entry;
                })
                .ToList();
            lock (filterFieldCache)
            {
                filterFieldCache[market] = fields;
            }
            return fields;
        }

        /// <summary>只 upsert 单个关注分组（无差量删除）：右键加入分组等轻量入口。
        /// DisplayOrder 取该组在会话缓存中的现位置；新组建档成功回填服务端 Id。</summary>
        public void UpsertFavGroup(FilterProfileService.MarketKind market, FavBondGroup group)
        {
            if (group == null) return;
            var marketCode = ToMarketCode(market);
            string serverId = null;
            string expected = null;
            if (group.Id != null && watchRevisions.TryGetValue(group.Id, out expected))
            {
                serverId = group.Id;
            }
            var index = FilterProfileService.GetFavGroups(market).IndexOf(group);
            var request = new WatchlistSaveRequest
            {
                WatchlistId = serverId,
                Name = group.Name,
                Market = marketCode,
                Scope = "MARKET",
                DisplayOrder = index < 0 ? 0 : index,
                Securities = group.Bonds
                    .Select((b, i) => new WatchlistItem { SecurityId = b.Code, DisplayOrder = i })
                    .Where(x => x.SecurityId != null)
                    .ToList(),
                ExpectedRevision = expected,
            };
            var data = requester.RequestAsync<WatchlistSaveRequest, WatchlistSaveData>(
                GatewayTopics.WatchlistSave, request, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            var saved = data != null ? data.Watchlist : null;
            if (saved != null && saved.WatchlistId != null)
            {
                watchRevisions[saved.WatchlistId] = saved.Revision;
                group.Id = saved.WatchlistId;
            }
        }

        public void SaveFavGroups(FilterProfileService.MarketKind market, IEnumerable<FavBondGroup> groups)
        {
            var marketCode = ToMarketCode(market);
            var list = (groups ?? new FavBondGroup[0]).ToList();
            var keptIds = new HashSet<string>(StringComparer.Ordinal);

            var order = 0;
            foreach (var group in list)
            {
                string serverId = null;
                string expected = null;
                if (group.Id != null && watchRevisions.ContainsKey(group.Id))
                {
                    serverId = group.Id;
                    watchRevisions.TryGetValue(group.Id, out expected);
                }
                var request = new WatchlistSaveRequest
                {
                    WatchlistId = serverId,
                    Name = group.Name,
                    Market = marketCode,
                    Scope = "MARKET",
                    DisplayOrder = order++,
                    Securities = group.Bonds
                        .Select((b, i) => new WatchlistItem { SecurityId = b.Code, DisplayOrder = i })
                        .Where(x => x.SecurityId != null)
                        .ToList(),
                    ExpectedRevision = expected,
                };
                var data = requester.RequestAsync<WatchlistSaveRequest, WatchlistSaveData>(
                    GatewayTopics.WatchlistSave, request, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                var saved = data != null ? data.Watchlist : null;
                if (saved != null && saved.WatchlistId != null)
                {
                    keptIds.Add(saved.WatchlistId);
                    watchRevisions[saved.WatchlistId] = saved.Revision;
                }
            }

            // 删除保护：本次没有任何一组建档成功解析时，绝不执行差量删除——
            // 否则"应答形状不符/保存全部失败"会把服务端现有分组清空（已踩坑）。
            if (list.Count > 0 && keptIds.Count == 0)
            {
                throw new GatewayApiException("gateway",
                    "保存结果无法确认（应答未返回分组 Id），已保留服务端现有分组，请重试");
            }

            DeleteMissing<WatchlistDeleteRequest, WatchlistDeleteData>(GatewayTopics.WatchlistDelete,
                watchRevisions.Keys.Except(keptIds, StringComparer.Ordinal).ToList(),
                id => new WatchlistDeleteRequest { WatchlistId = id, ExpectedRevision = watchRevisions[id] });
            foreach (var removed in watchRevisions.Keys.Where(id => !keptIds.Contains(id)).ToList())
            {
                watchRevisions.Remove(removed);
            }

            // 回写会话缓存：侧栏立即反映保存结果（内部触发 DataChanged）
            FilterProfileService.SaveFavGroups(market, list);
        }

        public void SaveFilterGroups(FilterProfileService.MarketKind market, IEnumerable<FilterNamedGroup> groups)
        {
            var marketCode = ToMarketCode(market);
            var list = (groups ?? new FilterNamedGroup[0]).ToList();
            var keptIds = new HashSet<string>(StringComparer.Ordinal);

            // 条件 Key 必须在后端字段目录内（1001 Unknown field 的前置拦截）；
            // 目录不可用时放行交给后端校验（保持旧行为）。
            IReadOnlyList<FilterFieldEntry> catalog;
            try { catalog = GetFilterFields(market); }
            catch { catalog = null; }
            var knownFields = catalog == null
                ? null
                : new HashSet<string>(catalog.Select(f => f.Field), StringComparer.Ordinal);
            if (knownFields != null)
            {
                foreach (var group in list)
                {
                    foreach (var sg in group.Subgroups)
                    {
                        foreach (var condition in sg.Conditions)
                        {
                            if (!knownFields.Contains(condition.Key))
                            {
                                throw new GatewayApiException("gateway",
                                    "条件 [" + condition.Label + ":" + condition.Value + "] 不在后端字段目录中，请移除后重试");
                            }
                        }
                    }
                }
            }

            var order = 0;
            foreach (var group in list)
            {
                string serverId = null;
                string expected = null;
                if (group.Id != null && filterRevisions.ContainsKey(group.Id))
                {
                    serverId = group.Id;
                    filterRevisions.TryGetValue(group.Id, out expected);
                }
                var request = new FilterGroupSaveRequest
                {
                    FilterGroupId = serverId,
                    Name = group.Name,
                    Market = marketCode,
                    Scope = "MARKET",
                    DisplayOrder = order++,
                    Subgroups = group.Subgroups.Select(s => new FilterSubgroupDto
                    {
                        Conditions = s.Conditions.Select(ToCondition).ToList(),
                    }).ToList(),
                    ExpectedRevision = expected,
                };
                var data = requester.RequestAsync<FilterGroupSaveRequest, FilterGroupSaveData>(
                    GatewayTopics.FilterGroupSave, request, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                var saved = data != null ? data.FilterGroup : null;
                if (saved != null && saved.FilterGroupId != null)
                {
                    keptIds.Add(saved.FilterGroupId);
                    filterRevisions[saved.FilterGroupId] = saved.Revision;
                }
            }

            // 删除保护：同上
            if (list.Count > 0 && keptIds.Count == 0)
            {
                throw new GatewayApiException("gateway",
                    "保存结果无法确认（应答未返回分组 Id），已保留服务端现有分组，请重试");
            }

            DeleteMissing<FilterGroupDeleteRequest, FilterGroupDeleteData>(GatewayTopics.FilterGroupDelete,
                filterRevisions.Keys.Except(keptIds, StringComparer.Ordinal).ToList(),
                id => new FilterGroupDeleteRequest { FilterGroupId = id, ExpectedRevision = filterRevisions[id] });
            foreach (var removed in filterRevisions.Keys.Where(id => !keptIds.Contains(id)).ToList())
            {
                filterRevisions.Remove(removed);
            }

            // 回写会话缓存：同 SaveFavGroups
            FilterProfileService.SaveFilterGroups(market, list);
        }

        #region 转换辅助

        /// <summary>
        /// UI chip → 后端条件。优先取结构化载荷（目录编解码产物：文本 CONTAINS/区间 BETWEEN/布尔 EQ 等）；
        /// 无载荷回退旧编码：单值 EQ、逗号分隔 IN（限制见类注释）。
        /// </summary>
        private static FilterGroupCondition ToCondition(FilterCondition condition)
        {
            var payload = condition.Protocol as Abstractions.Models.FilterConditionPayload;
            if (payload != null && payload.Field != null && payload.Operator != null)
            {
                return new FilterGroupCondition
                {
                    Field = payload.Field,
                    Operator = payload.Operator,
                    Value = payload.Value,
                };
            }

            var value = condition.Value ?? string.Empty;
            var parts = value.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            return new FilterGroupCondition
            {
                Field = condition.Key,
                Operator = parts.Length > 1 ? "IN" : "EQ",
                Value = parts.Length > 1 ? (object)parts.Select(p => p.Trim()).ToList() : value.Trim(),
            };
        }

        private static string FormatConditionValue(object value)
        {
            // 协议 IN 值是数组，反序列化后为 List<object>/JSONArray——按元素展开逗号连接。
            // 原实现 IEnumerable<string> 强转失败走 ToString()，chip 显示成 ["CorporateBonds"] 原文（实测踩坑）
            if (value is IEnumerable<object> list)
            {
                return string.Join(",", list.Select(v => v?.ToString() ?? string.Empty));
            }
            if (value is System.Collections.IEnumerable raw && !(value is string))
            {
                var parts = new List<string>();
                foreach (var item in raw) parts.Add(item?.ToString() ?? string.Empty);
                return string.Join(",", parts);
            }
            return value != null ? value.ToString() : string.Empty;
        }

        /// <summary>把落库值（单值或逗号分隔 IN 串）逐段翻译为中文 label；无任何映射命中时返回 null（chip 显示落库值）。</summary>
        private static string TranslateConditionValue(Dictionary<string, string> valueLabels, string field, string valueText)
        {
            var parts = valueText.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            var translated = new List<string>(parts.Length);
            var any = false;
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                string label;
                if (valueLabels.TryGetValue(field + "|" + trimmed, out label))
                {
                    any = true;
                    translated.Add(label);
                }
                else
                {
                    translated.Add(trimmed);
                }
            }
            return any ? string.Join(",", translated) : null;
        }

        private static string ToMarketCode(FilterProfileService.MarketKind market)
        {
            return market == FilterProfileService.MarketKind.Rate ? "RATE" : "CREDIT";
        }

        private GroupQueryData QueryGroups(string topic, string marketCode)
        {
            var request = new Dictionary<string, string> { { "market", marketCode }, { "scope", "MARKET" } };
            var data = requester.RequestAsync<Dictionary<string, string>, GroupQueryData>(
                topic, request, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            return data ?? new GroupQueryData();
        }

        private void DeleteMissing<TReq, TData>(string topic, List<string> ids, Func<string, TReq> buildRequest)
            where TReq : class
            where TData : class
        {
            foreach (var id in ids)
            {
                try
                {
                    requester.RequestAsync<TReq, TData>(topic, buildRequest(id), System.Threading.CancellationToken.None)
                        .GetAwaiter().GetResult();
                }
                catch
                {
                    // 单组删除失败不阻断其余组的保存；下次保存会再次尝试清理
                }
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using UiGroup = STC.SalesTrading.Plugin.Shared.ViewModel.FilterNamedGroup;
using UiSubgroup = STC.SalesTrading.Plugin.Shared.ViewModel.FilterSubgroup;
using MarketKind = STC.SalesTrading.Plugin.Shared.Services.FilterProfileService.MarketKind;

namespace STC.SalesTrading.Plugin.Ficc
{
    /// <summary>分组的类型化 DPS 边界；用户身份由 GW 登录会话决定，修改使用服务端 revision。</summary>
    public sealed class DpsGroupStore
    {
        public FiccApiClient Api { get; }
        private readonly Func<Task<DictionaryData>> dictionary;
        private GroupConditionCatalog catalog;
        private readonly Dictionary<string, string> pendingRequests = new Dictionary<string, string>();
        private async Task<TData> SaveAsync<TRequest, TData>(Endpoint<TRequest, TData> endpoint, TRequest request)
            where TRequest : Request where TData : ProtocolModel
        {
            // 应答丢失时，用户重试沿用相同业务请求的 requestId，由现有服务端回执去重。
            string key = endpoint.Topic + request.ToJson().ToString(Newtonsoft.Json.Formatting.None);
            if (!pendingRequests.TryGetValue(key, out string requestId))
            {
                requestId = "STC-" + Guid.NewGuid().ToString("N");
                pendingRequests[key] = requestId;
            }
            request.RequestId = requestId;
            var result = await Api.RequestAsync(endpoint, request);
            pendingRequests.Remove(key);
            return result;
        }
        public DpsGroupStore(FiccApiClient api, Func<Task<DictionaryData>> dictionary)
        {
            Api = api;
            this.dictionary = dictionary;
        }
        public sealed class Snapshot
        {
            public GroupConditionCatalog Catalog;
            public List<FavBondGroup>[] Favorites = { new List<FavBondGroup>(), new List<FavBondGroup>() };
            public List<UiGroup>[] Filters = { new List<UiGroup>(), new List<UiGroup>() };
        }
        public async Task<Snapshot> LoadAsync()
        {
            if (catalog == null) catalog = new GroupConditionCatalog(await dictionary());
            var result = new Snapshot { Catalog = catalog };
            var watchlists = await Api.RequestAsync(SalesTradingApi.WatchlistQuery, new WatchlistQueryRequest { Scope = "MARKET" });
            var groups = await Api.RequestAsync(SalesTradingApi.FilterGroupQuery, new FilterGroupQueryRequest { Scope = "MARKET" });
            foreach (var item in watchlists.Watchlists ?? new List<Watchlist>())
            {
                var group = new FavBondGroup { Id = item.WatchlistId, Revision = item.Revision, Name = item.Name };
                foreach (var security in (item.Securities ?? new List<WatchlistSecurity>()).OrderBy(bond => bond.DisplayOrder))
                    group.Bonds.Add(new FavBondEntry { Code = security.SecurityId });
                result.Favorites[item.Market == "RATE" ? 1 : 0].Add(group);
            }
            foreach (var item in groups.FilterGroups ?? new List<FilterGroup>())
            {
                var market = item.Market == "RATE" ? MarketKind.Rate : MarketKind.Credit;
                var group = new UiGroup { Id = item.FilterGroupId, Revision = item.Revision, Name = item.Name };
                foreach (var sub in item.Subgroups ?? new List<Protocol.FilterSubgroup>())
                {
                    var uiSub = new UiSubgroup();
                    foreach (var condition in sub.Conditions ?? new List<Protocol.FilterCondition>())
                    {
                        if (GroupConditionCatalog.Prohibited(market, condition.Field)) continue;
                        uiSub.Conditions.Add(result.Catalog.Display(condition));
                    }
                    if (uiSub.Conditions.Count > 0) group.Subgroups.Add(uiSub);
                }
                result.Filters[(int)market].Add(group);
            }
            return result;
        }

        private static void ValidateNames(IEnumerable<string> names)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("分组名称不能为空。");
                if (!seen.Add(name.Trim())) throw new InvalidOperationException("分组名称不能重复：" + name);
            }
        }

        public async Task SaveFavoritesAsync(MarketKind market, List<FavBondGroup> previous, List<FavBondGroup> draft)
        {
            ValidateNames(draft.Select(group => group.Name));
            // 先保存，再删除。逐项采用服务端回执更新 revision，失败时不谎报整体成功。
            foreach (var group in draft)
            {
                var original = previous.FirstOrDefault(item => item.Id == group.Id);
                if (original != null && original.Name == group.Name
                    && original.Bonds.Select(bond => bond.Code).SequenceEqual(group.Bonds.Select(bond => bond.Code)))
                {
                    continue;
                }
                var saved = await SaveAsync(SalesTradingApi.WatchlistSave, new WatchlistSaveRequest
                {
                    WatchlistId = group.Revision == null ? null : group.Id,
                    ExpectedRevision = group.Revision,
                    Name = group.Name.Trim(), Market = market.ToString().ToUpperInvariant(), Scope = "MARKET",
                    Securities = group.Bonds.Select((bond, index) => new WatchlistSecurity { SecurityId = bond.Code, DisplayOrder = index }).ToList()
                });
                group.Id = saved.Watchlist.WatchlistId;
                group.Revision = saved.Watchlist.Revision;
            }
            foreach (var removed in previous.Where(old => draft.All(group => group.Id != old.Id)))
                await SaveAsync(SalesTradingApi.WatchlistDelete, new WatchlistDeleteRequest { WatchlistId = removed.Id, ExpectedRevision = removed.Revision });
        }

        public async Task SaveFiltersAsync(MarketKind market, List<UiGroup> previous, List<UiGroup> draft, GroupConditionCatalog catalog)
        {
            ValidateNames(draft.Select(group => group.Name));
            var requests = draft.Select(group => new FilterGroupSaveRequest
            {
                FilterGroupId = group.Revision == null ? null : group.Id,
                ExpectedRevision = group.Revision,
                Name = group.Name.Trim(), Market = market.ToString().ToUpperInvariant(), Scope = "MARKET",
                Subgroups = group.Subgroups.Select((sub, index) => new Protocol.FilterSubgroup
                {
                    DisplayOrder = index,
                    Conditions = sub.Conditions.Where(condition => !GroupConditionCatalog.Prohibited(market, condition.Key))
                        .Select(condition => catalog.Compile(market, condition)).ToList()
                }).Where(sub => sub.Conditions.Count > 0).ToList()
            }).ToList();
            for (int index = 0; index < requests.Count; index++)
            {
                var original = previous.FirstOrDefault(item => item.Id == draft[index].Id);
                if (original != null && original.Name == draft[index].Name
                    && original.Subgroups.SelectMany(sub => sub.Conditions).Select(condition => condition.Identity)
                        .SequenceEqual(draft[index].Subgroups.SelectMany(sub => sub.Conditions).Select(condition => condition.Identity)))
                {
                    continue;
                }
                var saved = await SaveAsync(SalesTradingApi.FilterGroupSave, requests[index]);
                draft[index].Id = saved.FilterGroup.FilterGroupId;
                draft[index].Revision = saved.FilterGroup.Revision;
            }
            foreach (var removed in previous.Where(old => draft.All(group => group.Id != old.Id)))
                await SaveAsync(SalesTradingApi.FilterGroupDelete, new FilterGroupDeleteRequest { FilterGroupId = removed.Id, ExpectedRevision = removed.Revision });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using UiFilterGroup = STC.SalesTrading.Plugin.Shared.ViewModel.FilterNamedGroup;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>当前登录账户的分组缓存。服务端保存成功后发布变更，弹窗编辑副本不影响当前查询。</summary>
    public static class FilterProfileService
    {
        public enum MarketKind { Credit, Rate }
        public static event Action DataChanged;
        private static readonly ObservableCollection<FavBondGroup>[] favorites = { new ObservableCollection<FavBondGroup>(), new ObservableCollection<FavBondGroup>() };
        private static readonly ObservableCollection<UiFilterGroup>[] filters = { new ObservableCollection<UiFilterGroup>(), new ObservableCollection<UiFilterGroup>() };
        private static DpsGroupStore store;
        private static Task loading;
        private static bool loaded;
        private static readonly SemaphoreSlim saveGate = new SemaphoreSlim(1, 1);
        public static IMarketCenterService LookupService { get; private set; }
        public static GroupConditionCatalog Catalog { get; private set; }

        public static void Attach(FiccApiClient api, IMarketCenterService lookup, Func<Task<DictionaryData>> dictionary)
        {
            Reset();
            LookupService = lookup;
            store = new DpsGroupStore(api, dictionary);
        }

        public static void Detach(FiccApiClient api)
        {
            if (store != null && ReferenceEquals(store.Api, api)) Reset();
        }

        private static void Reset()
        {
            store = null;
            loading = null;
            loaded = false;
            Catalog = null;
            LookupService = null;
            foreach (var collection in favorites) collection.Clear();
            foreach (var collection in filters) collection.Clear();
            DataChanged?.Invoke();
        }

        public static Task EnsureLoadedAsync()
        {
            if (store == null) throw new InvalidOperationException("分组服务尚未连接到登录账户。");
            if (loaded) return Task.CompletedTask;
            if (loading == null || loading.IsFaulted || loading.IsCanceled) loading = LoadAsync(store);
            return loading;
        }

        public static async Task ReloadAsync()
        {
            if (!loaded)
            {
                await EnsureLoadedAsync();
                return;
            }
            await LoadAsync(store);
        }

        private static async Task LoadAsync(DpsGroupStore current)
        {
            var snapshot = await current.LoadAsync();
            if (!ReferenceEquals(store, current)) throw new OperationCanceledException("登录账户已切换。");
            Catalog = snapshot.Catalog;
            foreach (MarketKind market in Enum.GetValues(typeof(MarketKind)))
            {
                Replace(GetFavGroups(market), snapshot.Favorites[(int)market]);
                Replace(GetFilterGroups(market), snapshot.Filters[(int)market]);
            }
            loaded = true;
            DataChanged?.Invoke();
        }

        public static async Task RefreshBondDetailsAsync(MarketKind market)
        {
            var current = store;
            var lookup = LookupService;
            foreach (var bond in GetFavGroups(market).SelectMany(group => group.Bonds).ToList())
            {
                var hit = await lookup.LookupBondAsync(bond.Code, CancellationToken.None);
                if (!ReferenceEquals(store, current)) throw new OperationCanceledException("登录账户已切换。");
                bond.Name = hit.Name ?? string.Empty;
                bond.Tenor = hit.Tenor ?? "—";
                bond.BondLib = hit.BondLibrary ?? "—";
                bond.Valuation = hit.Valuation ?? "—";
                bond.Found = hit.Found;
                bond.Market = hit.Market;
            }
        }

        public static ObservableCollection<FavBondGroup> GetFavGroups(MarketKind market) => favorites[(int)market];
        public static ObservableCollection<UiFilterGroup> GetFilterGroups(MarketKind market) => filters[(int)market];
        public static List<CatalogSection> GetCatalog(MarketKind market) => Catalog?.ForMarket(market)
            ?? throw new InvalidOperationException("筛选目录尚未加载。");

        public static async Task SaveFavGroupsAsync(MarketKind market, IEnumerable<FavBondGroup> groups, List<FavBondGroup> originalGroups)
        {
            await EnsureLoadedAsync();
            var draft = groups.ToList();
            var current = store;
            await saveGate.WaitAsync();
            try
            {
                if (!ReferenceEquals(store, current)) throw new OperationCanceledException("登录账户已切换。请重新打开分组管理。 ");
                await current.SaveFavoritesAsync(market, originalGroups, draft);
                if (!ReferenceEquals(store, current)) throw new OperationCanceledException("登录账户已切换。");
                await LoadAsync(current);
            }
            catch
            {
                // 部分请求可能已成功；重新读取服务端事实，编辑副本保留已收到的 ID/revision。
                try { await LoadAsync(current); }
                catch (Exception reloadError) { com.gateway.log.LogHelper.Error(reloadError); }
                throw;
            }
            finally { saveGate.Release(); }
        }

        public static async Task SaveFilterGroupsAsync(MarketKind market, IEnumerable<UiFilterGroup> groups, List<UiFilterGroup> originalGroups)
        {
            await EnsureLoadedAsync();
            var draft = groups.ToList();
            var current = store;
            await saveGate.WaitAsync();
            try
            {
                if (!ReferenceEquals(store, current)) throw new OperationCanceledException("登录账户已切换。请重新打开分组管理。 ");
                await current.SaveFiltersAsync(market, originalGroups, draft, Catalog);
                if (!ReferenceEquals(store, current)) throw new OperationCanceledException("登录账户已切换。");
                await LoadAsync(current);
            }
            catch
            {
                // 部分请求可能已成功；重新读取服务端事实，编辑副本保留已收到的 ID/revision。
                try { await LoadAsync(current); }
                catch (Exception reloadError) { com.gateway.log.LogHelper.Error(reloadError); }
                throw;
            }
            finally { saveGate.Release(); }
        }

        private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
        {
            target.Clear();
            foreach (var value in values) target.Add(value);
        }
    }
}

using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private string groupTestPrefix;
        private async Task CheckGroupManagementAsync()
        {
            groupTestPrefix = "UI" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var checks = new JArray();
            Action<bool, string> check = (ok, message) =>
            {
                Require(ok, message);
                checks.Add(message);
                File.WriteAllText(Path.Combine(reportDirectory, "group-checks.json"), checks.ToString());
            };
            await NavigateAsync("market");
            var market = model.MarketCenterPage;
            market.SelectedSubTab = market.SubTabs.Single(tab => tab.Key == "rateAll");
            await FilterProfileService.EnsureLoadedAsync();
            await Until(() => Descendants<RateFilterPanel>(view).Any(panel => panel.IsVisible), "利率侧栏可见");
            var panel = Descendants<RateFilterPanel>(view).Single(item => item.IsVisible);
            Click(Descendants<Button>(panel).Single(button => Convert.ToString(button.ToolTip) == "管理关注债券分组"));
            await Until(() => Application.Current.Windows.OfType<FavBondManagerWindow>().Any(), "关注管理弹窗");
            var favorites = Application.Current.Windows.OfType<FavBondManagerWindow>().Single();
            CreateGroupThroughUi(favorites, groupTestPrefix);
            SetText((TextBox)favorites.FindName("BondInputBox"), Environment.GetEnvironmentVariable("STC_UI_SECURITY") ?? "240205.IB");
            Click((Button)favorites.FindName("IdentifyButton"));
            var favoriteModel = (FavBondManagerViewModel)favorites.DataContext;
            await Until(() => favoriteModel.Results.Count == 1 && favoriteModel.Results[0].Found, "真实基础资料识别债券");
            favorites.UpdateLayout();
            Click(Descendants<CheckBox>(favorites).Single(box => ReferenceEquals(box.DataContext, favoriteModel.Results[0])));
            check(favoriteModel.SelectedGroup.Bonds.Count == 1, "勾选识别结果加入编辑副本");
            Click(Button(favorites, "保存"));
            await Until(() => !favorites.IsVisible, "关注分组保存关闭");
            var watchlist = FilterProfileService.GetFavGroups(FilterProfileService.MarketKind.Rate).Single(group => group.Name == groupTestPrefix);
            check(watchlist.Id.StartsWith("W-") && watchlist.Revision != null, "关注分组采用服务端ID和版本");
            await FilterProfileService.ReloadAsync();
            check(FilterProfileService.GetFavGroups(FilterProfileService.MarketKind.Rate).Single(group => group.Id == watchlist.Id).Bonds.Count == 1, "重读服务端保留关注债券");
            view.UpdateLayout();
            Click(Descendants<ToggleButton>(panel).Single(button => Convert.ToString(button.Content) == groupTestPrefix));
            await Until(() => !market.RateAll.IsLoading && market.RateAll.Rows.Any(row => row.QuoteId == quoteId), "关注组激活后行情命中");
            string security = (string)(await model.Backend.Api.GetQuoteAsync(quoteId))["securityId"];
            check(market.RateAll.Rows.All(row => row.Code == security), "关注组由服务端过滤完整行情");

            Click(Descendants<Button>(panel).Single(button => Convert.ToString(button.ToolTip) == "管理筛选分组"));
            await Until(() => Application.Current.Windows.OfType<FilterGroupManagerWindow>().Any(), "筛选管理弹窗");
            var filters = Application.Current.Windows.OfType<FilterGroupManagerWindow>().Single();
            CreateGroupThroughUi(filters, groupTestPrefix);
            var filterModel = (FilterGroupManagerViewModel)filters.DataContext;
            check(filterModel.Catalog.SelectMany(section => section.Groups).Any(group => group.Key == "bondSubtype"), "目录来自实际侧栏字段");
            check(!filterModel.Catalog.SelectMany(section => section.Groups).Any(group => GroupConditionCatalog.Prohibited(FilterProfileService.MarketKind.Rate, group.Key)), "利率目录不含禁止条件");
            Click(Button(filters, "国开"));
            Click(Button(filters, "3-5Y"));
            Click(Button(filters, "5-7Y"));
            Capture("group-filter-editor", filters);
            check(filterModel.SelectedGroup.ConditionCount == 3, "界面选择跨字段AND与同字段OR条件");
            Click(Button(filters, "保存"));
            await Until(() => !filters.IsVisible, "筛选分组保存关闭");
            var filterGroup = FilterProfileService.GetFilterGroups(FilterProfileService.MarketKind.Rate).Single(group => group.Name == groupTestPrefix);
            await FilterProfileService.ReloadAsync();
            var reloaded = FilterProfileService.GetFilterGroups(FilterProfileService.MarketKind.Rate).Single(group => group.Id == filterGroup.Id);
            check(reloaded.ConditionCount == 3 && reloaded.Subgroups.SelectMany(sub => sub.Conditions).All(condition => condition.Protocol != null), "条件类型和值从服务端无损恢复");
            view.UpdateLayout();
            Click(Descendants<ToggleButton>(panel).Single(button => Convert.ToString(button.Content) == groupTestPrefix && button.DataContext is MarketGroupChip chip && !chip.IsFavorite));
            await Until(() => !market.RateAll.IsLoading && market.RateAll.Rows.All(row => row.QuoteId != quoteId), "组合分组查询结束");
            var combined = MarketGroupSelectionService.ApplyTo(new JObject(), "RATE");
            check(((JArray)combined["watchlistIds"]).Count == 1 && ((JArray)combined["filterGroupIds"]).Count == 1, "关注组与筛选组同时传给服务端");
            // 当前240205剩余期限约7年：两个不命中的期限区间应排除，不能被客户端近似匹配放行。
            check(market.RateAll.Rows.All(row => row.QuoteId != quoteId), "组合条件排除不匹配报价");
            MarketGroupSelectionService.ClearActive();
            await Until(() => !market.RateAll.IsLoading && market.RateAll.Rows.Any(row => row.QuoteId == quoteId), "清除激活后恢复行情");
            check(FilterProfileService.GetFilterGroups(FilterProfileService.MarketKind.Rate).Any(group => group.Id == filterGroup.Id), "清除过滤不删除保存分组");
            Click(Descendants<Button>(panel).Single(button => Convert.ToString(button.ToolTip) == "管理筛选分组"));
            await Until(() => Application.Current.Windows.OfType<FilterGroupManagerWindow>().Any(), "再次打开筛选分组");
            filters = Application.Current.Windows.OfType<FilterGroupManagerWindow>().Single();
            CreateGroupThroughUi(filters, groupTestPrefix + "X");
            Click(Button(filters, "取消"));
            await FilterProfileService.ReloadAsync();
            check(!FilterProfileService.GetFilterGroups(FilterProfileService.MarketKind.Rate).Any(group => group.Name == groupTestPrefix + "X"), "取消新建不落库");
            await NavigateAsync("mgmt");
            check(model.QuoteMgmtPage.MergedFavChips.Any(chip => chip.GroupId == watchlist.Id)
                && model.QuoteMgmtPage.MergedFilterChips.Any(chip => chip.GroupId == filterGroup.Id), "报价管理共用账户分组");

            // 两个编辑副本：后提交的旧版本必须冲突，不能覆盖已完成的重命名。
            var firstEditor = new FavBondManagerViewModel(FilterProfileService.MarketKind.Rate);
            var staleEditor = new FavBondManagerViewModel(FilterProfileService.MarketKind.Rate);
            firstEditor.Groups.Single(group => group.Id == watchlist.Id).Name = groupTestPrefix + "A";
            await firstEditor.SaveAsync();
            staleEditor.Groups.Single(group => group.Id == watchlist.Id).Name = groupTestPrefix + "B";
            bool conflict = false;
            try
            {
                await staleEditor.SaveAsync();
            }
            catch (InvalidOperationException error)
            {
                conflict = error.Message.StartsWith("1004:");
            }
            check(conflict && FilterProfileService.GetFavGroups(FilterProfileService.MarketKind.Rate)
                .Single(group => group.Id == watchlist.Id).Name == groupTestPrefix + "A", "旧编辑副本触发版本冲突且保留最新名称");

            await NavigateAsync("market");
            market.SelectedSubTab = market.SubTabs.Single(tab => tab.Key == "rateAll");
            await Until(() => Descendants<RateFilterPanel>(view).Any(item => item.IsVisible), "返回利率侧栏");
            panel = Descendants<RateFilterPanel>(view).Single(item => item.IsVisible);
            Click(Descendants<Button>(panel).Single(button => Convert.ToString(button.ToolTip) == "管理关注债券分组"));
            await Until(() => Application.Current.Windows.OfType<FavBondManagerWindow>().Any(), "重新打开关注管理");
            favorites = Application.Current.Windows.OfType<FavBondManagerWindow>().Single();
            favoriteModel = (FavBondManagerViewModel)favorites.DataContext;
            var groupList = (ListBox)favorites.FindName("GroupList");
            SelectItem(groupList, favoriteModel.Groups.Single(group => group.Id == watchlist.Id));
            groupList.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                { RoutedEvent = Control.MouseDoubleClickEvent });
            favorites.UpdateLayout();
            var rename = Descendants<TextBox>(groupList).Single(box => box.IsVisible && box.Name == "GEdit");
            SetText(rename, groupTestPrefix + "R");
            rename.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(rename), Environment.TickCount, Key.Enter)
                { RoutedEvent = Keyboard.KeyDownEvent });
            var bondRemove = Descendants<TextBlock>(favorites).Single(block => block.Tag is FavBondEntry && block.Text == "✕");
            bondRemove.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
            Capture("group-favorite-editor", favorites);
            Click(Button(favorites, "保存"));
            await Until(() => !favorites.IsVisible, "关注组编辑保存");
            var edited = FilterProfileService.GetFavGroups(FilterProfileService.MarketKind.Rate).Single(group => group.Id == watchlist.Id);
            check(edited.Name == groupTestPrefix + "R" && edited.Bonds.Count == 0, "界面重命名与移除债券保存成功且ID不变");

            Click(Descendants<Button>(panel).Single(button => Convert.ToString(button.ToolTip) == "管理关注债券分组"));
            await Until(() => Application.Current.Windows.OfType<FavBondManagerWindow>().Any(), "打开关注组删除");
            favorites = Application.Current.Windows.OfType<FavBondManagerWindow>().Single();
            favorites.UpdateLayout();
            Click(Descendants<Button>(favorites).Single(button => button.DataContext is FavBondGroup group && group.Id == watchlist.Id));
            Click(Button(favorites, "保存"));
            await Until(() => !favorites.IsVisible, "关注组删除保存");
            check(!FilterProfileService.GetFavGroups(FilterProfileService.MarketKind.Rate).Any(group => group.Id == watchlist.Id), "界面删除关注组已落库");

            Click(Descendants<Button>(panel).Single(button => Convert.ToString(button.ToolTip) == "管理筛选分组"));
            await Until(() => Application.Current.Windows.OfType<FilterGroupManagerWindow>().Any(), "打开筛选组删除");
            filters = Application.Current.Windows.OfType<FilterGroupManagerWindow>().Single();
            filters.UpdateLayout();
            Task confirmation = ConfirmGroupDeletionAsync();
            Click(Descendants<Button>(filters).Single(button => button.DataContext is FilterNamedGroup group && group.Id == filterGroup.Id));
            await confirmation;
            Click(Button(filters, "保存"));
            await Until(() => !filters.IsVisible, "筛选组删除保存");
            check(!FilterProfileService.GetFilterGroups(FilterProfileService.MarketKind.Rate).Any(group => group.Id == filterGroup.Id), "非空筛选组确认删除已落库");

        }

        private async Task ConfirmGroupDeletionAsync()
        {
            await Until(() => Application.Current.Windows.OfType<ConfirmDialog>().Any(), "非空分组删除确认");
            Click(Button(Application.Current.Windows.OfType<ConfirmDialog>().Single(), "确认"));
        }

        private static void CreateGroupThroughUi(Window window, string name)
        {
            Click((Button)window.FindName("NewGroupButton"));
            var input = (TextBox)window.FindName("NewGroupInput");
            SetText(input, name);
            input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), Environment.TickCount, Key.Enter)
                { RoutedEvent = Keyboard.KeyDownEvent });
        }

        private async Task CleanupGroupFixturesAsync()
        {
            if (groupTestPrefix == null) return;
            foreach (var window in Application.Current.Windows.OfType<Window>().Where(window => window is FavBondManagerWindow || window is FilterGroupManagerWindow).ToArray()) window.Close();
            MarketGroupSelectionService.ClearActive();
            var watches = await model.Backend.Api.RequestAsync(SalesTradingApi.WatchlistQuery, new WatchlistQueryRequest());
            foreach (var group in watches.Watchlists.Where(group => group.Name.StartsWith(groupTestPrefix)))
                await model.Backend.Api.RequestAsync(SalesTradingApi.WatchlistDelete, new WatchlistDeleteRequest { WatchlistId = group.WatchlistId, ExpectedRevision = group.Revision });
            var filters = await model.Backend.Api.RequestAsync(SalesTradingApi.FilterGroupQuery, new FilterGroupQueryRequest());
            foreach (var group in filters.FilterGroups.Where(group => group.Name.StartsWith(groupTestPrefix)))
                await model.Backend.Api.RequestAsync(SalesTradingApi.FilterGroupDelete, new FilterGroupDeleteRequest { FilterGroupId = group.FilterGroupId, ExpectedRevision = group.Revision });
            await FilterProfileService.ReloadAsync();
        }
    }
}

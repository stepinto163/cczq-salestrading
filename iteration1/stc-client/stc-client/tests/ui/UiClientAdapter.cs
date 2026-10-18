using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace SalesTrading.UiTests
{
    // 仅映射集成客户端的控件与数据结构；业务期望仍由原用例断言决定。
    internal sealed partial class UiScenarioWorker
    {
        private async Task VerifyPreviewRemainsLocalAsync(HashSet<string> previewIds)
        {
            string viewId = "ui-preview-" + Guid.NewGuid().ToString("N");
            try
            {
                var pending = await api.RequestAsync(Contracts.SalesTradingApi.QuoteViewOpen,
                    new Contracts.QuoteViewOpenRequest
                    {
                        ViewId = viewId, PageSize = 500, OwnerScope = "MINE",
                        Status = new List<string> { "PENDING_CONFIRM" },
                        Filter = new Contracts.Filter()
                    });
                Require(pending.View.TotalCount <= 500, "待确认断言超过单页上限，不能据不完整快照判定成功");
                Require(!pending.Quotes.Any(row => previewIds.Contains(row.ClientQuoteId)), "仅识别不得创建待确认报价");
            }
            finally
            {
                await api.RequestAsync(Contracts.SalesTradingApi.QuoteViewClose, new Contracts.ViewCloseRequest { ViewId = viewId });
            }
        }

        private DataGrid RecommendationGrid()
        {
            var grid = Descendants<DataGrid>(view).SingleOrDefault(control =>
                System.Windows.Data.BindingOperations.GetBinding(control, ItemsControl.ItemsSourceProperty)?.Path?.Path == "RecPageItems");
            Require(grid != null, "界面缺少推荐结果表：RecPageItems");
            return grid;
        }

        private static bool EditorReady(QuoteEntryDialog dialog)
        {
            return dialog.IsLoaded && ((Button)dialog.FindName("SubmitButton")).IsEnabled;
        }

        private static string EditorError(QuoteEntryDialog dialog)
        {
            return string.Join("\n", Descendants<TextBlock>(dialog).Select(text => text.Text)
                .Concat(ToastService.Messages.Select(message => message.Text)));
        }

        private static string FeedbackText(string feedback)
        {
            switch (feedback)
            {
                case "USEFUL": return "有用";
                case "NOT_USEFUL": return "无用";
                case "NOT_APPLICABLE": return "不适用";
                default: return "未反馈";
            }
        }

        private static Task ReloadRecommendationsAsync(BondDetailViewModel detail)
        {
            // 只用于验证重新读取后的持久化结果；选券、日期切换及反馈走真实界面。
            var method = typeof(BondDetailViewModel).GetMethod("ReloadRecommendationsAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("推荐重读适配器与客户端版本不匹配");
            return (Task)method.Invoke(detail, null);
        }

        private static async Task ReloadGroupsAsync()
        {
            var directory = FavBondManagerViewModel.Directory;
            if (directory == null) throw new InvalidOperationException("真实分组目录未初始化");
            await Task.Run(() =>
            {
                foreach (FilterProfileService.MarketKind market in Enum.GetValues(typeof(FilterProfileService.MarketKind)))
                {
                    directory.GetFavGroups(market);
                    directory.GetFilterGroups(market);
                }
            });
        }

        private static JObject ActiveGroupFilter()
        {
            var active = MarketGroupSelectionService.ActiveGroupIds(FilterProfileService.MarketKind.Rate);
            return new JObject { ["watchlistIds"] = new JArray(active.favorites), ["filterGroupIds"] = new JArray(active.filters) };
        }

        private IEnumerable<MarketGroupChip> ManagementChips(bool favorite)
        {
            // 直接检查实际显示的 chip；缺失时报错，不能用后台集合替代 UI 断言。
            return Descendants<System.Windows.FrameworkElement>(view)
                .Where(element => element.IsVisible)
                .Select(element => element.DataContext).OfType<MarketGroupChip>()
                .Where(chip => chip.IsFavorite == favorite).Distinct();
        }
    }

    internal static class FilterReportSerialization
    {
        public static JObject ToJson(this ProductFilter filter)
        {
            return JObject.FromObject(filter);
        }
    }
}

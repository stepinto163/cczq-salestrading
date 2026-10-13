using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using STC.SalesTrading.Plugin.Shell;
using STC.SalesTrading.Plugin.Shell.View;
using STC.SalesTrading.Plugin.View;

namespace STC.SalesTrading.Plugin.Ficc
{
    internal static class DpsUiContext
    {
        public static bool OpenQuote(Window owner, string quoteId, string securityId)
        {
            var shell = Application.Current.Windows.OfType<STC.Frame.Window1>().FirstOrDefault();
            var model = shell?.Panel.Items.OfType<TabItem>().Select(item => item.Content).OfType<MainView>()
                .Select(view => view.DataContext).OfType<MainViewModel>().FirstOrDefault();
            if (model?.Backend == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(quoteId))
            {
                model.SelectedNavItem = model.NavItems.First(item => item.Key == "quote");
                model.QuotePage.SelectedSubTab = model.QuotePage.SubTabs.First(item => item.Key == "quote-precise");
                // 保留已有草稿，债券仅作为新一行的录入起点。
                if (!string.IsNullOrWhiteSpace(securityId))
                    model.QuotePage.Precise.InputText += (string.IsNullOrWhiteSpace(model.QuotePage.Precise.InputText) ? "" : Environment.NewLine) + securityId + " ";
            }
            else
            {
                Modules.MarketCenter.View.QuoteEntryDialog.ShowForQuote(owner ?? shell, model.Backend.Api, quoteId);
            }
            return true;
        }
    }
}

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
            new QuoteOperationWindow(model.Backend.Api, quoteId, securityId) { Owner = owner ?? shell }.ShowDialog();
            return true;
        }
    }
}

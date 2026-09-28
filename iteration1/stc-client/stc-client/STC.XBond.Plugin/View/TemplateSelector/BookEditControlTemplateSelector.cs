using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.XBond.Plugin.View.TemplateSelector
{
    public class BookEditControlTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container == null)
            {
                return null;
            }

            if (item is BookEditItemViewModel)
            {
                BookEditItemViewModel bookEditItem = item as BookEditItemViewModel;
                if (bookEditItem.PanelType.Equals("IDB", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("IDB_PART");
                }
                else if (bookEditItem.PanelType.Equals("MARKET", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("MARKET_PART");
                }
                else if (bookEditItem.PanelType.Equals("BROKER", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("BROKER_PART");
                }
                else if (bookEditItem.PanelType.Equals("SELF", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("SELF_PART");
                }
                else if (bookEditItem.PanelType.Equals("IDB_UNDERLINE", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("IDB_UNDERLINE_PART");
                }
                else if (bookEditItem.PanelType.Equals("MARKET_UNDERLINE", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("MARKET_UNDERLINE_PART");
                }
                else if (bookEditItem.PanelType.Equals("BROKER_UNDERLINE", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("BROKER_UNDERLINE_PART");
                }
                else if (bookEditItem.PanelType.Equals("SELF_UNDERLINE", StringComparison.OrdinalIgnoreCase))
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("SELF_UNDERLINE_PART");
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}

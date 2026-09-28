using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.IRS.Plugin.View.Component
{
    public class FilterOptionStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            OptionItem optionItem = (OptionItem)item;
            if (string.IsNullOrEmpty(optionItem.Title))
            {
                return (Style)((FrameworkElement)container).FindResource("speratorStyle");
            }
            else
            {
                return (Style)((FrameworkElement)container).FindResource("optionItemStyle");
            }
        }
    }
}

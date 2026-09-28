using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BDB.Trade.Plugin.View.Trade
{
    public class MyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FirstTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return FirstTemplate;

        }
    }
}

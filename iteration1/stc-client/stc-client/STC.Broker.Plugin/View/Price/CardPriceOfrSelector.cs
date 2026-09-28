using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace STC.Broker.Plugin.View.Price
{
    public class CardPriceOfrSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var u = container as FrameworkElement;

            if (item == null)
                return u.FindResource("FullOfrQuotesModel") as DataTemplate;


            if (item != null)
            {
                if (Common.Utils.LocalEnv.GrLayoutSetting.QuotesShowMode == 1)
                    return u.FindResource("FullOfrQuotesModel") as DataTemplate;
                else
                    return u.FindResource("SimpleOfrQuotesModel") as DataTemplate;
            }

            return u.FindResource("FullOfrQuotesModel") as DataTemplate;
        }
    }
}

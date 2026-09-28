using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace STC.Broker.Plugin.View.Price
{
    public class CardPriceBidSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var u = container as FrameworkElement;

            if (item == null)
                return u.FindResource("FullBidQuotesModel") as DataTemplate;

            if (item != null)
            {
                if (Common.Utils.LocalEnv.GrLayoutSetting.QuotesShowMode == 1)
                    return u.FindResource("FullBidQuotesModel") as DataTemplate;
                else
                    return u.FindResource("SimpleBidQuotesModel") as DataTemplate;
            }

            return u.FindResource("FullBidQuotesModel") as DataTemplate;

        }
    }
}

using STC.Common.ViewModel.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace BDB.Common.View.UI
{
    public class MsgAlertSettingSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var u = container as FrameworkElement;

            if (item == null)
                return u.FindResource("msgItem") as DataTemplate;

            if (item != null)
            {
                MsgAlertDataModel message = item as MsgAlertDataModel;

                if (message.subList[0].isgroup == true)
                    return u.FindResource("msgGroup") as DataTemplate;
                else
                    return u.FindResource("msgItem") as DataTemplate;
            }

            return u.FindResource("msgItem") as DataTemplate;

        }
    }
}

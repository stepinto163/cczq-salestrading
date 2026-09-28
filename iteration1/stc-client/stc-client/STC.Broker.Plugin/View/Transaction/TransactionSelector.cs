using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Broker.Plugin.View.Transaction
{
    public class TransactionSelector: DataTemplateSelector
    {
        public DataTemplate ListFixDataTemplate { get; set; }
        public DataTemplate ListDataTemplate { get; set; }
        public DataTemplate CardDataemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var u = container as FrameworkElement;

            if (item == null)
            {
                return ListDataTemplate;
            }
            else {
                if (Common.Utils.LocalEnv.TransactionModel == "1")
                    return CardDataemplate;
                else if (Common.Utils.LocalEnv.TransactionModel == "2")
                {
                    return ListFixDataTemplate;
                }
                else {
                    return ListDataTemplate;
                }
                   
            }
        }
    }
}

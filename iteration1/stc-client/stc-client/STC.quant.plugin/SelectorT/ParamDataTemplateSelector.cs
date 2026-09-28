using STC.quant.plugin.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.quant.plugin.SelectorT
{
    public class ParamDataTemplateSelector : DataTemplateSelector
    {
        //public DataTemplate CheckBoxItem { get; set; }

        //public DataTemplate LowTemplate { get; set; }
        //public DataTemplate dt = new DataTemplate();
        // public FrameworkElementFactory label = new FrameworkElementFactory(typeof(Label));


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (container == null)
            {
                return (DataTemplate)((FrameworkElement)container).FindResource("lableItem");
            }
            if (item != null && item is DataRowView)
            {
                DataRowView dr = (DataRowView)item;
                string paratype = "";
                if (dr.Row.RowState != DataRowState.Deleted)
                    paratype = dr["ParamType"] + "";
                else
                    return (DataTemplate)((FrameworkElement)container).FindResource("lableItem");
                if (paratype.Equals(ParaType.CheckBoxItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("CheckBoxItem");
                if (paratype.Equals(ParaType.MultiSelectItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("MultiSelectItem");
                else if (paratype.Equals(ParaType.ComboBoxItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("ComboBoxItem");
                else if (paratype.Equals(ParaType.TextItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("TextItem");
                else if (paratype.Equals(ParaType.TextItemMulti.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("TextItemMulti");
                else if (paratype.Equals(ParaType.TextItem1.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("TextItem1");
                else if (paratype.Equals(ParaType.TextItem2.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("TextItem2");
                else if (paratype.Equals(ParaType.TextQtyItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("TextQtyItem");
                else if (paratype.Equals(ParaType.DatePickerItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("DatePickerItem");
                else if (paratype.Equals(ParaType.SearchTextBox.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("SearchTextBox");
                else if (paratype.Equals(ParaType.TextFieldItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("TextFieldItem");
                else if (paratype.Equals(ParaType.TreeViewComBoxItem.ToString()))
                    return (DataTemplate)((FrameworkElement)container).FindResource("TreeViewComBoxItem");
                else
                {
                    return (DataTemplate)((FrameworkElement)container).FindResource("lableItem");
                }
            }
            return (DataTemplate)((FrameworkElement)container).FindResource("lableItem"); ;

        }
    }
}

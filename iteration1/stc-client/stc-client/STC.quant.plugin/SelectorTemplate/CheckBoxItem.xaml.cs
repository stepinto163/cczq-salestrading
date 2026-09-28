using STC.quant.plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.quant.plugin.SelectorTemplate
{
    /// <summary>
    /// CheckBoxItem.xaml 的交互逻辑
    /// </summary>
    public partial class CheckBoxItem : UserControl
    {
        public CheckBoxItem()
        {
            InitializeComponent();
        }

        private void Ddd_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton ck = sender as RadioButton;
                DataRowView row = ck.DataContext as DataRowView;
                if (row == null)
                    return;
                row[MMListBoxViewModel.PARAMISCHECK_FIELD] = ck.IsChecked.Value;
                string paramId = row[MMListBoxViewModel.PARAMID_FIELD] + "";
                string id = row[MMListBoxViewModel.PID_FIELD] + "";
                DataRow[] drs = row.DataView.Table.Select("ParamId =" + paramId);
                foreach (DataRow d in drs)
                {
                    string did = d[MMListBoxViewModel.PID_FIELD] + "";
                    if (ck.IsChecked.Value && !id.Equals(did))
                    {
                        d[MMListBoxViewModel.PARAMISCHECK_FIELD] = false;
                        if (d.Table.Columns.Contains(MMListBoxViewModel.PARAMVALUE_FIELD))
                            d[MMListBoxViewModel.PARAMVALUE_FIELD] = "";
                        if (d.Table.Columns.Contains(MMListBoxViewModel.PARAMVALUE1_FIELD))
                            d[MMListBoxViewModel.PARAMVALUE1_FIELD] = "";
                        if (d.Table.Columns.Contains(MMListBoxViewModel.PARAMVALUE2_FIELD))
                            d[MMListBoxViewModel.PARAMVALUE2_FIELD] = "";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void Ddd_Unchecked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            DataRowView d = ck.DataContext as DataRowView;
            if (d == null)
                return;
            d[MMListBoxViewModel.PARAMISCHECK_FIELD] = ck.IsChecked.Value;
            if (d.Row.Table.Columns.Contains(MMListBoxViewModel.PARAMVALUE_FIELD))
                d[MMListBoxViewModel.PARAMVALUE_FIELD] = "";
            if (d.Row.Table.Columns.Contains(MMListBoxViewModel.PARAMVALUE1_FIELD))
                d[MMListBoxViewModel.PARAMVALUE1_FIELD] = "";
            if (d.Row.Table.Columns.Contains(MMListBoxViewModel.PARAMVALUE2_FIELD))
                d[MMListBoxViewModel.PARAMVALUE2_FIELD] = "";
        }
    }
}

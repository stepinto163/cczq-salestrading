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
    /// MultiSelectItem.xaml 的交互逻辑
    /// </summary>
    public partial class MultiSelectItem : UserControl
    {
        public MultiSelectItem()
        {
            InitializeComponent();
        }
        private void Ddd_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox ck = sender as CheckBox;
                DataRowView row = ck.DataContext as DataRowView;
                if (row == null)
                    return;
                string paramId = row[MMListBoxViewModel.PARAMID_FIELD] + "";
                string id = row[MMListBoxViewModel.PID_FIELD] + "";
                DataRow[] drs = row.DataView.Table.Select("ParamId =" + paramId);
                foreach (DataRow d in drs)
                {
                    string did = d[MMListBoxViewModel.PID_FIELD] + "";
                    if (id.Equals(did))
                    {
                        d[MMListBoxViewModel.PARAMISCHECK_FIELD] = ck.IsChecked.Value;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

       
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// RadioItem.xaml 的交互逻辑
    /// </summary>
    public partial class DatePickerItem : UserControl
    {
        public DatePickerItem()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //TextBox tb = sender as TextBox;
            //Regex regex = new Regex("@^((20|21|22|23|[0-1]?\\d):[0-5]?\\d:[0-5]?\\d)$");//("^([01]\\d?|2[0-4]):[0-5]\\d(:[0-5]\\d)?$");
            //e.Handled = !regex.IsMatch(e.Text);

            //if (e.Handled)
            //    return;
        }
    }
}

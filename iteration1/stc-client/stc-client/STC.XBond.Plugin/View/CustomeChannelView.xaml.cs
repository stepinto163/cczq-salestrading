using STC.XBond.Plugin.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// CustomeChannelView.xaml 的交互逻辑
    /// </summary>
    public partial class CustomeChannelView : UserControl
    {
        private bool isInit = true;
        public event Action OnPropertyChange;

        public CustomeChannelView()
        {
            InitializeComponent();

            this.Loaded += CustomeChannelView_Loaded;
        }

        private void CustomeChannelView_Loaded(object sender, RoutedEventArgs e)
        {
            isInit = false;
        }
        
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9.-]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string maxAmount = tb.Tag as string;

            int max = 1000;

            int tem = 0;
            if (Int32.TryParse(maxAmount, out tem))
                max = tem;

            int min = -1000;
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;

            if (selectTxt.Length == text.Length)
                return;

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);

            decimal d = 0;
            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8)
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        

        private void cbDefault_Checked(object sender, RoutedEventArgs e)
        {
            if (cbDefault.IsChecked.Value)
            {
                cbDirection.IsChecked = cbNotOut.IsChecked = false;
                //lbDirect.IsEnabled = false;

                tbRandomNum.IsEnabled = true;
                SetListBoxItemEnable(lbSecondLevel, true);
                SetListBoxItemEnable(lbFirstLevel, true);
                SetListBoxItemEnable(lbDirect, false);

                lbDirect.SelectedIndex = lbSecondLevel.SelectedIndex = lbFirstLevel.SelectedIndex = -1;

                if (!isInit && !string.IsNullOrEmpty(tbRandomNum.Text))
                    OnPropertyChange?.Invoke();
            }
        }

        private void cbDirection_Checked(object sender, RoutedEventArgs e)
        {
            if (cbDirection.IsChecked.Value)
            {
                cbDefault.IsChecked = cbNotOut.IsChecked = false;
                lbSecondLevel.SelectedIndex = lbFirstLevel.SelectedIndex = -1;
                //tbRandomNum.IsEnabled = lbSecondLevel.IsEnabled = lbFirstLevel.IsEnabled = false;

                SetListBoxItemEnable(lbSecondLevel, false);
                SetListBoxItemEnable(lbFirstLevel, false);
                SetListBoxItemEnable(lbDirect, true);

                lbDirect.SelectedIndex = 0;

                if (!isInit)
                    OnPropertyChange?.Invoke();
            }
        }

        private void cbNotOut_Checked(object sender, RoutedEventArgs e)
        {
            if (cbNotOut.IsChecked.Value)
            {
                cbDefault.IsChecked = cbDirection.IsChecked = false;
                lbSecondLevel.SelectedIndex = lbDirect.SelectedIndex = lbFirstLevel.SelectedIndex = -1;
                //tbRandomNum.IsEnabled = false;
                SetListBoxItemEnable(lbSecondLevel, false);
                SetListBoxItemEnable(lbFirstLevel, false);
                SetListBoxItemEnable(lbDirect, false);

                if (!isInit)
                    OnPropertyChange?.Invoke();
            }
        }

        private void SetListBoxItemEnable(ListBox listBox, bool status)
        {
            //foreach (ListBoxItemData item in listBox.Items)
            //{
            //    item.ItemEnable = status;
            //}
        }

        private void tbRandomNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbDefault.IsChecked.Value)
            {
                if (!isInit)
                    OnPropertyChange?.Invoke();
            }
        }
    }
}

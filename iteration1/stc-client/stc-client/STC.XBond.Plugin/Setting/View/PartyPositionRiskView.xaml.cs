using STC.XBond.Plugin.Setting.ViewModel;
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

namespace STC.XBond.Plugin.Setting.View
{
    /// <summary>
    /// PartyPositionRiskView.xaml 的交互逻辑
    /// </summary>
    public partial class PartyPositionRiskView : UserControl
    {
        private PartyPositionRiskViewModel _viewModel = new PartyPositionRiskViewModel();
        public PartyPositionRiskView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this._viewModel.init();
        }

        private void SearchAccount_TextBox_ValueChanged(object sender, RoutedEventArgs e)
        {
            this._viewModel.setAccount();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _viewModel;
        }

        private void Qty_TB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string maxAmount = tb.Tag as string;

            int max = 100000000;

            int tem = 0;
            if (Int32.TryParse(maxAmount, out tem))
                max = tem;

            int min = 0;
            string text = tb.Text;

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
        }
    }
}

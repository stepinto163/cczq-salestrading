using STC.Broker.Plugin.ViewModel.Setting;
using STC.Common.View.Component;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// TraderMangerWin.xaml 的交互逻辑
    /// </summary>
    public partial class TraderMangerWin : WindowBase
    {
        private TraderMangerWinViewModel viewModel;

        private bool textChangeFlag = false;


        private string currentTxt = "";

        private TextBox textBox;

        public bool isChange = true;

        public TraderMangerWin(long id, string instId)
        {
            InitializeComponent();
            viewModel = new TraderMangerWinViewModel(id, instId, this);
            DataContext = viewModel;
        }

        /// <summary>
        /// 点击交易所联系人，弹窗
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExchangeContactClick(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;

                dgInstResult_MouseLeftButtonUp(null, null);
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgInstResult.SelectedIndex > 0)
                    dgInstResult.SelectedIndex--;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgInstResult.SelectedIndex < dgInstResult.Items.Count - 1)
                    dgInstResult.SelectedIndex++;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
            else if (e.Key == Key.Back)
            {
                textChangeFlag = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!isChange)
            {
                return;
            }

            if (!textChangeFlag)
            {
                return;
            }
            textBox = sender as TextBox;
            currentTxt = textBox.Text.Trim();
            if (textBox == null)
            {
                return;
            }
            List<InstTraderViewModel> instTraderViews = viewModel.InstTraderViewModels.ToList();

            if (!string.IsNullOrEmpty(textBox.Text) && textChangeFlag && instTraderViews != null && instTraderViews.Count > 0)
            {
                string param = textBox.Text.Replace("'", "");
                tbInstPop.PlacementTarget = textBox;
                dgInstResult.DataContext = instTraderViews.Where(_ => _.Contact.Contains(param)).ToList();
                dgInstResult.SelectedIndex = 0;
                tbInstPop.IsOpen = true;
            }
            else
            {
                tbInstPop.IsOpen = false;
            }
        }

        private void tbInstPop_LostFocus(object sender, RoutedEventArgs e)
        {
            tbInstPop.IsOpen = false;
        }
        private void tbInstPInput(object sender, TextCompositionEventArgs e)
        {
            textChangeFlag = true;
            isChange = true;
        }

        private void tbInstPInputUpdate(object sender, TextCompositionEventArgs e)
        {
            textChangeFlag = false;
        }

        private async void dgInstResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dv = dgInstResult.SelectedItem as InstTraderViewModel;
            if (dv != null && !string.IsNullOrWhiteSpace(currentTxt) && textBox != null && textBox.DataContext != null)
            {
                InstTraderViewModel instTraderView = textBox.DataContext as InstTraderViewModel;
                if (instTraderView == null)
                {
                    return;
                }
                instTraderView.Issuer = dv.Contact;
                instTraderView.IssuerId = dv.ContactId;
                currentTxt = dv.Contact;
                tbInstPop.IsOpen = false;

                var response = await viewModel.UpdateTrader(instTraderView);
                if (response != null && response.IsSuccess())
                {
                    //this.Close();
                    await viewModel.InitData();
                    textChangeFlag = false;
                }
                else
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    warn.Owner = this;
                    if (response == null || string.IsNullOrEmpty(response.Msg))
                    {
                        warn.setWarnMsg("");
                    }
                    else
                    {
                        warn.setWarnMsg(response.Msg);
                    }
                    
                    warn.ShowDialog();
                }
            }
        }

        private void btnEditImage_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            InstTraderViewModel instTraderViewModel = button.DataContext as InstTraderViewModel;
            if (instTraderViewModel != null)
            {
                List<InstTraderViewModel> instTraderViews = viewModel.InstTraderViewModels.ToList();
                EditContactsWin traderMangerWin = new EditContactsWin(instTraderViewModel, instTraderViews);
                traderMangerWin.ReflachAction += TraderMangerWin_ReflachAction;
                traderMangerWin.ShowDialog();
            }
        }

        private async void TraderMangerWin_ReflachAction(bool obj)
        {
            isChange = false;
            await viewModel.InitData();
        }

        private bool contactTextChangeFlag = false;
        private void tbContact_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            contactTextChangeFlag = true;
        }

        private void tbContact_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            contactTextChangeFlag = false;
        }

        private void tbContact_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dgContactResult_MouseLeftButtonUp(null, null);
            }
        }

        private void tbContact_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgContactResult.SelectedIndex > 0)
                    dgContactResult.SelectedIndex--;
                dgContactResult.ScrollIntoView(dgContactResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgContactResult.SelectedIndex < dgContactResult.Items.Count - 1)
                    dgContactResult.SelectedIndex++;
                dgContactResult.ScrollIntoView(dgContactResult.SelectedItem);
            }
        }

        private void tbContact_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbContact.Text) && contactTextChangeFlag)
            {
                string param = tbContact.Text.Replace("'", "");
                dgContactResult.DataContext = viewModel.InstTraderViewModels.Where(_ => _.Contact.Contains(param));
                dgContactResult.SelectedIndex = 0;
                tbContactPop.IsOpen = true;
            }
            else
            {
                tbContactPop.IsOpen = false;
            }
        }

        private void tbContact_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbContact.Text))
            {
                tbContactPop.IsOpen = false;
            }
        }

        private void tbContactPop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbContact.Text))
            {
                tbContactPop.IsOpen = false;
            }
        }

        private void dgContactResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var contact = dgContactResult.SelectedItem as InstTraderViewModel;
            if (contact != null && !string.IsNullOrWhiteSpace(this.tbContact.Text))
            {
                tbContact.Text = contact.Contact;
                tbContactPop.IsOpen = false;
                //定位
                int index = TraderDataGrid.Items.IndexOf(viewModel.InstTraderViewModels.Where(_ => _.ContactId == contact.ContactId).FirstOrDefault());
                if (index >= 0)
                {
                    TraderDataGrid.SelectedIndex = index;
                    TraderDataGrid.ScrollIntoView(viewModel.InstTraderViewModels.Where(_ => _.ContactId == contact.ContactId).FirstOrDefault());
                }
            }
        }
    }
}

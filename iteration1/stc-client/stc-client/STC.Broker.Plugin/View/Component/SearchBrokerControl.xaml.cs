using STC.Broker.Plugin.Utils;
using STC.Common.Model.CRM;
using STC.Common.Utils;
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
using CommonUtils = STC.Broker.Plugin.Utils.CommonUtils;

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// SearchBrokerControl.xaml 的交互逻辑
    /// </summary>
    public partial class SearchBrokerControl : UserControl
    {

        public event Action<BrokerInfoPo,object> AfterSearch;
        //public event Action<object, BondsInfoPo> AfterSearchData;
        private bool TextChangeFlag = true;
        private bool IsInit = true;
        /// <summary>
        /// 债券类型
        /// </summary>
        public string SecurityType { get; set; }
        public SearchBrokerControl()
        {
            InitializeComponent();
        }

        public void SetSearchTxt(string txt)
        {
            IsInit = true;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(() => tbBrokerAChange.Focus()));

            tbBrokerAChange.Text = txt;
            tbBrokerAChange.SelectAll();

            IsInit = false;
        }

        private void tbBrokerAChange_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgBrokerA.SelectedIndex > 0)
                    dgBrokerA.SelectedIndex--;
                dgBrokerA.ScrollIntoView(dgBrokerA.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgBrokerA.SelectedIndex < dgBrokerA.Items.Count - 1)
                    dgBrokerA.SelectedIndex++;
                dgBrokerA.ScrollIntoView(dgBrokerA.SelectedItem);
            }

        }

        private void tbBrokerAChange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dgBrokerA_MouseUp(null, null);
            }

        }

        private void tbBrokerAChange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TextChangeFlag || IsInit)
                return;
            AppUtils.IsInnerInput = true;
           if (!string.IsNullOrEmpty(tbBrokerAChange.Text))
            {
                string txt = tbBrokerAChange.Text.Replace("'", "");

                List<BrokerInfoPo> result = new List<BrokerInfoPo>();
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    result = CommonUtils.GetBrokerInfoPo(txt, SecurityType, 20);
                }
                if (result != null && result.Count > 0)
                {
                    if (!popBrokerA.IsOpen)
                        popBrokerA.IsOpen = true;
                    dgBrokerA.DataContext = result;
                    dgBrokerA.SelectedIndex = 0;
                }
            }
            else
            {
                dgBrokerA.DataContext = null;
                popBrokerA.IsOpen = false;
            }
        }

        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = true;
        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = false;
        }

        private void dgBrokerA_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgBrokerA.SelectedItem != null)
            {
                var dv = dgBrokerA.SelectedItem as BrokerInfoPo;
                tbBrokerAChange.Text = dv.AccountName;
                popBrokerA.IsOpen = false;
                dgBrokerA.SelectedIndex = -1;
                AfterSearch?.Invoke(dv,this.DataContext);
            }
            else {
                AfterSearch?.Invoke(null, this.DataContext);
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}

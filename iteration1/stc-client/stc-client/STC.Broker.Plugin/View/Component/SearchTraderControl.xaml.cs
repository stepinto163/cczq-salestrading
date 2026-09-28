using STC.Common.Model.CRM;
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

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// SearchTraderControl.xaml 的交互逻辑
    /// </summary>
    public partial class SearchTraderControl : UserControl
    {

        public event Action<object, object> AfterSearch;
        public object Data;
        private bool TextChangeFlag = false;
        /// <summary>
        /// 债券类型
        /// </summary>
        public string SecurityType { get; set; }
        private bool IsInit = true;

        public SearchTraderControl()
        {
            InitializeComponent();
        }

        public void SetSearchTxt(string txt)
        {
            IsInit = true;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(() => tbTraderAInfoChange.Focus()));

            tbTraderAInfoChange.Text = txt;
            tbTraderAInfoChange.SelectAll();
            //tbSearch.Focus();

            IsInit = false;
        }

        private void tbTraderAInfoChange_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgTraderA.SelectedIndex > 0)
                    dgTraderA.SelectedIndex--;
                dgTraderA.ScrollIntoView(dgTraderA.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgTraderA.SelectedIndex < dgTraderA.Items.Count - 1)
                    dgTraderA.SelectedIndex++;
                dgTraderA.ScrollIntoView(dgTraderA.SelectedItem);
            }
        }

        private void tbTraderAInfoChange_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                dgTraderA_MouseUp(null, null);
            }
        }

        private void tbTraderAInfoChange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TextChangeFlag || IsInit)
                return;

            if (!string.IsNullOrEmpty(tbTraderAInfoChange.Text))
            {

                List<DataGridDataPo> dataList = Utils.CommonUtils.GetContactByContactName(tbTraderAInfoChange.Text, 100);
                if (dataList == null || dataList.Count <= 0)
                {

                    dataList = Utils.CommonUtils.GetContactByInstName(tbTraderAInfoChange.Text, 100, SecurityType);
                }
                if (dataList.Count > 0)
                {
                    dgTraderA.DataContext = dataList;
                    if (!popTraderA.IsOpen)
                        popTraderA.IsOpen = true;
                    dgTraderA.SelectedIndex = 0;
                }
                else
                {
                    dgTraderA.DataContext = null;
                    popTraderA.IsOpen = false;
                }
            }
            else
            {
                dgTraderA.DataContext = null;
                popTraderA.IsOpen = false;
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

        private void dgTraderA_MouseUp(object sender, MouseButtonEventArgs e)
        {
           
            var dv = dgTraderA.SelectedItem as DataGridDataPo;
            popTraderA.IsOpen = false;
            dgTraderA.SelectedIndex = -1;
            AfterSearch?.Invoke(Data, dv);
        }
    }
}

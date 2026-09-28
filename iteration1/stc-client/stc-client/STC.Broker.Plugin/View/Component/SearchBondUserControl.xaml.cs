using STC.Common.Model.Base;
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

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// SearchBondUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class SearchBondUserControl : UserControl
    {
        public event Action<SimpleBondsInfoPo> AfterSearch;
        public event Action<SimpleBondsInfoPo, object> AfterSearchData;
        public object Data;
        private bool TextChangeFlag = true;
        private bool IsInit = true;
        public SearchBondUserControl()
        {
            InitializeComponent();
        }


        public void SetButtonVisible(Visibility status)
        {
            object obj = tbSearch.Template.FindName("btnDelete", tbSearch);
            if (obj != null)
            {
                Button button = obj as Button;
                if (button != null)
                    button.Visibility = status;
            }
        }

        public void SetSearchTxt(string txt)
        {
            IsInit = true;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(() => tbSearch.Focus()));

            tbSearch.Text = txt;
            tbSearch.SelectAll();
            //tbSearch.Focus();

            IsInit = false;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TextChangeFlag)
                return;

            if (IsInit)
                return;

            if (!popBondCode.IsOpen)
                popBondCode.IsOpen = true;

            if (!string.IsNullOrEmpty(tbSearch.Text))
            {
                List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
                string txt = tbSearch.Text.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<SimpleBondsInfoPo> selectDatas = CommonUtils.GetBondsInfoPo(str, 20, "");
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = CommonUtils.GetBondGridData(tbSearch.Text.Trim(), 20);
                }
                dgBondCode.DataContext = result;
                dgBondCode.SelectedIndex = 0;
            }
        }

        private void tbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgBondCode.SelectedIndex > 0)
                    dgBondCode.SelectedIndex--;
                dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgBondCode.SelectedIndex < dgBondCode.Items.Count - 1)
                    dgBondCode.SelectedIndex++;
                dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Escape)
            {
                tbSearch.Text = string.Empty;
            }
        }

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dgBondCode_MouseDoubleClick(null, null);
            }
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            dgBondCode_MouseDoubleClick(null, null);
        }

        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = true;
        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = false;
        }

        private void dgBondCode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SimpleBondsInfoPo dv = dgBondCode.SelectedItem as SimpleBondsInfoPo;
            dgBondCode.SelectedIndex = -1;
            popBondCode.IsOpen = false;
            AfterSearch?.Invoke(dv);
            AfterSearchData?.Invoke(dv,Data);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsInit = true;
            tbSearch.Text = string.Empty;
            tbSearch.Focus();
            IsInit = false;
        }
    }
}

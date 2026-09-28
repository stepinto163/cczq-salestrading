using STC.Position.Plugin.View.Chart;
using STC.Position.Plugin.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// RiskExposureView.xaml 的交互逻辑
    /// </summary>
    public partial class RiskExposureView : UserControl
    {
        public RiskExposureViewModel riskviewmodel;

        public RiskExposureView()
        {
            InitializeComponent();
           
            riskviewmodel = new RiskExposureViewModel();
            this.DataContext = riskviewmodel;
        }

        public void TryDispose()
        {
            (chart as RiskExposureChartView).TryDispose();
            (this.DataContext as RiskExposureViewModel).TryDispose();
        }


        private void SearchTextBox_AfterSearchBond(Common.Model.Base.SimpleBondsInfoPo arg1, object arg2)
        {

        }

        private void SettingSearchTextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj != null)
            {
                (this.DataContext as RiskExposureViewModel).RefreshSecurityid(obj.Bond_ID, obj.Short_Name, obj.RemaingingTime);
            }
        }

        private void TreeView_NodeList_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            string bookid = "";
            string bookname = "";
            bookid = ((sender as Common.View.Component.MultiTreeComboBox).SelectedItem??"" ).ToString();
            bookname = ((sender as Common.View.Component.MultiTreeComboBox).SelectedTexts ?? "").ToString();
            (this.DataContext as RiskExposureViewModel).RefreshBookid(bookid, bookname);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((tBox.Text.Length > 200) && (e.Key != Key.Delete) && (e.Key != Key.Back))
            {
                e.Handled = true;
                return;
            }
        }

        public void Dispose()
        {
            (riskviewmodel as RiskExposureViewModel).Dispose();
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            CheckBox ch = sender as CheckBox; 
            if (ch.IsChecked.Value == true)
            {
                DataRow[] drlit2 = riskviewmodel.GlobalView.Select($"ischecked = '1'");
                if (drlit2.Length == riskviewmodel.GlobalView.Rows.Count)
                {
                    riskviewmodel.IsAllChecked = true;
                }
                else
                {
                    riskviewmodel.IsAllChecked = false;
                }
                riskviewmodel.ChkJoinSimTrading = false;
                riskviewmodel.ChkJoinSimTrading = true;
            }
            else
            {
                if (riskviewmodel != null)
                {
                    DataRow[] drlit2 = riskviewmodel.GlobalView.Select($"ischecked = '1'");
                    if (drlit2.Length == 0)
                    {
                        riskviewmodel.IsAllChecked = false;
                        riskviewmodel.ChkJoinSimTrading = false;
                    }
                    else
                    {
                        if (drlit2.Length == riskviewmodel.GlobalView.Rows.Count)
                        {
                            riskviewmodel.IsAllChecked = true;
                        }
                        else
                        {
                            riskviewmodel.IsAllChecked = false;
                        }
                        riskviewmodel.ChkJoinSimTrading = false;
                        riskviewmodel.ChkJoinSimTrading = true;
                    }
                }
            }
        }

        private void AllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            riskviewmodel.OnCheckAll();

            CheckBox ch = sender as CheckBox;
            if (ch.IsChecked.Value == true)
            {
                riskviewmodel.ChkJoinSimTrading = true;
            }
            else
            {
                riskviewmodel.ChkJoinSimTrading = false;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ActualHeight != 0)
                chartGrid.Height = new GridLength(gridMain.ActualHeight / 3);
        }
    }
}

using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.WorkTable.Plugin.Command;
using STC.WorkTable.Plugin.ViewModel.Data;
using STC.WorkTable.Plugin.ViewModel.WorkTable;
using System;
using System.Collections;
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

namespace STC.WorkTable.Plugin.View.WorkTable
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl
    {

        private MainViewModel mainViewModel = new MainViewModel();

        public MainView()
        {
            InitializeComponent();
            this.DataContext = mainViewModel;
        }

        private void IntentionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.intentionPanel.UpdateIntentionID(this.mainViewModel.SelectedItem_Worked.IntentionId);
            bool isAll = false;
            foreach (ListBoxItem item in this.historyIntentionList.SelectedItems_HistoryFilter)
            {
                if (item.Content.ToString() == "全部")
                {
                    isAll = true;
                }
            }
            if (isAll == false)
            {
                this.historyIntentionList.SelectedIndex_HistoryFilter = 0;
            }
        }

        private bool _verClose = false;
        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            if (!_verClose)
            {
                intentionGrid.ColumnDefinitions[4].Width = new GridLength(0);
                _verClose = true;

                btnVer.Content = "<";

            }
            else
            {
                intentionGrid.ColumnDefinitions[4].Width = new GridLength(282, GridUnitType.Pixel);
                _verClose = false;

                btnVer.Content = ">";
            }
        }

        private bool _verCloseLeft = false;
        private void btnVerLeft_Click(object sender, RoutedEventArgs e)
        {
            if (!_verCloseLeft)
            {
                intentionGrid.ColumnDefinitions[0].Width = new GridLength(0);
                _verCloseLeft = true;

                btnVerLeft.Content = ">";

            }
            else
            {
                intentionGrid.ColumnDefinitions[0].Width = new GridLength(200, GridUnitType.Pixel);
                _verCloseLeft = false;

                btnVerLeft.Content = "<";
            }
        }

        private bool _verCloseBottom = false;
        private void BtnHorBottom_Click(object sender, RoutedEventArgs e)
        {
            if (!_verCloseBottom)
            {
                intentionGrid.RowDefinitions[2].Height = new GridLength(0);
                _verCloseBottom = true;

                btnHorBottom.Content = "∧";
            }
            else
            {
                intentionGrid.RowDefinitions[2].Height = new GridLength(400, GridUnitType.Pixel);
                _verCloseBottom = false;

                btnHorBottom.Content = "∨";
            }
        }

        /// <summary>
        /// 弹窗命令处理类
        /// </summary>
        public static PopupWindowCommand PopupWindowCommand = new PopupWindowCommand();
        private void TradeInfo_tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TradeInfo_tabControl.SelectedItem != null)
            {
                
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyOrders_TabItem")
                {
                    //我的订单
                    btnHistory.Visibility = Visibility.Visible;
                    this.btnFullScreen.Command = PopupWindowCommand;
                    this.btnFullScreen.CommandParameter = PopupTypeEnum.MyOrders;
                    this.myOrdersPart.IsMyOrdersShowRedDot = "";
                    //_myTraderViewModel.IsMyOrdersShowRedDot = "";
                    //_myTraderViewModel.TradeTabSelectedItem = "MyOrders_TabItem";
                }
                else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyExes_TabItem")
                {
                    //我的成交
                    btnHistory.Visibility = Visibility.Visible;
                    this.btnFullScreen.Command = PopupWindowCommand;
                    this.btnFullScreen.CommandParameter = PopupTypeEnum.MyExecutions;
                    this.myExesPart.IsMyExecutionsShowRedDot = "";
                    //_myTraderViewModel.IsMyExecutionsShowRedDot = "";
                    //_myTraderViewModel.TradeTabSelectedItem = "MyExecutions_TabItem";
                }
                else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "historyIntentionList_TabItem")
                {
                    //历史需求
                    btnHistory.Visibility = Visibility.Collapsed;
                    this.btnFullScreen.Command = PopupWindowCommand;
                    this.btnFullScreen.CommandParameter = new object[] { PopupTypeEnum.HistoryIntentionList, this.historyIntentionList };
                    //_myTraderViewModel.TradeTabSelectedItem = "MyCancels_TabItem";
                }
                string tabItemheader = (this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString();
                this.myOrdersPart.TradeTabSelectedItem = tabItemheader;
                this.myExesPart.TradeTabSelectedItem = tabItemheader;
            }
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            bool isExec = false;
            if (this.TradeInfo_tabControl.SelectedItem != null)
            {
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyExecutions_TabItem")
                {
                    //我的成交
                    mainViewModel.OpenHistoryPage(true);
                    isExec = true;
                }
            }
            if (!isExec)
                mainViewModel.OpenHistoryPage(false);
        }
    }
}

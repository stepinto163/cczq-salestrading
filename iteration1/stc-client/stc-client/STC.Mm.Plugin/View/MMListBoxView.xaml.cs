using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Frame.CustomUI.Component;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Utils.Data;
using STC.Mm.Plugin.View.Component;
using STC.Mm.Plugin.View.HomePage;
using STC.Mm.Plugin.ViewModel;

namespace STC.Mm.Plugin.View
{
    /// <summary>
    /// MMListBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class MMListBoxView : UserControl
    {

        public MMListBoxView()
        {
            InitializeComponent();
            QuotePopup = new Popup();
            QuotePopup.MouseLeave += QuotePopup_MouseLeave;

            OfrQuotePopup = new Popup();
            OfrQuotePopup.MouseLeave += OfrQuotePopup_MouseLeave;
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender != null && sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                if (cb != null && cb.DataContext != null && cb.DataContext is DataRowView)
                {
                    DataRowView currentDRV = cb.DataContext as DataRowView;
                    string securityID = CommonUtils.ConvertToString(currentDRV[MMListBoxViewModel.SECURITY_ID_FIELD]);
                    string settltype = CommonUtils.ConvertToString(currentDRV[MMListBoxViewModel.SETTLE_TYPE_FIELD]);
                    bool enable;
                    bool.TryParse(CommonUtils.ConvertToString(currentDRV[MMListBoxViewModel.ENABLE_FIELD]), out enable);

                    MMUpdateEnableReq req = new MMUpdateEnableReq();
                    req.securityid = securityID;
                    req.settltype = settltype;
                    req.enable = enable ? "0" : "1";
                    req.macID = EngineHelper.GetMacByNetworkInterface();
                    req.insideIP = EngineHelper.GetIp();
                    req.hardDiskID = EngineHelper.Getharddiskid2();

                    if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
                    {
                        MMListBoxViewModel mmListBoxViewModel = (this.MMListView.DataContext as MMListBoxViewModel);
                        mmListBoxViewModel.MMUpdateEnable(req);
                    }
                }
            }
        }

        private void del_btn_Click(object sender, RoutedEventArgs e)
        {
            var item = this.MMListView.ItemContainerGenerator.ContainerFromItem((sender as Button).DataContext as DataRowView);
            (item as DataGridRow).IsSelected = true;

            DataRowView item2 = this.MMListView.SelectedItem as DataRowView;
            string securityID = item2["securityID"].ToString();
            string symbol = item2["symbol"].ToString();
            string enable = item2["enable"].ToString();

            if ("True" == enable)
            {
                CustomMessageBox.Warning($"债券\"{securityID}\"未关闭策略，请确认关闭后再删除");
                return;
            }


            string text = $"确定要删除债券\"{securityID}\"吗？";// " " + symbol + "？";
            string title = "删除债券";
            if (CustomMessageBox.Question(text, title) == true)
            {
                if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
                {
                    MMListBoxViewModel mmListBoxViewModel = (this.MMListView.DataContext as MMListBoxViewModel);
                    mmListBoxViewModel.DelBond();
                }
            }
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (this.DataContext != null && this.DataContext is MMListBoxViewModel)
                {
                    (this.DataContext as MMListBoxViewModel).CheckBoxAllValueChange(flag);
                }
            }
        }

        private void OperationGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void MMListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void SetMyOrderOperationGridMargin()
        {
            //PART_HorizontalScrollBar
            ScrollBar hScrollBar = CommonUtils.GetVisualChild<ScrollBar>(MMListView, dp => dp.Name == "PART_HorizontalScrollBar" && dp.Tag != null);

            if (hScrollBar != null)
            {
                if (hScrollBar.Visibility == Visibility.Visible)
                {
                    this.OperationGrid.Margin = new Thickness(0, 0, 0, 10);
                }
                else
                {
                    this.OperationGrid.Margin = new Thickness(0);
                }
            }
        }

        private void LogBtn_Click(object sender, RoutedEventArgs e)
        {
            MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;

            if (mmListBoxViewModel != null && mmListBoxViewModel.SelectedItem != null)
            {
                string securityId = mmListBoxViewModel.SelectedItem[MMListBoxViewModel.SECURITY_ID_FIELD].ConvertToString();
                string groupId = mmListBoxViewModel.groupID;
                string key = $"MMLogsView_{groupId}_{securityId}";
                if (!WindowManager.Instance.IsExist(key))
                {
                    MMLogsWindow mmLogsWindow = new MMLogsWindow();
                    mmLogsWindow.SetCurrentMM(mmListBoxViewModel.SelectedItem);
                    WindowManager.Instance.OpenWindow(key, mmLogsWindow);
                }
                else
                {
                    MMLogsWindow existWin = WindowManager.Instance.GetExistWin(key) as MMLogsWindow;
                    existWin.SetCurrentMM(mmListBoxViewModel.SelectedItem);
                }
            }
        }

        private void enableGroupChk_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            MMListBoxViewModel vm = cb.DataContext as MMListBoxViewModel;

            string enable = cb.IsChecked.Value ? "0" : "1";
            PResult result = MMHandler.Instance.HandleUnifyGroup(vm.groupID, enable);

            e.Handled = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            MMListBoxViewModel vm = cb.DataContext as MMListBoxViewModel;

            if (vm == null)
                return;

            if (vm.IsUnify == false)
                return;

            if (string.IsNullOrEmpty(vm.ModeType))
                return;

            if (!vm.isUpdateModeType())
                return;

            if (vm.Enable_Unify)
            {
                CustomMessageBox.Warning("当前分组内存在未关闭策略，请确认关闭后再切换策略类型");
                return;
            }
            vm.UpdateInf2();
        }

        private void GroupParamsBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            MMListBoxViewModel vm = btn.DataContext as MMListBoxViewModel;
            if (vm.Enable_Unify)
            {
                CustomMessageBox.Warning("当前分组内存在未关闭策略，请确认关闭后再修改策略参数");
                return;
            }
            vm.OpenGroupDetailView();
        }
        private Popup QuotePopup;
        private Popup OfrQuotePopup;
        private void DataGridCell_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseDepthPop();
        }


        private void QuotePopup_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseDepthPop();
        }

        private void CloseDepthPop()
        {
            this.QuotePopup.IsOpen = false;
            MyQuotePopupView quotePopupView = this.QuotePopup.Child as MyQuotePopupView;
            if (quotePopupView != null && quotePopupView.DataContext is MyQuotePopupViewModel)
            {
                MyQuotePopupViewModel viewModel = quotePopupView.DataContext as MyQuotePopupViewModel;
                viewModel.Data = null;
            }
        }



        private void OfrQuotePopup_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseOfrDepthPop();
        }
        private void CloseOfrDepthPop()
        {
            this.OfrQuotePopup.IsOpen = false;
            MyQuotePopupView quotePopupView = this.OfrQuotePopup.Child as MyQuotePopupView;
            if (quotePopupView != null && quotePopupView.DataContext is MyQuotePopupViewModel)
            {
                MyQuotePopupViewModel viewModel = quotePopupView.DataContext as MyQuotePopupViewModel;
                viewModel.Data = null;
            }
        }
        private void DataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            this.QuotePopup.IsOpen = false;
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "我的Bid")
            {
                this.QuotePopup.PlacementTarget = currentDgCell;
                this.QuotePopup.Placement = PlacementMode.Bottom;
                this.QuotePopup.HorizontalOffset = 0;
                DataRowView currentItem = currentDgCell.DataContext as DataRowView;
                DataRowView vm = currentDgCell.DataContext as DataRowView;
                string securityID = CommonUtils.ConvertToString(currentItem[MMListBoxViewModel.SECURITY_ID_FIELD]);
                string info5 = CommonUtils.ConvertToString(currentItem[MMListBoxViewModel.INF5_FIELD]);
                bool enable = (bool)currentItem[MMListBoxViewModel.ENABLE_FIELD] ? true : false;
                OrderParamProp mmParams = currentItem[MMListBoxViewModel.PARAMS_FIELD] as OrderParamProp;
                if (true == enable && (mmParams.bidQuoteModel.Equals("Defend") || mmParams.bidQuoteModel.Equals("Algorithm"))
                    && !(string.IsNullOrEmpty(mmParams.bidBasePrice) && string.IsNullOrEmpty(mmParams.ofrBasePrice)))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MyQuotePopupView quotePopup = new MyQuotePopupView();
                        quotePopup.setBidParam(mmParams, info5);
                        this.QuotePopup.Child = quotePopup;
                        this.QuotePopup.IsOpen = true;
                    }));
                }
            }
        }

        private void OfrDataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            this.OfrQuotePopup.IsOpen = false;
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "我的Ofr")
            {
                this.OfrQuotePopup.PlacementTarget = currentDgCell;
                this.OfrQuotePopup.Placement = PlacementMode.Bottom;
                this.OfrQuotePopup.HorizontalOffset = 0;
                DataRowView currentItem = currentDgCell.DataContext as DataRowView;
                DataRowView vm = currentDgCell.DataContext as DataRowView;
                string securityID = CommonUtils.ConvertToString(currentItem[MMListBoxViewModel.SECURITY_ID_FIELD]);
                string info5 = CommonUtils.ConvertToString(currentItem[MMListBoxViewModel.INF5_FIELD]);
                bool enable = (bool)currentItem[MMListBoxViewModel.ENABLE_FIELD] ? true : false;
                OrderParamProp mmParams = currentItem[MMListBoxViewModel.PARAMS_FIELD] as OrderParamProp;
                if (true == enable && (mmParams.ofrQuoteModel.Equals("Defend") || mmParams.ofrQuoteModel.Equals("Algorithm"))
                    && !(string.IsNullOrEmpty(mmParams.bidBasePrice) && string.IsNullOrEmpty(mmParams.ofrBasePrice)))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MyQuotePopupView quotePopup = new MyQuotePopupView();
                        quotePopup.setOfrParam(mmParams, info5);
                        this.OfrQuotePopup.Child = quotePopup;
                        this.OfrQuotePopup.IsOpen = true;
                    }));
                }
            }
        }

        private void OfrDataGridCell_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseOfrDepthPop();
        }

        private string oldBidValue = string.Empty;
        private string oldOfrValue = string.Empty;

        private void bidStrategyMode_btn_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.IsSyncQuoteModel_ToggleButton.IsChecked)
            {
                //同步调节模式
                popBidStrategyList.IsOpen = false;
                popOfrStrategyList.IsOpen = false;
                if (!popAllStrategyList.IsOpen)
                {
                    popAllStrategyList.IsOpen = true;
                    AllStrategyList_dg.SelectedIndex = -1;
                }
            }
            else
            {
                popAllStrategyList.IsOpen = false;
                if (!popBidStrategyList.IsOpen)
                {
                    popBidStrategyList.IsOpen = true;
                    bidStrategyList_dg.SelectedIndex = -1;

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        (Action)(() => { Keyboard.Focus(popBidStrategyList); }));
                }
            }
        }

        private void bidStrategyList_dg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
            {
                MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;

                if (popBidStrategyList.IsOpen)
                {
                    popBidStrategyList.IsOpen = false;
                    if (bidStrategyList_dg.SelectedItem != null)
                    {
                        oldBidValue = mmListBoxViewModel.BidQuoteModel;
                        KeyValuePair<string, string> current = (KeyValuePair<string, string>)bidStrategyList_dg.SelectedItem;

                        //发送消息给后台，切换报价模式
                        if (current.Key != mmListBoxViewModel.BidQuoteModel)
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("bidQuoteModel", current.Key);
                            SendUpdateParamsTopic(dic);

                            //mmListBoxViewModel.BidQuoteModel = current.Key;
                        }
                    }
                }
            }
        }

        private void bidStrategyList_dg_LostFocus(object sender, RoutedEventArgs e)
        {
            if (popBidStrategyList.IsOpen)
            {
                popBidStrategyList.IsOpen = false;
            }
        }

        private void AllStrategyList_dg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
            {
                MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;
                if (popAllStrategyList.IsOpen)
                {
                    popAllStrategyList.IsOpen = false;
                    if (AllStrategyList_dg.SelectedItem != null)
                    {
                        oldBidValue = mmListBoxViewModel.BidQuoteModel;
                        oldOfrValue = mmListBoxViewModel.OfrQuoteModel;
                        int selectedIndex = AllStrategyList_dg.SelectedIndex; //0:手动 1：防守 2：算法 3：接口
                                                                              //Defend(防守) Algorithm(算法) Manual(人工) API("接口")

                        string current = string.Empty;
                        switch (selectedIndex)
                        {
                            case 0:
                                current = "Manual";
                                break;
                            case 1:
                                current = "Defend";
                                break;
                            case 2:
                                current = "Algorithm";
                                break;
                            case 3:
                                current = "API";
                                break;
                        }
                        //发送消息给后台，切换报价模式
                        if (oldBidValue != current || oldOfrValue != current)
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("bidQuoteModel", current);
                            dic.Add("ofrQuoteModel", current);
                            SendUpdateParamsTopic(dic);
                        }
                    }
                }
            }
        }

        private void AllStrategyList_dg_LostFocus(object sender, RoutedEventArgs e)
        {
            if (popAllStrategyList.IsOpen)
            {
                popAllStrategyList.IsOpen = false;
            }
        }

        private void ofrStrategyMode_btn_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.IsSyncQuoteModel_ToggleButton.IsChecked)
            {
                //同步调节模式
                popBidStrategyList.IsOpen = false;
                popOfrStrategyList.IsOpen = false;
                if (!popAllStrategyList.IsOpen)
                {
                    popAllStrategyList.IsOpen = true;
                    AllStrategyList_dg.SelectedIndex = -1;
                }
            }
            else
            {
                if (!popOfrStrategyList.IsOpen)
                {
                    popOfrStrategyList.IsOpen = true;
                    ofrStrategyList_dg.SelectedIndex = -1;

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => { Keyboard.Focus(popOfrStrategyList); }));
                }
            }
        }

        private void ofrStrategyList_dg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
            {
                MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;


                if (popOfrStrategyList.IsOpen)
                {
                    popOfrStrategyList.IsOpen = false;
                    if (ofrStrategyList_dg.SelectedItem != null)
                    {
                        oldOfrValue = mmListBoxViewModel.OfrQuoteModel;
                        KeyValuePair<string, string> current = (KeyValuePair<string, string>)ofrStrategyList_dg.SelectedItem;

                        //发送消息给后台，切换报价模式
                        if (current.Key != mmListBoxViewModel.OfrQuoteModel)
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("ofrQuoteModel", current.Key);
                            SendUpdateParamsTopic(dic);
                            //if (string.IsNullOrEmpty(SendUpdateParamsTopic(dic)))
                            //{
                            //    if (oldOfrValue == "Manual" && current.Key == "API")
                            //    {
                            //        string apiOfrQty = ((this.DataContext as DataRowView)["params"] as OrderParamProp).apiOfrQty;
                            //        if (!string.IsNullOrEmpty(apiOfrQty))
                            //        {
                            //            this.ofrAPIWarn_Img.ToolTip = "接口报量为" + apiOfrQty + "KW,请注意报量变化";
                            //            this.ofrAPIWarn_Img.Visibility = Visibility.Visible;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        this.ofrAPIWarn_Img.ToolTip = string.Empty;
                            //        this.ofrAPIWarn_Img.Visibility = Visibility.Collapsed;
                            //    }
                            //}
                        }
                    }
                }
            }
        }

        private void ofrStrategyList_dg_LostFocus(object sender, RoutedEventArgs e)
        {
            if (popOfrStrategyList.IsOpen)
            {
                popOfrStrategyList.IsOpen = false;
            }
        }

        private void SendUpdateParamsTopic(Dictionary<string, string> paramDic)
        {
            if (paramDic.ContainsKey("bidQuoteModel") == false
                || paramDic["bidQuoteModel"] == "--")
            {
                bool modifyBid = false;
                foreach (string key in paramDic.Keys)
                {
                    if (key.StartsWith("bid"))
                    {
                        modifyBid = true;
                        break;
                    }
                }
                if (modifyBid)
                {
                    paramDic["bidQuoteModel"] = "Manual";//手动
                }
            }
            if (paramDic.ContainsKey("ofrQuoteModel") == false
                || paramDic["ofrQuoteModel"] == "--")
            {
                bool modifyOfr = false;
                foreach (string key in paramDic.Keys)
                {
                    if (key.StartsWith("ofr"))
                    {
                        modifyOfr = true;
                        break;
                    }
                }
                if (modifyOfr)
                {
                    paramDic["ofrQuoteModel"] = "Manual";//手动
                }
            }

            List<MMOrderParamReq> requestList = new List<MMOrderParamReq>();
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
            {
                MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;
                mmListBoxViewModel.BatchOperateBtnEnable = false;
                lock (mmListBoxViewModel.lockObj)
                {
                    foreach (DataRowView drv in mmListBoxViewModel.BondView)
                    {
                        if (!(drv[MMListBoxViewModel.ISCHECKED_FIELD] is System.DBNull)
                                && (bool)drv[MMListBoxViewModel.ISCHECKED_FIELD] == true)
                        {
                            string securityID = CommonUtils.ConvertToString(drv[MMListBoxViewModel.SECURITY_ID_FIELD]);
                            string settltype = CommonUtils.ConvertToString(drv[MMListBoxViewModel.SETTLE_TYPE_FIELD]);
                            OrderParamProp mmParams = drv[MMListBoxViewModel.PARAMS_FIELD] as OrderParamProp;

                            MMOrderParamReq req = new MMOrderParamReq();
                            OrderDetailViewModel.Data2Request(mmParams, req);
                            foreach (var item in paramDic)
                            {
                                string field = item.Key;
                                string newValue = item.Value;

                                switch (field)
                                {
                                    case "bidQuoteModel":
                                        req.bidQuoteModel = newValue;
                                        break;
                                    case "manualBid":
                                        req.manualBid = MMCommonUtils.Format4Digit(newValue);
                                        break;
                                    case "bidBaseQty":
                                        req.bidBaseQty = newValue;
                                        break;
                                    case "bidBasePriority":
                                        req.bidBasePriority = newValue;
                                        break;
                                    case "ofrQuoteModel":
                                        req.ofrQuoteModel = newValue;
                                        break;
                                    case "manualOfr":
                                        req.manualOfr = MMCommonUtils.Format4Digit(newValue);
                                        break;
                                    case "ofrBaseQty":
                                        req.ofrBaseQty = newValue;
                                        break;
                                    case "ofrBasePriority":
                                        req.ofrBasePriority = newValue;
                                        break;
                                    case "baseSettleType":
                                        req.baseSettleType = newValue;
                                        break;
                                }
                            }

                            req = OrderDetailViewModel.CheckSendFieldNull(req);
                            if (paramDic.ContainsKey("manualBid") || paramDic.ContainsKey("manualOfr"))
                            {
                                if (!MMCommonUtils.CheckBaseQtyValid(req))
                                {
                                    continue;
                                }
                            }
                            requestList.Add(req);
                        }
                    }
                }
            }

            //InitCurrentOrderParams();
            string retText = string.Empty;

            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                foreach (MMOrderParamReq req in requestList)
                {
                    PResult result = STC.Common.Handler.Admin.MMHandler.Instance.mmUpdateParams(req);
                    if (result == null || !result.IsSuccess())
                    {
                        LogHandler.Warn($"{req.securityid}批量修改报价模式失败");
                    }
                }

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
                    {
                        MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;
                        mmListBoxViewModel.BatchOperateBtnEnable = true;
                    }
                }));
            });
        }


        private void SetAllButtonIsEnabled(bool isEnabled)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.bidStrategyMode_btn.IsEnabled = isEnabled;
                this.ofrStrategyMode_btn.IsEnabled = isEnabled;
            });
        }

        private void popAllStrategyList_Opened(object sender, EventArgs e)
        {
            var ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                popAllStrategyList.HorizontalOffset = 118;
            }
            else
            {
                popAllStrategyList.HorizontalOffset = 0;
            }
        }

        private int oldSettlTypeValue = -1;
        private void SettlTypeListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
            {
                MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;
                oldSettlTypeValue = mmListBoxViewModel.SettlTypeIndex;
                //KeyValuePair<int, string> current = (KeyValuePair<int, string>)SettlTypeListBox.SelectedItem;

                ////发送消息给后台，切换报价模式
                //if (current.Key != mmListBoxViewModel.SettlTypeIndex)
                //{

                //    //mmListBoxViewModel.BidQuoteModel = current.Key;
                //}
            }
        }

        private void SettlTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectIndex = ((ListBox)sender).SelectedIndex;
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
            {
                MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;
                oldSettlTypeValue = mmListBoxViewModel.LastSettlTypeIndex;
                if (selectIndex != oldSettlTypeValue) 
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    if (selectIndex == 0)
                    {
                        dic.Add("baseSettleType", "1");
                    }
                    else if (selectIndex == 1) 
                    {
                        dic.Add("baseSettleType", "2");
                    }
                    SendUpdateParamsTopic(dic);
                }
                mmListBoxViewModel.LastSettlTypeIndex = mmListBoxViewModel.SettlTypeIndex;
            }
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "债券代码"
               || columnHeader == "债券简称")
            {
                if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
                {
                    MMListBoxViewModel mmListBoxViewModel = this.MMListView.DataContext as MMListBoxViewModel;
                    if (mmListBoxViewModel != null && mmListBoxViewModel.SelectedItem != null)
                    {
                        mmListBoxViewModel.OpenTradeBook(mmListBoxViewModel.SelectedItem[MMListBoxViewModel.SECURITY_ID_FIELD]+"");
                    }
                }
            }
        }
    }
}

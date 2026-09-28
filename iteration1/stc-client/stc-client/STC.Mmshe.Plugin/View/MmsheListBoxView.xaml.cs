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
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MMshe;
using STC.Common.Utils;
using STC.Frame.CustomUI.Component;
using STC.Mmshe.Plugin.Utils;
using STC.Mmshe.Plugin.View.Component;
using STC.Mmshe.Plugin.View.HomePage;
using STC.Mmshe.Plugin.ViewModel;
using STC.MMshe.Plugin.View;

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// MMListBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class MMsheListBoxView : UserControl
    {

        public MMsheListBoxView()
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
                    string securityID = CommonUtils.ConvertToString(currentDRV[MmsheListBoxViewModel.SECURITY_ID_FIELD]);
                    string settltype = CommonUtils.ConvertToString(currentDRV[MmsheListBoxViewModel.SETTLE_TYPE_FIELD]);
                    bool enable;
                    bool.TryParse(CommonUtils.ConvertToString(currentDRV[MmsheListBoxViewModel.ENABLE_FIELD]), out enable);

                    MMsheUpdateEnableReq req = new MMsheUpdateEnableReq();
                    req.securityid = securityID;
                    req.settltype = settltype;
                    req.enable = enable ? "0" : "1";
                    req.venuetypegw = CommonUtils.ConvertToString(currentDRV[MmsheListBoxViewModel.VENUETYPEGW_FIELD]);
                    req.venues = CommonUtils.ConvertToString(currentDRV[MmsheListBoxViewModel.VENUES_FIELD]);

                    if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
                    {
                        MmsheListBoxViewModel MmsheListBoxViewModel = (this.MMListView.DataContext as MmsheListBoxViewModel);
                        MmsheListBoxViewModel.MMUpdateEnable(req);
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
                if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
                {
                    MmsheListBoxViewModel MmsheListBoxViewModel = (this.MMListView.DataContext as MmsheListBoxViewModel);
                    MmsheListBoxViewModel.DelBond();
                }
            }
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (this.DataContext != null && this.DataContext is MmsheListBoxViewModel)
                {
                    (this.DataContext as MmsheListBoxViewModel).CheckBoxAllValueChange(flag);
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
            MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;

            if (MmsheListBoxViewModel != null && MmsheListBoxViewModel.SelectedItem != null)
            {
                string securityId = MmsheListBoxViewModel.SelectedItem[MmsheListBoxViewModel.SECURITY_ID_FIELD].ConvertToString();
                string groupId = MmsheListBoxViewModel.groupID;
                string key = $"MMLogsView_{groupId}_{securityId}";
                if (!WindowManager.Instance.IsExist(key))
                {
                    MMLogsWindow mmLogsWindow = new MMLogsWindow();
                    mmLogsWindow.SetCurrentMM(MmsheListBoxViewModel.SelectedItem);
                    WindowManager.Instance.OpenWindow(key, mmLogsWindow);
                }
                else
                {
                    MMLogsWindow existWin = WindowManager.Instance.GetExistWin(key) as MMLogsWindow;
                    existWin.SetCurrentMM(MmsheListBoxViewModel.SelectedItem);
                }
            }
        }

        private void enableGroupChk_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            MmsheListBoxViewModel vm = cb.DataContext as MmsheListBoxViewModel;
            MMsheUnifyGroupPo unifyGroupPo = new MMsheUnifyGroupPo();
            string enable = cb.IsChecked.Value ? "0" : "1";
            unifyGroupPo.enable = enable;
            unifyGroupPo.groupId = vm.groupID;
            unifyGroupPo.venuetypegw = unifyGroupPo.venues = vm.SelectedGroup.venues;
            PResult result = MMsheHandler.Instance.HandleUnifyGroup(unifyGroupPo);

            e.Handled = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            MmsheListBoxViewModel vm = cb.DataContext as MmsheListBoxViewModel;

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
            MmsheListBoxViewModel vm = btn.DataContext as MmsheListBoxViewModel;
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
                string securityID = CommonUtils.ConvertToString(currentItem[MmsheListBoxViewModel.SECURITY_ID_FIELD]);
                string info5 = CommonUtils.ConvertToString(currentItem[MmsheListBoxViewModel.INF5_FIELD]);
                bool enable = (bool)currentItem[MmsheListBoxViewModel.ENABLE_FIELD] ? true : false;
                OrderParamProp mmParams = currentItem[MmsheListBoxViewModel.PARAMS_FIELD] as OrderParamProp;
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
                string securityID = CommonUtils.ConvertToString(currentItem[MmsheListBoxViewModel.SECURITY_ID_FIELD]);
                string info5 = CommonUtils.ConvertToString(currentItem[MmsheListBoxViewModel.INF5_FIELD]);
                bool enable = (bool)currentItem[MmsheListBoxViewModel.ENABLE_FIELD] ? true : false;
                OrderParamProp mmParams = currentItem[MmsheListBoxViewModel.PARAMS_FIELD] as OrderParamProp;
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
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
            {
                MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;

                if (popBidStrategyList.IsOpen)
                {
                    popBidStrategyList.IsOpen = false;
                    if (bidStrategyList_dg.SelectedItem != null)
                    {
                        oldBidValue = MmsheListBoxViewModel.BidQuoteModel;
                        KeyValuePair<string, string> current = (KeyValuePair<string, string>)bidStrategyList_dg.SelectedItem;

                        //发送消息给后台，切换报价模式
                        if (current.Key != MmsheListBoxViewModel.BidQuoteModel)
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("bidQuoteModel", current.Key);
                            SendUpdateParamsTopic(dic);

                            //MmsheListBoxViewModel.BidQuoteModel = current.Key;
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
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
            {
                MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;
                if (popAllStrategyList.IsOpen)
                {
                    popAllStrategyList.IsOpen = false;
                    if (AllStrategyList_dg.SelectedItem != null)
                    {
                        oldBidValue = MmsheListBoxViewModel.BidQuoteModel;
                        oldOfrValue = MmsheListBoxViewModel.OfrQuoteModel;
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
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
            {
                MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;


                if (popOfrStrategyList.IsOpen)
                {
                    popOfrStrategyList.IsOpen = false;
                    if (ofrStrategyList_dg.SelectedItem != null)
                    {
                        oldOfrValue = MmsheListBoxViewModel.OfrQuoteModel;
                        KeyValuePair<string, string> current = (KeyValuePair<string, string>)ofrStrategyList_dg.SelectedItem;

                        //发送消息给后台，切换报价模式
                        if (current.Key != MmsheListBoxViewModel.OfrQuoteModel)
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

            List<MMsheOrderParamReq> requestList = new List<MMsheOrderParamReq>();
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
            {
                MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;
                MmsheListBoxViewModel.BatchOperateBtnEnable = false;
                lock (MmsheListBoxViewModel.lockObj)
                {
                    foreach (DataRowView drv in MmsheListBoxViewModel.BondView)
                    {
                        if (!(drv[MmsheListBoxViewModel.ISCHECKED_FIELD] is System.DBNull)
                                && (bool)drv[MmsheListBoxViewModel.ISCHECKED_FIELD] == true)
                        {
                            string securityID = CommonUtils.ConvertToString(drv[MmsheListBoxViewModel.SECURITY_ID_FIELD]);
                            string settltype = CommonUtils.ConvertToString(drv[MmsheListBoxViewModel.SETTLE_TYPE_FIELD]);
                            OrderParamProp mmParams = drv[MmsheListBoxViewModel.PARAMS_FIELD] as OrderParamProp;

                            MMsheOrderParamReq req = new MMsheOrderParamReq();
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
                                        req.manualBid = MMsheCommonUtils.Format4Digit(newValue);
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
                                        req.manualOfr = MMsheCommonUtils.Format4Digit(newValue);
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
                                if (!MMsheCommonUtils.CheckBaseQtyValid(req))
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
                foreach (MMsheOrderParamReq req in requestList)
                {
                    PResult result = STC.Common.Handler.Admin.MMsheHandler.Instance.mmUpdateParams(req);
                    if (result == null || !result.IsSuccess())
                    {
                        LogHandler.Warn($"{req.securityid}批量修改报价模式失败");
                    }
                }

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
                    {
                        MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;
                        MmsheListBoxViewModel.BatchOperateBtnEnable = true;
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
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
            {
                MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;
                oldSettlTypeValue = MmsheListBoxViewModel.SettlTypeIndex;
                //KeyValuePair<int, string> current = (KeyValuePair<int, string>)SettlTypeListBox.SelectedItem;

                ////发送消息给后台，切换报价模式
                //if (current.Key != MmsheListBoxViewModel.SettlTypeIndex)
                //{

                //    //MmsheListBoxViewModel.BidQuoteModel = current.Key;
                //}
            }
        }

        private void SettlTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectIndex = ((ListBox)sender).SelectedIndex;
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
            {
                MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;
                oldSettlTypeValue = MmsheListBoxViewModel.LastSettlTypeIndex;
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
                MmsheListBoxViewModel.LastSettlTypeIndex = MmsheListBoxViewModel.SettlTypeIndex;
            }
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "债券代码"
               || columnHeader == "债券简称")
            {
                if (this.MMListView.DataContext != null && this.MMListView.DataContext is MmsheListBoxViewModel)
                {
                    MmsheListBoxViewModel MmsheListBoxViewModel = this.MMListView.DataContext as MmsheListBoxViewModel;
                    if (MmsheListBoxViewModel != null && MmsheListBoxViewModel.SelectedItem != null)
                    {
                        MmsheListBoxViewModel.OpenTradeBook(MmsheListBoxViewModel.SelectedItem[MmsheListBoxViewModel.SECURITY_ID_FIELD] + "");
                    }
                }
            }
        }
    }
}

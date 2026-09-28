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
using System.Data;
using System.Text.RegularExpressions;
using STC.Mmshe.Plugin.View;
using STC.Mmshe.Plugin.ViewModel;
using STC.Common.Model.MMshe;
using STC.Mmshe.Plugin.Utils;
using STC.Common.Model.Base;
using STC.Mmshe.Plugin.Convertor;
using STC.Common.Model.Fix.Message;
using STC.Common.View.UI;
using System.Windows.Threading;
using STC.Common.Utils;
using STC.Mmshe.Plugin.Utils.Data;
using STC.Frame.CustomUI;
using System.Windows.Controls.Primitives;

namespace STC.Mmshe.Plugin.View.Component
{
    /// <summary>
    /// MMsheOrderControl.xaml 的交互逻辑
    /// </summary>
    public partial class MMsheOrderControl : UserControl
    {
        private const decimal SpreadRate = 100m;
        public static event Action<string> IsSyncCheckedValueChange;
        private MMOrderControlModel vm = new MMOrderControlModel();

        public MMsheOrderControl()
        {
            InitializeComponent();
            this.DataContext = vm;

            this.bidAPIWarn_Img.Visibility = Visibility.Collapsed;
            this.ofrAPIWarn_Img.Visibility = Visibility.Collapsed;

            //初始化DataContext
            //DataTable dt = new DataTable();
            //dt.Columns.Add("enable", typeof(bool));
            //dt.Columns.Add("securityID", typeof(string));
            //dt.Columns.Add("symbol", typeof(string));
            //dt.Columns.Add("Params", typeof(OrderParamProp));
            //dt.Columns.Add("LastOperation", typeof(string));
            //dt.Columns.Add("LastOperationTime", typeof(string));
            //DataRow row = dt.NewRow();
            //dt.Rows.Add(row);
            //this.DataContext = dt.DefaultView[0];

            //Tab_DgPrice.SelectedIndex = 0;
            //InitPriceView(this.securityID);
            //this.Tab_DgPrice.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.Tab_DgPrice_SelectionChanged);

            IsSyncCheckedValueChange += MMOrderControl_IsSyncCheckedValueChange;

            IsSyncQuoteModel_ToggleButton.Click += IsSyncQuoteModel_ToggleButton_Click; ;
            //this.IsSyncQuoteModel_ToggleButton.IsChecked = false;
        }

        private void IsSyncQuoteModel_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = sender as ToggleButton;
            MMsheCommonUtils.SaveOrderControlIsSyncQuoteModel(securityID, (bool)btn.IsChecked ? "1" : "0");
            IsSyncCheckedValueChange?.Invoke(securityID);
        }

        private void MMOrderControl_IsSyncCheckedValueChange(string securityID2)
        {
            if (securityID == securityID2)
            {
                this.IsSyncQuoteModel_ToggleButton.IsChecked = MMsheCommonUtils.ReadOrderControlIsSyncQuoteModel(securityID2);
            }
        }

        //private OrderParamProp vm.Params = null;

        private string oldBidValue = string.Empty;
        private string oldOfrValue = string.Empty;
        private string _oldSettleType = string.Empty;
        private string _oldSecuityID = "Tradebook";

        private string _securityID = "Tradebook";

        public string securityID
        {
            get { return _securityID; }
            set
            {
                if (string.IsNullOrEmpty(_securityID) == false && string.IsNullOrEmpty(Venue) == false)
                {
                    MMsheGroupHandler.Instance.UnregisterMM(string.Format("{0}-{1}", securityID, Venue), Instance_ONMM);
                    Clear();
                }

                _securityID = value;

                ChangeSecurityid();
            }
        }
        private string _Venue = "";
        public string Venue
        {
            get { return _Venue; }
            set
            {
                _Venue = value;
                this.dgPriceQdmesp.Venue = value;
                ChangeSecurityid();
            }
        }
        private void ChangeSecurityid()
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
                if (string.IsNullOrEmpty(securityID))
                {
                    Clear();
                    STC.Common.Model.MMshe.MMshe obj = new STC.Common.Model.MMshe.MMshe();
                    obj.enable = "1";
                    obj.securityid = "Tradebook";
                    obj.symbol = null;
                    obj.Params = null;
                    SetMM(obj);
                }
                else
                {
                    if (!string.IsNullOrEmpty(securityID) && !string.IsNullOrEmpty(Venue))
                    {
                        MMsheGroupHandler.Instance.RegisterMM(string.Format("{0}-{1}", securityID, Venue), Instance_ONMM);
                    }
                }
            });
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UserControl_DataContextChanged();
            }));
        }

        private void Clear()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //DataRowView currentDV = this.DataContext as DataRowView;

                if (vm != null)
                {
                    vm.enable = false;
                    vm.securityID = null;
                    vm.symbol = null;
                    vm.LastOperation = null;
                    vm.LastOperationTime = null;
                    vm.Params = null;
                }
            }));
        }

        public void SetMM(STC.Common.Model.MMshe.MMshe obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (this.DataContext == null) return;

                //DataRowView currentDV = this.DataContext as DataRowView;
                if (!string.IsNullOrEmpty(Venue) && !Venue.Equals(obj.venuetypegw)) 
                {
                    return;
                }
                vm.enable = obj.enable == "1" ? true : false;
                vm.Venue = obj.venuetypegw;
                vm.securityID = obj.securityid == "Tradebook" ? "" : obj.securityid;
                string IsInterest = "false";
                if (string.IsNullOrEmpty(obj.symbol))
                {
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(this.securityID);
                    if (bondsInfoPo != null)
                    {
                        obj.symbol = bondsInfoPo.Short_Name;

                        if ("BNC".Equals(bondsInfoPo.symbolType))
                        {
                            IsInterest = "true";
                        }
                        else
                        {
                            IsInterest = "false";
                        }
                    }
                }

                vm.symbol = obj.symbol;
                vm.LastOperation = obj.remark;

                if (obj.Params != null)
                {
                    vm.LastOperationTime = obj.Params.lastOptTime;

                    if (string.IsNullOrEmpty(obj.Params.isQuoteSuccess) || string.IsNullOrEmpty(obj.enable) || obj.enable.Equals("0"))
                    {
                        obj.Params.isQuoteSuccess = "0";
                    }

                    vm.Params = new OrderParamProp(obj.Params);
                    vm.Params.IsInterest = IsInterest;
                }

                #region 根据settletype刷新【仅双边】


                if (obj != null && string.IsNullOrEmpty(obj.securityid) == false && _oldSecuityID != obj.securityid)
                {
                    LogHandler.Info($"MMsheOrderControl->ChangeSecurityID old:[{_oldSecuityID}] new:[{obj.securityid}]");
                    _oldSecuityID = obj.securityid;
                    InitPriceView(this.securityID); // 这里会有outofmemory的风险
                    _oldSettleType = string.Empty;
                }

                if (obj != null && obj.Params != null && string.IsNullOrEmpty(obj.Params.baseSettleType) == false && _oldSettleType != obj.Params.baseSettleType)
                {
                    LogHandler.Info($"MMsheOrderControl->ChangeSettleType old:[{_oldSettleType}] new:[{obj.Params.baseSettleType}]");
                    _oldSettleType = obj.Params.baseSettleType;

                    //设置全双边过滤条件
                    //this.dgPriceQdmesp.SetFilterDicSetttle();

                    this.dgPriceQdmesp.SetFilterDicSetttle(vm.Params.baseSettleType == "1" ? "+0" : "+1");

                    _oldSettleType = obj.Params.baseSettleType;

                    this.dgPriceQdmesp.FilterPanel_FilterDicChanged(null, null);
                }
                //else
                //{
                //    if (vm != null && vm.Params != null && string.IsNullOrEmpty(vm.Params.baseSettleType) == false)
                //    {
                //        this.dgPriceQdmesp.SetFilterDicSetttle(vm.Params.baseSettleType == "1" ? "+0" : "+1");
                //    }
                //    this.dgPriceQdmesp.FilterPanel_FilterDicChanged(null, null);
                //}

                #endregion
            }));
        }

        private void Instance_ONMM(STC.Common.Model.MMshe.MMshe obj)
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
                if (obj.Action == "3")
                {
                    Clear();
                }
                else
                {
                    SetMM(obj);
                }
            });
        }

        public void UnregistMM()
        {
            MMsheGroupHandler.Instance.UnregisterMM(string.Format("{0}-{1}", securityID, Venue), Instance_ONMM);
            //DataRowView drv = (this.DataContext as DataRowView);
            //this.DataContext = null;
            //drv.Row.Table.Rows.Clear();
            //drv.Row.Table.Dispose();
            vm.enable = false;
            vm.securityID = null;
            vm.symbol = null;
            vm.LastOperation = null;
            vm.LastOperationTime = null;
            vm.Params = null;
        }

        private void InitControlBinding()
        {
            // ===   securityID_tb   === //
            Binding securityID_Binding = new Binding();
            securityID_Binding.Path = new PropertyPath(MmsheListBoxViewModel.SECURITY_ID_FIELD);
            securityID_Binding.Mode = BindingMode.TwoWay;
            securityID_Binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            securityID_tb.SetBinding(TextBlock.TextProperty, securityID_Binding);

            // === symbol_tb === //
            Binding symbol_Binding = new Binding();
            symbol_Binding.Path = new PropertyPath(MmsheListBoxViewModel.SYMBOL_FIELD);
            symbol_Binding.Mode = BindingMode.TwoWay;
            symbol_Binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            symbol_tb.SetBinding(TextBlock.TextProperty, symbol_Binding);

            // === LastOperation ===//
            Binding LastOperation_Binding = new Binding();
            LastOperation_Binding.Path = new PropertyPath(MmsheListBoxViewModel.LAST_OPERATION_FIELD);
            LastOperation_Binding.Mode = BindingMode.TwoWay;
            LastOperation_Binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //LastOperation_tb.SetBinding(TextBlock.TextProperty, LastOperation_Binding);

            // === LastOperationTime === //
            Binding LastOperationTime_Binding = new Binding();
            LastOperationTime_Binding.Path = new PropertyPath(MmsheListBoxViewModel.LAST_OPERATION_TIME_FIELD);
            LastOperationTime_Binding.Mode = BindingMode.TwoWay;
            LastOperationTime_Binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //LastOperationTime_tb.SetBinding(TextBlock.TextProperty, LastOperationTime_Binding);

            // === Params_bidQuoteModel ===//
            Binding Params_bidQuoteModel_Binding = new Binding();
            Params_bidQuoteModel_Binding.Path = new PropertyPath(MmsheListBoxViewModel.PARAMS_FIELD + ".bidQuoteModel");
            Params_bidQuoteModel_Binding.Mode = BindingMode.OneWay;

            // === BidBasePrice === //
            // TODO. Params下字段暂未定义

            // === BidBaseQty === //
            // TODO. Params下字段暂未定义

            // === Params_bidBasePriority1Text === //
            Binding Params_bidBasePriority1Text_Binding = new Binding();
            Params_bidBasePriority1Text_Binding.Path = new PropertyPath(MmsheListBoxViewModel.PARAMS_FIELD + ".bidBasePriority");
            Params_bidBasePriority1Text_Binding.Mode = BindingMode.OneWay;

            MMBasePriorityTextConverter bidPriTextCon = new MMBasePriorityTextConverter();
            Params_bidBasePriority1Text_Binding.Converter = bidPriTextCon;
            Params_bidBasePriority1Text_Binding.ConverterParameter = "1";
            //BidBasePriority1Text_tb.SetBinding(TextBlock.TextProperty, Params_bidBasePriority1Text_Binding);

            // === Params_bidBasePriority1Value ===//
            Binding Params_bidDefenCalPrice_Binding = new Binding();
            Params_bidDefenCalPrice_Binding.Path = new PropertyPath(MmsheListBoxViewModel.PARAMS_FIELD + ".bidDefenCalPrice");
            Params_bidDefenCalPrice_Binding.Mode = BindingMode.OneWay;

            Binding Params_bidAlgoCalPrice_Binding = new Binding();
            Params_bidAlgoCalPrice_Binding.Path = new PropertyPath(MmsheListBoxViewModel.PARAMS_FIELD + ".bidAlgoCalPrice");
            Params_bidAlgoCalPrice_Binding.Mode = BindingMode.OneWay;

            MultiBinding Params_bidBasePriority1Value_MultiBinding = new MultiBinding();


            Binding Params_bidBasePriority1Text_Binding2 = new Binding();
            Params_bidBasePriority1Text_Binding2.Path = new PropertyPath(MmsheListBoxViewModel.PARAMS_FIELD + ".bidBasePriority");
            Params_bidBasePriority1Text_Binding2.Mode = BindingMode.OneWay;
            Params_bidBasePriority1Value_MultiBinding.Bindings.Add(Params_bidBasePriority1Text_Binding2);



            Params_bidBasePriority1Value_MultiBinding.Bindings.Add(Params_bidDefenCalPrice_Binding);
            Params_bidBasePriority1Value_MultiBinding.Bindings.Add(Params_bidAlgoCalPrice_Binding);
            Params_bidBasePriority1Value_MultiBinding.Mode = BindingMode.OneWay;

            MMBasePriorityValueConverter bidPriValue1Con = new MMBasePriorityValueConverter();
            Params_bidBasePriority1Value_MultiBinding.Converter = bidPriValue1Con;
            Params_bidBasePriority1Value_MultiBinding.ConverterParameter = "1";


        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = tb.Tag.ToString();
            decimal max;
            decimal min;

            Regex regex = null;

            if (tag == "Price")
            {
                regex = new Regex("^[0-9.-]+$");
                max = 1000;
                min = 0;
            }
            else
            {
                regex = new Regex("^[0-9]+$");
                max = 99999;
                min = 1;
            }


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            if (selectTxt.Length == text.Length)
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);


            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }

        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;

                return;
            }
        }

        private void BidBasePrice_tb_GotFocus(object sender, RoutedEventArgs e)
        {
            //SetBidBasePriceEnterColor();
        }

        private void BidBasePrice_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            ////InitCurrentOrderParams();
            //if (vm.Params == null) { return; }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void BidBaseQty_tb_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void BidBaseQty_tb_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void BidBasePrice_tb_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void BidBasePrice_tb_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void BidBaseQty_tb_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void BidBaseQty_tb_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void OfrBasePrice_tb_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void OfrBasePrice_tb_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void OfrBasePrice_tb_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void OfrBasePrice_tb_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void OfrBaseQty_tb_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void OfrBaseQty_tb_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void OfrBaseQty_tb_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void OfrBaseQty_tb_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        //private void InitCurrentOrderParams()
        //{
        //    DataRowView dt = this.DataContext as DataRowView;
        //    if (dt == null) return;

        //    if (dt["params"] == null) 
        //    {
        //        vm.Params = null;
        //        return; 
        //    }
        //    vm.Params = dt["params"] as OrderParamProp;
        //}

        //private void SendQuoteModelToManual(string mode)
        //{
        //    string text = string.Empty;

        //    //InitCurrentOrderParams();

        //    //报价模式1:手动、2:自动
        //    switch (mode.ToLower())
        //    {
        //        case "bid":
        //            if (vm.Params.bidQuoteModel != "Manual")
        //            {
        //                Dictionary<string, string> sendDic = new Dictionary<string, string>();
        //                sendDic.Add("bidQuoteModel", "Manual");
        //                SendUpdateParamsTopic(sendDic);
        //            }
        //            break;

        //        case "ofr":
        //            if (vm.Params.ofrQuoteModel != "Manual")
        //            {
        //                Dictionary<string, string> sendDic = new Dictionary<string, string>();
        //                sendDic.Add("ofrQuoteModel", "Manual");
        //                SendUpdateParamsTopic(sendDic);
        //            }
        //            break;
        //    }

        //    return text;
        //}

        private void BidBasePricePlus_btn_Click(object sender, RoutedEventArgs e)
        {
            ////InitCurrentOrderParams();
            //if (vm.Params == null) { return; }

            decimal defaultManualSpread;

            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            else
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }

            decimal oldPrice;
            if (vm.Params.bidQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.bidBasePrice, out oldPrice))
                {
                    return;
                }
            }
            if (!decimal.TryParse(vm.Params.bidBasePrice, out oldPrice))
            {
                oldPrice = 0;
            }
            decimal newPrice = oldPrice + defaultManualSpread;
            if (newPrice <= -100 || newPrice > 9999)
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            Dictionary<string, string> send = new Dictionary<string, string>();
            send.Add("manualBid", newPrice.ToString());
            if (vm.Params.bidQuoteModel != "Manual")
            {
                send.Add("bidQuoteModel", "Manual");
            }
            this.SendUpdateParamsTopic(send);

        }

        private void BidBasePriceMinus_btn_Click(object sender, RoutedEventArgs e)
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            decimal defaultManualSpread;
            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            else
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }

            decimal oldPrice;
            if (vm.Params.bidQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.bidBasePrice, out oldPrice))
                {
                    return;
                }
            }
            if (!decimal.TryParse(vm.Params.bidBasePrice, out oldPrice))
            {
                oldPrice = 0;
            }
            decimal newPrice = oldPrice - defaultManualSpread;
            if (newPrice <= -100 || newPrice > 9999)
            {
                //List<Inline> runLines = new List<Inline>();
                //runLines.Add(new Run("报价不能小于等于-100或者大于9999"));
                //CustomMessageBox.ShowDialog(CustomMessageBox.EnumNotifyType.Warning, runLines);
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            if (oldPrice != newPrice)
            {
                Dictionary<string, string> send = new Dictionary<string, string>();
                send.Add("manualBid", newPrice.ToString());
                if (vm.Params.bidQuoteModel != "Manual")
                {
                    send.Add("bidQuoteModel", "Manual");
                }
                this.SendUpdateParamsTopic(send);
            }

        }

        private void DoBidQuoteMethodShift()
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            string oldValue = vm.Params.bidBasePriority;
            string newValue = string.Empty;

            if (oldValue.Substring(0, 1) == "1")
            {
                newValue = "2,1";
            }
            else
            {
                newValue = "1,2";
            }

            Dictionary<string, string> send = new Dictionary<string, string>();
            send.Add("bidBasePriority", newValue);
            SendUpdateParamsTopic(send);
        }

        private void DoOfrQuoteMethodShift()
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            string oldValue = vm.Params.ofrBasePriority;
            string newValue = string.Empty;

            if (oldValue.Substring(0, 1) == "1")
            {
                newValue = "2,1";
            }
            else
            {
                newValue = "1,2";
            }

            Dictionary<string, string> send = new Dictionary<string, string>();
            send.Add("ofrBasePriority", newValue);
            SendUpdateParamsTopic(send);
        }

        private void OfrQuoteMethodShift_btn_Click(object sender, RoutedEventArgs e)
        {
            this.DoOfrQuoteMethodShift();
        }

        private void OfrBasePriceMinus_btn_Click(object sender, RoutedEventArgs e)
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            decimal defaultManualSpread;
            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            else
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }

            decimal oldPrice;
            if (vm.Params.ofrQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldPrice))
                {
                    return;
                }
            }
            if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldPrice))
            {
                oldPrice = 0;
            }
            decimal newPrice = oldPrice - defaultManualSpread;
            if (newPrice <= -100 || newPrice > 9999)
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            if (oldPrice != newPrice)
            {
                Dictionary<string, string> send = new Dictionary<string, string>();
                send.Add("manualOfr", newPrice.ToString());
                if (vm.Params.ofrQuoteModel != "Manual")
                {
                    send.Add("ofrQuoteModel", "Manual");
                }
                this.SendUpdateParamsTopic(send);
            }
        }

        private void OfrBasePricePlus_btn_Click(object sender, RoutedEventArgs e)
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            decimal defaultManualSpread;

            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            else
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }

            decimal oldPrice;
            if (vm.Params.ofrQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldPrice))
                {
                    return;
                }
            }
            if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldPrice))
            {
                oldPrice = 0;
            }
            decimal newPrice = oldPrice + defaultManualSpread;
            if (newPrice <= -100 || newPrice > 9999)
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            Dictionary<string, string> send = new Dictionary<string, string>();
            send.Add("manualOfr", newPrice.ToString());
            if (vm.Params.ofrQuoteModel != "Manual")
            {
                send.Add("ofrQuoteModel", "Manual");
            }
            this.SendUpdateParamsTopic(send);
        }

        private void BidQuoteMethodShift_btn_Click(object sender, RoutedEventArgs e)
        {
            this.DoBidQuoteMethodShift();
        }

        private decimal GetAdjustValue(string str)
        {
            decimal adjust = 0;

            string value = str.Substring(0, str.IndexOf("bp"));

            decimal out_value;

            if (decimal.TryParse(value, out out_value))
            {
                adjust = out_value / SpreadRate;
            }

            return adjust;
        }

        private void PriceWiden_btn_Click(object sender, RoutedEventArgs e)
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            decimal oldBidPrice;
            decimal oldOfrPrice;
            decimal defaultManualSpread;
            bool bidPriceIsNull = false;
            bool ofrPriceIsNull = false;

            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = (vm.Params.stepLength.ConvertToDecimal());
            }
            else
            {
                defaultManualSpread = (vm.Params.stepLength.ConvertToDecimal());
            }

            if (vm.Params.bidQuoteModel != "Manual" || vm.Params.ofrQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice) && !decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
                {
                    return;
                }
            }

            if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice))
            {
                bidPriceIsNull = true;
                oldBidPrice = 0;
            }
            decimal newBidPrice = oldBidPrice - defaultManualSpread;
            if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
            {
                ofrPriceIsNull = true;
                oldOfrPrice = 0;
            }
            decimal newOfrPrice = oldOfrPrice + defaultManualSpread;
            if (newBidPrice <= -100 || newBidPrice > 9999 || newOfrPrice <= -100 || newOfrPrice > 9999)
            {
                //List<Inline> runLines = new List<Inline>();
                //runLines.Add(new Run("报价不能小于等于-100或者大于9999"));
                //CustomMessageBox.ShowDialog(CustomMessageBox.EnumNotifyType.Warning, runLines);
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            if (newBidPrice != oldBidPrice && newOfrPrice != oldOfrPrice)
            {
                Dictionary<string, string> send = new Dictionary<string, string>();
                send.Add("manualBid", newBidPrice.ToString());
                send.Add("manualOfr", newOfrPrice.ToString());
                if (!bidPriceIsNull && vm.Params.bidQuoteModel != "Manual")
                {
                    send.Add("bidQuoteModel", "Manual");
                }
                if (!ofrPriceIsNull && vm.Params.ofrQuoteModel != "Manual")
                {
                    send.Add("ofrQuoteModel", "Manual");
                }
                this.SendUpdateParamsTopic(send);
            }
        }

        private void PriceNarrow_btn_Click(object sender, RoutedEventArgs e)
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            decimal oldBidPrice;
            decimal oldOfrPrice;
            decimal defaultManualSpread;
            bool bidPriceIsNull = false;
            bool ofrPriceIsNull = false;

            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = (vm.Params.stepLength.ConvertToDecimal());
            }
            else
            {
                defaultManualSpread = (vm.Params.stepLength.ConvertToDecimal());
            }

            if (vm.Params.bidQuoteModel != "Manual" || vm.Params.ofrQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice) && !decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
                {
                    return;
                }
            }
            if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice))
            {
                bidPriceIsNull = true;
                oldBidPrice = 0;
            }
            decimal newBidPrice = oldBidPrice + defaultManualSpread;
            if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
            {
                ofrPriceIsNull = true;
                oldOfrPrice = 0;
            }
            decimal newOfrPrice = oldOfrPrice - defaultManualSpread;

            if (newBidPrice <= -100 || newBidPrice > 9999 || newOfrPrice <= -100 || newOfrPrice > 9999)
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            if (newBidPrice != oldBidPrice && newOfrPrice != oldOfrPrice)
            {
                Dictionary<string, string> send = new Dictionary<string, string>();
                send.Add("manualBid", newBidPrice.ToString());
                send.Add("manualOfr", newOfrPrice.ToString());
                if (!bidPriceIsNull && vm.Params.bidQuoteModel != "Manual")
                {
                    send.Add("bidQuoteModel", "Manual");
                }
                if (!ofrPriceIsNull && vm.Params.ofrQuoteModel != "Manual")
                {
                    send.Add("ofrQuoteModel", "Manual");
                }
                this.SendUpdateParamsTopic(send);
            }
        }

        private void PricePlus_btn_Click(object sender, RoutedEventArgs e)
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            decimal oldBidPrice;
            decimal oldOfrPrice;
            decimal defaultManualSpread;
            bool bidPriceIsNull = false;
            bool ofrPriceIsNull = false;
            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            else
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            if (vm.Params.bidQuoteModel != "Manual" || vm.Params.ofrQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice) && !decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
                {
                    return;
                }
            }

            if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice))
            {
                bidPriceIsNull = true;
                oldBidPrice = 0;
            }
            decimal newBidPrice = oldBidPrice + defaultManualSpread;
            if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
            {
                ofrPriceIsNull = true;
                oldOfrPrice = 0;
            }
            decimal newOfrPrice = oldOfrPrice + defaultManualSpread;

            if (newBidPrice <= -100 || newBidPrice > 9999 || newOfrPrice <= -100 || newOfrPrice > 9999)
            {
                //List<Inline> runLines = new List<Inline>();
                //runLines.Add(new Run("报价不能小于等于-100或者大于9999"));
                //CustomMessageBox.ShowDialog(CustomMessageBox.EnumNotifyType.Warning, runLines);
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            Dictionary<string, string> send = new Dictionary<string, string>();
            send.Add("manualBid", newBidPrice.ToString());
            send.Add("manualOfr", newOfrPrice.ToString());
            if (!bidPriceIsNull && vm.Params.bidQuoteModel != "Manual")
            {
                send.Add("bidQuoteModel", "Manual");
            }
            if (!ofrPriceIsNull && vm.Params.ofrQuoteModel != "Manual")
            {
                send.Add("ofrQuoteModel", "Manual");
            }
            this.SendUpdateParamsTopic(send);
        }

        private void PriceMinus_btn_Click(object sender, RoutedEventArgs e)
        {
            //InitCurrentOrderParams();
            if (vm.Params == null) { return; }

            decimal oldBidPrice;
            decimal oldOfrPrice;
            decimal defaultManualSpread;
            bool bidPriceIsNull = false;
            bool ofrPriceIsNull = false;

            if (!decimal.TryParse(vm.Params.defaultManualSpread, out defaultManualSpread))
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            else
            {
                defaultManualSpread = vm.Params.stepLength.ConvertToDecimal();
            }
            if (vm.Params.bidQuoteModel != "Manual" || vm.Params.ofrQuoteModel != "Manual")
            {
                if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice) && !decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
                {
                    return;
                }
            }

            if (!decimal.TryParse(vm.Params.bidBasePrice, out oldBidPrice))
            {
                bidPriceIsNull = true;
                oldBidPrice = 0;
            }
            decimal newBidPrice = oldBidPrice - defaultManualSpread;
            if (!decimal.TryParse(vm.Params.ofrBasePrice, out oldOfrPrice))
            {
                ofrPriceIsNull = true;
                oldOfrPrice = 0;
            }
            decimal newOfrPrice = oldOfrPrice - defaultManualSpread;
            if (newBidPrice <= -100 || newBidPrice > 9999 || newOfrPrice <= -100 || newOfrPrice > 9999)
            {
                //List<Inline> runLines = new List<Inline>();
                //runLines.Add(new Run("报价不能小于等于-100或者大于9999"));
                //CustomMessageBox.ShowDialog(CustomMessageBox.EnumNotifyType.Warning, runLines);
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("报价不能小于等于-100或者大于9999", true);
                msgBox.ShowDialog();
                return;
            }

            if (newBidPrice != oldBidPrice && newOfrPrice != oldOfrPrice)
            {
                Dictionary<string, string> send = new Dictionary<string, string>();
                send.Add("manualBid", newBidPrice.ToString());
                send.Add("manualOfr", newOfrPrice.ToString());
                if (!bidPriceIsNull && vm.Params.bidQuoteModel != "Manual")
                {
                    send.Add("bidQuoteModel", "Manual");
                }
                if (!ofrPriceIsNull && vm.Params.ofrQuoteModel != "Manual")
                {
                    send.Add("ofrQuoteModel", "Manual");
                }
                this.SendUpdateParamsTopic(send);
            }
        }

        private void configBtn_Click(object sender, RoutedEventArgs e)
        {

            if (vm != null && vm.Params != null)
            {
                OrderParamProp param = vm.Params;

                if (param != null)
                {
                    if (string.IsNullOrEmpty(param.securityid))
                    {
                        param.securityid = vm.securityID;
                    }

                    if (!string.IsNullOrEmpty(param.securityid))
                    {
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(param.securityid);
                        String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");

                        if (today.CompareTo(bondsInfoPo.Maturity_Date.ToString()) > 0)
                        {
                            return;
                        }

                        if (string.IsNullOrEmpty(param.symbol))
                        {
                            param.symbol = bondsInfoPo.Short_Name;
                        }

                        OrderDetailView odView = new OrderDetailView();

                        OrderDetailViewModel odVM;
                        odVM = new OrderDetailViewModel(param);
                        odVM.SelectedItemRow = vm;
                        odVM.MyWindow = odView;

                        odView.DataContext = odVM;
                        odView.ShowDialog();
                    }

                }
            }
        }

        private void bidPrice_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private async void InitPriceView(string securityID, Window win = null, bool IsReinit = false)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    int tabindex = Tab_DgPrice.SelectedIndex;
                    //if (tabindex == 0)
                    //{
                    this.dgPrice.RemoveDbDoubleClickEvent();
                    this.dgPrice.Init(securityID, win, true);
                    //}
                    //else
                    //{
                    this.dgPriceQdmesp.RemoveDbDoubleClickEvent();
                    //设置全双边过滤条件
                    this.dgPriceQdmesp.SetFilterDicSetttle();
                    if (vm != null && vm.Params != null && string.IsNullOrEmpty(vm.Params.baseSettleType) == false && _oldSettleType != vm.Params.baseSettleType)
                    {
                        this.dgPriceQdmesp.SetFilterDicSetttle(vm.Params.baseSettleType == "1" ? "+0" : "+1");
                    }
                    this.dgPriceQdmesp.Init(securityID, win, true);
                    //}

                    this.dgMarketCard.SetSecurtityID(securityID);
                    int priceTypeIndex = 0;
                    if (securityID.EndsWith(".SH") || securityID.EndsWith(".SZ")) 
                    {
                        priceTypeIndex = 1;
                    }
                    this.dgMarketCard.UpdatePriceType(priceTypeIndex);
                }));
            });
        }

        private void SetAllButtonIsEnabled(bool isEnabled)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.BidBasePricePlus_btn.IsEnabled = isEnabled;
                this.BidBasePriceMinus_btn.IsEnabled = isEnabled;
                this.OfrBasePricePlus_btn.IsEnabled = isEnabled;
                this.OfrBasePriceMinus_btn.IsEnabled = isEnabled;

                this.PricePlus_btn.IsEnabled = isEnabled;
                this.PriceMinus_btn.IsEnabled = isEnabled;
                this.PriceWiden_btn.IsEnabled = isEnabled;
                this.PriceNarrow_btn.IsEnabled = isEnabled;

                this.bidStrategyMode_btn.IsEnabled = isEnabled;
                this.ofrStrategyMode_btn.IsEnabled = isEnabled;
            });
        }

        private void SendUpdateParamsTopic(Dictionary<string, string> paramDic)
        {
            if (paramDic.ContainsKey("bidQuoteModel") == false)
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
            if (paramDic.ContainsKey("ofrQuoteModel") == false)
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

            //InitCurrentOrderParams();
            string retText = string.Empty;

            MMsheOrderParamReq req = new MMsheOrderParamReq();

            OrderDetailViewModel.Data2Request(vm.Params, req);

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
                        req.manualBid = MMsheCommonUtils.Format3Digit(newValue);
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
                        req.manualOfr = MMsheCommonUtils.Format3Digit(newValue);
                        break;
                    case "ofrBaseQty":
                        req.ofrBaseQty = newValue;
                        break;
                    case "ofrBasePriority":
                        req.ofrBasePriority = newValue;
                        break;
                }
            }

            string errorMsg = string.Empty;

            //当QuoteModel为手动时，需要将baseprice的值付给manual
            //if (req.bidQuoteModel == "Manual") { req.manualBid = req.manualBid; }
            //if (req.ofrQuoteModel == "Manual") { req.manualOfr = req.manualOfr; }

            req = OrderDetailViewModel.CheckSendFieldNull(req);
            if (paramDic.ContainsKey("manualBid") || paramDic.ContainsKey("manualOfr"))
            {
                if (!MMsheCommonUtils.CheckBaseQtyValid(req))
                {
                    return;
                }
            }

            SetAllButtonIsEnabled(false);

            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                PResult result = STC.Common.Handler.Admin.MMsheHandler.Instance.mmUpdateParams(req);
                if (result != null && result.Code == "0000")
                {
                    SetAllButtonIsEnabled(true);
                    //errorMsg = "更新[" + req.securityid + "]参数成功！";
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (oldBidValue == "Manual" && req.bidQuoteModel == "API")
                        {
                            string apiBidQty = vm.Params.apiBidQty;
                            if (!string.IsNullOrEmpty(apiBidQty))
                            {
                                this.bidAPIWarn_Img.ToolTip = "接口报量为" + apiBidQty + "KW,请注意报量变化";
                                this.bidAPIWarn_Img.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            this.bidAPIWarn_Img.ToolTip = string.Empty;
                            this.bidAPIWarn_Img.Visibility = Visibility.Collapsed;
                        }

                        if (oldOfrValue == "Manual" && req.ofrQuoteModel == "API")
                        {
                            string apiOfrQty = vm.Params.apiOfrQty;
                            if (!string.IsNullOrEmpty(apiOfrQty))
                            {
                                this.ofrAPIWarn_Img.ToolTip = "接口报量为" + apiOfrQty + "KW,请注意报量变化";
                                this.ofrAPIWarn_Img.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            this.ofrAPIWarn_Img.ToolTip = string.Empty;
                            this.ofrAPIWarn_Img.Visibility = Visibility.Collapsed;
                        }
                    });
                }
                else
                {
                    if (result != null && !string.IsNullOrEmpty(result.Msg))
                    {
                        errorMsg = result.Msg;
                        SetAllButtonIsEnabled(true);
                    }
                    else
                    {
                        errorMsg = "服务器接收数据失败！";
                    }
                }

                if ((result != null && result.Code != "0000") || result == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg(errorMsg, true);
                        msgBox.ShowDialog();
                    });
                }
            });
        }

        private void Params_bidQuoteModel_cb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender != null && sender is RadioButton)
            {
                RadioButton rb = sender as RadioButton;
                if (rb != null && rb.DataContext != null && rb.DataContext is DataRowView)
                {
                    //InitCurrentOrderParams();
                    Dictionary<string, string> sendDic = new Dictionary<string, string>();
                    sendDic.Add("bidQuoteModel", vm.Params.bidQuoteModel == "1" ? "2" : "1");
                    SendUpdateParamsTopic(sendDic);
                }
            }
        }

        private void Params_ofrQuoteModel_cb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender != null && sender is RadioButton)
            {
                RadioButton rb = sender as RadioButton;
                if (rb != null && rb.DataContext != null && rb.DataContext is DataRowView)
                {
                    //InitCurrentOrderParams();
                    Dictionary<string, string> sendDic = new Dictionary<string, string>();
                    sendDic.Add("ofrQuoteModel", vm.Params.ofrQuoteModel == "1" ? "2" : "1");
                    SendUpdateParamsTopic(sendDic);

                }
            }
        }

        private void UserControl_DataContextChanged()//object sender, DependencyPropertyChangedEventArgs e)
        {
            oldBidValue = string.Empty;
            oldOfrValue = string.Empty;

            this.bidAPIWarn_Img.Visibility = Visibility.Collapsed;
            this.ofrAPIWarn_Img.Visibility = Visibility.Collapsed;

            //if (this.DataContext == null) 
            //{
            //    this.bidAPIWarn_Img.Visibility = Visibility.Collapsed;
            //    this.ofrAPIWarn_Img.Visibility = Visibility.Collapsed;
            //    return; 
            //}

            //this.Tab_DgPrice.SelectedIndex = 0;
            //InitPriceView(this.securityID);

            //DataRowView currentDV = this.DataContext as DataRowView;
            //if (currentDV != null && currentDV is DataRowView)
            //{
            //    OrderParamProp orderParams = currentDV["Params"] as OrderParamProp;

            //    if (orderParams != null)
            //    {
            //        if (string.IsNullOrEmpty(orderParams.isQuoteSuccess))
            //        {
            //            orderParams.isQuoteSuccess = "0";
            //        }                  

            //        vm.Params = orderParams;
            //    }
            //}

            if (vm.Params != null)
            {
                if (string.IsNullOrEmpty(vm.Params.isQuoteSuccess))
                {
                    vm.Params.isQuoteSuccess = "0";
                }
            }

            this.bidAPIWarn_Img.Visibility = Visibility.Collapsed;
            this.ofrAPIWarn_Img.Visibility = Visibility.Collapsed;

            //this.IsSyncQuoteModel_ToggleButton.IsChecked = false;

            this.IsSyncQuoteModel_ToggleButton.IsChecked = MMsheCommonUtils.ReadOrderControlIsSyncQuoteModel(securityID);
        }

        //private void AllMarket_rb_Click(object sender, RoutedEventArgs e)
        //{
        //    AllMarket_rb.IsChecked = true;
        //    OnlyMM_rb.IsChecked = false;

        //    if (this.DataContext != null)
        //    {
        //        string securityID = (this.DataContext as DataRowView)["SecurityID"].ToString();
        //        Tab_DgPrice.SelectedIndex = 0;
        //        InitPriceView(securityID);

        //    }
        //}

        //private void OnlyMM_rb_Click(object sender, RoutedEventArgs e)
        //{
        //    OnlyMM_rb.IsChecked = true;
        //    AllMarket_rb.IsChecked = false;

        //    if (this.DataContext != null)
        //    {
        //        string securityID = (this.DataContext as DataRowView)["SecurityID"].ToString();
        //        Tab_DgPrice.SelectedIndex = 1;
        //        InitPriceView(securityID);
        //    }
        //}

        private void popupBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupWindow popOrderWin = new PopupWindow();
            MMsheOrderControl orderControl = new MMsheOrderControl();
            orderControl.DataContext = vm;
            orderControl.popupBtn.Visibility = Visibility.Collapsed;
            popOrderWin.Content = orderControl;
            popOrderWin.Width = 350;
            popOrderWin.SizeToContent = SizeToContent.Height;
            popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            popOrderWin.Show();
        }

        private void SetStrategyType(string mode, string newValue)
        {
            Dictionary<string, string> sendDic = new Dictionary<string, string>();

            switch (mode.ToLower())
            {
                case "bid":
                    sendDic.Add("bidQuoteModel", newValue);
                    break;
                case "ofr":
                    sendDic.Add("ofrQuoteModel", newValue);
                    break;
            }

            if (sendDic.Count > 0)
            { SendUpdateParamsTopic(sendDic); }

        }

        private void bidStrategyMode_cbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
            ////InitCurrentOrderParams();

            //ComboBox cb = sender as ComboBox;
            //newBidValue = cb.SelectedValue != null ? cb.SelectedValue.ToString() : string.Empty;
            //cb.SelectedValue = vm.Params.bidQuoteModel;

            //string a = ((this.DataContext as DataRowView)["params"] as OrderParamProp).bidQuoteModel;
            //if (this.DataContext != null)
            //{
            //    string newValue = ((this.DataContext as DataRowView)["params"] as OrderParamProp).bidQuoteModel;
            //    if (oldBidValue == "Manual" && newValue == "API")
            //    {
            //        string apiBidQty = ((this.DataContext as DataRowView)["params"] as OrderParamProp).apiBidQty;
            //        if (!string.IsNullOrEmpty(apiBidQty))
            //        {
            //            this.bidAPIWarn_Img.ToolTip = "接口报量为" + apiBidQty + "KW,请注意报量变化";
            //            this.bidAPIWarn_Img.Visibility = Visibility.Visible;
            //        }
            //    }
            //    else
            //    {
            //        this.bidAPIWarn_Img.ToolTip = string.Empty;
            //        this.bidAPIWarn_Img.Visibility = Visibility.Collapsed;
            //    }
            //}

        }

        private void bidStrategyMode_cbx_DropDownClosed(object sender, EventArgs e)
        {
            if (this.DataContext != null)
            {
                ComboBox cb = sender as ComboBox;
                oldBidValue = vm.Params.bidQuoteModel;
                if (!string.IsNullOrEmpty(oldBidValue))
                {
                    string newValue = cb.SelectedValue.ToString();
                    //cb.SelectedValue = oldBidValue;
                    vm.Params.bidQuoteModel = oldBidValue;
                    SetStrategyType("bid", newValue);
                }
            }
        }

        private void ofrStrategyMode_cbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (this.DataContext != null)
            //{
            //    string newValue = ((this.DataContext as DataRowView)["params"] as OrderParamProp).ofrQuoteModel;
            //    if (oldOfrValue == "Manual" && newValue == "API")
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

        private void ofrStrategyMode_cbx_DropDownClosed(object sender, EventArgs e)
        {
            if (this.DataContext != null)
            {
                ComboBox cb = sender as ComboBox;
                oldOfrValue = vm.Params.ofrQuoteModel;
                if (!string.IsNullOrEmpty(oldOfrValue))
                {
                    string newValue = cb.SelectedValue.ToString();
                    vm.Params.ofrQuoteModel = oldOfrValue;
                    SetStrategyType("ofr", newValue);
                }
            }
        }

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
            if (popBidStrategyList.IsOpen)
            {
                popBidStrategyList.IsOpen = false;
                if (bidStrategyList_dg.SelectedItem != null)
                {
                    oldBidValue = vm.Params.bidQuoteModel;
                    KeyValuePair<string, string> current = (KeyValuePair<string, string>)bidStrategyList_dg.SelectedItem;

                    //发送消息给后台，切换报价模式
                    if (current.Key != vm.Params.bidQuoteModel)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("bidQuoteModel", current.Key);
                        SendUpdateParamsTopic(dic);
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
            if (popOfrStrategyList.IsOpen)
            {
                popOfrStrategyList.IsOpen = false;
                if (ofrStrategyList_dg.SelectedItem != null)
                {
                    oldOfrValue = vm.Params.ofrQuoteModel;
                    KeyValuePair<string, string> current = (KeyValuePair<string, string>)ofrStrategyList_dg.SelectedItem;

                    //发送消息给后台，切换报价模式
                    if (current.Key != vm.Params.ofrQuoteModel)
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

        private void ofrStrategyList_dg_LostFocus(object sender, RoutedEventArgs e)
        {
            if (popOfrStrategyList.IsOpen)
            {
                popOfrStrategyList.IsOpen = false;
            }
        }


        private void btnFixFloatButton_Checked(object sender, RoutedEventArgs e)
        {
            Window parentWindow = CommonUtils.GetParentObject<Window>(this);
            if (parentWindow != null)
            {
                if (btnFixFloatButton.IsChecked.Value)
                {
                    parentWindow.Topmost = true;
                }
                else
                {
                    parentWindow.Topmost = false;
                }
            }

        }

        private void btnShutDown_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = CommonUtils.GetParentObject<Window>(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
                GC.Collect();
            }
        }

        private void AllStrategyList_dg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (popAllStrategyList.IsOpen)
            {
                popAllStrategyList.IsOpen = false;
                if (AllStrategyList_dg.SelectedItem != null)
                {
                    oldBidValue = vm.Params.bidQuoteModel;
                    oldOfrValue = vm.Params.ofrQuoteModel;
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

        private void AllStrategyList_dg_LostFocus(object sender, RoutedEventArgs e)
        {
            if (popAllStrategyList.IsOpen)
            {
                popAllStrategyList.IsOpen = false;
            }
        }

        private void OrderControlMain_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OrderControlMain_Initialized(object sender, EventArgs e)
        {

        }

        private void OrderControlMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Tab_DgPrice.Width = e.NewSize.Width;
            if (e.NewSize.Height - 152 > 0)
            {
                Pnl_dgPrice.Height = e.NewSize.Height - 152;
                //if (Pnl_dgPrice.Height < 268)
                //{
                if (Pnl_dgPrice.Height - TitleBar.ActualHeight > 0)
                {
                    dgMarketCard.Height = Pnl_dgPrice.Height - TitleBar.ActualHeight;
                }
            }
            //Pnl_dgPrice.Height
        }



        public bool IsUnify
        {
            get { return (bool)GetValue(IsUnifyProperty); }
            set { SetValue(IsUnifyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsUnify.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUnifyProperty =
            DependencyProperty.Register("IsUnify", typeof(bool), typeof(MMsheOrderControl), new PropertyMetadata(false));

        private static void OnIsUnifyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MMsheOrderControl control = d as MMsheOrderControl;
            control.vm.IsUnify = (bool)e.NewValue;
        }

        private void popAllStrategyList_Opened(object sender, EventArgs e)
        {
            var ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                popAllStrategyList.HorizontalOffset = 163;
            }
            else
            {
                popAllStrategyList.HorizontalOffset = 0;
            }
        }
    }

    public class MMOrderControlModel : ViewModelBase
    {
        private bool _isUnify = false;

        public bool IsUnify
        {
            get { return _isUnify; }
            set
            {
                _isUnify = value;
                this.OnPropertyChanged();
            }
        }

        private bool _enable = false;
        public bool enable
        {
            get { return _enable; }
            set
            {
                _enable = value;
                OnPropertyChanged();
            }
        }

        private string _securityID = string.Empty;
        public string securityID
        {
            get { return _securityID; }
            set
            {
                if (_securityID != value)
                {
                    _securityID = value;

                    IsUnify = MMsheGroupHandler.Instance.IsUnify(string.Format("{0}-{1}", value, Venue));

                    OnPropertyChanged();
                }
            }
        }
        private string _Venue = "";
        public string Venue
        {
            get { return _Venue; }
            set
            {
                _Venue = value; 
                OnPropertyChanged(); 
            }
        }
        private string _symbol = string.Empty;
        public string symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                OnPropertyChanged();
            }
        }

        private OrderParamProp _Params = new OrderParamProp(null);
        public OrderParamProp Params
        {
            get { return _Params; }
            set
            {
                _Params = value;
                OnPropertyChanged();
            }
        }

        private string _LastOperation = string.Empty;
        public string LastOperation
        {
            get { return _LastOperation; }
            set
            {
                _LastOperation = value;
                OnPropertyChanged();
            }
        }

        private string _LastOperationTime = string.Empty;
        public string LastOperationTime
        {
            get { return _LastOperationTime; }
            set
            {
                _LastOperationTime = value;
                OnPropertyChanged();
            }
        }

        private int _curtabIndex = 0;
        public int CurTabIndex
        {
            get { return _curtabIndex; }
            set
            {
                _curtabIndex = value;
                OnPropertyChanged(nameof(CurTabIndex));
            }
        }

        public MMOrderControlModel()
        {


        }
    }
}

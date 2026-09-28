using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using STC.Common.ViewModel.Order;
using STC.Common.Utils;
using STC.Common.View.Component;
using Newtonsoft.Json;
using STC.Common.Model.Drp;

namespace STC.Common.View.Order
{
    /// <summary>
    /// SendPointPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class SendPointPriceView : Window
    {
        private SendPointPriceModel viewModel = null;

        public SendPointPriceView()
        {
            InitializeComponent();
            datePicker.BlackoutDates.AddDatesInPast();
        }

        /// <summary>
        /// 初始调用参数
        /// </summary>
        /// <param name="security">债券代码 例如 180210.IB</param>
        /// <param name="isBid">方向 1 = Bid 2 = Ofr</param>
        /// <param name="clickPrice">收益率（%）</param>
        /// <param name="clickAmount">数量（万）</param>
        /// <param name="settle"></param>
        /// <param name="netPrice"></param>
        /// <param name="source">渠道代码或渠道名称或渠道数字代码</param>
        /// <param name="wholeflag"></param>
        /// <param name="isFrim"></param>
        /// <param name="orderinfotip">订单盈利信息  例如 3.2250% 2000*+4000(明+0)+1000(+0)+3000(远)</param>
        /// <param name="clOrdID">下单ID</param>
        /// <param name="isDefaultDrp">默认打开直接回价页面</param>
        public SendPointPriceView(string security, bool isBid, string clickPrice, string clickAmount, string settle, string netPrice, string source,
            string wholeflag, bool isFrim, string id = null, string trsactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null,
            string orderinfotip = "", string clOrdID = "", string venueTypeGW = null, string iocTimeOut = null, string info4 = null, bool isDefaultDrp = false, string partySubName = null, string orderPrice=null,
            string orderAmount=null, string orderwholeflag = null, string bargainyield=null, DRPParamInfoModel dRPParam=null)
        {
            decimal tmpprice = 0;

            if (!string.IsNullOrEmpty(clickPrice))
            {
                if (!decimal.TryParse(clickPrice, out tmpprice))
                {
                    this.Close();
                    return;
                }

                clickPrice = tmpprice.ToString("F4");
            }

            InitializeComponent();

            wholeflag = wholeflag ?? "0";

            //建议价
            if (!string.IsNullOrEmpty(bargainyield))
            {
                if (!decimal.TryParse(bargainyield, out tmpprice))
                {
                    this.Close();
                    return;
                }

                bargainyield = tmpprice.ToString("F4");
            }

            qtyView.ValueChanged += QtyView_ValueChanged;

            datePicker.BlackoutDates.AddDatesInPast();

            this.Icon = LocalEnv.ServerInfo.AppImage;
            viewModel = new SendPointPriceModel();
            this.DataContext = viewModel;

            //拆分settle,判断是否含远期，+0 ，+1
            bool isirs = CommonUtils.IsIRS(security);
            if (isirs)
            {
                viewModel.IsShowIRS = Visibility.Collapsed;
            }

            List<string> listSettle = new List<string>();
            if (!isirs)
               listSettle = settle.Split(':').ToList();
            Dictionary<string, string> DictInSettle = new Dictionary<string, string>();
            foreach (string x in listSettle)
            {
                if (!DictInSettle.ContainsKey(x))
                {
                    DictInSettle.Add(x, x);
                }
            }

            string titlebidorofr = !isBid ? "TKN" : "GVN";
            Style s = Application.Current.MainWindow.FindResource("DailanGreenOrangeListBoxItemStyle") as Style;
            Thickness margin = new Thickness(0, 0, 2, 2);

            if (DictInSettle.ContainsKey("1"))
            {
                //是否含有 +0
                LstSettleBox.Items.Add(new ListBoxItem() { Content = "+0", Style = s, Width = 60, Height = 24, Margin = margin, ToolTip = titlebidorofr });
                viewModel.DictSettleBox.Add(0, "1");
                //判断是否只有一个 +0,隐藏+1
                if (DictInSettle.Count == 1)
                {
                    viewModel.NormalAddOneOpenedTag = "0";
                    SettlTypeCombox.Visibility = Visibility.Collapsed;
                    LstSettleBox.Margin = new Thickness(12, 0, 2, 2);
                }
            }

            // 判断是否是多远期
            // settle = "5.28+1";
            List<string> lstsettle = new List<string>();
            if (!isirs)
            {
                string[] tmpsettle = settle.Split(':');
                string tmp = string.Empty;
                for (int i = 0; i < tmpsettle.Length; i++)
                {
                    tmp = tmpsettle[i];
                    // 远
                    if (tmp.Contains("."))
                    {
                        if (!lstsettle.Contains(tmp))
                        {
                            lstsettle.Add(tmp);
                        }
                    }
                }
            }

            if (lstsettle.Count > 0)
            {
                //判断是否有+1
                if (!(DictInSettle.ContainsKey("2") || DictInSettle.ContainsKey("3") || DictInSettle.ContainsKey("2,3")))
                {
                    viewModel.NormalAddOneOpenedTag = "0";
                    SettlTypeCombox.Visibility = Visibility.Collapsed;
                    LstSettleBox.Margin = new Thickness(12, 0, 2, 2);
                }
            }


            // 如果混合 就是 +1
            if (lstsettle.Count > 0)
            {
                // 判断settle的值 +1 的情况
                if (settle.Contains("."))
                {
                    List<string> lstSettle = settle.Split(':').ToList();
                    for (int i = 0; i < lstSettle.Count; i++)
                    {
                        if (lstSettle[i].Contains("."))
                        {
                            LstSettleBox.Items.Add(new ListBoxItem() { Content = lstSettle[i], Style = s, Width = 60, Height = 24, Margin = margin, ToolTip = titlebidorofr });
                            viewModel.DictSettleBox.Add(viewModel.DictSettleBox.Count, lstSettle[i]);
                        }
                    }
                }
            }

            ////test 
            //LstSettleBox.Items.Add(new ListBoxItem() { Content = "+0", Style = s, Width = 60, Height = 24, Margin = margin, ToolTip = titlebidorofr });
            //viewModel.DictSettleBox.Add(0, "1");

            if (LstSettleBox.Items.Count > 5)
            {
                this.Height = 328;
                viewModel.MaxPriceAreaHeight = 258;
            }
            else
            {
                this.Height = 304;
                viewModel.MaxPriceAreaHeight = 234;
            }
            this.Activated += SendPointPriceView_Activated;

            viewModel.Init(this, security, isBid, clickPrice, clickAmount, settle, netPrice, source, wholeflag, isFrim, id, trsactTime, deliveryType, clearingMethod, quoteType, orderinfotip, clOrdID, venueTypeGW, iocTimeOut, info4, isDefaultDrp, partySubName, orderPrice, orderAmount, orderwholeflag, bargainyield, dRPParam);
        }

        /// <summary>
        /// 定向报价改价
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isCancelOrder">是否允许自动撤单</param>
        public SendPointPriceView(DataRowView drv)
        {
            InitializeComponent();
            datePicker.BlackoutDates.AddDatesInPast();

            //定向报价撤单
            STC.Common.Model.Fix.Message.OrderCancelRequest rs = Common.ViewModel.Drp.Utils.DRPTools.GetCancelOrder(drv);

            this.Icon = LocalEnv.ServerInfo.AppImage;
            viewModel = new SendPointPriceModel();
            this.DataContext = viewModel;

            string security = rs.SecurityID;
            string clickPrice = rs.Price;
            string clickAmount = rs.OrderQty;
            string settle = drv["SettleType"].ToString();
            string netPrice = drv["NetPrice"].ToString();
            string source= drv["Veneues"].ToString();
            string wholeflag = drv["WholeFlag"].ToString();
            wholeflag = wholeflag ?? "0";
            //反向
            bool isFrim = drv["FirmFlag"].ToString() == "0";
            string id = drv["QuoteID"].ToString();
            string trsactTime = drv["TransactTime"].ToString();
            string deliveryType = string.Empty;
            string clearingMethod = string.Empty;
            string quoteType = string.Empty;
            string orderinfotip = string.Empty;
            string venueTypeGW = drv["VenueTypeGW"].ToString();
            
            decimal tmpprice = 0;
            if (!string.IsNullOrEmpty(clickPrice))
            {
                if (!decimal.TryParse(clickPrice, out tmpprice))
                {
                    this.Close();
                    return;
                }

                clickPrice = tmpprice.ToString("F4");
            }

            //拆分settle,判断是否含远期，+0 ，+1
            bool isirs = CommonUtils.IsIRS(security);
            if (isirs)
            {
                viewModel.IsShowIRS = Visibility.Collapsed;
            }

            //拆分settle,判断是否含远期，+0 ，+1
            List<string> listSettle = new List<string>();
            if (!isirs)
            {
                listSettle = settle.Split(':').ToList();
            }
               
            Dictionary<string, string> DictInSettle = new Dictionary<string, string>();
            foreach (string x in listSettle)
            {
                if (!DictInSettle.ContainsKey(x))
                {
                    DictInSettle.Add(x, x);
                }
            }

            string titlebidorofr = rs.Side == "1" ? "TKN" : "GVN";
            Style s = Application.Current.MainWindow.FindResource("DailanGreenOrangeListBoxItemStyle") as Style;
            Thickness margin = new Thickness(0, 0, 2, 2);

            if (DictInSettle.ContainsKey("1"))
            {
                //是否含有 +0
                LstSettleBox.Items.Add(new ListBoxItem() { Content = "+0", Style = s, Width = 60, Height = 24, Margin = margin, ToolTip= titlebidorofr });
                viewModel.DictSettleBox.Add(0, "1");
                //判断是否只有一个 +0,隐藏+1
                if (DictInSettle.Count == 1)
                {
                    viewModel.NormalAddOneOpenedTag = "0";
                    SettlTypeCombox.Visibility = Visibility.Collapsed;
                    LstSettleBox.Margin = new Thickness(12, 0, 2, 2);
                }
            }

            // 判断是否是多远期
            // settle = "5.28+1";
            List<string> lstsettle = new List<string>();
            if (!isirs)
            {
                string[] tmpsettle = settle.Split(':');
                string tmp = string.Empty;
                for (int i = 0; i < tmpsettle.Length; i++)
                {
                    tmp = tmpsettle[i];
                    // 远
                    if (tmp.Contains("."))
                    {
                        if (!lstsettle.Contains(tmp))
                        {
                            lstsettle.Add(tmp);
                        }
                    }
                }
            }

            if (lstsettle.Count > 0)
            {
                //判断是否有+1
                if (!(DictInSettle.ContainsKey("2") || DictInSettle.ContainsKey("3") || DictInSettle.ContainsKey("2,3")))
                {
                    viewModel.NormalAddOneOpenedTag = "0";
                    SettlTypeCombox.Visibility = Visibility.Collapsed;
                    LstSettleBox.Margin = new Thickness(12, 0, 2, 2);
                }
            }


            // 如果混合 就是 +1
            if (lstsettle.Count > 0)
            {
                // 判断settle的值 +1 的情况
                if (settle.Contains("."))
                {
                    List<string> lstSettle = settle.Split(':').ToList();
                    for (int i = 0; i < lstSettle.Count; i++)
                    {
                        if (lstSettle[i].Contains("."))
                        {
                            LstSettleBox.Items.Add(new ListBoxItem() { Content = lstSettle[i], Style = s, Width = 60, Height = 24, Margin = margin, ToolTip = titlebidorofr });
                            viewModel.DictSettleBox.Add(viewModel.DictSettleBox.Count, lstSettle[i]);
                        }
                    }
                }
            }

            if (LstSettleBox.Items.Count > 5)
            {
                this.Height = 328;
                viewModel.MaxPriceAreaHeight = 258;
            }
            else
            {
                this.Height = 304;
                viewModel.MaxPriceAreaHeight = 234;
            }

            this.Activated += SendPointPriceView_Activated;

            string partySubName = string.Empty;
            string orderPrice = string.Empty;
            string orderAmount = string.Empty;
            string orderwholeflag = string.Empty;

            bool isBid = rs.Side == "1" ?  false : true;

            if (JsonSplit.IsJson(drv["Info1"].ToString()))
            {
                DqInfo1Model dim = JsonConvert.DeserializeObject<DqInfo1Model>(drv["Info1"].ToString());
                partySubName = dim.CnPartySubName;
                orderPrice = dim.Price;
                orderAmount = dim.Amount;
                orderwholeflag = dim.Wholeflag;
            }

            DRPParamInfoModel dRPParam = new DRPParamInfoModel() { OrderId = drv["Key"].ToString() };
            viewModel.Init(rs,this, security, isBid, clickPrice, clickAmount, settle, netPrice, source, wholeflag, isFrim, id, trsactTime, deliveryType, clearingMethod, quoteType, orderinfotip, null, venueTypeGW, null, null, true, partySubName, orderPrice, orderAmount, orderwholeflag, dRPParam);
        }

        private void SendPointPriceView_Activated(object sender, EventArgs e)
        {
            SetPriceFocus();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void PART_Button_Click(object sender, RoutedEventArgs e)
        {
            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                Button btn = (Button)sender;
                Grid g = btn.Parent as Grid;
                if (g != null)
                {
                    Popup popup = g.FindName("PART_Popup") as Popup;
                    popup.HorizontalOffset = 103;
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            if (tb.Name == "AmountTxt")
            {
                tag = "1";
            }
            else if ((tb.Name == "PriceTxt") || (tb.Name == "netPrice"))
            {
                tag = "2";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 1000;
            int min = -1;

            if (tag == "1")//量
            {
                max = 1000;
                min = 1;
            }
            else if (tag == "2")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            //else if (tag == "4")//随机渠道
            //{
            //    max = viewModel.RandomMax;
            //    min = 1;
            //}

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }

                if (tag == "1") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

                }
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (!CommonUtils.IsBrokerSecurity( viewModel.SendPriceItem.Security) && tb.Name == "netPrice")
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 3)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                
            }


            e.Handled = false;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            // 增加 ↑↓来改变价格
            if (tBox.Name.ToString() == "PriceTxt")
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.AddPriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    viewModel.ReducePriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }
            else if (tBox.Name.ToString() == "AmountTxt")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.AddAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    viewModel.ReduceAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
                //AmountTxt.Focus();
                //// 选择最后位数
                //int pos = AmountTxt.Text.Length;
                //if (pos > 0)
                //{
                //    int sellength = AmountTxt.Text.Length;
                //    AmountTxt.SelectionStart = 0;
                //    AmountTxt.SelectionLength = sellength;
                //}
                e.Handled = true;
                return;
            }
            // 判读是否是功能键
            // tag 改变用途
            string tag = string.Empty;


            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }


                // tag 改变用途
                if (tBox.Name == "AmountTxt")
                {
                    tag = "1";
                }
                else if ((tBox.Name == "PriceTxt") || (tBox.Name == "netPrice"))
                {
                    tag = "2";
                }
                else
                {
                    tag = tBox.Tag as string;
                }
                int max = 1000;
                int min = 0;

                if (tag == "1")//量
                {
                    max = 1000;
                    min = 0;
                }
                else if (tag == "2")//价格
                {
                    max = 9999;
                    min = -100;
                }
                else if (tag == "3")//过期时间
                {
                    max = 120;
                    min = 1;
                }
                //else if (tag == "4")//随机渠道
                //{
                //    max = viewModel.RandomMax;
                //    min = 1;
                //}

                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // this.Topmost = false;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void NumericUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            NumericUpDown tBox = sender as NumericUpDown;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
           
            // 判读是否是功能键
            // tag 改变用途
            string tag = string.Empty;


            // 功能键有效性检查
            string text = tBox.NumericValue.ToString();

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }
            }
        }

        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (this.viewModel != null)
            //{
            //    int intAmount = 0;
            //    if (int.TryParse(viewModel.GetQty(), out intAmount))
            //    {
            //        if (CommonUtils.IsBrokerSecurity(viewModel.Security))
            //        {
            //            if (intAmount % 1 != 0)
            //            {
            //                viewModel.SendPriceEnable = false;
            //            }
            //            else
            //            {
            //                viewModel.SendPriceEnable = intAmount >= 1 && intAmount <= 1000;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        viewModel.SendPriceEnable = false;
            //    }


            //    if (viewModel.SendPriceEnable)
            //    {
            //        viewModel.SendPriceEnable = viewModel.PriceValid;
            //    }

            //    viewModel.SetAmountTxtSelectIndex(AmountTxt.Text);
            //}
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //判断用户的按键是否为Enter
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                        viewModel.OkCommand.Execute(null);

                }
            }
        }


        private void bdSetleBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettlTypeCombox.IsDropDownOpen = !SettlTypeCombox.IsDropDownOpen;
            if (viewModel != null)
            {
                viewModel.NormalSettleBoxIndex = 0;
                viewModel.NormalAddOneOpenedTag = "1";
            }
        }

        private void DrpSetleBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DrpSettlTypeCombox.IsDropDownOpen = !DrpSettlTypeCombox.IsDropDownOpen;
            if (viewModel != null)
            {
                viewModel.DrpSettleBoxIndex = 0;
                viewModel.DrpAddOneOpenedTag = "1";
            }
        }

        public void SetPriceFocus()
        {
            //int sellength = AmountTxt.Text.Length;
            //AmountTxt.Focus();
            //AmountTxt.SelectionStart = 0;
            //AmountTxt.SelectionLength = sellength;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPointPriceModel))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPointPriceModel))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("GetFocusBorderBrush");
            }
        }

        private void AccountUserCbc_DropDownOpened(object sender, EventArgs e)
        {

        }

        private void AccountUserCbc_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        /// <summary>
        /// 点价模式改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbPointType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
           // cbSettle2.IsDropDownOpen = !cbSettle2.IsDropDownOpen;
        }
        /// <summary>
        /// 禁止粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidMinute_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 检查输入范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidMinute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = null;
            regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;

            string tag = tb.Tag as string;
            int max = 500;
            int min = (tag == "0") ? 0 : 1;

            if (tag == "0")// [0-60)
            {
                max = 59;
            }
            else if (tag == "1") // [0-500]
            {
                min = 0;
                max = 500;
            }
            else if (tag == "2") // (0-24]
            {
                max = 24;
            }
            else if (tag == "3") // [0-60)
            {
                min = 0;
                max = 59;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
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
        }

        #region DRP 最优价点击界面处理
        private void tbBestBid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPointPriceModel))
            {
                viewModel.ClickBestBidCommand.Execute(this);
            }
        }

        private void tbBestOfr_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPointPriceModel))
            {
                viewModel.ClickBestOfrCommand.Execute(this);
            }
        }

        private void tbBestBid_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPointPriceModel))
            {
                if (viewModel.ListPointTypeIndex == 1)
                {
                    tbBestBid.Cursor = Cursors.Hand;
                }
            }
        }

        private void tbBestBid_MouseLeave(object sender, MouseEventArgs e)
        {
            tbBestBid.Cursor = Cursors.Arrow;
        }

        private void tbBestOfr_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPointPriceModel))
            {
                if (viewModel.ListPointTypeIndex == 1)
                {
                    tbBestOfr.Cursor = Cursors.Hand;
                }
            }
        }

        private void tbBestOfr_MouseLeave(object sender, MouseEventArgs e)
        {
            tbBestOfr.Cursor = Cursors.Arrow;
        }
        #endregion


        private void QtyView_ValueChanged(object sender, RoutedEventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPointPriceModel))
            {
                string amount = viewModel.GetQtyViewModel().TrueAmount.ToString();
                //if (!string.IsNullOrEmpty(amount))
                //{
                //    if (viewModel.GetQtyViewModel().IsBrokerSecurity)
                //        amount = (decimal.Parse(amount) * 1000).ToString();
                //    else
                //        amount = (decimal.Parse(amount)).ToString();
                //}
                viewModel.AmountText = amount;
            }
        }

        #region 显示弹出式报价跟踪
        /// <summary>
        /// 打开报价追踪
        /// </summary>
        //// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Expand_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowWarnPopup();
        }

        /// <summary>
        /// 关闭报价追踪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Expand_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!btn_Expand.IsMouseOver && !this.AnsAttention.IsMouseOver)
            {
                CollapsedWarnPopup();
            }
        }

        private void ShowWarnPopup()
        {
            MM_Duty_pop.Visibility = Visibility.Visible;
            MM_Duty_pop.IsOpen = true;
            AnsAttention.QueryLog((this.DataContext as SendPointPriceModel).OrderId.Replace("DQLimit_", ""),"2");
        }

        private void CollapsedWarnPopup()
        {
            MM_Duty_pop.IsOpen = false;
            MM_Duty_pop.Visibility = Visibility.Collapsed;
        }

        private void Ans_status_tb_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapsedWarnPopup();
        }
        #endregion
    }
}

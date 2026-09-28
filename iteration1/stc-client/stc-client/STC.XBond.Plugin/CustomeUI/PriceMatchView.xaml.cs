using STC.Common.Handler.FinQ;
using STC.Common.Handler.QQ;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using STC.XBond.Plugin.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// PriceMatchView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceMatchView : Window
    {
        private PriceMatchViewModel _priceMatchViewModel = null;

        public PriceMatchView()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;

            if (!LocalEnv.ConfInfo.IsQQ)
            {
                StartQQBtn.Visibility = Visibility.Collapsed;
                StartQQ2Btn.Visibility = Visibility.Collapsed;
            }

            this.dgData.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.MyOrders_dataGrid_MouseLeftButtonDown), true);

            this.Closed += PriceMatchView_Closed;
            this.Unloaded += PriceMatchView_Unloaded;
        }

        private void PriceMatchView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Closed -= PriceMatchView_Closed;
            this.Unloaded -= PriceMatchView_Unloaded;

            if (_priceMatchViewModel != null)
                _priceMatchViewModel.Clear();
        }

        private void PriceMatchView_Closed(object sender, EventArgs e)
        {
            _priceMatchViewModel.Clear();
            this.Close();
        }

        public PriceMatchView(MsgPo msg)
        {
            InitializeComponent();

            if (!LocalEnv.ConfInfo.IsQQ)
            {
                StartQQBtn.Visibility = Visibility.Collapsed;
                StartQQ2Btn.Visibility = Visibility.Collapsed;
            }


            this.Unloaded += PriceMatchView_Unloaded;
            this.Closed += PriceMatchView_Closed;

            this.Icon = LocalEnv.ServerInfo.AppImage;

            _priceMatchViewModel = new PriceMatchViewModel();
            this.DataContext = _priceMatchViewModel;
            _priceMatchViewModel.Init(this, msg);
            if (!LocalEnv.ConfInfo.EnableIOC) 
            {
                btnSendPrice.Visibility = Visibility.Collapsed;
            }
        }
        

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            GridData gridData = tb.DataContext as GridData;

            Regex regex = new Regex("^[0-9]+$");

            bool isBroker = CommonUtils.IsBrokerSecurity(gridData.Symbol);
            if (gridData != null && !isBroker)
            {
                regex = new Regex("^[0-9.-]+$");
            }

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string maxAmount = string.Empty;
            string tag = tb.Tag as string;

            decimal min = 0;
            if (tag.Equals("1"))
                maxAmount = _priceMatchViewModel == null ? string.Empty : _priceMatchViewModel.OutChannelNum;
            else if (tag.Equals("2"))
            {
                maxAmount = "10000000";
                min = 1;
            }
            else if (tag.Equals("3"))
            {
                maxAmount = "10000000";
                min = 1;
            }

            if (!isBroker)
            {
                min = 0;
            }

            decimal max = 20;

            decimal tem = 0;
            if (decimal.TryParse(maxAmount, out tem))
                max = tem;


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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8)
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

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell))
                cell = VisualTreeHelper.GetParent(cell);

            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;

            string head = string.Empty;
            if (targetCell != null && targetCell.Column.Header != null)
            {
                head = targetCell.Column.Header.ToString();

                if (head == "点价量(万元)")
                    return;

            }

            if (head == "来源")
            {
                StartQQBtn_Click(null, null);
            }
            else
            {
                if (!CommonUtils.IsBDBAppName())
                {
                    DataGrid dg = sender as DataGrid;
                    _priceMatchViewModel.OnDoubleClick(dg.SelectedItem);
                }
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.ComputeAmount();
            }
        }

        private void CheckBox_All_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.CheckBoxAllValueChange();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.ChangeAmount(tb.Text);
            }
        }

        private void CheckBox_All_Checked(object sender, RoutedEventArgs e)
        {
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.ComputeAmount();
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (LocalEnv.ConfInfo.IsQQ)
            {
                StartQQBtn.Visibility = Visibility.Collapsed;
                StartQQ2Btn.Visibility = Visibility.Collapsed;
                GridData item = dgData.SelectedItem as GridData;
                if (item != null && item.VenueTypeGW == "INNER")
                {
                    StartQQBtn.Visibility = Visibility.Visible;
                    StartQQ2Btn.Visibility = Visibility.Visible;
                    //string receiver = CommonUtils.GetQQNumber(item.PartySubID);
                    //if (!string.IsNullOrEmpty(receiver))
                    //{
                    //    if (!string.IsNullOrEmpty(receiver))
                    //    {
                    //        StartQQBtn.IsEnabled = true;
                    //        StartQQ2Btn.IsEnabled = true;
                    //    }
                    //    else
                    //    {
                    //        StartQQBtn.IsEnabled = false;
                    //        StartQQ2Btn.IsEnabled = false;
                    //    }
                    //}
                }
            }
        }

        private void StartQQBtn_Click(object sender, RoutedEventArgs e)
        {
            Sub_DoStartQQCommand(false);
        }

        private void StartQQ2Btn_Click(object sender, RoutedEventArgs e)
        {
            Sub_DoStartQQCommand(true);
        }

        private void Sub_DoStartQQCommand(bool is_send_direct)
        {
            if (!LocalEnv.ConfInfo.IsQQ)
            {
                return;
            }

            GridData item = dgData.SelectedItem as GridData;
            if (item.VenueTypeGW == "INNER")
            {
                string receiver = CommonUtils.GetQQNumber(item.PartySubID);
                string remainingTime = string.Empty;
                string symbol = item.Symbol;
                string securityid = item.Symbol;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityid);
                if (bondsInfoPo != null && string.IsNullOrEmpty(remainingTime))
                {
                    remainingTime = bondsInfoPo.RemaingingTime;
                    symbol = bondsInfoPo.Short_Name;
                }
                string side = item.Side == "1" ? "Bid" : "Ofr";
                string price_temp = item.Price;
                string price = string.IsNullOrEmpty(price_temp) ? string.Empty : $" {price_temp}";
                string qty_temp = item.Amount;
                string qty = string.IsNullOrEmpty(qty_temp) ? string.Empty : $" {qty_temp}w";
                string settlType_temp = item.Settle;
                string settlType = string.Empty;
                if (settlType_temp != "T+1"
                    || settlType_temp != "T+1(不限)")
                {
                    settlType = " " + settlType_temp;
                }
                string message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}";

                if (!string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                {
                    ThreadPoolHelper.InvokeThreadPool(o =>
                    {
                        SendDesktopFinQReq request = new SendDesktopFinQReq();
                        request.Username = LocalEnv.UserInfo.UserName;
                        request.sender = LocalEnv.UserInfo.QQNumber;
                        request.receiver = receiver;
                        request.send_direct = is_send_direct;
                        request.clear_edit = true;
                        request.channel = "0";
                        request.message = is_send_direct ? $"您的报价【{message}】，还能再好吗？" : message;
                        Common.Model.Fix.Message.PResult result = FinQHandler.Instance.SendDesktopFinQ(request);

                        if (result != null && !result.IsSuccess())
                        {
                            string errorMsg = "失败";
                            if (!string.IsNullOrEmpty(result.Msg))
                                errorMsg = result.Msg;
                            CommonUtils.Prompt2Second("发起客户单聊QQ", errorMsg);
                        }
                    });
                    ClipboardHelper.CopyToClipboard(message);
                }
                else
                {
                    if (string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                        CommonUtils.Prompt2Second("发起客户单聊QQ", "请联系管理员设置本用户对应QQ号码!");
                    else if (string.IsNullOrEmpty(receiver))
                        CommonUtils.Prompt2Second("发起客户单聊QQ", "找不到客户QQ号");
                }
            }
            else if (item.Venue == "TPSC"
                || item.Venue == "CNEX"
                || item.Venue == "CCTB"
                || item.Venue == "PTCN"
                || item.Venue == "CTTJ"
                || item.Venue == "UEDA")
            {
                List<dynamic> paramList = new List<dynamic>();
                dynamic d = new System.Dynamic.ExpandoObject();
                d.side = item.Side;
                d.isCtrl = false;
                d.isNeedSource = false;
                d.isBestInvoke = false;
                d.html = string.Empty;
                d.plainTxt = string.Empty;
                paramList.Add(d);
                Sub_CopyPointOrderData(paramList, item);
            }
        }

        int i = 0;
        bool isDouble = false;
        private void MyOrders_dataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            i += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; MyOrders_UIElement_Click(sender, e); isDouble = false; };
            timer.IsEnabled = true;
            if (i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                isDouble = true;

                // 双击
                
            }
        }

        private void MyOrders_UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            //ignore
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side"></param>
        /// <param name="type">1:join 2:报对价</param>
        public void Sub_CopyPointOrderData(List<dynamic> paramList, GridData gData)
        {
            if (paramList.Count == 0)
                return;

            dynamic condition = paramList[0];

            string html_onlyCopy;
            string plainTxt_onlyCopy;

            string s = string.Empty;
            string qty = string.Empty;
            string px = string.Empty;

            string securityId = gData.Symbol;
            string symbolName = string.Empty;
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            if (bondsInfo != null)
            {
                symbolName = bondsInfo.Short_Name;
            }

            if (condition.side == "1")//GVN
            {
                s = "GVN";
            }
            else if (condition.side == "2")//TKN
            {
                s = "TKN";
            }

            qty = gData.Amount;
            px = gData.Price;

            string htmlPx = string.Empty;
            string txtPx = string.Empty;
            if (px.Length > 0)
            {
                htmlPx = $"&#9;{px}";
                txtPx = $" {px}";
            }

            string htmlQty = string.Empty;
            string txtQty = string.Empty;
            if (qty.Length > 0)
            {
                htmlQty = $"&#9;{qty}w";
                txtQty = $" {qty}w";
            }

            html_onlyCopy = $"<p>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>";
            plainTxt_onlyCopy = $"{s} {securityId} {symbolName}{txtPx}{txtQty}";

            condition.html_onlyCopy = html_onlyCopy;
            condition.plainTxt_onlyCopy = plainTxt_onlyCopy;
            condition.Qty = qty;

            //复制
            ClipboardHelper.CopyToClipboard(html_onlyCopy, plainTxt_onlyCopy);

            if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
            {
                SendHttpQQ(condition.side, null, plainTxt_onlyCopy.ToString(), gData);
            }
            else
            {
                CommonUtils.Prompt2Second("复制", "复制成功");
            }
        }
       
        public STC.Common.Model.QQ.DataMessage GenDataMessage(string side, string type, GridData gData)
        {
            string[,] vol_delivery = null;
            string side_txt = side == "1" ? "bid" : "ofr";
            string reserve_side = side == "1" ? "ofr" : "bid";
            string source = ComboSourceUtil.Instance.FindEnum(gData.Venue, "venues_en_tonumber");
            string join = string.Empty;
            string marketText;
            string settleType;

            if (type == "1")
            {
                join = $"join_{side_txt}";
            }
            else if (type == "2")
            {
                join = $"join_{reserve_side}";
            }

            string px;
            string qty;

            string securityId = gData.Symbol;
            string symbolName = string.Empty;
            string bond_type = string.Empty;
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            if (bondsInfo != null)
            {
                symbolName = bondsInfo.Short_Name;
                bond_type = bondsInfo.SECURITY_TYPE;
            }

            marketText = string.Empty;
            settleType = ComboSourceUtil.Instance.FindEnum(gData.Settle, "settletype_revert");
            px = gData.Price;
            qty = gData.Amount;

            if (!string.IsNullOrEmpty(marketText))
            {
                string output = Regex.Replace(marketText, @"\(.*?\)", string.Empty);
                string[] splitArr = output.Split(CommonConstants.PLUS_CHAR);
                string[] splitArr1 = settleType.Split(CommonConstants.COLON_CHAR);
                if (splitArr.Length > 0 && splitArr.Length == splitArr1.Length)
                {
                    vol_delivery = new string[splitArr.Length, 2];
                    for (int i = 0; i < splitArr.Length; i++)
                    {
                        vol_delivery[i, 0] = splitArr[i];
                        vol_delivery[i, 1] = ResulfApiHelper.ConvertSettleType(splitArr1[i]);
                    }
                }
            }
            else
            {
                vol_delivery = new string[1, 2];
                vol_delivery[0, 0] = qty;
                vol_delivery[0, 1] = ResulfApiHelper.ConvertSettleType(settleType);
            }

            STC.Common.Model.QQ.DataMessage dm = new STC.Common.Model.QQ.DataMessage();
            dm.id = Guid.NewGuid().ToString("N");
            dm.comp_id = source;
            dm.code = securityId;
            dm.name = symbolName;
            dm.side = side_txt;
            dm.join = join;
            dm.price = (px == CommonConstants.NO_PRICE || px == CommonConstants.NO_BROKER) ? string.Empty : px;
            dm.vol_delivery = vol_delivery;
            dm.bond_type = bond_type;
            dm.price_type = "3";
            return dm;
        }

        private void SendHttpQQ(string side, string type, string content, GridData gData)
        {
            SendFinQFriendReq qqsmm = new SendFinQFriendReq();
            qqsmm.content = content;
            qqsmm.data = GenDataMessage(side, type, gData);
            qqsmm.fromQB = LocalEnv.UserInfo.UserName;
            qqsmm.openChat = true;
            qqsmm.fromQBAccountID = LocalEnv.UserInfo.UserId;

            PResult result = QQHandler.Instance.SendQQ(qqsmm);
            if (result != null && result.IsSuccess())
            {
                CommonUtils.Prompt2Second("金融Q", "已成功推送至金融Q");
            }
            else
            {
                CommonUtils.Prompt2Second("金融Q", (result == null || string.IsNullOrEmpty(result.Msg)) ? "发送金融Q失败" : result.Msg);
            }
        }
    }
}

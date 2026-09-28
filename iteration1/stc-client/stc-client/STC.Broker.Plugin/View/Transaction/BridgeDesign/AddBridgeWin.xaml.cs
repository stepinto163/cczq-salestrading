
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Handler.CRM;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Frame.UI;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// AddBridgeWin.xaml 的交互逻辑
    /// 加桥面板
    /// </summary>
    public partial class AddBridgeWin : Window
    {
        private UserBridge CurrentBridge = new UserBridge();
        private UserBridge oldBridge = new UserBridge();
        public event Action<object, UserBridge, UserBridge, bool, bool> OnAddBridge;
        public AddBridgeWin()
        {
            InitializeComponent();
            //this.Icon = LocalEnv.ServerInfo.AppImage;

            tbRemarks.Document.Blocks.Clear();
        }

        public void Init(UserBridge bridge, bool isUpdate)
        {
            if (isUpdate)
            {
                tbInst.IsEnabled = false;
                tbInstFee.IsEnabled = false;
            }

            if (bridge != null)
            {
                CurrentBridge = UserBridge.Copy(bridge);
                oldBridge = UserBridge.Copy(bridge);
                ///桥
                TextChangeFlag = false;
                tbInst.Text = string.Format("{0}({1})", bridge.bankInstitutionname, bridge.bankTradername);

                ///计费
                TextChangeFlag = false;
                tbInstFee.Text = string.IsNullOrEmpty(bridge.feeInstitutionname) ? bridge.feeTradername : $"{bridge.feeTradername}({bridge.feeInstitutionname})";
                ///联系方式
                tbContactType.Document.Blocks.Clear();
                if (!string.IsNullOrEmpty(bridge.contactType))
                {
                    Run run = new Run(bridge.contactType);
                    Paragraph p = new Paragraph();
                    p.Inlines.Add(run);
                    tbContactType.Document.Blocks.Add(p);
                }
                ///备注
                tbRemarks.Document.Blocks.Clear();
                if (!string.IsNullOrEmpty(bridge.remark))
                {
                    Run run = new Run(bridge.remark);
                    Paragraph p = new Paragraph();
                    p.Inlines.Add(run);
                    tbRemarks.Document.Blocks.Add(p);
                }
                ///发给
                tbSendTo.Text = bridge.sendto;
                ///渠道
                if (!string.IsNullOrEmpty(bridge.channel))
                {
                    if (bridge.channel.Equals("1"))
                        lbTalk.IsSelected = true;
                    else if (bridge.channel.Equals("2"))
                        lbRequest.IsSelected = true;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lbChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbTalk.IsSelected)
                lbRequest.IsSelected = false;
            else
                lbRequest.IsSelected = true;
        }

        /// <summary>
        /// 更新桥设置，该用户添加的桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbInst.Text))
            {
                tbWarn.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(tbInstFee.Text))
            {
                tbWarnFee.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrEmpty(CurrentBridge.bankTraderid) && string.IsNullOrEmpty(CurrentBridge.bankInstitutionid))
            {
                Close();
                return;
            }
            if (string.IsNullOrEmpty(CurrentBridge.feeInstitutionid) && string.IsNullOrEmpty(CurrentBridge.feeTraderid))
            {
                Close();
                return;
            }
            bool isUpdate = false;
            foreach (UserBridge bridge in UserBridgeHandler.Instance.Bridges)
            {
                ///此处应该用ID判断。由于简称的存在会导致添加两座一样的桥
                bool same = bridge.bankInstitutionid == CurrentBridge.bankInstitutionid && bridge.bankTraderid == CurrentBridge.bankTraderid;
                //当前桥已经存在
                if (same)
                {
                    if (tbInst.IsEnabled)
                    {
                        SysWarnGUI warGUI = new SysWarnGUI();
                        warGUI.setWarnMsg("有相同的桥");
                        warGUI.Owner = this;
                        warGUI.Show();
                        return;
                    }
                    else
                    {
                        //更新桥
                        isUpdate = true;
                        break;
                    }
                }
            }
            //发给修改的话，需要重置桥下单子
            bool needReset = CurrentBridge.sendto == null ? true : CurrentBridge.sendto.Equals(tbSendTo.Text);
            CurrentBridge.channel = lbTalk.IsSelected ? "1" : lbRequest.IsSelected ? "2" : string.Empty;
            CurrentBridge.sendto = tbSendTo.Text;
            TextRange textRange = new TextRange(tbRemarks.Document.ContentStart, tbRemarks.Document.ContentEnd);
            //去掉备注的头部和尾部的换行回车字符
            CurrentBridge.remark = textRange.Text.Trim(new char[] { '\r', '\n' });
            textRange = new TextRange(tbContactType.Document.ContentStart, tbContactType.Document.ContentEnd);
            //联系方式
            CurrentBridge.contactType = textRange.Text.Trim(new char[] { '\r', '\n' });
            //broker
            CurrentBridge.partySubid = LocalEnv.UserInfo.UserId;
            CurrentBridge.partySubname = LocalEnv.UserInfo.UserName;
            ThreadPool.QueueUserWorkItem(o =>
            {

                if (isUpdate)
                {
                    UserBridgeHandler.Instance.UpdateBridge(CurrentBridge);

                }
                else
                {
                    UserBridgeHandler.Instance.AddBridge(CurrentBridge);
                }

                this.Dispatcher.Invoke(() =>
                    {
                        OnAddBridge?.Invoke(this, CurrentBridge, oldBridge, isUpdate, !needReset && isUpdate);

                        this.Close();
                    });
            });
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tbInst_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (popInst.IsOpen)
                    dgTraderA_MouseDoubleClick(null, null);
            }
        }

        private void tbInst_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void tbInst_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextChangeFlag为false 返回
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            if (!TextChangeFlag)
            {
                TextChangeFlag = true;
                return;
            }

            if (!string.IsNullOrEmpty(tbInst.Text))
            {
                List<DataGridDataPo> dataList = Utils.CommonUtils.GetContactByContactName(tbInst.Text, 100);
                if (dataList.Count <= 0)
                    dataList = Utils.CommonUtils.GetContactByInstName(tbInst.Text, 100, string.Empty);
                if (dataList.Count > 0)
                {
                    dgTraderA.DataContext = dataList;
                    popInst.IsOpen = true;
                    dgTraderA.SelectedIndex = 0;
                }
                else
                {
                    CurrentBridge.bankTraderid = string.Empty;
                    CurrentBridge.bankTradername = string.Empty;
                    CurrentBridge.bankInstitutionid = string.Empty;
                    CurrentBridge.bankInstitutionname = string.Empty;

                    dgTraderA.DataContext = null;
                    popInst.PlacementTarget = tbInst;
                    popInst.IsOpen = false;
                }
            }
            else
            {
                dgTraderA.DataContext = null;
                popInst.IsOpen = false;
            }
            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;
            LogHelper.Info("example2 time {0}", ts2.TotalMilliseconds);
        }

        private string BizTypeConvert(string ids)
        {
            string result = string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            string[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in idArr)
            {
                if (id == "1")
                    //result += string.IsNullOrEmpty(result) ? "BCO" : ",BCO";
                    stringBuilder.Append(",BCO");
                else if (id == "2")
                    //result += string.IsNullOrEmpty(result) ? "BNC" : ",BNC";
                    stringBuilder.Append(",BNC");
                else if (id == "11")
                    //result += string.IsNullOrEmpty(result) ? "ABS" : ",ABS";
                    stringBuilder.Append(",ABS");
                else if (id == "12")
                    //result += string.IsNullOrEmpty(result) ? "NCD" : ",NCD";
                    stringBuilder.Append(",NCD");
            }
            result = stringBuilder.ToString().Trim(',');
            return result;
        }

        private bool TextChangeFlag = false;
        private void tbInst_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = true;
        }

        private void tbInst_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = false;
        }

        private void dgTraderA_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dv = dgTraderA.SelectedItem as DataGridDataPo;
            popInst.IsOpen = false;

            if (dv != null)
            {

                CurrentBridge.bankTraderid = dv.IdbKey;
                CurrentBridge.bankTradername = dv.TraderName;
                CurrentBridge.bankInstitutionid = dv.InstIdbKey;
                CurrentBridge.bankInstitutionname = dv.InstName;
                ///TextChangeFlag置为false在对tbInst进行赋值，强制退出其tbInst_TextChanged事件处理
                TextChangeFlag = false;
                ///此处赋值，会触发tbInst_TextChanged文本改变事件，从而导致pop弹出。
                tbInst.Text = dv.InstTraderName;

            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //btnConfirm_Click(null, null);
        }

        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private void tbInstFee_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (popInstFee.IsOpen)
                    dgInstFee_MouseDoubleClick(null, null);
            }

        }

        private void tbInstFee_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgInstFee.SelectedIndex > 0)
                    dgInstFee.SelectedIndex--;
                dgInstFee.ScrollIntoView(dgInstFee.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgInstFee.SelectedIndex < dgInstFee.Items.Count - 1)
                    dgInstFee.SelectedIndex++;
                dgInstFee.ScrollIntoView(dgInstFee.SelectedItem);
            }
        }

        private void tbInstFee_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TextChangeFlag)
            {
                TextChangeFlag = true;
                return;
            }


            if (!string.IsNullOrEmpty(tbInstFee.Text))
            {
                List<DataGridDataPo> dataList = Utils.CommonUtils.GetContactByContactName(tbInstFee.Text, 100);
                if (dataList.Count <= 0)
                    dataList = Utils.CommonUtils.GetContactByInstName(tbInstFee.Text, 100, string.Empty);
                if (dataList.Count > 0)
                {
                    dgInstFee.DataContext = dataList;
                    popInstFee.IsOpen = true;
                    dgInstFee.SelectedIndex = 0;
                }
                else
                {
                    CurrentBridge.feeInstitutionid = string.Empty;
                    CurrentBridge.feeInstitutionname = string.Empty;
                    CurrentBridge.feeTraderid = string.Empty;
                    CurrentBridge.feeTradername = string.Empty;
                    dgInstFee.DataContext = null;
                    popInstFee.PlacementTarget = tbInstFee;
                    popInstFee.IsOpen = false;
                }
            }
            else
            {
                dgInstFee.DataContext = null;
                popInstFee.IsOpen = false;
            }
        }

        private void tbInstFee_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = true;

        }

        private void tbInstFee_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = false;
        }

        private void dgInstFee_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dv = dgInstFee.SelectedItem as DataGridDataPo;
            popInstFee.IsOpen = false;

            if (dv != null)
            {

                CurrentBridge.feeInstitutionid = dv.IdbKey;
                CurrentBridge.feeInstitutionname = dv.TraderName;
                CurrentBridge.feeTraderid = dv.InstIdbKey;
                CurrentBridge.feeTradername = dv.InstName;
                ///TextChangeFlag置为false在对tbInst进行赋值，强制退出其tbInst_TextChanged事件处理
                TextChangeFlag = false;
                ///此处赋值，会触发tbInst_TextChanged文本改变事件，从而导致pop弹出。
                tbInstFee.Text = dv.InstTraderName;

            }
        }
    }
}

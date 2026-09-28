using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Handler.PT;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.UI;
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
using System.Windows.Threading;

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// FixedSendStandUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class FixedSendStandUserControl : UserControl
    {
        /// <summary>
        ///当前鼠标获取的数据项
        /// </summary>
        private TransactionOrdermModel _indexOrder = null;

        public event Action<bool> ActionClick;


        /// <summary>
        /// 原来选中渠道
        /// </summary>
        private ComboxBaseModel _oldComboxBaseModel = null;

        /// <summary>
        /// 选择变化标识
        /// </summary>
        private bool selectChangeFlag = false;

        private bool isCanSendDir = true;
        public FixedSendStandUserControl()
        {
            InitializeComponent();
            this.Loaded += SendStandUserControl_Loaded;
            this.Unloaded += SendStandUserControl_Unloaded;
        }

        private void SendStandUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            st_PayInstChange.AfterSearch -= St_PayInstChange_AfterSearch;
            sp_SendDir.MouseDown -= Sp_SendDir_MouseDown;
            cb_Channel.PreviewMouseDown -= Cb_Channel_PreviewMouseDown;
            cb_Channel.SelectionChanged -= Cb_Channel_SelectionChanged;
        }

        private void SendStandUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            st_PayInstChange.AfterSearch += St_PayInstChange_AfterSearch;
            sp_SendDir.MouseDown += Sp_SendDir_MouseDown;
            cb_Channel.PreviewMouseDown += Cb_Channel_PreviewMouseDown;
            cb_Channel.SelectionChanged += Cb_Channel_SelectionChanged;
        }

        private void Tb_remarks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void Cb_Channel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TransactionOrdermModel orderDetailPo = this.DataContext as TransactionOrdermModel;
                if (orderDetailPo == null)
                {
                    return;
                }
                if (selectChangeFlag)
                {
                    if (orderDetailPo.ComboxBaseSelect != null && orderDetailPo.ComboxBaseSelect.ValueMemberPath == orderDetailPo.IssueChannel)
                    {
                        return;
                    }

                    if (!Check(orderDetailPo))
                    {
                        orderDetailPo.ComboxBaseSelect = _oldComboxBaseModel;
                        return;
                    }
                    ExecutionReport execOrderPo = new ExecutionReport();
                    execOrderPo.ExecID = orderDetailPo.ExecID;
                    execOrderPo.CommandType = Convert.ToString((int)ECommandType.UpdateSendChannel);
                    execOrderPo.IssueChannel = orderDetailPo.ComboxBaseSelect?.ValueMemberPath;
                    execOrderPo.CloseById = LocalEnv.UserInfo.UserId;
                    //OrderPo orderPo = OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId);
                    await UpdateExecOrderData(execOrderPo);
                    // OrderHelper.ChangeOrder(OrderHelper.GetUpdateOrderPoNew(orderDetailPo.ComboxBaseSelect?.ValueMemberPath, ECommandType.UpdateSendChannel, orderDetailPo.OrderId, orderDetailPo.ExecId));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                selectChangeFlag = false;
            }
        }

        private void Cb_Channel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TransactionOrdermModel orderDetailPo = this.DataContext as TransactionOrdermModel;
            if (orderDetailPo == null)
            {
                return;
            }
            _oldComboxBaseModel = orderDetailPo.ComboxBaseSelect;
            selectChangeFlag = true;
        }

        private async void Sp_SendDir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null)
            {
                return;
            }
            Border border = null;
            UIElementCollection uIElement = grid.Children;
            if (uIElement.Count > 0)
            {
                foreach (var item in uIElement)
                {
                    if (item is Border)
                    {
                        border = item as Border;
                        border.Visibility = Visibility.Visible;
                        break;
                    }
                }
            }

            if (!isCanSendDir)
            {
                LogHelper.Info("300ms只能点击一次");
                return;
            }
            Dispatcher.Invoke(() =>
            {
                isCanSendDir = false;
            });
            int cout = 3;
            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += new EventHandler(
               (object sender1, EventArgs e1) =>
               {
                   Dispatcher.Invoke(() =>
                   {
                       cout--;
                       if (cout <= 0)
                       {
                           isCanSendDir = true;
                           if (border != null)
                           {
                               border.Visibility = Visibility.Collapsed;
                           }
                           _timer.Stop();
                       }
                   });
               });
            _timer.Start();

            e.Handled = true;

            TransactionOrdermModel orderDetailPo = this.DataContext as TransactionOrdermModel;
            if (orderDetailPo == null)
            {
                return;
            }
            string issueDirStr = "1";
            if (orderDetailPo.IssueDir == "1")
            {
                issueDirStr = "2";
            }
            else
            {
                issueDirStr = "1";
            }
            if (!Check(orderDetailPo))
            {
                return;
            }
            ExecutionReport execOrderPo = new ExecutionReport();
            execOrderPo.ExecID = orderDetailPo.ExecID;
            execOrderPo.CommandType = Convert.ToString((int)ECommandType.UpdateSendDirection);
            execOrderPo.IssueDir = issueDirStr;
            execOrderPo.CloseById = LocalEnv.UserInfo.UserId;
            //OrderPo orderPo = OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId);
            await UpdateExecOrderData(execOrderPo);
        }


        /// <summary>
        /// 检查成交单是否可以修改
        /// </summary>
        /// <param name="transactionOrderm"></param>
        /// <returns></returns>
        public bool Check(TransactionOrdermModel transactionOrderm)
        {
            if (!("G".Equals(transactionOrderm.OrderStatus) || "I".Equals(transactionOrderm.OrderStatus)))
                return false;
            //线下录入在brokerB未确认的时候，brokerB不可以修改
            if (("I".Equals(transactionOrderm.OrderStatus) && LocalEnv.UserInfo.IsMyself(transactionOrderm.BrokerIdB)))
                return false;
            if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdA) && !SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdB))
                return false;
            return true;
        }


        private async void St_PayInstChange_AfterSearch(object obj, object data)
        {
            try
            {
                TransactionOrdermModel orderDetailPo = obj as TransactionOrdermModel;
                orderDetailPo.PayInstChangeVisible = Visibility.Collapsed;
                orderDetailPo.PayInstTextVisible = Visibility.Visible;
                DataGridDataPo dv = data as DataGridDataPo;
                if (dv != null)
                {
                    if (orderDetailPo.IssueTraderId != dv.IdbKey)
                    {
                        ExecutionReport execOrderPo = new ExecutionReport();
                        execOrderPo.ExecID = orderDetailPo.ExecID;
                        execOrderPo.CommandType = Convert.ToString((int)ECommandType.UpdateIssuer);
                        execOrderPo.IssueTraderId = dv.IdbKey;
                        execOrderPo.CloseById = LocalEnv.UserInfo.UserId;
                        //OrderPo orderPo = OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId);
                        await UpdateExecOrderData(execOrderPo);
                        //OrderHelper.ChangeOrder(OrderHelper.GetUpdateOrderPoNew(dv.IdbKey, ECommandType.UpdateIssuer, orderDetailPo.OrderId, orderDetailPo.ExecId));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                LogHelper.Error(ex.StackTrace);
                LogHelper.Error(obj);
                LogHelper.Error(data);
            }
            var win = Window.GetWindow(this);
            if (win != null)
                win.Focus();
        }


        public async Task<PResult> UpdateExecOrderData(ExecutionReport req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                req.CloseById = LocalEnv.UserInfo.UserId;
                reply = GRHandler.Instance.ExecOrderUpdate(req);
                if (reply != null && reply.Code != "0000")
                {
                    ShowMessage(reply.Msg);
                }
            });

            return reply;
        }

        private void ShowMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.ShowDialog();

            }));
        }

        private void tb_PayInstInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ActionClick?.Invoke(false);
                e.Handled = true;
                TransactionOrdermModel orderDetailPo = this.DataContext as TransactionOrdermModel;
                if (orderDetailPo == null)
                {
                    return;
                }
                if (!Check(orderDetailPo))
                    return;
                orderDetailPo.PayInstChangeVisible = Visibility.Visible;
                orderDetailPo.PayInstTextVisible = Visibility.Collapsed;

                st_PayInstChange.SecurityType = orderDetailPo.SecurityType;
                st_PayInstChange.Data = orderDetailPo;
                st_PayInstChange.SetSearchTxt(orderDetailPo.IssueTraderShowNameDisplay.Replace("发", "").Replace(" ", ""));
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                TransactionOrdermModel transactionPanelViewModel = border.DataContext as TransactionOrdermModel;
                if (transactionPanelViewModel != null)
                {
                    transactionPanelViewModel.IsMouseover = false;
                    if (transactionPanelViewModel.OrderStatus == "G")
                    {
                        border.BorderThickness = new Thickness(1);
                        Cursor = Cursors.Hand;
                        border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(0);

                    }
                }
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            TransactionOrdermModel operationOrder = this.DataContext as TransactionOrdermModel;
            if (operationOrder == null)
            {
                return;
            }
            _indexOrder = operationOrder;

            if (_indexOrder != null && _indexOrder.OrderStatus.Equals("G"))
            {

                if (!SystemParamHandler.IsMyModifys(_indexOrder.BrokerNameA) && !SystemParamHandler.IsMyModifys(_indexOrder.BrokerNameB))
                    return;

                Border border = sender as Border;
                if (border != null)
                {
                    TransactionOrdermModel transactionPanelViewModel = border.DataContext as TransactionOrdermModel;
                    if (transactionPanelViewModel != null)
                    {
                        transactionPanelViewModel.IsMouseover = true;
                        if (transactionPanelViewModel.OrderStatus == "G")
                        {
                            border.BorderThickness = new Thickness(1);
                            Cursor = Cursors.Hand;
                            border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#238F7E"));
                        }
                        else
                        {
                            border.BorderThickness = new Thickness(0);
                        }
                    }
                    //border.BorderBrush = (Brush)FindResource("ColorBrush.Yellow5");
                }
            }
            else
                Cursor = null;
        }

        private async void txt_Remarks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string remarks = "";


                TextBox textBox = sender as TextBox;
                if (textBox == null)
                {
                    return;
                }
                remarks = textBox.Text.Trim();
                TransactionOrdermModel orderDetailPo = this.DataContext as TransactionOrdermModel;
                if (orderDetailPo == null)
                {
                    return;
                }

                orderDetailPo.RemarkEditVisibility = Visibility.Collapsed;
                orderDetailPo.RemarkDisplayVisibility = Visibility.Visible;

                if (orderDetailPo.IssueRemark == remarks)
                {
                    return;
                }
                if (!Check(orderDetailPo))
                {
                    textBox.Text = orderDetailPo.IssueRemark;
                    return;
                }
                ExecutionReport execOrderPo = new ExecutionReport();
                execOrderPo.ExecID = orderDetailPo.ExecID;
                execOrderPo.CommandType = Convert.ToString((int)ECommandType.UpdateSendRemarks);
                execOrderPo.IssueRemark = remarks;
                execOrderPo.CloseById = LocalEnv.UserInfo.UserId;
                //OrderPo orderPo = OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId);
                await UpdateExecOrderData(execOrderPo);
                // OrderHelper.ChangeOrder(OrderHelper.GetUpdateOrderPoNew(remarks, ECommandType.UpdateSendRemarks, orderDetailPo.OrderId, orderDetailPo.ExecId));
            }
        }

        private void tb_Remarks_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    AppUtils.IsInnerInput = true;
                    if (e.ClickCount == 2)
                    {
                        e.Handled = true;
                        TextBlock textBlock = sender as TextBlock;

                        TransactionOrdermModel orderDetailPo = textBlock.DataContext as TransactionOrdermModel;
                        if (orderDetailPo == null)
                            return;

                        if (!Check(orderDetailPo))
                            return;
                        orderDetailPo.RemarkEditVisibility = Visibility.Visible;
                        orderDetailPo.RemarkDisplayVisibility = Visibility.Collapsed;
                        txt_Remarks.SelectAll();
                        txt_Remarks.Focus();
                    }
                }));
            }
        }
    }
}

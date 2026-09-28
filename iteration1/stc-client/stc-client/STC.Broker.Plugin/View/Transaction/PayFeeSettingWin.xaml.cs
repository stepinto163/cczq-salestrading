using Ats.Foundation.Message;
using com.gateway.log;
using IDC.Frame.Utils;
using Newtonsoft.Json;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Transaction
{
    /// <summary>
    /// PayFeeSettingWin.xaml 的交互逻辑
    /// </summary>
    public partial class PayFeeSettingWin : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 数据模型
        /// </summary>
        private TransactionOrdermModel _operationOrder;


        private string payInstIdA;
        /// <summary>
        /// 代付A信息
        /// </summary>
        public string PayInstIdA
        {
            get { return payInstIdA; }
            set
            {
                if (payInstIdA != value)
                {
                    payInstIdA = value;
                    NotifyPropertyChanged("PayInstIdA");
                }
            }
        }

        private string payInstIdB;
        /// <summary>
        /// 代付B信息
        /// </summary>
        public string PayInstIdB
        {
            get { return payInstIdB; }
            set
            {
                if (payInstIdB != value)
                {
                    payInstIdB = value;
                    NotifyPropertyChanged("PayInstIdB");
                }
            }
        }

        private string payInstNameA;
        /// <summary>
        /// 代付A信息
        /// </summary>
        public string PayInstNameA
        {
            get { return payInstNameA; }
            set
            {
                if (payInstNameA != value)
                {
                    payInstNameA = value;
                    NotifyPropertyChanged("PayInstNameA");
                }
            }
        }

        private string payInstNameB;
        /// <summary>
        /// 代付B信息
        /// </summary>
        public string PayInstNameB
        {
            get { return payInstNameB; }
            set
            {
                if (payInstNameB != value)
                {
                    payInstNameB = value;
                    NotifyPropertyChanged("PayInstNameB");
                }
            }
        }



        private bool instAIsChecked;
        /// <summary>
        /// 代付A信息
        /// </summary>
        public bool InstAIsChecked
        {
            get { return instAIsChecked; }
            set
            {
                if (instAIsChecked != value)
                {
                    instAIsChecked = value;
                    NotifyPropertyChanged("InstAIsChecked");
                }
            }
        }

        private bool instBIsChecked;
        /// <summary>
        /// 代付B信息
        /// </summary>
        public bool InstBIsChecked
        {
            get { return instBIsChecked; }
            set
            {
                if (instBIsChecked != value)
                {
                    instBIsChecked = value;
                    NotifyPropertyChanged("InstBIsChecked");
                }
            }
        }


        private string issueProxyFee;
        /// <summary>
        /// 代付费用信息
        /// </summary>
        public string IssueProxyFee
        {
            get { return issueProxyFee; }
            set
            {
                if (issueProxyFee != value)
                {
                    issueProxyFee = value;
                    NotifyPropertyChanged("IssueProxyFee");
                }
            }
        }


        public PayFeeSettingWin(TransactionOrdermModel orderDetailPo)
        {
            InitializeComponent();
            _operationOrder = orderDetailPo;
            if (orderDetailPo != null)
            {
                if (!string.IsNullOrWhiteSpace(orderDetailPo.InstitutionID)
                    && !string.IsNullOrWhiteSpace(orderDetailPo.TraderAInfoDisplay))
                {
                    if (!string.IsNullOrWhiteSpace(orderDetailPo.InstitutionShowName))
                    {
                        PayInstNameA = "帮" + orderDetailPo.InstitutionShowName + "代付";
                    }
                    else
                    {
                        PayInstNameA = "帮" + orderDetailPo.InstitutionName + "代付";
                    }
                    PayInstIdA = orderDetailPo.InstitutionID;
                }
                else
                {
                    PayInstNameA = "帮代付";
                    PayInstIdA = "";
                }

                if (!string.IsNullOrWhiteSpace(orderDetailPo.CounterPartyInstitutionId)
                    && !string.IsNullOrWhiteSpace(orderDetailPo.TraderBInfoDisplay))
                {
                    if (!string.IsNullOrWhiteSpace(orderDetailPo.CounterPartyInstitutionShowName))
                    {
                        PayInstNameB = "帮" + orderDetailPo.CounterPartyInstitutionShowName + "代付";
                    }
                    else
                    {
                        PayInstNameB = "帮" + orderDetailPo.CounterPartyInstitutionName + "代付";
                    }
                    //PayInstNameB = "帮" + orderDetailPo.c + "代付";

                    PayInstIdB = orderDetailPo.CounterPartyInstitutionId;
                }
                else
                {
                    PayInstNameB = "帮代付";
                    PayInstIdB = "";
                }

                //查询代付信息
                if (orderDetailPo.IssueProxyFlag == "1")
                {
                    if (!string.IsNullOrWhiteSpace(orderDetailPo.IssueProxyInstId) && orderDetailPo.IssueProxyInstId == orderDetailPo.CounterPartyInstitutionId)
                    {
                        InstAIsChecked = false;
                        InstBIsChecked = true;
                    }
                    else
                    {
                        InstAIsChecked = true;
                        InstBIsChecked = false;
                    }
                    IssueProxyFee = orderDetailPo.IssueProxyFee;
                    btnCancel.IsEnabled = true;
                }
                else
                {
                    queryPayFee(orderDetailPo);
                }
            }
            else
            {
                PayInstNameA = "帮代付";
                PayInstNameB = "帮代付";
                PayInstIdB = "";
                PayInstIdA = "";
                btnCancel.IsEnabled = false;
                btnOk.IsEnabled = false;
            }

            if (string.IsNullOrWhiteSpace(vtb_PayFee.Text))
            {
                btnOk.IsEnabled = false;
            }
            else
            {
                btnOk.IsEnabled = true;
            }

            vtb_PayFee.TextChanged += Vtb_PayFee_TextChanged;
        }

        private async void queryPayFee(TransactionOrdermModel orderDetailPo)
        {
            PayFeeRequest request = new PayFeeRequest() { instId = PayInstIdA, instId2 = PayInstIdB, securityid = orderDetailPo.SecurityID };
            PayFeeReply payFeeReplyPo = await GetExecOrderData(request);
            if (payFeeReplyPo != null && payFeeReplyPo.Code == "0000" && payFeeReplyPo.Datas != null)
            {
                PayFeeModel payFeeModel = JsonConvert.DeserializeObject<PayFeeModel>(payFeeReplyPo.Datas);
               
                this.Dispatcher.Invoke(() =>
                {
                    string instId = payFeeModel.instId;
                    string instProxyFee = payFeeModel.instProxyFee;

                    if (!string.IsNullOrWhiteSpace(instId) && instId == orderDetailPo.CounterPartyInstitutionId)
                    {
                        InstAIsChecked = false;
                        InstBIsChecked = true;
                    }
                    else
                    {
                        InstAIsChecked = true;
                        InstBIsChecked = false;
                    }
                    if (!string.IsNullOrWhiteSpace(instProxyFee))
                    {
                        instProxyFee = instProxyFee.Replace("厘", "");
                    }
                    IssueProxyFee = instProxyFee;
                });
            }
            else
            {
                InstAIsChecked = true;
                InstBIsChecked = false;
            }
            btnCancel.IsEnabled = false;
        }

        public async Task<PayFeeReply> GetExecOrderData(PayFeeRequest req)
        {
            PayFeeReply reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecPayFeeInfoQuery(req);
            });

            return reply;
        }


        private void Vtb_PayFee_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldTxt = vtb_PayFee.Text.Trim();
            double fee = 0.0;
            if (double.TryParse(oldTxt, out fee))
            {
                if (fee >= 0)
                {
                    vtb_PayFee.Text = oldTxt;
                }
                else
                {
                    vtb_PayFee.Text = "";
                }
            }
            else
            {
                vtb_PayFee.Text = "";
            }
            if (string.IsNullOrWhiteSpace(vtb_PayFee.Text))
            {
                btnOk.IsEnabled = false;
            }
            else
            {
                btnOk.IsEnabled = true;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 修改代付费用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ExecutionReport requestTransaction1 = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                //await UpdateExecOrderData(requestTransaction1);
                ExecutionReport result = new ExecutionReport();
                result.OrderID = _operationOrder.OrderID;
                result.ExecID = _operationOrder.ExecID;
                result.CommandType = ((int)ECommandType.UpdatePayFee).ToString();
                if (string.IsNullOrWhiteSpace(vtb_PayFee.Text))
                {
                    return;
                }
                result.ProxyFee = vtb_PayFee.Text.Trim();
                result.ProxyFlag = "1";

                if (InstAIsChecked)
                {
                    result.ProxyInstId = PayInstIdA;
                }
                else
                {
                    result.ProxyInstId = PayInstIdB;
                }
                await UpdateExecOrderData(result);
                this.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                LogHelper.Error(ex.StackTrace);
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

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecutionReport result = new ExecutionReport();
                result.OrderID = _operationOrder.OrderID;
                result.ExecID = _operationOrder.ExecID;
                result.CommandType = ((int)ECommandType.UpdatePayFee).ToString();
                result.ProxyFee = "";
                result.ProxyFlag = "0";
                result.ProxyInstId = "";
                result.CommandType = ((int)ECommandType.CancelPayFee).ToString();
                await UpdateExecOrderData(result);
                this.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                LogHelper.Error(ex.StackTrace);
            }
            var win = Window.GetWindow(this);
            if (win != null)
                win.Focus();
        }
    }
}

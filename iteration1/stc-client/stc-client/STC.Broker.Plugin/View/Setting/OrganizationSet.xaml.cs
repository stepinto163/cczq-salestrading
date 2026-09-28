using com.gateway.log;
using STC.Broker.Plugin.Utils;
using STC.Common.Handler.PT;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// OrganizationSet.xaml 的交互逻辑
    /// </summary>
    public partial class OrganizationSet : WindowBase
    {


        /// <summary>
        /// 代付机构列表
        /// </summary>
        public ObservableCollection<PaymentInstDetail> paymentOrg = new ObservableCollection<PaymentInstDetail>();
        /// <summary>
        /// 非代付机构列表
        /// </summary>
        public ObservableCollection<CrmInst> notPaymentOrg = new ObservableCollection<CrmInst>();

        ///// <summary>
        ///// 费代付机构集合缓存
        ///// </summary>
        //private List<CrmInst> notPayments = new List<CrmInst>();

        /// <summary>
        /// 机构集合缓存
        /// </summary>
        private List<CrmInst> PaymentInits = new List<CrmInst>();
        public OrganizationSet()
        {
            InitializeComponent();
            QueryNotPaymentOrg();
        }

        /// <summary>
        /// 设置费用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CostSetting_Click(object sender, RoutedEventArgs e)
        {

            bool isNewWin = false;
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            PaymentInstDetail paymentInst = button.DataContext as PaymentInstDetail;
            if (paymentInst == null)
            {
                return;
            }
            CostSettingWin win = CostSettingWin.CreateInstrance(paymentInst.instId, paymentInst.instName, out isNewWin);
            if (isNewWin)
            {
                win.Show();
            }
            else
            {
                win.Activate();
                win.WindowState = WindowState.Normal;
            }
        }

        private void btnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOkClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tb_Organization_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgRecive.SelectedIndex > 0)
                {
                    dgRecive.SelectedIndex--;
                }
                else
                {
                    if (dgRecive.SelectedIndex == 0)
                    {
                        dgRecive.SelectedIndex = dgRecive.Items.Count - 1;
                    }
                }
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgRecive.SelectedIndex < dgRecive.Items.Count - 1)
                {
                    dgRecive.SelectedIndex++;
                }
                else
                {
                    if (dgRecive.SelectedIndex == dgRecive.Items.Count - 1)
                    {
                        dgRecive.SelectedIndex = 0;
                    }
                }
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
        }

        private void tb_Organization_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dgRecive_MouseUp(null, null);
            }
        }

        private void tb_Organization_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!TextChangeFlag)
            //    return;

            string organizationKey = tb_Organization.Text.Trim();
            organizationKey = organizationKey.Replace("'", "").Replace("’", "");
            if (!string.IsNullOrEmpty(organizationKey))
            {
                List<CrmInst> dataList = PaymentInits.Where(o => o.shortnamecn.Contains(organizationKey)
                || o.pinyin.Contains(organizationKey)
                || o.pinyinfull.Contains(organizationKey)).OrderBy(o => o.shortnamecn.Length).Take(10).ToList();

                if (dataList.Count > 0)
                {
                    dgRecive.DataContext = dataList;
                    pop_Organization.IsOpen = true;
                    dgRecive.SelectedIndex = 0;
                }
                else
                {
                    dgRecive.DataContext = null;
                    pop_Organization.PlacementTarget = tb_Organization;
                    pop_Organization.IsOpen = false;
                }
            }
            else
            {
                dgRecive.DataContext = null;
                pop_Organization.IsOpen = false;
            }

        }

        private void dgRecive_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CrmInst dataPo = dgRecive.SelectedItem as CrmInst;
                if (dataPo != null)
                {
                    GoToOrderBySeq(dataPo);
                    pop_Organization.IsOpen = false;
                    visTextTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// 定位
        /// </summary>
        private void GoToOrderBySeq(CrmInst dataPo)
        {
            if (notPaymentOrg.Count > 0)
            {
                //定位单子
                int index = boxGroupList.Items.IndexOf(notPaymentOrg.Where(_ => _.idbkey == dataPo.idbkey).FirstOrDefault());
                if (index >= 0)
                {
                    boxGroupList.SelectedIndex = index;
                    boxGroupList.ScrollIntoView(notPaymentOrg.Where(_ => _.idbkey == dataPo.idbkey).FirstOrDefault());
                    tb_Organization.Text = dataPo.shortnamecn;
                    lborganList.SelectedIndex = -1;
                }
                else
                {
                    index = lborganList.Items.IndexOf(paymentOrg.Where(_ => _.instId == dataPo.idbkey).FirstOrDefault());
                    if (index >= 0)
                    {
                        lborganList.SelectedIndex = index;
                        lborganList.ScrollIntoView(paymentOrg.Where(_ => _.instId == dataPo.idbkey).FirstOrDefault());
                        tb_Organization.Text = dataPo.shortnamecn;
                        boxGroupList.SelectedIndex = -1;
                    }
                }
            }
        }

        private void QueryNotPaymentOrg()
        {
            Task.Factory.StartNew(() =>
            {
                GetPaymentOrg();
                GetNotPaymentOrg();
            });
        }

        private void GetNotPaymentOrg()
        {
            //Console.WriteLine("1:" + DateTime.Now.ToString("HH:mm:ss.fff"));
            List<CrmInst> dataGridDatas = Common.Utils.CommonUtils.GetInstitutionsInfoPo(string.Empty, 10000);
            //Console.WriteLine("2:" + DateTime.Now.ToString("HH:mm:ss.fff"));
            //赋值给缓存，用于搜索
            //notPayments = dataGridDatas.ToList();
            PaymentInits = dataGridDatas.ToList();
            //Console.WriteLine("3:" + DateTime.Now.ToString("HH:mm:ss.fff"));
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
                if (dataGridDatas != null && dataGridDatas.Count > 0)
                {
                    foreach (CrmInst item in dataGridDatas)
                    {
                        if (paymentOrg.Count > 0)
                        {
                            if (!paymentOrg.Any(_ => _.instId == item.idbkey))
                            {
                                notPaymentOrg.Add(item);
                            }
                        }
                        else
                        {
                            notPaymentOrg.Add(item);
                        }
                    }
                }
                else
                {
                    notPaymentOrg.Clear();
                }
            //}));
            //Console.WriteLine("4:" + DateTime.Now.ToString("HH:mm:ss.fff"));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //Console.WriteLine("5:" + DateTime.Now.ToString("HH:mm:ss.fff"));
                boxGroupList.DataContext = notPaymentOrg;
                //Console.WriteLine("6:" + DateTime.Now.ToString("HH:mm:ss.fff"));
            }));
        }

        private void GetPaymentOrg()
        {
            PaymentInstRequestPo request = new PaymentInstRequestPo();
            request.action = "0";
            PaymentInstReplyPo paymentInstReplyPo = GRHandler.Instance.OperateInstProxySetting(request);

            if (paymentInstReplyPo != null && "0000".Equals(paymentInstReplyPo.Code) && paymentInstReplyPo.Datas != null && paymentInstReplyPo.Datas.Any())
            {
                //this.Dispatcher.BeginInvoke(new Action(() =>
                //{
                    foreach (PaymentInstDetail item in paymentInstReplyPo.Datas)
                    {
                        if (item.businessType == "1")
                        {
                            paymentOrg.Add(item);
                        }
                    }
                    //paymentOrg = new ObservableCollection<PaymentInstDetail>(paymentInstReplyPo.Data);
                    //lborganList.DataContext = paymentOrg;
                //}));
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lborganList.DataContext = paymentOrg;
            }));
        }

        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {

        }

        private void popReciveName_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// 新增代付机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (boxGroupList.SelectedItems != null && boxGroupList.SelectedItems.Count > 0)
            {
                List<PaymentInstDetail> paymentInsts = new List<PaymentInstDetail>();
                StringBuilder errorSB = new StringBuilder();
                foreach(CrmInst ci in this.boxGroupList.SelectedItems)
                {
                    PaymentInstRequestPo paymentInst = new PaymentInstRequestPo();
                    paymentInst.action = "1";
                    paymentInst.businessType = "1";   
                    paymentInst.instId = ci.idbkey;
                    paymentInst.instName = ci.shortnamecn;
                    paymentInst.instFullName = ci.fullnamecn;
                    paymentInst.createby = LocalEnv.UserInfo.UserName;
                    PaymentInstReplyPo reply = GRHandler.Instance.OperateInstProxySetting(paymentInst);
                    if (reply == null || !reply.IsSuccess())
                    {
                        errorSB.Append($"新增代付机构失败：{reply.Msg}");
                    }
                    else
                    {
                        paymentInsts.Add(reply.Datas[0]);
                    }
                }

                if (errorSB.Length > 0)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "新增代付机构失败！";
                    warn.ShowDialog();
                }
                else
                {
                    //移除未代付缓存
                    List<CrmInst> infos = new List<CrmInst>();
                    foreach (var item in boxGroupList.SelectedItems)
                    {
                        CrmInst dataPo = item as CrmInst;
                        if (dataPo != null)
                        {
                            infos.Add(dataPo);
                        }
                    }
                    foreach (var item in infos)
                    {
                        CrmInst dataPo = item;
                        if (dataPo != null)
                        {
                            notPaymentOrg.Remove(notPaymentOrg.Where(o => o.idbkey == dataPo.idbkey).FirstOrDefault());
                            //notPayments.Remove(notPayments.Where(o => o.idbkey == dataPo.idbkey).FirstOrDefault());
                        }
                    }
                    //重新加载代付机构列表
                    //foreach (PaymentInstDetail item in paymentInsts) {
                    //    paymentOrg.Add(item);
                    //}

                    for (int i = paymentInsts.Count - 1; i >= 0; i--)
                    {
                        paymentOrg.Insert(0, paymentInsts[i]);
                    }
                    lborganList.DataContext = paymentOrg;
                    //BaseRequestPo request = new BaseRequestPo();
                    //PaymentInstReplyPo paymentInstReplyPo = BrokerCEDAServer.Instance.QueryProxyInst(request);
                    //if (paymentInstReplyPo != null && "0000".Equals(paymentInstReplyPo.Code) && paymentInstReplyPo.Data != null && paymentInstReplyPo.Data.Any()) {
                    //    paymentOrg = new ObservableCollection<PaymentInstDetail>(paymentInstReplyPo.Data);
                    //    lborganList.DataContext = paymentOrg;
                    //}
                }
            }
        }

        /// <summary>
        /// 删除代付机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (lborganList.SelectedItems != null && lborganList.SelectedItems.Count > 0)
            {
                StringBuilder errorSB = new StringBuilder();
                List<PaymentInstRequestPo> requestList = new List<PaymentInstRequestPo>();
                foreach (var item in lborganList.SelectedItems)
                {
                    PaymentInstDetail dataPo = item as PaymentInstDetail;
                    PaymentInstRequestPo paymentInst = new PaymentInstRequestPo();
                    paymentInst.action = "3";
                    paymentInst.id = dataPo.id;
                    paymentInst.instId = dataPo.instId;
                    paymentInst.businessType = dataPo.businessType;
                    requestList.Add(paymentInst);
                }
                await Task.Run(() => {
                    foreach (var paymentInst in requestList)
                    {
                        PaymentInstReplyPo reply = GRHandler.Instance.OperateInstProxySetting(paymentInst);
                        if (reply == null || !reply.IsSuccess())
                        {
                            errorSB.Append($"删除代付机构失败：{reply.Msg}");
                        }
                        else
                        {
                            CrmInst ci = PaymentInits.Find(_ => _.idbkey == paymentInst.instId);
                            if (ci != null)
                            {
                                this.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    if (!notPaymentOrg.Any(_ => _.idbkey == paymentInst.instId))
                                    {
                                        notPaymentOrg.Insert(0, ci);
                                    }
                                }));
                            }
                        }
                    }
                    Thread.Sleep(500);
                });

                if (errorSB.Length > 0)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "删除代付机构失败";
                    warn.ShowDialog();
                }
                else
                {
                    //重新加载代付机构列表
                    PaymentInstRequestPo request = new PaymentInstRequestPo();
                    request.action = "0";
                    PaymentInstReplyPo paymentInstReplyPo = GRHandler.Instance.OperateInstProxySetting(request);
                    if (paymentInstReplyPo != null && "0000".Equals(paymentInstReplyPo.Code) && paymentInstReplyPo.Datas != null && paymentInstReplyPo.Datas.Any())
                    {
                        paymentOrg = new ObservableCollection<PaymentInstDetail>(paymentInstReplyPo.Datas.Where(_ => _.businessType == "1"));
                        lborganList.DataContext = paymentOrg;
                    }
                    else
                    {
                        paymentOrg = new ObservableCollection<PaymentInstDetail>();
                        lborganList.DataContext = paymentOrg;
                    }
                    
                    //GetNotPaymentOrg();
                }
            }
        }
    }
}

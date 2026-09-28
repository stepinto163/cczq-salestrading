using com.gateway.log;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.ViewModel.Setting;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.View.Component;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// OutboundManagementWin.xaml 的交互逻辑
    /// </summary>
    public partial class OutboundManagementWin : WindowBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 机构集合缓存
        /// </summary>
        private List<PaymentInstDetail> _paymentInits = new List<PaymentInstDetail>();


        /// <summary>
        /// 机构数据源列表
        /// </summary>
        public ObservableCollection<PaymentInstDetail> paymentOrg = new ObservableCollection<PaymentInstDetail>();

        private ObservableCollection<OutboundManagementModel> _outboundManagementModels = new ObservableCollection<OutboundManagementModel>();


        public ObservableCollection<OutboundManagementModel> OutboundManagementModels
        {
            get { return _outboundManagementModels; }
            set
            {
                if (_outboundManagementModels != value)
                {
                    _outboundManagementModels = value;
                    NotifyPropertyChanged("OutboundManagementModels");
                }
            }
        }


        /// <summary>
        /// 是否全选
        /// </summary>
        private bool isAllChecked = false;

        /// <summary>
        /// 是否全选
        /// </summary>
        public bool IsAllChecked
        {
            get { return isAllChecked; }
            set
            {
                if (isAllChecked != value)
                {
                    isAllChecked = value;
                    NotifyPropertyChanged("IsAllChecked");
                }
            }
        }


        /// <summary>
        /// x选中的数据列表
        /// </summary>
        private List<OutboundManagementModel> _selectoutboundManagementModels = new List<OutboundManagementModel>();


        public OutboundManagementWin()
        {
            InitializeComponent();
            QueryNotPaymentOrg();
            this.MouseLeftButtonDown += OutboundManagementWin_MouseLeftButtonDown;
        }

        private void OutboundManagementWin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void QueryNotPaymentOrg()
        {
            Task.Factory.StartNew(() =>
            {
                GetOutInstInfo();
            });
        }



        /// <summary>
        /// 获取机构信息
        /// </summary>
        private void GetOutInstInfo()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                dgRecords.DataContext = OutboundManagementModels;
                btn_Del.IsEnabled = false;
                IsAllChecked = false;
                OutboundManagementModels.Clear();
                _paymentInits.Clear();
                paymentOrg.Clear();
            }));

            PaymentInstRequestPo request = new PaymentInstRequestPo() { action = "0" };

            bool isQuery = true;
            int totalCount = 0;
            while (isQuery)
            {
                PaymentInstReplyPo paymentInstReplyPo = GRHandler.Instance.OperateInstProxySetting(request);
                if (paymentInstReplyPo != null && "0000".Equals(paymentInstReplyPo.Code) && paymentInstReplyPo.Datas != null && paymentInstReplyPo.Datas.Any())
                {
                    totalCount += paymentInstReplyPo.Datas.Count;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (PaymentInstDetail item in paymentInstReplyPo.Datas)
                        {
                            if (item.businessType == "2")
                            {
                                OutboundManagementModel outboundManagementModel = new OutboundManagementModel();
                                outboundManagementModel.Id = item.id;
                                outboundManagementModel.City = item.city;
                                outboundManagementModel.OperateTime = item.createtime;
                                outboundManagementModel.OrganizationId = item.instId;
                                outboundManagementModel.OrganizationShortName = item.instName;
                                outboundManagementModel.OrganizationFullName = item.instFullName;
                                outboundManagementModel.Operator = item.createby;
                                OutboundManagementModels.Add(outboundManagementModel);
                                paymentOrg.Add(item);
                                _paymentInits.Add(item);
                            }
                        }
                    }));
                    //request.pageNum++;
                    if (totalCount >= paymentInstReplyPo.total)
                    {
                        isQuery = false;
                    }
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OutboundManagementModels.Clear();
                        //if (request.pageNum <= 1)
                        //{
                        //    OutboundManagementModels.Clear();
                        //}
                    }));
                    isQuery = false;
                }
            }
        }


        /// <summary>
        /// 数据定位
        /// </summary>
        /// <param name="dataPo"></param>
        private void GoToOrderBySeq(CrmInst dataPo)
        {
            if (OutboundManagementModels.Count > 0)
            {
                //定位单子
                int index = dgRecords.Items.IndexOf(OutboundManagementModels.Where(_ => _.OrganizationId == dataPo.idbkey).FirstOrDefault());
                if (index >= 0)
                {
                    dgRecords.SelectedIndex = index;
                    dgRecords.ScrollIntoView(OutboundManagementModels.Where(_ => _.OrganizationId == dataPo.idbkey).FirstOrDefault());
                }
                else
                {
                    pop_Organization.IsOpen = false;
                    dgRecords.SelectedIndex = -1;
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "该机构尚未添加外发管理！";
                    warn.ShowDialog();
                }
            }
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
            string organizationKey = tb_Organization.Text.Trim();
            organizationKey = organizationKey.Replace("'", "").Replace("’", "");
            if (!string.IsNullOrEmpty(organizationKey))
            {
                List<CrmInst> dataGridDatas = CommonUtils.GetInstitutionsInfoPo(organizationKey, 20);

                if (dataGridDatas != null && dataGridDatas.Count > 0)
                {
                    dgRecive.DataContext = dataGridDatas;
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

        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {

        }

        private void dgRecive_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CrmInst dataPo = dgRecive.SelectedItem as CrmInst;
                if (dataPo != null)
                {
                    tb_Organization.Text = dataPo.shortnamecn;
                    GoToOrderBySeq(dataPo);
                    visTextTextBox.Focus();
                    pop_Organization.IsOpen = false;
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
        /// 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            AddInstWin4Out addInstWin = new AddInstWin4Out(_paymentInits);
            addInstWin.onAddSec += Reflash;
            bool show = addInstWin.ShowDialog().Value;
        }


        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="paymentInst"></param>
        private void Reflash(PaymentInstDetail paymentInst)
        {
            QueryNotPaymentOrg();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Del_Click(object sender, RoutedEventArgs e)
        {
            if (_selectoutboundManagementModels.Count <= 0)
            {
                return;
            }

            //一条一条删。。。
            StringBuilder errorSB = new StringBuilder();
            List<PaymentInstRequestPo> reqeustList = new List<PaymentInstRequestPo>();
            foreach (OutboundManagementModel item in _selectoutboundManagementModels)
            {
                PaymentInstRequestPo paymentInst = new PaymentInstRequestPo();
                paymentInst.action = "3";
                paymentInst.id = item.Id;
                paymentInst.instId = item.OrganizationId;
                reqeustList.Add(paymentInst);
            }

            await Task.Run(() => {
                foreach(PaymentInstRequestPo paymentInst in reqeustList)
                {
                    PaymentInstReplyPo reply = GRHandler.Instance.OperateInstProxySetting(paymentInst);

                    if (reply == null || !reply.IsSuccess())
                    {
                        errorSB.AppendLine($"解除外发管理机构失败！{reply.Msg}");
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            var removeItem = OutboundManagementModels.First(_ => _.Id == paymentInst.id);
                            if (removeItem != null)
                            {
                                OutboundManagementModels.Remove(removeItem);
                            }
                        }));
                    }
                }
            });
            

            if (errorSB.Length > 0)
            {
                SysWarnGUI warn = new SysWarnGUI();
                Window win = Window.GetWindow(this);
                if (win != null)
                    warn.Owner = win;
                warn.tbTitle.Text = "提示";
                warn.tbMsg.Text = "解除外发管理机构失败！";
                warn.ShowDialog();
            }
            else
            {
                QueryNotPaymentOrg();
            }
        }

        private void BackGround_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void popReciveName_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            _selectoutboundManagementModels.Clear();
            foreach (OutboundManagementModel item in OutboundManagementModels)
            {
                if (item.IsChecked)
                {
                    _selectoutboundManagementModels.Add(item);
                }
            }

            if (_selectoutboundManagementModels.Count > 0)
            {
                if (_selectoutboundManagementModels.Count == OutboundManagementModels.Count)
                {
                    IsAllChecked = true;
                }
                else
                {
                    IsAllChecked = false;
                }
                btn_Del.IsEnabled = true;
            }
            else
            {
                if (OutboundManagementModels.Count <= 0)
                {
                    IsAllChecked = true;
                }
                else
                {
                    IsAllChecked = false;
                }
                btn_Del.IsEnabled = false;
            }
        }

        private void cb_all_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox == null)
            {
                return;
            }
            _selectoutboundManagementModels.Clear();
            if (checkBox.IsChecked.Value)
            {
                foreach (OutboundManagementModel item in OutboundManagementModels)
                {
                    item.IsChecked = true;
                    _selectoutboundManagementModels.Add(item);
                }
            }
            else
            {
                foreach (OutboundManagementModel item in OutboundManagementModels)
                {
                    item.IsChecked = false;
                }
            }
            if (_selectoutboundManagementModels.Count > 0)
            {
                btn_Del.IsEnabled = true;
            }
            else
            {
                btn_Del.IsEnabled = false;
            }
        }
    }
}


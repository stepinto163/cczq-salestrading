using com.gateway.log;
using STC.Broker.Plugin.ViewModel.Setting;
using STC.Common.Handler.PT;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.UI;
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

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// AddInstWin4Out.xaml 的交互逻辑
    /// </summary>
    public partial class AddInstWin4Out : WindowBase
    {
        private List<PaymentInstDetail> _instDetails;

        private CrmInst _dataPo;

        public Action<PaymentInstDetail> onAddSec;

        public AddInstWin4Out(List<PaymentInstDetail> instDetails)
        {
            InitializeComponent();
            _instDetails = instDetails;
        }

        private void btnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            if (_dataPo == null)
            {
                return;
            }
            PaymentInstRequestPo paymentInst = new PaymentInstRequestPo()
            {
                action = "1",
                instId = _dataPo.idbkey,
                instName = _dataPo.shortnamecn,
                createby = LocalEnv.UserInfo.FullName,
                businessType = "2",//业务类型：1-代付费用机构；2-外发手工发送机构管理；3-外发加桥发送机构管理
                city = _dataPo.cityvalue,
                instFullName = _dataPo.fullnamecn,
            };
           

            PaymentInstReplyPo reply = GRHandler.Instance.OperateInstProxySetting(paymentInst);

            if (reply == null || reply.Code != "0000")
            {
                SysWarnGUI warn = new SysWarnGUI();
                Window win = Window.GetWindow(this);
                if (win != null)
                    warn.Owner = win;
                warn.tbTitle.Text = "提示";
                warn.tbMsg.Text = "新增外发管理机构失败！";
                warn.ShowDialog();
            }
            else
            {
                onAddSec?.Invoke(reply.Datas[0]);
                this.Close();
            }
        }

        private void tbGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tb_Organization_TextChanged(object sender, TextChangedEventArgs e)
        {
            string organizationKey = tb_Organization.Text.Trim();
            organizationKey = organizationKey.Replace("'", "").Replace("’", "");
            if (!string.IsNullOrEmpty(organizationKey))
            {
                List<CrmInst> dataGridDatas = CommonUtils.GetInstitutionsInfoPo(organizationKey, 100);
                List<CrmInst> infos = new List<CrmInst>();
                if (dataGridDatas != null && dataGridDatas.Count > 0)
                {
                    foreach (CrmInst item in dataGridDatas)
                    {
                        if (_instDetails.Any(_ => _.instId == item.idbkey))
                        {
                            continue;
                        }
                        infos.Add(item);
                    }
                    dgRecive.DataContext = infos;
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

        private void tb_Organization_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dgRecive_MouseUp(null, null);
            }
        }

        private void dgRecive_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CrmInst dataPo = dgRecive.SelectedItem as CrmInst;
                if (dataPo != null)
                {
                    _dataPo = dataPo;
                    tb_Organization.Text = dataPo.shortnamecn;
                    pop_Organization.IsOpen = false;
                    visTextTextBox.Focus();
                }
                else
                {
                    _dataPo = null;
                }
            }
            catch (Exception ex)
            {
                _dataPo = null;
                LogHelper.Error(ex);
            }
            finally
            {
            }
        }

        private void popReciveName_LostFocus(object sender, RoutedEventArgs e)
        {

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

    }
}

using STC.Broker.Plugin.Model.Transaction.DesignModel;
using STC.Broker.Plugin.Utils;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
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
    /// AddRelevanceInstWin.xaml 的交互逻辑
    /// </summary>
    public partial class AddRelevanceInstWin : WindowBase
    {
        /// <summary>
        /// 机构
        /// </summary>
        private CrmInst _institutionalInfo = null;
        /// <summary>
        /// 关联机构
        /// </summary>
        private CrmInst _relevanceinstItutionalInfo = null;

        public event Action ReflshWin;
        public AddRelevanceInstWin()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 设置按钮状态
        /// </summary>
        private void SetControlStatus()
        {
            if (_institutionalInfo == null || _relevanceinstItutionalInfo == null)
            {
                this.btn_Ok.IsEnabled = false;
            }
            else
            {
                if (_institutionalInfo.idbkey == _relevanceinstItutionalInfo.idbkey)
                {
                    this.btn_Ok.IsEnabled = false;
                }
                else
                {
                    this.btn_Ok.IsEnabled = true;
                }

            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tb_Organization_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tb_Organization_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        //防止多次提交
        bool isChannge = false;
        /// <summary>
        /// 添加关联机构设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            BaseReplyPo baseDataReplyPo = null;
            await Task.Run(() =>
            {
                InstAssociatedRequest relevanceRequestPo = new InstAssociatedRequest()
                {
                    action = "1",
                    instId = _institutionalInfo.idbkey,
                    instName = _institutionalInfo.shortnamecn,
                    associatedInstId = _relevanceinstItutionalInfo.idbkey,
                    associatedInstName = _relevanceinstItutionalInfo.shortnamecn
                };
                if (isChannge)
                {
                    isChannge = false;
                    baseDataReplyPo = GRHandler.Instance.OperateInstAssociated(relevanceRequestPo);
                }
            });

            if (baseDataReplyPo != null && baseDataReplyPo.IsSuccess())
            {
                isChannge = true;
                ReflshWin?.Invoke();
                this.Close();
            }
            else
            {
                isChannge = true;
                SysWarnGUI warn = new SysWarnGUI();
                Window win = GetWindow(this);
                if (win != null)
                    warn.Owner = win;
                if (baseDataReplyPo == null || string.IsNullOrEmpty(baseDataReplyPo.Msg))
                {
                    warn.setWarnMsg("添加关联方失败");
                }
                else
                {
                    warn.setWarnMsg(baseDataReplyPo.Msg);
                }
                warn.ShowDialog();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbInst_TextChanged(object sender, TextChangedEventArgs e)
        {
            isChannge = true;
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            tbInstPop.PlacementTarget = textBox;
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                string param = textBox.Text.Replace("'", "");
                dgInstResult.DataContext = CommonUtils.GetInstitutionsInfoPo(param, 20);
                dgInstResult.SelectedIndex = 0;
                tbInstPop.IsOpen = true;
            }
            else
            {
                tbInstPop.IsOpen = false;
                if (textBox.Name == "tbInst")
                {
                    _institutionalInfo = null;
                }
                else
                {
                    _relevanceinstItutionalInfo = null;
                }
                SetControlStatus();
            }
        }




        private void dgInstResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dv = dgInstResult.SelectedItem as CrmInst;
            if (dv != null && !string.IsNullOrWhiteSpace(this.tbInst.Text))
            {
                if (tbInstPop.PlacementTarget == null)
                {
                    return;
                }
                TextBox textBox = tbInstPop.PlacementTarget as TextBox;
                if (textBox == null)
                {
                    return;
                }
                textBox.Text = dv.shortnamecn;
                tbInstPop.IsOpen = false;
                tb_hide.Focus();

                if (textBox.Name == "tbInst")
                {
                    _institutionalInfo = dv;
                }
                else
                {
                    _relevanceinstItutionalInfo = dv;
                }
                SetControlStatus();
            }
        }

        private void tbInst_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgInstResult.SelectedIndex > 0)
                    dgInstResult.SelectedIndex--;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgInstResult.SelectedIndex < dgInstResult.Items.Count - 1)
                    dgInstResult.SelectedIndex++;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
        }

        private void tbInst_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                TextBox textBox = sender as TextBox;
                if (textBox == null)
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "tbInst")
                    {
                        _institutionalInfo = null;
                    }
                    else
                    {
                        _relevanceinstItutionalInfo = null;
                    }
                    SetControlStatus();
                }
                else
                {
                    dgInstResult_MouseLeftButtonUp(null, null);
                }

            }
        }

        private void tbInstPInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tbInstPInputUpdate(object sender, TextCompositionEventArgs e)
        {

        }

        private void tbInstPop_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void tbInst_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }

        private void tbInst_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            tbInstPop.PlacementTarget = textBox;
        }
    }
}

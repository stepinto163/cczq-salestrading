using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
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
using static STC.Broker.Plugin.Utils.CommonEnum;

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// PersonalSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class PersonalSettingView : UserControl, ISetting
    {

        private SystemParameter CurrentSystemParamter;

        public PersonalSettingView()
        {
            InitializeComponent();

            this.Loaded += PersonalSettingView_Loaded;
        }

        private bool IsInit = true;

        private void PersonalSettingView_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentSystemParamter = GRSettingsHandler.GlobSystemParameter;

            //lbBridgeExe.Visibility = LocalEnv.ConfInfo.ShowBridgeRoleSetting ? Visibility.Visible : Visibility.Collapsed;
            //lb_OtherBridge.Visibility = LocalEnv.ConfInfo.ShowLookupOtherSetting ? Visibility.Visible : Visibility.Collapsed;
            //lbPersonExe.Visibility = LocalEnv.ConfInfo.ShowLookupSetting ? Visibility.Visible : Visibility.Collapsed;
            //lbUpdatePersonExe.Visibility = LocalEnv.ConfInfo.ShowAssistantSetting ? Visibility.Visible : Visibility.Collapsed;

            // 错误日志显示
            if (!LocalEnv.ConfInfo.ShowSentLogBtn)
            {
                spSendLog.Visibility = Visibility.Collapsed;
                this.Height -= 50;
                this.gd_Setting.Height -= 50;
            }


            
            if ("true".Equals(CurrentSystemParamter.formatShortNameCheck))
                cbShortName.IsChecked = true;
            else
                cbShortName.IsChecked = false;
            if ("true".Equals(CurrentSystemParamter.formatTenorCheck))
                cbTenor.IsChecked = true;
            else
                cbTenor.IsChecked = false;

            // 地方债带简称
            cbLGBShortName.IsChecked = GRSettingsHandler.AppSettingIsTrue(Constant.WithShortNameLGB);

            ////带剩余量信息
            //cb_withQty.IsChecked = GRSettingsHandler.AppSettingIsTrue(Constant.WithResidualQuantity);

            //qq发送带要素变化
            cb_SendQQ.IsChecked = GRSettingsHandler.AppSettingIsTrue(Constant.WithQQElementChange);

            //通过助理qq发送消息
            cb_SendQQAssistant.IsChecked = GRSettingsHandler.AppSettingIsTrue(Constant.WithAssistantQQSendMsg);

            //20220421---  v2.19.0 新增
            if ("true".Equals(CurrentSystemParamter.formatDebtTypeCheck))
            {
                cb_bondType.IsChecked = true;
            }
            else
            {
                cb_bondType.IsChecked = false;
            }

            //20231013---  v3.5.0 新增
            //带私募债类型
            cb_ppnBondType.IsChecked = GRSettingsHandler.AppSettingIsTrue(Constant.WithpPivatebondtype);

            //获取系统快捷键
            tbOffLineTransaction.Text = GetSysHotKey(GRSettingsHandler.OffLineTag);
            tbPricingTabTransaction.Text = GetSysHotKey(GRSettingsHandler.PricingTabTag);

            IsInit = false;
        }


        public string GetName()
        {
            return "操作设置";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {
            //throw new NotImplementedException();
        }

        private void btn_SendLog_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbShortName_Checked(object sender, RoutedEventArgs e)
        {
            SetShortName(ParamterType.WithShortName);
        }

        private void cbShortName_Unchecked(object sender, RoutedEventArgs e)
        {
            SetShortName(ParamterType.WithShortName);
        }
        private void SetShortName(ParamterType paramterType)
        {
            if (IsInit)
                return;

            SystemParameter sp = new SystemParameter();
            switch (paramterType)
            {
                case ParamterType.WithShortName:
                    {
                        sp.formatShortNameCheck = cbShortName.IsChecked.Value ? "true" : "false";
                        break;
                    }
                case ParamterType.WithShortNameLGB:
                    {
                        sp.ClientAppSettings[Constant.WithShortNameLGB] = cbLGBShortName.IsChecked.Value ? "true" : "false";
                        break;
                    }
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

        private void cbTenor_Checked(object sender, RoutedEventArgs e)
        {
            setTenor();
        }

        private void cbTenor_Unchecked(object sender, RoutedEventArgs e)
        {
            setTenor();
        }
        private void setTenor()
        {
            if (IsInit)
                return;
            SystemParameter sp = new SystemParameter();
            sp.formatTenorCheck = cbTenor.IsChecked.Value ? "true" : "false";
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

        private void tbOffLineTransaction_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tbTem = sender as TextBox;
            bool isEnterKey = e.Key == Key.Enter;
            string hotKey = string.Empty;
            if (e.Key < Key.F1 || e.Key > Key.F12)
            {
                //f10为系统键需要特别处理
                if (e.Key == Key.System && e.SystemKey == Key.F10)
                {
                    hotKey = e.SystemKey.ToString();
                    e.Handled = true;
                }
                else
                {
                    return;
                }
            }
            else
            {
                hotKey = e.Key.ToString();
            }

            if (!string.IsNullOrEmpty(hotKey))
            {
                tbTem.Text = hotKey;

                if (!GRSettingsHandler.OffLineKeys.Contains(tbOffLineTransaction.Text))
                {
                    tbTem.Clear();
                    tbTem.SelectionStart = tbTem.Text.Length;
                }
                else
                {
                    string oldHotKey = GetSysHotKey(GRSettingsHandler.OffLineTag);
                    if (!string.IsNullOrWhiteSpace(oldHotKey))
                    {
                        if (oldHotKey != tbOffLineTransaction.Text.Trim())
                        {
                            //将快捷键设置记录到系统设置中
                            SetHotKey();
                        }
                    }
                    else
                    {
                        SetHotKey();
                    }
                }
                //按下enter键快捷键录入框失去焦点
                this.txtVis.Focus();
            }
            
            #region 可以设置组合快捷键 暂时不用
            //if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            //{
            //    Console.WriteLine(e.Key);
            //    if (!hotKey.Contains("CTRL"))
            //        hotKey += " + CTRL";
            //    else
            //        return;
            //}
            //else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            //{
            //    Console.WriteLine(e.Key);
            //    if (!hotKey.Contains("ALT"))
            //        hotKey += " + ALT";
            //    else
            //        return;
            //}
            //else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            //{
            //    Console.WriteLine(e.Key);
            //    if (!hotKey.Contains("SHIFT"))
            //        hotKey += " + SHIFT";
            //    else
            //        return;
            //}
            //else if (e.Key == Key.LWin || e.Key == Key.RWin)
            //{
            //    Console.WriteLine(e.Key);
            //    if (!hotKey.Contains("WIN"))
            //        hotKey += " + WIN";
            //    else
            //        return;
            //}
            //else
            //{
            //    Console.WriteLine(e.Key);
            //    string[] keyArr = hotKey.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);

            //    foreach (string key in keyArr)
            //    {
            //        if (key.Trim().Equals(e.Key.ToString()))
            //            return;
            //    }

            //    hotKey += " + " + e.Key.ToString();
            //}

            //if (hotKey.StartsWith(" + "))
            //    tbTem.Text = hotKey.Substring(3);
            //else
            //    tbTem.Text = hotKey;
            #endregion

            tbTem.SelectionStart = tbTem.Text.Length;
        }

        private void tbOffLineTransaction_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tbTem = sender as TextBox;
            bool isEnterKey = e.Key == Key.Enter;
            if (e.Key < Key.F1 || e.Key > Key.F12)
            {
                if (!isEnterKey)
                {
                    if (!(e.Key == Key.System && e.SystemKey == Key.F10))
                    {
                        tbTem.Clear();
                        tbTem.SelectionStart = tbTem.Text.Length;
                    }
                }
            }
        }

        /// <summary>
        /// 将快捷键设置记录到系统设置中
        /// </summary>
        private void SetHotKey()
        {
            //将快捷键设置记录到系统设置中
            SystemParameter sp = new SystemParameter();
            sp.HotKey = $"{LocalEnv.UserInfo.UserName},{GRSettingsHandler.OffLineTag}:{tbOffLineTransaction.Text.Trim()}";
            sp.HotKey = $"{LocalEnv.UserInfo.UserName},{GRSettingsHandler.OffLineTag}:{tbOffLineTransaction.Text.Trim()}|{GRSettingsHandler.PricingTabTag}:{tbPricingTabTransaction.Text.Trim()}";
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }


        /// <summary>
        /// 获取指定系统快捷键
        /// </summary>
        /// <returns></returns>
        private string GetSysHotKey(string tag)
        {
            string hotKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(GRSettingsHandler.GlobSystemParameter.HotKey))
            {
                string[] hotKeyList = GRSettingsHandler.GlobSystemParameter.HotKey.Split(',');
                if (hotKeyList.Length == 2 && hotKeyList[0] == LocalEnv.UserInfo.UserName)
                {
                    string[] hotKeyValues = hotKeyList[1].Split('|');
                    foreach (string hotKeyValue in hotKeyValues)
                    {
                        string[] hotKeys = hotKeyValue.Split(':');
                        if (hotKeys.Length == 2)
                        {
                            if (tag == hotKeys[0])
                                hotKey = hotKeys[1];
                        }
                    }
                }
            }
            return hotKey;
        }

        private void cbLGBShortName_Checked(object sender, RoutedEventArgs e)
        {
            SetShortName(ParamterType.WithShortNameLGB);
        }

        private void cbLGBShortName_Unchecked(object sender, RoutedEventArgs e)
        {
            SetShortName(ParamterType.WithShortNameLGB);
        }


        private void cb_bondType_Click(object sender, RoutedEventArgs e)
        {
            if (IsInit)
                return;
            SystemParameter sp = new SystemParameter();
            sp.formatDebtTypeCheck = cb_bondType.IsChecked.Value ? "true" : "false";
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

        private void cb_ppnBondType_Click(object sender, RoutedEventArgs e)
        {
            SetSysParms(ParamterType.WithpPivatebondtype);
        }

        /// <summary>
        /// 设置系统参数
        /// </summary>
        /// <param name="paramterType"></param>
        private void SetSysParms(ParamterType paramterType)
        {
            if (IsInit)
                return;

            SystemParameter sp = new SystemParameter();
            switch (paramterType)
            {
                //case ParamterType.WithResidualQuantity:
                //    {
                //        sp.ClientAppSettings[Constant.WithResidualQuantity] = cb_withQty.IsChecked.Value ? "true" : "false";
                //        break;
                //    }
                case ParamterType.WithQQElementChange:
                    {
                        sp.ClientAppSettings[Constant.WithQQElementChange] = cb_SendQQ.IsChecked.Value ? "true" : "false";
                        break;
                    }

                case ParamterType.WithAssistantQQSendMsg:
                    {
                        sp.ClientAppSettings[Constant.WithAssistantQQSendMsg] = cb_SendQQAssistant.IsChecked.Value ? "true" : "false";
                        break;
                    }
                case ParamterType.WithpPivatebondtype:
                    {
                        sp.ClientAppSettings[Constant.WithpPivatebondtype] = cb_ppnBondType.IsChecked.Value ? "true" : "false";
                        break;
                    }
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

        private void cb_SendQQ_Click(object sender, RoutedEventArgs e)
        {
            SetSysParms(ParamterType.WithQQElementChange);
        }

        private void cb_SendQQAssistant_Click(object sender, RoutedEventArgs e)
        {
            SetSysParms(ParamterType.WithAssistantQQSendMsg);
        }

        private void tbPricingTabTransaction_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tbTem = sender as TextBox;
            bool isEnterKey = e.Key == Key.Enter;
            string hotKey = e.Key.ToString();
            bool isPricingTabKeys = e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.Tab || e.Key == Key.Space;
            if (isPricingTabKeys)
            {
                tbTem.Text = hotKey;

                if (!GRSettingsHandler.PricingTabKeys.Contains(tbPricingTabTransaction.Text))
                {
                    tbTem.Clear();
                    tbTem.SelectionStart = tbTem.Text.Length;
                }
                else
                {
                    string oldHotKey = GetSysHotKey(GRSettingsHandler.PricingTabTag);
                    if (!string.IsNullOrWhiteSpace(oldHotKey))
                    {
                        if (oldHotKey != tbPricingTabTransaction.Text.Trim())
                        {
                            //将快捷键设置记录到系统设置中
                            SetHotKey();
                        }
                    }
                    else
                    {
                        SetHotKey();
                    }
                }
                //按下enter键快捷键录入框失去焦点
                this.txtVis.Focus();

                tbTem.SelectionStart = tbTem.Text.Length;

                e.Handled = true;
            }
        }

        private void tbPricingTabTransaction_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tbTem = sender as TextBox;
            bool isEnterKey = e.Key == Key.Enter;
            bool isPricingTabKeys = e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.Tab || e.Key == Key.Space;
            if (!isPricingTabKeys)
            {
                if (!isEnterKey)
                {
                    tbTem.Clear();
                    tbTem.SelectionStart = tbTem.Text.Length;
                }
            }
        }
    }
}

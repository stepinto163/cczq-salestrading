using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Transaction.Design;
using STC.Common.Model;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Frame.UI;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// ManageSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class ManageSettingView : UserControl, ISetting
    {
        private SystemParameter CurrentSystemParamter;

        public ManageSettingView()
        {
            InitializeComponent();

            this.Loaded += ManageSettingView_Loaded;
        }
        private void ManageSettingView_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentSystemParamter = GRSettingsHandler.Instance.GetSystemParameter();
            sp_PaymentSetup.Visibility = GRSettingsHandler.Instance.CanPayment() ? Visibility.Visible : Visibility.Collapsed;

            bool canOutSend = GRSettingsHandler.Instance.CanOutSend();
            bool canRelevanceSetup = GRSettingsHandler.Instance.CanRelevance();
            if (canOutSend || canRelevanceSetup)
            {
                btn_OutgoingSetup.Visibility = canOutSend ? Visibility.Visible : Visibility.Collapsed;
                btn_RelevanceSetup.Visibility = canRelevanceSetup ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                spOutSendSetup.Visibility = Visibility.Collapsed;
            }

            spSendSetting.Visibility = GRSettingsHandler.Instance.CanSend() ? Visibility.Visible : Visibility.Collapsed;


            //闭市时间设置
            spMarketSetup.Visibility = GRSettingsHandler.Instance.CanMarketSetup() ? Visibility.Visible : Visibility.Collapsed;

            if ("true".Equals(CurrentSystemParamter.CanOpenSet))
                cbAutoOpen.IsChecked = true;
            else
                cbAutoOpen.IsChecked = false;
            if ("true".Equals(CurrentSystemParamter.CanCloseSet))
                cbAutoClose.IsChecked = true;
            else
                cbAutoClose.IsChecked = false;

            string openOpenInterval = GRSettingsHandler.GlobSystemParameter.AutoOpenInterval;
            if (!string.IsNullOrWhiteSpace(openOpenInterval))
            {
                int indext = openOpenInterval.IndexOf("-");
                if (indext != -1)
                {
                    string[] openCloseIntervalList = openOpenInterval.Split('-');
                    if (openCloseIntervalList.Length == 2)
                    {
                        int child = openCloseIntervalList[0].IndexOf(":");
                        if (child != -1)
                        {
                            string[] childList = openCloseIntervalList[0].Split(':');
                            if (childList.Length == 2)
                            {
                                tb_startHours_Open.Text = childList[0];
                                tb_startMinute_Open.Text = childList[1];
                            }
                        }

                        //结束时间
                        child = openCloseIntervalList[1].IndexOf(":");
                        if (child != -1)
                        {
                            string[] childList = openCloseIntervalList[1].Split(':');
                            if (childList.Length == 2)
                            {
                                tb_endHours_Open.Text = childList[0];
                                tb_endMinute_Open.Text = childList[1];
                            }
                        }
                    }
                }
            }

            string openCloseInterval = GRSettingsHandler.GlobSystemParameter.AutoCloseInterval;
            if (!string.IsNullOrWhiteSpace(openCloseInterval))
            {
                int indext = openCloseInterval.IndexOf("-");
                if (indext != -1)
                {
                    string[] openCloseIntervalList = openCloseInterval.Split('-');
                    if (openCloseIntervalList.Length == 2)
                    {
                        int child = openCloseIntervalList[0].IndexOf(":");
                        if (child != -1)
                        {
                            string[] childList = openCloseIntervalList[0].Split(':');
                            if (childList.Length == 2)
                            {
                                tb_startHours.Text = childList[0];
                                tb_startMinute.Text = childList[1];
                            }
                        }

                        //结束时间
                        child = openCloseIntervalList[1].IndexOf(":");
                        if (child != -1)
                        {
                            string[] childList = openCloseIntervalList[1].Split(':');
                            if (childList.Length == 2)
                            {
                                tb_endHours.Text = childList[0];
                                tb_endMinute.Text = childList[1];
                            }
                        }
                    }
                }
            }

            IsInit = false;
          // LogHelper.Info($"CompanyCode:{CurrentSystemParamter.CompanyCode + ""}");
        }

        private bool IsInit = true;

        public string GetName()
        {
            return "机构设置";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {
            throw new NotImplementedException();
        }

        private void btn_PaymentSetup_Click(object sender, RoutedEventArgs e)
        {
            OrganizationSet win = new OrganizationSet();
            win.ShowDialog();
        }

        private void btn_OutgoingSetup_Click(object sender, RoutedEventArgs e)
        {
            OutboundManagementWin outboundManagementWin = new OutboundManagementWin();
            outboundManagementWin.ShowDialog();
        }

        private void btn_SendSetting_Click(object sender, RoutedEventArgs e)
        {
            InstSettingWin instSettingWin = new InstSettingWin();
            instSettingWin.ShowDialog();
        }
        private void btn_RelevanceSetup_Click(object sender, RoutedEventArgs e)
        {
            RelevanceInstWin relevanceInstWin = new RelevanceInstWin();
            relevanceInstWin.ShowDialog();
        }

        /// <summary>
        /// 闭市设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_startHours_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isEnterKey = e.Key == Key.Enter;

            if (!isEnterKey)
            {
                return;
            }
            bool isHasChecked = true;
            if (string.IsNullOrWhiteSpace(tb_startHours.Text.Trim()))
            {
                tb_startHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_startHours.Tag = "小时格式错误";
                isHasChecked = false;
            }
            else
            {
                int hours;
                if (!int.TryParse(tb_startHours.Text.Trim(), out hours) || hours > 23 || hours < 0 || tb_startHours.Text.Trim().Length > 2)
                {
                    tb_startHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_startHours.Tag = "小时格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_startHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_startHours.Tag = tb_startHours.Text.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(tb_startMinute.Text.Trim()))
            {
                tb_startMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_startMinute.Tag = "分钟格式错误";
                isHasChecked = false;
            }
            else
            {
                int minute;
                if (!int.TryParse(tb_startMinute.Text.Trim(), out minute) || minute > 59 || minute < 0 || tb_startMinute.Text.Trim().Length > 2)
                {
                    tb_startMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_startMinute.Tag = "分钟格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_startMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_startMinute.Tag = tb_startMinute.Text.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(tb_endHours.Text.Trim()))
            {
                tb_endHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_endHours.Tag = "小时格式错误";
                isHasChecked = false;
            }
            else
            {
                int hours;
                if (!int.TryParse(tb_endHours.Text.Trim(), out hours) || hours > 23 || hours < 0 || tb_endHours.Text.Trim().Length > 2)
                {
                    tb_endHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_endHours.Tag = "小时格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_endHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_endHours.Tag = tb_endHours.Text.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(tb_endMinute.Text.Trim()))
            {
                tb_endMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_endMinute.Tag = "分钟格式错误";
                isHasChecked = false;
            }
            else
            {
                int minute;
                if (!int.TryParse(tb_endMinute.Text.Trim(), out minute) || minute > 59 || minute < 0 || tb_endMinute.Text.Trim().Length > 2)
                {
                    tb_endMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_endMinute.Tag = "分钟格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_endMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_endMinute.Tag = tb_endMinute.Text.Trim();
                }
            }
            this.txtVis.Focus();
            string OpenCloseInterval = tb_startHours.Text.Trim() + ":" + tb_startMinute.Text.Trim() + "-" + tb_endHours.Text.Trim() + ":" + tb_endMinute.Text.Trim();
            if (string.IsNullOrWhiteSpace(tb_startHours.Text.Trim())
                && string.IsNullOrWhiteSpace(tb_startMinute.Text.Trim())
                && string.IsNullOrWhiteSpace(tb_endHours.Text.Trim())
                && string.IsNullOrWhiteSpace(tb_endMinute.Text.Trim()))
            {
                isHasChecked = true;
                tb_startHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_startHours.Tag = tb_startHours.Text.Trim();
                tb_startMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_startMinute.Tag = tb_startMinute.Text.Trim();
                tb_endHours.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_endHours.Tag = tb_endHours.Text.Trim();
                tb_endMinute.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_endMinute.Tag = tb_endMinute.Text.Trim();
                OpenCloseInterval = "";
            }
            if (!isHasChecked)
            {
                return;
            }

            CurrentSystemParamter.AutoCloseInterval = OpenCloseInterval;
            SystemParameter sp = new SystemParameter();
            sp.AutoCloseInterval = OpenCloseInterval;
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

        private void tb_startHours_Open_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isEnterKey = e.Key == Key.Enter;

            if (!isEnterKey)
            {
                return;
            }
            bool isHasChecked = true;
            if (string.IsNullOrWhiteSpace(tb_startHours_Open.Text.Trim()))
            {
                tb_startHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_startHours_Open.Tag = "小时格式错误";
                isHasChecked = false;
            }
            else
            {
                int hours;
                if (!int.TryParse(tb_startHours_Open.Text.Trim(), out hours) || hours > 23 || hours < 0 || tb_startHours_Open.Text.Trim().Length > 2)
                {
                    tb_startHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_startHours_Open.Tag = "小时格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_startHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_startHours_Open.Tag = tb_startHours_Open.Text.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(tb_startMinute_Open.Text.Trim()))
            {
                tb_startMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_startMinute_Open.Tag = "分钟格式错误";
                isHasChecked = false;
            }
            else
            {
                int minute;
                if (!int.TryParse(tb_startMinute_Open.Text.Trim(), out minute) || minute > 59 || minute < 0 || tb_startMinute_Open.Text.Trim().Length > 2)
                {
                    tb_startMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_startMinute_Open.Tag = "分钟格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_startMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_startMinute_Open.Tag = tb_startMinute_Open.Text.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(tb_endHours_Open.Text.Trim()))
            {
                tb_endHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_endHours_Open.Tag = "小时格式错误";
                isHasChecked = false;
            }
            else
            {
                int hours;
                if (!int.TryParse(tb_endHours_Open.Text.Trim(), out hours) || hours > 23 || hours < 0 || tb_endHours_Open.Text.Trim().Length > 2)
                {
                    tb_endHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_endHours_Open.Tag = "小时格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_endHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_endHours_Open.Tag = tb_endHours_Open.Text.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(tb_endMinute_Open.Text.Trim()))
            {
                tb_endMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                tb_endMinute_Open.Tag = "分钟格式错误";
                isHasChecked = false;
            }
            else
            {
                int minute;
                if (!int.TryParse(tb_endMinute_Open.Text.Trim(), out minute) || minute > 59 || minute < 0 || tb_endMinute_Open.Text.Trim().Length > 2)
                {
                    tb_endMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    tb_endMinute_Open.Tag = "分钟格式错误";
                    isHasChecked = false;
                }
                else
                {
                    tb_endMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                    tb_endMinute_Open.Tag = tb_endMinute_Open.Text.Trim();
                }
            }
            this.txtVis.Focus();
            string OpenCloseInterval = tb_startHours_Open.Text.Trim() + ":" + tb_startMinute_Open.Text.Trim() + "-" + tb_endHours_Open.Text.Trim() + ":" + tb_endMinute_Open.Text.Trim();
            if (string.IsNullOrWhiteSpace(tb_startHours_Open.Text.Trim())
                && string.IsNullOrWhiteSpace(tb_startMinute_Open.Text.Trim())
                && string.IsNullOrWhiteSpace(tb_endHours_Open.Text.Trim())
                && string.IsNullOrWhiteSpace(tb_endMinute_Open.Text.Trim()))
            {
                isHasChecked = true;
                tb_startHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_startHours_Open.Tag = tb_startHours_Open.Text.Trim();
                tb_startMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_startMinute_Open.Tag = tb_startMinute_Open.Text.Trim();
                tb_endHours_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_endHours_Open.Tag = tb_endHours_Open.Text.Trim();
                tb_endMinute_Open.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                tb_endMinute_Open.Tag = tb_endMinute_Open.Text.Trim();
                OpenCloseInterval = "";
            }
            if (!isHasChecked)
            {
                return;
            }

            CurrentSystemParamter.AutoOpenInterval = OpenCloseInterval;
            SystemParameter sp = new SystemParameter();
            sp.AutoOpenInterval = OpenCloseInterval;
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

        private void cbAutoOpen_Checked(object sender, RoutedEventArgs e)
        {
            setAutoOpen();
        }

        private void cbAutoOpen_Unchecked(object sender, RoutedEventArgs e)
        {
            setAutoOpen();
        }

        private void setAutoOpen()
        {
            if (IsInit)
                return;
            SystemParameter sp = new SystemParameter();
            sp.CanOpenSet = cbAutoOpen.IsChecked.Value ? "true" : "false";
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

        private void cbAutoClose_Checked(object sender, RoutedEventArgs e)
        {
            setAutoClose();
        }

        private void cbAutoClose_Unchecked(object sender, RoutedEventArgs e)
        {
            setAutoClose();
        }

        private void setAutoClose()
        {
            if (IsInit)
                return;
            SystemParameter sp = new SystemParameter();
            sp.CanCloseSet = cbAutoClose.IsChecked.Value ? "true" : "false";
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
        }

    }
}

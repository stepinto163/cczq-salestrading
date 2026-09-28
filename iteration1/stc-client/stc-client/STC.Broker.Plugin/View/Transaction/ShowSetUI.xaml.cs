
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
using Newtonsoft.Json;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Transaction.Design;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.TBook;
using STC.Common.Utils;

namespace STC.Broker.Plugin.View.Transaction
{
    /// <summary>
    /// ShowSetUI.xaml 的交互逻辑
    /// 根据机构统计明细的配置窗口
    /// </summary>
    public partial class ShowSetUI : Window
    {
        private Dictionary<string, UserRoleSettingDetail> dicName = new Dictionary<string, UserRoleSettingDetail>();
        public Dictionary<string, UserRoleSettingDetail> dicChange = new Dictionary<string, UserRoleSettingDetail>();
        //方向配置
        private readonly string ShowSetUITag = "ShowSetUI";
        //方向配置
        private readonly string DirectionTag = "DirectionTag";

        /// <summary>
        /// 是否刷新明细
        /// </summary>
        public event Action RefrashDetail;
        public ShowSetUI()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += ShowSetUI_MouseLeftButtonDown;
            this.Loaded += ShowSetUI_Loaded;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        private void ShowSetUI_Loaded(object sender, RoutedEventArgs e)
        {
            dicName.Clear();
            SystemParameter set = GRSettingsHandler.Instance.GetSystemParameter();
            if (set != null && set.FormatInst != null && set.FormatInst.Count > 0)
            {
                foreach (var ins in set.FormatInst)
                {
                    string key = string.Format("{0}-{1}", ins.userName, ins.userDisplayName);
                    if (!dicName.ContainsKey(key))
                    {
                        dicName.Add(key, ins);
                        lbRecive.Items.Add(createUserCheckUI(ins, true));
                    }
                }
            }

            string isSplitStr;
            bool isSplit;
            if (set.ClientAppSettings.TryGetValue(Constant.SplitShowOtherOrders, out isSplitStr) && bool.TryParse(isSplitStr, out isSplit))
            {
                rbSplit.IsChecked = isSplit;
                rbCombine.IsChecked = !isSplit;
            }
            else
            {
                // 默认是合并的
                rbCombine.IsChecked = true;
                rbSplit.IsChecked = false;
            }


            //明细含公司名称展示
            string withCompanyName;
            bool iswithCompanyName;
            if (set.ClientAppSettings.TryGetValue(Constant.WithCompanyName, out withCompanyName)
                 && bool.TryParse(withCompanyName, out iswithCompanyName))
            {
                cb_withCompanyName.IsChecked = iswithCompanyName;
            }
            else
            {
                cb_withCompanyName.IsChecked = false;
            }

            //从客户端数据文件中读取方向配置默认不展示
            string directionTagStr = IniFileHelper.ReadValue(string.Format("{0}_{1}", ShowSetUITag, LocalEnv.UserInfo.UserId), DirectionTag);
            if (string.IsNullOrWhiteSpace(directionTagStr))
            {
                this.rbNoShow.IsChecked = true;
            }
            else
            {
                if (directionTagStr == "1")
                {
                    this.rbShow.IsChecked = true;
                }
                else
                {
                    this.rbNoShow.IsChecked = true;
                }
            }
        }


        private UserCheckUI createUserCheckUI(UserRoleSettingDetail ai, bool isRecive)
        {
            UserCheckUI userCheckUI = new UserCheckUI();
            userCheckUI.Margin = new Thickness(-6, 0, 0, 0);
            if (isRecive)
                userCheckUI.OnDelete += UserCheckUI_OnDelete1;
            else
                userCheckUI.OnDelete += UserCheckUI_OnDelete1;
            userCheckUI.Width = lbRecive.Width;
            userCheckUI.setData(ai);

            return userCheckUI;
        }

        private void ShowSetUI_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void tbReciveName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgRecive.SelectedIndex > 0)
                    dgRecive.SelectedIndex--;
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgRecive.SelectedIndex < dgRecive.Items.Count - 1)
                    dgRecive.SelectedIndex++;
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
        }
        private void tbReciveName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dgRecive_MouseDoubleClick(null, null);
            }
        }
        private void dgRecive_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CrmInst dv = dgRecive.SelectedItem as CrmInst;
            if (dv != null)
            {
                //var dr = dv.Row;
                UserRoleSettingDetail accountInfo = new UserRoleSettingDetail();
                accountInfo.userName = dv.instcode;//dr["instcode"] + "";
                accountInfo.userDisplayName = dv.TipsShortnamelist;//dr["instbncshortname"] + "";
                string key = string.Format("{0}-{1}", accountInfo.userName, accountInfo.userDisplayName);
                if (!dicName.ContainsKey(key))
                {
                    UserCheckUI userCheckUI = new UserCheckUI();
                    userCheckUI.Margin = new Thickness(-6, 0, 0, 0);
                    userCheckUI.OnDelete += UserCheckUI_OnDelete1;

                    userCheckUI.setData(accountInfo);
                    userCheckUI.Width = lbRecive.Width;

                    lbRecive.Items.Add(userCheckUI);
                    dicName.Add(key, accountInfo);
                }
                if (!dicChange.ContainsKey(key))
                    dicChange.Add(key, accountInfo);
                tbReciveName.Text = string.Empty;

                popReciveName.IsOpen = false;


                //save();
            }
        }
        private void popReciveName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbReciveName.Text))
                popReciveName.IsOpen = false;
        }
        private void tbReciveName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbReciveName.Text))
            {
                string param = tbReciveName.Text.Replace("'", "");

                dgRecive.DataContext = Utils.CommonUtils.GetInstitutionsTipsNameInfo(param, 20);//CRMService.Instance.GetInstByNameTerm(param, 20);
                dgRecive.SelectedIndex = 0;
                popReciveName.IsOpen = true;
            }
            else
            {
                popReciveName.IsOpen = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window win = Window.GetWindow(this);
            if (win != null)
            {
                win.Close();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.rbShow.IsChecked.HasValue && this.rbShow.IsChecked.Value)
            {
                IniFileHelper.WriteValue(string.Format("{0}_{1}", ShowSetUITag, LocalEnv.UserInfo.UserId), DirectionTag, "1");
            }
            else
            {
                IniFileHelper.WriteValue(string.Format("{0}_{1}", ShowSetUITag, LocalEnv.UserInfo.UserId), DirectionTag, "0");
            }
            SystemParameter sp = new SystemParameter();
            sp.FormatInst = new List<UserRoleSettingDetail>();
            foreach (UIElement ui in lbRecive.Items)
            {
                UserCheckUI uc = ui as UserCheckUI;
                if (uc != null)
                    sp.FormatInst.Add(uc.getAccountInfo());
            }
            // 其他人成交单是否按照机构交易员拆分
            sp.ClientAppSettings[Constant.SplitShowOtherOrders] = (rbSplit.IsChecked.HasValue && rbSplit.IsChecked.Value).ToString().ToLower();
            // 明细含公司名称展示
            sp.ClientAppSettings[Constant.WithCompanyName] = (cb_withCompanyName.IsChecked.HasValue && cb_withCompanyName.IsChecked.Value).ToString().ToLower();
            ThreadPool.QueueUserWorkItem(o =>
            {
                GRSettingsHandler.Instance.UpdateSystemParameter(sp);
            });
            //刷新明细
            RefrashDetail?.Invoke();

            var win = Window.GetWindow(this);
            if (win != null)
            {
                this.DialogResult = true;
                win.Close();
            }
        }

        private void UserCheckUI_OnDelete1(UserCheckUI obj)
        {
            if (obj != null && obj.AccountInfo != null)
            {
                string key = string.Format("{0}-{1}", obj.AccountInfo.userName, obj.AccountInfo.userDisplayName);
                if (dicName.ContainsKey(key))
                {
                    dicName.Remove(key);
                    lbRecive.Items.Remove(obj);
                }
                if (!dicChange.ContainsKey(key))
                    dicChange.Add(key, obj.AccountInfo);
                obj.OnDelete -= UserCheckUI_OnDelete1;
            }
            // save();
        }

        /// <summary>
        /// 选中是否展示方向
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbNotShow_Click(object sender, RoutedEventArgs e) {
            //RadioButton radioButton = sender as RadioButton;
            //if (radioButton!=null) {
            //    if (radioButton.Name == "rbShow") {
            //        IniFileHelper.WriteValue(string.Format("{0}_{1}", ShowSetUITag, LocalEnv.UserInfo.UserId), DirectionTag, "1");
            //    }
            //    else {
            //        IniFileHelper.WriteValue(string.Format("{0}_{1}", ShowSetUITag, LocalEnv.UserInfo.UserId), DirectionTag, "0");
            //    }
            //}
        }
    }

}

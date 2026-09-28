using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Transaction.Design;
using STC.Common.Model;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.Utils;
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
    /// PermissionSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class PermissionSettingView : UserControl, ISetting
    {
        private SystemParameter CurrentSystemParamter;


        private PowerSettingModel _powerSettingModel;

        public PermissionSettingView()
        {
            InitializeComponent();

            this.Loaded += PermissionSettingView_Loaded;
        }

        private void PermissionSettingView_Loaded(object sender, RoutedEventArgs e)
        {


            CurrentSystemParamter = GRSettingsHandler.Instance.GetSystemParameter();

            _powerSettingModel = GRSettingsHandler.PowerSettingModel;

            lbOpenClose.Visibility = GRSettingsHandler.Instance.CanOpenClose() ? Visibility.Visible : Visibility.Collapsed;
            lbBridgeExe.Visibility = GRSettingsHandler.Instance.CanBridgeExe()&& LocalEnv.ConfInfo.ShowBridgeRoleSetting ? Visibility.Visible : Visibility.Collapsed;
            lb_OtherBridge.Visibility = GRSettingsHandler.Instance.CanOtherBridge() && LocalEnv.ConfInfo.ShowLookupOtherSetting ? Visibility.Visible : Visibility.Collapsed;
            lbPersonExe.Visibility = GRSettingsHandler.Instance.CanPersonExe() ? Visibility.Visible : Visibility.Collapsed;
            lbUpdatePersonExe.Visibility = GRSettingsHandler.Instance.CanUpdatePersonExe() ? Visibility.Visible : Visibility.Collapsed;
            lbOwnHistory.Visibility = GRSettingsHandler.Instance.CanOwnHistory() ? Visibility.Visible : Visibility.Collapsed;

            IsInit = false;

            if (lbOpenClose.Visibility == Visibility.Visible)
            {
                listBox.SelectedIndex = 1;
            }
            else if (lbBridgeExe.Visibility == Visibility.Visible)
            {
                listBox.SelectedIndex = 2;
            }
            else if (lb_OtherBridge.Visibility == Visibility.Visible)
            {
                listBox.SelectedIndex = 3;
            }
            else if (lbPersonExe.Visibility == Visibility.Visible)
            {
                listBox.SelectedIndex = 4;

            }
            else if (lbUpdatePersonExe.Visibility == Visibility.Visible)
            {
                listBox.SelectedIndex =5;
            }
            else if (lbOwnHistory.Visibility == Visibility.Visible)
            {
                listBox.SelectedIndex = 6;
            }
        }



        private bool IsInit = true;

        public string GetName()
        {
            return "权限设置";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {
            throw new NotImplementedException();
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
                dgRecive_MouseUp(null, null);
            }
        }

        private void tbReciveName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TextChangeFlag)
                return;

            if (!string.IsNullOrEmpty(tbReciveName.Text))
            {
                string param = tbReciveName.Text.Replace("'", "");
                popReciveName.IsOpen = true;
                List<string> filterList = new List<string>();
                foreach (UserCheckUI ucleft in lbRecive.Items)
                {
                    filterList.Add(ucleft.AccountInfo.userDisplayName);
                }
                dgRecive.DataContext = Utils.CommonUtils.GetBroker(param, filterList, 20);//CRMDataHandler.Instance.SearchBroker(param, "");
                dgRecive.SelectedIndex = 0;

            }
            else
            {
                popReciveName.IsOpen = false;
            }
        }


        private bool TextChangeFlag = false;
        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = true;
        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = false;
        }

        private void popReciveName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbReciveName.Text))
                popReciveName.IsOpen = false;
        }

        private async void dgRecive_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var dv = dgRecive.SelectedItem as BrokerInfoPo;
                if (dv != null)
                {
                    UserRoleSettingDetail accountInfo = new UserRoleSettingDetail();
                    accountInfo.userId = dv.Idb_key;
                    accountInfo.userName = dv.Account;
                    accountInfo.userDisplayName = dv.AccountName;

                    bool isContain = false;
                    foreach (UserCheckUI uc in lbRecive.Items)
                    {
                        if (uc.AccountInfo.userDisplayName.Equals(accountInfo.userDisplayName) && uc.AccountInfo.userName.Equals(accountInfo.userName))
                        {
                            LogHelper.Debug("Authorized person contain User:" + accountInfo.userDisplayName);
                            isContain = true;
                            break;
                        }
                    }
                    if (!isContain)
                    {
                        UserRoleSettingDetail newAccountInfo = await save(accountInfo, "1");

                        if (newAccountInfo != null)
                        {
                            UserCheckUI userCheckUI = new UserCheckUI();
                            userCheckUI.Margin = new Thickness(-6, 0, 0, 0);
                            userCheckUI.OnDelete += UserCheckUI_OnDelete1;
                            userCheckUI.setData(newAccountInfo);

                            lbRecive.Items.Add(userCheckUI);

                            lbRecive.SelectedItem = userCheckUI;
                        }
                    }

                    tbReciveName.Text = string.Empty;

                    popReciveName.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private async void lbRecive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedIndex == 6 || listBox.SelectedIndex == 1 || listBox.SelectedIndex == 2 || listBox.SelectedIndex == 3)
                return;

            string roleType = string.Empty;
            if (listBox.SelectedIndex == 4)
            {
                roleType = UserRoleEnum.VIEW_APPOINTED_TRADE.ToString();
            }
            else if (listBox.SelectedIndex == 5)
            {
                roleType = UserRoleEnum.ASSISTANT.ToString();
            }
            await Task.Run(() =>
            {
                GRSettingsHandler.Instance.loadUserRoleSettings();
            });

            foreach (UserCheckUI ucUI in lbName.Items)
                ucUI.OnDelete -= UserCheckUI_OnDelete;

            lbName.Items.Clear();

            UserCheckUI uc = lbRecive.SelectedItem as UserCheckUI;

            if (uc != null)
            {
                if (listBox.SelectedIndex == 4)
                {
                    List<UserRoleSettingDetail> accountInfos = (from a in _powerSettingModel.QueryOrders where a.userDisplayName == uc.getAccountInfo().userDisplayName select a).ToList();

                    if (accountInfos != null && accountInfos.Count > 0 && accountInfos[0].relatedUsers != null && accountInfos[0].relatedUsers.Count > 0)
                    {
                        List<UserRoleSettingDetail> accountInfosC = new List<UserRoleSettingDetail>();
                        foreach (UserRoleSettingDetail ai in accountInfos[0].relatedUsers)
                        {
                            if (!accountInfosC.Any(_ => _.userName == ai.userName))
                            {
                                accountInfosC.Add(ai);
                                lbName.Items.Add(createUserCheckUI(ai, false));
                            }

                        }


                    }
                }
                else if (listBox.SelectedIndex == 5)
                {
                    List<UserRoleSettingDetail> accountInfos = (from a in _powerSettingModel.UpdateOrders where a.userDisplayName == uc.getAccountInfo().userDisplayName select a).ToList();

                    if (accountInfos != null && accountInfos.Count > 0 && accountInfos[0].relatedUsers != null && accountInfos[0].relatedUsers.Count > 0)
                    {
                        List<UserRoleSettingDetail> accountInfosC = new List<UserRoleSettingDetail>();
                        foreach (UserRoleSettingDetail ai in accountInfos[0].relatedUsers)
                        {
                            if (!accountInfosC.Any(_ => _.userName == ai.userName))
                            {
                                accountInfosC.Add(ai);
                                lbName.Items.Add(createUserCheckUI(ai, false));
                            }
                        }

                    }
                }

            }
        }

        private void tbName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgResult.SelectedIndex > 0)
                    dgResult.SelectedIndex--;
                dgResult.ScrollIntoView(dgResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgResult.SelectedIndex < dgResult.Items.Count - 1)
                    dgResult.SelectedIndex++;
                dgResult.ScrollIntoView(dgResult.SelectedItem);
            }
        }

        private void tbName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dgResult_MouseUp(null, null);
            }
        }

        private void tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TextChangeFlag)
                return;

            UserCheckUI uc = lbRecive.SelectedItem as UserCheckUI;
            if (!string.IsNullOrEmpty(tbName.Text))
            {
                if (uc == null)
                    return;

                string param = tbName.Text.Replace("'", "");
                popName.IsOpen = true;
                List<string> filterList = new List<string>();
                filterList.Add(uc.AccountInfo.userDisplayName);
                foreach (UserCheckUI ucRight in lbName.Items)
                {
                    filterList.Add(ucRight.AccountInfo.userDisplayName);
                }
                dgResult.DataContext = Utils.CommonUtils.GetBroker(param, filterList, 20);//CRMDataHandler.Instance.SearchBroker(param, uc.getAccountInfo().Name);
                dgResult.SelectedIndex = 0;
            }
            else
            {
                popName.IsOpen = false;
            }
        }

        private async void dgResult_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var dv = dgResult.SelectedItem as BrokerInfoPo;
                if (dv != null)
                {
                    UserRoleSettingDetail accountInfo = new UserRoleSettingDetail();
                    accountInfo.userId = dv.Idb_key;
                    accountInfo.userName = dv.Account;
                    accountInfo.userDisplayName = dv.AccountName;
                    bool isContain = false;
                    foreach (UserCheckUI uc in lbName.Items)
                    {
                        if (uc.AccountInfo.userDisplayName.Equals(accountInfo.userDisplayName) && uc.AccountInfo.userName.Equals(accountInfo.userName))
                        {
                            isContain = true;
                            LogHelper.Debug("Designated person contain User:" + accountInfo.userDisplayName);
                            break;
                        }
                    }

                    if (!isContain)
                    {
                        UserCheckUI reciveUC = lbRecive.SelectedItem as UserCheckUI;
                        if (reciveUC != null && reciveUC.AccountInfo != null)
                        {
                            List<UserRoleSettingDetail> relatedUsersList = reciveUC.AccountInfo.relatedUsers.ToList();
                            relatedUsersList.Add(accountInfo);
                            UserRoleSettingDetail newAccountInfo = await save(reciveUC.AccountInfo, "2", relatedUsersList);
                            if (newAccountInfo != null)
                            {
                                reciveUC.AccountInfo = newAccountInfo;

                                UserCheckUI userCheckUI = new UserCheckUI();
                                userCheckUI.Margin = new Thickness(-6, 0, 0, 0);
                                userCheckUI.OnDelete += UserCheckUI_OnDelete;
                                userCheckUI.setData(accountInfo);

                                lbName.Items.Add(userCheckUI);
                            }
                        }
                    }
                    tbName.Text = string.Empty;

                    popName.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }



        private async void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInit)
                return;

            string roleType = string.Empty;
            if (listBox.SelectedIndex == 1)
            {
                roleType = UserRoleEnum.OPEN_CLOSE_MARKET.ToString();
            }
            else if (listBox.SelectedIndex == 2)
            {
                roleType = UserRoleEnum.BRIDGE_ATTACHE.ToString();
            }
            else if (listBox.SelectedIndex == 3)
            {
                roleType = UserRoleEnum.VIEW_OTHER_BRIDGE_TRADE.ToString();
            }
            else if (listBox.SelectedIndex == 4)
            {
                roleType = UserRoleEnum.VIEW_APPOINTED_TRADE.ToString();
            }
            else if (listBox.SelectedIndex == 5)
            {
                roleType = UserRoleEnum.ASSISTANT.ToString();
            }
            else if (listBox.SelectedIndex == 6)
            {
                roleType = UserRoleEnum.NOT_VIEW_OTHER_HIST_TRADE.ToString();
            }
            await Task.Run(() =>
            {
                GRSettingsHandler.Instance.loadUserRoleSettings();
            });

            tbName.Text = tbReciveName.Text = string.Empty;

            if (listBox.SelectedIndex == 6 || listBox.SelectedIndex == 1 || listBox.SelectedIndex == 2 || listBox.SelectedIndex == 3 || listBox.SelectedIndex == 7)
                spName.Visibility = Visibility.Collapsed;
            else
                spName.Visibility = Visibility.Visible;

            foreach (UserCheckUI uc in lbRecive.Items)
                uc.OnDelete -= UserCheckUI_OnDelete1;

            lbRecive.Items.Clear();
            switch (listBox.SelectedIndex)
            {
                case 1:
                    //开闭市权限
                    if (_powerSettingModel.OpenCloses != null)
                    {
                        SetAccount(_powerSettingModel.OpenCloses);
                    }
                    break;
                case 2:
                    //查看过桥成交
                    if (_powerSettingModel.BridgePermissions != null)
                    {
                        //foreach (UserRoleSettingDetail ai in _powerSettingModel.BridgePermissions)
                        //    lbRecive.Items.Add(createUserCheckUI(ai, true));

                        SetAccount(_powerSettingModel.BridgePermissions);
                    }
                    break;
                case 3:
                    //查看过桥成交
                    if (_powerSettingModel.OtherBridgePermissions != null)
                    {
                        //foreach (UserRoleSettingDetail ai in _powerSettingModel.OtherBridgePermissions)
                        //    lbRecive.Items.Add(createUserCheckUI(ai, true));

                        SetAccount(_powerSettingModel.OtherBridgePermissions);
                    }
                    break;
                case 4:
                    //查看指定人成交
                    if (_powerSettingModel.QueryOrders != null)
                    {
                        //foreach (UserRoleSettingDetail ai in _powerSettingModel.QueryOrders)
                        //    lbRecive.Items.Add(createUserCheckUI(ai, true));

                        SetAccount(_powerSettingModel.QueryOrders);
                    }
                    break;
                case 5:
                    //修改指定人成交
                    if (_powerSettingModel.UpdateOrders != null)
                    {
                        //foreach (UserRoleSettingDetail ai in _powerSettingModel.UpdateOrders)
                        //    lbRecive.Items.Add(createUserCheckUI(ai, true));
                        SetAccount(_powerSettingModel.UpdateOrders);
                    }
                    break;
                case 6:
                    //不可查看他人历史成交
                    if (_powerSettingModel.onlyQueryOwnHistoryOrder != null)
                    {
                        //foreach (UserRoleSettingDetail ai in _powerSettingModel.onlyQueryOwnHistoryOrder)
                        //    lbRecive.Items.Add(createUserCheckUI(ai, true));

                        SetAccount(_powerSettingModel.onlyQueryOwnHistoryOrder);
                    }
                    break;
                //case 7:
                //    //设置权限
                //    if (CurrentSystemParamter.Settings != null)
                //    {
                //        foreach (AccountInfo ai in CurrentSystemParamter.Settings)
                //            lbRecive.Items.Add(createUserCheckUI(ai, true));
                //    }
                //    break;
                default:
                    break;
            }
        }


        private void SetAccount(List<UserRoleSettingDetail> userRoleSettingDetails)
        {
            if (userRoleSettingDetails != null && userRoleSettingDetails.Count > 0)
            {
                var roleSettingDetails = userRoleSettingDetails.Where((x, i) => userRoleSettingDetails.FindIndex(s => s.userId == x.userId) == i).ToList();

                foreach (UserRoleSettingDetail ai in roleSettingDetails)
                {
                    lbRecive.Items.Add(createUserCheckUI(ai, true));
                }
            }
            else
            {
                lbRecive.Items.Clear();
            }

        }
        private UserCheckUI createUserCheckUI(UserRoleSettingDetail ai, bool isRecive)
        {
            UserCheckUI userCheckUI = new UserCheckUI();
            userCheckUI.Margin = new Thickness(-6, 0, 0, 0);
            if (isRecive)
                userCheckUI.OnDelete += UserCheckUI_OnDelete1;
            else
                userCheckUI.OnDelete += UserCheckUI_OnDelete;

            userCheckUI.setData(ai);

            return userCheckUI;
        }

        private void popName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbName.Text))
                popName.IsOpen = false;
        }

        private async void UserCheckUI_OnDelete(UserCheckUI userCheckUI)
        {
            UserCheckUI leftUC = lbRecive.SelectedItem as UserCheckUI;
            if (leftUC.AccountInfo != null)
            {
                List<UserRoleSettingDetail> relatedUsersList = leftUC.AccountInfo.relatedUsers.ToList();
                relatedUsersList.RemoveAll(_ => _.userId == userCheckUI.AccountInfo.userId);
                UserRoleSettingDetail accountInfo = await save(leftUC.AccountInfo, "2", relatedUsersList);

                if (accountInfo != null)
                {
                    leftUC.AccountInfo = accountInfo;

                    lbName.Items.Remove(userCheckUI);
                    userCheckUI.OnDelete -= UserCheckUI_OnDelete;
                }
            }
        }

        private async void UserCheckUI_OnDelete1(UserCheckUI obj)
        {
            try
            {
                var tag = ((FrameworkElement)listBox.SelectedItem).Tag;
                if (tag?.ToString() == "adminSetting" && lbRecive.Items.Count <= 1)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    warn.setWarnMsg("不能全部删除");
                    warn.Show();
                    return;
                }
            }
            catch (Exception ex)
            {
            }

            UserRoleSettingDetail accountInfo = await save(obj.AccountInfo, "3");
            if (accountInfo != null)
            {
                lbRecive.Items.Remove(obj);
                obj.OnDelete -= UserCheckUI_OnDelete1;
            }
        }

        /// <summary>
        /// 保存指定成交人信息  /  修改指定人成交
        /// </summary>
        /// <param name="sp"></param>
        private void SetRelatedUsers(UserRoleSettingRequest sp)
        {
            if (sp.relatedUsers == null)
                sp.relatedUsers = new List<UserRoleSettingDetail>();
            else
                sp.relatedUsers.Clear();

            foreach (UserCheckUI ucUI in lbName.Items)
            {
                if (!sp.relatedUsers.Any(_ => _.userName == ucUI.getAccountInfo().userName))
                {
                    sp.relatedUsers.Add(ucUI.getAccountInfo());
                }
            }
        }

        private async Task<UserRoleSettingDetail> save(UserRoleSettingDetail detail, string action, List<UserRoleSettingDetail> relatedUsersList = null)
        {
            if (listBox.SelectedIndex == -1)
            {
                return null;
            }
            if (CurrentSystemParamter == null)
                return null;
            int position = 0;
            UserRoleSettingRequest sp = new UserRoleSettingRequest();
            sp.action = action;
            sp.id = detail.id;
            sp.userId = detail.userId;
            sp.userName = detail.userName;
            sp.userDisplayName = detail.userDisplayName;
            if (action == "3")
            {
                detail.relatedUsers.Clear();
            }
            sp.relatedUsers = detail.relatedUsers;
            switch (listBox.SelectedIndex)
            {
                case 1:
                    //设置开闭市
                    sp.roleName = UserRoleEnum.OPEN_CLOSE_MARKET.GetDescription();
                    sp.roleType = UserRoleEnum.OPEN_CLOSE_MARKET.ToString();
                    position = 1;
                    break;
                case 2:
                    //设置桥专员成交, 缺少接口参数
                    sp.roleName = UserRoleEnum.BRIDGE_ATTACHE.GetDescription();
                    sp.roleType = UserRoleEnum.BRIDGE_ATTACHE.ToString();
                    position = 2;
                    break;
                case 3:
                    //设置过桥成交, 缺少接口参数
                    sp.roleName = UserRoleEnum.VIEW_OTHER_BRIDGE_TRADE.GetDescription();
                    sp.roleType = UserRoleEnum.VIEW_OTHER_BRIDGE_TRADE.ToString();
                    position = 3;
                    break;
                case 4:
                    //设置查看指定人成交
                    sp.roleName = UserRoleEnum.VIEW_APPOINTED_TRADE.GetDescription();
                    sp.roleType = UserRoleEnum.VIEW_APPOINTED_TRADE.ToString();
                    //SetRelatedUsers(sp);
                    sp.relatedUsers = relatedUsersList;
                    position = 4;
                    break;
                case 5:
                    //修改指定人成交
                    sp.roleName = UserRoleEnum.ASSISTANT.GetDescription();
                    sp.roleType = UserRoleEnum.ASSISTANT.ToString();
                    //SetRelatedUsers(sp);
                    sp.relatedUsers = relatedUsersList;
                    position = 5;
                    break;
                case 6:
                    //不可查看他人历史成交
                    sp.roleName = UserRoleEnum.NOT_VIEW_OTHER_HIST_TRADE.GetDescription();
                    sp.roleType = UserRoleEnum.NOT_VIEW_OTHER_HIST_TRADE.ToString();
                    position = 6;
                    break;
                default:
                    break;
            }
            //异步更新设置信息

            var reply = await GRSettingsHandler.Instance.UpdateUserRoleSettings(sp);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    return reply.Datas[0];
                }
                else
                {
                    return detail;
                }
            }

            SysWarnGUI warn = new SysWarnGUI();
            if (reply == null || string.IsNullOrEmpty(reply.Msg))
            {
                warn.setWarnMsg("操作失败");
            }
            else
            {
                warn.setWarnMsg(reply.Msg);
            }

            warn.Show();

            //重新加载
            listBox_SelectionChanged(null, null);
            lbRecive_SelectionChanged(null, null);

            return null;

        }
    }
}

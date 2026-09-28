using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.ViewModel.Setting;
using STC.Common.Handler.PT;
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
    /// EditContactsWin.xaml 的交互逻辑
    /// </summary>
    public partial class EditContactsWin : WindowBase
    {
        private InstTraderViewModel _instTraderViewModel;

        private List<InstTraderViewModel> _instTraderViews;

        private bool textChangeFlag = false;

        public event Action<bool> ReflachAction;

        public EditContactsWin(InstTraderViewModel instTraderViewModel, List<InstTraderViewModel> instTraderViewModels)
        {
            InitializeComponent();
            _instTraderViewModel = instTraderViewModel;
            _instTraderViews = instTraderViewModels;
            tb_startMinute.Text = _instTraderViewModel.ExchangeContact;
            tbox_qq.Text = _instTraderViewModel.ExchangeQQ;

        }

        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errorSb = new StringBuilder();
            string instExchangeContactsQQ = tbox_qq.Text.Trim();
            await Task.Run(() => {

                try
                {
                    QueryIssueGroupRequest request = new QueryIssueGroupRequest { action = (int)ActionType.Query };
                    QueryIssueGroupReply result = GRHandler.Instance.QueryIssueGroupSetting(request);
                    if (result != null && result.datas != null)
                    {
                        IEnumerable<IssueGroup> groupList = result.datas.Where(item => item.instInfos.Any(subitem => subitem.instId == _instTraderViewModel.InstId));
                        foreach (IssueGroup ig in groupList)
                        {
                            IssueInst ii = ig.instInfos.Find(item => item.instId == _instTraderViewModel.InstId);
                            IssueInstTrader iit = ii.instTraders.Find(item => item.instTraderId == _instTraderViewModel.ContactId);
                            if (iit != null)
                            {
                                iit.instId = _instTraderViewModel.InstId;
                                iit.instTraderId = _instTraderViewModel.ContactId;
                                iit.instIssueId = _instTraderViewModel.IssuerId;
                                iit.instExchangeContactsId = _instTraderViewModel.ExchangeContactId;
                                iit.instExchangeContactsName = _instTraderViewModel.ExchangeContact;
                                iit.instExchangeContactsQQ = instExchangeContactsQQ;
                            }
                            else
                            {
                                ii.instTraders.Add(new IssueInstTrader
                                {
                                    instId = _instTraderViewModel.InstId,
                                    instTraderId = _instTraderViewModel.ContactId,
                                    instIssueId = _instTraderViewModel.IssuerId,
                                    instExchangeContactsId = _instTraderViewModel.ExchangeContactId,
                                    instExchangeContactsName = _instTraderViewModel.ExchangeContact,
                                    instExchangeContactsQQ = instExchangeContactsQQ,
                            }
                                );
                            }
                            if (ii != null)
                            {
                                OperateIssueInstRequest issueInstTrader = new OperateIssueInstRequest()
                                {
                                    action = (int)ActionType.Update,
                                    id = ii.id,
                                    groupId = ii.groupId,
                                    instId = _instTraderViewModel.InstId,
                                    instShortName = ii.instShortName,
                                    instFullName = ii.instFullName,
                                    instTraders = ii.instTraders,
                                    defaultRequest = ii.defaultRequest
                                };
                                var response = GRHandler.Instance.OperateIssueInstInfos(issueInstTrader);

                                if (response == null || !response.IsSuccess())
                                {
                                    errorSb.Append("更新联系人失败");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorSb.Append($"更新联系人失败,{ex}");
                }
            });
            if (errorSb.Length == 0)
            {
                ReflachAction?.Invoke(true);
                this.Close();
            }
            else
            {
                SysWarnGUI warn = new SysWarnGUI();
                warn.Owner = this;
                warn.setWarnMsg(errorSb.ToString());
                warn.ShowDialog();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tb_startHours_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void tbInstPop_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void tb_startMinute_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;

                dgInstResult_MouseDoubleClick(null, null);
            }
        }

        private void tb_startMinute_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tb_startMinute.Text) && textChangeFlag && _instTraderViews != null && _instTraderViews.Count > 0)
            {
                string param = tb_startMinute.Text.Replace("'", "");

                dgInstResult.DataContext = _instTraderViews.Where(_ => _.Contact.Contains(param)).ToList();
                dgInstResult.SelectedIndex = 0;
                tbInstPop.IsOpen = true;
            }
            else
            {
                tbInstPop.IsOpen = false;
            }
        }

        private void dgInstResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dv = dgInstResult.SelectedItem as InstTraderViewModel;
            if (dv != null && !string.IsNullOrWhiteSpace(this.tb_startMinute.Text))
            {
                tb_startMinute.Text = dv.Contact;
                _instTraderViewModel.ExchangeContact = dv.Contact;
                _instTraderViewModel.ExchangeContactId = dv.ContactId;


                List<CrmContact> contactList = CommonUtils.GetContactInfoPo(dv.Contact, 2000, dv.InstId);
                foreach(CrmContact cc in contactList)
                {
                    var temp = cc.qqlist;
                    if (!string.IsNullOrEmpty(temp) && temp.EndsWith(","))
                    {
                        temp = temp.Substring(0, temp.Length - 1);
                    }
                    _instTraderViewModel.ExchangeQQ = temp;
                    tbox_qq.Text = temp;
                }

                tbInstPop.IsOpen = false;
            }
        }

        private void tbInstPInput(object sender, TextCompositionEventArgs e)
        {
            textChangeFlag = true;
        }

        private void tbInstPInputUpdate(object sender, TextCompositionEventArgs e)
        {
            textChangeFlag = false;
        }
    }
}

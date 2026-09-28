using IDC.Matching.Plugin.Utils;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Risk;
using STC.Frame.UI;
using STC.XBond.Plugin.Setting.Utils.DataProcesser;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class PartyPositionRiskViewModel : ViewModelBase
    {
        #region 事件
        public ICommand AddCommand { get { return new RelayCommand(AddClick); } }
        public ICommand DelCommand { get { return new RelayCommand(DelClick); } }
        public ICommand EditCommand { get { return new RelayCommand(EditClick); } }
        public ICommand CancelCommand { get { return new RelayCommand(CancelClick); } }
        public ICommand SaveCommand { get { return new RelayCommand(SaveClick); } }

        #endregion

        private object lockObj = new object();
        private bool addStatus = false;

        #region 表格字段
        private const string KEY_FIELD = "Key";
        private const string STC_ACCOUNT_FIELD = "STCAccount";
        private const string USER_NAME_FIELD = "UserName";
        private const string DEPT_FIELD = "Dept";
        private const string INTER_RATE_LONG_EXTREME_FIELD = "InterestRateLongExtremeValue";
        private const string INTER_RATE_SHORT_EXTREME_FIELD = "InterestRateShortExtremeValue";
        private const string CREDIT_EXTREME_FIELD = "CreditExtremeValue";

        private const string STC_ACCOUNT_TEXT_FIELD = "STCAccountText";
        private const string USER_NAME_TEXT_FIELD = "UserNameText";
        private const string DEPT_TEXT_FIELD = "DeptText";
        private const string INTER_RATE_LONG_EXTREME_TEXT_FIELD = "InterestRateLongExtremeValueText";
        private const string INTER_RATE_SHORT_EXTREME_TEXT_FIELD = "InterestRateShortExtremeValueText";
        private const string CREDIT_EXTREME_TEXT_FIELD = "CreditExtremeValueText";

        private const string UPDATE_TIME_FIELD = "UpdateTime";
        private const string EDIT_ENABLE_FIELD = "EditEnable";
        private const string EDIT_BTN_VISIBLE_FIED = "EditBtnVisible";
        private const string SAVE_BTN_VISIBLE_FIED = "SaveBtnVisible";
        #endregion
        private DataView _PartyRiskParaView;

        public DataView PartyRiskParaView
        {
            get
            {
                lock (lockObj)
                {
                    if (_PartyRiskParaView == null)
                    {
                        _PartyRiskParaView = new DataTable().DefaultView;
                        _PartyRiskParaView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(STC_ACCOUNT_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(USER_NAME_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(DEPT_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(INTER_RATE_LONG_EXTREME_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(INTER_RATE_SHORT_EXTREME_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(CREDIT_EXTREME_FIELD, typeof(string));

                        _PartyRiskParaView.Table.Columns.Add(STC_ACCOUNT_TEXT_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(USER_NAME_TEXT_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(DEPT_TEXT_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(INTER_RATE_LONG_EXTREME_TEXT_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(INTER_RATE_SHORT_EXTREME_TEXT_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(CREDIT_EXTREME_TEXT_FIELD, typeof(string));

                        _PartyRiskParaView.Table.Columns.Add(UPDATE_TIME_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(EDIT_ENABLE_FIELD, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(EDIT_BTN_VISIBLE_FIED, typeof(string));
                        _PartyRiskParaView.Table.Columns.Add(SAVE_BTN_VISIBLE_FIED, typeof(string));
                        _PartyRiskParaView.Table.PrimaryKey = new DataColumn[] {
                            _PartyRiskParaView.Table.Columns[KEY_FIELD]
                        };
                        this.OnPropertyChanged();
                    }
                    return this._PartyRiskParaView;
                }
            }
        }
        // <summary>
        /// 选中的行数据
        /// </summary>
        private DataRowView _selectedItem;
        public DataRowView SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;
                this.OnPropertyChanged();
            }
        }
        public void init()
        {
            selectPartyRisk();
        }

        private void selectPartyRisk()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                RiskParaReq req = new RiskParaReq();
                RiskParaReplyPo po = TBookHandler.Instance.queryRiskPara(req);
                if (po != null)
                {
                    RiskParaReq rpp = po.datas;
                    if (rpp.partyPositionRiskParameters != null)
                        Instance_OnList(rpp.partyPositionRiskParameters);
                }
            });
        }
        private void Instance_OnList(List<PartyPositionRiskParameter> list)
        {
            lock (lockObj)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (PartyPositionRiskParameter po in list)
                    {
                        UpdatePartyRisk(po);
                    }
                }));
            }
        }
        private void UpdatePartyRisk(PartyPositionRiskParameter obj)
        {
            try
            {
                lock (lockObj)
                {
                    DataRow Item = this.PartyRiskParaView.Table.Rows.Find(new object[] { obj.stcAccount });
                    if (Item == null)
                    {
                        Item = this._PartyRiskParaView.Table.NewRow();
                        Item[KEY_FIELD] = obj.stcAccount;
                        Item[STC_ACCOUNT_FIELD] = obj.stcAccount;
                        Item[USER_NAME_FIELD] = obj.userName;
                        Item[DEPT_FIELD] = obj.dept;
                        Item[INTER_RATE_LONG_EXTREME_FIELD] = obj.interestRateLongExtremeValue;
                        Item[INTER_RATE_SHORT_EXTREME_FIELD] = obj.interestRateShortExtremeValue;
                        Item[CREDIT_EXTREME_FIELD] = obj.creditExtremeValue;

                        Item[STC_ACCOUNT_TEXT_FIELD] = obj.stcAccount;
                        Item[USER_NAME_TEXT_FIELD] = obj.userName;
                        Item[DEPT_TEXT_FIELD] = obj.dept;
                        Item[INTER_RATE_LONG_EXTREME_TEXT_FIELD] = obj.interestRateLongExtremeValue;
                        Item[INTER_RATE_SHORT_EXTREME_TEXT_FIELD] = obj.interestRateShortExtremeValue;
                        Item[CREDIT_EXTREME_TEXT_FIELD] = obj.creditExtremeValue;
                        Item[UPDATE_TIME_FIELD] = obj.update;
                        Item[EDIT_ENABLE_FIELD] = "False";
                        Item[EDIT_BTN_VISIBLE_FIED] = "0";
                        Item[SAVE_BTN_VISIBLE_FIED] = "1";
                        this._PartyRiskParaView.Table.Rows.Add(Item);
                    }
                    this._PartyRiskParaView.Table.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void AddClick()
        {
            try
            {
                lock (lockObj)
                {
                    if (!addStatus)
                    {
                        addStatus = true;
                        DataRow Items = this.PartyRiskParaView.Table.Rows.Find(new object[] { "" });
                        Items = this._PartyRiskParaView.Table.NewRow();
                        Items[KEY_FIELD] = "";
                        Items[EDIT_BTN_VISIBLE_FIED] = "1";
                        Items[SAVE_BTN_VISIBLE_FIED] = "0";
                        Items[EDIT_ENABLE_FIELD] = "True";
                        this._PartyRiskParaView.Table.Rows.Add(Items);
                        this._PartyRiskParaView.Table.AcceptChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DelClick()
        {
            DataRowView drv = this.SelectedItem;
            string key = drv[KEY_FIELD] as string;
            if ("".Equals(key))
            {
                removeRow(key);
                return;
            }
            else
            {
                RiskParaReq req = new RiskParaReq();
                req.partyPositionRiskParameters = toPoDel(key);
                PResult reply = TBookHandler.Instance.updateRiskPara(req);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        drv[KEY_FIELD] = drv[STC_ACCOUNT_FIELD];
                        addStatus = false;
                        removeRow(key);
                    }
                    else
                    {
                        addStatus = true;
                        showMessage(string.Format($"删除失败, 原因:{reply.Msg}."));
                    }
                }
            }
        }
        /// <summary>
         /// 提示消息弹框
         /// </summary>
         /// <param name="message"></param>
        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
        private List<PartyPositionRiskParameter> toPoDel(string key)
        {
            List<PartyPositionRiskParameter> list = new List<PartyPositionRiskParameter>();
            foreach (DataRow drv in _PartyRiskParaView.Table.Rows)
            {
                if ("".Equals(key) || !key.Equals(drv[KEY_FIELD] as string))
                {
                    PartyPositionRiskParameter po = new PartyPositionRiskParameter();
                    po.stcAccount = drv[STC_ACCOUNT_FIELD] as string;
                    po.userName = drv[USER_NAME_FIELD] as string;
                    po.dept = drv[DEPT_FIELD] as string;
                    po.interestRateLongExtremeValue = drv[INTER_RATE_LONG_EXTREME_FIELD] as string;
                    po.interestRateShortExtremeValue = drv[INTER_RATE_SHORT_EXTREME_FIELD] as string;
                    po.creditExtremeValue = drv[CREDIT_EXTREME_FIELD] as string;
                    po.update = drv[UPDATE_TIME_FIELD] as string;
                    list.Add(po);
                }
            }
            return list;
        }
        /// <summary>
        /// 根据债券id移除数据
        /// </summary>
        /// <param name="key"></param>
        private void removeRow(string key)
        {
            DataRow bondInfosItems = this._PartyRiskParaView.Table.Rows.Find(new object[] { key });
            this._PartyRiskParaView.Table.Rows.Remove(bondInfosItems);
            this._PartyRiskParaView.Table.AcceptChanges();
        }
        private void CancelClick()
        {
            DataRowView Item = this.SelectedItem;
            string key = Item[KEY_FIELD] as string;
            addStatus = false;
            if ("".Equals(key))
            {
                removeRow(key);
            }
            else
            {
                Item[STC_ACCOUNT_FIELD] = Item[KEY_FIELD];
                Item[USER_NAME_FIELD] = Item[USER_NAME_TEXT_FIELD];
                Item[DEPT_FIELD] = Item[DEPT_TEXT_FIELD];
                Item[INTER_RATE_LONG_EXTREME_FIELD] = Item[INTER_RATE_LONG_EXTREME_TEXT_FIELD];
                Item[INTER_RATE_SHORT_EXTREME_FIELD] = Item[INTER_RATE_SHORT_EXTREME_TEXT_FIELD];
                Item[CREDIT_EXTREME_FIELD] = Item[CREDIT_EXTREME_TEXT_FIELD];
                setStatus(Item);
            }
        }
        private void setStatus(DataRowView drv)
        {
            drv[EDIT_ENABLE_FIELD] = drv[EDIT_ENABLE_FIELD].Equals("True") ? "False" : "True";
            drv[EDIT_BTN_VISIBLE_FIED] = drv[EDIT_BTN_VISIBLE_FIED].Equals("1") ? "0" : "1";
            drv[SAVE_BTN_VISIBLE_FIED] = drv[SAVE_BTN_VISIBLE_FIED].Equals("1") ? "0" : "1";
        }
        private void EditClick()
        {
            DataRowView drv = this.SelectedItem;
            if (!addStatus)
            {
                addStatus = true;
                setStatus(drv);
            }
        }

        private void SaveClick()
        {
            DataRowView drv = this.SelectedItem;

            string account = drv[STC_ACCOUNT_FIELD] as string;
            string key = drv[KEY_FIELD] as string;
            DataRow Items = this._PartyRiskParaView.Table.Rows.Find(new object[] { account });
            if (null != Items)
            {
                if ("".Equals(key))
                {
                    showMessage(string.Format($"账号({account})已在列表中."));
                    return;
                }
            }
            if (checkData(drv))
            {
                showMessage(string.Format($"{failureMsg}."));
                return;
            }
            RiskParaReq req = new RiskParaReq();
            req.partyPositionRiskParameters = toPo(key);
            PResult reply = TBookHandler.Instance.updateRiskPara(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
                    drv[KEY_FIELD] = drv[STC_ACCOUNT_FIELD];
                    addStatus = false;
                    setStatus(drv);
                    drv[STC_ACCOUNT_FIELD] = drv[KEY_FIELD];
                    drv[USER_NAME_TEXT_FIELD] = drv[USER_NAME_FIELD];
                    drv[DEPT_TEXT_FIELD] = drv[DEPT_FIELD];
                    drv[INTER_RATE_LONG_EXTREME_TEXT_FIELD] = drv[INTER_RATE_LONG_EXTREME_FIELD];
                    drv[INTER_RATE_SHORT_EXTREME_TEXT_FIELD] = drv[INTER_RATE_SHORT_EXTREME_FIELD];
                    drv[CREDIT_EXTREME_TEXT_FIELD] = drv[CREDIT_EXTREME_FIELD];
                }
                else
                {
                    addStatus = true;
                    showMessage(string.Format($"更新失败, 原因:{reply.Msg}."));
                }
            }
        }
        private string failureMsg = "";
        private bool checkData(DataRowView drv)
        {
            bool status = false;
            failureMsg = "";
            string longText = drv[INTER_RATE_LONG_EXTREME_FIELD] as string;
            decimal longValue = "".Equals(longText) ? 0 : decimal.Parse(drv[INTER_RATE_LONG_EXTREME_FIELD] as string);
            if (longValue % 1000 != 0)
            {
                failureMsg = "利率债多头合计非1000的整数倍";
                status = true;
            }
            string shortText = drv[INTER_RATE_SHORT_EXTREME_FIELD] as string;
            decimal shortValue = "".Equals(shortText) ? 0 : decimal.Parse(drv[INTER_RATE_SHORT_EXTREME_FIELD] as string);
            if (shortValue % 1000 != 0)
            {
                if (!"".Equals(failureMsg))
                {
                    failureMsg = failureMsg + ",利率债空头合计非1000的整数倍";
                }
                else
                {
                    failureMsg = "利率债空头合计非1000的整数倍";
                }
                status = true;
            }
            string creText = drv[CREDIT_EXTREME_FIELD] as string;
            decimal creValue = "".Equals(creText) ? 0 : decimal.Parse(drv[CREDIT_EXTREME_FIELD] as string);
            if (creValue % 1000 != 0)
            {
                if (!"".Equals(failureMsg))
                {
                    failureMsg = failureMsg + ",信用债规模非1000的整数倍";
                }
                else
                {
                    failureMsg = "信用债规模非1000的整数倍";
                }
                status = true;
            }
            return status;
        }
        private List<PartyPositionRiskParameter> toPo(string key)
        {
            List<PartyPositionRiskParameter> list = new List<PartyPositionRiskParameter>();
            foreach (DataRow drv in _PartyRiskParaView.Table.Rows)
            {
                PartyPositionRiskParameter po = new PartyPositionRiskParameter();
                po.stcAccount = drv[STC_ACCOUNT_FIELD] as string;
                po.userName = drv[USER_NAME_FIELD] as string;
                po.dept = drv[DEPT_FIELD] as string;
                po.interestRateLongExtremeValue = drv[INTER_RATE_LONG_EXTREME_FIELD] as string;
                po.interestRateShortExtremeValue = drv[INTER_RATE_SHORT_EXTREME_FIELD] as string;
                po.creditExtremeValue = drv[CREDIT_EXTREME_FIELD] as string;
                if (po.stcAccount.Equals(key))
                {
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    drv[UPDATE_TIME_FIELD] = time;
                    po.update = time;
                }
                else
                {
                    po.update = drv[UPDATE_TIME_FIELD] as string;
                }
                list.Add(po);
            }
            return list;
        }
        public void setAccount()
        {
            DataRowView drv = this.SelectedItem;
            string userName = drv[STC_ACCOUNT_FIELD] as string;
            if (!"".Equals(userName))
            {
                StcTbookUsers po = getUsers(userName);
                if (po != null)
                {
                    drv[USER_NAME_FIELD] = po.Name;
                    drv[DEPT_FIELD] = po.Biztypestr;
                }
            }
        }

        private StcTbookUsers getUsers(String securityid)
        {
            List<StcTbookUsers> result = new List<StcTbookUsers>();
            result = STCUserProcesser.Processer.GetTbookUsersPo(securityid,1);
            if (result.Count > 0)
            {
                return result.ToArray()[0];
            }
            return null;
        }
    }
}

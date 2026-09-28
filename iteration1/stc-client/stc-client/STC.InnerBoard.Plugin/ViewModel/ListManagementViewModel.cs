using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.InnerBoard.Plugin.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.InnerBoard.Plugin.ViewModel
{
    public class ListManagementViewModel : ViewModelBase
    {
        private static ListManagementViewModel userManager = new ListManagementViewModel();
        public static ListManagementViewModel UserManager
        {
            get { return userManager; }
        }

        private BrokerListView view;
        public BrokerListView View
        {
            get
            {
                return view;
            }

            set
            {
                view = value;
            }
        }
        public static List<KeyValuePair<string, string>> UpdateTypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("UpdateType");
            }
        }

        #region 事件
        public ICommand AddCommand { get { return new ComRelayCommand(AddClick); } }
        public ICommand DelCommand { get { return new ComRelayCommand(DelClick); } }
        public ICommand EditCommand { get { return new ComRelayCommand(EditClick); } }
        public ICommand CancelCommand { get { return new ComRelayCommand(CancelClick); } }
        public ICommand SaveCommand { get { return new ComRelayCommand(SaveClick); } }
      
        #endregion

        #region 表格
        private const string KEY_FIELD = "Key";
        private const string BrokerId_FIELD = "BrokerId";
        private const string BrokerName_FIELD = "BrokerName";
        private const string WhiteListEnable_FIELD = "WhiteListEnable";
        private const string UpdateType_FIELD = "UpdateType";

       // private const string EDIT_ENABLE_FIELD = "EditEnable";
        private const string EDIT_BTN_VISIBLE_FIED = "EditBtnVisible";
        private const string SAVE_BTN_VISIBLE_FIED = "SaveBtnVisible";
        private const string SWITCH_ENABLE_FIED = "SwitchBtnEnable";
        private const string iscanaddedField = "IsCanAdded";
        private const string WhiteListType_FIED = "WhiteListType";
        private string chkMsgField = "ChkMsg";
        //private const string ADD_ENABLE_FIELD = "ADDEnable";
        //private const string ADD_VISIBLE_FIELD = "ADDVisible";
        //private const string ADD_VISIBLENo_FIELD = "ADDVisibleNo";

        #endregion
        private object lockObj = new object();
        public bool IsInAdd = false;
        public bool IsEdit { get; set; }
        private int action = 0;//操作类型 0-查询 1-新增 2-修改 

        private DataTable _PartyUserParaView;

        public DataTable PartyUserView 
        {
            get
            {
                lock (lockObj)
                {
                    if (_PartyUserParaView == null)
                    {
                        _PartyUserParaView = new DataTable();
                        _PartyUserParaView.Columns.Add(KEY_FIELD, typeof(string));
                        _PartyUserParaView.Columns.Add(BrokerId_FIELD, typeof(string));
                        _PartyUserParaView.Columns.Add(BrokerName_FIELD, typeof(string));
                        _PartyUserParaView.Columns.Add(WhiteListEnable_FIELD, typeof(string));
                        _PartyUserParaView.Columns.Add(WhiteListType_FIED, typeof(string));
                        
                        _PartyUserParaView.Columns.Add(UpdateType_FIELD, typeof(string));

                        _PartyUserParaView.Columns.Add(EDIT_BTN_VISIBLE_FIED, typeof(string)); 
                        _PartyUserParaView.Columns.Add(SAVE_BTN_VISIBLE_FIED, typeof(string));
                        _PartyUserParaView.Columns.Add(SWITCH_ENABLE_FIED, typeof(string));
                        _PartyUserParaView.Columns.Add(iscanaddedField, typeof(string));
                        _PartyUserParaView.Columns.Add(chkMsgField, typeof(string));

                        _PartyUserParaView.PrimaryKey = new DataColumn[] {
                        _PartyUserParaView.Columns[KEY_FIELD]
                        };
                        this.OnPropertyChanged(nameof(PartyUserView));
                    }
                    return this._PartyUserParaView;
                }
            }
            set
            {
                _PartyUserParaView = value;
                this.OnPropertyChanged(nameof(PartyUserView));
            }
        }

        /// <summary>
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

        #region 查询
        public void init()
        {
            clear();
            action = 0;
            QueryBrokerFilter();
        }

        public async void QueryBrokerFilter()
        {
            BrokerWhiteList req = new BrokerWhiteList();
            req.action = action;
            req.userId = LocalEnv.UserInfo.UserId;
            req.username = LocalEnv.UserInfo.UserName;
            BrokerWhiteListReplyPo reply = await QueryBrokerWhiteList(req);

            CommonUtils.BrokerDic = new Dictionary<string, BrokerWhiteList>();
            if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
            {
                foreach (BrokerWhiteList obj in reply.Datas)
                {
                    if (!string.IsNullOrEmpty(obj.brokerId) && !CommonUtils.BrokerDic.ContainsKey(obj.brokerId))
                        CommonUtils.BrokerDic.Add(obj.brokerId, obj);
                    lock (lockObj)
                    {
                        DataRow Item = this.PartyUserView.Rows.Find(new object[] { obj.id });
                        //bool isHaveNew = Item != null ? true : false;
                        // 如果有新增加,则不更新其它记录
                        if (Item == null)
                        {
                            DataRow row = this.PartyUserView.NewRow();

                            row[KEY_FIELD] = obj.id;
                            row[BrokerId_FIELD] = obj.brokerId;
                            row[BrokerName_FIELD] = obj.brokerName;
                            row[WhiteListEnable_FIELD] = obj.whiteListEnable;
                            row[WhiteListType_FIED] = obj.whiteListType;
                            row[UpdateType_FIELD] = obj.updateType;
                            //手动新增的可以修改 QQ号码和QQ名称。这种“非手动”的，就不支持编辑， 只支持删除，配置开关
                            if (!string.IsNullOrEmpty(obj.updateType) && "1".Equals(obj.updateType))
                            {
                                row[EDIT_BTN_VISIBLE_FIED] = "true";
                                row[SWITCH_ENABLE_FIED] = "false";
                            }
                            else
                            {
                                row[EDIT_BTN_VISIBLE_FIED] = "false";
                                row[SWITCH_ENABLE_FIED] = "true";
                            }
                            row[SAVE_BTN_VISIBLE_FIED] = "false";

                            row[chkMsgField] = "";
                            row[iscanaddedField] = "false";
                            this.PartyUserView.Rows.Add(row);
                            this.PartyUserView.AcceptChanges();
                            this.PartyUserView.AcceptChanges();
                        }
                        else
                        {
                            DataRow row = Item;
                            row.BeginEdit();
                            row[KEY_FIELD] = obj.id;
                            row[BrokerId_FIELD] = obj.brokerId;
                            row[BrokerName_FIELD] = obj.brokerName;
                            row[WhiteListEnable_FIELD] = obj.whiteListEnable;
                            row[WhiteListType_FIED] = obj.whiteListType;
                            row[UpdateType_FIELD] = obj.updateType;
                            //手动新增的可以修改 QQ号码和QQ名称。这种“非手动”的，就不支持编辑， 只支持删除，配置开关
                            if (!string.IsNullOrEmpty(obj.updateType) && "1".Equals(obj.updateType))
                            {
                                row[EDIT_BTN_VISIBLE_FIED] = "true";
                                row[SWITCH_ENABLE_FIED] = "false";
                            }
                            else
                            {
                                row[EDIT_BTN_VISIBLE_FIED] = "false";
                                row[SWITCH_ENABLE_FIED] = "true";
                            }
                            row[SAVE_BTN_VISIBLE_FIED] = "false";
                            row[iscanaddedField] = "false";
                            row.EndEdit();
                            this.PartyUserView.AcceptChanges();
                        }
                    }
                }
            }
        }

        public async Task<BrokerWhiteListReplyPo> QueryBrokerWhiteList(BrokerWhiteList req)
        {
            BrokerWhiteListReplyPo reply = null;

            await Task.Run(() =>
            {
                reply = TableHandler.Instance.brokerWhiteList(req);
            });
            return reply;
        }

        public async Task<BrokerWhiteReplyPo> UpdateBrokerWhite(BrokerWhiteList req)
        {
            BrokerWhiteReplyPo reply = null;

            await Task.Run(() =>
            {
                reply = TableHandler.Instance.brokerWhite(req);
            });
            return reply;
        }




        #endregion

        #region 新增
        private void AddClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (!IsEdit)
                    {
                        IsEdit = true;
                        action = 1;
                        lock (lockObj)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                DataRow row = this.PartyUserView.NewRow();
                                row[KEY_FIELD] = "0";
                                row[WhiteListEnable_FIELD] = "1";
                                row[EDIT_BTN_VISIBLE_FIED] = "false";
                                row[SAVE_BTN_VISIBLE_FIED] = "true";
                                row[SWITCH_ENABLE_FIED] = "true";
                                row[iscanaddedField] = "true";
                                row[UpdateType_FIELD] = "1";//手工;
                                this.PartyUserView.Rows.Add(row);
                                this.PartyUserView.AcceptChanges();
                                View.ScrollTable();
                            }));
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        #endregion

        #region 编辑
        private void EditClick()
        {
            lock (lockObj)
            {
                try
                {
                    action = 2;
                    IsEdit = true;
                    DataRowView row = this.SelectedItem;
                    row.BeginEdit();
                    row[EDIT_BTN_VISIBLE_FIED] = "false";
                    row[SAVE_BTN_VISIBLE_FIED] = "true";
                    row[SWITCH_ENABLE_FIED] = "true";
                    row[iscanaddedField] = "true";
                    row.EndEdit();
                    this.PartyUserView.AcceptChanges();
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }
        }
        public void ChangeStatus(string isEnable, DataRowView currentDRV)
        {
            lock (lockObj)
            {
                DataRowView row = currentDRV;
                row.BeginEdit();
                row[WhiteListEnable_FIELD] = isEnable;
                row.EndEdit();
                this.PartyUserView.AcceptChanges();
                if (!IsEdit)
                {
                    action = 2;
                    SaveClick(currentDRV, true);
                }
            }
        }
        #endregion


        #region 删除
        private StringBuilder deleteWarnMsg = new StringBuilder();
        private void DelClick()
        {
            MessageBoxGUI messageBoxGUI = new MessageBoxGUI();
            messageBoxGUI.setWarnMsg(string.Format("确认删除 {0} ?", deleteWarnMsg.ToString()));
            messageBoxGUI.MOK += DelClick_MOK;
            messageBoxGUI.Owner = Application.Current.MainWindow;
            messageBoxGUI.ShowDialog();

        }

        /// <summary>
        /// 确认删除执行删除数据
        /// </summary>
        private async void DelClick_MOK()
        {
            try
            {
                DataRowView drv = SelectedItem;
                BrokerWhiteList req = new BrokerWhiteList();
                req.id = drv[KEY_FIELD] + "";
                req.action = 3;
                req.brokerId = drv[BrokerId_FIELD] + "";
                req.brokerName = drv[BrokerName_FIELD] + "";
                req.whiteListType = drv[WhiteListType_FIED] + "";
                req.updateType = drv[UpdateType_FIELD] + "";
                req.username = LocalEnv.UserInfo.UserName;
                req.userId = LocalEnv.UserInfo.UserId;
                BrokerWhiteReplyPo reply = await UpdateBrokerWhite(req);
                if (reply != null && reply.Code == "0000")
                {
                    showMessage("删除成功");
                    IsEdit = false;
                    init();
                    System.Threading.ThreadPool.QueueUserWorkItem(o =>
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            DesignHandler.Instance.SendBrokerSettingChange(req);
                        }));
                    });
                }
                else
                {
                    showMessage(string.Format($"删除失败, 原因:{reply.Msg}."));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
        #endregion

        #region 删除或撤销
        private void CancelClick()
        {
            lock (PartyUserView)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        DataRowView row = SelectedItem;
                        lock (lockObj)
                        {
                            if (row[KEY_FIELD].ToString() == "0")
                            {
                                row.BeginEdit();
                                row.Delete();
                                row.EndEdit();
                                this.PartyUserView.AcceptChanges();
                            }
                            else
                            {
                                row.BeginEdit();
                                row[EDIT_BTN_VISIBLE_FIED] = "true";
                                row[SAVE_BTN_VISIBLE_FIED] = "false";
                                row.EndEdit();
                                this.PartyUserView.AcceptChanges();
                                init();
                            }
                            IsEdit = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                }));
            }
        }
        #endregion


        #region 保存事件
        private void SaveClick()
        {
            DataRowView row = this.SelectedItem;
            if (check(row))
            {
                row.BeginEdit();
                if (string.IsNullOrWhiteSpace(row[iscanaddedField].ToString()))
                {
                    row[iscanaddedField] = "true";
                }
                row.EndEdit();
                IsEdit = false;
                this.PartyUserView.AcceptChanges();
                SaveClick(row,false);
            }
        }
        public async void SaveClick(DataRowView row, bool isSwitch)
        {
            IsInAdd = true;
            BrokerWhiteList req = new BrokerWhiteList();
            string id = row[KEY_FIELD] + "";
            if (!id.Equals("0"))
                req.id = id;
            req.brokerId = row[BrokerId_FIELD] + "";
            req.brokerName = row[BrokerName_FIELD] + "";
            req.whiteListEnable = Convert.ToInt32(row[WhiteListEnable_FIELD] + "");
            req.username = LocalEnv.UserInfo.UserName;
            req.userId = LocalEnv.UserInfo.UserId;
            req.whiteListType = "";
            req.updateType = row[UpdateType_FIELD] + "";
            req.action = action;
            req.whiteListType = "FINQ";
            BrokerWhiteReplyPo reply = await UpdateBrokerWhite(req);
            if (reply != null && reply.Datas != null && reply.Code == "0000")
            {
                row.BeginEdit();
                row[iscanaddedField] = "false";
                //手动新增的可以修改 QQ号码和QQ名称。这种“非手动”的，就不支持编辑， 只支持删除，配置开关
                if (!string.IsNullOrEmpty(req.updateType) && "1".Equals(req.updateType))
                {
                    row[EDIT_BTN_VISIBLE_FIED] = "true";
                    row[SAVE_BTN_VISIBLE_FIED] = "false";
                }
                row.EndEdit();
                this.PartyUserView.AcceptChanges();
                if (isSwitch)
                {
                    // showMessage($"开关更新成功!");
                }
                else
                    showMessage($"保存成功!");
                init();
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                { 
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        DesignHandler.Instance.SendBrokerSettingChange(req);
                    }));
                });
            }
            else
            {
                if (isSwitch)
                    STC.Common.View.Component.CustomMessageBox.Error($"开关更新失败!\r\n{reply.Msg}", "提示", null);
                else
                    STC.Common.View.Component.CustomMessageBox.Error($"保存失败!\r\n{reply.Msg}", "提示", null);
            }
        }

        public bool check(DataRowView drv)
        {
            bool ischeck = false;

            string name = drv[BrokerId_FIELD] as string;
            if (string.IsNullOrWhiteSpace(name))
            {
                showMessage("QQ号不能为空！");
                return ischeck;
            }
            ischeck = true;
            return ischeck;
        }
        #endregion
   
        #region 清理
        public void clear()
        {
            try
            {
                lock (lockObj)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (DataRow dr in this._PartyUserParaView.Rows)
                        {
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        this._PartyUserParaView.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        #endregion

        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();
                //me.setTitle("提示");
                me.setWarnMsg(message);
                me.Owner = Application.Current.MainWindow;
                me.ShowDialog();
            }));
        }

    }
}

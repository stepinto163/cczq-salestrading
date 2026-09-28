using Newtonsoft.Json;
using STC.Common.Command;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Ans;
using STC.Common.Model.Admin;
using STC.Common.Model.Ans;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Drp.Component;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.ViewModel.Drp.Component
{
    public class MMAgencyMsgViewModel:ViewModelBase
    {
        private AsyncThreadQueue<List<TTbookSystemMessage>> _msgTask = null;
        //private MsgPo _currentMsg = null;
        private object _lockObj = new object();
        private Window _myWindow;
        //private string message;
        ///// <summary>
        ///// 原始消息
        ///// </summary>
        //public string Message
        //{
        //    get
        //    {
        //        return message;
        //    }
        //    set
        //    {
        //        if (message != value)
        //        {
        //            message = value;
        //            this.OnPropertyChanged(nameof(Message));
        //        }
        //    }
        //}
        private DataRowView _SelectedItem;

        /// <summary>
        /// 我的订单选中行记录
        /// </summary>
        public DataRowView SelectedItem
        {
            get
            {
                return _SelectedItem;
            }

            set
            {
                _SelectedItem = value;
                this.OnPropertyChanged();
            }
        }
        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
        /// <summary>
        /// 边框颜色，激活和失去焦点后变化
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }
        private string currentMsgId = string.Empty;
        public string CurrentMsgId
        {
            get { return currentMsgId; }
            set
            {
                if (value != null)
                {
                    currentMsgId = value;
                    this.OnPropertyChanged(nameof(currentMsgId));
                }
            }
        }
        private MMAgencyMsgView _mMAgencyMsgView;
        public MMAgencyMsgView mMAgencyMsgView 
        {
            get { return _mMAgencyMsgView; }
            set 
            {
                if (value != null) 
                {
                    _mMAgencyMsgView = value;
                }
            }
        }
        #region 字段定义
        public const string GROUPID_FIELD = "GroupId";
        public const string GROUPNAME_FIELD = "GroupName";
        public const string GROUPTYPE_FIELD = "GroupType";
        public const string MONITORSTATUS_FIELD = "MonitorStatus";
        public const string REASON_FIELD = "Reason";
        public const string SECURITYID_FIELD = "SecurityId";
        public const string SYMBOL_FIELD = "Symbol";
        public const string CREATETIEM_FIELD = "CreateTime";
        #endregion

        private DataView _DataDV;
        public DataView DataDV
        {
            get
            {
                lock (_lockObj)
                {
                    if (_DataDV == null)
                    {
                        _DataDV = new DataTable().DefaultView;
                        _DataDV.Table.Columns.Add(GROUPID_FIELD, typeof(string));
                        _DataDV.Table.Columns.Add(GROUPNAME_FIELD, typeof(string));
                        _DataDV.Table.Columns.Add(GROUPTYPE_FIELD, typeof(string));
                        _DataDV.Table.Columns.Add(MONITORSTATUS_FIELD, typeof(string));
                        _DataDV.Table.Columns.Add(REASON_FIELD, typeof(string));
                        _DataDV.Table.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _DataDV.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _DataDV.Table.Columns.Add(CREATETIEM_FIELD, typeof(string));
                        //_DataDV.Table.PrimaryKey = new DataColumn[] { _DataDV.Table.Columns[GROUPID_FIELD], _DataDV.Table.Columns[SECURITYID_FIELD] };
                    }
                    return _DataDV;
                }
            }
        }
        public ICommand CancelCommand { get; set; }
        public ICommand DealCommand { get; set; }
        public ICommand IgnoreCommand { get; set; }
        private object Updatelock = new object();
        public MMAgencyMsgViewModel()
        {
            this.CancelCommand = new ComRelayCommand(CancelClick);
            this.DealCommand = new DelegateCommand(new Action<object>(OnDealCommand));
            this.IgnoreCommand = new ComRelayCommand(OnIgnoreCommand);
        }
        /// <summary>
        /// 初始化界面 订阅系统参数消息数据
        /// </summary>
        /// <param name="ownWindow"></param>
        /// <param name="msg"></param>
        public void Init(Window ownWindow, MsgPo msg)
        {
            if (_msgTask == null)
                _msgTask = new AsyncThreadQueue<List<TTbookSystemMessage>>(Process);
            else
                _msgTask.Clear();

            _myWindow = ownWindow;

            InitUI(msg);

            AdminHandler.Instance.Init(OnAdminMsg);
        }
        /// <summary>
        /// 接收系统参数消息数据
        /// </summary>
        /// <param name="sysMsg"></param>
        private void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            _msgTask.Enqueue(sysMsg);
        }
        #region 参数消息数据处理
        private void InitUI(MsgPo msg)
        {
            lock (Updatelock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.DataDV.Table.Rows.Clear();
                    if (msg != null)
                    {
                        if (!string.IsNullOrEmpty(msg.Detail))
                        {
                            MMBreakAlarmModel mMBreak = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(msg.Detail);
                            CurrentMsgId = msg.Id;
                            if (mMBreak.infos.Count > 0)
                            {
                                foreach (BreakAlarmModel po in mMBreak.infos) 
                                {
                                    DataRow row = this.DataDV.Table.NewRow();
                                    row[GROUPID_FIELD] = po.groupId;
                                    row[GROUPNAME_FIELD] = po.groupName;
                                    row[GROUPTYPE_FIELD] = "UNIFY".Equals(po.groupType)?"整组":"单券组";
                                    row[MONITORSTATUS_FIELD] = po.monitorStatus;
                                    row[REASON_FIELD] = po.reason;
                                    row[SECURITYID_FIELD] = po.securityId;
                                    row[SYMBOL_FIELD] = po.symbol;
                                    row[CREATETIEM_FIELD] = po.creatTime;
                                    this.DataDV.Table.Rows.Add(row);
                                }
                                this.DataDV.Table.AcceptChanges();
                            }
                        }
                    }
                });
            }
        }
        /// <summary>
        /// 处理系统参数消息数据
        /// </summary>
        /// <param name="sysMsg"></param>
        private void Process(List<TTbookSystemMessage> sysMsg)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lockObj)
                {
                    if (sysMsg != null && sysMsg.Count > 0 && CurrentMsgId.Equals(sysMsg[0].id))
                    {
                        foreach (TTbookSystemMessage msg in sysMsg)
                        {
                            if (msg != null)
                            {
                                // 加入删除逻辑
                                string isdel = msg.delflag ?? "0";
                                if (isdel == "1")
                                {
                                    if (msg.id == CurrentMsgId)
                                    {
                                        CloseClick();
                                        break;
                                    }
                                }
                            }
                            //更新消息列表
                            if (msg.content != null)
                            {
                                TMessageContent detail = Newtonsoft.Json.JsonConvert.DeserializeObject<TMessageContent>(msg.content); 
                                if (detail.contentType.ToUpper().Equals("ANS"))
                                {
                                    MsgPo msgPo = new MsgPo();
                                    msgPo.Id = detail.id;
                                    msgPo.Detail = detail.detail;
                                    Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                    {
                                        mMAgencyMsgView.msgPo = msgPo;
                                        //mMAgencyMsgView.setCombo("GROUP");
                                    }));
                                    InitUI(msgPo);
                                }
                            }
                        }
                    }
                }
            }));
        }
        #endregion
        #region 筛选数据
        private StringBuilder reasonFilter = new StringBuilder();
        public void ReasonFilter(string param)
        {
            StringBuilder tempFilter = new StringBuilder();
            string reason = "自动关闭";
            if ("0001".Equals(param))
            {
                tempFilter.Append($"({REASON_FIELD} like '%{reason}%')");
            }
            else if ("0010".Equals(param))
            {
                tempFilter.Append($"({REASON_FIELD} not like '%{reason}%')");
            }
            reasonFilter = tempFilter;
            FilterData();
        }
        private StringBuilder groupFilter = new StringBuilder();
        public void GroupFilter(string param)
        {
            StringBuilder tempFilter = new StringBuilder();
            if (!string.IsNullOrEmpty(param))
            {
                string[] groupArray = param.Split(',');
                StringBuilder groupsb = new StringBuilder();
                foreach (string group in groupArray)
                {
                    if (groupsb.Length > 0)
                    {
                        groupsb.Append(",");
                    }
                    groupsb.Append($"'{group}'");
                }
                tempFilter.Append($"{GROUPID_FIELD} in ({groupsb})");
            }
            groupFilter = tempFilter;
            FilterData();
        }
        private void FilterData()
        {
            StringBuilder tempFilter = new StringBuilder();
            if (reasonFilter.Length > 0)
            {
                tempFilter.Append(reasonFilter);
            }
            if (groupFilter.Length > 0)
            {
                if (tempFilter.Length > 0)
                {
                    tempFilter.Append(" and ");
                }
                tempFilter.Append(groupFilter);
            }
            lock (_lockObj)
            {
                this.DataDV.RowFilter = tempFilter.ToString();
            }
        }
        #endregion

        #region 关闭窗口
        /// <summary>
        /// 取消并关闭窗口
        /// </summary>
        private void CloseClick()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
            _myWindow.Close();

        }

        /// <summary>
        /// 取消
        /// </summary>
        private void CancelClick()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
            _myWindow.Close();
        }
        #endregion

        #region 消息按钮
        private void OnDealCommand(object obj)
        {
            if (SelectedItem != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                {
                    BreakAlarmModel model = new BreakAlarmModel();
                    model.groupId = CommonUtils.ConvertToString(SelectedItem[GROUPID_FIELD]);
                    model.securityId = CommonUtils.ConvertToString(SelectedItem[SECURITYID_FIELD]);
                    //string msg = 
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "MMAgencyMsg", Info1 = "BreakAlarmModel", Msg = JsonConvert.SerializeObject(model) });
                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                }));
            }
        }

        private void OnIgnoreCommand()
        {
            try
            {
                if (SelectedItem != null)
                {
                    IgnoreBreakReq req = new IgnoreBreakReq();
                    req.userName = LocalEnv.UserInfo.UserName;
                    req.securityId = CommonUtils.ConvertToString(SelectedItem[SECURITYID_FIELD]);
                    PResult result = AnsHandler.Instance.IgnoreBreak(req);
                    if (result != null && "0000".Equals(result.Code))
                    {

                    }
                    else 
                    {
                        showMessage($"双边断报提醒忽略债券{req.securityId}失败");
                    }
                }
            }
            catch (Exception e) 
            {
                LogHandler.Error(e);
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
        #endregion

    }
}

using com.gateway.log;
using STC.Common.Handler.Risk;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Risk;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Common.ViewModel.UI
{
    public class WhiteSecuritySettingViewModel : BaseViewModel
    {
        #region 构造初始化
        public ICommand AddRecordCommand { get { return new ComRelayCommand(OnAddRecordCommandClick); } }
        public ICommand DelRecordCommand { get { return new ComRelayCommand(OnDelRecordCommandClick); } }

        public ICommand EditRecordCommand { get { return new ComRelayCommand(OnEditRecordCommandClick); } }

        public ICommand SaveRecordCommand { get { return new ComRelayCommand(OnSaveRecordCommandClick); } }

        public ICommand DownloadFileCommand { get { return new ComRelayCommand(OnDownloadFileCommandClick); } }

        public ICommand RefreshCommand { get { return new ComRelayCommand(OnRefreshCommandClick); } }
        

        private bool _isCanSave = true;
        public bool IsCanSave
        {
            get
            {
                return _isCanSave;
            }
            set
            {
                _isCanSave = value;
                this.OnPropertyChanged(nameof(IsCanSave));
            }
        }

        bool _isEdit = false;
        public bool IsEdit
        {
            get
            {
                return _isEdit;
            }
            set
            {
                _isEdit = value;
                IsCanSave = !value;
                this.OnPropertyChanged(nameof(IsEdit));
            }
        }

        public bool IsAdd = false;

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

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                this.OnPropertyChanged();
            }
        }

        private object lockObj = new object();


        private string idField = "id";
        private string usernameField = "username";
        private string securityidField = "securityid";
        private string codeField = "code";
        private string messageField = "message";
        private string enableField = "enable";
        private string updatetimeField = "updatetime";
        private string itemSourceField = "itemsource";
        private string iscanaddedField = "IsCanAdded";
        private string chkMsgField = "ChkMsg";
        private DataTable _GlobalView;
        public DataTable GlobalView
        {
            get
            {
                if (_GlobalView == null)
                {
                    _GlobalView = new DataTable();
                    _GlobalView.Columns.Add("id", typeof(string));
                    _GlobalView.Columns.Add("username", typeof(string));
                    _GlobalView.Columns.Add("securityid", typeof(string));
                    _GlobalView.Columns.Add("code", typeof(string));
                    _GlobalView.Columns.Add("message", typeof(string));
                    _GlobalView.Columns.Add("enable", typeof(string));
                    _GlobalView.Columns.Add("updatetime", typeof(string));
                    _GlobalView.Columns.Add("EditBtnVisible", typeof(string));
                    _GlobalView.Columns.Add("SaveBtnVisible", typeof(string));
                    _GlobalView.Columns.Add("itemsource", typeof(List<KeyValuePair<string, string>>));
                    _GlobalView.Columns.Add("IsCanAdded", typeof(string));
                    _GlobalView.Columns.Add("ChkMsg", typeof(string));
                    _GlobalView.DefaultView.Sort = $"{updatetimeField} desc";
                    this.OnPropertyChanged(nameof(GlobalView));
                }
                return this._GlobalView;
            }
            set
            {
                _GlobalView = value;
                this.OnPropertyChanged(nameof(GlobalView));
            }
        }


        public bool IsInit = false;
        public WhiteSecuritySettingViewModel()
        {

        }

        public void Init()
        {
            if (!IsInit)
            {
                //下载黑名单
                GetAllRisBlack();

                //查询当前用户的设置的特殊审批黑名单
                QueryRisFilter();

                IsInit = true;
            }
        }
        #endregion

        #region 查询所有黑名单生成字典

        /// <summary>
        /// 所有的黑名单列表
        /// </summary>
        private ObservableCollection<RisBlackModel> obsRisBlack = new ObservableCollection<RisBlackModel>();
        public ObservableCollection<RisBlackModel> ObsRisBlack
        {
            get
            {
                return obsRisBlack;
            }

            set
            {
                obsRisBlack = value;
                this.OnPropertyChanged(nameof(ObsRisBlack));
            }
        }

        /// <summary>
        /// 债券黑名单字典表
        /// </summary>
        public static Dictionary<string, List<KeyValuePair<string, string>>> DictSecurityidRisBlack = new Dictionary<string, List<KeyValuePair<string, string>>>();

        /// <summary>
        /// RTL信息列表
        /// </summary>
        public static Dictionary<string, string> DictAllRTLMsg = new Dictionary<string, string>();

        /// <summary>
        /// 所有的Key
        /// </summary>
        public Dictionary<string, string> DictKey = new Dictionary<string, string>();

        private bool IsShowDownloadRisBlackOk = false;
        /// <summary>
        /// 下载黑名单到内存
        /// </summary>
        public async void GetAllRisBlack()
        {
            RisBlackResult reply = null;
            DictSecurityidRisBlack.Clear();
            DictAllRTLMsg.Clear();
            await Task.Run(() =>
            {
                reply = RiskHandler.Instance.QueryRisBlack();
            });

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    ObsRisBlack = new ObservableCollection<RisBlackModel>(reply.Datas);
                    foreach (RisBlackModel pbm in reply.Datas)
                    {
                        if (pbm.enable != null)
                        {
                            List<KeyValuePair<string, string>> tmpRisBlack = new List<KeyValuePair<string, string>>();
                            if (DictSecurityidRisBlack.ContainsKey(pbm.securityid))
                            {
                                tmpRisBlack = DictSecurityidRisBlack[pbm.securityid];
                                if (!tmpRisBlack.Contains(new KeyValuePair<string, string>(pbm.code, $"{pbm.code}")))
                                {
                                    tmpRisBlack.Add(new KeyValuePair<string, string>(pbm.code, $"{pbm.message}"));
                                    DictSecurityidRisBlack[pbm.securityid] = tmpRisBlack;
                                }
                            }
                            else
                            {
                                if (!tmpRisBlack.Contains(new KeyValuePair<string, string>(pbm.code, $"{pbm.code}")))
                                {
                                    tmpRisBlack.Add(new KeyValuePair<string, string>(pbm.code, $"{pbm.message}"));
                                    DictSecurityidRisBlack.Add(pbm.securityid, tmpRisBlack);
                                }
                            }

                            if (pbm.code != null)
                            {
                                if (!DictAllRTLMsg.ContainsKey(pbm.code))
                                    DictAllRTLMsg.Add(pbm.code, pbm.message);
                            }
                        }
                    }

                    if (IsShowDownloadRisBlackOk)
                    {
                        showSuccessMessage($"特殊审批黑名单刷新成功!");
                    }

                    IsShowDownloadRisBlackOk = false;
                }
            }
        }

        #endregion

        #region 动态更新债券的RTL
        public void RefreshSecurityid(string securityid)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (DictSecurityidRisBlack.ContainsKey(securityid))
                {
                    DataRowView row = SelectedItem;
                    lock (lockObj)
                    {
                        row.BeginEdit();
                        row[itemSourceField] = DictSecurityidRisBlack[securityid];
                        row.EndEdit();
                        this.GlobalView.AcceptChanges();
                    }
                }
                else
                {
                    DataRowView row = SelectedItem;
                    lock (lockObj)
                    {
                        row.BeginEdit();
                        row[itemSourceField] = new List<KeyValuePair<string, string>>();
                        row.EndEdit();
                        this.GlobalView.AcceptChanges();
                    }
                }
            }));
        }
        #endregion

        #region 查询当前用户的设置的特殊审批黑名单
        public async void QueryRisFilter()
        {
            QueryRisFilterReq req = new QueryRisFilterReq();
            req.username = LocalEnv.UserInfo.UserName;
            RisBlackResult reply = await QueryRisFilterData(req);
            if (reply != null)
            {

                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    DataRow[] drlit2 = this.GlobalView.Select($"id='0'");
                    bool isHaveNew = drlit2.Length > 0 ? true : false;
                    DictKey.Clear();
                    foreach (RisBlackModel pbm in reply.Datas)
                    {
                        lock (lockObj)
                        {
                            // 如果有新增加,则不更新其它记录
                            if (isHaveNew)
                            {
                                if (drlit2[0][securityidField].ToString() == pbm.securityid.ToString() && drlit2[0][codeField].ToString() == pbm.code.ToString())
                                {
                                    DataRow row = drlit2[0];
                                    row.BeginEdit();
                                    row[idField] = pbm.id;
                                    row[usernameField] = pbm.username;
                                    row[securityidField] = pbm.securityid;
                                    row[codeField] = pbm.code;
                                    row[messageField] = pbm.message;
                                    row[updatetimeField] = pbm.updatetime;
                                    row[enableField] = pbm.enable;
                                    row["EditBtnVisible"] = "true";
                                    row["SaveBtnVisible"] = "false";
                                    row[chkMsgField] = "";
                                    row[iscanaddedField] = "false";
                                    row.EndEdit();
                                    this.GlobalView.AcceptChanges();
                                }
                            }
                            else
                            {
                                DataRow[] drlst = this.GlobalView.Select($"id='{pbm.id}'");
                                //如果是修改
                                if (drlst.Length > 0)
                                {
                                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    //{
                                    DataRow row = drlst[0];
                                    row.BeginEdit();
                                    row[securityidField] = pbm.securityid;
                                    row[codeField] = pbm.code;
                                    row[messageField] = pbm.message;
                                    row[updatetimeField] = pbm.updatetime;
                                    row[enableField] = pbm.enable;
                                    row["EditBtnVisible"] = "true";
                                    row["SaveBtnVisible"] = "false";

                                    if (!DictSecurityidRisBlack.ContainsKey(pbm.securityid))
                                    {
                                        row[chkMsgField] = "该债券已恢复交易";
                                    }
                                    else
                                    {
                                        List<string> Lst = DictSecurityidRisBlack[pbm.securityid].Select(x => x.Key).ToList();
                                        if (!Lst.Contains(pbm.code))
                                        {
                                            row[chkMsgField] = "黑名单类别失效";
                                        }
                                    }

                                    //if (string.IsNullOrEmpty(row[chkMsgField].ToString()))
                                    //{
                                    //    row[chkMsgField] = pbm.enable.ToString() == "1" ? "有效" : "无效";
                                    //}

                                    row[iscanaddedField] = "false";
                                    row.EndEdit();
                                    this.GlobalView.AcceptChanges();
                                    //}));
                                }
                                else
                                {

                                    //如果是查询
                                    DataRow row = this.GlobalView.NewRow();
                                    row[idField] = pbm.id;
                                    row[usernameField] = pbm.username;
                                    row[securityidField] = pbm.securityid;
                                    row[codeField] = pbm.code;
                                    row[messageField] = pbm.message;
                                    row[updatetimeField] = pbm.updatetime;
                                    row[enableField] = pbm.enable;
                                    row["EditBtnVisible"] = "true";
                                    row["SaveBtnVisible"] = "false";

                                    if (!DictSecurityidRisBlack.ContainsKey(pbm.securityid))
                                    {
                                        row[chkMsgField] = "该债券已恢复交易";
                                    }
                                    else
                                    {
                                        List<string> Lst = DictSecurityidRisBlack[pbm.securityid].Select(x => x.Key).ToList();
                                        if (!Lst.Contains(pbm.code))
                                        {
                                            row[chkMsgField] = "黑名单类别失效";
                                        }
                                    }

                                    //if (string.IsNullOrEmpty(row[chkMsgField].ToString()))
                                    //{
                                    //    row[chkMsgField] = pbm.enable.ToString() == "1" ? "有效" : "无效";
                                    //}

                                    row[iscanaddedField] = "false";
                                    this.GlobalView.Rows.Add(row);
                                    this.GlobalView.AcceptChanges();

                                }
                            }

                        }
                    }
                }
            }
        }

        public async Task<RisBlackResult> QueryRisFilterData(QueryRisFilterReq req)
        {
            RisBlackResult reply = null;
            await Task.Run(() =>
            {
                reply = RiskHandler.Instance.QueryRisFilter(req);
            });

            return reply;
        }

        #endregion

        #region 新增/修改 特殊审批黑名单 提交到服务器
        /// <summary>
        /// 新增/修改 特殊审批黑名单
        /// </summary>
        public async void SaveResFilter(DataRowView row)
        {
            SetRisFilterReq req = new SetRisFilterReq();
            string id = row[idField].ToString();
            req.id = id == "0" ? "" : id;
            req.username = LocalEnv.UserInfo.UserName;
            req.securityid = row[securityidField].ToString();
            req.code = row[codeField].ToString();
            req.message = row[messageField].ToString();
            req.updatetime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
            req.enable = row[enableField].ToString(); ;
            PResult reply = await CommmitSetRisFilter(req);

            if (reply != null && reply.IsSuccess())
            {
                row.BeginEdit();
                row[iscanaddedField] = "false";
                row["EditBtnVisible"] = "true";
                row["SaveBtnVisible"] = "false";
                row.EndEdit();
                this.GlobalView.AcceptChanges();
                showSuccessMessage($"特殊审批黑名单保存成功!");
            }
            else
            {
                row.BeginEdit();
                row[iscanaddedField] = "true";
                row["EditBtnVisible"] = "false";
                row["SaveBtnVisible"] = "true";
                row.EndEdit();
                this.GlobalView.AcceptChanges();
                STC.Common.View.Component.CustomMessageBox.Error($"特殊审批黑名单保存失败!\r\n{reply.Msg}", "提示", null);
            }

            IsCanSave = true;

            QueryRisFilter();
            //((ManualInput)myWindow).ShowEMSMask(false);
        }

        public async Task<PResult> CommmitSetRisFilter(SetRisFilterReq req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = RiskHandler.Instance.SetRisFilter(req);
            });

            return reply;
        }
        #endregion

        #region 添加
        private void OnAddRecordCommandClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (!IsEdit)
                    {
                        IsEdit = true;
                        IsAdd = true;
                        lock (lockObj)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                DataRow row = this.GlobalView.NewRow();
                                row["id"] = "0";
                                row[enableField] = "1";
                                row[itemSourceField] = new List<KeyValuePair<string, string>>();
                                row["EditBtnVisible"] = "false";
                                row["SaveBtnVisible"] = "true";
                                row[iscanaddedField] = "true";
                                this.GlobalView.Rows.Add(row);
                                this.GlobalView.AcceptChanges();
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

        #region 删除或撤销
        private void OnDelRecordCommandClick()
        {
            lock (GlobalView)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        DataRowView row = SelectedItem;
                        lock (lockObj)
                        {

                            if (row[idField].ToString() == "0")
                            {
                                row.BeginEdit();
                                row.Delete();
                                row.EndEdit();
                                this.GlobalView.AcceptChanges();
                            }
                            else
                            {
                                row.BeginEdit();
                                row["EditBtnVisible"] = "true";
                                row["SaveBtnVisible"] = "false";
                                row.EndEdit();
                                this.GlobalView.AcceptChanges();
                                //查询当前用户的设置的特殊审批黑名单
                                QueryRisFilter();
                            }

                            IsEdit = false;
                            IsAdd = false;
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

        #region 编辑
        private void OnEditRecordCommandClick()
        {
            lock (lockObj)
            {
                try
                {
                    DataRowView row = this.SelectedItem;
                    ChangeStatus(row[enableField].ToString() == "1" ?  true : false);
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }
        }


        public void ChangeStatus(bool isEnable)
        {
            if (!IsEdit)
            {
                IsEdit = true;
                IsAdd = false;
                lock (lockObj)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        DataRowView row = this.SelectedItem;
                        row.BeginEdit();
                        row[enableField] = isEnable ? "1" : "0";
                        row["EditBtnVisible"] = "false";
                        row["SaveBtnVisible"] = "true";
                        row.EndEdit();
                        this.GlobalView.AcceptChanges();
                        this.RefreshSecurityid(row[securityidField].ToString());
                    }));
                }
            }
        }

        #endregion

        #region 保存检查
        private void OnSaveRecordCommandClick()
        {
            DataRowView row = this.SelectedItem;
            string securityid = row[securityidField] as string;

            if (string.IsNullOrWhiteSpace(securityid))
            {
                showMessage("请填入债券代码、黑名单类别、审批信息");
                return;
            }

            if (!DictSecurityidRisBlack.ContainsKey(securityid))
            {
                showMessage("债券代码不在当前债券黑名单字典表中！");
                return;
            }


            string code = row[codeField].ToString();
            if (string.IsNullOrWhiteSpace(code))
            {
                showMessage("请填入黑名单类别");
                return;
            }

            string message = row[messageField].ToString();
            if (string.IsNullOrWhiteSpace(message))
            {
                showMessage("请填入审批信息");
                return;
            }

            byte[] bytestr = System.Text.Encoding.Default.GetBytes(message);
            if (bytestr.Length > 200)
            {
                showMessage("审批信息的长度应小于200个字节");
                return;
            }

            //检查债券代码+黑名单类别是否重复
            DictKey.Clear();
            string key = string.Empty;
            foreach (DataRow dr in this.GlobalView.Rows)
            { 
                key = $"{dr[securityidField].ToString()}#{dr[codeField].ToString()}";
                if (dr[idField].ToString() != row[idField].ToString())
                {
                    if (!DictKey.ContainsKey(key))
                    {
                        DictKey.Add(key, dr[idField].ToString());
                    }
                }
            }

            string key2 = $"{securityid}#{code}";
            if (DictKey.ContainsKey(key2))
            {
                showMessage("债券代码、黑名单类别不能重复!");
                return;
            }


            row.BeginEdit();

            if (string.IsNullOrWhiteSpace(row[iscanaddedField].ToString()))
            {
                row[iscanaddedField] = "true";
            }

            row.EndEdit();

            IsEdit = false;
            IsAdd = false;

            this.GlobalView.AcceptChanges();

            SaveResFilter(row);

        }

        #endregion

        #region 刷新黑名单
        private void OnRefreshCommandClick()
        {
            IsShowDownloadRisBlackOk = true;
            GetAllRisBlack();
                
        }
        #endregion

        #region 下载黑名单
        private void OnDownloadFileCommandClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = $"rtl-feed_{DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss")}";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    ExportCSV(fileName);
                }
            }));
        }

        private void ExportCSV(string fileName)
        {
            try
            {
                using (StreamWriter writer = CreateStream(fileName))
                {
                    // 输出表头
                    writer.WriteLine($"债券代码,黑名单类别,审批信息,是否启用,更新时间");
                    foreach (RisBlackModel rbm in ObsRisBlack)
                    {
                        writer.WriteLine($"\"{rbm.securityid}\",\"{rbm.code}\",\"{rbm.message}\",{rbm.enable},\"{rbm.updatetime}\"");
                    }
                    // 释放资源
                    writer.Flush();
                    writer.Close();
                }

                showSuccessMessage($"特殊审批黑名单导出成功!\r\n{fileName}");

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                STC.Common.View.Component.CustomMessageBox.Error($"特殊审批黑名单导出失败!\r\n{e.Message}", "提示", null);
            }
        }

        public StreamWriter CreateStream(string fileName)
        {
            FileStream fs;
            StreamWriter sw;
            try
            {
                string savePath = Path.GetDirectoryName(fileName);
                string saveFile = fileName;
                //创建路径和文件
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                if (!File.Exists(saveFile))
                {
                    fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                else
                {
                    File.Delete(saveFile);
                    fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                sw = new StreamWriter(fs, Encoding.UTF8);
                return sw;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return null;
        }
        #endregion

        #region 显示消息
        /// <summary>
        /// 提示Warn消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = null;
                me.ShowDialog();
            }));
        }

        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showSuccessMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SuccessBoxView me = new SuccessBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = null;
                me.ShowDialog();
            }));
        }
        #endregion
    }
}

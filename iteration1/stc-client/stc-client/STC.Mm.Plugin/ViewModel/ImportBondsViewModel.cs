using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.Utils.ExcelHelper;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Utils.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.Mm.Plugin.ViewModel
{
    public class ImportBondsViewModel : ViewModelBase
    {
        public MMGroupInfo SelectGroup { get; set; }

        //public ICommand ImportCommand { get; set; }

        #region 获取整组
        private ObservableCollection<GroupEntity> _unifyGroupDic = new ObservableCollection<GroupEntity>();

        public ObservableCollection<GroupEntity> UnifyGroupDic
        {
            get { return _unifyGroupDic; }
            set { _unifyGroupDic = value;
                this.OnPropertyChanged();
            }
        }

        private GroupEntity _SelectedUnifyGroup;
        public GroupEntity SelectedUnifyGroup
        {
            get { 
                return _SelectedUnifyGroup;
            }
            set
            {
                _SelectedUnifyGroup = value;
                this.OnPropertyChanged();
            }
        }

        public async void QueryGroups()
        {
            var groups = await MMGroupHandler.Instance.AsyncRequestGroups();
            foreach(MMGroupInfo info in groups)
            {
                if (info.inf1 == STC.Mm.Plugin.Utils.MMConstants.MM_GROPE_TYPE_UNIFY)
                {
                    GroupEntity gi = new GroupEntity();
                    gi.GroupId = info.id;
                    gi.GroupName = info.groupname;
                    if (SelectedUnifyGroup == null || info.id == this.SelectGroup.id)
                        SelectedUnifyGroup = gi;
                    UnifyGroupDic.Add(gi);
                }
            }
        }

        #endregion

        #region 获取主页
        private ObservableCollection<GroupEntity> _homeGroupDic = new ObservableCollection<GroupEntity>();

        public ObservableCollection<GroupEntity> HomeGroupDic
        {
            get { return _homeGroupDic; }
            set
            {
                _homeGroupDic = value;
                this.OnPropertyChanged();
            }
        }

        private GroupEntity _SelectedHomeGroup;

        public GroupEntity SelectedHomeGroup
        {
            get { return _SelectedHomeGroup; }
            set
            {
                _SelectedHomeGroup = value;
                this.OnPropertyChanged();
            }
        }

        public async void QueryHomeGroups()
        {
            var groups = await HomePageDataHandler.AsyncRequestGroups(true);
            foreach (KeyValuePair<string,HomeGroupInfo> info in groups)
            {
                GroupEntity ge = new GroupEntity();
                ge.GroupId = info.Key;
                ge.GroupName = info.Value.groupname;
                if (SelectedHomeGroup == null)
                    SelectedHomeGroup = ge;

                HomeGroupDic.Add(ge);
            }
            
        }
        #endregion

        private string _CodeText;

        public string CodeText
        {
            get { return _CodeText; }
            set 
            {
                _CodeText = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isHomeChecked = false;

        public bool IsHomeChecked
        {
            get { return _isHomeChecked; }
            set
            {
                _isHomeChecked = value;
                this.OnPropertyChanged();
            }
        }

        public ImportBondsViewModel()
        {
            QueryGroups();
            QueryHomeGroups();
            //ImportCommand = new ComRelayCommand(this.DoImportCommand);
        }

        private bool _ImportEnabled = true;

        public bool ImportEnabled
        {
            get
            {
                return _ImportEnabled;
            }
            set
            {
                this._ImportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private int _checkedCount = 0;

        public int CheckedCount
        {
            get
            {
                return _checkedCount;
            }

            set
            {
                if (_checkedCount != value)
                {
                    _checkedCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _sumCount = 0;

        public int SumCount
        {
            get
            {
                return _sumCount;
            }

            set
            {
                if (_sumCount != value)
                {
                    _sumCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private volatile int successCount = 0;

        public int SuccessCount
        {
            get
            {
                return successCount;
            }

            set
            {
                successCount = value;
                this.OnPropertyChanged();
            }
        }

        private volatile int failCount = 0;

        public int FailCount
        {
            get
            {
                return failCount;
            }

            set
            {
                failCount = value;
                this.OnPropertyChanged();
            }
        }


        public const string ISCHECK_FIELD = "IsCheck";
        public const string SECURITYID_FIELD = "SecurityID";
        public const string SYMBOL_FIELD = "Symbol";
        public const string REMAININGDATE_FIELD = "RemainingDate";

        private object lockObj = new object();

        private DataView _ImportDV;
        public DataView ImportDv
        {
            get
            {
                lock (lockObj)
                {
                    if (_ImportDV == null)
                    {
                        _ImportDV = new DataTable().DefaultView;
                        _ImportDV.Table.Columns.Add(ISCHECK_FIELD, typeof(bool));
                        _ImportDV.Table.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _ImportDV.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _ImportDV.Table.Columns.Add(REMAININGDATE_FIELD, typeof(string));


                        _ImportDV.Table.PrimaryKey = new DataColumn[] {
                            _ImportDV.Table.Columns[SECURITYID_FIELD]
                        };
                        this.OnPropertyChanged();
                    }
                    return this._ImportDV;
                }
            }
        }

        private string[] splitStrArr = new string[] { "\n" };
        public void BindBonds(string importStr)
        {
            List<string> bonds = new List<string>();
            if (IsHomeChecked)
            {
                // 根据选中HomeGroup
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = HomePageDataHandler.RequestGroups(false);
                HomeGroupInfo homeGroupInfo = HomePageDataHandler.GetHomeGroupInfo(dic, this.SelectedHomeGroup.GroupId);
                if (homeGroupInfo != null && homeGroupInfo.securityids != null && homeGroupInfo.securityids.Count > 0)
                {
                    foreach(HomeGroupSecurityIDsInfo homeSecurtity in homeGroupInfo.securityids)
                    {
                        bonds.Add(homeSecurtity.securityid);
                    }
                }
            }
            else
            {
                importStr = importStr.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
                string[] bondArr = importStr.Split(splitStrArr, StringSplitOptions.RemoveEmptyEntries);
                bonds.AddRange(bondArr);
            }
            
            foreach (string bond in bonds)
            {
                try
                {
                    string newBond = CommonUtils.AdjustSecurityID(bond);

                    BondsInfoPo bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(newBond.ToUpper());
                    //如果合约代码用的是小写
                    if (bondInfoPo == null)
                    {
                        bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(newBond.ToLower());
                    }

                    String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                   
                    DataRow dr = this.ImportDv.Table.Rows.Find(bondInfoPo == null ? newBond : bondInfoPo.Bond_ID);
                    if (dr == null)
                    {
                        dr = this.ImportDv.Table.NewRow();
                        dr[ISCHECK_FIELD] = true;
                        dr[SECURITYID_FIELD] = bondInfoPo == null ? newBond : bondInfoPo.Bond_ID;
                        dr[SYMBOL_FIELD] = bondInfoPo == null ? string.Empty : bondInfoPo.Short_Name;
                        dr[REMAININGDATE_FIELD] = bondInfoPo == null ? string.Empty : bondInfoPo.RemaingingTime;
                        this.ImportDv.Table.Rows.Add(dr);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error($"识别失败:{bond}", e);
                }
            }
        }

        public void ClearBinds()
        {
            this.ImportDv.Table.Clear();
        }

        public bool DoImportCommand()
        {
            STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
            try
            {
                this.ImportEnabled = false;
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();                              //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "Excel Documents|*.xls;*.xlsx";
                ofd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !ofd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return false;
                    }

                    // 3.导入操作
                    StringBuilder sb = new StringBuilder();

                    // 1 检查文件是否被占用
                    bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(ofd.FileName);
                    if (isFileLock)
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = $"{ofd.FileName} 被占用，请关闭后再试。";
                        prompt.Show();
                        return false;
                    }

                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(ofd.FileName);

                    Dictionary<int, string> dic = helper.ReturnSheetList();

                    DataTable dt = null;
                    if (dic.ContainsKey(0))
                    {
                        dt = helper.ExcelToDataTable(0);
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            StringBuilder lineSB = new StringBuilder();
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (dc.ColumnName == "债券代码")
                                {
                                    string value = dr[dc.ColumnName].ConvertToString();
                                    value = value.Trim('\"');

                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        if (lineSB.Length > 0)
                                        {
                                            lineSB.Append(" ");
                                        }
                                        lineSB.Append(value);
                                    }
                                }
                            }
                            sb.AppendLine(lineSB.ToString());
                        }
                    }
                    else
                    {
                        promptTxt = "无数据或文件有误或文件被占用";
                    }

                    CodeText = sb.ToString();
                    //if (sb.Length > 0)
                    //{
                    //    await Task.Run(() =>
                    //    {
                    //        IntentionOldHandler.Instance.ImportIntentionQuote(sb.ToString(), dt);
                    //    });
                    //}


                    //if (errorList != null && errorList.Count > 0)
                    //{
                    //    StringBuilder errors = new StringBuilder();
                    //    foreach (string error in errorList)
                    //    {
                    //        if (errors.Length > 0)
                    //        {
                    //            errors.AppendLine();
                    //        }
                    //        errors.Append(error);
                    //    }
                    //    promptTxt = errors.ToString();
                    //}
                    //else
                    //{
                    //    promptTxt = "导入成功";
                    //}
                }
                else
                {
                    return false;
                }

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导入";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                    return false;
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导入";
                prompt.PromptContext = e.Message;
                prompt.Show();

                LogHelper.Error(e);
                return false;
            }
            finally
            {

                this.ImportEnabled = true;
                    if (helper != null)
                        helper.Dispose();
            }
            return true;
        }

        public void CalCheckedCount()
        {
            this.SumCount = this.ImportDv.Count;

            int sumCount = 0;
            foreach (DataRowView drv in this.ImportDv)
            {
                if ((bool)drv[ISCHECK_FIELD] == true)
                {
                    sumCount++;
                }
            }
            this.CheckedCount = sumCount;
        }

        private ConcurrentDictionary<HomeGroupSecurityidsUpdateReq, PResult> resultDic = new ConcurrentDictionary<HomeGroupSecurityidsUpdateReq, PResult>();


        private volatile string resultStr = string.Empty;

        public string ResultStr
        {
            get
            {
                return resultStr;
            }
            set
            {
                resultStr = value;
                this.OnPropertyChanged();
            }
        }

        private volatile bool btnOK3Enabled = false;

        public bool BtnOK3Enabled
        {
            get
            {
                return btnOK3Enabled;
            }
            set
            {
                btnOK3Enabled = value;
                this.OnPropertyChanged();
            }
        }

        private volatile bool _isCanceled = false;

        public void CancelOpt()
        {
            _isCanceled = true;
        }

        public bool FinishOpt()
        {
            return resultDic.Count == ImportDv.Count;
        }

        public void BatchImportBonds()
        {
            BtnOK3Enabled = false;

            MMGroupInfo saveGroupInfo = MMGroupHandler.Instance.GetGroupInfoByGroupID(this.SelectedUnifyGroup.GroupId);
            
            MMOrderParamReq param;
            try
            {
                param = JsonConvert.DeserializeObject<MMOrderParamReq>(saveGroupInfo.inf2);
            }
            catch
            {
                param = new MMOrderParamReq();
                //param.InitData();
                MMCommonUtils.MMOrderParamReqToDefault(param);
            }

            if (ImportDv != null && ImportDv.Count > 0)
            {
                List<STC.Common.Model.MM.MM> reqList = new List<STC.Common.Model.MM.MM>();
                foreach (DataRowView drv in ImportDv)
                {
                    if ((bool)drv[ImportBondsViewModel.ISCHECK_FIELD] == true)
                    {
                        STC.Common.Model.MM.MM request = new STC.Common.Model.MM.MM();
                        request.Action = "1"; // 添加
                        MMOrderParamReq req = param.Clone();
                        request.securityid = req.securityid = CommonUtils.AdjustSecurityID(CommonUtils.ConvertToString(drv[ImportBondsViewModel.SECURITYID_FIELD]));
                        request.symbol = CommonUtils.ConvertToString(drv[ImportBondsViewModel.SYMBOL_FIELD]);
                        request.groupid = saveGroupInfo.id;
                        request.settltype = "2";
                        request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                        request.enable = "0";
                        request.Params = req;
                        reqList.Add(request);
                    }
                }
                if (reqList.Count > 0)
                {
                    ImportBonds(reqList);
                }
            }
        }

        public async void ImportBonds(List<STC.Common.Model.MM.MM> reqList)
        {
            List<STC.Common.Model.MM.MM> existMMs = MMGroupHandler.Instance.GetMMsByGroupID(this.SelectedUnifyGroup.GroupId);

            try
            {
                MMReq request = new MMReq();
                request.Action = "1";
                request.mmList = new List<STC.Common.Model.MM.MM>();

                foreach (STC.Common.Model.MM.MM req in reqList)
                {
                    if (existMMs != null && existMMs.Any(item => item.securityid == req.securityid))
                    {
                        FailCount = FailCount + 1;
                        ResultStr += $"当前组下债券{req.securityid}已存在" + "\r\n";
                    }
                    else if (CommonUtils.GetBondsInfoPoBySecurityID(req.securityid) == null
                        || !CommonUtils.IsCIB(CommonUtils.GetBondsInfoPoBySecurityID(req.securityid)))
                    {
                        FailCount = FailCount + 1;
                        ResultStr += $"债券{req.securityid}无效" + "\r\n";
                    }
                    else
                    {
                        request.mmList.Clear();
                        request.mmList.Add(req);
                        PResult pResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateSecurityids(request);
                        if (pResult != null && pResult.IsSuccess())
                        {
                            SuccessCount = successCount + 1;
                            foreach (MM mm in request.mmList)
                            {
                                BondsInfoPo bip = CommonUtils.GetBondsInfoPoBySecurityID(mm.securityid);
                                string remainingTime = bip == null ? string.Empty : bip.RemaingingTime;
                                ResultStr += $"{mm.securityid} {mm.symbol} {remainingTime}\r\n";
                            }
                        }
                        else
                        {
                            foreach (MM mm in request.mmList)
                            {
                                string errMsg = (pResult != null && !string.IsNullOrEmpty(pResult.Msg)) ? pResult.Msg : $"{mm.securityid}导入失败";
                                FailCount = FailCount + 1;
                                ResultStr += $"{errMsg}\r\n";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            finally
            {
                BtnOK3Enabled = true;
            }
        }
    }

    public class GroupEntity : ViewModelBase
    {
        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value;
                this.OnPropertyChanged();
            }
        }

        private string _groupid;

        public string  GroupId
        {
            get { return _groupid; }
            set { _groupid = value;
                this.OnPropertyChanged();
            }
        }
    }
}

using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Frame.UI;
using STC.Frame.Utils.ExcelHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Frame.ViewModel
{
    public class SecurityGroupManagerViewModel : ViewModelBase
    {
        private CollectionViewSource groupVS = new CollectionViewSource();
        public CollectionViewSource GroupVS
        {
            get { return groupVS; }
            set
            {
                groupVS = value;
                this.OnPropertyChanged(nameof(GroupVS));
            }
        }
        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();
                    this.GroupVS.View.Refresh();
                }
            }
        }
        private bool _DynamicsGroupChecked = false;

        public bool DynamicsGroupChecked
        {
            get { return _DynamicsGroupChecked; }
            set
            {
                if (_DynamicsGroupChecked != value)
                {
                    _DynamicsGroupChecked = value;
                    refreshShowAll();
                    this.OnPropertyChanged();
                    this.GroupVS.View.Refresh();
                }
            }
        }
        private bool _AllInstGroupChecked = false;

        public bool AllInstGroupChecked
        {
            get { return _AllInstGroupChecked; }
            set
            {
                if (_AllInstGroupChecked != value)
                {
                    _AllInstGroupChecked = value;
                    refreshShowAll();
                    this.OnPropertyChanged();
                    this.GroupVS.View.Refresh();
                }
            }
        }
        private bool _IsMyselfGroupChecked = false;

        public bool IsMyselfGroupChecked
        {
            get { return _IsMyselfGroupChecked; }
            set
            {
                if (_IsMyselfGroupChecked != value)
                {
                    _IsMyselfGroupChecked = value;
                    refreshShowAll();
                    this.OnPropertyChanged();
                    this.GroupVS.View.Refresh();
                }
            }
        }
        private int _TagSelectedIndex;

        public int TagSelectedIndex
        {
            get { return _TagSelectedIndex; }
            set
            {
                if (_TagSelectedIndex != value)
                {
                    _TagSelectedIndex = value;
                    refreshShowAll();
                    this.OnPropertyChanged();
                    this.GroupVS.View.Refresh();
                }
            }
        }

        private bool _IsShowAll = true;

        public bool IsShowAll
        {
            get { return _IsShowAll; }
            set
            {
                if (_IsShowAll != value)
                {
                    _IsShowAll = value;
                    this.OnPropertyChanged();
                    if (IsShowAll == true) 
                    {
                        RefreshCondition();
                    }
                }
            }
        }
        public void RefreshCondition()
        {
            TagSelectedIndex = 0;
            DynamicsGroupChecked = false;
            AllInstGroupChecked = false;
            IsMyselfGroupChecked = false;
        }

        private void refreshShowAll() 
        {
            if (TagSelectedIndex == 0 && DynamicsGroupChecked == false && AllInstGroupChecked == false && IsMyselfGroupChecked == false)
            {
                IsShowAll = true;
            }
            else 
            {
                IsShowAll = false;
            }
        }

        private object _lockGObj = new object();
        public object LockGObj
        {
            get
            {
                return _lockGObj;
            }
        }
        
        private HomeGroupData _selectedGroupItem;
        public HomeGroupData SelectedGroupItem
        {
            get
            {
                return _selectedGroupItem;
            }

            set
            {
                _selectedGroupItem = value;
                this.OnPropertyChanged();
            }
        }
        public SecurityGroupManagerView securityGroupManagerView = null;
        //策略债券组
        public const string G_Id = "Id";
        public const string G_Groupname = "Groupname";
        public const string G_BondNum = "BondNum";
        public const string G_BondCode = "BondCode";
        public const string G_updatetime = "updatetime";
        public const string G_GroupTextVisible = "GroupTextVisible";
        public const string G_GroupTextVisibleNo = "GroupTextVisibleNo";
        public const string G_IsChecked = "IsChecked";
        public const string G_BondList = "BondList";
        public const string G_Tag = "Tag";
        public const string G_IsDynamicsGroup = "IsDynamicsGroup";
        public const string G_IsAllInstGroup = "IsAllInstGroup";
        public const string G_IsCareGroup = "IsCareGroup";
        public const string G_Ordersn = "Ordersn";


        public const string CLOSEBY_FIELD = "Closeby";
        public const string USERNAME_FIELD = "Username";
        public const string ShowDeleteBtn = "ShowDeleteBtn";
        private ObservableCollection<HomeGroupData> _HomeGroups = new ObservableCollection<HomeGroupData>();

        public ObservableCollection<HomeGroupData> HomeGroups
        {
            get
            {
                if (_HomeGroups == null)
                    _HomeGroups = new ObservableCollection<HomeGroupData>();

                return _HomeGroups;
            }

            set
            {
                _HomeGroups = value;
                this.OnPropertyChanged(nameof(HomeGroups));
                this.GroupVS.View.Refresh();
            }
        }


        public static Dictionary<string, HomeGroupInfo> dicGroup = new Dictionary<string, HomeGroupInfo>();
        public ICommand AddCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        
        public string defaultTag = "";

        public SecurityGroupManagerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(this.HomeGroups, this.LockGObj);
            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
            this.AddCommand = new ComRelayCommand(this.AddClick);
            this.ImportCommand = new ComRelayCommand(this.ImportClick);
            this.ExportCommand = new ComRelayCommand(this.ExportClick);
        }
        internal void load()
        {
            this.GroupVS.Source = HomeGroups;
            this.GroupVS.View.CollectionChanged += View_CollectionChanged;
            this.GroupVS.IsLiveFilteringRequested = true;
            this.GroupVS.IsLiveSortingRequested = true;
            this.GroupVS.SortDescriptions.Add(new SortDescription("inf1", ListSortDirection.Ascending));
            this.GroupVS.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));
            FilterData();
        }

        private void View_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        public void initGroup()
        {
            dicGroup.Clear();
            HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
            //request.type = QUANT_Type;
            if ("*".Equals(AdminHandler.Instance.getSecurityGroupUser()))
            {
                request.username = "";
            }
            else
            {
                request.username = LocalEnv.UserInfo.UserName;
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery(request);
                    if (replyPo != null)
                    {
                        if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
                        {
                            var templist = replyPo.Datas.OrderByDescending(a => a.Value.updatetime).ToList();
                            foreach (KeyValuePair<string, HomeGroupInfo> pair in templist)
                            {
                                HomeGroupInfo homeGroupInfo = pair.Value;
                                if (!string.IsNullOrWhiteSpace(homeGroupInfo.inf2))
                                {
                                    GroupMsgPo groupMsgPo = JsonConvert.DeserializeObject<GroupMsgPo>(homeGroupInfo.inf2);
                                    Dictionary<string, string> tagTempDic = new Dictionary<string, string>();
                                    if (!string.IsNullOrWhiteSpace(groupMsgPo.Tag))
                                    {
                                        string[] splits = groupMsgPo.Tag.Split(' ');
                                        foreach (string tag in splits)
                                        {
                                            if (Consts.TagDic.ContainsValue(tag))
                                            {
                                                string key = Consts.TagDic.FirstOrDefault(q => q.Value.Equals(tag)).Key;
                                                tagTempDic.Add(key, tag);
                                            }
                                        }
                                    }
                                    homeGroupInfo.tagDic = tagTempDic;
                                    homeGroupInfo.groupMsgPo = groupMsgPo;
                                }
                                if (!dicGroup.ContainsKey(homeGroupInfo.id))
                                    dicGroup.Add(homeGroupInfo.id, homeGroupInfo);
                                else
                                {
                                    dicGroup[homeGroupInfo.id] = homeGroupInfo;
                                }
                                UpdateGroupList(homeGroupInfo);
                            }
                            Application.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                GroupVS.View.Refresh();
                                if (this.SelectedGroupItem == null)
                                    this.SelectedGroupItem = HomeGroups.Count() > 0 && GroupVS.View != null ? GroupVS.View.Cast<HomeGroupData>().FirstOrDefault() : null;
                            }));
                        }
                    }
                }
                catch (Exception e) 
                {
                    LogHelper.Error("homeGroupSecurityidsQuery error:",e);
                }
            });
        }
        private void UpdateGroupList(HomeGroupInfo obj)
        {
            try
            {
                lock (LockGObj)
                {
                    HomeGroupData dataRow = this.HomeGroups.FirstOrDefault(o => o.id == obj.id);
                    bool isold = false;
                    if (dataRow != null)
                    {
                        isold = true;
                    }
                    else
                    {
                        dataRow = new HomeGroupData();
                        dataRow.id = obj.id;
                    }
                    dataRow.groupname = obj.groupname;
                    dataRow.ordersn = obj.ordersn;
                    dataRow.closeby = obj.closeby;
                    dataRow.username = obj.username;
                    dataRow.updatetime = obj.updatetime;
                    dataRow.inf1 = obj.inf1;
                    dataRow.inf2 = obj.inf2;
                    dataRow.tagDic = obj.tagDic;
                    dataRow.groupMsgPo = obj.groupMsgPo;
                    dataRow.securityids = obj.securityids;
                    dataRow.IsMyselfGroup = LocalEnv.UserInfo.UserName.Equals(obj.username) ? "true" : "false";
                    dataRow.IsHomeTag = obj.tagDic != null && obj.tagDic.ContainsKey(CommonConstants.HOME_Type)?"true":"false";
                    dataRow.IsINNERTag = obj.tagDic != null && obj.tagDic.ContainsKey(CommonConstants.INNER_Type) ? "true" : "false";
                    dataRow.IsQuantTag = obj.tagDic != null && obj.tagDic.ContainsKey(CommonConstants.QUANT_Type) ? "true" : "false";
                    dataRow.IsMMTag = obj.tagDic != null && obj.tagDic.ContainsKey(CommonConstants.MM_Type) ? "true" : "false";
                    dataRow.IsMMSHETag = obj.tagDic != null && obj.tagDic.ContainsKey(CommonConstants.MMSHE_Type) ? "true" : "false";
                    if (!isold)
                    {
                        HomeGroups.Insert(0, dataRow);
                    }
                    else 
                    {
                        if (SelectedGroupItem != null && SelectedGroupItem.id.Equals(obj.id))
                        {
                            securityGroupManagerView.detailView.initBondList(SelectedGroupItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
       
        public void clearGroup()
        {
            try
            {
                lock (LockGObj)
                {
                    this.HomeGroups.Clear();
                    //this.GroupView.Table.AcceptChanges();
                    //this.GroupVS.View?.Refresh();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        private async void ExportClick()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"关注组导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
                //设置保存的文件的类型，注意过滤器的语法
                sfd.Filter = "Excel Documents|*.xls;*.xlsx;";
                sfd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!sfd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !sfd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        CommonUtils.Prompt2Second("批量导出", "当前只支持xls文件和xlsx文件");
                        return;
                    }

                    List<string> errorList = await Sub_ExportExcel(sfd.FileName);

                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导出成功";
                    }
                }
                else
                {
                    promptTxt = "取消导出";
                }

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    CommonUtils.Prompt2Second("批量导出", promptTxt);
                }
            }
            catch (Exception e)
            {
                CommonUtils.Prompt2Second("批量导出", e.Message);

                LogHelper.Error(e);
            }
        }
        private Task<List<string>> Sub_ExportExcel(string filePath)
        {
            return Task.Run(() =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    if (File.Exists(filePath))
                    {
                        bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                        if (isFileLock)
                        {
                            errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                            return errorlist;
                        }

                        File.Delete(filePath);
                    }


                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    if (dicGroup != null)
                    {
                        Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                        foreach (KeyValuePair<string, HomeGroupInfo> pair in dicGroup)
                        {
                            if (!CommonConstants.HOME_Type.Equals(pair.Value.inf1) && !CommonConstants.QUANT_Type.Equals(pair.Value.inf1)) 
                            {
                                continue;
                            }
                            DataTable dt = new DataTable();
                            dt.Columns.Add(ExcelExportUtils.Excel_SecurityIdField);
                            dt.Columns.Add(ExcelExportUtils.Excel_SymbolField);
                            dt.Columns.Add(ExcelExportUtils.Excel_IsHomeField);

                            foreach (HomeGroupSecurityIDsInfo info in pair.Value.securityids)
                            {
                                DataRow newDR = dt.NewRow();
                                newDR[ExcelExportUtils.Excel_SecurityIdField] = info.securityid;
                                newDR[ExcelExportUtils.Excel_SymbolField] = info.symbol;
                                newDR[ExcelExportUtils.Excel_IsHomeField] = CommonConstants.HOME_Type.Equals(pair.Value.inf1)?"是":"否";
                                dt.Rows.InsertAt(newDR, 0);
                            }

                            dataDic[pair.Value.groupname] = dt;
                        }
                        Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                        if (result == null) errorlist.Add("无数据导出");
                    }
                    else
                    {
                        errorlist.Add("无数据导出");
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }
        private async void ImportClick()
        {
            try
            {
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
                        CommonUtils.Prompt2Second("批量导入", "当前只支持xls文件和xlsx文件");
                        return;
                    }

                    // 2.等待

                    // 3.导入操作
                    List<string> errorList = await Sub_ImportExcel(ofd.FileName);


                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导入成功";
                    }
                }
                else
                {
                    promptTxt = "取消导入";
                }
                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    CommonUtils.Prompt2Second("批量导入", promptTxt);
                }
            }
            catch (Exception e)
            {
                CommonUtils.Prompt2Second("批量导入", e.Message);
                LogHelper.Error(e);
            }
        }
        private Task<List<string>> Sub_ImportExcel(string filePath)
        {
            return Task<List<string>>.Run(async () =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                    if (isFileLock)
                    {
                        errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                        return errorlist;
                    }

                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);

                    Dictionary<int, string> dic = helper.ReturnSheetList();

                    if (dic.Count > 20)
                    {
                        errorlist.Add("分组总数应不超过20个");
                        return errorlist;
                    }

                    // 3.读文件
                    if (dic.Count > 0)
                    {
                        bool isContainHomeGroup = false;
                        //记录需删除组
                        List<KeyValuePair<string, HomeGroupInfo>> removeGroupList = new List<KeyValuePair<string, HomeGroupInfo>>();

                        //需要删除的组
                        foreach (KeyValuePair<string, HomeGroupInfo> pair in dicGroup) 
                        {
                            if (CommonConstants.HOME_Type.Equals(pair.Value.inf1) || CommonConstants.QUANT_Type.Equals(pair.Value.inf1)) 
                            {
                                removeGroupList.Add(pair);
                            }
                        }
                        // 1.删除原有组
                        List<string> updateRemoveHomeGroupList = new List<string>();
                        foreach (KeyValuePair<string, HomeGroupInfo> removeGroupID in removeGroupList)
                        {
                            if (!isContainHomeGroup && CommonConstants.HOME_Type.Equals(removeGroupID.Value.inf1)) 
                            {
                                isContainHomeGroup = true;
                            }
                            HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                            request.id = removeGroupID.Key;//组id
                            request.action = "3";//删除
                            request.username = removeGroupID.Value.username;
                            request.closeby = LocalEnv.UserInfo.UserName;
                            PResult pResult = HomePageDataHandler.UpdateGroups(request);
                            if (pResult != null && pResult.IsSuccess())
                            {
                                if (CommonConstants.HOME_Type.Equals(removeGroupID.Value.inf1))
                                {
                                    updateRemoveHomeGroupList.Add(removeGroupID.Key);
                                }
                            }
                        }

                        //新增组
                        int ordersn = 0;
                        Dictionary<string, List<string>> importDic = new Dictionary<string, List<string>>();
                        foreach (KeyValuePair<int, string> groupPair in dic)
                        {
                            DataTable importDT = helper.ExcelToDataTable(groupPair.Key);
                            bool isHomeGroup = false;
                            if (importDT.Rows.Count > 0)
                            {
                                isHomeGroup = ExcelExportUtils.YES_VALUE.Equals(importDT.Rows[0][ExcelExportUtils.Excel_IsHomeField]) ? true : false;
                                if (isHomeGroup && !isContainHomeGroup) 
                                {
                                    isContainHomeGroup = true;
                                }
                            }
                            // 2.添加新组
                            HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                            request.groupname = groupPair.Value;//组名
                            request.action = "1";//新增
                            if (isHomeGroup)
                            {
                                request.ordersn = ordersn.ToString();//顺序
                            }
                            request.terminal = LocalEnv.ServerInfo.Version; // 终端
                            GroupMsgPo groupMsgPo = new GroupMsgPo();
                            groupMsgPo.Tag = isHomeGroup ? "主页" : "";
                            groupMsgPo.IsCareGroup = isHomeGroup?"true":"false";
                            groupMsgPo.IsAllInstGroup = "false";
                            groupMsgPo.IsDynamicsGroup = "false";
                            request.inf2 = JsonConvert.SerializeObject(groupMsgPo);
                            request.type = isHomeGroup ? CommonConstants.HOME_Type : CommonConstants.QUANT_Type;
                            request.username = LocalEnv.UserInfo.UserName;
                            request.closeby = LocalEnv.UserInfo.UserName;
                            HomeGroupUpdateReplyPo pResult = AdminHandler.Instance.homeGroupUpdate(request);
                            if (pResult != null && pResult.Code == "0000" && pResult.Datas != null && !string.IsNullOrEmpty(pResult.Datas.id))
                            {
                                if (isHomeGroup)
                                {
                                    if (!importDic.ContainsKey(pResult.Datas.id))
                                    {
                                        importDic[pResult.Datas.id] = new List<string>();
                                    }
                                }
                                // 3.向组里导入债券
                                foreach (DataRow dr in importDT.Rows)
                                {
                                    HomeGroupSecurityidsUpdateReq addBondRequest = new HomeGroupSecurityidsUpdateReq();
                                    addBondRequest.securityid = CommonUtils.ConvertToString(dr[ExcelExportUtils.Excel_SecurityIdField]);
                                    addBondRequest.symbol = CommonUtils.ConvertToString(dr[ExcelExportUtils.Excel_SymbolField]);
                                    addBondRequest.action = "1"; // 添加
                                    addBondRequest.remark = $"{addBondRequest.securityid}  {addBondRequest.symbol}";
                                    addBondRequest.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                                    addBondRequest.groupid = pResult.Datas.id;
                                    addBondRequest.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                                    addBondRequest.type = isHomeGroup ? CommonConstants.HOME_Type : CommonConstants.QUANT_Type;
                                    addBondRequest.username = LocalEnv.UserInfo.UserName;
                                    PResult result = HomePageDataHandler.UpdateSecurityids(addBondRequest);
                                    if (result != null && result.IsSuccess())
                                    {
                                        if (isHomeGroup)
                                        {
                                            importDic[pResult.Datas.id].Add(addBondRequest.securityid);
                                        }
                                    }
                                }
                            }
                            if (isHomeGroup)
                            {
                                ordersn++;



                            }
                        }

                        // 5.更新界面
                        if (isContainHomeGroup)
                        {
                            HomePageDataHandler.RequestGroups(true);
                        }

                        foreach (KeyValuePair<string, List<string>> pair in importDic)
                        {
                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddGroup, pair.Key, string.Empty);

                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.BatchImport, pair.Key, string.Empty);

                            foreach (string securityID in pair.Value)
                            {
                                HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddBond, pair.Key, securityID);
                            }
                        }
                        if (importDic.Count() <= 0) 
                        {
                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.ManagerBatchImport, string.Empty, string.Empty);
                        }
                        foreach (string removeGroupID in updateRemoveHomeGroupList)
                        {
                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelGroup, removeGroupID, string.Empty);
                        }
                        initGroup();
                    }
                    else
                    {
                        errorlist.Add("没有可导入组");
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }

        private void AddClick()
        {
            AddGroupWindow addGroupWindow = new AddGroupWindow();
            addGroupWindow.comTag.setData(Consts.TagDic);
            addGroupWindow.setDefaultTag(defaultTag);
            //addGroupWindow.defaultTag = 
            addGroupWindow.ShowDialog();
            if (addGroupWindow.Result)
            {
                if (addGroupWindow.Result == true)
                {
                    
                    HomePageDataHandler.AddGroup(addGroupWindow.GroupName,addGroupWindow.groupMsgPo);
                }
            }
        }
        public static bool IsGroupNameExist(string groupName,string groupid)
        {
            if (dicGroup != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> pair in dicGroup)
                {
                    if (string.IsNullOrWhiteSpace(groupid))
                    {
                        if (pair.Value.groupname == groupName && LocalEnv.UserInfo.UserName.Equals(pair.Value.username) && !CommonConstants.MM_Type.Equals(pair.Value.inf1) && !CommonConstants.MMSHE_Type.Equals(pair.Value.inf1))
                        {
                            return true;
                        }
                    }
                    else 
                    {
                        if (pair.Value.groupname == groupName && !pair.Value.id.Equals(groupid) && !CommonConstants.MM_Type.Equals(pair.Value.inf1) && !CommonConstants.MMSHE_Type.Equals(pair.Value.inf1))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static List<HomeGroupInfo> GetHomeGroupInfo(string condition, int resultNum)
        {
            List<HomeGroupInfo> result = new List<HomeGroupInfo>();

            if (dicGroup == null)
                return result;

            lock (dicGroup)
            {
                IEnumerable<KeyValuePair<string, HomeGroupInfo>> v = v = (from d in dicGroup where d.Value.groupname.ToLower().Contains(condition.ToLower()) || d.Value.id.ToLower().Contains(condition.ToLower()) select d);      
                int i = 0;
                foreach (KeyValuePair<string, HomeGroupInfo> kv in v)
                {
                    if (i < resultNum)
                        result.Add(kv.Value);
                    else
                        break;

                    i++;
                }
            }

            return result;
        }

        private void FilterData()
        {
            this.GroupVS.View.Filter = item =>
            {
                bool result = true;
                HomeGroupData data = item as HomeGroupData;
                if (!string.IsNullOrEmpty(this.SearchText))
                {
                    if (!SearchText.Equals(data.groupname))
                    {
                        return false;
                    }
                }
                if (TagSelectedIndex != 0)
                {
                    Dictionary<string, string> tagTempDic = data.tagDic != null ? data.tagDic : new Dictionary<string, string>();
                    if (TagSelectedIndex == 1 && !tagTempDic.ContainsKey(CommonConstants.HOME_Type))
                    {
                        return false;
                    }
                    if (TagSelectedIndex == 2 && !tagTempDic.ContainsKey(CommonConstants.INNER_Type))
                    {
                        return false;
                    }
                    if (TagSelectedIndex == 3 && !tagTempDic.ContainsKey(CommonConstants.QUANT_Type))
                    {
                        return false;
                    }
                    if (TagSelectedIndex == 4 && !tagTempDic.ContainsKey(CommonConstants.MM_Type))
                    {
                        return false;
                    }
                    if (TagSelectedIndex == 5 && !tagTempDic.ContainsKey(CommonConstants.MMSHE_Type))
                    {
                        return false;
                    }
                }
                if (DynamicsGroupChecked)
                {
                    if (!(data.groupMsgPo != null && "true".Equals(data.groupMsgPo.IsDynamicsGroup)))
                    {
                        return false;
                    }
                }
                if (AllInstGroupChecked)
                {
                    if (!(data.groupMsgPo != null && "true".Equals(data.groupMsgPo.IsAllInstGroup)))
                    {
                        return false;
                    }
                }
                if (IsMyselfGroupChecked)
                {
                    if (!"true".Equals(data.IsMyselfGroup))
                    {
                        return false;
                    }
                }
                return result;
            };
        }
        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum favorityType, string groupID, string securityID)
        {
            if (favorityType == HomePageDataHandler.FavoriteTypeEnum.DelGroup 
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.UpdateGroup
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.AddGroup
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.AddBond
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.DelBond
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.BatchImport)
            {
                if (favorityType == HomePageDataHandler.FavoriteTypeEnum.DelGroup) 
                {
                    clearGroup();
                }
                initGroup();
            }
        }
    }
}

using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Quant;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.ViewModel;
using STC.quant.plugin.Model;
using STC.quant.plugin.Utils;
using STC.quant.plugin.Utils.Data;
using STC.quant.plugin.View.HomePage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.quant.plugin.ViewModel
{
    public class AlterParamViewModel : BaseViewModel
    {
        private static AlterParamViewModel alterParamViewModel = new AlterParamViewModel();
        public static AlterParamViewModel Instance { get { return alterParamViewModel; } }

        public string defaultTag = "";

        public ICommand AddGroupCommand { get { return new RelayCommand(AddGroupClick); } }
        public ICommand DelGroupCommand { get { return new RelayCommand(DelGroupClick); } }
        public ICommand EditGroupCommand { get { return new RelayCommand(EditGroupBondClick); } }
        //public ICommand EditGroupNameCommand { get { return new RelayCommand(EditGroupNameClick); } }//编辑组名
        //public ICommand EditGroupNameOKCommand { get { return new RelayCommand(EditGroupNameOKClick); } }//更改组名
        
       // public ICommand AlterGroupCommand { get { return new RelayCommand(AlterGroupBondClick); } }


        public ICommand AddBondCommand { get { return new RelayCommand(AddBondClick); } }
        public ICommand DelBondCommand { get { return new RelayCommand(DelBondClick); } }
        public ICommand BondOKCommand { get { return new RelayCommand(OkBondClick); } }
        public ICommand BondCancelCommand { get { return new RelayCommand(CancelBondClick); } }


        public ICommand BatchImportBondCommand { get { return new RelayCommand(BatchImportBondClick); } }
        public ICommand ImportBondCommand { get { return new RelayCommand(ImportBondClick); } }
        public ICommand ExportBondCommand { get { return new RelayCommand(ExportBondClick); } }

        public Dictionary<string, HomeGroupInfo> dicGroup = new Dictionary<string, HomeGroupInfo>();

        public BatachAddBondTab1 batachAddBondTab1 { get; set; }

        #region treeview控件
        //private ObservableCollection<TreeNodeModel> _TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        //public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        //{
        //    get { return _TreeComboxItemsSource; }
        //    set
        //    {
        //        if (_TreeComboxItemsSource != value)
        //        {
        //            _TreeComboxItemsSource = value;
        //            this.OnPropertyChanged("MultiTreeComboxItemsSource");
        //        }
        //    }
        //}
        //private string _SelectedItem = null;
        //public string SelectedItem
        //{
        //    get { return _SelectedItem; }
        //    set
        //    {
        //        _SelectedItem = value;
        //        this.OnPropertyChanged();
        //    }
        //}


        //private string _SelectedText = null;
        //public string SelectedText
        //{
        //    get { return _SelectedText; }
        //    set
        //    {
        //        _SelectedText = value;
        //        this.OnPropertyChanged();
        //    }
        //}
        #endregion

        #region 表格
        //新增还是编辑，默认是编辑(债券)
        public bool IsAdd = false;

        public string quantID = string.Empty;
        public string UserName = null;
        public string[] info5 = null;
        /// <summary>
        /// 资源锁
        /// </summary>
        private object _lockObj = new object();
        public object LockObj
        {
            get
            {
                return _lockObj;
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

        private object _lockBObj = new object();
        public object LockBObj
        {
            get
            {
                return _lockBObj;
            }
        }

        private object _lockCGObj = new object();
        public object LockCGObj
        {
            get
            {
                return _lockCGObj;
            }
        }

        private DataRowView _selectedParamItem;
        public DataRowView SelectedParamItem
        {
            get
            {
                return _selectedParamItem;
            }

            set
            {
                _selectedParamItem = value;
                this.OnPropertyChanged();
            }
        }

        private DataRowView _selectedGroupItem;
        public DataRowView SelectedGroupItem
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

        private DataRowView _selectedBondItem;
        public DataRowView SelectedBondItem
        {
            get
            {
                return _selectedBondItem;
            }
            set
            {
                _selectedBondItem = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// 参数管理是否能点击新增和删除按钮
        /// </summary>
        private bool _isParamEnabled = false;
        public bool IsParamEnabled
        {
            get
            {
                return _isParamEnabled;
            }
            set
            {
                _isParamEnabled = value;
                this.OnPropertyChanged();
            }
        }
  


        private DataView _ParamView;
        public DataView ParamView
        {
            get
            {
                lock (this.LockObj)
                {
                    if (_ParamView == null)
                    {
                        _ParamView = new DataTable().DefaultView;
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PID_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMID_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMKEY_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.SUBPARAMKEY_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMTYPE_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMVALUE_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMVALUEBACK_FIELD, typeof(string));
                        //_ParamView.Table.Columns.Add(MMListBoxViewModel.ISPARAMVALUE_FIELD, typeof(bool));
                        //_ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMCHECKKEY_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMISCHECK_FIELD, typeof(bool)); 
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDEESC_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDEESCMuti_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDSIDN_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDUNIT_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDCOMMENT_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDCOMBOX_FIELD, typeof(Dictionary<string,string>));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.BUTTONNAME_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.BUTTONGROUPNAME_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.ParamDescColor_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.ParamCheckValue_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMTREEITEMSOURCE_FIELD, typeof(ObservableCollection<TreeNodeModel>));

                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMKEY1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMTYPE1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMVALUE1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMVALUE1BACK_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDEESC1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDSIDN1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDUNIT1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDCOMMENT1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDCOMBOX1_FIELD, typeof(List<string>));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.ParamDescColor1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.ParamCheckValue1_FIELD, typeof(string));

                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMKEY2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMTYPE2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMVALUE2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMVALUE2BACK_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDEESC2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDSIDN2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDUNIT2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDCOMMENT2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMDCOMBOX2_FIELD, typeof(List<string>));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.ParamDescColor2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.ParamCheckValue2_FIELD, typeof(string));

                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMINFO1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.USERNAME_FIELD, typeof(string));

                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMISENABLED_FIELD, typeof(bool));
                        _ParamView.Table.Columns.Add(MMListBoxViewModel.PARAMISALTERENABLED_FIELD, typeof(bool));
                        
                        _ParamView.Table.PrimaryKey = new DataColumn[] {
                        _ParamView.Table.Columns[MMListBoxViewModel.PID_FIELD]
                    };
                        this.OnPropertyChanged();
                    }
                    return this._ParamView;
                }
            }
        }

        private DataView _GroupView;
        public DataView GroupView
        {
            get
            {
                lock (this.LockGObj)
                {
                    if (_GroupView == null)
                    {
                        _GroupView = new DataTable().DefaultView;
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.G_Id, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.G_Groupname, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.G_BondNum, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.G_BondCode, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.G_updatetime, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.CLOSEBY_FIELD, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.USERNAME_FIELD, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.ShowDeleteBtn, typeof(string));//是否有操作权限
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.G_GroupTextVisible, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.G_GroupTextVisibleNo, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.INFO2_FIELD, typeof(string));
                        _GroupView.Table.Columns.Add(MMListBoxViewModel.TYPE_FIELD, typeof(string));
                        _GroupView.Table.PrimaryKey = new DataColumn[] {
                        _GroupView.Table.Columns[MMListBoxViewModel.G_Id]
                    };
                        this.OnPropertyChanged();
                    }
                    return this._GroupView;
                }
            }
        }

        private DataView _BondView;
        public DataView BondView
        {
            get
            {
                lock (this.LockBObj)
                {
                    if (_BondView == null)
                    {
                        _BondView = new DataTable().DefaultView;
                        _BondView.Table.Columns.Add(MMListBoxViewModel.B_Id, typeof(string));
                        _BondView.Table.Columns.Add(MMListBoxViewModel.B_GroupId, typeof(string));
                        _BondView.Table.Columns.Add(MMListBoxViewModel.B_securityid, typeof(string));
                        _BondView.Table.Columns.Add(MMListBoxViewModel.B_symbol, typeof(string));
                        _BondView.Table.Columns.Add(MMListBoxViewModel.B_tenor, typeof(string));
                        _BondView.Table.Columns.Add(MMListBoxViewModel.B_type, typeof(string));

                        _BondView.Table.PrimaryKey = new DataColumn[] {
                        _BondView.Table.Columns[MMListBoxViewModel.B_securityid]
                    };
                        this.OnPropertyChanged();
                    }
                    return this._BondView;
                }
            }
        }


        private DataView _CheckGroupView;
        public DataView CheckGroupView
        {
            get
            {
                lock (this.LockCGObj)
                {
                    if (_CheckGroupView == null)
                    {
                        _CheckGroupView = new DataTable().DefaultView;
                        _CheckGroupView.Table.Columns.Add(MMListBoxViewModel.G_Id, typeof(string));
                        _CheckGroupView.Table.Columns.Add(MMListBoxViewModel.G_IsChecked, typeof(bool));
                        _CheckGroupView.Table.Columns[MMListBoxViewModel.G_IsChecked].DefaultValue = false;
                        _CheckGroupView.Table.Columns.Add(MMListBoxViewModel.G_Groupname, typeof(string));
                        _CheckGroupView.Table.Columns.Add(MMListBoxViewModel.G_BondCode, typeof(string));
                        _CheckGroupView.Table.Columns.Add(MMListBoxViewModel.CLOSEBY_FIELD, typeof(string));
                        _CheckGroupView.Table.Columns.Add(MMListBoxViewModel.USERNAME_FIELD, typeof(string));
                        _CheckGroupView.Table.Columns.Add(MMListBoxViewModel.INFO2_FIELD, typeof(string));
                        //_CheckGroupView.Table.Columns.Add(MMListBoxViewModel.ShowDeleteBtn, typeof(string));//只能勾自己的
                        _CheckGroupView.Table.PrimaryKey = new DataColumn[] {
                        _CheckGroupView.Table.Columns[MMListBoxViewModel.G_Id]
                    };
                        this.OnPropertyChanged();
                    }
                    return this._CheckGroupView;
                }
            }
        }
        #endregion


        /// <summary>
        /// 构造函数
        /// </summary>
        public AlterParamViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(this.ParamView, this.LockObj);
            BindingOperations.EnableCollectionSynchronization(this.GroupView, this.LockGObj);
            BindingOperations.EnableCollectionSynchronization(this.BondView, this.LockBObj);
            BindingOperations.EnableCollectionSynchronization(this.CheckGroupView, this.LockCGObj);

            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
        }

        public void UnsubscribeAppMsg()
        {
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.UnSubscribeAppMsg(OnAppMsg);
        }

        /// <summary>
        /// 应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "refreshClientBondGroup" && obj.Msg == "*")
            {
                clearGroup();
                initGroup();
            }
        }

        public void init()
        {
            //ObservableCollection<TBookBookTreeItemModel> treeList = AdminHandler.Instance.GetTCurrentBookCollection(this.UserName);
           
            List<TTbookQuantParameter> list = QuantGroupHandler.Instance.GetParasByQuantID(quantID);
            Instance_OnParasList(list);

            //设置树数据源
            // MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(treeList));
          

        }


        #region 债券组
        public void initGroup()
        {
            dicGroup.Clear();
            HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
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
                HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery2(request);
                if (replyPo != null)
                {
                    if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
                    {
                        var templist = replyPo.Datas.OrderByDescending(a => a.Value.updatetime).ToList();
                        foreach (KeyValuePair<string, HomeGroupInfo> pair in templist)
                        {
                            HomeGroupInfo homeGroupInfo = pair.Value;
                            if (!dicGroup.ContainsKey(homeGroupInfo.id))
                                dicGroup.Add(homeGroupInfo.id, homeGroupInfo);
                            UpdateGroupList(homeGroupInfo);
                        }
                        if (this.SelectedGroupItem == null)
                            this.SelectedGroupItem = this.GroupView.Count > 0 ? this.GroupView[0] : null;
                    }
                }
            });
        }

        private void UpdateGroupList(HomeGroupInfo obj)
        {
            try
            {
                lock (LockGObj)
                {
                    if (!string.IsNullOrWhiteSpace(obj.inf2))
                    {
                        GroupMsgPo groupMsgPo = JsonConvert.DeserializeObject<GroupMsgPo>(obj.inf2);
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
                        obj.tagDic = tagTempDic;
                        obj.groupMsgPo = groupMsgPo;
                    }
                    if (obj.tagDic == null || !obj.tagDic.ContainsKey(defaultTag)) 
                    {
                        return;
                    }
                    DataRow Item = this.GroupView.Table.Rows.Find(new object[] { obj.id });
                    if (Item == null)
                    {
                        Item = this.GroupView.Table.NewRow();
                        Item[MMListBoxViewModel.G_Id] = obj.id;
                        Item[MMListBoxViewModel.G_Groupname] = obj.groupname;
                        if (obj != null && obj.securityids != null && obj.securityids.Count > 0)
                        {
                            StringBuilder str = new StringBuilder();
                            foreach (HomeGroupSecurityIDsInfo homeSecurtity in obj.securityids)
                            {
                                str.Append(homeSecurtity.securityid);
                                str.Append(",");
                            }
                            str.Remove(str.Length - 1, 1);
                            Item[MMListBoxViewModel.G_BondNum] = obj.securityids.Count;
                            Item[MMListBoxViewModel.G_BondCode] = str;
                        }
                        else
                            Item[MMListBoxViewModel.G_BondNum] = 0;
                        Item[MMListBoxViewModel.CLOSEBY_FIELD] = obj.closeby;
                        Item[MMListBoxViewModel.USERNAME_FIELD] = obj.username;
                        Item[MMListBoxViewModel.G_updatetime] = obj.updatetime;
                        Item[MMListBoxViewModel.G_GroupTextVisible] = "1";
                        Item[MMListBoxViewModel.G_GroupTextVisibleNo] = "0";
                        
                        Item[MMListBoxViewModel.ShowDeleteBtn] = obj.tagDic != null && obj.tagDic.ContainsKey(defaultTag)?QuantGroupHandler.Instance.IsCheckOwn(obj.username):"1";
                        Item[MMListBoxViewModel.INFO2_FIELD] = obj.inf2;
                        Item[MMListBoxViewModel.TYPE_FIELD] = obj.inf1;
                        this.GroupView.Table.Rows.Add(Item);
                    }
                    this.GroupView.Table.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }


        private void AddGroupClick()
        {
            AddGroupWindow addGroupWindow = new AddGroupWindow();
            addGroupWindow.Owner = Application.Current.MainWindow;
            addGroupWindow.ShowDialog();
            if (addGroupWindow.Result == true)
            {
                try
                {
                    string groupname = addGroupWindow.GroupName;
                    if (string.IsNullOrEmpty(groupname) == false)
                    {
                        HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                        request.groupname = groupname;//组名
                        request.action = "1";//新增
                        request.terminal = LocalEnv.ServerInfo.Version; // 终端
                        request.username = LocalEnv.UserInfo.UserName;
                        request.closeby = LocalEnv.UserInfo.UserName;
                        GroupMsgPo groupMsgPo = new GroupMsgPo();
                        if (CommonConstants.QUANT_Type.Equals(defaultTag))
                        {
                            groupMsgPo.Tag = CommonConstants.QUANT_Type_Msg;
                            request.type = MMListBoxViewModel.QUANT_Type;
                        }
                        else 
                        {
                            groupMsgPo.Tag = CommonConstants.INNER_Type_Msg;
                            request.type = CommonConstants.INNER_Type;
                        }
                        groupMsgPo.IsCareGroup = "false";
                        groupMsgPo.IsAllInstGroup = "false";
                        groupMsgPo.IsDynamicsGroup = "false";
                        request.inf2 = JsonConvert.SerializeObject(groupMsgPo);
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            PResult pResult = AdminHandler.Instance.homeGroupUpdate2(request);
                            if (pResult != null && pResult.Code == "0000")
                            {
                              
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    clearGroup();
                                    initGroup();
                                    DesignHandler.Instance.NotifyUpdateGroup(null);
                                    BatachAddBondTab1 batachAddBondTab1 = new BatachAddBondTab1();
                                    batachAddBondTab1.Owner = Application.Current.MainWindow;
                                    batachAddBondTab1.ShowDialog();
                                }));
                            }
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "我的债券库", Info2 = "refreshWebBondGroup", Info3 = "AddGroup" });
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex);
                }
            }
        }

        private void DelGroupClick()
        {
            string text = "是否确定删除该债券组吗？";
            string title = "删除债券组";
            if (CustomMessageBox.Question(text, title) == true)
            {
                DataRowView drv = SelectedGroupItem;
                if (drv == null)
                    return;
                HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                request.id = drv[MMListBoxViewModel.G_Id] + "";
                request.action = "3";//删除
                request.type = drv[MMListBoxViewModel.TYPE_FIELD] + "";
                request.username = drv[MMListBoxViewModel.USERNAME_FIELD] + "";
                ThreadPool.QueueUserWorkItem(o =>
                {
                    PResult pResult = AdminHandler.Instance.homeGroupUpdate2(request);
                    if (pResult != null && pResult.Code == "0000")
                    {
                        dicGroup.Remove(request.id);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            drv.BeginEdit();
                            drv.Delete();
                            drv.EndEdit();
                            this.GroupView.Table.AcceptChanges();
                            DesignHandler.Instance.NotifyUpdateGroup(null);
                            CustomMessageBox.Info(string.Format($"删除成功."), "删除债券组");
                        }));
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "我的债券库", Info2 = "refreshWebBondGroup", Info3 = "DelGroup" });
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            CustomMessageBox.Info(string.Format($"撤销订单失败, 原因:{pResult.Msg}."), "删除债券组");
                        }));
                    }
                });
            }
        }

        public string getGName()
        {
            string name = "";
            DataRowView drv = SelectedGroupItem;
            if (drv != null)
            {
                name = drv[MMListBoxViewModel.G_Groupname] + "";
            }
            return name;
        }

        public void EditGroupNameClick()
        {
            DataRowView drv = SelectedGroupItem;
            drv[MMListBoxViewModel.G_GroupTextVisible] = "0";
            drv[MMListBoxViewModel.G_GroupTextVisibleNo] = "1";
        }

        public void EditGroupNameOKClick()
        {
            if (SelectedGroupItem == null)
                return;
            DataRowView drv = SelectedGroupItem;
            string groupId = drv[MMListBoxViewModel.G_Id] + "";
            string group = drv[MMListBoxViewModel.G_Groupname] + "";
            HomeGroupInfo oldGroup = dicGroup.ContainsKey(groupId) ? dicGroup[groupId] : null;
            if (oldGroup.groupname.Equals(group))
            {
                //drv.BeginEdit();
                drv[MMListBoxViewModel.G_GroupTextVisible] = "1";
                drv[MMListBoxViewModel.G_GroupTextVisibleNo] = "0";
                //drv.EndEdit();
                //this.GroupView.Table.AcceptChanges();
                return;
            }
            //HomeGroupInfo homeGroupInfo = dicGroup.ContainsKey(groupId) ? dicGroup[groupId] : null;
            //if (homeGroupInfo == null)
            //{
            //    LogHandler.Warn("homeGroupInfo is null" + groupId + group);
            //    return;
            //}
            UpdateGroups();
        }

        public void UpdateGroups()
        {
            if (SelectedGroupItem == null)
                return;
            DataRowView drv = SelectedGroupItem;
            HomeGroupUpdateReq request = new HomeGroupUpdateReq();
            request.id = drv[MMListBoxViewModel.G_Id] + "";
            request.action = "2";//修改
            request.type = drv[MMListBoxViewModel.TYPE_FIELD] + "";
            request.groupname = drv[MMListBoxViewModel.G_Groupname] + "";
            request.ordersn = "0";
            request.username = drv[MMListBoxViewModel.USERNAME_FIELD] + "";
            request.closeby = LocalEnv.UserInfo.UserName;
           
            request.inf2 = drv[MMListBoxViewModel.INFO2_FIELD] + "";
            ThreadPool.QueueUserWorkItem(o =>
            {
                PResult pResult = AdminHandler.Instance.homeGroupUpdate2(request);
                if (pResult != null && pResult.Code == "0000")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        drv[MMListBoxViewModel.G_GroupTextVisible] = "1";
                        drv[MMListBoxViewModel.G_GroupTextVisibleNo] = "0";
                    }));
                    if (dicGroup.ContainsKey(request.id))
                        dicGroup[request.id].groupname = request.groupname;
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "我的债券库", Info2 = "refreshWebBondGroup", Info3 = "UpdateGroup" });
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        CustomMessageBox.Info(string.Format($"修改组名失败, 原因:{pResult.Msg}."), "修改债券组");
                    }));
                }
            });
        }

        private void EditGroupBondClick()
        {
            DataRowView drv = SelectedGroupItem;
            if (drv == null)
                return;
            string groupId = drv[MMListBoxViewModel.G_Id] + "";
            string group = drv[MMListBoxViewModel.G_Groupname] + "";
            HomeGroupInfo homeGroupInfo = dicGroup.ContainsKey(groupId) ? dicGroup[groupId] : null;
            if (homeGroupInfo == null)
            {
                LogHandler.Warn("homeGroupInfo is null" + groupId + group);
                return;
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                initBond(homeGroupInfo.securityids);
            });
            BatachAddBondTab1 batachAddBondTab1 = new BatachAddBondTab1();
            batachAddBondTab1.Owner = Application.Current.MainWindow;
            batachAddBondTab1.ShowDialog();
        }

        public string[] AlterGroupBondClick()
        {
            string[] args = new string[2];
            string groupid = "";
            string groupName = "";
            DataRowView drv = SelectedGroupItem;
            StringBuilder str = new StringBuilder();
            StringBuilder strNmae = new StringBuilder();
            foreach (DataRow row in CheckGroupView.Table.Rows)
            {
                bool isCheck = (bool)row[MMListBoxViewModel.G_IsChecked];
                if (isCheck)
                {
                    string temp = row[MMListBoxViewModel.G_Id] + "";
                    str.Append(temp);
                    str.Append(",");
                    string tempN = row[MMListBoxViewModel.G_Groupname] + "";
                    strNmae.Append(tempN);
                    strNmae.Append(",");
                }
            }
            int length = str.ToString().Length;
            if (length > 0)
            {
                groupid = str.ToString().Substring(0, length - 1);
                groupName = strNmae.ToString().Substring(0, strNmae.ToString().Length - 1);
            }
            args[0] = groupid;
            args[1] = groupName;
            return args;
        }

        public void clearGroup()
        {
            try
            {
                lock (LockGObj)
                {
                    this.GroupView.Table.Rows.Clear();
                    this.GroupView.Table.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        public void clearCheckGroup()
        {
            try
            {
                
                lock (LockCGObj)
                {
                    this.CheckGroupView.Table.Rows.Clear();
                    this.CheckGroupView.Table.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        #endregion


        #region 组下债券
        List<HomeGroupSecurityidsUpdateReq> RequestList = new List<HomeGroupSecurityidsUpdateReq>();
        public void initBond(List<HomeGroupSecurityIDsInfo> list)
        {
            DataRowView drv = SelectedGroupItem;
            if (list != null && list.Count > 0)
            {
                foreach (HomeGroupSecurityIDsInfo obj in list)
                {
                    lock (LockBObj)
                    {
                        DataRow Item = this.BondView.Table.Rows.Find(new object[] { obj.securityid });
                        if (Item == null)
                        {
                            Item = this.BondView.Table.NewRow();
                            Item[MMListBoxViewModel.B_Id] = obj.id;
                            Item[MMListBoxViewModel.B_GroupId] = obj.groupId;
                            Item[MMListBoxViewModel.B_securityid] = obj.securityid;
                            Item[MMListBoxViewModel.B_symbol] = obj.symbol;
                            string[] str = obj.remark.Replace("  ", ",").Split(',');
                            if (str != null && str.Count() == 3)
                                Item[MMListBoxViewModel.B_tenor] = str[2];
                            Item[MMListBoxViewModel.B_type] = obj.inf1;
                            this.BondView.Table.Rows.Add(Item);
                        }
                        this.BondView.Table.AcceptChanges();
                    }
                }
            }
        }
        private int pid = 0;
        private void AddBondClick()
        {
            AddBondWindow addBondWind = new AddBondWindow();
            addBondWind.Owner = Application.Current.MainWindow;
            addBondWind.ShowDialog();
            bool result = addBondWind.Result;
            if (result == true)
            {
                if (this.BondView.Table.Rows.Find(addBondWind.securityID) != null)
                {
                    return;
                }

                DataRowView drv = SelectedGroupItem;
                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                request.securityid = addBondWind.securityID;
                request.symbol = addBondWind.symbol;
                request.action = "1"; // 添加
                request.remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}";
                request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                request.type = SelectedGroupItem[MMListBoxViewModel.TYPE_FIELD] +"";
                request.groupid = drv == null ? "" : drv[MMListBoxViewModel.G_Id] + "";
                request.username = SelectedGroupItem[MMListBoxViewModel.USERNAME_FIELD] + "";
                RequestList.Add(request);
                DataRow Item = this.BondView.Table.NewRow();
                Item[MMListBoxViewModel.B_Id] = request.securityid;//pid++;
                Item[MMListBoxViewModel.B_GroupId] = request.groupid;
                Item[MMListBoxViewModel.B_securityid] = request.securityid;
                Item[MMListBoxViewModel.B_symbol] = request.symbol;
                string[] str = request.remark.Replace("  ", ",").Split(',');
                if (str != null && str.Count() == 3)
                    Item[MMListBoxViewModel.B_tenor] = str[2];
                Item[MMListBoxViewModel.B_type] = request.type;
                this.BondView.Table.Rows.Add(Item);
            }
            this.BondView.Table.AcceptChanges();
            //AddAndUpdateBond(request);
        }


        //public void AddAndUpdateBond(HomeGroupSecurityidsUpdateReq req)
        //{
        //ThreadPool.QueueUserWorkItem(o =>
        //{
        //    PResult result = AdminHandler.Instance.homeSecurityidsUpdate(req);
        //    if (result != null && result.Code == "0000")
        //    {
        //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        {
        //            CustomMessageBox.Info($"{req.remark}\r\n 债券新增成功！", "新增债券");
        //        }));
        //        clearGroup();
        //        clearBond();
        //        HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
        //        request.type = MMListBoxViewModel.QUANT_Type;
        //        HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery(request);
        //        if (replyPo != null)
        //        {
        //            if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
        //            {
        //                foreach (KeyValuePair<string, HomeGroupInfo> pair in replyPo.Datas)
        //                {
        //                    HomeGroupInfo homeGroup = pair.Value;
        //                    if (!dicGroup.ContainsKey(homeGroup.id))
        //                        dicGroup.Add(homeGroup.id, homeGroup);
        //                }
        //            }
        //            string groupId = req.groupid;
        //            HomeGroupInfo homeGroupInfo = dicGroup.ContainsKey(groupId) ? dicGroup[groupId] : null;
        //            if (homeGroupInfo == null)
        //            {
        //                LogHandler.Warn("homeGroupInfo is null" + groupId);
        //                return;
        //            }
        //            initBond(homeGroupInfo.securityids);
        //        }
        //    }
        //    else
        //    {
        //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        {
        //            CustomMessageBox.Warning(result.Msg, "新增债券");
        //        }));
        //    }
        //});
        //}


        private void DelBondClick()
        {
            string text = "是否确定删除该债券？";
            string title = "删除债券";
            if (CustomMessageBox.Question(text, title) == true)
            {
                DataRowView drvGP = SelectedGroupItem;
                DataRowView drv = SelectedBondItem;
                HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
                req.groupid = drv[MMListBoxViewModel.B_GroupId] + "";
                req.id = drv[MMListBoxViewModel.B_Id] + "";
                req.action = "3"; //删除;
                req.type = drv[MMListBoxViewModel.B_type] + "";
                req.username = drvGP[MMListBoxViewModel.USERNAME_FIELD] + "";

                RequestList.Add(req);
                drv.BeginEdit();
                drv.Delete();
                drv.EndEdit();
                this.BondView.Table.AcceptChanges();

                //ThreadPool.QueueUserWorkItem(o =>
                //{
                //    PResult result = AdminHandler.Instance.homeSecurityidsUpdate(req);
                //    if (result != null && result.Code == "0000")
                //    {
                //        HomeGroupInfo home = dicGroup[req.groupid];
                //        var securityidList = from m in home.securityids
                //                             where (m.id == req.id)
                //                             select m;
                //        foreach (HomeGroupSecurityIDsInfo security in securityidList)
                //        {
                //            home.securityids.Remove(security);
                //        }
                //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //        {
                //            drv.BeginEdit();
                //            drv.Delete();
                //            drv.EndEdit();
                //            this.BondView.Table.AcceptChanges();
                //            CustomMessageBox.Info(string.Format($"删除成功."), "删除债券");
                //        }));
                //    }
                //    else
                //    {
                //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //        {
                //            CustomMessageBox.Info(string.Format($"撤销订单失败, 原因:{result.Msg}."), "删除债券");
                //        }));
                //    }
                //});
            }
        }


        public void OkBondClick()
        {

            if (RequestList != null && RequestList.Count > 0)
            {
                string ResultStr = "";
                foreach (HomeGroupSecurityidsUpdateReq req in RequestList)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        PResult result = AdminHandler.Instance.homeSecurityidsUpdate2(req);
                        if (result != null && result.Code == "0000")
                        {
                            // ResultStr += req.remark + "\r\n";
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "我的债券库", Info2 = "refreshWebBondGroup", Info3 = "UpdateSecuritys" });
                        }
                        else
                        {
                            ResultStr += result.Msg + "\r\n";
                        }
                    });

                }
                if (!string.IsNullOrEmpty(ResultStr))
                    CustomMessageBox.Warning(ResultStr, "操作债券");

                UpdateGroups();//修改组下面的券同时修改组更新时间
            }
            batachAddBondTab1.Close();
            clearGroup();
            initGroup();
            DesignHandler.Instance.NotifyUpdateGroup(null);
            //RequestList.Clear();
        }

        public void CancelBondClick()
        {
            batachAddBondTab1.Close();
        }

        public void clearBond()
        {
            try
            {
                if (RequestList != null)
                    RequestList.Clear();
                lock (LockBObj)
                {
                    this.BondView.Table.Rows.Clear();
                    this.BondView.Table.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void BatchImportBondClick()
        {
            BatchAddBondsWindow batch = new BatchAddBondsWindow(true, "");
            batch.Show();
        }


        //public void Instance_OnGroupList(IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroups)
        //{
        //    foreach (KeyValuePair<string, HomeGroupInfo> pair in sortedGroups)
        //    {
        //        HomeGroupInfo homeGroupInfo = pair.Value;
        //        UpdateGroupList(homeGroupInfo);
        //    }
        //}


        //private void UpdateGroupList(HomeGroupInfo obj)
        //{
        //    lock (obj)
        //    {
        //        DataRow Item = this.GroupView.Table.Rows.Find(new object[] { obj.id });
        //        if (Item == null)
        //        {
        //            Item = this.GroupView.Table.NewRow();
        //            Item[MMListBoxViewModel.G_Id] = obj.id;
        //            Item[MMListBoxViewModel.G_Groupname] = obj.groupname;
        //            if (obj != null && obj.securityids != null && obj.securityids.Count > 0)
        //            {
        //                StringBuilder str = new StringBuilder();
        //                foreach (HomeGroupSecurityIDsInfo homeSecurtity in obj.securityids)
        //                {
        //                    str.Append(homeSecurtity.securityid);
        //                    str.Append(",");
        //                }
        //                str.Remove(str.Length - 1, 1);
        //                Item[MMListBoxViewModel.G_BondCode] = str;
        //            }

        //            Item[MMListBoxViewModel.G_updatetime] = obj.updatetime;
        //            this.GroupView.Table.Rows.Add(Item);
        //        }
        //        this.GroupView.Table.AcceptChanges();
        //    }
        //}

        #endregion

        #region 编辑参数选择债券组
        public void initCheckGroup(string GroupId,string GroupName,string UserName)
        {
            clearCheckGroup();
            //if (dicGroup != null && dicGroup.Count > 0)
            //{
            //    foreach (KeyValuePair<string, HomeGroupInfo> pair in dicGroup)
            //    {
            //        HomeGroupInfo homeGroupInfo = pair.Value;
            //        if (!dicGroup.ContainsKey(homeGroupInfo.id))
            //            dicGroup.Add(homeGroupInfo.id, homeGroupInfo);
            //        if (UserName.Equals(homeGroupInfo.username))
            //            UpdateCheckGroupList(homeGroupInfo, GroupId, GroupName);
            //    }
            //}
            //else
            //{
            ThreadPool.QueueUserWorkItem(o =>
            {
                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                if ("*".Equals(AdminHandler.Instance.getSecurityGroupUser()))
                {
                    request.username = "";
                }
                else
                {
                    request.username = LocalEnv.UserInfo.UserName;
                }
                //request.type = CommonConstants.QUANT_Type;
               HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery2(request);
                    if (replyPo != null)
                    {
                        if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
                        {
                            foreach (KeyValuePair<string, HomeGroupInfo> pair in replyPo.Datas)
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
                                if ((UserName.Equals(homeGroupInfo.username)||"*".Equals(QuantGroupHandler.Instance.user)) && (homeGroupInfo.tagDic != null) &&homeGroupInfo.tagDic.ContainsKey(CommonConstants.QUANT_Type))
                                    UpdateCheckGroupList(homeGroupInfo, GroupId, GroupName);
                            }
                        }
                    }
                });
            //}
        }

        private void UpdateCheckGroupList(HomeGroupInfo obj,string GroupId,string GroupName)
        {
            try
            {
                lock (LockCGObj)
                {
                    DataRow Item = this.CheckGroupView.Table.Rows.Find(new object[] { obj.id });
                    if (Item == null)
                    {
                        Item = this.CheckGroupView.Table.NewRow();
                        Item[MMListBoxViewModel.G_Id] = obj.id;
                        Item[MMListBoxViewModel.G_Groupname] = obj.groupname;
                        if (GroupId.Contains(obj.id))
                            Item[MMListBoxViewModel.G_IsChecked] = true;
                        else
                            Item[MMListBoxViewModel.G_IsChecked] = false;
                        StringBuilder str = new StringBuilder();
                        foreach (HomeGroupSecurityIDsInfo homeSecurtity in obj.securityids)
                        {
                            str.Append(homeSecurtity.securityid);
                            str.Append(",");
                        }
                        if (str.Length > 0)
                            str.Remove(str.Length - 1, 1);
                        Item[MMListBoxViewModel.G_BondCode] = str;
                        Item[MMListBoxViewModel.CLOSEBY_FIELD] = obj.closeby;
                        Item[MMListBoxViewModel.USERNAME_FIELD] = obj.username;
                        Item[MMListBoxViewModel.INFO2_FIELD] = obj.inf2;
                        //Item[MMListBoxViewModel.ShowDeleteBtn] = ApiManager.Instance.GetDefaultApi().UserName.Equals(obj.username) ? "True" : "False";
                        this.CheckGroupView.Table.Rows.Add(Item);
                    }
                    this.CheckGroupView.Table.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }



        #endregion



        #region 导入
        public void ImportBondClick()
        {
            ImportExcel();
        }
        private async void ImportExcel()
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
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }
                    // 2.导入操作
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
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导入";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                    //CustomMessageBox.Info(promptTxt, "批量导入");
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
                    StringBuilder str = new StringBuilder();
                    foreach (KeyValuePair<int, string> pair in dic)
                    {
                        DataTable importDT = helper.ExcelToDataTable(pair.Key);
                        if (importDT != null && importDT.Rows.Count > 0)
                        {
                            foreach (DataRow dr in importDT.Rows)
                            {
                                if (this.BondView.Table.Rows.Find(CommonUtils.ConvertToString(dr[MMListBoxViewModel.SecurityID_ExcelField])) != null)
                                {
                                    continue;
                                }

                                DataRowView drv = SelectedGroupItem;
                                HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
                                req.action = "1"; // 添加
                                req.groupid = drv == null ? "" : drv[MMListBoxViewModel.G_Id] + "";
                                req.type = SelectedGroupItem[MMListBoxViewModel.TYPE_FIELD] + "";
                                req.securityid = CommonUtils.ConvertToString(dr[MMListBoxViewModel.SecurityID_ExcelField]);
                                req.symbol = CommonUtils.ConvertToString(dr[MMListBoxViewModel.Symbol_ExcelField]);
                                req.remark = $"{req.securityid}  {req.symbol}  {CommonUtils.ConvertToString(dr[MMListBoxViewModel.Tenor_ExcelField])}";
                                req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);

                                req.username = SelectedGroupItem[MMListBoxViewModel.USERNAME_FIELD] + "";
                                DataRow Item = this.BondView.Table.NewRow();
                                Item[MMListBoxViewModel.B_Id] = pid++;
                                Item[MMListBoxViewModel.B_GroupId] = req.groupid;
                                Item[MMListBoxViewModel.B_securityid] = req.securityid;
                                Item[MMListBoxViewModel.B_symbol] = req.symbol;
                                Item[MMListBoxViewModel.B_tenor] = CommonUtils.ConvertToString(dr[MMListBoxViewModel.Tenor_ExcelField]);
                                Item[MMListBoxViewModel.B_type] = req.type;
                                this.BondView.Table.Rows.Add(Item);
                                this.BondView.Table.AcceptChanges();
                                RequestList.Add(req);
                            }
                        }
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
                return null;
            });
        }
        #endregion


        #region 导出
        private void ExportBondClick()
        {
            ExportExcel();
        }

        private async void ExportExcel()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"策略债券导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
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
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导出";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //string text = this.importTxt.Text;
                    //});
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
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导出";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                    //CustomMessageBox.Info(promptTxt, "批量导入");
                }
            }
            catch (Exception ex)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导出";
                prompt.PromptContext = ex.Message;
                prompt.Show();
                LogHelper.Error(ex);
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
                    if (BondView != null && BondView.Table.Rows.Count > 0)
                    {
                        Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                        DataTable dt = this.BondView.Table;
                        dataDic[MMListBoxViewModel.B_GroupId] = GenerateExcelDataTable(dt);
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
        private DataTable GenerateExcelDataTable(DataTable data)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(MMListBoxViewModel.SecurityID_ExcelField);
            dt.Columns.Add(MMListBoxViewModel.Symbol_ExcelField);
            dt.Columns.Add(MMListBoxViewModel.Tenor_ExcelField);
            foreach (DataRow row in data.Rows)
            {
                DataRow newDR = dt.NewRow();
                newDR[MMListBoxViewModel.SecurityID_ExcelField] = row[MMListBoxViewModel.B_securityid] + "";
                newDR[MMListBoxViewModel.Symbol_ExcelField] = row[MMListBoxViewModel.B_symbol] + "";
                newDR[MMListBoxViewModel.Tenor_ExcelField] = row[MMListBoxViewModel.B_tenor] + "";
                dt.Rows.Add(newDR);
            }
            return dt;
        }


        #endregion


        public void Instance_OnParasList(List<TTbookQuantParameter> list)
        {
            try
            {
                if (this.ParamView != null)
                {
                    this.ParamView.Table.Rows.Clear();
                }
            }
            catch (Exception e)
            {
                LogHandler.Error("Instance_OnParasList:" + e);
            }
            if (list == null)
                return;
            int i = 0;
            foreach (TTbookQuantParameter param in list)
            {
                i++;
                UpdateParasAsync(param);
                if (info5 != null)//显示分割线
                {
                    foreach (string s in info5)
                    {
                        if (!string.IsNullOrEmpty(s) && Convert.ToInt32(s) == i)
                            UpdateParasAsync(null);
                    }
                }
            }
        }

        private void UpdateParasAsync(TTbookQuantParameter mm)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
                UpdateParasList(mm);
            //}));
        }
        private void UpdateParasList(TTbookQuantParameter obj)
        {
            try
            {
                lock (this.LockObj)
                {
                    if (obj != null)
                    {
                        if (string.IsNullOrWhiteSpace(obj.Param_Type))
                            obj.Param_Type = obj.Para_Type;

                        if (("Radio").Equals(obj.Param_Type) || ("MultiSelect").Equals(obj.Param_Type) || ("MultiString").Equals(obj.Param_Type))
                        {
                            int col = 0;
                            string[] str = obj.Param_Value.Contains(",") ? obj.Param_Value.Split(',') : null;
                            foreach (ParaOptionalValues v in obj.Para_Optional_Values)
                            {
                                col++;
                                if (("MultiSelect").Equals(obj.Param_Type) || ("MultiString").Equals(obj.Param_Type))
                                {
                                    v.Param_Value = (str != null && str.Count() > (col - 1)) ? str[col - 1] : "";
                                }
                                DataRow bondInfosItems = UpdateParas(obj, obj.Id + "-" + v.Param_Key, v);
                                if (bondInfosItems == null)
                                    return;
                                bondInfosItems[MMListBoxViewModel.SUBPARAMKEY_FIELD] = v.Param_Key;
                                if (obj.Param_Value.Equals(v.Param_Key) || (str != null && str.Contains(v.Param_Key)))
                                    bondInfosItems[MMListBoxViewModel.PARAMISCHECK_FIELD] = true;
                                else
                                    bondInfosItems[MMListBoxViewModel.PARAMISCHECK_FIELD] = false;
                                if (col != 1)
                                {
                                    bondInfosItems[MMListBoxViewModel.PARAMKEY_FIELD] = "";
                                    bondInfosItems[MMListBoxViewModel.PARAMDEESC_FIELD] = "";
                                }
                                if (obj.Is_Independent_Para)
                                {
                                    if (obj.Sub_Param.Count > col - 1)
                                    {
                                        SubParam sub = obj.Sub_Param[col - 1];
                                        for (int subcol = 0; subcol < sub.Param.Count; subcol++)
                                        {
                                            if (subcol == 0)
                                                UpdateSubParas1(bondInfosItems, sub.Param[0]);
                                            if (subcol == 1)
                                                UpdateSubParas2(bondInfosItems, sub.Param[1]);
                                            //if (subcol == 2)
                                            //    UpdateSubParas3(bondInfosItems, sub.Param[2]);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            DataRow bondInfosItems = UpdateParas(obj, obj.Id, null);
                            if (obj.Is_Independent_Para)
                            {
                                foreach (SubParam sub in obj.Sub_Param)
                                {
                                    foreach (QuantParameter pa in sub.Param)
                                    {
                                        // UpdateSubParas(bondInfosItems, pa);
                                    }
                                }
                            }
                            else
                            {
                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                foreach (ParaOptionalValues pv in obj.Para_Optional_Values)
                                {
                                    if (!dic.ContainsKey(pv.Param_Key))
                                        dic.Add(pv.Param_Key, pv.Param_Desc);
                                }
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMBOX_FIELD] = dic;
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE1_FIELD] = "";
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(obj.Inf5) && obj.Username.Equals(LocalEnv.UserInfo.UserName))
                        {
                            CustomMessageBox.Warning($"{obj.Inf5}");
                            obj.Inf5 = null;
                        }
                    }
                    else
                    {
                        DataRow bondInfosItems = this.ParamView.Table.NewRow();
                        bondInfosItems[MMListBoxViewModel.PID_FIELD] = DataTimeSequence.Instance.GetId();
                        bondInfosItems[MMListBoxViewModel.PARAMID_FIELD] = -1;
                        this.ParamView.Table.Rows.Add(bondInfosItems);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error("UpdateParasList:" + e + obj);
            }
        }

        #region 更新主参数
        private DataRow UpdateParas(TTbookQuantParameter obj, string Id, ParaOptionalValues v)
        {
            lock (this.LockObj)
            {
                DataRow bondInfosItems = this.ParamView.Table.Rows.Find(new object[] { Id });
                TTbookQuantStrategys quant = QuantGroupHandler.Instance.GetQuantByQuantID(obj.Quantid);
                if (quant == null)
                {
                    LogHandler.Warn("Quantid not in Cache:" + obj.Quantid);
                }
                //if (obj.Param_Type != null && obj.Param_Type.Equals("ComboBox"))
                //{
                //    obj.Param_Type = "TreeViewComBoxItem";
                //}
                if (bondInfosItems != null)
                {
                    if ("1" == obj.Action || "2" == obj.Action)
                    {
                        bondInfosItems[MMListBoxViewModel.PID_FIELD] = Id;
                        bondInfosItems[MMListBoxViewModel.PARAMID_FIELD] = obj.Id;
                        bondInfosItems[MMListBoxViewModel.PARAMKEY_FIELD] = obj.Param_Key;
                        bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = obj.Param_Type;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUE_FIELD] = obj.Param_Value;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUEBACK_FIELD] = obj.Param_Value;
                        //if (obj.Param_Value.Equals("True"))
                        //    bondInfosItems[MMListBoxViewModel.ISPARAMVALUE_FIELD] = true;
                        //else 
                        //    bondInfosItems[MMListBoxViewModel.ISPARAMVALUE_FIELD] = false;

                        switch (obj.Param_Type)
                        {
                            case "Radio":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.CheckBoxItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                                break;
                            case "MultiSelect":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.MultiSelectItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                                break;
                            case "Text":
                                //bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextFieldItem + "";
                                if (string.IsNullOrEmpty(obj.Param_Value))
                                    bondInfosItems[MMListBoxViewModel.BUTTONNAME_FIELD] = "添加";
                                else
                                    bondInfosItems[MMListBoxViewModel.BUTTONNAME_FIELD] = "编辑";
                                bondInfosItems[MMListBoxViewModel.BUTTONGROUPNAME_FIELD] = QuantCommonUtils.setGroupName(obj.Param_Value, dicGroup);
                                break;
                            case "ComboBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.ComboBoxItem + "";
                                break;
                            case "DatePicker":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.DatePickerItem + "";
                                break;
                            case "SearchTextBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                                break;
                            case "MultiString":
                                bondInfosItems[MMListBoxViewModel.PARAMDEESCMuti_FIELD] = v != null ? v.Param_Desc : "";
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItemMulti + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = obj != null ? obj.Para_Unit : "";
                                bondInfosItems[MMListBoxViewModel.PARAMVALUE_FIELD] = v != null ? v.Param_Value : "";
                                break;
                            case "TreeView":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TreeViewComBoxItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMTREEITEMSOURCE_FIELD] = STC.Common.Utils.CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(STC.Common.Utils.CommonUtils.SerializeObject(STC.Common.Handler.Admin.AdminHandler.Instance.GetTCurrentBookCollection(this.UserName)));
                                //接续手工单默认组合
                                //this.SelectedItem = obj.Param_Value ?? null;
                                break;
                            default:
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItem + "";
                                break;
                        }

                        //bondInfosItems[PARAMDEESC_FIELD] = obj.Param_Desc;
                        //bondInfosItems[PARAMDSIDN_FIELD] = obj.B_Bext_Box_String;
                        //bondInfosItems[PARAMDUNIT_FIELD] = obj.Para_Unit;
                        bondInfosItems[MMListBoxViewModel.PARAMISENABLED_FIELD] = IsParamEnabled;
                        bondInfosItems[MMListBoxViewModel.PARAMISALTERENABLED_FIELD] = IsParamEnabled;
                        if (quant != null && "0".Equals(quant.Enable) && !string.IsNullOrEmpty(quant.Inf1))
                        {
                            bondInfosItems[MMListBoxViewModel.PARAMISALTERENABLED_FIELD] = !IsParamEnabled;
                        }
                        bondInfosItems[MMListBoxViewModel.PARAMINFO1_FIELD] = obj.Inf1;
                        bondInfosItems[MMListBoxViewModel.ParamDescColor_FIELD] = obj.Param_Desc_Color;
                        bondInfosItems[MMListBoxViewModel.ParamCheckValue_FIELD] = obj.Param_Check_Value;
                        bondInfosItems[MMListBoxViewModel.USERNAME_FIELD] = obj.Username;
                    }
                }
                else
                {
                    if ("1" == obj.Action || "2" == obj.Action)
                    {
                        bondInfosItems = this.ParamView.Table.NewRow();
                        bondInfosItems[MMListBoxViewModel.PID_FIELD] = Id;
                        bondInfosItems[MMListBoxViewModel.PARAMID_FIELD] = obj.Id;
                        bondInfosItems[MMListBoxViewModel.PARAMKEY_FIELD] = obj.Param_Key;
                        bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = obj.Param_Type;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUE_FIELD] = obj.Param_Value;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUEBACK_FIELD] = obj.Param_Value;
                        bondInfosItems[MMListBoxViewModel.PARAMDEESC_FIELD] = obj.Param_Desc;
                        bondInfosItems[MMListBoxViewModel.PARAMDSIDN_FIELD] = obj.B_Text_Box_String;
                        bondInfosItems[MMListBoxViewModel.PARAMDUNIT_FIELD] = obj.Para_Unit;
                        bondInfosItems[MMListBoxViewModel.PARAMISENABLED_FIELD] = IsParamEnabled;
                        bondInfosItems[MMListBoxViewModel.PARAMISALTERENABLED_FIELD] = IsParamEnabled;
                        //if (obj.Param_Value.Equals("True"))
                        //    bondInfosItems[MMListBoxViewModel.ISPARAMVALUE_FIELD] = true;
                        //else 
                        //    bondInfosItems[MMListBoxViewModel.ISPARAMVALUE_FIELD] = false;
                        if (quant != null && "0".Equals(quant.Enable) && !string.IsNullOrEmpty(quant.Inf1))
                        {
                            bondInfosItems[MMListBoxViewModel.PARAMISALTERENABLED_FIELD] = !IsParamEnabled;
                        }
                        switch (obj.Param_Type)
                        {
                            case "Radio":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.CheckBoxItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                                break;
                            case "MultiSelect":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.MultiSelectItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                                // bondInfosItems[MMListBoxViewModel.ParamISCheck] = v != null ? v.Param_Value : "";
                                break;
                            case "Text":
                                //bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextFieldItem + "";
                                if (string.IsNullOrEmpty(obj.Param_Value))
                                    bondInfosItems[MMListBoxViewModel.BUTTONNAME_FIELD] = "添加";
                                else
                                    bondInfosItems[MMListBoxViewModel.BUTTONNAME_FIELD] = "编辑";
                                bondInfosItems[MMListBoxViewModel.BUTTONGROUPNAME_FIELD] = QuantCommonUtils.setGroupName(obj.Param_Value, dicGroup);
                                break;
                            case "ComboBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.ComboBoxItem + "";
                                break;
                            case "DatePicker":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.DatePickerItem + "";
                                break;
                            case "SearchTextBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                                break;
                            case "MultiString":
                                bondInfosItems[MMListBoxViewModel.PARAMDEESCMuti_FIELD] = v != null ? v.Param_Desc : "";
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItemMulti + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = obj != null ? obj.Para_Unit : "";
                                bondInfosItems[MMListBoxViewModel.PARAMVALUE_FIELD] = v != null ? v.Param_Value : "";
                                break;
                            case "TreeView":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TreeViewComBoxItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMTREEITEMSOURCE_FIELD] = STC.Common.Utils.CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(STC.Common.Utils.CommonUtils.SerializeObject(STC.Common.Handler.Admin.AdminHandler.Instance.GetTCurrentBookCollection(this.UserName)));
                                //接续手工单默认组合
                                //this.SelectedItem = obj.Param_Value ?? null;
                                break;
                            default:
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItem + "";
                                break;
                        }
                        bondInfosItems[MMListBoxViewModel.PARAMINFO1_FIELD] = obj.Inf1;
                        bondInfosItems[MMListBoxViewModel.USERNAME_FIELD] = obj.Username;
                        bondInfosItems[MMListBoxViewModel.ParamDescColor_FIELD] = obj.Param_Desc_Color;
                        bondInfosItems[MMListBoxViewModel.ParamCheckValue_FIELD] = obj.Param_Check_Value;
                        // this.ParamView.Table.Rows.InsertAt(bondInfosItems, 0);
                        this.ParamView.Table.Rows.Add(bondInfosItems);
                        //if (this.SelectedParamItem == null)
                        //{
                        //    this.SelectedParamItem = this.ParamView[0];
                        //}
                    }
                }
                return bondInfosItems;
            }
        }
        #endregion

        #region 更新子参数
        private void UpdateSubParas1(DataRow bondInfosItems, QuantParameter obj)
        {
            lock (this.LockObj)
            {
                try
                {
                    if ("1" == obj.Action || "2" == obj.Action)
                    {
                        bondInfosItems[MMListBoxViewModel.PARAMKEY1_FIELD] = obj.Param_Key;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUE1_FIELD] = obj.Param_Value;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUE1BACK_FIELD] = obj.Param_Value;
                        bondInfosItems[MMListBoxViewModel.PARAMDEESC1_FIELD] = obj.Param_Desc;
                        bondInfosItems[MMListBoxViewModel.PARAMDSIDN1_FIELD] = obj.B_Text_Box_String;
                        bondInfosItems[MMListBoxViewModel.PARAMDUNIT1_FIELD] = obj.Para_Unit;
                        bondInfosItems[MMListBoxViewModel.ParamDescColor1_FIELD] = obj.Param_Desc_Color;
                        bondInfosItems[MMListBoxViewModel.ParamCheckValue1_FIELD] = obj.Param_Check_Value;
                        switch (obj.Param_Type)
                        {
                            case "Radio":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE1_FIELD] = ParaType.CheckBoxItem + "";
                                //bondInfosItems[PARAMDCOMMENT1_FIELD] = v != null ? v.Option_Desc : "";
                                break;
                            case "Text":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE1_FIELD] = ParaType.TextItem1 + "";
                                break;
                            case "ComboBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE1_FIELD] = ParaType.ComboBoxItem + "";
                                break;
                            case "DatePicker":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE1_FIELD] = ParaType.DatePickerItem + "";
                                break;
                            case "SearchTextBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                                break;
                            case "MultiString":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = obj != null ? obj.Para_Unit : "";
                                break;
                            case "TreeView":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TreeViewComBoxItem + "";
                                break;
                            default:
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE1_FIELD] = ParaType.TextItem1 + "";
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error("UpdateSubParas1:" + e + obj);
                }
            }
        }

        private void UpdateSubParas2(DataRow bondInfosItems, QuantParameter obj)
        {
            lock (this.LockObj)
            {
                try
                {
                    if ("1" == obj.Action || "2" == obj.Action)
                    {
                        bondInfosItems[MMListBoxViewModel.PARAMKEY2_FIELD] = obj.Param_Key;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUE2_FIELD] = obj.Param_Value;
                        bondInfosItems[MMListBoxViewModel.PARAMVALUE2BACK_FIELD] = obj.Param_Value;
                        bondInfosItems[MMListBoxViewModel.PARAMDEESC2_FIELD] = obj.Param_Desc;
                        bondInfosItems[MMListBoxViewModel.PARAMDSIDN2_FIELD] = obj.B_Text_Box_String;
                        bondInfosItems[MMListBoxViewModel.PARAMDUNIT2_FIELD] = obj.Para_Unit;
                        bondInfosItems[MMListBoxViewModel.ParamDescColor2_FIELD] = obj.Param_Desc_Color;
                        bondInfosItems[MMListBoxViewModel.ParamCheckValue2_FIELD] = obj.Param_Check_Value;
                        switch (obj.Param_Type)
                        {
                            case "Radio":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE2_FIELD] = ParaType.CheckBoxItem + "";
                                //bondInfosItems[PARAMDCOMMENT1_FIELD] = v != null ? v.Option_Desc : "";
                                break;
                            case "MultiSelect":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE2_FIELD] = ParaType.MultiSelectItem + "";
                                //bondInfosItems[PARAMDCOMMENT1_FIELD] = v != null ? v.Option_Desc : "";
                                break;
                            case "Text":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE2_FIELD] = ParaType.TextItem2 + "";
                                break;
                            case "ComboBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE2_FIELD] = ParaType.ComboBoxItem + "";
                                break;
                            case "DatePicker":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE2_FIELD] = ParaType.DatePickerItem + "";
                                break;
                            case "SearchTextBox":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                                break;
                            case "MultiString":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TextItem + "";
                                bondInfosItems[MMListBoxViewModel.PARAMDCOMMENT_FIELD] = obj != null ? obj.Para_Unit : "";
                                break;
                            case "TreeView":
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE_FIELD] = ParaType.TreeViewComBoxItem + "";
                                break;
                            default:
                                bondInfosItems[MMListBoxViewModel.PARAMTYPE2_FIELD] = ParaType.TextItem2 + "";
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error("UpdateSubParas2:" + e + obj);
                }
            }
        }
        #endregion


        #region 批量导入
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
            this.ImportDv.Table.Rows.Clear();
            importStr = importStr.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            string[] bonds = importStr.Split(splitStrArr, StringSplitOptions.RemoveEmptyEntries);
            foreach (string bond in bonds)
            {
                try
                {
                    BondsInfoPo bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(bond.ToUpper());
                    //如果合约代码用的是小写
                    if (bondInfoPo == null)
                    {
                        bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(bond.ToLower());
                    }

                    String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                    if (bondInfoPo == null || today.CompareTo(bondInfoPo.Maturity_Date.ToString()) > 0)
                    {
                        continue;
                    }
                    DataRow dr = this.ImportDv.Table.Rows.Find(bondInfoPo.Bond_ID);
                    if (dr == null)
                    {
                        dr = this.ImportDv.Table.NewRow();
                        dr[ISCHECK_FIELD] = true;
                        dr[SECURITYID_FIELD] = bondInfoPo.Bond_ID;
                        dr[SYMBOL_FIELD] = bondInfoPo.Short_Name;
                        dr[REMAININGDATE_FIELD] = bondInfoPo.RemaingingTime;
                        this.ImportDv.Table.Rows.Add(dr);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error($"识别失败:{bond}", e);
                }
            }
            CalCheckedCount();
        }

        private bool isStartRecog = false;
        public void RecognizeIntentionQuote(string content)
        {
            isStartRecog = true;
            this.ImportDv.Table.Rows.Clear();
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    string pattern = @"(\r\n)+";
                    string txtcont = System.Text.RegularExpressions.Regex.Replace(content, pattern, "\r\n");
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = txtcont;
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserBond(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                {
                                    int index = 0;
                                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        DataRow item = AddRecognize(obj);
                                        //item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }
                                }));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        private DataRow AddRecognize(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            if (isStartRecog)
            {
                isStartRecog = false;
            }

            DataRow dr = this.ImportDv.Table.Rows.Find(obj.securityId);
            if (dr == null)
            {
                dr = this.ImportDv.Table.NewRow();
                dr[ISCHECK_FIELD] = true;
                dr[SECURITYID_FIELD] = obj.securityId;
                dr[SYMBOL_FIELD] = obj.symbol;
                dr[REMAININGDATE_FIELD] = obj.timeToMaturity;
                this.ImportDv.Table.Rows.Add(dr);
            }

            return dr;
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

        public void BatchImportBonds()
        {
            BtnOK3Enabled = false;
            //if (ImportDv != null && ImportDv.Count > 0)
            //{
                List<HomeGroupSecurityidsUpdateReq> reqList = new List<HomeGroupSecurityidsUpdateReq>();
                foreach (DataRowView drv in ImportDv)
                {
                    if ((bool)drv[AlterParamViewModel.ISCHECK_FIELD] == true)
                    {
                        HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                        request.securityid = CommonUtils.ConvertToString(drv[AlterParamViewModel.SECURITYID_FIELD]);
                        request.symbol = CommonUtils.ConvertToString(drv[AlterParamViewModel.SYMBOL_FIELD]);
                        request.action = "1"; // 添加
                        request.remark = $"{request.securityid}  {request.symbol}  {CommonUtils.ConvertToString(drv[AlterParamViewModel.REMAININGDATE_FIELD])}";
                        request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    //request.type = CommonConstants.QUANT_Type;
                    reqList.Add(request);
                    }
                }
                //if (reqList.Count > 0)
                //{
                    ImportBonds(reqList);
                //}
            //}
        }

        private string groupID = string.Empty;

        public string GroupID
        {
            get
            {
                return this.groupID;
            }
            set
            {
                this.groupID = value;
                this.OnPropertyChanged();
            }
        }

        private string groupName = string.Empty;

        public string GroupName
        {
            get
            {
                return this.groupName;
            }
            set
            {
                this.groupName = value;
                this.OnPropertyChanged();
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

        private int totalCount = 0;
        private int cancleCount = 0;

        public void ImportBonds(List<HomeGroupSecurityidsUpdateReq> reqList)
        {
            //DataRowView row = SelectedParamItem;
            //StringBuilder str = new StringBuilder();
            //if (IsAdd)
            //{
            //    str.Append(row[MMListBoxViewModel.PARAMVALUE_FIELD] + "");
            //    if (str.Length > 0)
            //        str.Append(",");
            //}
            //int i = 0;
            //foreach (HomeGroupSecurityidsUpdateReq req in reqList)
            //{
            //    i++;
            //    str.Append(req.securityid);
            //    if (i < reqList.Count)
            //        str.Append(",");
            //}
            //SuccessCount = successCount + 1;
            //row[MMListBoxViewModel.PARAMVALUE_FIELD] = str;

            //DataRowView drv = SelectedGroupItem;
            //HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
            //request.securityid = addBondWind.securityID;
            //request.symbol = addBondWind.symbol;
            //request.action = "1"; // 添加
            //request.remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}";
            //request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            //request.type = MMListBoxViewModel.QUANT_Type;
            //request.groupid = drv == null ? "" : drv[MMListBoxViewModel.G_Id] + "";
            //RequestList.Add(request);
            DataRowView drv = SelectedGroupItem;
            foreach (HomeGroupSecurityidsUpdateReq req in reqList)
            {
                if (this.BondView.Table.Rows.Find(req.securityid) != null)
                {
                    continue;
                }

                req.action = "1"; // 添加
                req.groupid = drv == null ? "" : drv[MMListBoxViewModel.G_Id] + "";
                req.type = drv[MMListBoxViewModel.TYPE_FIELD] + "";
                req.username = SelectedGroupItem[MMListBoxViewModel.USERNAME_FIELD] + "";

                DataRow Item = this.BondView.Table.NewRow();
                Item[MMListBoxViewModel.B_Id] = pid++;
                Item[MMListBoxViewModel.B_GroupId] = req.groupid;
                Item[MMListBoxViewModel.B_securityid] = req.securityid;
                Item[MMListBoxViewModel.B_symbol] = req.symbol;
                string[] str = req.remark.Replace("  ", ",").Split(',');
                if (str != null && str.Count() == 3)
                    Item[MMListBoxViewModel.B_tenor] = str[2];
                Item[MMListBoxViewModel.B_type] = req.type;
                this.BondView.Table.Rows.Add(Item);
                RequestList.Add(req);
            }
            this.BondView.Table.AcceptChanges();
        }

        private volatile int callbackCount = 0;

        public int CallbackCount
        {
            get
            {
                return callbackCount;
            }

            set
            {
                callbackCount = value;
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





        #endregion

      

    }
}

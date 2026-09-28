using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Frame.ViewModel
{
    public class SecurityGroupDetailViewModel : ViewModelBase
    {
        public SecurityGroupDetailView securityGroupDetailView = null;
        private HomeGroupData _GroupItem;
        public HomeGroupData GroupItem
        {
            get
            {
                return _GroupItem;
            }

            set
            {
                _GroupItem = value;
                refreashData();
                this.OnPropertyChanged(nameof(GroupItem));
            }
        }
        private bool _CareGroupChecked;

        public bool CareGroupChecked
        {
            get { return _CareGroupChecked; }
            set
            {
                if (_CareGroupChecked != value)
                {
                    _CareGroupChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private bool _DynamicsGroupChecked;

        public bool DynamicsGroupChecked
        {
            get { return _DynamicsGroupChecked; }
            set
            {
                if (_DynamicsGroupChecked != value)
                {
                    _DynamicsGroupChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private bool _AllInstGroupChecked;

        public bool AllInstGroupChecked
        {
            get { return _AllInstGroupChecked; }
            set
            {
                if (_AllInstGroupChecked != value)
                {
                    _AllInstGroupChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private Dictionary<string, string> _SelectedValues;
        public Dictionary<string, string> SelectedValues
        {
            get { return _SelectedValues; }
            set
            {
                _SelectedValues = value;
                this.OnPropertyChanged();
            }
        }
        private bool _IsEnable = false;

        public bool IsEnable
        {
            get { return _IsEnable; }
            set
            {
                if (_IsEnable != value)
                {
                    _IsEnable = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private DataRowView _SelectedBondItem;
        public DataRowView SelectedBondItem
        {
            get
            {
                return _SelectedBondItem;
            }

            set
            {
                _SelectedBondItem = value;
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
        public const string B_Id = "Id";
        public const string B_GroupId = "GroupId";
        public const string B_securityid = "Securityid";
        public const string B_symbol = "Symbol";
        public const string B_tenor = "Tenor";
        public const string B_type = "Type";
        public const string B_RemainingTime = "RemainingTime";
        public const string B_updateTime = "UpdateTime";
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
                        _BondView.Table.Columns.Add(B_Id, typeof(string));
                        _BondView.Table.Columns.Add(B_GroupId, typeof(string));
                        _BondView.Table.Columns.Add(B_securityid, typeof(string));
                        _BondView.Table.Columns.Add(B_symbol, typeof(string));
                        _BondView.Table.Columns.Add(B_tenor, typeof(string));
                        _BondView.Table.Columns.Add(B_type, typeof(string));
                        _BondView.Table.Columns.Add(B_updateTime, typeof(string));
                        _BondView.Table.Columns.Add(B_RemainingTime, typeof(string));
                        _BondView.Sort = $"{B_updateTime} DESC ";

                        _BondView.Table.PrimaryKey = new DataColumn[] {
                        _BondView.Table.Columns[B_Id]
                    };
                        this.OnPropertyChanged();
                    }
                    return this._BondView;
                }
            }
        }
        public ICommand UpdateCommand { get { return new ComRelayCommand(UpdateClick); } }
        public ICommand DeleteCommand { get { return new ComRelayCommand(DeleteClick); } }
        public ICommand AddBondCommand { get { return new ComRelayCommand(AddBondClick); } }

        public ICommand DelBondCommand { get { return new ComRelayCommand(DelBondClick); } }

        public ICommand RecognizeImportCommand { get { return new ComRelayCommand(RecognizeImportClick); } }
 
        public SecurityGroupDetailViewModel() 
        {
            BindingOperations.EnableCollectionSynchronization(this.BondView, this.LockBObj);
            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
            AdminHandler.Instance.QuerySecurityGroupModule();
        }

        private void refreashData() 
        {
            if (GroupItem != null && (string.IsNullOrEmpty(GroupItem.inf1) || CommonConstants.HOME_Type.Equals(GroupItem.inf1)|| CommonConstants.QUANT_Type.Equals(GroupItem.inf1) || CommonConstants.INNER_Type.Equals(GroupItem.inf1)))
            {
                CareGroupChecked = GroupItem.groupMsgPo != null && "true".Equals(GroupItem.groupMsgPo.IsCareGroup) ? true : false;
                DynamicsGroupChecked = GroupItem.groupMsgPo != null && "true".Equals(GroupItem.groupMsgPo.IsDynamicsGroup) ? true : false;
                AllInstGroupChecked = GroupItem.groupMsgPo != null && "true".Equals(GroupItem.groupMsgPo.IsAllInstGroup) ? true : false;
                if (LocalEnv.UserInfo.UserName.Equals(GroupItem.username) || (!AdminHandler.Instance.getManageYourselfViewAll() && "*".Equals(AdminHandler.Instance.getSecurityGroupUser())))
                {
                    IsEnable = true;
                }
                else 
                {
                    IsEnable = false;
                }
            }
            else 
            {
                CareGroupChecked = false;
                DynamicsGroupChecked = false;
                AllInstGroupChecked = false;
                IsEnable = false;
            }
        }
        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum favorityType, string groupid, string securityid)
        {
            if (favorityType == HomePageDataHandler.FavoriteTypeEnum.DelBond && GroupItem.id.Equals(groupid))
            {
                lock (LockBObj)
                {
                    DataRow[] drs = this.BondView.Table.Select($"{B_securityid}= '{securityid}'");
                    if (drs != null && drs.Count() > 0)
                    {
                        this.BondView.Table.Rows.Remove(drs[0]);
                    }
                }
            }
        }
        public void initBond(List<HomeGroupSecurityIDsInfo> list)
        {
            if (list != null && list.Count > 0)
            {
                foreach (HomeGroupSecurityIDsInfo obj in list)
                {
                    lock (LockBObj)
                    {
                        DataRow Item = this.BondView.Table.Rows.Find(new object[] { obj.id });
                        if (Item == null)
                        {
                            Item = this.BondView.Table.NewRow();
                            Item[B_Id] = obj.id;
                            Item[B_GroupId] = obj.groupId;
                            Item[B_securityid] = obj.securityid;
                            Item[B_symbol] = obj.symbol;
                            string[] str = (obj.remark??"").Replace("  ", ",").Split(',');
                            if (str != null && str.Count() == 3)
                                Item[B_tenor] = str[2];
                            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(obj.securityid);
                            if (bondsInfoPo != null)
                            {
                                if (string.IsNullOrEmpty(bondsInfoPo.RemaingingTime) && "4".Equals(bondsInfoPo.MarketIndicator))
                                {
                                    Item[B_RemainingTime] = "已到期";
                                }
                                else
                                {
                                    Item[B_RemainingTime] = bondsInfoPo.RemaingingTime;
                                }
                            }
                            Item[B_updateTime] = obj.updatetime;
                            this.BondView.Table.Rows.Add(Item);
                        }
                        this.BondView.Table.AcceptChanges();
                    }
                }
            }
            else 
            {
                this.BondView.Table.AcceptChanges();
            }
        }
        public void clear()
        {
            try
            {
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
        private async void UpdateClick()
        {
            if (GroupItem.username.Equals(LocalEnv.UserInfo.UserName) && SecurityGroupManagerViewModel.IsGroupNameExist(GroupItem.groupname, GroupItem.id)) 
            {
                CustomMessageBox.Warning(string.Format("{0}组名已存在",GroupItem.groupname), "更新组");
                return;
            }
            HomeGroupUpdateReq request = new HomeGroupUpdateReq();
            request.id = GroupItem.id;
            request.groupname = GroupItem.groupname;//组名
            request.action = "2";//更新
            request.type = CareGroupChecked ? CommonConstants.HOME_Type : CommonConstants.QUANT_Type;
            GroupMsgPo groupMsgPo = new GroupMsgPo();
            groupMsgPo.IsCareGroup = CareGroupChecked ? "true" : "false";
            groupMsgPo.IsDynamicsGroup = DynamicsGroupChecked ? "true" : "false";
            groupMsgPo.IsAllInstGroup = AllInstGroupChecked ? "true" : "false";
            int i = 1;
            if (SelectedValues.Values.Count > 0)
            {
                foreach (string key in SelectedValues.Values)
                {
                    groupMsgPo.Tag += key;
                    if (i < SelectedValues.Values.Count)
                        groupMsgPo.Tag += " ";
                    i++;
                }
            }
            else
                groupMsgPo.Tag = "";
            request.inf2 = JsonConvert.SerializeObject(groupMsgPo);
            request.ordersn = GroupItem.ordersn.ToString();
            request.terminal = LocalEnv.ServerInfo.Version;
            request.closeby = LocalEnv.UserInfo.UserName;
            request.username = GroupItem.username;
            request.closeby = LocalEnv.UserInfo.UserName;
            PResult pResult = await HomePageDataHandler.AsyncUpdateGroups(request);
            if (pResult != null && pResult.Code == "0000")
            {
                HomePageDataHandler.RequestGroups(true);
                if (CareGroupChecked)
                {
                    if (GroupItem.groupMsgPo != null && "true".Equals(GroupItem.groupMsgPo.IsCareGroup))
                    {
                        HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.UpdateGroup, GroupItem.id, string.Empty);
                    }
                    else 
                    {
                        HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddGroup, GroupItem.id, string.Empty);
                    }
                }
                else
                {
                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelGroup, GroupItem.id, string.Empty);
                }
            }
        }

        private void DeleteClick()
        {
            string text = "是否确定删除券组？";
            string title = "删除券组";
            if (CustomMessageBox.Question(text, title,Window.GetWindow(securityGroupDetailView)) == true)
            {
                HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                request.id = GroupItem.id;//组名
                request.action = "3";//删除
                request.username = GroupItem.username;
                PResult pResult = HomePageDataHandler.UpdateGroups(request);
                if (pResult != null && pResult.Code == "0000")
                {
                    if (CommonConstants.HOME_Type.Equals(GroupItem.inf1))
                    {
                        HomePageDataHandler.RequestGroups(true);
                        HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelGroup, GroupItem.id, string.Empty);
                    }
                }
            }
        }
        private void AddBondClick()
        {
            AddBondWindow addBondWind = new AddBondWindow();
            //addBondWind.Owner = Application.Current.MainWindow;
            addBondWind.ShowDialog();
            bool result = addBondWind.Result;

            if (result == true)
            {
                bool isHomeGroup = GroupItem.groupMsgPo != null && GroupItem.groupMsgPo.IsCareGroup.Equals("true") ? true : false;

                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                request.securityid = addBondWind.securityID;
                request.symbol = addBondWind.symbol;
                request.action = "1"; // 添加
                request.remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}";
                request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                request.type = isHomeGroup ? CommonConstants.HOME_Type : CommonConstants.QUANT_Type;
                request.groupid = GroupItem.id;
                request.username = GroupItem.username;
                AddAndUpdateBond(request, isHomeGroup);
            }
        }
        public async void AddAndUpdateBond(HomeGroupSecurityidsUpdateReq req,bool isHomeGroup)
        {
            try
            {
                PResult result = await HomePageDataHandler.AsyncUpdateSecurityids(req);
                if (result != null && result.Code == "0000")
                {
                    if (isHomeGroup)
                    {
                        await HomePageDataHandler.AsyncRequestGroups(true);
                    }
                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddBond, req.groupid, req.securityid);
                }
                else
                {
                    CustomMessageBox.Warning(result.Msg, "新增债券");
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }
        private void DelBondClick()
        {
            if (SelectedBondItem == null)
            {
                CustomMessageBox.Info("请先选择债券！", "");
                return;
            }
            string text = "是否确定删除该债券？";
            string title = "删除债券";
            if (CustomMessageBox.Question(text, title, Window.GetWindow(securityGroupDetailView)) == true)
            {
                string id = CommonUtils.ConvertToString(SelectedBondItem[B_Id]);
                string securityID = CommonUtils.ConvertToString(SelectedBondItem[B_securityid]);
                bool isHomeGroup = GroupItem.groupMsgPo != null && GroupItem.groupMsgPo.IsCareGroup.Equals("true") ? true : false;

                HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
                req.groupid = GroupItem.id;
                req.id = id;
                req.action = "3"; //删除;
                DelBond(req,isHomeGroup);
            }
        }
        public async void DelBond(HomeGroupSecurityidsUpdateReq req,bool isHomeGroup)
        {
            PResult result = await HomePageDataHandler.AsyncUpdateSecurityids(req);

            if (result != null && result.Code == "0000")
            {
                if (isHomeGroup)
                {
                    await HomePageDataHandler.AsyncRequestGroups(true);
                }
                HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelBond, req.groupid, SelectedBondItem[B_securityid]+"");
            }
        }
        private void RecognizeImportClick()
        {
            if (GroupItem == null)
            {
                CustomMessageBox.Warning("请先选择组！", "批量导入");
                return;
            }
            ObservableCollection<HomeGroupInfo> AddHomeGroupDic = new ObservableCollection<HomeGroupInfo>();
            foreach (KeyValuePair<string, HomeGroupInfo> pair in SecurityGroupManagerViewModel.dicGroup)
            {
                if (CommonConstants.HOME_Type.Equals(pair.Value.inf1) || CommonConstants.QUANT_Type.Equals(pair.Value.inf1))
                {
                    AddHomeGroupDic.Add(pair.Value);
                }
            }
            RecognizeBondView recognizeBondView = new RecognizeBondView();
            recognizeBondView.setDefaultGroupID(GroupItem.id, AddHomeGroupDic);
            recognizeBondView.ShowDialog();
            bool result = recognizeBondView.Result;

            if (result == true)
            {
                if (recognizeBondView.UpdateList != null && recognizeBondView.SelectedAddHomeGroup != null) 
                {
                    Task task = AddAndUpdateBondAsync(recognizeBondView.UpdateList, recognizeBondView.isHomeGroup, recognizeBondView.SelectedAddHomeGroup);
                }
            } 
        }

        public async Task AddAndUpdateBondAsync(List<HomeGroupSecurityidsUpdateReq> reqList, bool isHomeGroup,HomeGroupInfo SelectedAddHomeGroup)
        {
            try
            {
                List<string> errorList = new List<string>();
                foreach (HomeGroupSecurityidsUpdateReq req in reqList)
                {
                    PResult result = await HomePageDataHandler.AsyncUpdateSecurityids(req);
                    if (!(result != null && result.Code == "0000"))
                    {
                        errorList.Add(string.Format("{0}", result != null ? result.Msg : "无"));
                    }
                }
                if (isHomeGroup)
                {
                    await HomePageDataHandler.AsyncRequestGroups(true);
                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.BatchImport, SelectedAddHomeGroup.id, string.Empty);
                }
                if (errorList.Count() > 0)
                {
                    string errorMsg = "";
                    foreach (string error in errorList)
                    {
                        errorMsg = errorMsg + error + "\n";
                    }
                    CustomMessageBox.Warning(string.Format("{0}部分导入失败原因:\n{1}", SelectedAddHomeGroup.groupname, errorMsg), "批量导入债券");
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }
        /// <summary>
        /// 提示Warn消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                STC.Common.View.UI.MessageBoxGUI me = new STC.Common.View.UI.MessageBoxGUI();
                me.HideCancelButton();
                me.setWarnMsg(message);
                me.ShowDialog();
            }));
        }
    }
}

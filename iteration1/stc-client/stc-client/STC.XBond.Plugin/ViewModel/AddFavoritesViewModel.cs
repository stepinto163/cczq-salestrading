using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 添加收藏AddFavoritesView的数据内容
    /// </summary>
    public class AddFavoritesViewModel : ViewModelBase
    {
        private string _securityID = string.Empty;
        private string _securityName = string.Empty;
        private object _lock = new object();
        private ObservableCollection<GroupItems> _groupsList = new ObservableCollection<GroupItems>();

        public string SecurityID
        {
            get
            {
                return _securityID;
            }
            set
            {
                if (SecurityID != value)
                {
                    _securityID = value;
                    LoadGroups();
                    this.OnPropertyChanged();
                }
            }
        }


        public string SecurityName
        {
            get
            {
                return _securityName;
            }

            set
            {
                _securityName = value;
            }
        }

        public ObservableCollection<GroupItems> GroupsList
        {
            get
            {
                lock(_lock)
                {
                    return this._groupsList;
                }
            }
        }

        public AddFavoritesViewModel()
        {
            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

            BindingOperations.EnableCollectionSynchronization(_groupsList, _lock);
        }

        ~AddFavoritesViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            HomePageDataHandler.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;
            base.Dispose();
        }

        private async void LoadGroups()
        {
            IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroupList  = await HomePageDataHandler.AsyncRequestGroups(true);
            if (sortedGroupList != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> pair  in sortedGroupList)
                {
                    HomeGroupInfo homeGroupInfo = pair.Value;
                    GroupItems groupItem = new GroupItems();
                    groupItem.IsChecked = homeGroupInfo.securityids.Any(bond => bond.securityid == this.SecurityID);
                    groupItem.GroupId = homeGroupInfo.id;
                    groupItem.GroupName = homeGroupInfo.groupname;
                    _groupsList.Add(groupItem);
                }
            }
        }

        public void ChangeGroupBonds()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = HomePageDataHandler.RequestGroups(false);
                    if (dic != null)
                    {
                        foreach (GroupItems groupItems in _groupsList)
                        {
                            HomeGroupInfo homeGroupInfo = HomePageDataHandler.GetHomeGroupInfo(dic, groupItems.GroupId);
                            if (homeGroupInfo == null)
                            {
                                continue;
                            }
                            IEnumerable<HomeGroupSecurityIDsInfo> info = homeGroupInfo.securityids.Where(value => value.securityid == this.SecurityID);
                            bool securityid_Server = info.Count() > 0 ? true : false;
                            bool securityid_Local = groupItems.IsChecked;

                            if (securityid_Server == true && securityid_Local == false)
                            {
                                // 删除操作
                                HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
                                req.groupid = groupItems.GroupId;
                                req.id = info.First().id;
                                req.action = "3"; //删除;
                                PResult result = HomePageDataHandler.UpdateSecurityids(req);

                                if (result != null && result.Code == "0000")
                                {
                                    HomePageDataHandler.RequestGroups(true);
                                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelBond, groupItems.GroupId, this.SecurityID);
                                }
                            }
                            else if (securityid_Server == false && securityid_Local == true)
                            {
                                // 添加操作
                                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                                request.securityid = this.SecurityID;
                                request.symbol = this.SecurityName;
                                request.action = "1"; // 添加
                                request.remark = $"{this.SecurityID}  {this.SecurityName} ";
                                request.terminal = LocalEnv.ServerInfo.Version;
                                request.groupid = groupItems.GroupId;
                                request.type = CommonConstants.HOME_Type;
                                request.username = LocalEnv.UserInfo.UserName;
                                PResult result = HomePageDataHandler.UpdateSecurityids(request);
                                if (result != null && result.Code == "0000")
                                {
                                    HomePageDataHandler.RequestGroups(true);
                                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddBond, groupItems.GroupId, this.SecurityID);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum favoriteTypeEnum, string groupID, string securityID)
        {
            if (favoriteTypeEnum == HomePageDataHandler.FavoriteTypeEnum.AddGroup)
            {
                // 刷新缓存数据, 并添加新的标签页
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroups = HomePageDataHandler.RequestGroups(false);
                if (sortedGroups != null)
                {
                    HomeGroupInfo gi = HomePageDataHandler.GetHomeGroupInfo(sortedGroups, groupID);
                    GroupItems groupItem = new GroupItems();
                    groupItem.IsChecked = false;
                    groupItem.GroupId = gi.id;
                    groupItem.GroupName = gi.groupname;
                    _groupsList.Add(groupItem);
                }
            }
        }
    }
}

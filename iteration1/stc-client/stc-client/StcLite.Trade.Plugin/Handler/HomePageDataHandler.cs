using com.gateway.log;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using BDB.Price.Plugin.Handler;

namespace BDB.Trade.Plugin.Handler
{
    public class HomePageDataHandler
    {
        private static HomePageDataHandler _homePageDataHandler = new HomePageDataHandler();
        public static HomePageDataHandler Instance { get { return _homePageDataHandler; } }

        private HomePageDataHandler()
        { }


        /// <summary>
        /// 有增删改操作， 就要通知其他界面， 更新收藏标志
        /// 参数1：  AddGroup: 添加关注组； UpdateGroup：更新关注组；  DelGroup： 删除关注组 ； AddBond: 添加关注债券； DelBond: 删除债券
        /// 参数2： GroupID 
        /// 参数3： SecurityID 
        /// </summary>
        private readonly object m_OnFavoriteEventLock = new object();
        private Action<FavoriteTypeEnum, string, string, string> _OnFavoritesChanged;
        public event Action<FavoriteTypeEnum, string, string, string> OnFavoritesChanged
        {
            add
            {
                lock (m_OnFavoriteEventLock)
                {
                    _OnFavoritesChanged += value;
                }
            }
            remove
            {
                lock (m_OnFavoriteEventLock)
                {
                    _OnFavoritesChanged -= value;
                }
            }
        }



        /// <summary>
        /// 缓存组内容(订阅)
        /// </summary>
        private IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> _CacheSortedGroups;

        private Dictionary<string, string> _FavoriteSecurityDic = new Dictionary<string, string>();
        public void LoadGroupBonds(bool loadFromDB)
        {
            HomeGroupSecurityidsReplyPo replyPo = null;

            if (loadFromDB)
            {
                replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery(null);
            }
            else
            {
                if (_CacheSortedGroups == null)
                {
                    replyPo = AdminHandler.Instance.GetCacheHomeGroupSecurityidsQuery();
                    if (replyPo == null)
                        replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery(null);
                }
            }

            if (replyPo != null)
            {
                if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
                {
                    // 收集收藏债券
                    lock (_FavoriteSecurityDic)
                    {
                        _FavoriteSecurityDic.Clear();

                        foreach (HomeGroupInfo hgi in replyPo.Datas.Values)
                        {
                            foreach (HomeGroupSecurityIDsInfo hsi in hgi.securityids)
                            {
                                if (hsi != null)
                                {
                                    _FavoriteSecurityDic[hsi.securityid] = hsi.securityid;
                                }
                                else
                                {
                                    LogHelper.Info("收藏数据有空值");
                                }

                            }
                        }
                    }

                    _CacheSortedGroups = replyPo.Datas.OrderBy(o => o.Value.ordersn);
                    
                }
            }
        }

        private void UpdateCacheGroup(string groupName, HomeGroupInfo groupInfo)
        {
            bool isExist = this.IsGroupNameExist(groupName);
            if (!isExist)
            {
                ((Dictionary<string, HomeGroupInfo>)_CacheSortedGroups)[groupName] = groupInfo;
            }

        }

        /// <summary>
        /// 获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer">是否重新从服务器取数据</param>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> RequestGroups(bool isUpdateFromServer)
        {
            if (isUpdateFromServer == false)
            {
                if (_CacheSortedGroups == null)
                {
                    LoadGroupBonds(false);
                }
            }
            else
            {
                LoadGroupBonds(true);
            }

            return _CacheSortedGroups;
        }


        public void Notify(FavoriteTypeEnum favoriteTypeEnum, string groupid, string securtityID, string id)
        {
            _OnFavoritesChanged?.Invoke(favoriteTypeEnum, groupid, securtityID, id);
        }


        /// <summary>
        /// 异步获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer"></param>
        /// <returns></returns>
        //public Task<IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>>> AsyncRequestGroups(bool isUpdateFromServer)
        public IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> AsyncRequestGroups(bool isUpdateFromServer)
        {
            //return Task.Run(() => {

            if (isUpdateFromServer)
            {
                int count = 0;
                while (true)
                {
                    try
                    {
                        LogHelper.Info("SyncIdcStatus:" + BaseHandler.Instance.GetSyncIdcStatus());
                        if (BaseHandler.Instance.GetSyncIdcStatus() || count == 50)
                            break;
                        else
                        {
                            count++;
                            Thread.Sleep(200);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("AsyncRequestGroups wait sync idc execption:" + e.Message);
                    }
                }
            }


            IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> orderedInfo = RequestGroups(isUpdateFromServer);

                return orderedInfo;
            //});
        }

        public void DeleteGroup(string groupId)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                    request.id = groupId;//组名
                    request.action = "3";//删除
                    PResult pResult = UpdateGroups(request);
                    if (pResult != null && pResult.Code == "0000")
                    {
                        SettleCache.Instance.RemoveByGroupId(groupId);

                        RequestGroups(true);
                        Notify(FavoriteTypeEnum.DelGroup, groupId, string.Empty, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        public void DelAndAddBond(string groupID, string id, string securityId, HomeGroupSecurityidsUpdateReq addReq, Action<PResult, string, string> callBack)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
                    req.groupid = groupID;
                    req.id = id;
                    req.action = "3"; //删除;
                    PResult pResult = UpdateSecurityids(req);


                    if (pResult != null && (pResult.Code == "0000" || pResult.Code == "1003"))
                    {
                        SettleCache.Instance.RemoveById(groupID, id);

                        var tem = AsyncRequestGroups(true);

                        Notify(FavoriteTypeEnum.DelBond, groupID, securityId, id);



                        this.AddBond(addReq, callBack);
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }


        public async void DelBond(DisplayMode displayMode, string groupID, string id, string securityId)
        {
            await Task.Run(() =>
            {
                try
                {
                    int count = HomePageDataHandler.Instance.GetSecurityCount(securityId);
                    if (count <= 1)
                    {
                        Price.Plugin.ViewModel.Data.MsgSettingItem msgSettingItem = MsgSettingHandler.Instance.GetMsgSettingItemBySecurtiyID(securityId);
                        if (msgSettingItem != null)
                        {
                            MsgSettingHandler.Instance.SaveGlobalPara(msgSettingItem, true, false);
                        }
                    }
                    HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
                    req.groupid = groupID;
                    req.id = id;
                    req.action = "3"; //删除;
                    PResult pResult = UpdateSecurityids(req);


                    if (pResult != null && (pResult.Code == "0000" || pResult.Code == "1003"))
                    {
                        SettleCache.Instance.RemoveById(groupID, id);

                        var tem = AsyncRequestGroups(true);

                        //Notify(FavoriteTypeEnum.DelBondFromTable, groupID, securityId, id);

                        if (displayMode == DisplayMode.List)
                        {
                            Notify(FavoriteTypeEnum.DelBondFromTable, groupID, securityId, id);
                            QuoteConfigHandler.Instance.NotifyOnGroupTableUpdate(groupID, id, securityId, FavoriteTypeEnum.DelBondFromTable);
                        }
                        else
                        {
                            Notify(FavoriteTypeEnum.DelBond, groupID, securityId, id);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });

        }

        public void AddGroup(string groupName)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    // 取缓存的最后一条数据，算顺序
                    int ordersn = 1;
                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroups = RequestGroups(false);
                    if (sortedGroups != null && sortedGroups.Count() > 0)
                    {
                        ordersn = sortedGroups.Last().Value.ordersn + 1;
                    }

                    string groupname = groupName;
                    if (string.IsNullOrEmpty(groupname) == false)
                    {
                        HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                        request.groupname = groupname;//组名
                        request.action = "1";//新增
                        request.ordersn = ordersn.ToString();//顺序
                        request.terminal = LocalEnv.ServerInfo.Version; // 终端
                        request.id = string.Empty;
                        request.remark = string.Empty;
                        PResult pResult = AdminHandler.Instance.homeGroupUpdate(request);
                        if (pResult != null && pResult.Code == "0000")
                        {

                            this.RequestGroups(true);

                            _OnFavoritesChanged?.Invoke(FavoriteTypeEnum.AddGroup, string.Empty, string.Empty, string.Empty);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        public PResult UpdateGroups(HomeGroupUpdateReq request)
        {
            PResult pResult = AdminHandler.Instance.homeGroupUpdate(request);
            return pResult;
        }
        
        public PResult UpdateSecurityids(HomeGroupSecurityidsUpdateReq request)
        {
            PResult pResult = AdminHandler.Instance.homeSecurityidsUpdate(request);
            return pResult;
        }


        public bool IsGroupNameExist(string groupName)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> pair in _CacheSortedGroups)
                {
                    if (pair.Value.groupname == groupName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public string GetGroupNameByGroupID(string groupId)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> pair in _CacheSortedGroups)
                {
                    if (pair.Key == groupId)
                    {
                        return pair.Value.groupname;
                    }
                }
            }
            return string.Empty;
        }

        public HomeGroupInfo GetHomeGroupInfoByCache(string groupId)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> item in _CacheSortedGroups)
                {
                    if (item.Key == groupId)
                    {
                        return item.Value;
                    }
                }
            }

            return null;
        }

        public HomeGroupInfo GetHomeGroupInfo(IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> list, string groupId)
        {
            if (list != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> item in list)
                {
                    if (item.Key == groupId)
                    {
                        return item.Value;
                    }
                }
            }

            return null;
        }


        public HomeGroupInfo GetHomeGroupInfo(string groupId)
        {
            if (this._CacheSortedGroups != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> item in _CacheSortedGroups)
                {
                    if (item.Key == groupId)
                    {
                        return item.Value;
                    }
                }
            }

            return null;
        }

        public HomeGroupSecurityIDsInfo GetHomeGroupSecurityIDsInfo(string groupId, string bondId)
        {
            HomeGroupSecurityIDsInfo result = null;
            HomeGroupInfo homeGroupInfo = GetHomeGroupInfo(groupId);

            if (homeGroupInfo != null)
            {
                List< HomeGroupSecurityIDsInfo> selectItems = homeGroupInfo.securityids.Where(delegate (HomeGroupSecurityIDsInfo item)
                {
                    return item.id == bondId;
                }).ToList();

                if (selectItems != null && selectItems.Count > 0)
                    result = selectItems[0];
            }
            
            return result;
        }

        public int GetSecurityCount(string bondId)
        {
            int count = 0;
            if (this._CacheSortedGroups != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> item in _CacheSortedGroups)
                {
                    foreach(HomeGroupSecurityIDsInfo securityInfo in item.Value.securityids)
                    {
                        if (securityInfo.securityid == bondId)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        public void AddBond(HomeGroupSecurityidsUpdateReq req, Action<PResult, string, string> callBack)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {

                try
                {
                    PResult result = UpdateSecurityids(req);

                    Dictionary<string, string> oldDic = GetAllSecurityId(req.groupid, req.securityid);

                    RequestGroups(true);

                    Dictionary<string, string> newDic = GetAllSecurityId(req.groupid, req.securityid);

                    string id = GetNewId(oldDic, newDic);

                    if (result != null && result.IsSuccess())
                    {
                        callBack?.Invoke(result, req.securityid, id);


                        Notify(FavoriteTypeEnum.AddBond, req.groupid, req.securityid, id);

                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }


        private string GetNewId(Dictionary<string, string> oldDic, Dictionary<string, string> newDic)
        {
            string result = string.Empty;

            foreach (string id in newDic.Keys)
            {
                if (!oldDic.ContainsKey(id))
                {
                    result = id;

                    break;
                }
            }

            return result;
        }


        private Dictionary<string, string> GetAllSecurityId(string groupId, string securityId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            HomeGroupInfo homeGroupInfo = GetHomeGroupInfo(groupId);

            foreach (HomeGroupSecurityIDsInfo homeGroupSecurityIDsInfo in homeGroupInfo.securityids)
            {
                if (homeGroupSecurityIDsInfo.securityid == securityId && !dic.Keys.Contains(homeGroupSecurityIDsInfo.id))
                {
                    dic.Add(homeGroupSecurityIDsInfo.id, homeGroupSecurityIDsInfo.securityid);
                }
            }

            return dic;
        }

    }


    public enum FavoriteTypeEnum
    {
        NoGroup,
        AddGroup,
        DelGroup,
        UpdateGroup,
        AddBond,
        DelBond,
        DelBondFromTable,
        BatchImport,
        AddBondFromTable,
        AddBondFromCard,
        Adjust
    }
}

using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    /// <summary>
    /// 主页数据工具类
    /// </summary>
    public class HomePageDataHandler
    {
        public enum FavoriteTypeEnum
        {
            AddGroup,
            DelGroup,
            UpdateGroup,
            AddBond,
            DelBond,
            BatchImport,
        }
        /// <summary>
        /// 有增删改操作， 就要通知其他界面， 更新收藏标志
        /// 参数1：  AddGroup: 添加关注组； UpdateGroup：更新关注组；  DelGroup： 删除关注组 ； AddBond: 添加关注债券； DelBond: 删除债券
        /// 参数2： GroupID 
        /// 参数3： SecurityID 
        /// </summary>
        private static readonly Object m_OnFavoriteEventLock = new Object();
        private static Action<FavoriteTypeEnum, string, string> _OnFavoritesChanged;
        public static event Action<FavoriteTypeEnum, string, string> OnFavoritesChanged
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
        private static IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> _CacheSortedGroups;
        /// <summary>
        /// 获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer">是否重新从服务器取数据</param>
        /// <returns></returns>
        public static IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> RequestGroups(bool isUpdateFromServer)
        {
            if (isUpdateFromServer == false)
            {
                if (_CacheSortedGroups == null)
                {
                    LoadGroupBonds();
                }
            }
            else
            {
                LoadGroupBonds();
            }
            
            return _CacheSortedGroups;
        }

        /// <summary>
        /// 异步获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer"></param>
        /// <returns></returns>
        public static Task<IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>>> AsyncRequestGroups(bool isUpdateFromServer)
        {
            return Task.Run(() => {
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> orderedInfo = RequestGroups(isUpdateFromServer);
                return orderedInfo;
            });
        }

        public static HomeGroupInfo GetHomeGroupInfo(IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> list, string groupId)
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

        ///// <summary>
        ///// 缓存券内容(订阅), key : groupid, value : HomeGroupInfo
        ///// </summary>
        //private static SortedDictionary<string, HomeGroupInfo> _CacheGroupBonds = new SortedDictionary<string, HomeGroupInfo>(());
        //public static SortedDictionary<string, HomeGroupInfo> RequestGroupBonds(bool isUpdateFromServer)
        //{
        //    if (isUpdateFromServer == false)
        //    {
        //        if (_CacheGroupBonds == null)
        //        {
        //            UpdateGroupBonds();
        //        }
        //    }
        //    else
        //    {
        //        UpdateGroupBonds();
        //    }

        //    return _CacheGroupBonds;
        //}


        private static Dictionary<string, string> _FavoriteSecurityDic = new Dictionary<string, string>();
        private static void LoadGroupBonds()
        {
            HomeGroupSecurityidsReplyPo replyPo = null;
            if (_CacheSortedGroups == null)
            {
                replyPo = AdminHandler.Instance.GetCacheHomeGroupSecurityidsQuery();
            }
            else
            {
                replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery();
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
                                    LogHandler.Info("收藏数据有空值");
                                }
                               
                            }
                        }
                    }

                    _CacheSortedGroups = replyPo.Datas.OrderBy(o => o.Value.ordersn);

                }
            }
        }

        public static string IsFavorite(string securityID)
        {
            if (_FavoriteSecurityDic != null)
            {
                RequestGroups(false);
                lock (_FavoriteSecurityDic)
                {
                    if (string.IsNullOrEmpty(securityID) == false && _FavoriteSecurityDic.ContainsKey(securityID))
                    {
                        return "1";
                    }
                }
            }
           
            return "0";
        }

        public static async void AddGroup(string groupName)
        {
            try
            {
                // 取缓存的最后一条数据，算顺序
                int ordersn = 1;
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroups = HomePageDataHandler.RequestGroups(false);
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
                    PResult pResult = AdminHandler.Instance.homeGroupUpdate(request);
                    if (pResult != null && pResult.Code == "0000")
                    {
                        // 刷新缓存数据, 并添加新的标签页
                        //HomeGroupInfo gi = HomePageDataHandler.RequestGroups(true).Last().Value;
                        //AddGroupTabItem(gi);
                        await AsyncRequestGroups(true);
                        HomePageDataHandler._OnFavoritesChanged?.Invoke(FavoriteTypeEnum.AddGroup, string.Empty, string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        public static PResult UpdateGroups(HomeGroupUpdateReq request)
        {
            PResult pResult = AdminHandler.Instance.homeGroupUpdate(request);
            return pResult;
        }

        public static Task<PResult> AsyncUpdateGroups(HomeGroupUpdateReq request)
        {
            return Task.Run(() =>
            {
                PResult pResult = null;
                try
                {
                    pResult = UpdateGroups(request);
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
                return pResult;
            });
        }

        public static PResult UpdateSecurityids(HomeGroupSecurityidsUpdateReq request)
        {
            PResult pResult = AdminHandler.Instance.homeSecurityidsUpdate(request);
            return pResult;
        }

        public static Task<PResult> AsyncUpdateSecurityids(HomeGroupSecurityidsUpdateReq request)
        {
            return Task.Run(() =>
            {
                PResult pResult = null;
                try
                {
                    pResult = UpdateSecurityids(request);
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
                return pResult;
            });
        }

        public static void Notify(FavoriteTypeEnum favoriteTypeEnum, string groupid, string securtityID)
        {
            HomePageDataHandler._OnFavoritesChanged?.Invoke(favoriteTypeEnum, groupid, securtityID);
        }

        #region 记录每页显示类型
        private const string HomePageMode = "HomePageMode";
        private const string Modes = "Modes";

        public static ConcurrentDictionary<string, string> CatchHomePageDisplayModeDic = null;

        private static string GetCacheKey()
        {
            return string.Format("{0}-{1}", HomePageMode, LocalEnv.UserInfo.UserName);
        }

        public static void WriteHomePageDisplayMode()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach(KeyValuePair<string, string> pair in CatchHomePageDisplayModeDic)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                
                sb.Append($"{pair.Key}-{pair.Value}");
            }

            if (sb.Length > 0)
            {
                IniFileHelper.WriteValue(GetCacheKey(), Modes, sb.ToString());
            }
        }

        public static void ReadHomePageDisplayMode()
        {
            if (CatchHomePageDisplayModeDic == null)
            {
                CatchHomePageDisplayModeDic = new ConcurrentDictionary<string, string>();
                string str = IniFileHelper.ReadValue(GetCacheKey(), Modes);
                if (string.IsNullOrEmpty(str) == false)
                {
                    string[] arr = str.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 0)
                    {
                        foreach (string item in arr)
                        {
                            string[] itemArr = item.Split(CommonConstants.MID_LINE_CHAR, StringSplitOptions.RemoveEmptyEntries);
                            if (itemArr != null && itemArr.Length == 2)
                            {
                                CatchHomePageDisplayModeDic.AddOrUpdate(itemArr[0], itemArr[1], (key, value) => { return itemArr[1]; });
                            }
                        }
                    }
                }
            }
        }
        #endregion

        public static bool IsGroupNameExist(string groupName)
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
        
        public static string GetGroupNameByGroupID(string groupId)
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
    }
}

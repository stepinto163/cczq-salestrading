using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MMshe;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Mmshe.Plugin.Command;
using STC.Mmshe.Plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Mmshe.Plugin.Utils.Data
{
    /// <summary>
    /// 主页数据工具类
    /// </summary>
    public class MMsheGroupHandler
    {
        public enum FavoriteTypeEnum
        {
            AddGroup,
            DelGroup,
            UpdateGroup,
            //AddBond,
            //DelBond,
            //BatchImport,
        }
        /// <summary>
        /// 有增删改操作， 就要通知其他界面， 更新收藏标志
        /// 参数1：  AddGroup: 添加关注组； UpdateGroup：更新关注组；  DelGroup： 删除关注组 ；
        /// 参数2： GroupID 
        /// 参数3： SecurityID 
        /// </summary>
        private readonly Object m_OnFavoriteEventLock = new Object();
        private Action<FavoriteTypeEnum, string, string> _OnFavoritesChanged;
        public event Action<FavoriteTypeEnum, string, string> OnFavoritesChanged
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
        public int FavoritesChangedCount
        {
            get
            {
                if (_OnFavoritesChanged != null)
                {
                    return _OnFavoritesChanged.GetInvocationList().Count();
                }
                return 0;
            }
        }

        private readonly Object m_OnMMBondsEventLock = new Object();
        private Action<STC.Common.Model.MMshe.MMshe> _OnMMBonds;
        public event Action<STC.Common.Model.MMshe.MMshe> OnMMBonds
        {
            add
            {
                lock (m_OnMMBondsEventLock)
                {
                    _OnMMBonds += value;
                }
            }
            remove
            {
                lock (m_OnMMBondsEventLock)
                {
                    _OnMMBonds -= value;
                }
            }

        }

        private ConcurrentDictionary<string, STC.Common.Model.MMshe.MMshe> _CacheMMDic = new ConcurrentDictionary<string, STC.Common.Model.MMshe.MMshe>();
        private ConcurrentDictionary<string, Action<STC.Common.Model.MMshe.MMshe>> _MMCallbackDic = new ConcurrentDictionary<string, Action<STC.Common.Model.MMshe.MMshe>>();

        /// <summary>
        /// 缓存组债券信息
        /// key: groupid; value: List<MM>
        /// </summary>
        private ConcurrentDictionary<string, List<STC.Common.Model.MMshe.MMshe>> _CacheMMListDic = new ConcurrentDictionary<string, List<STC.Common.Model.MMshe.MMshe>>();

        /// <summary>
        /// 构造函数
        /// </summary>
        private MMsheGroupHandler()
        {
            this.RegSystemReadyEvent();
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private volatile bool isInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            if (isInit == false)
            {
                MMsheHandler.Instance.SubscribeMM(Instance_OnMM);

                isInit = true;
            }
        }
        public void RegSystemReadyEvent()
        {
            DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
        }

        /// <summary>
        /// 恢复窗口
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSystemReady(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PopupWindowCommand popupWindowCommand = new PopupWindowCommand();
                //加载行情小窗
                string lstkeys = IniFileHelper.ReadValue($"OrderControlShe-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyList = lstkeys.Split(',');
                for (int i = 0; i < keyList.Length; i++)
                {
                    string securityID = keyList[i];
                    if (!string.IsNullOrEmpty(securityID))
                    {
                        List<object> pobject = new List<object>();
                        PopupTypeEnum pte = PopupTypeEnum.OrderControl;
                        pobject.Add(pte);
                        pobject.Add(securityID);
                        popupWindowCommand.Execute(pobject.ToArray());
                    }
                }

                //加载打开的窗口
                string lstkeysw = IniFileHelper.ReadValue($"STC.Mmshe.Plugin-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyListw = lstkeysw.Split(',');
                for (int i = 0; i < keyListw.Length; i++)
                {
                    string winid = keyListw[i];
                    if (!string.IsNullOrEmpty(winid))
                    {
                        string para = IniFileHelper.ReadValue($"STC.Mmshe.Plugin_{LocalEnv.UserInfo.UserName}_{winid}", "CommandParameter");
                        if (!string.IsNullOrEmpty(para))
                        {
                            PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), para);
                            List<object> pobject = new List<object>();
                            pobject.Add(pye);
                            popupWindowCommand.Execute(pobject.ToArray());
                        }
                    }
                }
            }));
        }
        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("MMSheGroupHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                _CacheMMListDic.Clear();
                isInit = false;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        init();
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
        }

        ///// <summary>
        ///// 做市业务债券详细信息回调方法
        ///// </summary>
        ///// <param name="obj"></param>
        //private void Instance_OnANSMonitorInfo(ANSMonitorInfo obj)
        //{
        //    string primarykey = $"{obj.securityId}#{obj.settleType}";
        //    lock (string.Intern(primarykey))
        //    {
        //        _CacheAnsMonitorInfoDic[primarykey] = obj;
        //    }
        //    _OnAnsMonitorInfo?.Invoke(obj);

        //}

        private void Instance_OnMM(STC.Common.Model.MMshe.MMshe obj)
        {
            if (string.IsNullOrEmpty(obj.groupid) == false)
            {
                lock (string.Intern(obj.groupid))
                {
                    if ("3" == obj.Action)
                    {
                        if (_CacheMMListDic.ContainsKey(obj.groupid) == true)
                        {
                            int findIndex = _CacheMMListDic[obj.groupid].FindIndex(mm => mm.securityid == obj.securityid);
                            if (findIndex > -1)
                            {
                                _CacheMMListDic[obj.groupid].RemoveAt(findIndex);

                            }
                        }
                    }
                    else if ("1" == obj.Action || "2" == obj.Action)
                    {
                        if (_CacheMMListDic.ContainsKey(obj.groupid) == false)
                        {
                            _CacheMMListDic[obj.groupid] = new List<STC.Common.Model.MMshe.MMshe>();
                        }

                        int findIndex = _CacheMMListDic[obj.groupid].FindIndex(mm => mm.securityid == obj.securityid);
                        if (findIndex > -1)
                        {
                            _CacheMMListDic[obj.groupid].RemoveAt(findIndex);
                            _CacheMMListDic[obj.groupid].Insert(findIndex, obj);
                        }
                        else
                        {
                            _CacheMMListDic[obj.groupid].Insert(0, obj);
                        }
                    }
                }
            }
            _OnMMBonds?.Invoke(obj);

            if (string.IsNullOrEmpty(obj.getKey()) == false)
            {
                lock (string.Intern(obj.getKey()))
                {
                    if ("3" == obj.Action)
                    {
                        STC.Common.Model.MMshe.MMshe removeMM = null;
                        _CacheMMDic.TryRemove(obj.getKey(), out removeMM);
                    }
                    else if ("1" == obj.Action || "2" == obj.Action)
                    {
                        _CacheMMDic[obj.getKey()] = obj;
                    }
                }
            }
            if (_MMCallbackDic.ContainsKey(obj.getKey()))
            {
                _MMCallbackDic[obj.getKey()]?.Invoke(obj);
            }
           
        }



        /// <summary>
        /// 根据SecurityID注册MM价格
        /// </summary>
        /// <param name="venueSecurityID"></param>
        /// <param name="MMCallBack"></param>
        public void RegisterMM(string venueSecurityID, Action<STC.Common.Model.MMshe.MMshe> MMCallBack)
        {
            lock (venueSecurityID)
            {
                if (MMCallBack != null)
                {
                    if (_MMCallbackDic.ContainsKey(venueSecurityID) == false || _MMCallbackDic[venueSecurityID] == null)
                    {
                        Action<STC.Common.Model.MMshe.MMshe> newAction = null;
                        _MMCallbackDic.AddOrUpdate(venueSecurityID, newAction, (key, value) => newAction);
                        _MMCallbackDic[venueSecurityID] += MMCallBack;
                    }
                    else
                    {
                        int index = Array.IndexOf(_MMCallbackDic[venueSecurityID].GetInvocationList(), (Action<STC.Common.Model.MMshe.MMshe>)MMCallBack);
                        if (index < 0)
                        {
                            _MMCallbackDic[venueSecurityID] += MMCallBack;
                        }
                    }

                    if (_CacheMMDic.ContainsKey(venueSecurityID))
                    {
                        MMCallBack?.Invoke(_CacheMMDic[venueSecurityID]);
                    }
                }
            }
        }

        /// <summary>
        /// 注销MM
        /// </summary>
        /// <param name="key"></param>
        /// <param name="MMCallBack"></param>
        public void UnregisterMM(string key, Action<STC.Common.Model.MMshe.MMshe> MMCallBack)
        {
            lock (key)
            {
                if (_MMCallbackDic.ContainsKey(key))
                {
                    _MMCallbackDic[key] -= MMCallBack;
                }
            }
        }

        public STC.Common.Model.MMshe.MMshe GetMMBySecurityID(string securityID)
        {
            if (_CacheMMDic.ContainsKey(securityID))
            {
               return  _CacheMMDic[securityID];
            }
            return null;
        }

        /// <summary>
        /// 判断是否有打开的策略
        /// </summary>
        /// <returns></returns>
        public bool HasOpen()
        {
            foreach(STC.Common.Model.MMshe.MMshe mm in _CacheMMDic.Values)
            {
                if (mm.enable == "1")
                    return true;
            }
            return false;
        }


        /// <summary>
        /// AnsDataHandler实例
        /// </summary>
        private static MMsheGroupHandler server = new MMsheGroupHandler();
        public static MMsheGroupHandler Instance { get { return server; } }

        /// <summary>
        /// 缓存组内容(订阅)
        /// </summary>
        private IOrderedEnumerable<MMsheGroupInfo> _CacheSortedGroups;
        public IOrderedEnumerable<MMsheGroupInfo> CacheSortedGroups { get { return _CacheSortedGroups; }  }
        /// <summary>
        /// 获取关注组数据
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<MMsheGroupInfo> RequestGroups()
        {
            LoadGroupBonds();
            return _CacheSortedGroups;
        }

        /// <summary>
        /// 异步获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer"></param>
        /// <returns></returns>
        public Task<IOrderedEnumerable<MMsheGroupInfo>> AsyncRequestGroups()
        {
            return Task.Run(() =>
            {
                IOrderedEnumerable<MMsheGroupInfo> orderedInfo = RequestGroups();
                return orderedInfo;
            });
        }

        ///// <summary>
        ///// 缓存券内容(订阅), key : groupid, value : MMsheGroupInfo
        ///// </summary>
        //private static SortedDictionary<string, MMsheGroupInfo> _CacheGroupBonds = new SortedDictionary<string, MMsheGroupInfo>();
        //public static SortedDictionary<string, MMsheGroupInfo> RequestGroupBonds(bool isUpdateFromServer)
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


        private Dictionary<string, string> _FavoriteSecurityDic = new Dictionary<string, string>();
        private void LoadGroupBonds()
        {
            MMsheGroupReplyPo replyPo = MMsheHandler.Instance.mmGroupQuery();

            if (replyPo != null)
            {
                if (!(bool)replyPo.IsSuccess())
                {
                    LogHandler.Warn($"LoadGroupBonds Code:{replyPo.Code} Msg:{replyPo.Msg}");
                }

                if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
                {
                    _CacheSortedGroups = replyPo.Datas.OrderBy(o => o.ordersn);
                }
            }
        }

        public string IsFavorite(string securityID)
        {
            if (_FavoriteSecurityDic != null)
            {
                RequestGroups();
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

        private PResult UpdateGroups(MMsheGroupReq request)
        {
            PResult pResult = MMsheHandler.Instance.mmGroupUpdate(request);
            if (!(bool)pResult.IsSuccess())
            {
                LogHandler.Warn($"UpdateGroups Code:{pResult.Code} Msg:{pResult.Msg}");
            }

            return pResult;
        }

        public Task<PResult> AsyncUpdateGroups(MMsheGroupReq request, bool isLoadsGroup = false)
        {
            return Task.Run(() =>
            {
                PResult pResult = null;
                try
                {
                    pResult = UpdateGroups(request);

                    if (isLoadsGroup)
                    {
                        if (pResult != null && pResult.IsSuccess())
                        {
                            LoadGroupBonds();
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
                return pResult;
            });
        }

        private PResult UpdateSecurityids(MMsheReq request)
        {
            PResult pResult = MMsheHandler.Instance.mmUpdate(request);
            return pResult;
        }

        public Task<PResult> AsyncUpdateSecurityids(MMsheReq request)
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

        public void Notify(FavoriteTypeEnum favoriteTypeEnum, string groupid, string securtityID)
        {
            _OnFavoritesChanged?.Invoke(favoriteTypeEnum, groupid, securtityID);
        }

        #region 记录每页显示类型
        private const string HomePageMode = "HomePageMode";
        private const string Modes = "Modes";

        public ConcurrentDictionary<string, string> CatchHomePageDisplayModeDic = null;

        private string GetCacheKey()
        {
            return string.Format("{0}-{1}", HomePageMode, LocalEnv.UserInfo.UserName);
        }

        public void WriteHomePageDisplayMode()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> pair in CatchHomePageDisplayModeDic)
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

        public void ReadHomePageDisplayMode()
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

        public MMsheGroupInfo IsGroupNameExist(string groupName)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (MMsheGroupInfo item in _CacheSortedGroups)
                {
                    if (item.groupname == groupName)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public MMsheGroupInfo GetGroupInfoByGroupID(string groupId)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (MMsheGroupInfo item in _CacheSortedGroups)
                {
                    if (item.id == groupId)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public Dictionary<string, string> GetAllGroupInfo()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (_CacheSortedGroups != null)
            {
                foreach (MMsheGroupInfo item in _CacheSortedGroups)
                {
                    dic.Add(item.id,item.groupname);
                }
                return dic;
            }
            return null;
        }

        public List<STC.Common.Model.MMshe.MMshe> GetMMsByGroupID(string groupid)
        {
            if (_CacheMMListDic.ContainsKey(groupid))
            {
                return _CacheMMListDic[groupid];

            }
            return null;
        }

        /// <summary>
        /// 获得指定债券信息
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public STC.Common.Model.MMshe.MMshe GetMMBondInfo(string groupid, string securityID)
        {
            if (string.IsNullOrEmpty(groupid) == false)
            {
                lock (string.Intern(groupid))
                {
                    List<STC.Common.Model.MMshe.MMshe> mmList = GetMMsByGroupID(groupid);
                    if (mmList != null)
                    {
                        STC.Common.Model.MMshe.MMshe findItem = mmList.Find(o => o.securityid == securityID);
                        if (findItem != null) return findItem;
                    }
                }
            }
            
            return null;
        }
        /// <summary>
        /// 判断是否有打开的策略
        /// </summary>
        /// <returns></returns>
        public bool HasOpen(string groupid)
        {
            if (_CacheMMListDic.ContainsKey(groupid))
            {
                foreach (STC.Common.Model.MMshe.MMshe mm in _CacheMMListDic[groupid])
                {
                    if (mm.enable == "1")
                        return true;
                }
            }

            return false;
        }

        public bool IsUnify(string key)
        {
            if (!string.IsNullOrEmpty(key) && _CacheMMDic != null && _CacheMMDic.ContainsKey(key))
            {
                string groupid = _CacheMMDic[key].groupid;

                MMsheGroupInfo gi = GetGroupInfoByGroupID(groupid);

                if (gi != null && gi.inf1 == MMsheConstants.MM_GROPE_TYPE_UNIFY)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

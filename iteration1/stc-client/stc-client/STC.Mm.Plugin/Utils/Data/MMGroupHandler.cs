using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Mm.Plugin.Utils.Data
{
    /// <summary>
    /// 主页数据工具类
    /// </summary>
    public class MMGroupHandler
    {
        public enum FavoriteTypeEnum
        {
            AddGroup,
            DelGroup,
            UpdateGroup,
            ChangeColumns,
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
        private Action<STC.Common.Model.MM.MM> _OnMMBonds;
        public event Action<STC.Common.Model.MM.MM> OnMMBonds
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

        private ConcurrentDictionary<string, STC.Common.Model.MM.MM> _CacheMMDic = new ConcurrentDictionary<string, STC.Common.Model.MM.MM>();
        private ConcurrentDictionary<string, Action<STC.Common.Model.MM.MM>> _MMCallbackDic = new ConcurrentDictionary<string, Action<STC.Common.Model.MM.MM>>();

        /// <summary>
        /// 缓存组债券信息
        /// key: groupid; value: List<MM>
        /// </summary>
        private ConcurrentDictionary<string, List<STC.Common.Model.MM.MM>> _CacheMMListDic = new ConcurrentDictionary<string, List<STC.Common.Model.MM.MM>>();

        /// <summary>
        /// 构造函数
        /// </summary>
        private MMGroupHandler()
        {
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
                this.RegSystemReadyEvent();
                ThreadPoolHelper.InvokeThreadPool(o => {
                    MMHandler.Instance.SubscribeMM(Instance_OnMM);
                });
                
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
                string lstkeys = IniFileHelper.ReadValue($"OrderControl-{LocalEnv.UserInfo.UserName}", "Keys");
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
                string lstkeysw = IniFileHelper.ReadValue($"STC.Mm.Plugin-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyListw = lstkeysw.Split(',');
                for (int i = 0; i < keyListw.Length; i++)
                {
                    string winid = keyListw[i];
                    if (!string.IsNullOrEmpty(winid))
                    {
                        string para = IniFileHelper.ReadValue($"STC.Mm.Plugin_{LocalEnv.UserInfo.UserName}_{winid}", "CommandParameter");
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
            LogHandler.Info("MMGroupHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                //_CacheMMListDic.Clear();
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

        private void Instance_OnMM(STC.Common.Model.MM.MM obj)
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
                            _CacheMMListDic[obj.groupid] = new List<STC.Common.Model.MM.MM>();
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

            if (string.IsNullOrEmpty(obj.securityid) == false)
            {
                lock (string.Intern(obj.securityid))
                {
                    if ("3" == obj.Action)
                    {
                        STC.Common.Model.MM.MM removeMM = null;
                        _CacheMMDic.TryRemove(obj.securityid, out removeMM);
                    }
                    else if ("1" == obj.Action || "2" == obj.Action)
                    {
                        _CacheMMDic[obj.securityid] = obj;
                    }
                }
            }
            if (_MMCallbackDic.ContainsKey(obj.securityid))
            {
                _MMCallbackDic[obj.securityid]?.Invoke(obj);
            }
           
        }



        /// <summary>
        /// 根据SecurityID注册MM价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="MMCallBack"></param>
        public void RegisterMM(string securityID, Action<STC.Common.Model.MM.MM> MMCallBack)
        {
            lock (securityID)
            {
                if (MMCallBack != null)
                {
                    if (_MMCallbackDic.ContainsKey(securityID) == false || _MMCallbackDic[securityID] == null)
                    {
                        Action<STC.Common.Model.MM.MM> newAction = null;
                        _MMCallbackDic.AddOrUpdate(securityID, newAction, (key, value) => newAction);
                        _MMCallbackDic[securityID] += MMCallBack;
                    }
                    else
                    {
                        int index = Array.IndexOf(_MMCallbackDic[securityID].GetInvocationList(), (Action<STC.Common.Model.MM.MM>)MMCallBack);
                        if (index < 0)
                        {
                            _MMCallbackDic[securityID] += MMCallBack;
                        }
                    }

                    if (_CacheMMDic.ContainsKey(securityID))
                    {
                        MMCallBack?.Invoke(_CacheMMDic[securityID]);
                    }
                }
            }
        }

        /// <summary>
        /// 注销MM
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="MMCallBack"></param>
        public void UnregisterMM(string securityID, Action<STC.Common.Model.MM.MM> MMCallBack)
        {
            lock (securityID)
            {
                if (_MMCallbackDic.ContainsKey(securityID))
                {
                    _MMCallbackDic[securityID] -= MMCallBack;
                }
            }
        }

        public STC.Common.Model.MM.MM GetMMBySecurityID(string securityID)
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
            foreach(MM mm in _CacheMMDic.Values)
            {
                if (mm.enable == "1")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否有打开的策略
        /// </summary>
        /// <returns></returns>
        public bool HasOpen(string groupid)
        {
            if (_CacheMMListDic.ContainsKey(groupid))
            {
                foreach (MM mm in _CacheMMListDic[groupid])
                {
                    if (mm.enable == "1")
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// AnsDataHandler实例
        /// </summary>
        private static MMGroupHandler server = new MMGroupHandler();
        public static MMGroupHandler Instance { get { return server; } }

        /// <summary>
        /// 缓存组内容(订阅)
        /// </summary>
        private IOrderedEnumerable<MMGroupInfo> _CacheSortedGroups;
        public IOrderedEnumerable<MMGroupInfo> CacheSortedGroups { get { return _CacheSortedGroups; }  }
        /// <summary>
        /// 获取关注组数据
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<MMGroupInfo> RequestGroups()
        {
            LoadGroupBonds();
            return _CacheSortedGroups;
        }

        /// <summary>
        /// 异步获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer"></param>
        /// <returns></returns>
        public Task<IOrderedEnumerable<MMGroupInfo>> AsyncRequestGroups()
        {
            return Task.Run(() =>
            {
                IOrderedEnumerable<MMGroupInfo> orderedInfo = RequestGroups();
                return orderedInfo;
            });
        }

        ///// <summary>
        ///// 缓存券内容(订阅), key : groupid, value : MMGroupInfo
        ///// </summary>
        //private static SortedDictionary<string, MMGroupInfo> _CacheGroupBonds = new SortedDictionary<string, MMGroupInfo>();
        //public static SortedDictionary<string, MMGroupInfo> RequestGroupBonds(bool isUpdateFromServer)
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
            MMGroupReplyPo replyPo = MMHandler.Instance.mmGroupQuery();

            if (replyPo != null)
            {
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

        private PResult UpdateGroups(MMGroupReq request)
        {
            PResult pResult = MMHandler.Instance.mmGroupUpdate(request);
            return pResult;
        }

        public Task<PResult> AsyncUpdateGroups(MMGroupReq request, bool isLoadsGroup = false)
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

        private PResult UpdateSecurityids(MMReq request)
        {
            PResult pResult = MMHandler.Instance.mmUpdate(request);
            return pResult;
        }

        public Task<PResult> AsyncUpdateSecurityids(MMReq request)
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

        public MMGroupInfo IsGroupNameExist(string groupName)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (MMGroupInfo item in _CacheSortedGroups)
                {
                    if (item.groupname == groupName)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public MMGroupInfo GetGroupInfoByGroupID(string groupId)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (MMGroupInfo item in _CacheSortedGroups)
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
                foreach (MMGroupInfo item in _CacheSortedGroups)
                {
                    dic.Add(item.id,item.groupname);
                }
                return dic;
            }
            return null;
        }

        public List<STC.Common.Model.MM.MM> GetMMsByGroupID(string groupid)
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
        public STC.Common.Model.MM.MM GetMMBondInfo(string groupid, string securityID)
        {
            if (string.IsNullOrEmpty(groupid) == false)
            {
                lock (string.Intern(groupid))
                {
                    List<STC.Common.Model.MM.MM> mmList = GetMMsByGroupID(groupid);
                    if (mmList != null)
                    {
                        STC.Common.Model.MM.MM findItem = mmList.Find(o => o.securityid == securityID);
                        if (findItem != null) return findItem;
                    }
                }
            }
            
            return null;
        }

        public bool IsUnify(string securityid)
        {
            if (!string.IsNullOrEmpty(securityid) && _CacheMMDic != null && _CacheMMDic.ContainsKey(securityid))
            {
                string groupid = _CacheMMDic[securityid].groupid;

                MMGroupInfo gi = GetGroupInfoByGroupID(groupid);

                if (gi != null && gi.inf1 == MMConstants.MM_GROPE_TYPE_UNIFY)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

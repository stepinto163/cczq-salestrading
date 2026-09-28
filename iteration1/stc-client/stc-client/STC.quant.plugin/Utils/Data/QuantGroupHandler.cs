using netapi.Api;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Quant;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Quant;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.quant.plugin.Command;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.quant.plugin.Utils.Data
{
    /// <summary>
    /// 主页数据工具类
    /// </summary>
    public class QuantGroupHandler
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

        private readonly Object m_OnMMBondsEventLock = new Object();
        private Action<TTbookQuantStrategys> _OnMMBonds;
        public event Action<TTbookQuantStrategys> OnMMBonds
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

        /// <summary>
        /// 缓存组债券信息
        /// key: groupid; value: List<MM>
        /// </summary>
        private ConcurrentDictionary<string, List<TTbookQuantStrategys>> _CacheMMListDic = new ConcurrentDictionary<string, List<TTbookQuantStrategys>>();

        //缓存所有策略
        private ConcurrentDictionary<string, TTbookQuantStrategys> _CacheQuantIDic = new ConcurrentDictionary<string, TTbookQuantStrategys>();
        //缓存所有策略标签
        private ConcurrentDictionary<string, TTbookQuantStrategys> _CacheLableIDic = new ConcurrentDictionary<string, TTbookQuantStrategys>();
        //已订阅的策略
        private ConcurrentDictionary<string, TTbookQuantStrategys> _SubQuantIDic = new ConcurrentDictionary<string, TTbookQuantStrategys>();

        /// <summary>
        /// 构造函数
        /// </summary>
        private QuantGroupHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private volatile bool isInit = false;

        public const string _ModelId = "4007";
        public bool ManageYourselfViewAll = false;
        public string user = ApiManager.Instance.GetDefaultApi().UserName;
        public Dictionary<string, ObservableCollection<TBookBookTreeItemModel>> dicBookUser = new Dictionary<string, ObservableCollection<TBookBookTreeItemModel>>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            dicBookUser = AdminHandler.Instance.GetDicTbookAllUser();
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                if (isInit == false)
                {
                    //InitInQuant();
                    List<ModulesAction> Action = AdminHandler.Instance.GetModulesActionById(_ModelId);
                    if (Action != null && Action.Count > 0)
                    {
                        foreach (ModulesAction m in Action)
                        {
                            if ("400702".Equals(m.id))//管理自己&查看全部
                            {
                                user = "*";
                                ManageYourselfViewAll = true;
                            }
                            else if ("400703".Equals(m.id))//管理全部
                            {
                                user = "*";
                            }
                            else
                            {//400701管理自己
                            }
                            break;
                        }
                        QuantHandler.Instance.SubscribeMM(Instance_OnMM, user);
                        QuantHandler.Instance.SubscribeParas(Instance_OnParas);
                        QuantHandler.Instance.SubscribeQuantLog(Instance_OnQuantLog);
                        QuantHandler.Instance.SubscribeIndicator(Instance_OnQuantIndicator);



                        //InitIndicator();
                        isInit = true;
                        ObservableCollection<TBookBookTreeItemModel> li = AdminHandler.Instance.GetTCurrentBookCollection();
                        if (li != null && li.Count > 0)
                        {

                        }
                    }
                }
            });
        }

        public void RegSystemReadyEvent()
        {
            DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
        }

        private void Instance_OnSystemReady(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.init();
                PopupWindowCommand popupWindowCommand = new PopupWindowCommand();
                //加载打开的窗口
                string lstkeysw = IniFileHelper.ReadValue($"STC.quant.plugin-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyListw = lstkeysw.Split(',');
                for (int i = 0; i < keyListw.Length; i++)
                {
                    string winid = keyListw[i];
                    if (!string.IsNullOrEmpty(winid))
                    {
                        string para = IniFileHelper.ReadValue($"STC.quant.plugin_{LocalEnv.UserInfo.UserName}_{winid}", "CommandParameter");
                        if (!string.IsNullOrEmpty(para))
                        {
                            PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), para);
                            List<object> pobject = new List<object>();
                            pobject.Add(pye);
                            popupWindowCommand.Execute(pye);
                        }
                    }
                }

                DesignHandler.Instance.OnSystemReady -= Instance_OnSystemReady;
            }));
        }
        string ReqQuantid = "";
        bool isFirst = false;
        private void Instance_OnMM(List<TTbookQuantStrategys> list)
        {
            if (!isFirst)
            {
                //订阅到所有策略的镜像
                foreach (TTbookQuantStrategys obj in list)
                {
                    Instance_OnMM(obj);
                    //if ("1" == obj.Action || "2" == obj.Action)
                    //    ReqQuantid += obj.Quantid + ",";
                    //else if ("3" == obj.Action)
                    //    ReqQuantid = ReqQuantid.Replace(obj.Quantid + ",", "");
                    if (!_SubQuantIDic.ContainsKey(obj.Quantid))
                    {
                        _SubQuantIDic.AddOrUpdate(obj.Quantid, obj, (key, value) => obj);
                        ReqQuantid += obj.Quantid + ",";
                        QuantOrdersHandler.Instance.SubscribeQuantId(obj);
                    }
                }
                //查询所有策略订单
                QuantOrdersHandler.Instance.init(ReqQuantid);
            }
            else
            {//实时消息
                TTbookQuantStrategys obj = list[0];
                Instance_OnMM(obj);
                if (!_SubQuantIDic.ContainsKey(obj.Quantid))
                {
                    _SubQuantIDic.AddOrUpdate(obj.Quantid, obj, (key, value) => obj);
                    QuantOrdersHandler.Instance.SubscribeQuantId(obj);
                }
            }
            isFirst = true;
        }
    

        private void Instance_OnMM(TTbookQuantStrategys obj)
        {
            obj.Groupid = QuantCommonUtils.groupID;//默认都在一个组下
            TTbookQuantStrategys result;
            if ("3" == obj.Action)
            {
                if (_CacheMMListDic.ContainsKey(obj.Groupid) == true)
                {
                    int findIndex = _CacheMMListDic[obj.Groupid].FindIndex(mm => mm.Id == obj.Id);
                    if (findIndex > -1)
                    {
                        _CacheMMListDic[obj.Groupid].RemoveAt(findIndex);
                        _CacheQuantIDic.TryRemove(obj.Quantid, out result);
                        if (_CacheLableIDic.ContainsKey(obj.Quantid))
                            _CacheLableIDic.TryRemove(obj.Quantid, out result);
                        _OnMMBonds?.Invoke(obj);
                    }
                }
            }
            else if ("1" == obj.Action || "2" == obj.Action)
            {
                if (_CacheMMListDic.ContainsKey(obj.Groupid) == false)
                {
                    _CacheMMListDic[obj.Groupid] = new List<TTbookQuantStrategys>();
                }

                int findIndex = _CacheMMListDic[obj.Groupid].FindIndex(mm => mm.Id == obj.Id);
                if (findIndex > -1)
                {
                    _CacheMMListDic[obj.Groupid].RemoveAt(findIndex);
                    _CacheMMListDic[obj.Groupid].Insert(findIndex, obj);
                }
                else
                {
                    _CacheMMListDic[obj.Groupid].Insert(0, obj);
                }
                _CacheQuantIDic.AddOrUpdate(obj.Quantid, obj, (key, value) => obj);
                if (obj.Lable != null)
                    _CacheLableIDic.AddOrUpdate(obj.Quantid, obj, (key, value) => obj);
                _OnMMBonds?.Invoke(obj);
            }
        }

        //public void InitInQuant()
        //{
        //    TTbookQuantStrategysReplyPo reply = QuantHandler.Instance.QuantAllStrategys();
        //    if (reply != null)
        //    {
        //        if (reply != null && reply.Code == "0000" && reply.Datas != null)
        //        {
        //            foreach (TTbookQuantStrategys templet in reply.Datas)
        //            {
        //                Instance_OnMM(templet);
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// QuantGroupHandler
        /// </summary>
        private static QuantGroupHandler server = new QuantGroupHandler();
        public static QuantGroupHandler Instance { get { return server; } }

        /// <summary>
        /// 缓存组内容(订阅)
        /// </summary>
        private IOrderedEnumerable<TTbookQuantGroups> _CacheSortedGroups;
        public IOrderedEnumerable<TTbookQuantGroups> CacheSortedGroups { get { return _CacheSortedGroups; } }
        /// <summary>
        /// 获取关注组数据
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<TTbookQuantGroups> RequestGroups()
        {
            LoadGroupBonds();
            return _CacheSortedGroups;
        }

        /// <summary>
        /// 异步获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer"></param>
        /// <returns></returns>
        public Task<IOrderedEnumerable<TTbookQuantGroups>> AsyncRequestGroups()
        {
            return Task.Run(() =>
            {
                IOrderedEnumerable<TTbookQuantGroups> orderedInfo = RequestGroups();
                return orderedInfo;
            });
        }


        private Dictionary<string, string> _FavoriteSecurityDic = new Dictionary<string, string>();
        private void LoadGroupBonds()
        {
            TbookQuantGroupsReplyPo replyPo = QuantHandler.Instance.quantGroupQuery();

            if (replyPo != null)
            {
                if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
                {
                    _CacheSortedGroups = replyPo.Datas.OrderBy(o => o.Ordersn);
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

        private PResult UpdateGroups(QuantGroupReq request)
        {
            PResult pResult = QuantHandler.Instance.quantGroupUpdate(request);
            return pResult;
        }

        public Task<PResult> AsyncUpdateGroups(QuantGroupReq request)
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

        private PResult UpdateQuant(QuantStrategyReq request)
        {
            PResult pResult = QuantHandler.Instance.quantStrategyUpdate(request);
            return pResult;
        }


        public Task<PResult> AsyncUpdateQuants(QuantStrategyReq request)
        {
            return Task.Run(() =>
            {
                PResult pResult = null;
                try
                {
                    pResult = UpdateQuant(request);
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

        public TTbookQuantGroups IsGroupNameExist(string groupName)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (TTbookQuantGroups item in _CacheSortedGroups)
                {
                    if (item.Groupname == groupName)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public TTbookQuantGroups GetGroupInfoByGroupID(string groupId)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (TTbookQuantGroups item in _CacheSortedGroups)
                {
                    if (item.Id == groupId)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public List<TTbookQuantStrategys> GetMMsByGroupID(string groupid)
        {
            if (string.IsNullOrEmpty(groupid))
            {
                List<TTbookQuantStrategys> list = new List<TTbookQuantStrategys>();
                foreach (KeyValuePair<string, List<TTbookQuantStrategys>> kv in _CacheMMListDic)
                {
                    list.AddRange(kv.Value);
                }
                return list;
            }
            else
            {
                if (_CacheMMListDic.ContainsKey(groupid))
                {
                    var li = from d in _CacheMMListDic[groupid] orderby d.Updatetime ascending select d;
                    List<TTbookQuantStrategys> list = li.ToList();
                    return list;
                }
            }
            return null;
        }


        /// <summary>
        /// 获得指定策略信息
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="Quantid"></param>
        /// <returns></returns>
        public TTbookQuantStrategys GetMMBondInfo(string groupid, string Quantid)
        {
            List<TTbookQuantStrategys> mmList = GetMMsByGroupID(groupid);
            if (mmList != null)
            {
                TTbookQuantStrategys findItem = mmList.Find(o => o.Quantid == Quantid);
                if (findItem != null) return findItem;
            }
            return null;
        }

        public TTbookQuantStrategys GetQuantByQuantID(string quantid)
        {
            if (_CacheQuantIDic.ContainsKey(quantid))
            {
                return _CacheQuantIDic[quantid];

            }
            return null;
        }

        public List<TTbookQuantStrategys> GetAllLableByQuantID()
        {
            return _CacheLableIDic.Values.ToList();
        }

        

        #region 参数
        private ConcurrentDictionary<string, List<TTbookQuantParameter>> _CacheParasListDic = new ConcurrentDictionary<string, List<TTbookQuantParameter>>();
        private readonly Object m_OnParasEventLock = new Object();
        private Action<TTbookQuantParameter> _OnParas;
        public event Action<TTbookQuantParameter> OnParas
        {
            add
            {
                lock (m_OnParasEventLock)
                {
                    _OnParas += value;
                }
            }
            remove
            {
                lock (m_OnParasEventLock)
                {
                    _OnParas -= value;
                }
            }

        }
        private void Instance_OnParas(TTbookQuantParameter obj)
        {
            if ("3" == obj.Action)
            {
                if (_CacheParasListDic.ContainsKey(obj.Quantid) == true)
                {
                    int findIndex = _CacheParasListDic[obj.Quantid].FindIndex(mm => mm.Id == obj.Id);
                    if (findIndex > -1)
                    {
                        _CacheParasListDic[obj.Quantid].RemoveAt(findIndex);
                        _OnParas?.Invoke(obj);
                    }
                }
            }
            else if ("1" == obj.Action || "2" == obj.Action)
            {
                if (_CacheParasListDic.ContainsKey(obj.Quantid) == false)
                {
                    _CacheParasListDic[obj.Quantid] = new List<TTbookQuantParameter>();
                }

                int findIndex = _CacheParasListDic[obj.Quantid].FindIndex(mm => mm.Id == obj.Id);
                if (findIndex > -1)
                {
                    _CacheParasListDic[obj.Quantid].RemoveAt(findIndex);
                    _CacheParasListDic[obj.Quantid].Insert(findIndex, obj);
                }
                else
                {
                    _CacheParasListDic[obj.Quantid].Add(obj);//.Insert(0, obj);
                }
                _OnParas?.Invoke(obj);

            }
        }

        public List<TTbookQuantParameter> GetParasByQuantID(string quantid)
        {
            if (_CacheParasListDic.ContainsKey(quantid))
            {
                return _CacheParasListDic[quantid];

            }
            return null;
        }

        public TTbookQuantParameter GetParasByQuantIDAndID(string quantid, string id)
        {
            TTbookQuantParameter tParameter = new TTbookQuantParameter();
            if (_CacheParasListDic.ContainsKey(quantid))
            {
                List<TTbookQuantParameter> list = _CacheParasListDic[quantid];
                var li = from d in _CacheParasListDic[quantid] where d.Id == id select d;
                List<TTbookQuantParameter> t = li.ToList();

                if (t != null && t.Count > 0)
                {
                    tParameter = t[0];
                    return tParameter;
                }
            }
            return null;
        }
        
        #endregion

        #region 策略日志
        //private ConcurrentDictionary<string, List<TTbookQuantLog>> _CacheQuanLogListDic = new ConcurrentDictionary<string, List<TTbookQuantLog>>();
        private readonly Object m_OnQuantLogEventLock = new Object();
        private Action<TTbookQuantLog> _OnQuantLog;
        public event Action<TTbookQuantLog> QuantLog
        {
            add
            {
                lock (m_OnQuantLogEventLock)
                {
                    _OnQuantLog += value;
                }
            }
            remove
            {
                lock (m_OnQuantLogEventLock)
                {
                    _OnQuantLog -= value;
                }
            }

        }
        private void Instance_OnQuantLog(TTbookQuantLog obj)
        {
            //if (_CacheQuanLogListDic.ContainsKey(obj.Quantid) == false)
            //{
            //    _CacheQuanLogListDic[obj.Quantid] = new List<TTbookQuantLog>();
            //}

            //int findIndex = _CacheQuanLogListDic[obj.Quantid].FindIndex(mm => mm.Id == obj.Id);
            //if (findIndex > -1)
            //{
            //    _CacheQuanLogListDic[obj.Quantid].RemoveAt(findIndex);
            //    //_CacheQuanLogListDic[obj.Quantid].Insert(findIndex, obj);
            //    _CacheQuanLogListDic[obj.Quantid].Add(obj);
            //}
            //else
            //{
            //    _CacheQuanLogListDic[obj.Quantid].Add(obj);
            //}
            _OnQuantLog?.Invoke(obj);
        }

        public int LogPageSize = 300;
        public List<TTbookQuantLog> InitLog(string quantid, string id)
        {
            List<TTbookQuantLog> list = new List<TTbookQuantLog>();
            QuantLogSelReq req = new QuantLogSelReq();
            req.pageNum = 0;
            req.pageSize = LogPageSize;//以防界面卡顿//5000;
            req.quantid = quantid;
            req.id = id;
            TbookQuantLogReplyPo reply = QuantHandler.Instance.QuantLogQuery(req);
            if (reply != null)
            {
                if (reply != null && reply.Code == "0000" && reply.Datas != null)
                {
                    //for (int i = reply.Datas.Count - 1; i >= 0; i--)
                    //{
                    //    Instance_OnQuantLog(reply.Datas[i]);
                    //}
                    return reply.Datas.OrderBy(t => t.Createtime).ToList();
                }
            }
            return list.OrderBy(t => t.Createtime).ToList();
        }


        //public List<TTbookQuantLog> GetQuantLogByQuantID(string quantid)
        //{
        //    if (_CacheQuanLogListDic.ContainsKey(quantid))
        //    {
        //        return _CacheQuanLogListDic[quantid];

        //    }
        //    return null;
        //}
        #endregion


        #region 策略指标
        private readonly Object m_OnIndicatorEventLock = new Object();
        private Action<TbookQuantIndicator> _OnIndicator;
        public event Action<TbookQuantIndicator> OnIndicator
        {
            add
            {
                lock (m_OnIndicatorEventLock)
                {
                    _OnIndicator += value;
                }
            }
            remove
            {
                lock (m_OnIndicatorEventLock)
                {
                    _OnIndicator -= value;
                }
            }

        }
        private ConcurrentDictionary<string, List<TbookQuantIndicator>> _CacheQuanIndicatorListDic = new ConcurrentDictionary<string, List<TbookQuantIndicator>>();
       /* public void InitIndicator()
        {
            TbookQuantIndicatorReq req = new TbookQuantIndicatorReq();
            List<string> idList = new List<string>();
            if (_CacheQuantIDic != null && _CacheQuantIDic.Count > 0)
            {
                idList.AddRange(_CacheQuantIDic.Keys.ToList());
            }
            req.quantid = idList;
            TbookQuantIndicatorReplyPo reply = QuantHandler.Instance.QueryIndicator(req);
            if (reply != null)
            {
                if (reply != null && reply.Code == "0000" && reply.Datas != null)
                {
                    foreach (TbookQuantIndicator templet in reply.Datas)
                    {
                        Instance_OnQuantIndicator(templet);
                    }
                }
            }
        }*/

        private void Instance_OnQuantIndicator(TbookQuantIndicator obj)
        {
            if (_CacheQuanIndicatorListDic.ContainsKey(obj.Quantid) == false)
            {
                _CacheQuanIndicatorListDic[obj.Quantid] = new List<TbookQuantIndicator>();
            }

            int findIndex = _CacheQuanIndicatorListDic[obj.Quantid].FindIndex(mm => mm.Id == obj.Id);
            if (findIndex > -1)
            {
                _CacheQuanIndicatorListDic[obj.Quantid].RemoveAt(findIndex);
                _CacheQuanIndicatorListDic[obj.Quantid].Insert(findIndex, obj);
            }
            else
            {
                _CacheQuanIndicatorListDic[obj.Quantid].Insert(0, obj);
            }
            _OnIndicator?.Invoke(obj);
        }

        public List<TbookQuantIndicator> GetCacheQuanIndicatorList(string quantid)
        {
            if (_CacheQuanIndicatorListDic.ContainsKey(quantid))
            {
                return _CacheQuanIndicatorListDic[quantid];

            }
            return null;
        }
        #endregion

        public string IsCheckOwn(string Username)
        {
            //如果权限是：管理自己&查看全部，如果不是自己的策略，就不能操作
            string isOwn = "0";
            if (ManageYourselfViewAll && !Username.Equals(ApiManager.Instance.GetDefaultApi().UserName))
            {
                isOwn = "1";
            }
            return isOwn;
        }


        /// <summary>
        /// 清理回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("QuantOrdersHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                QuantOrdersHandler.Instance.Instance_OnClear(obj,ReqQuantid);
            }
        }

    }
}

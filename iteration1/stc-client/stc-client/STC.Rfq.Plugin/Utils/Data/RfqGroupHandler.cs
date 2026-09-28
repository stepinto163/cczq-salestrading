using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.RFQ;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.RFQ;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.Utils;
using STC.Rfq.Plugin.Command;
using STC.Rfq.Plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Rfq.Plugin.Utils.Data
{
    /// <summary>
    /// 主页数据工具类
    /// </summary>
    public class RfqGroupHandler
    {

        /// <summary>
        /// 失效时间
        /// </summary>
        public Dictionary<string, string> BaseValidUntilTime = new Dictionary<string, string>();
        public Dictionary<string, string> StrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> BaseSettleTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> AlgoMidTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> ValidSideList = new Dictionary<string, string>();


        public Dictionary<string, string> ReverseBaseValidUntilTime = new Dictionary<string, string>();
        public Dictionary<string, string> ReverseStrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> ReverseBaseSettleTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> ReverseAlgoMidTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> ReverseValidSideList = new Dictionary<string, string>();
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

        /// <summary>
        /// 所有策略列表
        /// </summary>
        private ConcurrentDictionary<string, Action<RfqstgViewModel>> _MMCallbackDic = new ConcurrentDictionary<string, Action<RfqstgViewModel>>();

        /// <summary>
        /// 缓存组债券信息
        /// key: groupid; value: List<MM>
        /// </summary>
        public ConcurrentDictionary<string, List<RfqstgViewModel>> _CacheMMListDic = new ConcurrentDictionary<string, List<RfqstgViewModel>>();


        /// <summary>
        /// 构造函数
        /// </summary>
        private RfqGroupHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
            InitDict();
        }

        private volatile bool isInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            if (isInit == false)
            {
                RfqHandler.Instance.SubscribeRfq(Instance_OnRfq);

                //分组策略查询
                RfqstgViewReq req = new RfqstgViewReq();
                req.username = LocalEnv.UserInfo.UserName;
                GetData(req);
                isInit = true;
            }
        }

        public void Clear()
        {
            RfqHandler.Instance.UnSubscribeRfq(Instance_OnRfq);
            _CacheMMListDic.Clear();
            isInit = false;
        }

        private void InitDict()
        {
            AlgoMidTypeList.Clear();
            AlgoMidTypeList.Add("1", "全市场中间价");
            AlgoMidTypeList.Add("2", "中债估值");
            AlgoMidTypeList.Add("3", "自定义%");

            ValidSideList.Clear();
            ValidSideList.Add("All", "不限");
            ValidSideList.Add("1", "Bid");
            ValidSideList.Add("2", "Ofr");

            BaseValidUntilTime.Clear();
            BaseValidUntilTime.Add("", "不限");
            BaseValidUntilTime.Add("5", "5分钟");
            BaseValidUntilTime.Add("15", "15分钟");
            BaseValidUntilTime.Add("30", "30分钟");
            BaseValidUntilTime.Add("45", "45分钟");
            BaseValidUntilTime.Add("60", "1小时");
            BaseValidUntilTime.Add("120", "2小时");

            StrategyModeList.Clear();
            StrategyModeList.Add("Manual", "手动");
            StrategyModeList.Add("Defend", "防守");
            StrategyModeList.Add("Algorithm", "算法");
            StrategyModeList.Add("API", "接口");

            BaseSettleTypeList.Clear();
            ReverseBaseSettleTypeList.Clear();

            List<KeyValuePair<string, string>> settlerfq = ComboSourceUtil.Instance.GetComboSource("settletype_rfq");
            foreach (var r in settlerfq)
            {
                BaseSettleTypeList.Add(r.Key, r.Value);
                ReverseBaseSettleTypeList.Add(r.Value, r.Key);
            }

            ReverseAlgoMidTypeList.Clear();
            ReverseAlgoMidTypeList.Add("全市场中间价","1");
            ReverseAlgoMidTypeList.Add("中债估值", "2");
            ReverseAlgoMidTypeList.Add("自定义%", "3");

            ReverseValidSideList.Clear();
            ReverseValidSideList.Add("不限","All");
            ReverseValidSideList.Add("Bid", "1");
            ReverseValidSideList.Add("Ofr", "2");

            ReverseBaseValidUntilTime.Clear();
            ReverseBaseValidUntilTime.Add("不限", "");
            ReverseBaseValidUntilTime.Add("5分钟", "5");
            ReverseBaseValidUntilTime.Add("15分钟", "15");
            ReverseBaseValidUntilTime.Add("30分钟", "30");
            ReverseBaseValidUntilTime.Add("45分钟", "45");
            ReverseBaseValidUntilTime.Add("1小时", "60");
            ReverseBaseValidUntilTime.Add("2小时", "120");

            ReverseStrategyModeList.Clear();
            ReverseStrategyModeList.Add("手动", "Manual");
            ReverseStrategyModeList.Add("防守", "Defend");
            ReverseStrategyModeList.Add("算法", "Algorithm");
            ReverseStrategyModeList.Add("接口","API");
        }


        public async void GetData(RfqstgViewReq req)
        {
            RfqstgViewResult reply = null;
            await Task.Run(() =>
            {
                reply = RfqHandler.Instance.RfqStgViewQuery(req);

                if (reply != null && reply.Code == "0000")
                {
                    foreach (RfqstgViewModel rm in reply.Datas)
                    {
                        Instance_OnRfq(rm);
                    }
                }
                else
                {
                    if (reply != null)
                    {
                        com.gateway.log.LogHelper.Error($"RfqQueryOrder exception.{reply.Msg}");
                    }
                }
            });
        }

        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("RfqGroupHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                this.Clear();

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


        public void Instance_OnRfq(RfqstgViewModel obj)
        {
            if (string.IsNullOrEmpty(obj.groupid) == false)
            {
                lock (string.Intern(obj.groupid))
                {
                    if ("3" == obj.action)
                    {
                        if (_CacheMMListDic.ContainsKey(obj.groupid) == true)
                        {
                            int findIndex = _CacheMMListDic[obj.groupid].FindIndex(mm => mm.id == obj.id);
                            if (findIndex > -1)
                            {
                                _CacheMMListDic[obj.groupid].RemoveAt(findIndex);
                            }
                        }
                    }
                    else if ("1" == obj.action || "0" == obj.action || null == obj.action || "2" == obj.action || "5" == obj.action || "6" == obj.action)
                    {
                        if (_CacheMMListDic.ContainsKey(obj.groupid) == false)
                        {
                            _CacheMMListDic[obj.groupid] = new List<RfqstgViewModel>();
                        }

                        int findIndex = _CacheMMListDic[obj.groupid].FindIndex(mm => mm.id == obj.id);
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
        }


        /// <summary>
        /// 停止所有策略
        /// </summary>
        public async void StopAllStg()
        {
            await Task.Run(() =>
            {
                List<RfqstgViewModel> allstg = new List<RfqstgViewModel>();
                foreach (List<RfqstgViewModel> rx in _CacheMMListDic.Values)
                {
                    allstg.AddRange(rx);
                }

                List<string> lststopstg = new List<string>();
                foreach (RfqstgViewModel ss in allstg)
                {
                    if (ss.enable == "1")
                    {
                        lststopstg.Add(ss.id);
                    }
                }

                if (lststopstg.Count > 0)
                {
                    RfqUpdateEnableReq req = new RfqUpdateEnableReq();
                    UpdateEnableModel strategy = new UpdateEnableModel();
                    strategy.ip = EngineHelper.GetIp();
                    strategy.terminal = LocalEnv.ServerInfo.Version; // 终端
                    strategy.username = LocalEnv.UserInfo.UserName; //用户名
                    strategy.closeby = LocalEnv.UserInfo.UserName; //用户名
                    strategy.macid = EngineHelper.GetMacByNetworkInterface();
                    strategy.harddiskid = EngineHelper.Getharddiskid2();
                    req.action = 6; //5:启动,6:停止
                    req.ids = lststopstg;

                    PResult result = null;

                    result = RfqHandler.Instance.RfqUpdateEnable(req);

                    string promptTitle;

                    if ("1" == strategy.enable)
                    {
                        promptTitle = $"债券策略[{strategy.securityid}]报价开启";
                    }
                    else
                    {
                        promptTitle = $"债券策略[{strategy.securityid}]报价关闭";
                    }

                    if (result == null)
                    {
                        Prompt2Second(promptTitle, "失败");
                    }
                    else
                    {
                        if (result != null && !result.IsSuccess())
                        {
                            Prompt2Second(promptTitle, result.Msg);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// AnsDataHandler实例
        /// </summary>
        private static RfqGroupHandler server = new RfqGroupHandler();
        public static RfqGroupHandler Instance { get { return server; } }

        /// <summary>
        /// 缓存组内容(订阅)
        /// </summary>
        private IOrderedEnumerable<RfqGroupInfo> _CacheSortedGroups;
        public IOrderedEnumerable<RfqGroupInfo> CacheSortedGroups { get { return _CacheSortedGroups; }  }
        /// <summary>
        /// 获取关注组数据
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<RfqGroupInfo> RequestGroups()
        {
            LoadGroupBonds();
            return _CacheSortedGroups;
        }

        /// <summary>
        /// 异步获取关注组数据
        /// </summary>
        /// <param name="isUpdateFromServer"></param>
        /// <returns></returns>
        public Task<IOrderedEnumerable<RfqGroupInfo>> AsyncRequestGroups()
        {
            return Task.Run(() =>
            {
                IOrderedEnumerable<RfqGroupInfo> orderedInfo = RequestGroups();
                return orderedInfo;
            });
        }

        private Dictionary<string, string> _FavoriteSecurityDic = new Dictionary<string, string>();
        private void LoadGroupBonds()
        {
            RfqGroupReplyPo replyPo = RfqHandler.Instance.RfqGroupQuery();

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

        private PResult UpdateGroups(RfqGroupReq request)
        {
            PResult pResult = RfqHandler.Instance.RfqGroupUpdate(request);
            return pResult;
        }

        public Task<PResult> AsyncUpdateGroups(RfqGroupReq request)
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

        private PResult UpdateSecurityids(RfqstgActReq request)
        {
            PResult pResult = RfqHandler.Instance.RfqUpdatestgAct(request);
            return pResult;
        }

        public Task<PResult> AsyncUpdateSecurityids(RfqstgActReq request)
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

        public RfqGroupInfo IsGroupNameExist(string groupName)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (RfqGroupInfo item in _CacheSortedGroups)
                {
                    if (item.groupname == groupName)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public RfqGroupInfo GetGroupInfoByGroupID(string groupId)
        {
            if (_CacheSortedGroups != null)
            {
                foreach (RfqGroupInfo item in _CacheSortedGroups)
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
                foreach (RfqGroupInfo item in _CacheSortedGroups)
                {
                    dic.Add(item.id,item.groupname);
                }
                return dic;
            }
            return null;
        }

        /// <summary>
        /// 查询当前组的策略列表
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public List<RfqstgViewModel> GetMMsByGroupID(string groupid)
        {
            if (_CacheMMListDic.ContainsKey(groupid))
            {
                return _CacheMMListDic[groupid];

            }
            return null;
        }


        #region  UI 
        private void Prompt2Second(string title, string text)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Sub_Prompt2Second(title, text);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Sub_Prompt2Second(title, text);
                }));
            }
        }

        private void Sub_Prompt2Second(string title, string text)
        {
            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.PromptTitle = title;
            prompt.PromptContext = text;
            prompt.Owner = Application.Current.MainWindow;
            prompt.Show();
        }
        #endregion

    }
}

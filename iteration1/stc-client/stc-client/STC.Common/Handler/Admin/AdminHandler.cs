using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Admin
{
    /// <summary>
    /// AdminSvr接口类
    /// </summary>
    public class AdminHandler : IAdminHandler
    {
        private static AdminHandler adminHandler = new AdminHandler();
        private bool isSubscribeForAdmin = false;

        private event Action<List<TTbookSystemMessage>> OnAdminMsg;
        private STCUserReplyPo _currentStcUser;
        private AccountAndUsersQueryReplyPo _currentAccountUser;

        public static IAdminHandler Instance { get { return adminHandler; } }

        public static Dictionary<string, HomeGroupInfo> dicGroup = new Dictionary<string, HomeGroupInfo>();

        private const string _SecurityGroupModelId = "4027";
        public string user = ApiManager.Instance.GetDefaultApi().UserName;
        public bool ManageYourselfViewAll = false;


        private AdminHandler()
        {

        }

        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnAdminMsg != null)
                OnAdminMsg = null;

            isSubscribeForAdmin = isSubscribe;
        }

        public void UnSubscribeOrder(Action<List<TTbookSystemMessage>> onMsgCallBack)
        {
            if (onMsgCallBack != null && OnAdminMsg != null)
                OnAdminMsg -= onMsgCallBack;
        }

        public STCUserReplyPo stcUserAndGroupsQuery()
        {
            _currentStcUser = ApiManager.Instance.GetDefaultApi().Request<STCUserReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_userAndGroupsQuery, null);//ClientApi.Instance.stcUserAndGroupsQuery();

            return _currentStcUser;
        }

        public List<StcTbookUsers> GetStcTbookUsers()
        {
            return _currentStcUser != null && _currentStcUser.Datas != null ? _currentStcUser.Datas.Where(delegate (StcTbookUsers stcTbookUsers)
            {
                return string.IsNullOrEmpty(stcTbookUsers.IsUsers);

            }).ToList() : null;
        }

        public List<StcTbookUsers> GetStcTbookUsersByUser(string user)
        {
            List<StcTbookUsers> result = (_currentStcUser != null && _currentStcUser.Datas != null) ? _currentStcUser.Datas.Where(delegate (StcTbookUsers stcTbookUsers)
            {
                if (!string.IsNullOrEmpty(stcTbookUsers.IsUsers))
                {
                    string[] arr = stcTbookUsers.Userid.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    return arr != null && arr.Length > 0 && arr.Contains(user);
                }
                else
                    return string.IsNullOrEmpty(stcTbookUsers.IsUsers) && stcTbookUsers.Userid.Equals(user);

            }).ToList() : null;

            if (result != null)
            {
                foreach (StcTbookUsers bookUser in result)
                {
                    if (!string.IsNullOrEmpty(bookUser.IsUsers))
                    {
                        string name = bookUser.Name;
                        string username = bookUser.Username;

                        bookUser.Username = name;
                        bookUser.Name = username;
                    }
                }
            }


            return result;
        }

        public TQueryMessage SearchSysMsg(SystemMessageReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TQueryMessage>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_admin_search, request);//.searchSystemMessageForAdmin(request);
        }

        public TQueryMessage SearchSysMsgmvt(SystemMessageReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TQueryMessage>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_admin_searchmvt, request);//ClientApi.Instance.searchSystemMessageForAdminmvt(request);
        }

        public void UpdateSysMsg(SystemMessageReq request)
        {
            ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_admin_update, request);//ClientApi.Instance.updateSystemMessageForAdmin(request);
        }

        public PResult BatchUpdateSysMsg(TBatchMsgReq batchMsgReq)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_admin_update, batchMsgReq); //ClientApi.Instance.batchUpdateSystemMessageForAdmin(batchMsgReq);
        }

        /// <summary>
        /// 订阅系统消息数据
        /// </summary>
        /// <param name="onAdminMsg"></param>
        public void Init(Action<List<TTbookSystemMessage>> onAdminMsg)
        {
            if (onAdminMsg != null && (OnAdminMsg == null || !OnAdminMsg.GetInvocationList().Contains(onAdminMsg)))
                OnAdminMsg += onAdminMsg;

            lock (this)
            {
                if (!isSubscribeForAdmin)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                        new string[] { string.Format("{0}{1}", ApiConsts.Topic_stc_admin_prefix, ApiManager.Instance.GetDefaultApi().UserName), string.Format("{0}all", ApiConsts.Topic_stc_admin_prefix) },
                        ApiConsts.AdminSvr, "");
                    //ClientApi.Instance.subscribeSystemMessageForAdmin(Instance_OnAdminMsg);
                    isSubscribeForAdmin = true;
                }
            }
        }

        private void Instance_OnAdminMsg(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessAdminMsg(NetMessage netMsg)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                TTbookSystemMessage data = BaseHelper.ConvertStcMessage<TTbookSystemMessage>(new string[] { $"{ ApiConsts.Topic_stc_admin_prefix}**" }, netMsg);
                if (data != null)
                {
                    List<TTbookSystemMessage> temList = new List<TTbookSystemMessage>();
                    temList.Add(data);
                    OnAdminMsg?.Invoke(temList);
                }
            });
        }


        private void Instance_OnAdminMsg(List<TTbookSystemMessage> msg)
        {
            OnAdminMsg?.Invoke(msg);
        }

        #region 账号/角色/模块/模块操作 API

        private TRoleModulesActionQuery _currentRoleModulesAction = null;

        public Module GetModuleById(string id)
        {
            Module result = null;

            if (_currentRoleModulesAction != null && _currentRoleModulesAction.Datas != null)
            {
                foreach (Module m in _currentRoleModulesAction.Datas)
                {
                    if (m.id.Equals(id))
                    {
                        result = m;

                        break;
                    }
                }
            }

            return result;
        }


        public bool CheckPermisionById(string parentModuleId, string moduleId)
        {
            bool result = false;

            Module module = GetModuleById(moduleId);
            if (module != null)
            {
                //module = GetModuleById(moduleId);
                //if (module != null)
                result = true;
            }

            return result;
        }

        public bool CheckPermission(string moduleId, string moduleActionId)
        {
            bool result = false;

            Module module = GetModuleById(moduleId);
            if (module != null)
            {
                if (!string.IsNullOrEmpty(moduleActionId))
                {
                    foreach (ModulesAction ma in module.moduleaction)
                    {
                        if (ma.id.Equals(moduleActionId))
                        {
                            result = true;
                            break;
                        }
                    }
                }
                else
                    result = true;//只需要判断到module层
            }

            return result;
        }

        public List<ModulesAction> GetModulesActionById(string id)
        {
            List<ModulesAction> result = null;

            if (_currentRoleModulesAction != null && _currentRoleModulesAction.Datas != null)
            {
                foreach (Module m in _currentRoleModulesAction.Datas)
                {
                    if (m.id.Equals(id))
                    {
                        List<ModulesAction> temp = new List<ModulesAction>();
                        foreach (ModulesAction n in m.moduleaction)
                        {
                            if (n.moduleid.Equals(m.id))
                                temp.Add(n);

                        }
                        result = temp;
                        break;
                    }
                }
            }
            return result;
        }

        //当前用户所属角色
        public TUsersQuery usersQuery(TUserRequest request = null)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TUsersQuery>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_usersQuery, request);// ClientApi.Instance.usersQuery();
        }

        //角色查询
        public TRolesQuery rolesQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<TRolesQuery>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_rolesQuery, "rolesQuery");// ClientApi.Instance.rolesQuery();
        }

        //角色增改删
        public TRoleUpdate roleUpdate(RoleUpdateReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TRoleUpdate>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_roleUpdate, req);//ClientApi.Instance.roleUpdate(req);
        }

        //模块查询
        public TModulesQuery modulesQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<TModulesQuery>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_modulesQuery, null);//ClientApi.Instance.modulesQuery();
        }

        //模块动作查询
        public TModulesActionQuery modulesActionQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<TModulesActionQuery>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_modulesActionQuery, null);//ClientApi.Instance.modulesActionQuery();
        }

        //角色模块动作设置
        public PResult rolesModulesActionUpdate(RolesModulesActionUpdateReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_rolesModulesActionUpdate, request);//ClientApi.Instance.rolesModulesActionUpdate(request);
        }

        // 角色模块动作查询
        public TRoleModulesActionQuery roleModulesActionQuery(RoleModulesActionQueryReq request)
        {
            _currentRoleModulesAction = ApiManager.Instance.GetDefaultApi().Request<TRoleModulesActionQuery>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_roleModulesActionQuery, request);//ClientApi.Instance.roleModulesActionQuery(request);

            return _currentRoleModulesAction;
        }

        //用户增删改
        public PResult userRoleUpdate(UserRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_userRoleUpdate, request);//ClientApi.Instance.userRoleUpdate(request);
        }

        private STCUserReplyPo _Cache_STCUserReplyPo;
        public STCUserReplyPo stcUserQuery()
        {
            UserRequest request = new UserRequest();
            request.username = LocalEnv.UserInfo.UserName;
            _Cache_STCUserReplyPo = ApiManager.Instance.GetDefaultApi().Request<STCUserReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_userQuery, request);//ClientApi.Instance.stcUserQuery();
            return _Cache_STCUserReplyPo;
        }

        public STCUserReplyPo GetCachestcUserQuery()
        {
            return _Cache_STCUserReplyPo;
        }

        // 关注组操作
        public HomeGroupUpdateReplyPo homeGroupUpdate(HomeGroupUpdateReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<HomeGroupUpdateReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupUpdate, request);//ClientApi.Instance.homeGroupUpdate(request);
        }

        // 关注组查询
        public HomeGroupReplyPo homeGroupQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<HomeGroupReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupQuery, null);//ClientApi.Instance.homeGroupQuery();
        }

        // 债券组操作, QA 不能重复债券
        public PResult homeSecurityidsUpdate(HomeGroupSecurityidsUpdateReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupSecurityIdsUpdate, request);//ClientApi.Instance.homeGroupSecurityidsUpdate(request);
        }

        private HomeGroupSecurityidsReplyPo _Cache_HomeGroupSecurityidsReplyPo;
        // 债券组查询
        public HomeGroupSecurityidsReplyPo homeGroupSecurityidsQuery(HomeGroupSecurityidsUpdateReq request)
        {
            _Cache_HomeGroupSecurityidsReplyPo = ApiManager.Instance.GetDefaultApi().Request<HomeGroupSecurityidsReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupSecurityIdsQuery, request);//ClientApi.Instance.homeGroupSecurityIdsQuery();
            if ((request == null || string.IsNullOrEmpty(request.type))) 
            {
                dicGroup.Clear();
                if (_Cache_HomeGroupSecurityidsReplyPo != null && _Cache_HomeGroupSecurityidsReplyPo.Datas != null && _Cache_HomeGroupSecurityidsReplyPo.Code.Equals("0000"))
                {
                    foreach (KeyValuePair<string, HomeGroupInfo> pair in _Cache_HomeGroupSecurityidsReplyPo.Datas)
                    {
                        dicGroup.Add(pair.Key, pair.Value);
                    }
                }
            }
            return _Cache_HomeGroupSecurityidsReplyPo;
        }

        public HomeGroupSecurityidsReplyPo GetCacheHomeGroupSecurityidsQuery()
        {
            return _Cache_HomeGroupSecurityidsReplyPo;
        }


        // 关注组操作
        public HomeGroupUpdateReplyPo homeGroupUpdate2(HomeGroupUpdateReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<HomeGroupUpdateReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupUpdate2, request);//ClientApi.Instance.homeGroupUpdate(request);
        }

        // 关注组查询
        public HomeGroupReplyPo homeGroupQuery2()
        {
            return ApiManager.Instance.GetDefaultApi().Request<HomeGroupReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupQuery2, null);//ClientApi.Instance.homeGroupQuery();
        }

        // 债券组操作, QA 不能重复债券
        public PResult homeSecurityidsUpdate2(HomeGroupSecurityidsUpdateReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupSecurityIdsUpdate2, request);//ClientApi.Instance.homeGroupSecurityidsUpdate(request);
        }

        public static Dictionary<string, HomeGroupInfo> dicGroup2 = new Dictionary<string, HomeGroupInfo>();
        private HomeGroupSecurityidsReplyPo _Cache_HomeGroupSecurityidsReplyPo2;
        // 债券组查询
        public HomeGroupSecurityidsReplyPo homeGroupSecurityidsQuery2(HomeGroupSecurityidsUpdateReq request)
        {
            _Cache_HomeGroupSecurityidsReplyPo2 = ApiManager.Instance.GetDefaultApi().Request<HomeGroupSecurityidsReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_homeGroupSecurityIdsQuery2, request);//ClientApi.Instance.homeGroupSecurityIdsQuery();
            if ((request == null || string.IsNullOrEmpty(request.type)))
            {
                dicGroup2.Clear();
                if (_Cache_HomeGroupSecurityidsReplyPo2 != null && _Cache_HomeGroupSecurityidsReplyPo2.Datas != null && _Cache_HomeGroupSecurityidsReplyPo2.Code.Equals("0000"))
                {
                    foreach (KeyValuePair<string, HomeGroupInfo> pair in _Cache_HomeGroupSecurityidsReplyPo2.Datas)
                    {
                        dicGroup2.Add(pair.Key, pair.Value);
                    }
                }
            }
            return _Cache_HomeGroupSecurityidsReplyPo2;
        }


        public HomeGroupSecurityidsReplyPo GetCacheHomeGroupSecurityidsQuery2()
        {
            return _Cache_HomeGroupSecurityidsReplyPo2;
        }

        public PResult userUpdate(TUserRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_admin_updateUser, req);//ClientApi.Instance.userUpdate(req);
        }

        public TSafeManger safeMangerService(TSafeMangerReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TSafeManger>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_safe, req);
        }

        #endregion

        public Task queryAccountAndUsers()
        {
            return Task.Run(() =>
            {
                if (_currentAccountUser == null)
                {
                    AccountAndUsersQueryReq req = new AccountAndUsersQueryReq();
                    req.userid = LocalEnv.UserInfo.UserId;
                    _currentAccountUser = ApiManager.Instance.GetDefaultApi().Request<AccountAndUsersQueryReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_queryAccountAndUsers, req);
                }
            });
        }

        public AccountAndUsersQueryReplyPo getAccountUser()
        {
            return _currentAccountUser;
        }

        /// <summary>
        /// 系统参数查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public TbookSystemParameterReplyPo querySystemParameter(SystemParameterReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TbookSystemParameterReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_systemPara, request);
        }

        public PResult updateSystemGlobalPara(SystemGlobalParamReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_updateSystemGlobalPara, request);
        }

        /// <summary>
        /// 返回网页Model
        /// </summary>
        /// <returns></returns>
        public List<Module> GetWebModule()
        {
            List<Module> result = new List<Module>();

            if (_currentRoleModulesAction != null && _currentRoleModulesAction.Datas != null)
            {
                foreach (Module m in _currentRoleModulesAction.Datas)
                {
                    if (!string.IsNullOrEmpty(m.inf1) && !string.IsNullOrEmpty(m.inf2))
                    {
                        //if (m.inf1.ToLower() == "web" &&  (string.IsNullOrWhiteSpace(m.inf3) ? "stc,bdb,idcclient" : m.inf3.ToLower()).Contains(LocalEnv.ServerInfo.AppExe.ToLower()))
                        if (m.inf1.ToLower() == "web")
                        {
                            result.Add(m);
                        }
                    }
                }
            }

            //是否在菜单显示演示程序菜单入口
            string isShowPlotDemoMenu = ConfigurationManager.AppSettings["IsShowPlotDemoMenu"];
            isShowPlotDemoMenu = string.IsNullOrEmpty(isShowPlotDemoMenu) ? "0" : isShowPlotDemoMenu;
            if (isShowPlotDemoMenu == "1")
            {
                //Add Demo Modle
                string urifilename = AppDomain.CurrentDomain.BaseDirectory + $@"bin\HTML\index.html";
                if (System.IO.File.Exists(urifilename))
                {
                    STC.Common.Model.Admin.Module demoweb = new STC.Common.Model.Admin.Module() { modulename = "C# Js Call", closeby = "test", createtime = "2025-03-20 16:00:00", id = "9898", inf1 = "web", inf2 = @"bin\HTML\index.html", inf3 = "", inf4 = "0", inf5 = "" };
                    result.Add(demoweb);
                }
            }

            if (LocalEnv.UserInfo.BdbBboPermissions == "1")
            {
                //BDB临时加入市场最优
                if (CommonUtils.IsBDBAppName())
                {
                    STC.Common.Model.Admin.Module bestmartmenu = result.FirstOrDefault(o => o.id == "4055");
                    if (bestmartmenu == null)
                    {
                        STC.Common.Model.Admin.Module newm = new STC.Common.Model.Admin.Module() { modulename = "市场最优", closeby = "test", createtime = "2025-03-20 16:00:00", id = "4055", inf1 = "web", inf2 = @"https://qb3.idbhost.com:28888/cust", inf3 = "", inf4 = "0", inf5 = "" };
                        result.Add(newm);
                    }
                }
            }

            if (LocalEnv.UserInfo.BdbStaratPermissions == "1")
            {
                //BDB临时加入策略定价
                if (CommonUtils.IsBDBAppName())
                {
                    STC.Common.Model.Admin.Module bestmartmenu = result.FirstOrDefault(o => o.id == "4056");
                    if (bestmartmenu == null)
                    {
                        STC.Common.Model.Admin.Module newm = new STC.Common.Model.Admin.Module() { modulename = "策略定价", closeby = "test", createtime = "2025-07-02 16:00:00", id = "4056", inf1 = "web", inf2 = @"https://qb3.idbhost.com:28888/quant/", inf3 = "", inf4 = "0", inf5 = "" };
                        result.Add(newm);
                    }
                }
            }

            return result;
        }

        #region 组合管理
        public BookBookReplyPo queryBookUser()
        {
            return ApiManager.Instance.GetDefaultApi().Request<BookBookReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_QueryBook, null);
        }

        public Dictionary<string, ObservableCollection<TBookBookTreeItemModel>> GetDicTbookAllUser()
        {
            return dicTbookAllUser;
        }

        public Dictionary<string, List<TTbookBook>> GetDicTTbook(string user = null)
        {
            return null;
        }
        public ObservableCollection<TBookBookTreeItemModel> GetTAllBookCollection()
        {
            ObservableCollection<TBookBookTreeItemModel> list = new ObservableCollection<TBookBookTreeItemModel>();
            foreach (KeyValuePair<string, ObservableCollection<TBookBookTreeItemModel>> pair in dicTbookAllUser)
            {
                if (pair.Value != null && pair.Value.Count() > 0)
                {
                    foreach (TBookBookTreeItemModel item in pair.Value)
                    {
                        list.Add(item);
                    }
                }
            }
            LogHelper.Info("GetTAllBookCollection:"+ list.Count);
            return list;
        }
        public ObservableCollection<TBookBookTreeItemModel> GetTCurrentBookCollection(string user = null)
        {
            if (string.IsNullOrEmpty(user))
                user = ApiManager.Instance.GetDefaultApi().UserName;
            if (dicTbookAllUser.ContainsKey(user))
                return dicTbookAllUser[user];
            return null;
        }

        public List<TTbookBook> GetTTbookBook(string user = null)
        {
            if (string.IsNullOrEmpty(user))
                user = ApiManager.Instance.GetDefaultApi().UserName;
            if (dicTbookUser.ContainsKey(user))
                return dicTbookUser[user];
            return null;
        }

        public TTbookBook GetTTbookByID(string id)
        {
            if (dicTbookId.ContainsKey(id))
                return dicTbookId[id];
            return null;
        }


        public List<TTbookBook> GetChildTTbookByID(string id)
        {
            List<TTbookBook> list = new List<TTbookBook>();
            List<TTbookBook> temp = dicTbookId.Values.Where(o => o.Parentnode.Equals(id)).ToList();
            list = AssmbleChildTTbookByID(list, temp, id);
            return list;
        }

        private List<TTbookBook> AssmbleChildTTbookByID(List<TTbookBook> list, List<TTbookBook> temp, string id)
        {

            if (temp != null && temp.Count > 0)
            {
                foreach (var p in temp)
                {
                    list.Add(p);
                    List<TTbookBook> temp2 = dicTbookId.Values.Where(o => o.Parentnode.Equals(p.Id)).ToList();
                    if (temp2 != null && temp2.Count > 0)
                    {
                        AssmbleChildTTbookByID(list, temp2, p.Id);
                    }
                }
            }
            return list;
        }

        private string rootKey = "root";
        private Dictionary<string, TTbookBook> dicTbookId = new Dictionary<string, TTbookBook>();
        private Dictionary<string, ObservableCollection<TBookBookTreeItemModel>> dicTbookAllUser = new Dictionary<string, ObservableCollection<TBookBookTreeItemModel>>();
        private Dictionary<string, List<TTbookBook>> dicTbookUser = new Dictionary<string, List<TTbookBook>>();

        public void InitBookDicByUser(string user)
        {
            dicTbookId.Clear();
            BookBookReplyPo reply = QueryBook();
            
            if (reply != null && reply.Datas != null)
            {
                LogHelper.Info("InitBookDicByUser queryBook size:" + reply.Datas.Count);
                foreach (TTbookBook po in reply.Datas)
                {
                    if (!dicTbookId.ContainsKey(po.Id))
                        dicTbookId.Add(po.Id, po);
                }
            }
            LogHelper.Info("InitBookDicByUser user:" + user);
            if ("*".Equals(user))
            {
                BookUserReplyPo reply1 = QueryUser();
                if (reply1 != null && reply1.Datas != null)
                {
                    LogHelper.Info("InitBookDicByUser QueryUser:" + reply1.Datas.Count);
                    List<string> userList = reply1.Datas.Select(p => p.Username).Distinct().ToList();
                    foreach (string s in userList)
                    {

                        dicTbookAllUser.Add(s, AssembleData(reply.Datas, s));
                    }
                }
            }
            else
            {
                AssembleData(reply.Datas, user);
            }
            LogHelper.Info("dicTbookAllUser QueryUser:" + dicTbookAllUser.Count);
        }

        public Dictionary<string, string> GetAllBookDic() 
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (TTbookBook tbookBook in dicTbookId.Values) 
            {
                if ("A".Equals(tbookBook.Booktype) && !"D".Equals(tbookBook.Status)) 
                {
                    if (!result.ContainsKey(tbookBook.Id)) 
                    {
                        result.Add(tbookBook.Id,tbookBook.Bookname);
                    }
                }
            }
            return result;
        }

        public ObservableCollection<TBookBookTreeItemModel> AssembleData(List<TTbookBook> dataList, string user)
        {
            ObservableCollection<TBookBookTreeItemModel> _currentBookCollection = new ObservableCollection<TBookBookTreeItemModel>();
            List<TTbookBook> UserBookList = new List<TTbookBook>();
            if (dataList != null && dataList.Count > 0)
            {
                List<TTbookBook> list = dataList.Where(delegate (TTbookBook tb)
                {
                    bool isResult = false;
                    if (!string.IsNullOrEmpty(tb.UserName))
                    {
                        if (tb.UserName.Contains(","))
                        {
                            string[] str = tb.UserName.Split(',');
                            if (str.Contains(user))
                                return true;
                        }
                        else if (tb.UserName.Equals(user))
                        {
                            return true;
                        }
                    }
                    return isResult;

                }).ToList();
                //dicTbook.Clear();
                Dictionary<string, List<TTbookBook>> dicTbook = new Dictionary<string, List<TTbookBook>>();
                UserBookList.Clear();
                try
                {
                    foreach (TTbookBook po in list)
                    {
                        if (!UserBookList.Contains(po))
                            UserBookList.Add(po);
                        AddParent(po, UserBookList);
                    }
                    if (!dicTbookUser.ContainsKey(user))
                        dicTbookUser.Add(user, UserBookList);
                    foreach (TTbookBook po in UserBookList)
                    {
                        string key = string.IsNullOrEmpty(po.Parentnode) ? rootKey : po.Parentnode;
                        if (!dicTbook.ContainsKey(key))
                        {
                            List<TTbookBook> temp = new List<TTbookBook>();
                            temp.Add(po);
                            dicTbook.Add(key, temp);
                        }
                        else if (!dicTbook[key].Contains(po))
                            dicTbook[key].Add(po);
                    }
                    foreach (string rootKey in dicTbook.Keys)
                    {
                        foreach (TTbookBook item in dicTbook[rootKey])
                        {
                            TBookBookTreeItemModel model = new TBookBookTreeItemModel();
                            model.Id = item.Id;
                            model.Name = item.Bookname;
                            model.UserName = item.UserName;
                            model.Remark = item.Remark;
                            model.External = item.External;
                            model.UserId = item.Userid;
                            model.BookType = item.Booktype;
                            model.Index = model.Index + 1;
                            model.Parentnode = item.Parentnode;
                            model.ProductId = item.Inf3 ?? "";
                            List<TTbookBook> childList = dicTbook.ContainsKey(item.Id) ? dicTbook[item.Id] : null;

                            if (string.IsNullOrEmpty(item.Parentnode) && !_currentBookCollection.Contains(model))
                                _currentBookCollection.Add(model);
                            AddData(model, childList, _currentBookCollection, dicTbook);
                        }
                    }

                }
                catch (Exception e)
                {
                    LogHelper.Error("AssembleData ERROR:" + e);
                }
            }
            return _currentBookCollection;
        }


        public BookBookReplyPo QueryBook()
        {
            return ApiManager.Instance.GetDefaultApi().Request<BookBookReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_QueryBook, null);
        }

        public BookUserReplyPo QueryUser()
        {
            return ApiManager.Instance.GetDefaultApi().Request<BookUserReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_QueryUser, null);
        }

        public void AddParent(TTbookBook po, List<TTbookBook> UserBookList)
        {
            if (!string.IsNullOrEmpty(po.Parentnode) && dicTbookId.ContainsKey(po.Parentnode))
            {
                TTbookBook parent = dicTbookId[po.Parentnode];
                if (!UserBookList.Contains(parent))
                    UserBookList.Add(parent);
                AddParent(parent, UserBookList);
            }
        }

        public void AddData(TBookBookTreeItemModel root, IList<TTbookBook> orgs, ObservableCollection<TBookBookTreeItemModel> _currentBookCollection, Dictionary<string, List<TTbookBook>> dicTbook)
        {
            if (orgs != null && orgs.Count > 0)
            {
                foreach (TTbookBook item in orgs)
                {
                    TBookBookTreeItemModel model = new TBookBookTreeItemModel();
                    model.Id = item.Id;
                    model.Name = item.Bookname;
                    model.UserName = item.UserName;
                    model.Remark = item.Remark;
                    model.External = item.External;
                    model.UserId = item.Userid;
                    model.BookType = item.Booktype;
                    model.Index = root.Index + 1;
                    model.Parent = root;
                    model.Parentnode = item.Parentnode;
                    model.ProductId = item.Inf3 ?? "";
                    if (root != null)
                    {
                        root.Nodes.Add(model);
                    }
                    List<TTbookBook> childList = dicTbook.ContainsKey(item.Id) ? dicTbook[item.Id] : null;
                    //if (!_currentBookCollection.Contains(model))
                    //    _currentBookCollection.Add(model);
                    AddData(model, childList, _currentBookCollection, dicTbook);
                }
            }
        }
        #endregion


        public TbookPositionReplyPo queryPosition(PositionReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TbookPositionReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_queryPosition, request);
        }

        public TbookTradeReplyPo queryTrade(PositionReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TbookTradeReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_queryTrade, request);
        }
        
        /// <summary>
        /// 查询所有有外部映射的组合
        /// </summary>
        /// <returns></returns>
        public BookBookReplyPo QueryBookUserByExternal()
        {
            return ApiManager.Instance.GetDefaultApi().Request<BookBookReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_QueryBookUserByExternal, null);
        }

        public void QuerySecurityGroupModule() 
        {
            List<ModulesAction> Action = AdminHandler.Instance.GetModulesActionById(_SecurityGroupModelId);
            if (Action != null && Action.Count > 0)
            {
                foreach (ModulesAction m in Action)
                {
                    if ("402702".Equals(m.id))//管理自己&查看全部
                    {
                        user = "*";
                        ManageYourselfViewAll = true;
                    }
                    else if ("402703".Equals(m.id))//管理全部
                    {
                        user = "*";
                    }
                    else
                    {

                    }
                    break;
                }
            }
        }
        public string getSecurityGroupUser() 
        {
            return user;
        }
        public bool getManageYourselfViewAll()
        {
            return ManageYourselfViewAll;
        }

        public ObservableCollection<TBookBookTreeItemModel> GetTParentBookCollection()
        {
            ObservableCollection<TBookBookTreeItemModel> _parentBookCollection = new ObservableCollection<TBookBookTreeItemModel>();
            Dictionary<string, List<TTbookBook>> dicTbookByParent = new Dictionary<string, List<TTbookBook>>();
            BookBookReplyPo reply = QueryBook();
            if (reply != null && reply.Datas != null)
            {
                foreach (TTbookBook po in reply.Datas)
                {
                    string key = string.IsNullOrEmpty(po.Parentnode) ? rootKey : po.Parentnode;
                    if (!dicTbookByParent.ContainsKey(key))
                    {
                        List<TTbookBook> temp = new List<TTbookBook>();
                        temp.Add(po);
                        dicTbookByParent.Add(key, temp);
                    }
                    else
                        dicTbookByParent[key].Add(po);
                }
            }
            if (dicTbookByParent.ContainsKey(rootKey))
            {
                foreach (TTbookBook item in dicTbookByParent[rootKey])
                {
                    TBookBookTreeItemModel model = new TBookBookTreeItemModel();
                    model.Id = item.Id;
                    model.Name = item.Bookname;
                    model.UserName = item.UserName;
                    model.Remark = item.Remark;
                    model.External = item.External;
                    model.UserId = item.Userid;
                    model.BookType = item.Booktype;
                    model.Index = 1;//根节点默认为1
                    List<TTbookBook> childList = dicTbookByParent.ContainsKey(item.Id) ? dicTbookByParent[item.Id] : null;
                    _parentBookCollection.Add(model);
                    AddData(model, childList, dicTbookByParent);
                }
            }
            return _parentBookCollection;
        }
        public void AddData(TBookBookTreeItemModel root, IList<TTbookBook> orgs, Dictionary<string, List<TTbookBook>> dicTbook)
        {
            if (orgs != null && orgs.Count > 0)
            {
                foreach (TTbookBook item in orgs)
                {
                    TBookBookTreeItemModel model = new TBookBookTreeItemModel();
                    model.Id = item.Id;
                    model.Name = item.Bookname;
                    model.UserName = item.UserName;
                    model.Remark = item.Remark;
                    model.External = item.External;
                    model.UserId = item.Userid;
                    model.BookType = string.IsNullOrEmpty(item.Booktype) ? "A" : item.Booktype;
                    model.Index = root.Index + 1;
                    model.Parent = root;
                    if (root != null)
                    {
                        root.Nodes.Add(model);
                    }
                    List<TTbookBook> childList = dicTbook.ContainsKey(item.Id) ? dicTbook[item.Id] : null;
                    AddData(model, childList, dicTbook);
                }
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <returns></returns>
        public PResult WriteDBLog(DBLogRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AdminSvr, ApiConsts.Topic_DB_Log, req);
        }


        public TbookUserGroupsReplyPo queryUserGroup()
        {
            _AllUserGroup = ApiManager.Instance.GetDefaultApi().Request<TbookUserGroupsReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_queryUserGroup, null);
            return _AllUserGroup;
        }

        private TbookUserGroupsReplyPo _AllUserGroup;

        public TbookUserGroups GetUserGroup(string biztypeid)
        {
            if (_AllUserGroup != null && _AllUserGroup.Code == "0000")
            {
                foreach(TbookUserGroups currentGroup in _AllUserGroup.Datas)
                {
                    if (currentGroup.Groupid == biztypeid)
                    {
                        return currentGroup;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// key: manageid
        /// value: key:type  value：userid
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> managerDic = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, List<string>> traderDic = new Dictionary<string, List<string>>();
        ///// <summary>
        ///// key:type  value:manager_trader;  
        ///// </summary>
        //private Dictionary<string, List<string>> typeDic = new Dictionary<string, List<string>>();

        public void queryManagerUser()
        {
            TQueryManagerUser tQueryManagerUser = ApiManager.Instance.GetDefaultApi().Request<TQueryManagerUser>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_admin_managerUser_queryManagerUser, null);
            if (tQueryManagerUser != null && tQueryManagerUser.IsSuccess())
            {
                foreach(ManagerUser mu in tQueryManagerUser.Datas)
                {
                    string key = string.Empty;
                    // -------------------------------------
                    if (!managerDic.ContainsKey(mu.mangerid))
                    {
                        managerDic[mu.mangerid] = new Dictionary<string, string>();
                        managerDic[mu.mangerid][Consts.IR] = null;
                        managerDic[mu.mangerid][Consts.CTP] = null;
                        managerDic[mu.mangerid][Consts.IB] = null;
                        managerDic[mu.mangerid][Consts.CP] = null;
                    }
                    //if (!managerDic[mu.mangerid][Consts.IR].Contains(mu.ir_userid))// 利率债
                    {
                        managerDic[mu.mangerid][Consts.IR] = mu.ir_userid;
                        //key = $"{mu.mangerid}_{mu.ir_userid}";
                        //GenTypeDic("ir", key);
                    }
                    //if (!managerDic[mu.mangerid][Consts.CTP].Contains(mu.cfp_userid))//商金二永
                    {
                        managerDic[mu.mangerid][Consts.CTP] = mu.cfp_userid;
                        //key = $"{mu.mangerid}_{mu.cfp_userid}";
                        //GenTypeDic("cfp", key);
                    }
                    //if (!managerDic[mu.mangerid][Consts.IB].Contains(mu.ib_userid))//银行间
                    {
                        managerDic[mu.mangerid][Consts.IB] = mu.ib_userid;
                        //key = $"{mu.mangerid}_{mu.ib_userid}";
                        //GenTypeDic("ib", key);
                    }
                    //if (!managerDic[mu.mangerid][Consts.CP].Contains(mu.cp_userid))//公司债
                    {
                        managerDic[mu.mangerid][Consts.CP] = mu.cp_userid;
                        //key = $"{mu.mangerid}_{mu.cp_userid}";
                        //GenTypeDic("cp", key);
                    }

                    // --------------------------
                    if (!traderDic.ContainsKey(mu.ir_userid))
                    {
                        traderDic[mu.ir_userid] = new List<string>();
                    }
                    if (!traderDic[mu.ir_userid].Contains(mu.mangerid))
                    {
                        traderDic[mu.ir_userid].Add(mu.mangerid);
                    }
                    if (!traderDic.ContainsKey(mu.cfp_userid))
                    {
                        traderDic[mu.cfp_userid] = new List<string>();
                    }
                    if (!traderDic[mu.cfp_userid].Contains(mu.mangerid))
                    {
                        traderDic[mu.cfp_userid].Add(mu.mangerid);
                    }
                    if (!traderDic.ContainsKey(mu.ib_userid))
                    {
                        traderDic[mu.ib_userid] = new List<string>();
                    }
                    if (!traderDic[mu.ib_userid].Contains(mu.mangerid))
                    {
                        traderDic[mu.ib_userid].Add(mu.mangerid);
                    }
                    if (!traderDic.ContainsKey(mu.cp_userid))
                    {
                        traderDic[mu.cp_userid] = new List<string>();
                    }
                    if (!traderDic[mu.cp_userid].Contains(mu.mangerid))
                    {
                        traderDic[mu.cp_userid].Add(mu.mangerid);
                    }
                }
            }
        }

        //private void GenTypeDic(string type, string key)
        //{
        //    if (!string.IsNullOrEmpty(key))
        //    {
        //        if (!typeDic.ContainsKey(type))
        //        {
        //            typeDic[type] = new List<string>();
        //        }
        //        if (!typeDic[type].Contains(key))
        //        {
        //            typeDic[type].Add(key);
        //        }
        //    }
        //}

        public List<string> GetTraderList(string managerUserName, string type = null)
        {
            
            if (!string.IsNullOrEmpty(managerUserName) && managerDic.ContainsKey(managerUserName))
            {
                List<string> traderList = new List<string>();
                if (!string.IsNullOrEmpty(type) && managerDic[managerUserName].ContainsKey(type))
                {
                    traderList.Add(managerDic[managerUserName][type]);
                    return traderList;
                }
                else
                {
                    traderList.AddRange(managerDic[managerUserName].Values);
                    return traderList;
                }
            }
            return null;
        }

        public List<string> GetManagerList(string traderUserName, string type = null)
        {
            if (managerDic != null)
            {
                List<string> manageList = new List<string>();
                foreach (KeyValuePair<string, Dictionary<string, string>> pair in managerDic)
                {
                    string managerId = pair.Key;
                    foreach (KeyValuePair<string, string> subPair in pair.Value)
                    {
                        if (subPair.Value == traderUserName)
                        {
                            if (string.IsNullOrEmpty(type) || subPair.Key == type)
                            {
                                manageList.Add(managerId);
                                continue;
                            }
                        }
                    }
                }
                return manageList;
            }
            
            //if (traderDic.ContainsKey(traderUserName))
            //{
            //    return traderDic[traderUserName];
            //}
            return null;
        }

        //public List<string> GetType(string securityId)
        //{
        //    string type = "";
        //    if (typeDic.ContainsKey(type))
        //    {
        //        return typeDic[type];
        //    }
        //    return null;
        //}
    }
}

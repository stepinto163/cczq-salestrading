using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Chrome.Plugin.Handler;
using STC.Common.Handler.Base;
using STC.Common.Handler.FinQ;
using STC.Common.Model.Base;
using STC.Common.Model.Chrome;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Model.Risk;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Chrome.Plugin.Data
{
    public class CefHandler
    {
        #region 构造
        /// <summary>
        /// CefHandler
        /// </summary>
        private static CefHandler instance = new CefHandler();

        public static CefHandler Instance { get { return instance; } }

        public ConcurrentDictionary<string, Window> _UniqueKeyMapWindowDic = new ConcurrentDictionary<string, Window>();
        private ConcurrentDictionary<int, string> _WinHashCodeMapUniqueKeyDic = new ConcurrentDictionary<int, string>();

        public  ConcurrentDictionary<string, string> DictTagGroupDeal = new ConcurrentDictionary<string, string>();

        /// <summary>
        ///  订阅的TagGroup   
        /// </summary>
        public ConcurrentDictionary<string, string> DictSubscribeTagGroup = new ConcurrentDictionary<string, string>(); 
        private object lockobj = new object();


        //FQ页面是否中准备好
        public bool IsFinQWebReady = false;

        #region FQ数据

        private FQBindTokenResponse fqclienttoken = new FQBindTokenResponse();
        /// <summary>
        /// FQ 绑定Token返回结果
        /// </summary>
        public FQBindTokenResponse FQClientToken
        {
            get { return fqclienttoken; }
            set
            {
                if (value == null || value.token == null || string.IsNullOrWhiteSpace(value?.token?.clientToken))
                {
                    IsValidServerToken = false;
                    return;
                }

                //触发回调，返回给前端
                if (!string.IsNullOrWhiteSpace(value?.token?.clientToken))
                {
                    fqclienttoken = value;

                    string clientToken = fqclienttoken.token.clientToken;
                    LastGetTokenTime = TokenCountdownHandler.Instance.FromUnixTimeSeconds(fqclienttoken.token.createTs).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    TokenExpirationTime = DateTimeEx.ConvertStringToDateTime(LastGetTokenTime).AddSeconds(fqclienttoken.token.expiresIn).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    
                    QQ = fqclienttoken.userInfo.qq;
                    createTs = fqclienttoken.token.createTs;
                    expiresIn = fqclienttoken.token.expiresIn;
                    //调试，发布移除
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "SendToken" });

                    IsValidServerToken = true;

                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "LocalSendToken", Info3 = JsonConvert.SerializeObject(fqclienttoken) });
                    // 启动倒计时
                    TokenCountdownHandler.Instance.Start(int.Parse(fqclienttoken.token.expiresIn.ToString()));
                    //TokenCountdownHandler.Instance.Start(30);
                }
                else
                {
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "SendToken" });
                }
            }
        }

        private FQBindTokenResponse _fqservertoken = new FQBindTokenResponse();
        /// <summary>
        /// FQ 服务器Token
        /// </summary>
        public FQBindTokenResponse FqServerToken
        {
            get { return _fqservertoken; }
            set
            {
                _fqservertoken = value;
            }
        }

        private string _fqtokenfanout;
        /// <summary>
        /// FQ Heat Token
        /// </summary>
        public string FQTokenFanout
        {
            get { return _fqtokenfanout; }
            set
            {
                _fqtokenfanout = value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQTokenFanout", Info3 = "" });
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "LocalSendTokenFanout", Info3 = FQTokenFanout });
                }
                else
                {
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "LocalSendTokenFanout", Info3 = "" });
                }
            }
        }

        private bool _isvalidservertoken = false;
        /// <summary>
        /// Token是否有效，如果无效，需要重新获取
        /// </summary>
        public bool IsValidServerToken
        {
            get { return _isvalidservertoken; }
            set
            {
                if (_isvalidservertoken != value)
                {
                    _isvalidservertoken = value;
                }
            }
        }


        private string _lastGetTokenTime = string.Empty;
        /// <summary>
        /// 最后获取Token时间
        /// </summary>
        public string LastGetTokenTime
        {
            get { return _lastGetTokenTime; }
            set
            {
                _lastGetTokenTime = value;
            }
        }


        private string _tokenexpirationtime = string.Empty;
        /// <summary>
        /// 过期时间
        /// </summary>
        public string TokenExpirationTime
        {
            get { return _tokenexpirationtime; }
            set
            {
                _tokenexpirationtime = value;
            }
        }


        private string _qq = string.Empty;
        /// <summary>
        /// FinQ 返回的 QQ
        /// </summary>
        public string QQ
        {
            get { return _qq; }
            set
            {
                _qq = value;
            }
        }

        public long createTs;
        public long expiresIn;

        #endregion

        public CefHandler()
        {

        }

        #endregion




        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            //初始化时清空数据
            BaseHandler.Instance.Init(Instance_OnClear);
            
            //Md原始行情处理
            Task.Factory.StartNew(() => dealMsg(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

            StartHttpServer();

            // 订阅事件
            TokenCountdownHandler.Instance.TokenInvalidated += (s, e) =>
            {
                LogHelper.Info("Token 即将失效，准备刷新...");
                // 这里可以调用你的刷新逻辑
                IsValidServerToken = false;
                //触发更新
                //获取Token
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(async () =>
                {
                    CefHandler.Instance.GetToken();
                }));
            };
            
        }

        #endregion

        #region Http服务 Token 相关

        public async void StartHttpServer()
        {
            try
            {
                //启动http服务 启动http服务，QB用7789,BDB用7790
                if ((LocalEnv.UserInfo.QeubeeFingPermissions == "1" || LocalEnv.ConfInfo.IsUseQBPrdTestFQNotOpenQB == "1") && LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
                {
                    await HttpServerService.Instance.StartAsync(7790);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("CefHandler StartHttpServer error", ex);
            }
        }

        public async void StopHttpServer()
        {
            try
            {
                //停止http服务
                await HttpServerService.Instance.StopAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Error("CefHandler StopHttpServer error", ex);
            }
        }

        public bool IsBussGetToken = false;
        /// <summary>
        /// 获取Token
        /// </summary>
        public async void GetToken()
        {
            if (IsBussGetToken)
                return;

            IsBussGetToken = true;
            string res = string.Empty;
            try
            {
                res = await GetServerTokenFromFQ();
                //string res = await GetServerTokenFromFinQSvrApi();
                if (JsonSplit.IsJson(res))
                {
                    FQBindTokenResponse fQBindTokenResponse = CommonUtils.DeserializeObject<FQBindTokenResponse>(res);
                    FqServerToken = fQBindTokenResponse;
                    if (fQBindTokenResponse.token == null)
                    {
                        object win = CefHandler.Instance.GetExistWin("WebFQ");
                        if (win != null && (win as BdbBorderWin).Opacity == 0)
                        {
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "ShowFQMessage", Info3 = fQBindTokenResponse.errMsg.Trim(), Info4 = "FinQ提醒:" });
                        }
                        else
                        {
                            if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                            {
                                QBPrompt3Seconds prompt = new QBPrompt3Seconds();
                                if (win != null)
                                {
                                    if (((BdbBorderWin)win).Visibility == Visibility.Visible)
                                        prompt.Owner = (BdbBorderWin)win;
                                }
                                else
                                    prompt.Owner = System.Windows.Application.Current.MainWindow;
                                prompt.PromptTitle = "提醒";
                                prompt.PromptContext = fQBindTokenResponse.errMsg;
                                prompt.Show();
                            }
                            else
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "ShowFQMessage", Info3 = fQBindTokenResponse.errMsg.Trim(), Info4 = "FinQ提醒:" });
                            }
                        }
                    }
                }

                IsBussGetToken = false;

                LogHelper.Info($"收到 ServerTokenFromFQ:{res}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("CefHandler StartHttpServer error", ex);
                LogHelper.Info($"收到 ServerTokenFromFQ:{res}");
            }
        }

        /// <summary>
        /// 获取ServerToken
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetServerTokenFromFQ()
        {
            try
            {
                //启动http服务，QB用7789 
                string prdurl = "http://42.192.33.198";
                string qautl = "https://finq.qmhost1.com";
                string qatoken = "";
                string prdtoken = "";
                string result = "";
                GetFinQTokenReq req = new GetFinQTokenReq();
                ////if (LocalEnv.ConfInfo.IsUseQBPrdTestFQNotOpenQB == "1")
                ////{
                ////    //req.fromQQ = !string.IsNullOrEmpty(LocalEnv.ConfInfo.QQApiSender) ? LocalEnv.ConfInfo.QQApiSender : LocalEnv.UserInfo.QQNumber;
                ////    req.fromQB = LocalEnv.UserInfo.UserName;
                ////    req.fromQBAccountID = LocalEnv.UserInfo.UserId;
                ////    req.fromService = "qb-client-api";

                ////    qatoken = "Bearer JK+orvJOC/TPJDH/omkYRPv5i9mFaJAVyEwXh5ASLezaojcSKFpau+VNcxQ0vEnydnFr2zxIAYRUHAZ+b4DWCw==";
                ////    prdtoken = "Bearer xEsgdq90TF+js4c20vxPmnuDeLgxwK9oTuu9r0+crn3qQjcy40JCVxj/09DFHhIhAVGgIPZmvNk/12FZGPubBQ==";
                ////    string json = CommonUtils.SerializeObject(req);
                ////    string url = $"{prdurl}/finq-server/v1/fq/action/bind_token?type=qb-client-api";
                ////    result = await ResulfApiHelper.PostJsonAsync(url, json, prdtoken);
                ////}
                ////else
                ////{
                    //req.fromQQ = !string.IsNullOrEmpty(LocalEnv.ConfInfo.QQApiSender) ? LocalEnv.ConfInfo.QQApiSender : LocalEnv.UserInfo.QQNumber;
                    req.fromQB = LocalEnv.UserInfo.UserName;
                    req.fromQBAccountID = LocalEnv.UserInfo.UserId;
                    req.fromService = "bdb-intent";

                    qatoken = "Bearer YPlRGd5+kDnpGxkPodZRhLOkeNHj6ETq+zvH9TC+G3/B18oCjqNgiVx4Nn0e/+RPhi8nNTf9IsRhWqNgLQnmAw==";
                    prdtoken = "Bearer F3K5E/RDFmM+pVw7bGDL2Oh/dqqzFT/MiOLOJ7MlR07NB+bQLsYUrMVrN1jnhyFR2bwfBGKl8HYG0iMIdwXFDg==";
                    string json = CommonUtils.SerializeObject(req);
                    string url = string.Empty;
                    string token = string.Empty;
                    if (LocalEnv.ConfInfo.LoginEnv.ToLower() == "prd")
                    {
                        url = $"{prdurl}/finq-server/v1/fq/action/bind_token?type=bdb-intent";
                        token = prdtoken;
                    }
                    else
                    {
                        url = $"{qautl}/finq-server/v1/fq/action/bind_token?type=bdb-intent";
                        token = qatoken;
                    }

                    result = await ResulfApiHelper.PostJsonAsync(url, json, token);
                ////}

                return result;
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + ex.Message + "\"}";
            }
        }


        private async Task<string> GetServerTokenFromFinQSvrApi()
        {
            string result = "";
            try
            {
                GetFinQTokenReq req = new GetFinQTokenReq();
                if (LocalEnv.ConfInfo.IsUseQBPrdTestFQNotOpenQB == "1")
                {
                    //req.fromQQ = !string.IsNullOrEmpty(LocalEnv.ConfInfo.QQApiSender) ? LocalEnv.ConfInfo.QQApiSender : LocalEnv.UserInfo.QQNumber;
                    req.fromQB = LocalEnv.UserInfo.UserName;
                    req.fromQBAccountID = LocalEnv.UserInfo.UserId;
                    req.fromService = "qb-client-api";

                    PResult reply = await GetFinQServerToken(req);
                    if (reply != null && reply.IsSuccess())
                    {

                    }
                    //result = await ResulfApiHelper.PostJsonAsync(url, json, prdtoken);
                }
                else
                {
                    //req.fromQQ = !string.IsNullOrEmpty(LocalEnv.ConfInfo.QQApiSender) ? LocalEnv.ConfInfo.QQApiSender : LocalEnv.UserInfo.QQNumber;
                    req.fromQB = LocalEnv.UserInfo.UserName;
                    req.fromQBAccountID = LocalEnv.UserInfo.UserId;
                    req.fromService = "bdb-intent";

                    PResult reply = await GetFinQServerToken(req);
                    if (reply != null && reply.IsSuccess())
                    {

                    }
                    
                    //result = await ResulfApiHelper.PostJsonAsync(url, json, token);
                }
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + ex.Message + "\"}";
            }

            return result;
        }

        public async Task<PResult> GetFinQServerToken(GetFinQTokenReq req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = FinQHandler.Instance.GetFinQServerToken(req);
            });

            return reply;
        }
        #endregion

        #region OnClear事件处理
        private void Instance_OnClear(ClearPo obj)
        {
            lock (lockobj)
            {
                DictTagGroupDeal.Clear();
                DictSubscribeTagGroup.Clear();
            }
        }
        #endregion


        public ConcurrentDictionary<string, List<string>> _SubscribeCallback = new ConcurrentDictionary<string, List<string>>();

        public ConcurrentDictionary<string, object> _CacheSubscribeResult = new ConcurrentDictionary<string, object>();

        public List<string> _ApiCacheTopic;

        /// <summary>
        /// 是否是有效的订阅Topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool IsValidSubscribe(string topic)
        {
            bool iscontains = true;
            List<string> topiclist = topic.Split(',').ToList();
            foreach (var primaryKey in topiclist)
            {
                if (!IsTopicSubscribed(primaryKey))
                {
                    iscontains = false;
                    break;  
                }
            }
            return iscontains;
        }

        /// <summary>
        /// 注册CT价格
        /// </summary>
        public void subscribe(string srvid, string topic, object param)
        {
            lock (lockobj)
            {
                List<string> topiclist = topic.Split(',').ToList();

                foreach (var primaryKey in topiclist)
                {
                    if (!_SubscribeCallback.ContainsKey(primaryKey))
                    {
                         ApiManager.Instance.GetDefaultApi().SubScribeWithImage(add, new string[] { primaryKey }, srvid, "");
                        _SubscribeCallback.TryAdd(primaryKey, new List<string>() { (string)param });
                    }
                    else
                    {
                        List<string> lst = new List<string>();
                        _SubscribeCallback.TryGetValue(primaryKey, out lst);
                        if (!lst.Contains((string)param))
                        {
                            lst.Add((string)param);
                            _SubscribeCallback.AddOrUpdate(primaryKey, lst, (key, value) => lst);
                        }
                    }

                    //回调数据
                    List<object> result = GetMatchingItems(primaryKey);
                    if (result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            add(item);
                        }
                    }
                }
            }
        }

        // 从字典中根据匹配条件筛选出符合条件的对象，并存入 List<object>
        public List<object> GetMatchingItems(string primaryKey)
        {
            List<object> matchingItems = new List<object>();

            foreach (var kvp in _CacheSubscribeResult)
            {
                if (IsMatch(primaryKey, kvp.Key ))
                {
                    matchingItems.Add(kvp.Value);
                }
            }

            return matchingItems;
        }

        public bool IsTopicSubscribed(string newTopic)
        {
            if (_ApiCacheTopic == null)
                _ApiCacheTopic = (ApiManager.Instance.GetDefaultApi().GetApiConnect() as com.gateway.connector.tcp.GateWayApi).cacheImageTopics;

            return _ApiCacheTopic.Any(topic => newTopic.StartsWith(topic));
        }


        /// <summary>
        /// 精细退订函数：只移除指定 topic 中的指定 param（支持部分退订）
        /// </summary>
        public void unsubscribe(string srvid, string topic, object param)
        {
            lock (lockobj)
            {
                List<string> topiclist = topic.Split(',').ToList();

                // 处理 _SubscribeCallback（按 topic 逐项退订 param）
                foreach (var primaryKey in topiclist)
                {
                    List<string> callbackList;
                    if (_SubscribeCallback.TryGetValue(primaryKey, out callbackList))
                    {
                        callbackList.Remove((string)param);

                        if (callbackList.Count == 0)
                        {
                            List<string> dummy;
                            _SubscribeCallback.TryRemove(primaryKey, out dummy);
                        }
                        else
                        {
                            _SubscribeCallback[primaryKey] = callbackList;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发送市场最优价订阅结果到前端，供前端更新TagGroupDeal数据
        /// </summary>
        public void PublishTagGroupDeal()
        {
            NetMessage setreschgmsg = new NetMessage();
            setreschgmsg.Topic = "GroupDealCallBack";
            setreschgmsg.Data = JsonConvert.SerializeObject(STC.Chrome.Plugin.Data.CefHandler.Instance.DictTagGroupDeal);
            setreschgmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            CefHandler.Instance.add(setreschgmsg);
        }

        public void PublishPubJsMessage(string topic, string data)
        {
            NetMessage setreschgmsg = new NetMessage();
            setreschgmsg.Topic = topic;
            setreschgmsg.Data = data;
            setreschgmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            CefHandler.Instance.add(setreschgmsg);
        }


        #region  安全处理CefMD线程,直接发送原始数据
        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private SemaphoreSlim semaphore = new SemaphoreSlim(0, int.MaxValue); // 计数信号量
        private ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
        private bool isStop = false;
        object proto;
        
        private void dealMsg(CancellationToken cancellationToken)
        {
            while (!isStop)
            {
                try
                {
                    // 使用非阻塞轮询，减少线程切换开销
                    semaphore.Wait(cancellationToken); // 等待新数据到来

                    if (queue.TryDequeue(out proto))
                    {
                        process(proto);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }
        }

        public void process(object t)
        {
            string topic = (t as NetMessage).Topic;

            _CacheSubscribeResult[topic] = t;
            // 判断 topic 是否以任意一个字典中的 key 开头
            List<string> toplist = new List<string>();
            foreach (string item in _SubscribeCallback.Keys)
            {
                if (IsMatch(item, topic))
                {
                    toplist.Add(item);
                    break;
                }
            }

            if (toplist.Count>0 )
            {
                foreach (string item in toplist)
                {
                    List<string> lst = new List<string>();
                    _SubscribeCallback.TryGetValue(item, out lst);
                    if (lst != null && lst.Count > 0)
                    {
                        foreach (string wintitle in lst)
                        {
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PushChrome", Info1 = wintitle, Info2 = "pushJS", Info3 = JsonConvert.SerializeObject(t as NetMessage) });
                        }
                    }
                }
            }
        }

        public void add(object t)
        {
            queue.Enqueue(t); // 将数据添加到队列中  
            semaphore.Release(); // 释放信号量，通知处理线程有新数据到来
            if (queue.Count > 100)
            {
                LogHelper.Warn("dispather[{0}] is critical[size:{1}] in CefHandler", this.GetType(), queue.Count);
            }
        }

        #endregion

        #region FinQ 助手窗口管理
        /// <summary>
        /// 只能打开一个窗口
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <param name="win"></param>
        /// <param name="owner"></param>
        public void OpenOnlyOneWindow(string uniqueKey, Window win, Window owner = null)
        {
            if (string.IsNullOrEmpty(uniqueKey) || win == null)
            {
                return;
            }

            lock (lockobj)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    if (_UniqueKeyMapWindowDic[uniqueKey].WindowState == WindowState.Minimized)
                    {
                        _UniqueKeyMapWindowDic[uniqueKey].WindowState = WindowState.Normal;
                    }
                    _UniqueKeyMapWindowDic[uniqueKey].Show();
                    _UniqueKeyMapWindowDic[uniqueKey].Activate();
                }
                else
                {
                    _UniqueKeyMapWindowDic[uniqueKey] = win;
                    _WinHashCodeMapUniqueKeyDic[win.GetHashCode()] = uniqueKey;
                    win.Owner = owner ?? Application.Current.MainWindow;
                    win.Show();
                }
            }
        }

        public void AddWindow(string uniqueKey, Window win, Window owner = null)
        {
            if (string.IsNullOrEmpty(uniqueKey) || win == null)
            {
                return;
            }

            lock (lockobj)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    if (_UniqueKeyMapWindowDic[uniqueKey].WindowState == WindowState.Minimized)
                    {
                        _UniqueKeyMapWindowDic[uniqueKey].WindowState = WindowState.Normal;
                    }
                    _UniqueKeyMapWindowDic[uniqueKey].Show();
                    _UniqueKeyMapWindowDic[uniqueKey].Activate();
                }
                else
                {
                    _UniqueKeyMapWindowDic[uniqueKey] = win;
                    _WinHashCodeMapUniqueKeyDic[win.GetHashCode()] = uniqueKey;
                }
            }
        }

        public void RemoveWindow(string uniqueKey)
        {
            if (string.IsNullOrEmpty(uniqueKey))
            {
                return;
            }

            lock (lockobj)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    Window dummy;
                    _UniqueKeyMapWindowDic.TryGetValue(uniqueKey, out dummy);
                    var hashcode = dummy.GetHashCode();
                    string dummy2;
                    _WinHashCodeMapUniqueKeyDic.TryRemove(hashcode, out dummy2);
                     _UniqueKeyMapWindowDic.TryRemove(uniqueKey, out dummy);
                }
            }
        }

        /// <summary>
        /// 返回打开的存在的窗口
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <returns></returns>
        public object GetExistWin(string uniqueKey)
        {
            if (string.IsNullOrEmpty(uniqueKey) == false)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    return _UniqueKeyMapWindowDic[uniqueKey];
                }
            }

            return null;
        }
        #endregion


        /// <summary>
        /// 判断是否匹配
        /// </summary>
        public bool IsMatch(string pattern, string topic )
        {
            if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(pattern))
                return false;

            // 1. 精确匹配
            if (pattern == topic)
                return true;

            // 2. 前缀匹配: stc.table.iqbiz.*
            if (pattern.EndsWith(".*"))
            {
                var prefix = pattern.Substring(0, pattern.Length - 1); // 去掉 *
                return topic.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            if (pattern.EndsWith(".**"))
            {
                var prefix = pattern.Substring(0, pattern.Length - 2); // 去掉 **
                return topic.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            // 3. 多占位符匹配: stc.ohlc.CDH.{0}.{1}.{2}
            if (pattern.Contains("{"))
            {
                // 把 {0} {1} {2} ... 替换成 (.+)
                string regexPattern = Regex.Escape(pattern)
                                          .Replace("\\{0\\}", "(.+)")
                                          .Replace("\\{1\\}", "(.+)")
                                          .Replace("\\{2\\}", "(.+)")
                                          .Replace("\\{3\\}", "(.+)");
                // 也可以用正则替换所有 {数字}
                regexPattern = Regex.Replace(regexPattern, @"\\\{\d+\\\}", "(.+)");
                regexPattern = "^" + regexPattern + "$";
                return Regex.IsMatch(topic, regexPattern, RegexOptions.IgnoreCase);
            }

            // 4. 正则匹配: stc.ans.monitor.(.+).detail
            if (pattern.Contains("(") || pattern.Contains("[") || pattern.Contains("\\"))
            {
                try
                {
                    return Regex.IsMatch(topic, "^" + pattern + "$", RegexOptions.IgnoreCase);
                }
                catch
                {
                    // pattern不是合法正则时 fallback 精确匹配
                    return false;
                }
            }

            return false;
        }


        #region QB在线客服
        /// <summary>
        /// QB在线客服参数
        /// </summary>
        /// <returns></returns>
        public ChromeWinModel GetQBAssistantModel()
        {
            string sUrl = string.Empty;
            if (LocalEnv.ConfInfo.LoginEnv.ToLower() == "qa")
            {
                sUrl = $"https://qmtest.sobot.com/chat/pc/v6/index.html?sysnum=277f9d03cee3413ea2bab8afee23db1e&channelid=2&from=iframe&params=";
            }
            else
            {
                sUrl = $"https://qbkefu.sobot.com/chat/pc/v6/index.html?sysnum=277f9d03cee3413ea2bab8afee23db1e&channelid=2&from=iframe&params=";
            }

            int w = 510;
            int h = 550;

            string szUrl = sUrl;
            string szParam;
            // 拼接参数
            // https://qmtest.sobot.com/chat/pc/v6/index.html?sysnum=277f9d03cee3413ea2bab8afee23db1e&channelid=2&from=iframe&params={"qmName":"李哲臣","qmOrg":"森浦10","qbAccount":"zhechen","Tel":"12222222222"}&partnerid=1160975144215425024&w=510&h=500
            QbAssistantModel qam = new QbAssistantModel();
            qam.qmName = UrlEncode(LocalEnv.UserInfo.FullName);
            qam.qmOrg = UrlEncode(LocalEnv.UserInfo.CompanyId);
            qam.qbAccount = LocalEnv.UserInfo.UserName;
            qam.Tel = "";

            szParam = $"{CommonUtils.SerializeObject(qam)}&partnerid={LocalEnv.UserInfo.UserId}=&w={w}&h={h}";
            szUrl += szParam;
            // 拼接到 URL
            ChromeWinModel m = new ChromeWinModel();
            m.Height = 550;
            m.Width = 510;
            m.Title = "在线小助手";
            m.Key = "QBOnlineHelp";
            m.Url = szUrl;
            m.UseChromePluginObject = false;
            m.CallBackObjectInstance = null;
            m.IsShowAddressViewSourceDevTools = false;
            return m;
        }

        /// <summary>
        /// URL 编码（兼容 UTF-8 中文、特殊字符）
        /// </summary>
        public string UrlEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (byte b in Encoding.UTF8.GetBytes(value))
            {
                // A-Z, a-z, 0-9 和 -_.~ 不需要编码
                if ((b >= 0x30 && b <= 0x39) || // 0-9
                    (b >= 0x41 && b <= 0x5A) || // A-Z
                    (b >= 0x61 && b <= 0x7A) || // a-z
                    b == 0x2D || b == 0x2E || b == 0x5F || b == 0x7E) // -_.~
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.Append('%').Append(b.ToString("X2"));
                }
            }
            return sb.ToString();
        }


        public string GetBaseAddress(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return string.Empty;

            // 仅处理 http / https
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return string.Empty;

            return uri.GetLeftPart(UriPartial.Authority);
        }
        #endregion
    }
}

using CefSharp;
using CefSharp.DevTools.Accessibility;
using CefSharp.DevTools.CSS;
using CefSharp.DevTools.Network;
using CefSharp.DevTools.Page;
using CefSharp.Internals;
using CefSharp.Wpf;
using com.gateway.log;
using Microsoft.Win32;
using netapi.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Chrome.Plugin.Data;
using STC.Chrome.Plugin.Model;
using STC.Common.Api;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Chrome;
using STC.Common.Model.Drp;
using STC.Common.Model.EMS;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Component;
using STC.Common.ViewModel.Data;
using STC.Frame.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Chrome.Plugin.Demo
{
    public class CefSharpJsHandler
    {
        #region 构造函数
        public static CefSharpJsHandler Instance { get { return instance; } }

        /// <summary>
        /// 单例模式，CefSharpJsHandler实例
        /// </summary>
        private static CefSharpJsHandler instance = new CefSharpJsHandler();

        private ChromiumWebBrowser browser;
        /// <summary>
        /// js直接调用的c#后台演示类
        /// </summary>
        /// <param name="browse"></param>
        public CefSharpJsHandler()
        {
            instance = this;
        }


        public void SetBrowser(ChromiumWebBrowser _browser)
        {
            browser = _browser;
        }

        #endregion

        //// pubClient(String key, String value)
        ////用途：网页发布消息到客户端。
        ////实现：网页可以直接调用 pubClient(key, value) 方法来发送消息。
        ////业务功能：
        ////网页吊起客户端行情小窗
        ////方法调用：pubClient("priceWin", "210210.IB")
        ////方法调用：pubClient("getUserInfo", "")
        ////方法调用：pubClient("getBondsInfo", "190")
        ////方法调用：pubClient("sendNotification", "190")
        ////参数说明：
        ////key: priceWin
        ////value: 债券代码（例如：210210.IB）
        /// <summary>
        /// 网页发布消息到客户端 JavaScript异步调用的函数，骆驼命名法
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string pubClient(string _key, string _value = null)
        {
            string res = string.Empty;
            var t = Task.Run(async () =>
            {
                res = await pubClientTask(_key, _value);
            });

            Task.WaitAll(t);
            return res;
        }

        /// <summary>
        /// 通用异步函数，骆驼命名法
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_value"></param>
        /// <returns></returns>
        private async Task<string> pubClientTask(string _key, string _value)
        {
            string reply = $"pubClient(\"{_key}\",\"{_value ?? ""}\")";
            LogHelper.Info($"Receive pubClient Event From Chrome:{reply}");
            await Task.Run(() =>
            {
                switch (_key)
                {
                    case "priceWin":
                        //打开行情小窗
                        if (STC.Common.Utils.CommonUtils.IsBDBAppName())
                        {
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBDBQuotesMiniWindows", Info1 = _value });
                        }
                        else if (STC.Common.Utils.CommonUtils.IsIDBAppName())
                        {
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBorkerQuotesMiniWindows", Info1 = _value });
                        }
                        else
                        {
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupSTCQuotesMiniWindows", Info1 = _value });
                        }

                        reply = getUserNameSID(reply, false);

                        break;
                    case "getUserInfo":
                        //获取用户登录名和密码
                        reply = getUserNameSID(reply, true);

                        break;
                    case "getBondsInfo":
                        //获取债券信息
                        reply = getBondsInfo(_value);

                        break;
                    case "sendNotification":
                        //发布系统消息  {"id":"Msg0001","title":"测试标题","content":"这是测试内容"}
                        if (JsonSplit.IsJson(_value))
                        {
                            AlertMessageContent amc = Newtonsoft.Json.JsonConvert.DeserializeObject<AlertMessageContent>(_value);
                            if (string.IsNullOrEmpty(amc.id) || string.IsNullOrEmpty(amc.title) || string.IsNullOrEmpty(amc.content))
                            {
                                reply = getUserNameSID(reply, false, "-2", "id,title,content id,title,content cannot be empty.");
                                return;
                            }

                            reply = getUserNameSID(reply, false);

                            string str = GetTestAlertMsg("ST", amc.id, "N", "2", amc.title, amc.content);
                            PublishSystemMsg(str);
                        }

                        break;
                    case "getTestAlertMsg":
                        //获取测试消息
                        reply = GetSimpleAlertmsg("Msg0001", "测试标题", "这是测试内容");

                        break;
                    case "requestsync":
                        //异步请求消息
                        _value = _value.Trim().Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        
                        if (string.IsNullOrEmpty(_value) || !JsonSplit.IsJson(_value))
                        {
                            reply = getUserNameSID(reply, false, "-1", "Incorrect value");
                        }
                        else
                        {
                            ReqGetWayModel gmw = JsonConvert.DeserializeObject<ReqGetWayModel>(_value);
                            object data = netapi.Api.ApiManager.Instance.GetDefaultApi().Request<object>(gmw.srvid, gmw.topic, gmw.paras);
                            if (data != null)
                            {
                                reply = JsonConvert.SerializeObject(data);
                            }
                            else
                            {
                                reply = getUserNameSID(reply, false, "-1", "Request failed or no data returned.");
                            }
                        }

                        break;
                    case "getTestRequestsyncMsg":
                        //获取测试Getway消息
                        SystemParameterReq req = new SystemParameterReq();
                        req.ParamKey = CommonUtils.PARAM_KEY_COPY;
                        req.Username = LocalEnv.UserInfo.UserName;
                        reply = GetReqTestRequestsyncMsg("AdminSvr", "stc.admin.systemPara", req);

                        break;
                    case "subscribe":
                        //订阅请求消息
                        //subscribesync {"svrid":"AdminSvr","topic":"stc.admin.systemPara","paras":"市场最优"};
                        _value = _value.Trim().Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        if (string.IsNullOrEmpty(_value) || !JsonSplit.IsJson(_value))
                        {
                            reply = getUserNameSID(reply, false, "-1", "Incorrect value");
                        }
                        else
                        {
                            GetWayModel gmw = JsonConvert.DeserializeObject<GetWayModel>(_value);

                            CefHandler.Instance.subscribe(gmw.srvid, gmw.topic, gmw.paras);
                            reply = getUserNameSID(reply, false, "0000", "Subscribe successfully.");
                        }

                        break;
                    case "unsubscribe":
                        //unsubscribesync { "svrid":"AdminSvr","topic":"stc.admin.systemPara","paras":"市场最优"};
                        _value = _value.Trim().Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        if (string.IsNullOrEmpty(_value) || !JsonSplit.IsJson(_value))
                        {
                            reply = getUserNameSID(reply, false, "-1", "Incorrect value");
                        }
                        else
                        {
                            GetWayModel gmw = JsonConvert.DeserializeObject<GetWayModel>(_value);
                            CefHandler.Instance.unsubscribe(gmw.srvid, gmw.topic, gmw.paras);

                            reply = getUserNameSID(reply, false, "0000", "UnSubscribe successfully.");
                        }

                        break;
                    case "getTestSubscribesyncMsg":
                        //获取订阅Getway测试消息
                        reply = GetTestSubscribeRequestsyncMsg("MDSvr", "stc.md.ct.230026IB=ct", "C# Js Call");
                        break;
                    case "getTestUnSubscribesyncMsg":
                        //获取退订Getway测试消息
                        reply = GetTestSubscribeRequestsyncMsg("MDSvr", "stc.md.ct.230026IB=ct", "C# Js Call");
                        break;
                    case "getGroupDeal":
                        reply = getGroupDeal(_value);
                        if (string.IsNullOrEmpty(reply))
                        {
                            //获取测试消息
                            Dictionary<string, string> resdictres = new Dictionary<string, string>();
                            resdictres.Add(_value, "0");
                            reply = JsonConvert.SerializeObject(resdictres.ToList());
                        }

                        break;
                    case "getGroupDealMsg":
                        //获取测试消息
                        GroupDealContent gdc = new GroupDealContent() { Key = "Test", Value = "1" };
                        reply = JsonConvert.SerializeObject(gdc);

                        break;
                    case "setGroupDeal":
                        //设置最优组  {"Key":"Test","Value":"1"}
                        if (setGroupDeal(_value))
                            reply = getUserNameSID(reply, false);
                        else
                            reply = getUserNameSID(reply, false, "-1", "Incorrect key");

                        break;
                    case "showRecognizeIntentionWin":
                        //弹出批量识别窗口
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            if (!string.IsNullOrEmpty(_value))
                            {
                                try
                                {
                                    Clipboard.SetText(_value);
                                }
                                catch { }
                            }

                            STC.Common.Data.DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(true);
                            reply = getUserNameSID(reply, false);
                        }));
                        break;
                    case "subscribeTagGroup":
                        //订阅客户端订阅组更新
                        _value = _value.Trim().Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        if (!string.IsNullOrWhiteSpace(_value))
                        {
                            if (!CefHandler.Instance.DictSubscribeTagGroup.ContainsKey(_value))
                            {
                                CefHandler.Instance.DictSubscribeTagGroup.TryAdd(_value, "");
                            }

                            reply = getUserNameSID(reply, false, "0000", "Subscribe to tag group successfully.");
                        }
                        else
                        {
                            reply = getUserNameSID(reply, false, "-1", "Incorrect value");
                        }

                        // Msg 为 * 表示需要先更新订阅组列表，然后再通知网页端订阅组更新
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "UpdateQuoteTagGroup", Msg = "*" });

                        break;
                    case "unsubscribeTagGroup":
                        //退订客户端订阅组更新
                        _value = _value.Trim().Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        if (!string.IsNullOrWhiteSpace(_value))
                        {
                            string tmpvalue = "";
                            if (CefHandler.Instance.DictSubscribeTagGroup.TryRemove(_value.Trim(), out tmpvalue))
                                reply = getUserNameSID(reply, false, "0000", "Already unsubscribed to the tag group.");
                            else
                                reply = getUserNameSID(reply, false, "-1", "Incorrect value");
                        }
                        else
                        {
                            reply = getUserNameSID(reply, false, "-1", "Incorrect value");
                        }

                        break;
                    case "publishTagGroup":
                        //JS网页端通知客户端订阅组更新
                        reply = getUserNameSID(reply, false, "0000", "Update tag group notification has been published.");
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "*", Info2 = "publishTagGroup", Info3 = "" });

                        break;
                    case "refreshClientBondGroup":
                        // 通知客户端的债券管理更新数据
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "refreshClientBondGroup", Msg = "*" });
                        reply = getUserNameSID(reply, false, "0000", "refreshClientBondGroup");
                        
                        break;
                      
                    case "fq_page_ready":
                        //处理Ready消息
                        CefHandler.Instance.IsFinQWebReady = true;
                        NetMessage resmsg = new NetMessage();
                        resmsg.Topic = "finQrInit";
                        resmsg.Data = "{}";
                        resmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                        CefHandler.Instance.add(resmsg);
                        reply = getUserNameSID(reply, false);
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQChangeBond", Info3 = "" });
                        break;

                    case "fq_token_bind":
                        //获取token todo...  {"token":"hXKHkuE+utcfME6Rf1w0YGq5NCZY7lllF6caaQ3GSOJb1e/qmPmPS9C+tqMtwtCnUJyUV/r2YnCclFGdpg6dLhLoVSvTqQRd0RYm"}
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "GetToken", Info3 = "" });
                        reply = getUserNameSID(reply, false);

                        break;

                    case "fq_page_close":
                        //'["fq_page_close", [{"req": "ZnEudG9rZW4uYmluZA==", "callback": "ZnFfdG9rZW5fYmluZF9jYWxsYmFjaw=="}]]'
                        //隐藏窗体
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "HideWebFQ", Info3 = "" });
                        reply = getUserNameSID(reply, false);
                        break;
                    case "fq_page_reset":
                        //重置 fq_page_ready 状态
                        CefHandler.Instance.IsFinQWebReady = false;
                        reply = getUserNameSID(reply, false);
                        break;
                    case "fq_getFQSetAction":
                        //内部获取盘口数据
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
                        reply = getUserNameSID(reply, false);
                        break;
                    case "fq_getFQChangeBond":
                        //内部获取全量数据
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQChangeBond", Info3 = "" });
                        reply = getUserNameSID(reply, false);
                        break;
                    case "fq_getRemainTime":
                        //内部获取token剩余时间
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "GetRemainTime", Info3 = "" });
                        reply = getUserNameSID(reply, false);
                        break;
                    case "showIssueRecogizationWin":
                        //弹出发行人识别
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            if (!string.IsNullOrEmpty(_value))
                            {
                                try
                                {
                                    ClipboardHelper.CopyToClipboard(_value);
                                }
                                catch { }
                            }
                           
                            reply = getUserNameSID(reply, false);
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "CC",Info2 = "OpenIssueRecogizationWin", Info3 = $"{_value}" });
                        }));
                        break;
                    case "getIssueRecogizationResult":
                        //获取发行人识别结果
                        if (string.IsNullOrWhiteSpace(_value))
                            reply = getUserNameSID(reply, false, "-1", "Incorrect search key");
                        else
                            reply = getIssueRecogizationResult(_value);
                        break;
                    default:
                        //其他的key值
                        reply = getUserNameSID(reply, false, "-1", "Incorrect key");
                        break;
                }

                LogHelper.Info($"{reply}");
            });

            return reply;
        }
        #region  获取发行人识别结果（支持分页）

        /// <summary>
        /// 获取发行人识别结果（支持分页）
        /// </summary>
        /// <param name="parameters">JSON参数格式: {"code":"搜索关键词","pageIndex":0,"pageSize":200}
        /// pageIndex: 页码（从0开始，0代表第1页），默认为0
        /// pageSize: 每页条数，默认为200
        /// </param>
        /// <returns></returns>
        private string getIssueRecogizationResult(string parameters)
        {
            try
            {
                // 默认参数
                string code = string.Empty;
                int pageIndex = 0;
                int pageSize = 200;

                // 解析参数
                if (!string.IsNullOrWhiteSpace(parameters))
                {
                    // 尝试作为JSON对象解析
                    if (JsonSplit.IsJson(parameters))
                    {
                        JObject jObj = JObject.Parse(parameters);
                        JToken codeToken = jObj["code"];
                        code = codeToken != null ? codeToken.ToString() : string.Empty;
                        
                        JToken pageIndexToken = jObj["pageIndex"];
                        string pageIndexStr = pageIndexToken != null ? pageIndexToken.ToString() : "0";
                        int idx = 0;
                        if (int.TryParse(pageIndexStr, out idx))
                            pageIndex = Math.Max(0, idx);
                        
                        JToken pageSizeToken = jObj["pageSize"];
                        string pageSizeStr = pageSizeToken != null ? pageSizeToken.ToString() : "200";
                        int size = 200;
                        if (int.TryParse(pageSizeStr, out size))
                            pageSize = Math.Max(1, size);
                    }
                    else
                    {
                        // 直接作为搜索关键词处理
                        code = parameters;
                    }
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    return getUserNameSID(string.Empty, false, "-1", "Incorrect search key");
                }

                // 调用函数获取完整结果（精确搜索）
                List<string> failedItems = new List<string>();
                CustomCollection<IssueRecogizeItem> allResults = Common.Utils.SqlLite.TagsSqliteManager.Instance.RecognizeIssuerItems(code, true, out failedItems);

                if (allResults == null || allResults.Count == 0)
                {
                    List<string> emptyFailedItems = failedItems != null ? failedItems : new List<string>();
                    return JsonConvert.SerializeObject(new PaginatedResult<IssueRecogizeItem>
                    {
                        Items = new List<IssueRecogizeItem>(),
                        TotalCount = 0,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        TotalPages = 0,
                        FailedItems = emptyFailedItems
                    });
                }

                // 计算分页
                int totalCount = allResults.Count;
                int totalPages = (totalCount + pageSize - 1) / pageSize;

                // 验证页码
                if (pageIndex >= totalPages && totalPages > 0)
                    pageIndex = totalPages - 1;

                // 获取当前页数据
                int skipCount = pageIndex * pageSize;
                var pageData = allResults.Skip(skipCount).Take(pageSize).ToList();

                // 对当前页的数据进行序号标记（根据页码累加）
                int startNo = pageIndex * pageSize + 1;
                for (int i = 0; i < pageData.Count; i++)
                {
                    pageData[i].No = startNo + i;
                }

                // 返回分页结果
                List<string> resultFailedItems = failedItems != null ? failedItems : new List<string>();
                var paginatedResult = new PaginatedResult<IssueRecogizeItem>
                {
                    Items = pageData,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    FailedItems = resultFailedItems
                };

                return JsonConvert.SerializeObject(paginatedResult, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("getIssueRecogizationResult error: {0}", ex.Message));
                return getUserNameSID(string.Empty, false, "-1", string.Format("Error: {0}", ex.Message));
            }
        }

        #endregion

        #region Js调用C#方法 在 JS 端主动拉行情或异步等待推送。
        private readonly ConcurrentDictionary<string, List<object>> _channels = new ConcurrentDictionary<string, List<object>>();
        /// <summary>
        /// JS端主动订阅C#回调函数
        /// </summary>
        /// <param name="callback"></param>
        public void subscribeCallback(object callback, string title = null)
        {
            List<object> list;
            string channel =  (string.IsNullOrEmpty(title as string) && callback.GetType()!= typeof(string)) ? "MDSvr" : title as string;
            lock (_channels) // Assuming _channels is an object that can be locked
            {
                if (!_channels.TryGetValue(channel, out list))
                {
                    list = new List<object>();
                    if (!list.Contains(callback as object))
                        list.Add(callback);
                    _channels.TryAdd(channel, list);
                }
                else
                {
                    if (!list.Contains(callback))
                        list.Add(callback as object);
                    _channels.AddOrUpdate(channel, list, (k, v) => list);
                }
            }
        }

        /// <summary>
        /// 推送消息到JS端
        /// </summary>
        /// <param name="data"></param>
        /// <param name="svrid"></param>
        public void OnPushJS(string data, string svrid="MDSvr")
        {
            try
            {
                List<object> calllist = GetCallbacks(svrid);
                if (calllist.Count == 0)
                {
                    calllist = GetCallbacks("MDSvr");
                }

                if (calllist.Count == 0)
                {
                    calllist.Add("window.cefCallback");
                }

                foreach (var callback in calllist)
                {
                    Cef.UIThreadTaskFactory.StartNew(async () =>
                    {
                        try
                        {
                            if (callback.GetType() == typeof(string))
                            {
                                string escaped = data.Replace("\\", "\\\\").Replace("\"", "\\\"");
                                string js = $"{callback as string}(\"{escaped}\");";
                                browser.ExecuteScriptAsync(js);
                            }
                            else
                            {
                                if ( ((IJavascriptCallback)callback).CanExecute)
                                    await ((IJavascriptCallback)callback)?.ExecuteAsync(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error($"推送失败: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"推送失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取订阅的回调函数列表 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public List<object> GetCallbacks(string channel = "MDSvr")
        {
            List<object> list = null;
            if (_channels.TryGetValue(channel, out list))
            {
                return new List<object>(list); // copy
            }
            return new List<object>();
        }
        #endregion

        #region 本地函数
        /// <summary>
        /// 发布系统消息
        /// </summary>
        /// <param name="content"></param>
        private void PublishSystemMsg(string content)
        {
            if (JsonSplit.IsJson(content))
            {
                Common.Model.Admin.TTbookSystemMessage messageContent = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Model.Admin.TTbookSystemMessage>(content);
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "AppSimulateMsg", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(new List<Common.Model.Admin.TTbookSystemMessage>() { messageContent }) });
            }
        }

        /// <summary>
        /// 获取用户登录名和密码，骆驼命名法
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private string getUserNameSID(string request, bool returnuserinfo = false, string code = "0000", string msg = "Success")
        {
            UserInfoClassModel ui = new UserInfoClassModel() { UserInfo = returnuserinfo ? LocalEnv.UserInfo : null, Code = code, Msg = msg };
            if (ui.UserInfo != null)
            {
                // 不改变原值
                string temp = JsonConvert.SerializeObject(ui, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                ui = JsonConvert.DeserializeObject<UserInfoClassModel>(temp);
                ui.UserInfo.Password = null;
                ui.UserInfo.termids = null;
            }

            // 序列化时忽略空值
            return JsonConvert.SerializeObject(ui, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// 搜索债券，骆驼命名法
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string getBondsInfo(string code)
        {
            string res = string.Empty;
            string txt = code.Replace("'", "");
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            if (txt.Contains(" "))
            {
                string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string str in arr)
                {
                    List<SimpleBondsInfoPo> selectDatas = CommonUtils.GetBondsInfoPo(str, 20);
                    result.AddRange(selectDatas);
                }
            }
            else
                result = CommonUtils.GetBondsInfoPo(txt.Trim(), 20);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 查询市场最优是否有最优价
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        private string getGroupDeal(string groupname)
        {
            string res = "0";
            groupname = groupname.Replace("\t", "").Trim();
            if (!string.IsNullOrEmpty(groupname) && groupname != "*")
            {
                if (CefHandler.Instance.DictTagGroupDeal.TryGetValue(groupname, out res))
                {
                    res = string.IsNullOrEmpty(res) ? "0" : res;
                    Dictionary<string, string> dictres = new Dictionary<string, string>();
                    dictres.Add(groupname, res);
                    return JsonConvert.SerializeObject(dictres.ToList());
                }
            }
            else
            {
                return JsonConvert.SerializeObject(CefHandler.Instance.DictTagGroupDeal.ToList());
            }

            return res;
        }

        /// <summary>
        /// 设置市场最优是否有最优价
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool setGroupDeal(string content)
        {
            bool res = false;
            if (JsonSplit.IsJson(content))
            {
                GroupDealContent gdc = JsonConvert.DeserializeObject<GroupDealContent>(content);
                if (!string.IsNullOrEmpty(gdc.Key))
                {
                    CefHandler.Instance.DictTagGroupDeal.AddOrUpdate(gdc.Key, gdc.Value, (k, v) => gdc.Value);
                    CefHandler.Instance.PublishTagGroupDeal();
                    res = true;
                }
            }

            return res;
        }

        #endregion

        #region 私有Funciton
        /// <summary>
        /// 获取简单的消息提示
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetSimpleAlertmsg(string id, string title, string content)
        {
            AlertMessageContent messageContent = new AlertMessageContent();
            messageContent.id = id;
            messageContent.title = title;
            messageContent.content = content;
            return JsonConvert.SerializeObject(messageContent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// 获取测试消息
        /// </summary>
        /// <param name="contentType">内容类别 ST,TABLESVR_ORDER_UNREF,PM,ER,ANS,IT,DQ,DQB,OD,BDB,Quant_Focus_Tag,TableSvr_FOCUS_TAG,CUST_MD</param>
        /// <param name="id">消息ID</param>
        /// <param name="level">紧急程度 C-紧急 A-重要 N-一般</param>
        /// <param name="msgtype">消息类型 1消息通知,2待办事项</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns>Json String</returns>
        private string GetTestAlertMsg(string contentType, string id, string level, string msgtype, string title, string content)
        {
            Common.Model.Admin.TMessageContent messageContent = new Common.Model.Admin.TMessageContent();
            messageContent.contentType = contentType;  //ST,TABLESVR_ORDER_UNREF,PM,ER,ANS,IT,DQ,DQB,OD,BDB,Quant_Focus_Tag,TableSvr_FOCUS_TAG,CUST_MD
            messageContent.id = id;
            messageContent.content = content; //消息内容
            messageContent.title = title;  //消息标题
            messageContent.type = level;//紧急程度 C-紧急 A-重要 N-一般
            messageContent.needSend = true; //是否需要发送
            messageContent.show = true; //是否显示

            Common.Model.Admin.TTbookSystemMessage tbookSystemMessage = new Common.Model.Admin.TTbookSystemMessage();
            tbookSystemMessage.id = id; //消息ID
            tbookSystemMessage.type = msgtype;//  消息类型 1消息通知,2待办事项
            tbookSystemMessage.username = "*";//LocalEnv.UserInfo.UserName;
            tbookSystemMessage.content = JsonConvert.SerializeObject(messageContent);
            tbookSystemMessage.readflag = "";
            tbookSystemMessage.closeby = LocalEnv.UserInfo.UserName;
            tbookSystemMessage.delflag = "0"; //0显示 1不显示

            return JsonConvert.SerializeObject(tbookSystemMessage, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// 获取网关请求消息
        /// </summary>
        /// <param name="svrId"></param>
        /// <param name="topic"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private string GetReqTestRequestsyncMsg(string svrId, string topic, object paras)
        {
            ReqGetWayModel getWayModel = new ReqGetWayModel();
            getWayModel.srvid = svrId; //服务端ID
            getWayModel.topic = topic; //消息主题
            getWayModel.paras = paras; //客户端参数
            return JsonConvert.SerializeObject(getWayModel, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// 获取网关订阅消息
        /// </summary>
        /// <param name="svrId"></param>
        /// <param name="topic"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private string GetTestSubscribeRequestsyncMsg(string svrId, string topic, object paras)
        {
            GetWayModel getWayModel = new GetWayModel();
            getWayModel.srvid = svrId; //服务端ID
            getWayModel.topic = topic; //消息主题
            getWayModel.paras = paras; //客户端参数
            return JsonConvert.SerializeObject(getWayModel, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }



        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                STC.Common.View.UI.MessageBoxGUI me = new STC.Common.View.UI.MessageBoxGUI();
                me.MOK += Me_MOK;
                me.HideCancelButton();
                me.setWarnMsg(message);
                me.ShowDialog();
            }));
        }

        private void Me_MOK()
        {
            try
            {
                LogHandler.Info("确定关闭程序");
                Application.Current.Shutdown();
            }
            catch
            {
                Environment.Exit(0);
            }
        }
        #endregion
    }

    #region 公共Class
    public class AlertMessageContent
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
    }

    public class GroupDealContent
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// 分页结果模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// 当前页数据列表
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码（从0开始）
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 未能识别的项目列表（仅用于发行人识别结果）
        /// </summary>
        public List<string> FailedItems { get; set; }
    }

    public class ReqGetWayModel
    {
        /// <summary>
        /// srvid 服务端ID
        /// </summary>
        public string srvid { get; set; } = "AdminSvr";
        /// <summary>
        /// topic 消息主题
        /// </summary>
        public string topic { get; set; } = "queryHomeGroup";

        /// <summary>
        /// client paras 客户端参数 
        /// </summary>
        public object paras { get; set; }
    }

    public class GetWayModel
    {
        private string _prefix = "";
        /// <summary>
        /// srvid 服务端ID
        /// </summary>
        public string srvid { get; set; } = "AdminSvr";
        /// <summary>
        /// topic 消息主题
        /// </summary>
        public string topic { get; set; } = "queryHomeGroup";

        /// <summary>
        /// client paras 客户端参数 
        /// </summary>
        public object paras { get; set; }
    }
    #endregion
}
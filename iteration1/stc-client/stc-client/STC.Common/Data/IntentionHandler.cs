using com.gateway.log;
using STC.Common.Api;
using STC.Common.Data.DataModel;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace STC.Common.Data
{
    public class IntentionHandler
    {
        // 订阅意向单识别
        private readonly Object m_OnRecognitionEventLock = new Object();
        private Action<IntentionQuoteInfo> _OnRecognition;
        public event Action<IntentionQuoteInfo> OnRecognition
        {
            add
            {
                lock (m_OnRecognitionEventLock)
                {
                    _OnRecognition += value;
                }
            }
            remove
            {
                lock (m_OnRecognitionEventLock)
                {
                    _OnRecognition -= value;
                }
            }
        }

        // 订阅原始请求
        private readonly Object m_OnOriginEventLock = new Object();
        private Action<IntentionQuoteInfo> _OnOrigins;
        public event Action<IntentionQuoteInfo> OnOrigins
        {
            add
            {
                lock (m_OnOriginEventLock)
                {
                    _OnOrigins += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheOrigins)
                    {
                        value?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnOriginEventLock)
                {
                    _OnOrigins -= value;
                }
            }
        }

        /// <summary>
        /// 缓存原始请求(订阅)
        /// </summary>
        private ConcurrentDictionary<string, IntentionQuoteInfo> _CacheOrigins = new ConcurrentDictionary<string, IntentionQuoteInfo>();

        // 订阅意向单
        private readonly Object m_OnIntentionQuoteEventLock = new Object();
        private Action<IntentionQuoteInfo> _OnIntentionQuotes;
        public event Action<IntentionQuoteInfo> OnIntentionQuotes
        {
            add
            {
                lock (m_OnIntentionQuoteEventLock)
                {
                    _OnIntentionQuotes += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheIntentionQuotes)
                    {
                        value?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnIntentionQuoteEventLock)
                {
                    _OnIntentionQuotes -= value;
                }
            }
        }

        /// <summary>
        /// 缓存意向(订阅)
        /// </summary>
        private ConcurrentDictionary<string, IntentionQuoteInfo> _CacheIntentionQuotes = new ConcurrentDictionary<string, IntentionQuoteInfo>();

        // 意向订单
        private readonly Object m_OnIntentionOrderEventLock = new Object();
        private Action<IntentionOrderInfo> _OnIntentionOrders;
        public event Action<IntentionOrderInfo> OnIntentionOrders
        {
            add
            {
                lock (m_OnIntentionOrderEventLock)
                {
                    _OnIntentionOrders += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheIntentionOrders)
                    {
                        value?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnIntentionOrderEventLock)
                {
                    _OnIntentionOrders -= value;
                }
            }
        }

        /// <summary>
        /// 缓存我的意向订单(订阅)
        /// </summary>
        private ConcurrentDictionary<string, IntentionOrderInfo> _CacheIntentionOrders = new ConcurrentDictionary<string, IntentionOrderInfo>();

        // 意向成交
        private readonly Object m_OnIntentionExecEventLock = new Object();
        private Action<IntentionExecInfo> _OnIntentionExecs;
        public event Action<IntentionExecInfo> OnIntentionExecs
        {
            add
            {
                lock (m_OnIntentionExecEventLock)
                {
                    _OnIntentionExecs += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheIntentionExecs)
                    {
                        value?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnIntentionExecEventLock)
                {
                    _OnIntentionExecs -= value;
                }
            }
        }

        /// <summary>
        /// 缓存我的意向成交(订阅)
        /// </summary>
        private ConcurrentDictionary<string, IntentionExecInfo> _CacheIntentionExecs = new ConcurrentDictionary<string, IntentionExecInfo>();


        // 意向历史成交
        private readonly Object m_OnIntentionHistoryExecEventLock = new Object();
        private Action<IntentionExecInfo> _OnIntentionHistoryExecs;
        public event Action<IntentionExecInfo> OnIntentionHistoryExecs
        {
            add
            {
                lock (m_OnIntentionHistoryExecEventLock)
                {
                    _OnIntentionHistoryExecs += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheIntentionHistoryExecs)
                    {
                        value?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnIntentionHistoryExecEventLock)
                {
                    _OnIntentionHistoryExecs -= value;
                }
            }
        }

        /// <summary>
        /// 缓存我的意向成交(订阅)
        /// </summary>
        private ConcurrentDictionary<string, IntentionExecInfo> _CacheIntentionHistoryExecs = new ConcurrentDictionary<string, IntentionExecInfo>();

        /// IntentionHandler实例
        /// </summary>
        private static IntentionHandler server = new IntentionHandler();
        public static IntentionHandler Instance { get { return server; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        private IntentionHandler()
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
                TableHandler.Instance.SubscribeRecognization(new string[] { $"{ApiConsts.Topic_stc_table_ir}{LocalEnv.UserInfo.UserName}" }, Instance_OnRecognition);
                //TBookHandler.Instance.SubscribeIntention(new string[] { $"{ApiConsts.Topic_stc_tbook_iq}{LocalEnv.UserInfo.UserName}.**" }, Instance_OnIntention);
                TableHandler.Instance.SubscribeOrigin(new string[] { $"{ApiConsts.Topic_stc_table_io}**" }, Instance_OnOrigins);
                TableHandler.Instance.SubscribeIntention(new string[] { $"{ApiConsts.Topic_stc_table_iq}{LocalEnv.UserInfo.UserName}" }, Instance_OnIntention);

                RequestOrigins();

                RequestIntentionQuotes();

                // 意向订单
                TableHandler.Instance.SubscribeIntentionOrder(Instance_OnIntentionOrder);

                //RequestIntentionOrder();

                // 意向成交
                TableHandler.Instance.SubscribeIntentionExec(Instance_OnIntentionExec);

                //RequestIntentionExec();

                RequestHistoryIntentionExec();

                isInit = true;
            }
        }

        private void RequestIntentionOrder()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionOrderRequest request = new IntentionOrderRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.date = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");

                    ///获取订单 
                    var reply = TableHandler.Instance.QueryIntentionOrder(request);


                    if (reply != null && reply.Datas != null)
                    {
                        foreach (IntentionOrderInfo obj in reply.Datas)
                        {
                            Instance_OnIntentionOrder(obj);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        private void Instance_OnIntentionOrder(IntentionOrderInfo obj)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                if (_CacheIntentionOrders.ContainsKey(obj.orderid))
                {
                    if (_CacheIntentionOrders[obj.orderid].updatetime.CompareTo(obj.updatetime) <= 0)
                        _CacheIntentionOrders[obj.orderid] = obj;
                }
                else
                    _CacheIntentionOrders[obj.orderid] = obj;
                _OnIntentionOrders?.Invoke(obj);
            });
        }

        /// <summary>
        /// 所有近一个月的历史数据
        /// </summary>
        private void RequestHistoryIntentionExec()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionExecRequest request = new IntentionExecRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.startDate = DateTimeEx.ServerTime.AddDays(-30).ToString("yyyy-MM-dd");
                    request.endDate = DateTimeEx.ServerTime.AddDays(-1).ToString("yyyy-MM-dd"); ;
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = TableHandler.Instance.QueryIntentionExec(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            foreach (IntentionExecInfo obj in reply.Datas)
                            {
                                Instance_OnIntentionHistoryExec(obj);
                            }
                            if (reply.Datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        private void Instance_OnIntentionHistoryExec(IntentionExecInfo obj)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                if (_CacheIntentionHistoryExecs.ContainsKey(obj.execid))
                {
                    if (_CacheIntentionHistoryExecs[obj.execid].updatetime.CompareTo(obj.updatetime) <= 0)
                        _CacheIntentionHistoryExecs[obj.execid] = obj;
                }
                else
                    _CacheIntentionHistoryExecs[obj.execid] = obj;
                _OnIntentionHistoryExecs?.Invoke(obj);
            });
        }

        private void RequestIntentionExec()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionExecRequest request = new IntentionExecRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.date = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = TableHandler.Instance.QueryIntentionExec(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            foreach (IntentionExecInfo obj in reply.Datas)
                            {
                                Instance_OnIntentionExec(obj);
                            }
                            if (reply.Datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        private void Instance_OnIntentionExec(IntentionExecInfo obj)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                if (_CacheIntentionExecs.ContainsKey(obj.execid))
                {
                    //if (_CacheIntentionExecs[obj.execid].updatetime.CompareTo(obj.updatetime) <= 0)
                        _CacheIntentionExecs[obj.execid] = obj;
                }
                else
                    _CacheIntentionExecs[obj.execid] = obj;
                _OnIntentionExecs?.Invoke(obj);
            });
        }

       
        /// <summary>
        /// 清理回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("OrdersHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                _CacheIntentionQuotes.Clear();
                _CacheIntentionOrders.Clear();
                _CacheIntentionExecs.Clear();
                _CacheIntentionHistoryExecs.Clear();

                isInit = false;

                init();
            }
        }
        public void RecognizeIntentionQuote(string content)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    RecognizeIntentionQuoteRequest request = new RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = content;

                    ///获取订单 
                    RecognizeIntentionQuoteReplyPo reply = TableHandler.Instance.RecognizeIntentionQuote(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                foreach (IntentionQuoteInfo obj in reply.Datas)
                                {
                                    _CacheIntentionQuotes[obj.intentionId] = obj;
                                    _OnIntentionQuotes?.Invoke(obj);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        List<IntentionQuoteInfo> quoteList = new List<IntentionQuoteInfo>();
        public void ImportIntentionQuote(string content)
        {

            try
            {
                RecognizeIntentionQuoteRequest request = new RecognizeIntentionQuoteRequest();
                request.userName = LocalEnv.UserInfo.UserName;
                request.content = content;

                lock (quoteList)
                {
                    quoteList.Clear();
                }
                ///获取订单 
                RecognizeIntentionQuoteReplyPo reply = TableHandler.Instance.RecognizeIntentionQuote(request);

                if (reply == null)
                {
                    CommonUtils.Prompt2Second("警告", $"报价识别失败");
                }
                else
                {
                    if (!reply.IsSuccess())
                    {
                        CommonUtils.Prompt2Second("警告", reply.Msg);
                    }
                    else
                    {
                        // 等待是被数据返回
                        int i = 0;
                        while (i < 5)
                        {
                            int count = quoteList.Count;
                            Thread.Sleep(500);

                            if (count != quoteList.Count)
                            {
                                i = 0;
                            }
                            else
                            {
                                i++;
                            }
                        }

                        // 插入数据
                        InsertIntentionQuoteReq insertReq = new InsertIntentionQuoteReq();
                        insertReq.quoteList = quoteList;
                        InsertIntentionQuote(insertReq);


                        //if (reply.Datas != null)
                        //{
                        //    foreach (IntentionQuoteInfo obj in reply.Datas)
                        //    {
                        //        _CacheIntentionQuotes[obj.intentionId] = obj;
                        //        _OnIntentionQuotes?.Invoke(obj);
                        //    }
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestOrders", e);
            }

        }

        public void RequestOrigins()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionQuoteRequest request = new IntentionQuoteRequest();
                    //request1.userName = LocalEnv.UserInfo.UserName;
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.date = today;
                    int index = 0;
                    request.pageSize = 2000;

                    ///获取订单 
                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = TableHandler.Instance.QueryOriginRequest(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            foreach (IntentionQuoteInfo obj in reply.Datas)
                            {
                                _CacheOrigins[obj.intentionId] = obj;
                                _OnOrigins?.Invoke(obj);
                            }
                            if (reply.Datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrigins", e);
                }
            });
        }

        public void Instance_OnOrigins(IntentionQuoteInfo obj)
        {
            //if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName)
            //{
            //    return;
            //}

            lock (string.Intern(obj.intentionId))
            {
                _CacheOrigins.AddOrUpdate(obj.intentionId, obj, (key, value) => obj);
            }

            _OnOrigins?.Invoke(obj);
        }

        public void RequestIntentionQuotes()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionQuoteRequest request = new IntentionQuoteRequest();
                    //request.userName = LocalEnv.UserInfo.UserName;
                    request.status = "2,4"; // 2:报价中; 4.已暂停
                    int index = 0;
                    request.pageSize = 2000;

                    ///获取订单 
                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = TableHandler.Instance.QueryIntentionQuote(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            foreach (IntentionQuoteInfo obj in reply.Datas)
                            {
                                _CacheIntentionQuotes[obj.intentionId] = obj;
                                _OnIntentionQuotes?.Invoke(obj);
                            }
                            if (reply.Datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }

                    IntentionQuoteRequest request1 = new IntentionQuoteRequest();
                    //request1.userName = LocalEnv.UserInfo.UserName;
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request1.date = today;
                    request1.status = "5"; // 0:待处理 3:已忽略; 5:有完成
                    request1.pageSize = 2000;

                    ///获取订单 
                    flag = true;
                    while (flag)
                    {
                        request1.pageNum = index;
                        ///获取订单 
                        var reply = TableHandler.Instance.QueryIntentionQuote(request1);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            foreach (IntentionQuoteInfo obj in reply.Datas)
                            {
                                _CacheIntentionQuotes[obj.intentionId] = obj;
                                _OnIntentionQuotes?.Invoke(obj);
                            }
                            if (reply.Datas.Count < request1.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        public ConfirmIntentionQuoteReplyPo ConfirmIntentionQuote(ConfirmIntentionQuoteRequest request)
        {
            return TableHandler.Instance.ConfirmIntentionQuote(request);
        }

        public Task<bool> InsertIntentionQuote(InsertIntentionQuoteReq request)
        {
            return Task.Run(() =>
            {
                bool isSuccess = false;
               if (request.quoteList == null || request.quoteList.Count == 0)
                    return isSuccess;

                PResult result = TableHandler.Instance.InsertIntentionQuote(request);
                if (result == null)
                {
                    CommonUtils.Prompt2Second("警告", $"意向报价添加失败");
                }
                else
                {
                    if (!result.IsSuccess())
                    {
                        CommonUtils.Prompt2Second("警告", result.Msg);
                    }
                    else
                    {
                        isSuccess = true;
                    }
                }
                return isSuccess;
            });
        }

        public Task<bool> UpdateIntentionQuote(UpdateIntentionQuoteRequest request)
        {
            return Task.Run(() =>
            {
                bool isSuccess = false;
                if (string.IsNullOrEmpty(request.intentionId))
                    return isSuccess;

                PResult result = TableHandler.Instance.UpdateIntentionQuote(request);
                if (result == null)
                {
                    CommonUtils.Prompt2Second("警告", $"意向id:[{request.intentionId}]修改失败");
                }
                else
                {
                    if (!result.IsSuccess())
                    {
                        CommonUtils.Prompt2Second("警告", result.Msg);
                    }
                    else
                    {
                        isSuccess = true;
                    }
                }
                return isSuccess;
            });
        }

        public void Instance_OnIntention(IntentionQuoteInfo obj)
        {
            //if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName)
            //{
            //    return;
            //}

            lock (string.Intern(obj.intentionId))
            {
                _CacheIntentionQuotes.AddOrUpdate(obj.intentionId, obj, (key, value) => obj);
            }

            _OnIntentionQuotes?.Invoke(obj);
        }

        public IntentionQuoteInfo GetIntentionByID(string id)
        {
            if (_CacheIntentionQuotes.ContainsKey(id))
            {
                return _CacheIntentionQuotes[id];
            }
            return null;
        }

        public void Instance_OnRecognition(IntentionQuoteInfo obj)
        {
            _OnRecognition?.Invoke(obj);

            lock (quoteList)
            {
                quoteList.Add(obj);
            }
        }

        public Task<PResult> RelateIntention(RelateIntentionReq request)
        {
            return Task.Run(() =>
            {
                // bool isSuccess = false;
                if (request.execList == null || request.execList.Count == 0)
                    return null;

                PResult result = TableHandler.Instance.RelateIntention(request);
                //if (result == null)
                //{
                //    CommonUtils.Prompt2Second("警告", $"意向关联失败");
                //}
                //else
                //{
                //    if (!result.IsSuccess())
                //    {
                //        CommonUtils.Prompt2Second("警告", result.Msg);
                //    }
                //    else
                //    {
                //        isSuccess = true;
                //    }
                //}
                return result;
            });
        }
    }
}

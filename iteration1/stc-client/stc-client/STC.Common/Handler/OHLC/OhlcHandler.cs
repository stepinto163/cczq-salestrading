using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Handler.Base;
using STC.Common.Handler.MDHist;
using STC.Common.Model.Base;
using STC.Common.Model.OHLC;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Handler.OHLC
{
    public class OhlcHandler : IOhlcHandler
    {

        private static OhlcHandler server = new OhlcHandler();
        public static OhlcHandler Instance { get { return server; } }

        /// <summary>
        /// key: securityID ;  value:回调函数
        /// </summary>
        
        /// <summary>
        /// 缓存Ems订单(订阅)
        /// key: orderID ; value: ems消息
        /// </summary>
        private ConcurrentDictionary<string, OhlcMsg> _CacheOhlcMsg = new ConcurrentDictionary<string, OhlcMsg>();
        private ConcurrentDictionary<string, List<Action<OhlcMsg>>> _kline1MCallbackDic = new ConcurrentDictionary<string, List<Action<OhlcMsg>>>();

        private event Action<OhlcMsg> OnKline1M;

        private event Action<OhlcMsg> OnOhlcMsg;

        private event Action<OhlcPo> OnKLineMsg;

        private object lockdict = new object();

        private volatile bool isInit = false;

        private AsyncThreadQueue<string[]> _subscribeQueue;

        public void SubscribeKLine(string msgType, string securityId, string timeType, Action<OhlcPo> klineCallBack)
        {
            if ((OnKLineMsg != null && !OnKLineMsg.GetInvocationList().Contains(klineCallBack)) || OnKLineMsg == null)
                OnKLineMsg += klineCallBack;


            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string topic = string.Format(ApiConsts.Topic_stc_OHLC_KLine, msgType, securityId, timeType);
                ApiManager.Instance.GetDefaultApi().SubScribe(DataBus.Instance.Instanse_OnMessage,
                    new string[] { topic }, ApiConsts.OHLCSvr, "");

            });
        }

        public void UnSubscribeKLine(string msgType, string securityId, string timeType, Action<OhlcPo> klineCallBack)
        {
            if (OnKLineMsg != null && OnKLineMsg.GetInvocationList().Contains(klineCallBack))
                OnKLineMsg -= klineCallBack;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string topic = string.Format(ApiConsts.Topic_stc_OHLC_KLine, msgType, securityId, timeType);
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.OHLCSvr, new string[] { topic });

            });
        }


        private void Instance_OnKlineMsg(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessKlineMsg(NetMessage netMsg)
        {
            OhlcPo data = BaseHelper.ConvertStcMessage<OhlcPo>(new string[] { ApiConsts.Topic_stc_OHLC_All }, netMsg);//symbolCtDic.Keys.ToArray(), obj);
            if (data != null)
                OnKLineMsg(data);
        }

        private OhlcHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);

            // 订阅线程
            if (_subscribeQueue == null)
            {
                _subscribeQueue = new AsyncThreadQueue<string[]>(ProcessSub);
            }
        }

        ~OhlcHandler()
        {
            if (_subscribeQueue != null)
            {
                _subscribeQueue.Dispose();
            }
        }

        private void ProcessSub(string[] param)
        {
            string securityID = string.Empty;
            string type = string.Empty;
            try
            {
                if (param.Length == 2)
                {
                    securityID = param[0];
                    type = param[1];

                    if (type == "SubscribeOHLC")
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnOhlcMsg, new string[] { $"{ApiConsts.Topic_stc_ohlc_1D}.{securityID}.1D" }, ApiConsts.OHLCSVR, "");
                    }
                    else if (type == "UnSubscribeOHLC")
                    {
                        ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.OHLCSVR, new string[] { $"{ApiConsts.Topic_stc_ohlc_1D}.{securityID}.1D" });
                    }
                    else if (type == "SubscribeOHLC_1M")
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnKline1M, new string[] { $"{ApiConsts.Topic_stc_ohlc_1D}.{securityID}.1M" }, ApiConsts.OHLCSVR, "");
                    }
                    else if (type == "UnSubscribeOHLC_1M")
                    {
                        ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.OHLCSVR, new string[] { $"{ApiConsts.Topic_stc_ohlc_1D}.{securityID}.1M" });
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error($"{type}：{securityID}", e);
            }

        }

        /// <summary>
        /// 清理
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("OhlcHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                isInit = false;
                List<string> subsecurityidlist = _CacheOhlcMsg.Keys.ToList();
                _CacheOhlcMsg.Clear();
                Init(subsecurityidlist);
            }
        }

        public void Init(List<string> subsecurityidlist)
        {

            ThreadPool.QueueUserWorkItem(o =>
            {
                if (!isInit)
                {
                    try
                    {
                        string securityID = string.Empty;
                        for (int i = 0; i < subsecurityidlist.Count; i++)
                        {
                            securityID = subsecurityidlist[i];
                            ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.OHLCSvr, new string[] { $"{ApiConsts.Topic_stc_OHLC}.{securityID}.1D" });
                            Thread.Sleep(10);
                        }

                        for (int i = 0; i < subsecurityidlist.Count; i++)
                        {
                            securityID = subsecurityidlist[i];
                            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, new string[] { $"{ApiConsts.Topic_stc_OHLC}.{securityID}.1D" }, ApiConsts.OHLCSvr, "");
                            Thread.Sleep(10);
                        }

                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("OhlcHandler Instance_OnClear", e);
                    }

                    isInit = true;
                }

            });
        }

        public void SubscribeOhlc(string securityID, Action<OhlcMsg> callBack)
        {
            if (securityID.ToLower() == "tradebook" || string.IsNullOrEmpty(securityID))
            {
                return;
            }
            if (OnOhlcMsg == null)
            {
                OnOhlcMsg += callBack;
            }
            else if (!OnOhlcMsg.GetInvocationList().Contains(callBack))
            {
                OnOhlcMsg += callBack;
            }

            lock (lockdict)
            {
                if (_CacheOhlcMsg.ContainsKey(securityID))
                {
                    OhlcMsg data;
                    if (_CacheOhlcMsg.TryGetValue(securityID, out data))
                    {
                        if (!string.IsNullOrEmpty(data.securityid))
                            callBack?.Invoke(data);
                    }
                }
                else
                {
                    _CacheOhlcMsg.TryAdd(securityID, new OhlcMsg());
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, new string[] { $"{ApiConsts.Topic_stc_OHLC}.{securityID}.1D" }, ApiConsts.OHLCSvr, "");
                    });
                }
            }
        }

        public void UnSubscribeOhlc(string securityID, Action<OhlcMsg> callBack)
        {
            if (OnOhlcMsg != null && OnOhlcMsg.GetInvocationList().Contains(callBack))
                OnOhlcMsg -= callBack;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.OHLCSvr, new string[] { $"{ApiConsts.Topic_stc_OHLC}.{securityID}.1D" });
            });
        }


        /// <summary>
        /// 返回Ohlc
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public OhlcMsg GetLastOhlc(string securityID)
        {
            // 已经注册， 使用缓存数据
            IEnumerable<KeyValuePair<string, OhlcMsg>> cacheItems = _CacheOhlcMsg.Where(item => item.Value.securityid == securityID);
            if (cacheItems != null)
            {
                foreach (KeyValuePair<string, OhlcMsg> item in cacheItems)
                {
                    return item.Value;
                }
            }

            return null;
        }

        private void Instance_OnOhlcMsg(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessOHLC(NetMessage netMsg)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                if (netMsg != null)
                {
                    string topic = netMsg.Topic.ToString();
                    if (topic.Substring(0, 8) == "stc.ohlc")
                    {
                        OhlcMsg data = JsonConvert.DeserializeObject<OhlcMsg>(netMsg.Data);
                        if (data != null)
                        {
                            _CacheOhlcMsg.AddOrUpdate(data.securityid, data, (key, value) => data);
                            OnOhlcMsg(data);
                        }
                    }
                }
            });
        }

        public void RequestOhlc(string securityId)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                    new string[] { $"{ApiConsts.Topic_stc_OHLC}.{securityId}.1D" }, ApiConsts.OHLCSvr, "");
            });
        }

        #region 1M kline

        public void SubscribeKline1M(string securityID, Action<OhlcMsg> callBack)
        {
            if (securityID.ToLower() == "tradebook" || string.IsNullOrEmpty(securityID))
            {
                return;
            }
            if (OnKline1M == null)
            {
                OnKline1M += callBack;
            }
            else if (!OnKline1M.GetInvocationList().Contains(callBack))
            {
                OnKline1M += callBack;
            }

            lock (lockdict)
            {
                if (!_kline1MCallbackDic.ContainsKey(securityID))
                {
                    _kline1MCallbackDic[securityID] = new List<Action<OhlcMsg>>();

                    _subscribeQueue.Enqueue(new string[] { securityID, "SubscribeOHLC_1M" });
                }
                _kline1MCallbackDic[securityID].Add(callBack);
            }
        }

        private void Instance_OnKline1M(object obj)
        {
            OhlcMsg data = BaseHelper.ConvertStcMessage<OhlcMsg>(new string[] { $"{ApiConsts.Topic_stc_ohlc_1D}.*.*.1M" }, obj);
            if (data != null)
            {
                Kline1MCache.Instance.AddOrUpdate(data);
                OnKline1M?.Invoke(data);
            }
        }

        public void UnSubscribeKline1M(string securityID, Action<OhlcMsg> callBack)
        {
            if (OnKline1M != null && OnKline1M.GetInvocationList().Contains(callBack))
            {
                OnKline1M -= callBack;
                if (_kline1MCallbackDic.ContainsKey(securityID) && _kline1MCallbackDic[securityID].Contains(callBack))
                {
                    _kline1MCallbackDic[securityID].Remove(callBack);
                }
            }

            if (_kline1MCallbackDic == null || !_kline1MCallbackDic.ContainsKey(securityID) || _kline1MCallbackDic[securityID].Count == 0)
            {
                //   _subscribeQueue.Enqueue(new string[] { securityID, "UnSubscribeOHLC_1M" });
            }
        }

        #endregion
    }
}

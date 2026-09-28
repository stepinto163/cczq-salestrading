using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using STC.Common.Model.Fix.Message;
using STC.Common.Api;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Dispatcher;
using com.gateway.log;
using STC.Common.Utils;

namespace STC.Common.Handler.Price
{
    /// <summary>
    /// 行情数据
    /// </summary>
    public class MDHandler : AsyncBase<object>, IMDHandler
    {
        private ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> dic = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>();
        private Dictionary<string, bool> symbolDic = new Dictionary<string, bool>();

        private MDHandler() 
        {
            this.start();
        }
        private static IMDHandler server = new MDHandler();


        public static IMDHandler Instance { get { return server; } }

        private event Action<MarketDataSnapshotFullRefresh> OnMDCTPrice;
        private event Action<MarketDataSnapshotFullRefresh> OnMDBestPrice;

        public void ClearCache()
        {
            dic.Clear();
        }


        /// <summary>
        /// 取消订阅最优行情数据和深度行情数据
        /// </summary>
        /// <param name="isSubscribe"></param>
        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnMDBestPrice != null)
                OnMDBestPrice = null;

            if (OnMDCTPrice != null)
                OnMDCTPrice = null;

            dic.Clear();
            symbolDic.Clear();
        }

        /// <summary>
        /// 取消指定债券代码的数据回调
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDCTPriceCallback"></param>
        /// <param name="onMDBestPriceCallback"></param>
        public void UnSubscribe(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            if (symbolDic.ContainsKey(symbol))
            {
                symbolDic.Remove(symbol);

                if (onMDCTPriceCallback != null)
                    OnMDCTPrice -= onMDCTPriceCallback;
                if (onMDBestPriceCallback != null)
                    OnMDBestPrice -= onMDBestPriceCallback;
            }
        }

        /// <summary>
        /// 取消指定接口的数据回调
        /// </summary>
        /// <param name="onMDCTPriceCallback"></param>
        /// <param name="onMDBestPriceCallback"></param>
        public void UnSubscribe(Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            if (onMDCTPriceCallback != null && OnMDCTPrice != null && OnMDCTPrice.GetInvocationList().Contains(onMDCTPriceCallback))
                OnMDCTPrice -= onMDCTPriceCallback;

            if (onMDBestPriceCallback != null && OnMDBestPrice!= null && OnMDBestPrice.GetInvocationList().Contains(onMDBestPriceCallback))
                OnMDBestPrice -= onMDBestPriceCallback;
        }

        public void UnSubscribe(Action<string, MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            ////if (onMDCTPriceCallback != null && OnMDCTPrice != null && OnMDCTPrice.GetInvocationList().Contains(onMDCTPriceCallback))
            ////    OnMDCTPrice -= onMDCTPriceCallback;

            if (onMDBestPriceCallback != null && OnMDBestPrice != null && OnMDBestPrice.GetInvocationList().Contains(onMDBestPriceCallback))
                OnMDBestPrice -= onMDBestPriceCallback;
        }

        /// <summary>
        /// 订阅最优行情数据
        /// </summary>
        /// <param name="onMDBestPriceCallback"></param>
        public void SubscribeBest(Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            if (OnMDBestPrice != null)
            {
                OnMDBestPrice += onMDBestPriceCallback;
            }
        }

        public void SubscribeFull(string symbol, string hoSymbol, Action<string, MarketDataSnapshotFullRefresh> onMDPriceCallback)
        {
            
        }

        /// <summary>
        /// 订阅深度行情（过时）
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDPriceCallback"></param>
        public void SubscribeFull(string symbol, Action<MarketDataSnapshotFullRefresh> onMDPriceCallback)
        {
            if (OnMDCTPrice != null)
            {
                if (!OnMDCTPrice.GetInvocationList().Contains(onMDPriceCallback))
                    OnMDCTPrice += onMDPriceCallback;

                Init(symbol, onMDPriceCallback, null);
            }
        }

        /// <summary>
        /// 订阅深度行情
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDCTPriceCallback"></param>
        public void SubscribeCT(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback)
        {
            if (onMDCTPriceCallback != null)
            {
                if (OnMDCTPrice == null)
                {
                    OnMDCTPrice += onMDCTPriceCallback;
                }
                else
                {
                    int index = Array.IndexOf(OnMDCTPrice.GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)onMDCTPriceCallback);
                    if (index < 0)
                    {
                        OnMDCTPrice += onMDCTPriceCallback;
                    }
                }

                MarketDataSnapshotFullRefresh cache = GetCacheCTPriceBySecurity(symbol);
                if (cache != null)
                {
                    onMDCTPriceCallback?.BeginInvoke(cache, null, null);
                }
            }

            //深度报价
            string ctTopic = symbol;

            if (!ctTopic.Contains(ApiConsts.Topic_stc_md_prefix))
                ctTopic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT,LocalEnv.UserInfo.Termids, symbol, string.Empty, string.Empty);

            lock (this)
            {
                if (!symbolDic.ContainsKey(ctTopic))
                {
                    List<string> rics = new List<string>();
                    rics.Add(ctTopic);

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnCTPriceNew, rics.ToArray(),
                                ApiConsts.MDSvr, "");
                    //object p = ClientApi.Instance.subscribeForMD(rics, Instance_OnCTPrice);

                    symbolDic.Add(ctTopic, true);
                }
            }
        }

        /// <summary>
        /// Cef订阅深度行情
        /// </summary>
        /// <param name="topic"></param>
        public void CefSubScribeWithImage(string[] topic)
        {
             ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnCTPriceNew, topic.ToArray(), ApiConsts.MDSvr, "");
        }
        
        private void Instance_OnCTPriceNew(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg == null)
                return;

            //Cef订阅深度行情广播消息
            Common.Utils.AppUtils.Application.PluginManager.Application.BroadCast("GateWayNetMessage", netMsg);

            if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_md_prefix_ct))
                this.add(netMsg);
        }

        private void ProcessCTPrice(NetMessage netMsg)
        {
            MarketDataSnapshotFullRefresh data = BaseHelper.ConvertStcMessage<MarketDataSnapshotFullRefresh>(new string[] { ApiConsts.Topic_stc_md_sdn_all }, netMsg);//symbolCtDic.Keys.ToArray(), obj);
            if (data != null)
                Instance_OnCTPrice(netMsg.Topic, data);
        }

        /// <summary>
        /// 取消订阅深度行情
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDCTPriceCallback"></param>
        public void UnsubscribeCT(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback)
        {
            if (onMDCTPriceCallback != null && OnMDCTPrice != null && OnMDCTPrice.GetInvocationList().Contains(onMDCTPriceCallback))
                OnMDCTPrice -= onMDCTPriceCallback;

            //深度报价
            string ctTopic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT,LocalEnv.UserInfo.Termids, symbol, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, symbol, string.Empty, string.Empty);

            if (symbolDic.ContainsKey(ctTopic) && (OnMDCTPrice == null || OnMDCTPrice.GetInvocationList().Count() == 0))
            {
                symbolDic.Remove(ctTopic);

                MarketDataSnapshotFullRefresh tem = null;
                dic.TryRemove(ctTopic, out tem);

                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.MDSvr, new string[] { ctTopic });
            }
        }

        /// <summary>
        /// 订阅最优行情
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDBestPriceCallback"></param>
        public void SubscribeBest(string symbol, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            if (onMDBestPriceCallback != null)
            {
                if (OnMDBestPrice == null)
                {
                    OnMDBestPrice += onMDBestPriceCallback;
                }
                else
                {
                    int index = Array.IndexOf(OnMDBestPrice.GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)onMDBestPriceCallback);
                    if (index < 0)
                    {
                        OnMDBestPrice += onMDBestPriceCallback;
                    }
                }
            }
            //最优报价
            string h0Topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0,"", symbol, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, symbol, string.Empty, string.Empty);

            lock (this)
            {
                if (!symbolDic.ContainsKey(h0Topic))
                {
                    List<string> rics = new List<string>();
                    rics.Add(h0Topic);

                    //ClientApi.Instance.subscribeForMD(rics, Instance_OnBestPrice);

                    symbolDic.Add(h0Topic, true);

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnBestPriceNew, rics.ToArray(),
                                ApiConsts.MDSvr, "");
                }
            }
        }

        private void Instance_OnBestPriceNew(object price)
        {
            NetMessage netMsg = price as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_md_prefix_best))
                this.add(netMsg);
        }

        private void ProcessH0Price(NetMessage netMsg)
        {
            MarketDataSnapshotFullRefresh data = BaseHelper.ConvertStcMessage<MarketDataSnapshotFullRefresh>(symbolDic.Keys.ToArray(), netMsg);
            if (data != null)
                Instance_OnBestPrice(netMsg.Topic, data);
        }


        private Dictionary<string, bool> symbolh0Dic = new Dictionary<string, bool>();
        private Dictionary<string, bool> symbolCtDic = new Dictionary<string, bool>();

        /// <summary>
        /// 取消订阅最优行情
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDBestPriceCallback"></param>
        public void UnsubscribeBest(string symbol, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            if (onMDBestPriceCallback != null && OnMDBestPrice != null && OnMDBestPrice.GetInvocationList().Contains(onMDBestPriceCallback))
                OnMDBestPrice -= onMDBestPriceCallback;

            //最优报价
            string h0Topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, "",symbol, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, symbol, string.Empty, string.Empty);

            if (symbolDic.ContainsKey(h0Topic) && (OnMDBestPrice == null || OnMDBestPrice.GetInvocationList().Count() == 0))
            {
                symbolDic.Remove(h0Topic);

                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.MDSvr, new string[] { h0Topic });
            }
        }

        public void Init(List<string> symbol, List<string> hoSymbol, Action<string, MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            if (symbol != null)
            {
                foreach (string ric in symbol)
                {
                    Init(string.Empty, ric, null, onMDBestPriceCallback);
                }
            }

            if (hoSymbol != null)
            {
                foreach (string ric in hoSymbol)
                    Init(string.Empty, ric, null, onMDBestPriceCallback);
            }
        }

        /// <summary>
        /// 同时订阅指定symbol的最优和深度报价
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDCallback"></param>
        public void Init(string symbol, string hoSymbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            lock (this)
            {
                if (onMDCTPriceCallback != null)
                {
                    if (OnMDCTPrice == null)
                    {
                        OnMDCTPrice += onMDCTPriceCallback;
                    }
                    else
                    {
                        int index = Array.IndexOf(OnMDCTPrice.GetInvocationList(), onMDCTPriceCallback);
                        if (index < 0)
                        {
                            OnMDCTPrice += onMDCTPriceCallback;
                        }
                    }
                }

                if (onMDBestPriceCallback != null)
                {
                    if (OnMDBestPrice == null)
                    {
                        OnMDBestPrice += onMDBestPriceCallback;
                    }
                    else
                    {
                        int index = Array.IndexOf(OnMDBestPrice.GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)onMDBestPriceCallback);
                        if (index < 0)
                        {
                            OnMDBestPrice += onMDBestPriceCallback;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(symbol) && !symbolCtDic.ContainsKey(symbol))
                {
                    //深度报价
                    string ctTopic = symbol;// ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, symbol, string.Empty, string.Empty);

                    if (!symbolCtDic.ContainsKey(ctTopic))
                        symbolCtDic.Add(ctTopic, true);

                    List<string> rics = new List<string>();
                    rics.Add(ctTopic);

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnCTPriceNew, rics.ToArray(),
                            ApiConsts.MDSvr, "");
                }

                if (!string.IsNullOrEmpty(hoSymbol) && !symbolh0Dic.ContainsKey(hoSymbol))
                {
                    //h0报价
                    string h0Topic = hoSymbol;// ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, symbol, string.Empty, string.Empty);
                    if (!symbolh0Dic.ContainsKey(h0Topic))
                        symbolh0Dic.Add(h0Topic, true);

                    List<string> rics = new List<string>();
                    rics.Add(h0Topic);

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnBestPriceNew, rics.ToArray(),
                            ApiConsts.MDSvr, "");

                }
            }
        }

        /// <summary>
        /// 同时订阅指定symbol的最优和深度报价
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="onMDCallback"></param>
        public void Init(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback)
        {
            if (onMDCTPriceCallback != null)
            {
                if (OnMDCTPrice == null)
                {
                    OnMDCTPrice += onMDCTPriceCallback;
                }
                else
                {
                    int index = Array.IndexOf(OnMDCTPrice.GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)onMDCTPriceCallback);
                    if (index < 0)
                    {
                        OnMDCTPrice += onMDCTPriceCallback;
                    }
                }
            }

            if (onMDBestPriceCallback != null)
            {
                if (OnMDBestPrice == null)
                {
                    OnMDBestPrice += onMDBestPriceCallback;
                }
                else
                {
                    int index = Array.IndexOf(OnMDBestPrice.GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)onMDBestPriceCallback);
                    if (index < 0)
                    {
                        OnMDBestPrice += onMDBestPriceCallback;
                    }
                }
            }

            //深度报价
            string ctTopic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT,LocalEnv.UserInfo.Termids, symbol, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT,LocalEnv.UserInfo.Termids, symbol, string.Empty, string.Empty);
            string h0Topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0,"", symbol, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, symbol, string.Empty, string.Empty);
            List<string> rics = new List<string>();
            lock (this)
            {
                if (!symbolDic.ContainsKey(ctTopic))
                {
                    rics.Add(ctTopic);

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnCTPriceNew, rics.ToArray(),
                                ApiConsts.MDSvr, "");
                    //ClientApi.Instance.subscribeForMD(rics, Instance_OnCTPrice);

                    symbolDic.Add(ctTopic, true);
                }

                if (!symbolDic.ContainsKey(h0Topic))
                {
                    symbolDic.Add(h0Topic, true);

                    rics.Clear();
                    rics.Add(h0Topic);

                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnBestPriceNew, rics.ToArray(),
                                ApiConsts.MDSvr, "");
                    });
                    //ClientApi.Instance.subscribeForMD(rics, Instance_OnBestPrice);

                }
            }
        }

        /// <summary>
        /// 获得最优行情topic
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public string GetCtTopic(string symbol)
        {
            return ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT,LocalEnv.UserInfo.Termids, symbol, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, symbol, string.Empty, string.Empty);
        }
        
        /// <summary>
        /// 最优行情数据回调方法
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="price">行情数据</param>
        private void Instance_OnBestPrice(string topic, MarketDataSnapshotFullRefresh price)
        {
            if (topic.StartsWith(ApiConsts.Topic_stc_md_prefix_best))
            {
                dic.AddOrUpdate(topic, price, (key, value) => price);

                OnMDBestPrice?.Invoke(price);
            }
        }

        /// <summary>
        /// 深度行情数据回调方法
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="price"></param>
        private void Instance_OnCTPrice(string topic, MarketDataSnapshotFullRefresh price)
        {
            if (topic.StartsWith(ApiConsts.Topic_stc_md_prefix_ct))
            {
                dic.AddOrUpdate(topic, price, (key, value) => price);

                OnMDCTPrice?.Invoke(price);
            }
        }

        /// <summary>
        /// 根据Topic， 返回缓存行情报价
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>缓存的行情报价</returns>
        public MarketDataSnapshotFullRefresh GetCachePriceByTopic(string topic)
        {
            MarketDataSnapshotFullRefresh price = null;
            if (dic.ContainsKey(topic))
            {
                dic.TryGetValue(topic, out price);
            }
            return price;
        }

        /// <summary>
        /// 根据债券代码，返回缓存行情报价
        /// </summary>
        /// <param name="security"></param>
        /// <returns>缓存的最优行情数据</returns>
        public MarketDataSnapshotFullRefresh GetCacheBestPriceBySecurity(string security)
        {
            string h0Topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0,"", security, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, security, string.Empty, string.Empty);
            MarketDataSnapshotFullRefresh price = null;
            if (dic.ContainsKey(h0Topic))
            {
                dic.TryGetValue(h0Topic, out price);
            }
            return price;
        }

        public MarketDataSnapshotFullRefresh GetCacheCTPriceBySecurity(string security)
        {
            string ctTopic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT,LocalEnv.UserInfo.Termids, security, string.Empty, string.Empty);
            //ClientApi.Instance.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, security, string.Empty, string.Empty);
            MarketDataSnapshotFullRefresh price = null;
            if (dic.ContainsKey(ctTopic))
            {
                dic.TryGetValue(ctTopic, out price);
            }
            return price;
        }

        public override void process(object t)
        {
            try
            {
                NetMessage netMsg = t as NetMessage;
                if (netMsg == null)
                    return;
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_md_prefix_ct))
                {
                    ProcessCTPrice(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_md_prefix_best))
                {
                    ProcessH0Price(netMsg);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
    }
}
